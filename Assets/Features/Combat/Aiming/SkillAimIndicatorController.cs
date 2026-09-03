using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Combat.Aiming.Renderers;

namespace ProjectZombie.Features.Combat.Aiming
{
    /// <summary>
    /// Bộ điều phối hiển thị chỉ dấu ngắm chiêu MOBA (Skill Aiming Indicator Service Coordinator).
    /// Triển khai Strategy Pattern & Open-Closed Principle (OCP):
    /// - Quản lý trạng thái ngắm bắn (Aiming state, Auto-aim throttling, Cancel hover).
    /// - Ủy quyền vẽ hình học trực quan cho các IAimIndicatorRenderer độc lập.
    /// - Cung cấp CurrentAimResult toàn vẹn cho tầng UI/Vũ khí khi nhả nút.
    /// </summary>
    public class SkillAimIndicatorController : MonoBehaviour, ISkillAimService
    {
        public static SkillAimIndicatorController Instance { get; private set; }

        [Header("Sprite Assets (Shared Cache)")]
        [SerializeField] private Sprite _circleSprite;
        [SerializeField] private Sprite _boxSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField] private Sprite _arrowSprite;
        [SerializeField] private Material _sectorMaterial;

        [Header("Indicator Colors")]
        [SerializeField] private Color _normalAimColor = new Color(0.2f, 0.85f, 1.0f, 0.75f);     // Xanh ngọc phát sáng
        [SerializeField] private Color _cancelAimColor = new Color(1.0f, 0.25f, 0.25f, 0.85f);    // Đỏ rực hủy chiêu
        [SerializeField] private Color _maxRangeBoundaryColor = new Color(1f, 1f, 1f, 0.2f);      // Vòng giới hạn mờ

        // Transform roots
        private Transform _playerTransform;
        private GameObject _indicatorRoot;
        private Transform _rangeBoundaryIndicator;
        private SpriteRenderer _rangeBoundaryRenderer;

        private bool _isAiming;
        private bool _isCancelHovered;
        private bool _hasExplicitDrag;
        private Vector2 _currentAimDirection;
        private float _currentPullPercent;
        private SkillAimConfig _currentConfig;

        // Tối ưu hiệu năng: Physics scan throttling (20Hz)
        private float _lastAutoAimScanTime;
        private const float AUTO_AIM_SCAN_INTERVAL = 0.05f;
        private Vector2 _cachedAutoAimDir = Vector2.right;

        // Registry các Strategy Renderer (Strategy Pattern / Open-Closed Principle)
        private readonly Dictionary<SkillAimType, IAimIndicatorRenderer> _renderers = new Dictionary<SkillAimType, IAimIndicatorRenderer>();

        /// <summary>
        /// Kết quả ngắm bắn định hướng hiện tại (Direction, Distance, TargetWorldPos, IsQuickTap).
        /// </summary>
        public AimResult CurrentAimResult { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadDefaultSprites();
            BuildIndicatorHierarchy();
            HideAll();
        }

        private void Start()
        {
            GetPlayerTransform();
        }

        private Transform GetPlayerTransform()
        {
            if (_playerTransform != null && _playerTransform.gameObject.activeInHierarchy)
                return _playerTransform;

            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                _playerTransform = PlayerProvider.PlayerTransform;
                return _playerTransform;
            }

            if (PlayerController.Instance != null)
            {
                _playerTransform = PlayerController.Instance.transform;
                return _playerTransform;
            }

            return _playerTransform;
        }

        private void LoadDefaultSprites()
        {
#if UNITY_EDITOR
            if (_circleSprite == null)
                _circleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Indicators/TEX_Indicator_Circle.png");
            if (_boxSprite == null)
                _boxSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Indicators/TEX_Indicator_Box.png");
            if (_fillSprite == null)
                _fillSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Indicators/TEX_Indicator_Fill.png");
            if (_arrowSprite == null)
                _arrowSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Tex_Attack_Aim_Arc_Reticle.png");
            if (_sectorMaterial == null)
            {
                var shader = Shader.Find("ProjectZombie/VFX/SkillIndicator_Sector");
                if (shader != null) _sectorMaterial = new Material(shader);
            }
#endif
            if (_circleSprite == null) _circleSprite = Resources.Load<Sprite>("Art/VFX/Indicators/TEX_Indicator_Circle");
            if (_boxSprite == null) _boxSprite = Resources.Load<Sprite>("Art/VFX/Indicators/TEX_Indicator_Box");
            if (_sectorMaterial == null)
            {
                var shader = Shader.Find("ProjectZombie/VFX/SkillIndicator_Sector");
                if (shader != null) _sectorMaterial = new Material(shader);
            }
        }

        private void BuildIndicatorHierarchy()
        {
            if (_indicatorRoot != null) return;

            _indicatorRoot = new GameObject("VFX_Skill_Aim_Indicators");
            _indicatorRoot.transform.SetParent(transform, false);

            var context = new IndicatorResourceContext
            {
                CircleSprite = _circleSprite,
                BoxSprite = _boxSprite,
                FillSprite = _fillSprite,
                ArrowSprite = _arrowSprite,
                SectorMaterial = _sectorMaterial
            };

            // Đăng ký các Strategy Renderers chuyên biệt
            RegisterRenderer(new LineArrowIndicatorRenderer(), context);
            RegisterRenderer(new ConeSectorIndicatorRenderer(), context);
            RegisterRenderer(new CircleReticleIndicatorRenderer(), context);
            RegisterRenderer(new VectorWallIndicatorRenderer(), context);
            RegisterRenderer(new DashLineIndicatorRenderer(), context);
            RegisterRenderer(new SelfAOEIndicatorRenderer(), context);
            RegisterRenderer(new CurvedTrajectoryIndicatorRenderer(), context);
            RegisterRenderer(new RhythmPulseIndicatorRenderer(), context);

            // Vòng tròn Max Range Boundary Indicator
            GameObject rangeObj = new GameObject("Max_Range_Boundary");
            rangeObj.transform.SetParent(_indicatorRoot.transform, false);
            _rangeBoundaryIndicator = rangeObj.transform;
            _rangeBoundaryRenderer = rangeObj.AddComponent<SpriteRenderer>();
            _rangeBoundaryRenderer.sprite = _circleSprite;
            _rangeBoundaryRenderer.sortingLayerName = "Skill";
            _rangeBoundaryRenderer.sortingOrder = 4;
            _rangeBoundaryRenderer.color = _maxRangeBoundaryColor;
        }

        /// <summary>
        /// Cho phép mở rộng đăng ký thêm Renderer động từ ngoài (Open-Closed Principle).
        /// </summary>
        public void RegisterRenderer(IAimIndicatorRenderer renderer, IndicatorResourceContext context = null)
        {
            if (renderer == null) return;

            if (context == null)
            {
                context = new IndicatorResourceContext
                {
                    CircleSprite = _circleSprite,
                    BoxSprite = _boxSprite,
                    FillSprite = _fillSprite,
                    ArrowSprite = _arrowSprite,
                    SectorMaterial = _sectorMaterial
                };
            }

            renderer.Initialize(_indicatorRoot.transform, context);
            _renderers[renderer.SupportedType] = renderer;
        }

        public void StartAim(SkillAimConfig config)
        {
            GetPlayerTransform();

            _currentConfig = config;
            _isAiming = true;
            _isCancelHovered = false;
            _hasExplicitDrag = false;
            _currentAimDirection = Vector2.zero;
            _currentPullPercent = 0.85f;
            _lastAutoAimScanTime = 0f;

            if (_indicatorRoot != null) _indicatorRoot.SetActive(true);

            if (config.range > 0f && config.aimType != SkillAimType.SelfAOE && _rangeBoundaryRenderer != null)
            {
                _rangeBoundaryRenderer.enabled = true;
                float spriteBounds = (_rangeBoundaryRenderer.sprite != null && _rangeBoundaryRenderer.sprite.bounds.size.x > 0.01f)
                    ? _rangeBoundaryRenderer.sprite.bounds.size.x : 1.0f;
                float boundaryScale = (config.range * 2.0f) / spriteBounds;
                _rangeBoundaryIndicator.localScale = Vector3.one * boundaryScale;
            }
            else if (_rangeBoundaryRenderer != null)
            {
                _rangeBoundaryRenderer.enabled = false;
            }

            RenderAimVisuals();
        }

        public void UpdateAim(Vector2 aimDirection, float pullPercent, bool isCancelHovered = false)
        {
            if (!_isAiming) return;

            _isCancelHovered = isCancelHovered;
            _currentPullPercent = pullPercent;

            if (aimDirection.sqrMagnitude > 0.001f)
            {
                _hasExplicitDrag = true;
                _currentAimDirection = aimDirection.normalized;
            }
        }

        private void LateUpdate()
        {
            if (!_isAiming) return;
            RenderAimVisuals();
        }

        private void RenderAimVisuals()
        {
            Transform p = GetPlayerTransform();
            Vector3 origin = p != null ? p.position : transform.position;
            origin.z = 0f;

            if (_rangeBoundaryIndicator != null && _rangeBoundaryRenderer != null && _rangeBoundaryRenderer.enabled)
            {
                _rangeBoundaryIndicator.position = origin;
            }

            Vector2 aimDir = _currentAimDirection;
            if (!_hasExplicitDrag || aimDir == Vector2.zero)
            {
                if (Time.time >= _lastAutoAimScanTime + AUTO_AIM_SCAN_INTERVAL)
                {
                    _lastAutoAimScanTime = Time.time;
                    Vector2 fallback = Vector2.right;
                    if (p != null)
                    {
                        var ctrl = p.GetComponent<PlayerController>();
                        if (ctrl != null && ctrl.MovementInput != Vector2.zero)
                        {
                            fallback = ctrl.MovementInput.normalized;
                        }
                        else
                        {
                            var anim = p.GetComponentInChildren<Player.PlayerAnimator>();
                            if (anim != null)
                            {
                                fallback = anim.FacingDirection >= 0f ? Vector2.right : Vector2.left;
                            }
                            else
                            {
                                fallback = p.localScale.x >= 0 ? Vector2.right : Vector2.left;
                            }
                        }
                    }

                    AutoTargetScanner.TryGetAutoAimDirection(origin, _currentConfig, fallback, out _cachedAutoAimDir, out _);
                }

                aimDir = _cachedAutoAimDir;
            }

            if (aimDir == Vector2.zero) aimDir = Vector2.right;
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            Color activeColor = _isCancelHovered ? _cancelAimColor : _normalAimColor;

            // Tính toán khoảng cách và tọa độ điểm rơi trong thế giới
            float distance = _currentConfig.range;
            if (_currentConfig.aimType == SkillAimType.CircleReticle)
            {
                distance = Mathf.Clamp01(_currentPullPercent) * _currentConfig.range;
            }
            Vector3 targetWorldPos = origin + (Vector3)(aimDir * distance);

            // Cập nhật kết quả ngắm chuẩn xác cho toàn bộ hệ thống
            CurrentAimResult = new AimResult(aimDir, distance, targetWorldPos, _currentPullPercent, !_hasExplicitDrag);

            // Phân nhánh Strategy: Điều phối vẽ hình học tương ứng
            if (_renderers.TryGetValue(_currentConfig.aimType, out var activeRenderer) && activeRenderer != null)
            {
                // Ẩn các renderer khác để tránh chồng chéo
                foreach (var kvp in _renderers)
                {
                    if (kvp.Key != _currentConfig.aimType && kvp.Value != null)
                    {
                        kvp.Value.Hide();
                    }
                }

                activeRenderer.Render(origin, aimDir, angle, _currentPullPercent, _currentConfig, activeColor);
            }
            else
            {
                HideAll();
            }
        }

        public void StopAim()
        {
            _isAiming = false;
            _hasExplicitDrag = false;
            _currentAimDirection = Vector2.zero;
            HideAll();
        }

        public void HideAll()
        {
            foreach (var renderer in _renderers.Values)
            {
                renderer?.Hide();
            }

            if (_rangeBoundaryRenderer != null) _rangeBoundaryRenderer.enabled = false;
            if (_indicatorRoot != null) _indicatorRoot.SetActive(false);
        }
    }
}

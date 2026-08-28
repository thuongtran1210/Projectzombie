using ProjectZombie.Features.Player;
using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming
{
    /// <summary>
    /// [LOẠI 2: CHỈ DẤU KỸ NĂNG & ĐÒN ĐÁNH MOBA (ACTIVE SKILL/ATTACK TELEGRAPH INDICATOR)]
    /// ---------------------------------------------------------------------------------------------
    /// - Vai trò: Điều phối hiển thị toàn bộ Chỉ Dấu Kỹ Năng 2.5D (Telegraph Skill Indicators) chuẩn MOBA Liên Quân.
    /// - Mục đích: Giúp người chơi căn cự ly, độ rộng Hitbox đòn đánh, ngắm bắn 360 độ và hủy chiêu.
    /// - Hỗ trợ 3 dạng hình thái: Line Arrow (Mũi tên), Cone Sector (Hình quạt), Circle Reticle (Tâm tròn AOE).
    /// - Thời điểm kích hoạt: CHỈ XUẤT HIỆN KHI ĐÈ (Hold > 0.12s) HOẶC KÉO TAY (Drag) vào nút Kỹ năng/Đánh thường.
    /// - Phân biệt với [LOẠI 1 - CombatAimIndicator]:
    ///     + Chỉ xuất hiện chủ động khi tương tác skill UI.
    ///     + Kích thước bằng đúng thông số vùng sát thương thực tế của kỹ năng (2.5m - 8.0m).
    ///     + Hỗ trợ đổi sang màu đỏ khi kéo vào Vùng Hủy Chiêu (UICancelSkillZone).
    /// ---------------------------------------------------------------------------------------------
    /// </summary>
    public class SkillAimIndicatorController : MonoBehaviour
    {
        public static SkillAimIndicatorController Instance { get; private set; }

        [Header("Sprites & Textures")]
        [SerializeField] private Sprite _circleSprite;
        [SerializeField] private Sprite _boxSprite;
        [SerializeField] private Sprite _fillSprite;
        [SerializeField] private Sprite _arrowSprite;

        [Header("Indicator Colors")]
        [SerializeField] private Color _normalAimColor = new Color(0.2f, 0.85f, 1.0f, 0.75f);     // Xanh ngọc phát sáng
        [SerializeField] private Color _cancelAimColor = new Color(1.0f, 0.25f, 0.25f, 0.85f);    // Đỏ rực hủy chiêu
        [SerializeField] private Color _maxRangeBoundaryColor = new Color(1f, 1f, 1f, 0.2f);      // Vòng giới hạn mờ

        // Transform roots
        private Transform _playerTransform;
        private GameObject _indicatorRoot;
        private Transform _lineIndicator;
        private Transform _coneIndicator;
        private Transform _circleIndicator;
        private Transform _rangeBoundaryIndicator;

        private SpriteRenderer _lineRenderer;
        private SpriteRenderer _coneRenderer;
        private SpriteRenderer _circleRenderer;
        private SpriteRenderer _rangeBoundaryRenderer;

        private bool _isAiming;
        private bool _isCancelHovered;
        private bool _hasExplicitDrag;
        private Vector2 _currentAimDirection;
        private float _currentPullPercent;
        private SkillAimConfig _currentConfig;

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

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
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
#endif
            if (_circleSprite == null) _circleSprite = Resources.Load<Sprite>("Art/VFX/Indicators/TEX_Indicator_Circle");
            if (_boxSprite == null) _boxSprite = Resources.Load<Sprite>("Art/VFX/Indicators/TEX_Indicator_Box");
        }

        private void BuildIndicatorHierarchy()
        {
            if (_indicatorRoot != null) return;

            _indicatorRoot = new GameObject("VFX_Skill_Aim_Indicators");
            _indicatorRoot.transform.SetParent(transform, false);

            // 1. Line / Arrow Indicator
            GameObject lineObj = new GameObject("Line_Indicator");
            lineObj.transform.SetParent(_indicatorRoot.transform, false);
            _lineIndicator = lineObj.transform;
            _lineRenderer = lineObj.AddComponent<SpriteRenderer>();
            _lineRenderer.sprite = _boxSprite != null ? _boxSprite : _fillSprite;
            _lineRenderer.sortingLayerName = "Skill";
            _lineRenderer.sortingOrder = 5;

            // 2. Cone / Sector Indicator
            GameObject coneObj = new GameObject("Cone_Indicator");
            coneObj.transform.SetParent(_indicatorRoot.transform, false);
            _coneIndicator = coneObj.transform;
            _coneRenderer = coneObj.AddComponent<SpriteRenderer>();
            _coneRenderer.sprite = _arrowSprite != null ? _arrowSprite : _circleSprite;
            _coneRenderer.sortingLayerName = "Skill";
            _coneRenderer.sortingOrder = 5;

            // 3. Circle / Reticle Indicator (AOE Drop Point)
            GameObject circleObj = new GameObject("Circle_Reticle_Indicator");
            circleObj.transform.SetParent(_indicatorRoot.transform, false);
            _circleIndicator = circleObj.transform;
            _circleRenderer = circleObj.AddComponent<SpriteRenderer>();
            _circleRenderer.sprite = _circleSprite;
            _circleRenderer.sortingLayerName = "Skill";
            _circleRenderer.sortingOrder = 6;

            // 4. Max Range Boundary Indicator (Vòng tròn giới hạn tầm bắn)
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
        /// Bắt đầu hiển thị ngắm chiêu khi người chơi chạm/kéo nút skill.
        /// </summary>
        public void StartAim(SkillAimConfig config)
        {
            GetPlayerTransform();

            _currentConfig = config;
            _isAiming = true;
            _isCancelHovered = false;
            _hasExplicitDrag = false;
            _currentAimDirection = Vector2.zero;
            _currentPullPercent = 0.85f;

            if (_indicatorRoot != null) _indicatorRoot.SetActive(true);

            // Bật vòng max range nếu kỹ năng có cự ly
            if (config.range > 0f && _rangeBoundaryRenderer != null)
            {
                _rangeBoundaryRenderer.enabled = true;
                float spriteBounds = (_rangeBoundaryRenderer.sprite != null && _rangeBoundaryRenderer.sprite.bounds.size.x > 0.01f)
                    ? _rangeBoundaryRenderer.sprite.bounds.size.x
                    : 1.0f;
                float boundaryScale = (config.range * 2.0f) / spriteBounds;
                _rangeBoundaryIndicator.localScale = Vector3.one * boundaryScale;
            }
            else if (_rangeBoundaryRenderer != null)
            {
                _rangeBoundaryRenderer.enabled = false;
            }

            RenderAimVisuals();
        }

        /// <summary>
        /// Cập nhật hướng ngắm và cự ly kéo theo thời gian thực (360 độ).
        /// </summary>
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

            RenderAimVisuals();
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

            // 1. Đồng bộ vòng Max Range theo bước chân nhân vật
            if (_rangeBoundaryIndicator != null && _rangeBoundaryRenderer != null && _rangeBoundaryRenderer.enabled)
            {
                _rangeBoundaryIndicator.position = origin;
            }

            // 2. Xác định hướng ngắm: Nếu người chơi đang chủ động kéo tay thì theo tay, nếu chưa kéo thì tự bám mục tiêu gần nhất / hướng chạy
            Vector2 aimDir = _currentAimDirection;
            if (!_hasExplicitDrag || aimDir == Vector2.zero)
            {
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
                        fallback = p.localScale.x >= 0 ? Vector2.right : Vector2.left;
                    }
                }

                AutoTargetScanner.TryGetAutoAimDirection(origin, _currentConfig, fallback, out aimDir, out _);
            }

            if (aimDir == Vector2.zero) aimDir = Vector2.right;
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            Color activeColor = _isCancelHovered ? _cancelAimColor : _normalAimColor;

            switch (_currentConfig.aimType)
            {
                case SkillAimType.LineArrow:
                    ShowLineIndicator(origin, aimDir, angle, activeColor);
                    break;

                case SkillAimType.ConeSector:
                    ShowConeIndicator(origin, aimDir, angle, activeColor);
                    break;

                case SkillAimType.CircleReticle:
                    ShowCircleReticle(origin, aimDir, _currentPullPercent, activeColor);
                    break;

                default:
                    HideAll();
                    break;
            }
        }

        private void ShowLineIndicator(Vector3 origin, Vector2 direction, float angle, Color color)
        {
            if (_lineRenderer == null) return;

            _lineRenderer.enabled = true;
            if (_coneRenderer != null) _coneRenderer.enabled = false;
            if (_circleRenderer != null) _circleRenderer.enabled = false;

            float length = Mathf.Max(2.0f, _currentConfig.range);
            float width = Mathf.Max(0.6f, _currentConfig.radius);

            float spriteBoundsX = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.x > 0.01f)
                ? _lineRenderer.sprite.bounds.size.x
                : 1.0f;
            float spriteBoundsY = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.y > 0.01f)
                ? _lineRenderer.sprite.bounds.size.y
                : 1.0f;

            float scaleX = length / spriteBoundsX;
            float scaleY = width / spriteBoundsY;

            _lineIndicator.position = origin + (Vector3)(direction * (length * 0.5f));
            _lineIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
            _lineIndicator.localScale = new Vector3(scaleX, scaleY, 1f);
            _lineRenderer.color = color;
        }

        private void ShowConeIndicator(Vector3 origin, Vector2 direction, float angle, Color color)
        {
            if (_coneRenderer == null) return;

            _coneRenderer.enabled = true;
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_circleRenderer != null) _circleRenderer.enabled = false;

            float reach = Mathf.Max(1.8f, _currentConfig.range);
            float width = Mathf.Max(1.4f, _currentConfig.radius);

            float spriteBoundsX = (_coneRenderer.sprite != null && _coneRenderer.sprite.bounds.size.x > 0.01f)
                ? _coneRenderer.sprite.bounds.size.x
                : 1.0f;
            float spriteBoundsY = (_coneRenderer.sprite != null && _coneRenderer.sprite.bounds.size.y > 0.01f)
                ? _coneRenderer.sprite.bounds.size.y
                : 1.0f;

            float scaleX = reach / spriteBoundsX;
            float scaleY = width / spriteBoundsY;

            _coneIndicator.position = origin + (Vector3)(direction * (reach * 0.45f));
            _coneIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
            _coneIndicator.localScale = new Vector3(scaleX, scaleY, 1f);
            _coneRenderer.color = color;
        }

        private void ShowCircleReticle(Vector3 origin, Vector2 direction, float pullPercent, Color color)
        {
            if (_circleRenderer == null) return;

            _circleRenderer.enabled = true;
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_coneRenderer != null) _coneRenderer.enabled = false;

            float distance = Mathf.Clamp01(pullPercent) * _currentConfig.range;
            Vector3 targetPos = origin + (Vector3)(direction * distance);
            float radius = Mathf.Max(1.0f, _currentConfig.radius);

            float spriteBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x
                : 1.0f;
            float scale = (radius * 2.0f) / spriteBounds;

            _circleIndicator.position = targetPos;
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * scale;
            _circleRenderer.color = color;
        }

        /// <summary>
        /// Kết thúc hoặc hủy ngắm chiêu.
        /// </summary>
        public void StopAim()
        {
            _isAiming = false;
            _hasExplicitDrag = false;
            _currentAimDirection = Vector2.zero;
            HideAll();
        }

        public void HideAll()
        {
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_coneRenderer != null) _coneRenderer.enabled = false;
            if (_circleRenderer != null) _circleRenderer.enabled = false;
            if (_rangeBoundaryRenderer != null) _rangeBoundaryRenderer.enabled = false;
            if (_indicatorRoot != null) _indicatorRoot.SetActive(false);
        }
    }
}

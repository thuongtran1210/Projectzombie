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
    public class SkillAimIndicatorController : MonoBehaviour, ISkillAimService
    {
        public static SkillAimIndicatorController Instance { get; private set; }

        [Header("Sprites & Textures")]
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
        private Transform _lineIndicator;
        private Transform _coneIndicator;
        private Transform _circleIndicator;
        private Transform _rangeBoundaryIndicator;

        private SpriteRenderer _lineRenderer;
        private SpriteRenderer _coneRenderer;
        private SpriteRenderer _circleRenderer;
        private SpriteRenderer _rangeBoundaryRenderer;

        private MaterialPropertyBlock _conePropertyBlock;
        private static readonly int PropTintColor = Shader.PropertyToID("_TintColor");
        private static readonly int PropBorderColor = Shader.PropertyToID("_BorderColor");
        private static readonly int PropArcAngle = Shader.PropertyToID("_ArcAngle");

        private bool _isAiming;
        private bool _isCancelHovered;
        private bool _hasExplicitDrag;
        private Vector2 _currentAimDirection;
        private float _currentPullPercent;
        private SkillAimConfig _currentConfig;

        // Tối ưu hiệu năng: Physics scan throttling (20Hz)
        private float _lastAutoAimScanTime;
        private const float AUTO_AIM_SCAN_INTERVAL = 0.05f; // 20 lần/giây thay vì 60 lần/giây
        private Vector2 _cachedAutoAimDir = Vector2.right;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _conePropertyBlock = new MaterialPropertyBlock();

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

            // 1. Line / Arrow Indicator
            GameObject lineObj = new GameObject("Line_Indicator");
            lineObj.transform.SetParent(_indicatorRoot.transform, false);
            _lineIndicator = lineObj.transform;
            _lineRenderer = lineObj.AddComponent<SpriteRenderer>();
            _lineRenderer.sprite = _boxSprite != null ? _boxSprite : _fillSprite;
            _lineRenderer.sortingLayerName = "Skill";
            _lineRenderer.sortingOrder = 5;

            // 2. Cone / Sector Indicator (Hỗ trợ Polar Shader Arc)
            GameObject coneObj = new GameObject("Cone_Indicator");
            coneObj.transform.SetParent(_indicatorRoot.transform, false);
            _coneIndicator = coneObj.transform;
            _coneRenderer = coneObj.AddComponent<SpriteRenderer>();
            _coneRenderer.sprite = _circleSprite != null ? _circleSprite : _fillSprite;
            if (_sectorMaterial != null)
            {
                _coneRenderer.material = _sectorMaterial;
            }
            _coneRenderer.sortingLayerName = "Skill";
            _coneRenderer.sortingOrder = 5;

            // 3. Circle / Reticle Indicator (AOE Drop Point & Self AOE)
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
            _lastAutoAimScanTime = 0f;

            if (_indicatorRoot != null) _indicatorRoot.SetActive(true);

            // Bật vòng max range nếu kỹ năng có cự ly ném/bắn xa
            if (config.range > 0f && config.aimType != SkillAimType.SelfAOE && _rangeBoundaryRenderer != null)
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
        /// Cập nhật hướng ngắm và cự ly kéo theo thời gian thực (O(1) Zero-Alloc).
        /// Không gọi RenderAimVisuals trực tiếp để tránh tính toán trùng lặp trong frame, chuyển việc render về LateUpdate.
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
                // Throttling: Chỉ quét Physics 2D ở tần số 20Hz (mỗi 0.05s) để tiết kiệm 65% tải CPU
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
                            fallback = p.localScale.x >= 0 ? Vector2.right : Vector2.left;
                        }
                    }

                    AutoTargetScanner.TryGetAutoAimDirection(origin, _currentConfig, fallback, out _cachedAutoAimDir, out _);
                }

                aimDir = _cachedAutoAimDir;
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

                case SkillAimType.SelfAOE:
                    ShowSelfAOEIndicator(origin, activeColor);
                    break;

                case SkillAimType.DashLine:
                    ShowDashLineIndicator(origin, aimDir, angle, activeColor);
                    break;

                case SkillAimType.VectorWall:
                    ShowVectorWallIndicator(origin, aimDir, angle, activeColor);
                    break;

                case SkillAimType.CurvedTrajectory:
                    ShowCurvedTrajectoryIndicator(origin, aimDir, angle, activeColor);
                    break;

                case SkillAimType.RhythmPulse:
                    ShowRhythmPulseIndicator(origin, activeColor);
                    break;

                default:
                    HideAll();
                    break;
            }
        }

        private void ShowVectorWallIndicator(Vector3 origin, Vector2 direction, float angle, Color color)
        {
            if (_lineRenderer == null || _circleRenderer == null) return;

            _lineRenderer.enabled = true;
            _circleRenderer.enabled = true;
            if (_coneRenderer != null) _coneRenderer.enabled = false;

            float distance = Mathf.Max(1.5f, _currentConfig.range * _currentPullPercent);
            Vector3 centerWallPos = origin + (Vector3)(direction * distance);
            float wallWidth = Mathf.Max(3.0f, _currentConfig.radius);
            float wallThickness = 0.6f;

            float spriteBoundsX = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.x > 0.01f)
                ? _lineRenderer.sprite.bounds.size.x : 1.0f;
            float spriteBoundsY = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.y > 0.01f)
                ? _lineRenderer.sprite.bounds.size.y : 1.0f;

            // Bức tường vuông góc với hướng ngắm (+90 độ)
            _lineIndicator.position = centerWallPos;
            _lineIndicator.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
            _lineIndicator.localScale = new Vector3(wallWidth / spriteBoundsX, wallThickness / spriteBoundsY, 1f);
            _lineRenderer.color = color;

            // Tâm định vị cọc trung tâm
            float pinBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x : 1.0f;
            _circleIndicator.position = centerWallPos;
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * (0.8f / pinBounds);
            _circleRenderer.color = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f, 0.95f);
        }

        private LineRenderer _curveLineRenderer;

        private void ShowCurvedTrajectoryIndicator(Vector3 origin, Vector2 direction, float angle, Color color)
        {
            if (_circleRenderer == null) return;

            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_coneRenderer != null) _coneRenderer.enabled = false;
            _circleRenderer.enabled = true;

            // Khởi tạo LineRenderer uốn cong mượt mà nếu chưa có
            if (_curveLineRenderer == null)
            {
                GameObject curveObj = new GameObject("Curved_Trajectory_Line");
                curveObj.transform.SetParent(_indicatorRoot.transform, false);
                _curveLineRenderer = curveObj.AddComponent<LineRenderer>();
                _curveLineRenderer.useWorldSpace = true;
                _curveLineRenderer.startWidth = 0.45f;
                _curveLineRenderer.endWidth = 0.2f;
                _curveLineRenderer.sortingLayerName = "Skill";
                _curveLineRenderer.sortingOrder = 6;
                _curveLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            _curveLineRenderer.enabled = true;
            _curveLineRenderer.startColor = color;
            _curveLineRenderer.endColor = new Color(color.r, color.g, color.b, color.a * 0.4f);

            float totalLength = Mathf.Max(3.0f, _currentConfig.range * _currentPullPercent);
            float curveOffsetDistance = 1.8f; // Độ cong vòng cung sang ngang

            // Tính 3 điểm Bezier: Start (Chân Hero) -> Control (Đỉnh uốn cong) -> Apex (Điểm đích)
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector3 startPoint = origin;
            Vector3 apexPoint = origin + (Vector3)(direction * totalLength);
            Vector3 controlPoint = origin + (Vector3)(direction * (totalLength * 0.5f)) + (Vector3)(perpendicular * curveOffsetDistance);

            int segments = 20;
            _curveLineRenderer.positionCount = segments + 1;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                // Quadratic Bezier Formula: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
                Vector3 pointOnCurve = (1f - t) * (1f - t) * startPoint + 2f * (1f - t) * t * controlPoint + t * t * apexPoint;
                _curveLineRenderer.SetPosition(i, pointOnCurve);
            }

            // Điểm rơi / quay đầu của Boomerang
            float circleBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x : 1.0f;
            _circleIndicator.position = apexPoint;
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * (1.3f / circleBounds);
            _circleRenderer.color = color;
        }

        private void ShowRhythmPulseIndicator(Vector3 origin, Color color)
        {
            if (_circleRenderer == null) return;

            _circleRenderer.enabled = true;
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_coneRenderer != null) _coneRenderer.enabled = false;

            // Vòng tròn co bóp theo sóng sin nhịp điệu (Rhythm Beat)
            float baseRadius = Mathf.Max(2.0f, _currentConfig.radius);
            float pulseOffset = Mathf.PingPong(Time.unscaledTime * 3.5f, 0.8f);
            float currentRadius = baseRadius + pulseOffset;

            float spriteBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x : 1.0f;

            _circleIndicator.position = origin;
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * ((currentRadius * 2.0f) / spriteBounds);
            _circleRenderer.color = color;
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
            float spriteBounds = (_coneRenderer.sprite != null && _coneRenderer.sprite.bounds.size.x > 0.01f)
                ? _coneRenderer.sprite.bounds.size.x
                : 1.0f;

            float scale = (reach * 2.0f) / spriteBounds;

            // Nếu đang dùng Sector Shader chuyên dụng, cập nhật góc và màu sắc qua MaterialPropertyBlock
            if (_sectorMaterial != null && _coneRenderer.sharedMaterial == _sectorMaterial)
            {
                float arcAngle = _currentConfig.sectorAngle > 0f ? _currentConfig.sectorAngle : 90f;
                _conePropertyBlock.SetFloat(PropArcAngle, arcAngle);
                _conePropertyBlock.SetColor(PropTintColor, color);
                _conePropertyBlock.SetColor(PropBorderColor, _isCancelHovered ? _cancelAimColor : new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, 0.95f));
                _coneRenderer.SetPropertyBlock(_conePropertyBlock);

                _coneIndicator.position = origin;
                _coneIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
                _coneIndicator.localScale = Vector3.one * scale;
            }
            else
            {
                // Fallback nếu dùng Sprite thường
                float width = Mathf.Max(1.4f, _currentConfig.radius);
                _coneIndicator.position = origin + (Vector3)(direction * (reach * 0.45f));
                _coneIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
                _coneIndicator.localScale = new Vector3(reach / spriteBounds, width / spriteBounds, 1f);
                _coneRenderer.color = color;
            }
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

        private void ShowSelfAOEIndicator(Vector3 origin, Color color)
        {
            if (_circleRenderer == null) return;

            _circleRenderer.enabled = true;
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_coneRenderer != null) _coneRenderer.enabled = false;

            float radius = Mathf.Max(1.2f, _currentConfig.radius);
            float spriteBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x
                : 1.0f;
            float scale = (radius * 2.0f) / spriteBounds;

            _circleIndicator.position = origin;
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * scale;
            _circleRenderer.color = color;
        }

        private void ShowDashLineIndicator(Vector3 origin, Vector2 direction, float angle, Color color)
        {
            if (_lineRenderer == null || _circleRenderer == null) return;

            // Bật cả đường kẻ và vòng tròn điểm đáp
            _lineRenderer.enabled = true;
            _circleRenderer.enabled = true;
            if (_coneRenderer != null) _coneRenderer.enabled = false;

            float length = Mathf.Max(2.0f, _currentConfig.range);
            float width = 0.5f;

            float spriteBoundsX = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.x > 0.01f)
                ? _lineRenderer.sprite.bounds.size.x
                : 1.0f;
            float spriteBoundsY = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.y > 0.01f)
                ? _lineRenderer.sprite.bounds.size.y
                : 1.0f;

            _lineIndicator.position = origin + (Vector3)(direction * (length * 0.5f));
            _lineIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
            _lineIndicator.localScale = new Vector3(length / spriteBoundsX, width / spriteBoundsY, 1f);
            _lineRenderer.color = color;

            // Vòng tròn điểm đáp tại cuối đường lướt
            float landingRadius = 0.8f;
            float circleBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x
                : 1.0f;
            float circleScale = (landingRadius * 2.0f) / circleBounds;

            _circleIndicator.position = origin + (Vector3)(direction * length);
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * circleScale;
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
            if (_curveLineRenderer != null) _curveLineRenderer.enabled = false;
            if (_rangeBoundaryRenderer != null) _rangeBoundaryRenderer.enabled = false;
            if (_indicatorRoot != null) _indicatorRoot.SetActive(false);
        }
    }
}

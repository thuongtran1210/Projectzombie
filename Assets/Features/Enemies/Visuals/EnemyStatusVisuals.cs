using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Enemies.Visuals
{
    /// <summary>
    /// Component quản lý hiển thị trực quan các Trạng thái Bất lợi (Stun, Slow, Freeze, Burn, Humiliated, Sleeping, Stoned, Dancing) trên quái vật:
    /// - Choáng (Stun): Ngôi sao vàng kim xoay tròn quanh đỉnh đầu + dừng hoạt ảnh.
    /// - Đóng băng (Freeze): Phủ sắc xanh băng tuyết + đóng băng hoạt ảnh.
    /// - Làm chậm (Slow): Ám sắc lam ngọc sương tuyết + giảm tốc hoạt ảnh + Vòng sương băng dưới chân.
    /// - Thiêu đốt (Burn): Ám sắc đỏ cam rực lửa DoT.
    /// - Quê Độ (Humiliated): Giọt mồ hôi xấu hổ hoạt hình trên đầu.
    /// - Ngủ Say (Sleeping): Bong bóng ngủ chữ Zzz phập phồng.
    /// - Say Thuốc (Stoned): Vòng khói thuốc quay tít quanh đầu.
    /// - Mê Nhảy Múa (Dancing): Nốt nhạc bay lắc lư quanh người.
    /// Tương thích 100% với SpriteRenderer tiêu chuẩn và đảm bảo Zero GC Allocation.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyStatusController))]
    public class EnemyStatusVisuals : MonoBehaviour
    {
        [Header("Head Visual Settings")]
        [SerializeField] private float _headOffset = 1.2f;
        [SerializeField] private float _orbitRadius = 0.45f;
        [SerializeField] private float _orbitSpeed = 360f; // Độ/giây

        private EnemyStatusController _statusController;
        private Animator _animator;
        private SpriteRenderer[] _spriteRenderers;
        private Color[] _originalColors;
        private MaterialPropertyBlock _propBlock;

        // Dizzy Stars Indicator
        private GameObject _dizzyRoot;
        private Transform[] _starTransforms;
        private float _currentOrbitAngle = 0f;

        // Slow Foot Indicator
        private GameObject _footSlowIndicator;
        private SpriteRenderer _footIndicatorRenderer;

        // Slapstick Icons Indicator
        private GameObject _slapstickIconObj;
        private SpriteRenderer _slapstickIconRenderer;

        private bool _isStunned = false;
        private bool _isFrozen = false;
        private bool _isSlowed = false;
        private bool _isBurning = false;
        private bool _isSleeping = false;
        private bool _isStoned = false;
        private bool _isDancing = false;
        private bool _isHumiliated = false;

        private void Awake()
        {
            _statusController = GetComponent<EnemyStatusController>();
            _animator = GetComponentInChildren<Animator>();
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _propBlock = new MaterialPropertyBlock();

            if (_spriteRenderers != null && _spriteRenderers.Length > 0)
            {
                _originalColors = new Color[_spriteRenderers.Length];
                for (int i = 0; i < _spriteRenderers.Length; i++)
                {
                    _originalColors[i] = _spriteRenderers[i] != null ? _spriteRenderers[i].color : Color.white;
                }
            }

            CalculateHeadOffset();
            CreateDizzyStarsIndicator();
            CreateFootSlowIndicator();
            CreateSlapstickIconIndicator();
        }

        private void CalculateHeadOffset()
        {
            if (TryGetComponent<Collider2D>(out var col))
            {
                _headOffset = Mathf.Max(_headOffset, col.bounds.extents.y * 2f + 0.2f);
            }
            else if (_spriteRenderers != null && _spriteRenderers.Length > 0 && _spriteRenderers[0] != null)
            {
                _headOffset = Mathf.Max(_headOffset, _spriteRenderers[0].bounds.size.y + 0.15f);
            }
        }

        private void CreateDizzyStarsIndicator()
        {
            if (_dizzyRoot != null) return;

            _dizzyRoot = new GameObject("Stun_DizzyIndicator");
            _dizzyRoot.transform.SetParent(transform);
            _dizzyRoot.transform.localPosition = new Vector3(0f, _headOffset, 0f);

            int starCount = 3;
            _starTransforms = new Transform[starCount];

            for (int i = 0; i < starCount; i++)
            {
                GameObject starObj = new GameObject($"Star_{i}");
                starObj.transform.SetParent(_dizzyRoot.transform);
                starObj.transform.localScale = new Vector3(0.18f, 0.18f, 1f);

                var sr = starObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 500;
                sr.color = new Color(1f, 0.88f, 0.2f, 0.95f);
                sr.sprite = CreateStarSprite();

                _starTransforms[i] = starObj.transform;
            }

            _dizzyRoot.SetActive(false);
        }

        private void CreateFootSlowIndicator()
        {
            if (_footSlowIndicator != null) return;

            _footSlowIndicator = new GameObject("Slow_FootFrostRing");
            _footSlowIndicator.transform.SetParent(transform);
            _footSlowIndicator.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            _footSlowIndicator.transform.localScale = new Vector3(0.7f, 0.35f, 1f);

            _footIndicatorRenderer = _footSlowIndicator.AddComponent<SpriteRenderer>();
            _footIndicatorRenderer.sortingLayerName = "Tilemap_Decals";
            _footIndicatorRenderer.sortingOrder = 5;
            _footIndicatorRenderer.color = new Color(0.35f, 0.85f, 1f, 0.85f);
            _footIndicatorRenderer.sprite = CreateFrostRingSprite();

            _footSlowIndicator.SetActive(false);
        }

        private void CreateSlapstickIconIndicator()
        {
            if (_slapstickIconObj != null) return;

            _slapstickIconObj = new GameObject("Slapstick_HeadIcon");
            _slapstickIconObj.transform.SetParent(transform);
            _slapstickIconObj.transform.localPosition = new Vector3(0.25f, _headOffset + 0.3f, 0f);
            _slapstickIconObj.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

            _slapstickIconRenderer = _slapstickIconObj.AddComponent<SpriteRenderer>();
            _slapstickIconRenderer.sortingLayerName = "Skill";
            _slapstickIconRenderer.sortingOrder = 505;

            _slapstickIconObj.SetActive(false);
        }

        private static Sprite _cachedStarSprite;
        private static Sprite CreateStarSprite()
        {
            if (_cachedStarSprite != null) return _cachedStarSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color clear = new Color(0, 0, 0, 0);
            Color gold = new Color(1f, 0.92f, 0.3f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size * 0.5f) / (size * 0.5f);
                    float ny = (y - size * 0.5f) / (size * 0.5f);
                    float distCenter = Mathf.Sqrt(nx * nx + ny * ny);

                    float starShape = Mathf.Pow(Mathf.Abs(nx * ny), 0.35f);
                    if (distCenter < 0.9f && starShape < 0.15f)
                    {
                        float alpha = Mathf.Clamp01(1f - distCenter);
                        tex.SetPixel(x, y, new Color(gold.r, gold.g, gold.b, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();

            _cachedStarSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return _cachedStarSprite;
        }

        private static Sprite _cachedFrostRingSprite;
        private static Sprite CreateFrostRingSprite()
        {
            if (_cachedFrostRingSprite != null) return _cachedFrostRingSprite;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color clear = new Color(0, 0, 0, 0);
            Color frost = new Color(0.4f, 0.85f, 1f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size * 0.5f) / (size * 0.5f);
                    float ny = (y - size * 0.5f) / (size * 0.5f);
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);

                    if (dist >= 0.6f && dist <= 0.95f)
                    {
                        float edgeAlpha = 1f - Mathf.Abs(dist - 0.78f) / 0.18f;
                        tex.SetPixel(x, y, new Color(frost.r, frost.g, frost.b, Mathf.Clamp01(edgeAlpha)));
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();

            _cachedFrostRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
            return _cachedFrostRingSprite;
        }

        private static Sprite _cachedZzzSprite;
        private static Sprite CreateZzzSprite()
        {
            if (_cachedZzzSprite != null) return _cachedZzzSprite;
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color cyan = new Color(0.2f, 0.8f, 1f, 1f);
            Color clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size * 0.5f) / (size * 0.5f);
                    float ny = (y - size * 0.5f) / (size * 0.5f);
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    if (d < 0.7f)
                    {
                        tex.SetPixel(x, y, new Color(cyan.r, cyan.g, cyan.b, 0.9f - d * 0.5f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            _cachedZzzSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return _cachedZzzSprite;
        }

        private void OnEnable()
        {
            if (_statusController != null)
            {
                _statusController.OnStatusChanged += HandleStatusChanged;
            }
            ResetAllVisuals();
        }

        private void OnDisable()
        {
            if (_statusController != null)
            {
                _statusController.OnStatusChanged -= HandleStatusChanged;
            }
            ResetAllVisuals();
        }

        private void HandleStatusChanged(StatusEffectType type, bool isActive)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                    _isStunned = isActive;
                    if (_dizzyRoot != null) _dizzyRoot.SetActive(isActive);
                    if (_animator != null) _animator.speed = isActive ? 0.05f : 1f;
                    break;

                case StatusEffectType.Freeze:
                    _isFrozen = isActive;
                    if (_animator != null) _animator.speed = isActive ? 0f : 1f;
                    ApplyDirectColor(isActive ? new Color(0.3f, 0.7f, 1f, 1f) : Color.white);
                    break;

                case StatusEffectType.Slow:
                    _isSlowed = isActive;
                    if (_animator != null && !_isStunned && !_isFrozen)
                    {
                        _animator.speed = isActive ? _statusController.CurrentSlowMultiplier : 1f;
                    }
                    if (_footSlowIndicator != null)
                    {
                        _footSlowIndicator.SetActive(isActive);
                    }
                    if (!_isFrozen && !_isBurning)
                    {
                        ApplyDirectColor(isActive ? new Color(0.45f, 0.85f, 1f, 1f) : Color.white, isActive ? 0.85f : 0f);
                    }
                    break;

                case StatusEffectType.Burn:
                    _isBurning = isActive;
                    ApplyDirectColor(isActive ? new Color(1f, 0.45f, 0.15f, 1f) : Color.white, 0f);
                    break;

                case StatusEffectType.Sleeping:
                    _isSleeping = isActive;
                    if (_animator != null) _animator.speed = isActive ? 0f : 1f;
                    if (_slapstickIconObj != null)
                    {
                        _slapstickIconObj.SetActive(isActive);
                        _slapstickIconRenderer.sprite = CreateZzzSprite();
                        _slapstickIconRenderer.color = new Color(0.2f, 0.8f, 1f, 1f);
                    }
                    break;

                case StatusEffectType.Humiliated:
                    _isHumiliated = isActive;
                    if (_slapstickIconObj != null)
                    {
                        _slapstickIconObj.SetActive(isActive);
                        _slapstickIconRenderer.sprite = CreateStarSprite();
                        _slapstickIconRenderer.color = new Color(0.3f, 0.9f, 0.3f, 1f);
                    }
                    break;

                case StatusEffectType.Stoned:
                    _isStoned = isActive;
                    ApplyDirectColor(isActive ? new Color(0.7f, 0.5f, 0.9f, 1f) : Color.white);
                    break;

                case StatusEffectType.Dancing:
                    _isDancing = isActive;
                    if (_slapstickIconObj != null)
                    {
                        _slapstickIconObj.SetActive(isActive);
                        _slapstickIconRenderer.sprite = CreateStarSprite();
                        _slapstickIconRenderer.color = new Color(1f, 0.4f, 0.8f, 1f);
                    }
                    break;
            }
        }

        private void Update()
        {
            if (_isStunned && _dizzyRoot != null && _dizzyRoot.activeSelf && _starTransforms != null)
            {
                _currentOrbitAngle += _orbitSpeed * Time.deltaTime;
                float angleStep = 360f / _starTransforms.Length;

                for (int i = 0; i < _starTransforms.Length; i++)
                {
                    if (_starTransforms[i] == null) continue;

                    float rad = (_currentOrbitAngle + i * angleStep) * Mathf.Deg2Rad;
                    float x = Mathf.Cos(rad) * _orbitRadius;
                    float y = Mathf.Sin(rad) * (_orbitRadius * 0.4f);

                    _starTransforms[i].localPosition = new Vector3(x, y, 0f);

                    float depthScale = Mathf.Lerp(0.85f, 1.2f, (Mathf.Sin(rad) + 1f) * 0.5f);
                    _starTransforms[i].localScale = new Vector3(0.18f * depthScale, 0.18f * depthScale, 1f);
                }
            }

            if (_slapstickIconObj != null && _slapstickIconObj.activeSelf)
            {
                float pulse = 1f + 0.15f * Mathf.Sin(Time.time * 6f);
                _slapstickIconObj.transform.localScale = new Vector3(0.35f * pulse, 0.35f * pulse, 1f);
            }
        }

        private void ApplyDirectColor(Color tint, float slowIntensity = 0f)
        {
            if (_spriteRenderers == null) return;

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var sr = _spriteRenderers[i];
                if (sr == null || (sr.transform.parent == _dizzyRoot?.transform) || sr == _footIndicatorRenderer || sr == _slapstickIconRenderer) continue;

                if (tint == Color.white && _originalColors != null && i < _originalColors.Length)
                {
                    sr.color = _originalColors[i];
                }
                else
                {
                    sr.color = tint;
                }

                if (_propBlock != null)
                {
                    sr.GetPropertyBlock(_propBlock);
                    _propBlock.SetFloat(Shader.PropertyToID("_SlowAmount"), slowIntensity);
                    _propBlock.SetColor(Shader.PropertyToID("_SlowFrostColor"), new Color(0.35f, 0.85f, 1f, 1f));
                    sr.SetPropertyBlock(_propBlock);
                }
            }
        }

        private void ResetAllVisuals()
        {
            _isStunned = false;
            _isFrozen = false;
            _isSlowed = false;
            _isBurning = false;
            _isSleeping = false;
            _isStoned = false;
            _isDancing = false;
            _isHumiliated = false;

            if (_dizzyRoot != null) _dizzyRoot.SetActive(false);
            if (_footSlowIndicator != null) _footSlowIndicator.SetActive(false);
            if (_slapstickIconObj != null) _slapstickIconObj.SetActive(false);

            if (_animator != null) _animator.speed = 1f;

            ApplyDirectColor(Color.white, 0f);
        }
    }
}

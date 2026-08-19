using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Enemies.Visuals
{
    /// <summary>
    /// Component quản lý hiển thị trực quan các Trạng thái Bất lợi (Stun, Slow, Freeze, Burn) trên quái vật.
    /// - Choáng (Stun): Ngôi sao vàng kim xoay tròn quanh đỉnh đầu + dừng hoạt ảnh.
    /// - Đóng băng (Freeze): Phủ sắc xanh băng tuyết + đóng băng hoạt ảnh.
    /// - Làm chậm (Slow): Ám sắc xanh bùn / giảm tốc hoạt ảnh.
    /// - Thiêu đốt (Burn): Ám sắc đỏ cam rực lửa DoT.
    /// Đảm bảo Zero GC Allocation và tự động thích ứng với chiều cao của từng loại quái/Boss.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyStatusController))]
    public class EnemyStatusVisuals : MonoBehaviour
    {
        [Header("Stun Visual Settings")]
        [SerializeField] private float _headOffset = 1.2f;
        [SerializeField] private float _orbitRadius = 0.45f;
        [SerializeField] private float _orbitSpeed = 360f; // Độ/giây

        private EnemyStatusController _statusController;
        private Animator _animator;
        private SpriteRenderer[] _spriteRenderers;
        private MaterialPropertyBlock _propBlock;

        // Dizzy Stars Indicator
        private GameObject _dizzyRoot;
        private Transform[] _starTransforms;
        private float _currentOrbitAngle = 0f;
        private bool _isStunned = false;
        private bool _isFrozen = false;
        private bool _isSlowed = false;
        private bool _isBurning = false;

        private static readonly int _FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int _FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private void Awake()
        {
            _statusController = GetComponent<EnemyStatusController>();
            _animator = GetComponentInChildren<Animator>();
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _propBlock = new MaterialPropertyBlock();

            CalculateHeadOffset();
            CreateDizzyStarsIndicator();
        }

        private void CalculateHeadOffset()
        {
            // Tự động đo chiều cao dựa trên Collider hoặc Sprite
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
                int skillLayerId = SortingLayer.NameToID("Skill");
                int vfxLayerId = SortingLayer.NameToID("VFX");
                if (skillLayerId != 0) sr.sortingLayerName = "Skill";
                else if (vfxLayerId != 0) sr.sortingLayerName = "VFX";
                sr.sortingOrder = 500;
                sr.color = new Color(1f, 0.88f, 0.2f, 0.95f); // Vàng Hoàng Kim

                // Tạo icon ngôi sao 4 cánh bằng procedurally generated texture
                sr.sprite = CreateStarSprite();

                _starTransforms[i] = starObj.transform;
            }

            _dizzyRoot.SetActive(false);
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

                    // Hình dáng ngôi sao 4 cánh kim cương nhấp nháy
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
                    ApplyTint(isActive ? new Color(0.4f, 0.8f, 1f, 1f) : Color.white, isActive ? 0.6f : 0f);
                    break;

                case StatusEffectType.Slow:
                    _isSlowed = isActive;
                    if (_animator != null && !_isStunned && !_isFrozen)
                    {
                        _animator.speed = isActive ? _statusController.CurrentSlowMultiplier : 1f;
                    }
                    break;

                case StatusEffectType.Burn:
                    _isBurning = isActive;
                    ApplyTint(isActive ? new Color(1f, 0.35f, 0.1f, 1f) : Color.white, isActive ? 0.4f : 0f);
                    break;
            }
        }

        private void Update()
        {
            // Xoay ngôi sao choáng theo hình Elip quanh đỉnh đầu quái
            if (_isStunned && _dizzyRoot != null && _dizzyRoot.activeSelf && _starTransforms != null)
            {
                _currentOrbitAngle += _orbitSpeed * Time.deltaTime;
                float angleStep = 360f / _starTransforms.Length;

                for (int i = 0; i < _starTransforms.Length; i++)
                {
                    if (_starTransforms[i] == null) continue;

                    float rad = (_currentOrbitAngle + i * angleStep) * Mathf.Deg2Rad;
                    // Elip 2.5D: Chiều ngang rộng hơn chiều sâu Y
                    float x = Mathf.Cos(rad) * _orbitRadius;
                    float y = Mathf.Sin(rad) * (_orbitRadius * 0.4f);

                    _starTransforms[i].localPosition = new Vector3(x, y, 0f);

                    // Tỉ lệ scale nhấp nháy nhẹ theo vị trí trước/sau
                    float depthScale = Mathf.Lerp(0.85f, 1.2f, (Mathf.Sin(rad) + 1f) * 0.5f);
                    _starTransforms[i].localScale = new Vector3(0.18f * depthScale, 0.18f * depthScale, 1f);
                }
            }
        }

        private void ApplyTint(Color color, float amount)
        {
            if (_spriteRenderers == null) return;

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var sr = _spriteRenderers[i];
                if (sr == null) continue;

                sr.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(_FlashColorId, color);
                _propBlock.SetFloat(_FlashAmountId, amount);
                sr.SetPropertyBlock(_propBlock);
            }
        }

        private void ResetAllVisuals()
        {
            _isStunned = false;
            _isFrozen = false;
            _isSlowed = false;
            _isBurning = false;

            if (_dizzyRoot != null)
            {
                _dizzyRoot.SetActive(false);
            }

            if (_animator != null)
            {
                _animator.speed = 1f;
            }

            ApplyTint(Color.white, 0f);
        }
    }
}

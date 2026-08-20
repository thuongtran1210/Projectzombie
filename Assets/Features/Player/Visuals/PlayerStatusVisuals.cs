using UnityEngine;

namespace ProjectZombie.Features.Player.Visuals
{
    /// <summary>
    /// Quản lý hiển thị trực quan các hiệu ứng Trạng thái (Làm chậm - Slow, Đóng băng, Tăng tốc...) trên Player:
    /// - Ám sắc xanh lam ngọc sương tuyết khi bị làm chậm (Tương thích 100% mọi shader/SpriteRenderer).
    /// - Giảm tốc độ hoạt ảnh của nhân vật đồng bộ với tốc độ di chuyển thực tế.
    /// - Hiệu ứng vòng tròn sương băng dưới chân (Foot Frost Ring) trên Sorting Layer chuẩn.
    /// - Đảm bảo Zero GC Allocation trong suốt trận đấu.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStatusVisuals : MonoBehaviour
    {
        private PlayerController _playerController;
        private Animator _animator;
        private SpriteRenderer[] _spriteRenderers;
        private Color[] _originalColors;
        private MaterialPropertyBlock _propBlock;

        private GameObject _footSlowIndicator;
        private SpriteRenderer _footIndicatorRenderer;

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>() ?? GetComponent<PlayerController>();
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

            CreateFootSlowIndicator();
        }

        private void CreateFootSlowIndicator()
        {
            if (_footSlowIndicator != null) return;

            _footSlowIndicator = new GameObject("Slow_FootFrostRing");
            _footSlowIndicator.transform.SetParent(transform);
            _footSlowIndicator.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            _footSlowIndicator.transform.localScale = new Vector3(0.9f, 0.45f, 1f); // Dạng elip 2.5D mặt đất

            _footIndicatorRenderer = _footSlowIndicator.AddComponent<SpriteRenderer>();
            _footIndicatorRenderer.sortingLayerName = "Tilemap_Decals";
            _footIndicatorRenderer.sortingOrder = 10;
            _footIndicatorRenderer.color = new Color(0.35f, 0.88f, 1f, 0.9f);
            _footIndicatorRenderer.sprite = CreateRingSprite();

            _footSlowIndicator.SetActive(false);
        }

        private static Sprite _cachedRingSprite;
        private static Sprite CreateRingSprite()
        {
            if (_cachedRingSprite != null) return _cachedRingSprite;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color clear = new Color(0, 0, 0, 0);
            Color cyan = new Color(0.4f, 0.88f, 1f, 1f);

            float center = size * 0.5f;
            float outerR = size * 0.46f;
            float innerR = size * 0.28f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist >= innerR && dist <= outerR)
                    {
                        float ringAlpha = Mathf.Sin((dist - innerR) / (outerR - innerR) * Mathf.PI);
                        tex.SetPixel(x, y, new Color(cyan.r, cyan.g, cyan.b, ringAlpha * 0.9f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();

            _cachedRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
            return _cachedRingSprite;
        }

        private void OnEnable()
        {
            if (_playerController != null)
            {
                _playerController.OnSlowStatusChanged += HandleSlowStatusChanged;
            }
            ResetVisuals();
        }

        private void OnDisable()
        {
            if (_playerController != null)
            {
                _playerController.OnSlowStatusChanged -= HandleSlowStatusChanged;
            }
            ResetVisuals();
        }

        private void HandleSlowStatusChanged(bool isSlowed, float slowMultiplier)
        {
            if (_animator != null)
            {
                _animator.speed = isSlowed ? slowMultiplier : 1f;
            }

            if (_footSlowIndicator != null)
            {
                _footSlowIndicator.SetActive(isSlowed);
            }

            // Ám sắc xanh lam sương băng ngọc rõ rệt khi bị làm chậm
            ApplyDirectColor(isSlowed ? new Color(0.45f, 0.85f, 1f, 1f) : Color.white, isSlowed ? 0.85f : 0f);
        }

        private void ApplyDirectColor(Color tint, float slowIntensity = 0f)
        {
            if (_spriteRenderers == null) return;

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var sr = _spriteRenderers[i];
                if (sr == null || sr == _footIndicatorRenderer) continue;

                // 1. Direct SpriteRenderer color
                if (tint == Color.white && _originalColors != null && i < _originalColors.Length)
                {
                    sr.color = _originalColors[i];
                }
                else
                {
                    sr.color = tint;
                }

                // 2. Shader MaterialPropertyBlock support
                if (_propBlock != null)
                {
                    sr.GetPropertyBlock(_propBlock);
                    _propBlock.SetFloat(Shader.PropertyToID("_SlowAmount"), slowIntensity);
                    _propBlock.SetColor(Shader.PropertyToID("_SlowFrostColor"), new Color(0.35f, 0.85f, 1f, 1f));
                    sr.SetPropertyBlock(_propBlock);
                }
            }
        }

        private void ResetVisuals()
        {
            if (_animator != null) _animator.speed = 1f;
            if (_footSlowIndicator != null) _footSlowIndicator.SetActive(false);
            ApplyDirectColor(Color.white, 0f);
        }
    }
}

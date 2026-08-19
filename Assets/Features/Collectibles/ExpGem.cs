using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Core.Events;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Trạng thái hoạt động của hạt kinh nghiệm (FSM nhẹ, 0 GC allocation).
    /// </summary>
    public enum GemState
    {
        Spawning,
        Idle,
        AnticipationBounce,
        Homing,
        Collected
    }

    /// <summary>
    /// Hạt kinh nghiệm (ExpGem) rớt từ quái vật.
    /// Tối ưu hóa 100% không dùng DOTween trong runtime để triệt tiêu rác bộ nhớ (Zero GC).
    /// Hỗ trợ phân cấp màu sắc (Visual Tiering) và cơ chế gộp hạt (Gem Merging).
    /// </summary>
    public class ExpGem : MonoBehaviour
    {
        [Header("Experience & Motion Settings")]
        [SerializeField] private float expAmount = 10f;
        [SerializeField] private float initialFlySpeed = 0f;
        [SerializeField] private float maxFlySpeed = 35f;
        [SerializeField] private float flyAcceleration = 60f;

        [Header("Visual Tier Colors")]
        [SerializeField] private Color tier1Color = new Color(0.3f, 0.93f, 0.92f, 1f); // Cyan Lam Ngọc (< 30)
        [SerializeField] private Color tier2Color = new Color(0.31f, 0.89f, 0.76f, 1f); // Emerald Lục Bảo (30-99)
        [SerializeField] private Color tier3Color = new Color(0.61f, 0.32f, 0.88f, 1f); // Purple Tím U Minh (100-249)
        [SerializeField] private Color tier4Color = new Color(1f, 0.84f, 0f, 1f); // Gold Hoàng Kim (>= 250)

        [Header("Scale Settings")]
        [SerializeField] private float baseScaleMultiplier = 0.35f;

        private SpriteRenderer _spriteRenderer;
        private ExpGemPoolConfig _poolConfig;
        private Transform _targetPlayer;
        private GemState _state = GemState.Idle;

        private float _timer;
        private float _currentFlySpeed;
        private Vector3 _baseScale = new Vector3(0.35f, 0.35f, 1f);
        private Vector3 _targetScale = new Vector3(0.35f, 0.35f, 1f);
        private Vector3 _jumpStartPos;
        private Vector3 _jumpTargetPos;

        private const float SPAWN_DURATION = 0.2f;
        private const float BOUNCE_DURATION = 0.2f;
        private const float COLLECT_RADIUS_SQ = 0.25f; // 0.5f * 0.5f

        public float ExpAmount => expAmount;
        public bool IsIdle => _state == GemState.Idle;
        public bool IsActiveOnGround => _state == GemState.Idle || _state == GemState.Spawning;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _poolConfig = GetComponent<ExpGemPoolConfig>();
            _baseScale = new Vector3(baseScaleMultiplier, baseScaleMultiplier, 1f);
        }

        private void OnEnable()
        {
            // Reset toàn bộ trạng thái khi lấy từ Pool
            _state = GemState.Spawning;
            _timer = 0f;
            _targetPlayer = null;
            _currentFlySpeed = initialFlySpeed;
            transform.localScale = Vector3.zero;

            SetupVisualTier(expAmount);

            // Đăng ký vào PoolManager active list
            if (ExpGemPoolManager.Instance != null)
            {
                ExpGemPoolManager.Instance.RegisterActiveGem(this);
            }
        }

        private void OnDisable()
        {
            if (ExpGemPoolManager.Instance != null)
            {
                ExpGemPoolManager.Instance.UnregisterActiveGem(this);
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case GemState.Spawning:
                    UpdateSpawning();
                    break;

                case GemState.AnticipationBounce:
                    UpdateAnticipationBounce();
                    break;

                case GemState.Homing:
                    UpdateHoming();
                    break;

                case GemState.Idle:
                case GemState.Collected:
                default:
                    break;
            }
        }

        private void UpdateSpawning()
        {
            _timer += Time.deltaTime;
            float t = Mathf.Clamp01(_timer / SPAWN_DURATION);

            // OutBack easing bằng toán học: scale từ 0 lên 1 với độ nảy nhẹ
            float s = 1.70158f;
            float progress = 1f + (s + 1f) * Mathf.Pow(t - 1f, 3f) + s * Mathf.Pow(t - 1f, 2f);
            transform.localScale = _targetScale * Mathf.Max(0f, progress);

            if (_timer >= SPAWN_DURATION)
            {
                transform.localScale = _targetScale;
                _state = GemState.Idle;
            }
        }

        private void UpdateAnticipationBounce()
        {
            _timer += Time.deltaTime;
            float t = Mathf.Clamp01(_timer / BOUNCE_DURATION);

            // OutQuad easing
            float factor = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(_jumpStartPos, _jumpTargetPos, factor);

            if (_timer >= BOUNCE_DURATION)
            {
                _state = GemState.Homing;
                _currentFlySpeed = initialFlySpeed;
            }
        }

        private void UpdateHoming()
        {
            if (_targetPlayer == null)
            {
                _state = GemState.Idle;
                return;
            }

            // Gia tốc tốc độ bay
            _currentFlySpeed = Mathf.Min(_currentFlySpeed + flyAcceleration * Time.deltaTime, maxFlySpeed);
            transform.position = Vector3.MoveTowards(transform.position, _targetPlayer.position, _currentFlySpeed * Time.deltaTime);

            // Kiểm tra cự ly nhặt bằng khoảng cách bình phương (tránh hàm căn bậc hai Sqrt)
            Vector2 diff = transform.position - _targetPlayer.position;
            if (diff.sqrMagnitude <= COLLECT_RADIUS_SQ)
            {
                Collect();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Fallback nếu người chơi chưa gắn PlayerMagnetTrigger nhưng chạm trực tiếp thân nhân vật
            if (IsIdle && collision.CompareTag("Player"))
            {
                StartMagnetEffect(collision.transform);
            }
        }

        /// <summary>
        /// Kích hoạt hiệu ứng hút hạt về phía người chơi.
        /// </summary>
        public void StartMagnetEffect(Transform player)
        {
            if (_state == GemState.AnticipationBounce || _state == GemState.Homing || _state == GemState.Collected)
                return;

            _targetPlayer = player;
            _state = GemState.AnticipationBounce;
            _timer = 0f;
            _jumpStartPos = transform.position;

            // Tính hướng văng lùi nhẹ ngược với người chơi (tạo đà bay thỏa mãn)
            Vector3 dirAway = (_jumpStartPos - _targetPlayer.position).normalized;
            if (dirAway == Vector3.zero)
            {
                Vector2 rand = Random.insideUnitCircle.normalized;
                dirAway = new Vector3(rand.x, rand.y, 0f);
            }

            _jumpTargetPos = _jumpStartPos + dirAway * 1.2f;
        }

        private void Collect()
        {
            if (_state == GemState.Collected) return;
            _state = GemState.Collected;

            if (_targetPlayer != null)
            {
                PlayerExperience playerExp = _targetPlayer.GetComponent<PlayerExperience>();
                if (playerExp == null) playerExp = _targetPlayer.GetComponentInParent<PlayerExperience>();
                if (playerExp != null)
                {
                    playerExp.AddExp(expAmount);
                }
            }

            // Phát sự kiện toàn cục để AudioEventListener và Quest/Tracker xử lý
            GameEventBus.Publish(new ExpCollectedEvent(Mathf.RoundToInt(expAmount), transform.position));

            // Trả về Pool an toàn
            if (_poolConfig != null)
            {
                _poolConfig.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Cấu hình lượng kinh nghiệm và cập nhật Visual Tiering (Màu sắc & Kích cỡ).
        /// </summary>
        public void SetExpAmount(float amount)
        {
            expAmount = amount;
            SetupVisualTier(expAmount);
        }

        /// <summary>
        /// Gộp thêm kinh nghiệm từ một viên Gem khác vào viên này (Gem Merging Mechanism).
        /// </summary>
        public void MergeExp(float addedExp)
        {
            expAmount += addedExp;
            SetupVisualTier(expAmount);

            // Hiệu ứng nảy nhẹ thông báo nâng cấp cấp độ hạt
            _state = GemState.Spawning;
            _timer = 0f;
        }

        private void SetupVisualTier(float amount)
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

            if (amount < 30f)
            {
                // Tier 1: Common (Nhỏ gọn, vừa vặn bàn chân nhân vật)
                if (_spriteRenderer != null) _spriteRenderer.color = tier1Color;
                _targetScale = _baseScale * 1.0f;
            }
            else if (amount < 100f)
            {
                // Tier 2: Rare
                if (_spriteRenderer != null) _spriteRenderer.color = tier2Color;
                _targetScale = _baseScale * 1.12f;
            }
            else if (amount < 250f)
            {
                // Tier 3: Epic
                if (_spriteRenderer != null) _spriteRenderer.color = tier3Color;
                _targetScale = _baseScale * 1.22f;
            }
            else
            {
                // Tier 4: Legendary / Boss / Merged Gems
                if (_spriteRenderer != null) _spriteRenderer.color = tier4Color;
                _targetScale = _baseScale * 1.35f;
            }
        }
    }
}

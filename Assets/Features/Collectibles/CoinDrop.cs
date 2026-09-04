using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Đồng tiền / Cổ Tiền rớt từ quái vật.
    /// Vận hành Zero GC (không dùng DOTween), FSM nhẹ tương tự ExpGem.
    /// Hỗ trợ Visual Tiering (Đồng -> Bạc -> Vàng) và tự động bay về Player khi vào tầm hút.
    /// </summary>
    public class CoinDrop : MonoBehaviour, ICollectible, IPoolable
    {
        [Header("Coin & Value Settings")]
        [SerializeField] private int coinValue = 1;
        [SerializeField] private float initialFlySpeed = 0f;
        [SerializeField] private float maxFlySpeed = 36f;
        [SerializeField] private float flyAcceleration = 65f;

        [Header("Visual Tier Colors")]
        [SerializeField] private Color bronzeColor = new Color(0.85f, 0.53f, 0.28f, 1f); // Đồng (< 5)
        [SerializeField] private Color silverColor = new Color(0.85f, 0.90f, 0.95f, 1f); // Bạc (5 - 19)
        [SerializeField] private Color goldColor = new Color(1.0f, 0.84f, 0.0f, 1f);    // Hoàng Kim (>= 20)

        [Header("Scale Settings")]
        [SerializeField] private float baseScaleMultiplier = 0.35f;

        private SpriteRenderer _spriteRenderer;
        private CoinPoolConfig _poolConfig;
        private Transform _targetPlayer;
        private GemState _state = GemState.Idle;

        private float _timer;
        private float _currentFlySpeed;
        private Vector3 _baseScale = new Vector3(0.35f, 0.35f, 1f);
        private Vector3 _targetScale = new Vector3(0.35f, 0.35f, 1f);
        private Vector3 _jumpStartPos;
        private Vector3 _jumpTargetPos;

        private const float SPAWN_DURATION = 0.2f;
        private const float BOUNCE_DURATION = 0.18f;
        private const float COLLECT_RADIUS_SQ = 0.30f; // 0.55f * 0.55f

        public int CoinValue => coinValue;
        public bool IsIdle => _state == GemState.Idle;
        public bool IsActiveOnGround => _state == GemState.Idle || _state == GemState.Spawning;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _poolConfig = GetComponent<CoinPoolConfig>();
            _baseScale = new Vector3(baseScaleMultiplier, baseScaleMultiplier, 1f);
        }

        public void OnSpawn()
        {
            _state = GemState.Spawning;
            _timer = 0f;
            _targetPlayer = null;
            _currentFlySpeed = initialFlySpeed;
            transform.localScale = Vector3.zero;

            SetupVisualTier(coinValue);

            if (CoinPoolManager.Instance != null)
            {
                CoinPoolManager.Instance.RegisterActiveCoin(this);
            }
        }

        public void OnDespawn()
        {
            if (CoinPoolManager.Instance != null)
            {
                CoinPoolManager.Instance.UnregisterActiveCoin(this);
            }
        }

        private void OnEnable()
        {
            OnSpawn();
        }

        private void OnDisable()
        {
            OnDespawn();
        }

        private void Update()
        {
            switch (_state)
            {
                case GemState.Spawning:
                    UpdateSpawning();
                    break;
                case GemState.Idle:
                    UpdateIdleFloat();
                    break;
                case GemState.AnticipationBounce:
                    UpdateAnticipationBounce();
                    break;
                case GemState.Homing:
                    UpdateHomingFly();
                    break;
            }
        }

        private void UpdateSpawning()
        {
            _timer += Time.deltaTime;
            float progress = Mathf.Clamp01(_timer / SPAWN_DURATION);

            // Pop-up scale curve
            float scaleProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, _targetScale, scaleProgress);

            if (progress >= 1f)
            {
                _state = GemState.Idle;
                _timer = 0f;
            }
        }

        private void UpdateIdleFloat()
        {
            _timer += Time.deltaTime;
            // Float nhẹ bồng bềnh
            float floatOffset = Mathf.Sin(_timer * 3.5f) * 0.04f;
            transform.localPosition += new Vector3(0f, floatOffset * Time.deltaTime, 0f);
        }

        private void UpdateAnticipationBounce()
        {
            _timer += Time.deltaTime;
            float progress = Mathf.Clamp01(_timer / BOUNCE_DURATION);

            transform.position = Vector3.Lerp(_jumpStartPos, _jumpTargetPos, Mathf.SmoothStep(0f, 1f, progress));

            if (progress >= 1f)
            {
                _state = GemState.Homing;
                _timer = 0f;
                _currentFlySpeed = initialFlySpeed;
            }
        }

        private void UpdateHomingFly()
        {
            if (_targetPlayer == null)
            {
                _state = GemState.Idle;
                return;
            }

            Vector3 targetPos = _targetPlayer.position;
            Vector3 currentPos = transform.position;
            Vector3 direction = (targetPos - currentPos);
            float distSq = direction.sqrMagnitude;

            // Kiểm tra chạm người chơi để thu thập
            if (distSq <= COLLECT_RADIUS_SQ)
            {
                Collect();
                return;
            }

            _currentFlySpeed += flyAcceleration * Time.deltaTime;
            _currentFlySpeed = Mathf.Min(_currentFlySpeed, maxFlySpeed);

            transform.position = Vector3.MoveTowards(currentPos, targetPos, _currentFlySpeed * Time.deltaTime);
        }

        public void StartMagnetEffect(Transform player)
        {
            if (_state == GemState.Homing || _state == GemState.Collected) return;

            _targetPlayer = player;
            _state = GemState.AnticipationBounce;
            _timer = 0f;

            _jumpStartPos = transform.position;
            Vector3 dirAway = (_jumpStartPos - player.position).normalized;
            if (dirAway == Vector3.zero) dirAway = Random.insideUnitCircle.normalized;

            _jumpTargetPos = _jumpStartPos + dirAway * 0.8f;
        }

        public void Collect()
        {
            if (_state == GemState.Collected) return;
            _state = GemState.Collected;

            // Ghi nhận vào RunStatsTracker
            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.RegisterCoinCollected(coinValue);
            }

            // Trả về Pool
            if (_poolConfig != null)
            {
                _poolConfig.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetCoinValue(int value)
        {
            coinValue = value;
            SetupVisualTier(coinValue);
        }

        public void MergeCoin(int addedValue)
        {
            coinValue += addedValue;
            SetupVisualTier(coinValue);
            _state = GemState.Spawning;
            _timer = 0f;
        }

        private void SetupVisualTier(int value)
        {
            Color tierColor;
            float scaleMultiplier;

            if (value < 5)
            {
                tierColor = bronzeColor;
                scaleMultiplier = 1.0f;
            }
            else if (value < 20)
            {
                tierColor = silverColor;
                scaleMultiplier = 1.25f;
            }
            else
            {
                tierColor = goldColor;
                scaleMultiplier = 1.55f;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = tierColor;
            }

            _targetScale = _baseScale * scaleMultiplier;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_state != GemState.Idle && _state != GemState.Spawning) return;

            if (collision.CompareTag("Player"))
            {
                StartMagnetEffect(collision.transform);
            }
        }
    }
}

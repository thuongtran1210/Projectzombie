using UnityEngine;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Collectibles.PowerUps
{
    /// <summary>
    /// Thực thể vật phẩm bổ trợ hiển thị trên sàn đấu.
    /// Kế thừa ICollectible (tương thích 100% với PlayerMagnetTrigger) và IPoolable (Object Pooling).
    /// Đạt chuẩn 0 GC Allocation: chuyển động lơ lửng thuần toán học trong Update.
    /// </summary>
    public class PowerUpItem : MonoBehaviour, ICollectible, IPoolable
    {
        [Header("Effect Strategy")]
        [Tooltip("Hiệu ứng tác động lên Player khi nhặt được")]
        [SerializeField] private ScriptableObject _effectStrategyAsset;

        [Header("Motion & Visuals")]
        [Tooltip("Tốc độ bay hút về phía Player")]
        [SerializeField] private float _flySpeed = 16f;

        [Tooltip("Tốc độ lอย nhấp nhô")]
        [SerializeField] private float _bobFrequency = 4f;

        [Tooltip("Biên độ nhấp nhô")]
        [SerializeField] private float _bobAmplitude = 0.12f;

        private IPowerUpEffect _effectStrategy;
        private Transform _targetPlayer;
        private Vector3 _spawnPosition;
        private float _spawnTime;
        private float _despawnTime;
        private bool _isBeingMagnetized;
        private bool _isActiveOnGround;

        private const float COLLECT_DISTANCE_SQ = 0.36f; // 0.6m * 0.6m

        public bool IsActiveOnGround => _isActiveOnGround;

        private void Awake()
        {
            if (_effectStrategyAsset is IPowerUpEffect effect)
            {
                _effectStrategy = effect;
            }
        }

        public void SetEffectStrategy(IPowerUpEffect effect)
        {
            _effectStrategy = effect;
        }

        public void Initialize(float lifeTime)
        {
            _spawnPosition = transform.position;
            _spawnTime = Time.time;
            _despawnTime = lifeTime > 0f ? Time.time + lifeTime : float.MaxValue;
            _isBeingMagnetized = false;
            _isActiveOnGround = true;
            _targetPlayer = null;
        }

        private void Update()
        {
            if (!_isActiveOnGround) return;

            // 1. Kiểm tra hết thời gian tồn tại (Tự biến mất nếu không nhặt)
            if (Time.time >= _despawnTime && !_isBeingMagnetized)
            {
                ReturnToPool();
                return;
            }

            // 2. Chuyển động bay hút về phía Player
            if (_isBeingMagnetized && _targetPlayer != null)
            {
                Vector3 currentPos = transform.position;
                Vector3 targetPos = _targetPlayer.position;
                Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, _flySpeed * Time.deltaTime);
                transform.position = newPos;

                if ((newPos - targetPos).sqrMagnitude <= COLLECT_DISTANCE_SQ)
                {
                    Collect();
                }
            }
            else
            {
                // 3. Hiệu ứng lơ lửng nhấp nhô nhẹ nhàng (0 GC)
                float elapsed = Time.time - _spawnTime;
                float offsetY = Mathf.Sin(elapsed * _bobFrequency) * _bobAmplitude;
                transform.position = new Vector3(_spawnPosition.x, _spawnPosition.y + offsetY, _spawnPosition.z);
            }
        }

        #region ICollectible Implementation

        public void StartMagnetEffect(Transform target)
        {
            if (!_isActiveOnGround) return;
            _targetPlayer = target;
            _isBeingMagnetized = true;
        }

        public void Collect()
        {
            if (!_isActiveOnGround) return;
            _isActiveOnGround = false;

            GameObject playerObj = _targetPlayer != null ? _targetPlayer.gameObject : null;
            if (playerObj == null)
            {
                playerObj = Player.PlayerProvider.PlayerGameObject;
            }

            if (_effectStrategy != null && playerObj != null)
            {
                _effectStrategy.Apply(playerObj);
            }

            ReturnToPool();
        }

        #endregion

        #region IPoolable Implementation

        public void OnSpawn()
        {
            _isActiveOnGround = true;
            _isBeingMagnetized = false;
            _targetPlayer = null;
        }

        public void OnDespawn()
        {
            _isActiveOnGround = false;
            _isBeingMagnetized = false;
            _targetPlayer = null;
        }

        #endregion

        private void ReturnToPool()
        {
            _isActiveOnGround = false;
            if (PowerUpPoolManager.Instance != null)
            {
                PowerUpPoolManager.Instance.ReleaseItem(gameObject, this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}

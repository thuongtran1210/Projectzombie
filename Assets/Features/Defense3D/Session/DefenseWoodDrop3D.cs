using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Physical Wood pickup, collected only when the Hero approaches.</summary>
    public sealed class DefenseWoodDrop3D : DefenseGameplayBehaviour
    {
        [SerializeField, Min(1)] private int woodAmount = 10;
        [SerializeField, Min(1f)] private float lifetime = 90f;
        [SerializeField, Min(0.1f)] private float pickupRadius = 1.4f;
        [SerializeField, Min(0f)] private float pickupDelay = 0.75f;
        [Header("── MOBILE SMART MAGNET (Cơ chế Nam Châm Hút Gỗ) ──")]
        [Tooltip("Bán kính hút tự động khi Tướng di chuyển lại gần (mét)")]
        [SerializeField, Min(0.5f)] private float magnetRadius = 3.8f;
        [Tooltip("Tốc độ bay tối đa khi bị hút về phía Tướng (m/s)")]
        [SerializeField, Min(1f)] private float magnetSpeed = 9f;
        [Tooltip("Gia tốc tăng tốc khi bắt đầu bị hút")]
        [SerializeField, Min(1f)] private float magnetAcceleration = 14f;
        public const string ResourcePath = "Defense3D/DefenseWoodDrop3D";
        private Transform hero;
        private DefenseWoodWallet3D wallet;
        private DefenseHeroHealth3D _collectorHealth;
        private bool collected;
        private float expireTime;
        private float pickupReadyTime;
        private float _currentFlySpeed;
        private System.Action<DefenseWoodDrop3D> _poolRelease;
        private int _generation;
        public bool CanBeGathered(IDefenseGameplayContext gameplay) => ReferenceEquals(Gameplay, gameplay) &&
            isActiveAndEnabled && CanAdvanceGameplay && _generation == Gameplay.RunGeneration && !collected && GameplayTime >= pickupReadyTime;
        public bool TryGather(IDefenseGameplayContext gameplay, int capacity, out int amount)
        {
            amount = 0;
            if (capacity <= 0 || !CanBeGathered(gameplay)) return false;
            amount = Mathf.Min(woodAmount, capacity);
            woodAmount -= amount;
            if (woodAmount == 0) DespawnOrDestroy();
            return true;
        }

        public void SetPoolRelease(System.Action<DefenseWoodDrop3D> releaseAction)
        {
            _poolRelease = releaseAction;
        }

        public void Initialize(int amount, Transform collector, DefenseWoodWallet3D collectorWallet)
        {
            woodAmount = Mathf.Max(1, amount);
            _generation = Gameplay != null ? Gameplay.RunGeneration : -1;
            hero = collector;
            wallet = collectorWallet;
            _collectorHealth = collector != null ? collector.GetComponent<DefenseHeroHealth3D>() : null;
            expireTime = GameplayTime + lifetime;
            pickupReadyTime = GameplayTime + pickupDelay;
            _currentFlySpeed = 0f;
            collected = false;
        }

        private void DespawnOrDestroy()
        {
            if (collected) return;
            collected = true;
            _currentFlySpeed = 0f;
            if (_poolRelease != null)
            {
                _poolRelease(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Xóa dữ liệu lượt cũ và callback; pool giữ quyền thu hồi.</summary>
        public void ResetForPool()
        {
            collected = true;
            _currentFlySpeed = 0f;
            _poolRelease = null;
            hero = null; wallet = null; _collectorHealth = null;
            _generation = -1; expireTime = 0f; pickupReadyTime = 0f;
        }

        protected override void OnGameplayCleanup()
        {
            DespawnOrDestroy();
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || collected) return;
            if (GameplayTime >= expireTime)
            {
                DespawnOrDestroy();
                return;
            }
            if (GameplayTime < pickupReadyTime) return;
            if (hero == null || wallet == null || !hero.gameObject.activeInHierarchy) return;
            if (_collectorHealth != null && _collectorHealth.IsDown) return;

            Vector3 heroPos = hero.position;
            Vector3 currentPos = transform.position;
            Vector3 delta = heroPos - currentPos;
            delta.y = 0f;
            float sqrDist = delta.sqrMagnitude;

            // 1. Kiểm tra đã đến bán kính nhặt -> Thu thập gỗ vào ví
            if (sqrDist <= pickupRadius * pickupRadius)
            {
                wallet.CollectWood(woodAmount);
                DespawnOrDestroy();
                return;
            }

            // 2. Nam châm hút gỗ về phía Tướng (Smart Magnet Attraction)
            if (sqrDist <= magnetRadius * magnetRadius)
            {
                float dt = Time.deltaTime;
                _currentFlySpeed = Mathf.MoveTowards(_currentFlySpeed, magnetSpeed, magnetAcceleration * dt);
                Vector3 targetPos = new Vector3(heroPos.x, currentPos.y, heroPos.z);
                transform.position = Vector3.MoveTowards(currentPos, targetPos, _currentFlySpeed * dt);
            }
            else if (_currentFlySpeed > 0f)
            {
                _currentFlySpeed = Mathf.MoveTowards(_currentFlySpeed, 0f, magnetAcceleration * Time.deltaTime);
            }
        }

        public static DefenseWoodDrop3D Spawn(Vector3 deathPosition, int amount, Transform collector, DefenseWoodWallet3D collectorWallet,
            IDefenseGameplayContext gameplay = null, Transform parent = null, IDefenseWoodDrops spawner = null)
        {
            var spawnPos = new Vector3(deathPosition.x, Mathf.Max(0.3f, deathPosition.y - 0.65f), deathPosition.z);

            // Luồng production nhận spawner trực tiếp; static chỉ dành cho caller tương thích cũ.
            var source = spawner ?? DefenseWoodDropPool.ActiveSessionPool;
            if (gameplay != null && source != null)
                return source.Spawn(spawnPos, amount, collector, collectorWallet, gameplay, parent);

            // Fallback khi chạy unit test độc lập hoặc chưa khởi tạo session pool
            var prefab = Resources.Load<GameObject>(ResourcePath);
            if (prefab == null)
                throw new System.InvalidOperationException("Missing WoodDrop prefab: Resources/" + ResourcePath);
#if UNITY_EDITOR
            var drop = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
#else
            var drop = Object.Instantiate(prefab, parent);
#endif
            drop.name = "WoodDrop_" + amount;
            drop.transform.position = spawnPos;
            var pickup = drop.GetComponent<DefenseWoodDrop3D>();
            if (gameplay != null) pickup.BindGameplay(gameplay);
            pickup.Initialize(amount, collector, collectorWallet);
            return pickup;
        }
    }
}

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
        public const string ResourcePath = "Defense3D/DefenseWoodDrop3D";
        private Transform hero;
        private DefenseWoodWallet3D wallet;
        private DefenseHeroHealth3D _collectorHealth;
        private bool collected;
        private float expireTime;
        private float pickupReadyTime;
        private System.Action<DefenseWoodDrop3D> _poolRelease;

        public void SetPoolRelease(System.Action<DefenseWoodDrop3D> releaseAction)
        {
            _poolRelease = releaseAction;
        }

        public void Initialize(int amount, Transform collector, DefenseWoodWallet3D collectorWallet)
        {
            woodAmount = Mathf.Max(1, amount);
            hero = collector;
            wallet = collectorWallet;
            _collectorHealth = collector != null ? collector.GetComponent<DefenseHeroHealth3D>() : null;
            expireTime = GameplayTime + lifetime;
            pickupReadyTime = GameplayTime + pickupDelay;
            collected = false;
        }

        private void DespawnOrDestroy()
        {
            collected = true;
            if (_poolRelease != null)
            {
                _poolRelease(this);
            }
            else
            {
                Destroy(gameObject);
            }
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
            Vector3 delta = hero.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > pickupRadius * pickupRadius) return;
            wallet.CollectWood(woodAmount);
            DespawnOrDestroy();
        }

        public static DefenseWoodDrop3D Spawn(Vector3 deathPosition, int amount, Transform collector, DefenseWoodWallet3D collectorWallet,
            IDefenseGameplayContext gameplay = null, Transform parent = null)
        {
            var spawnPos = new Vector3(deathPosition.x, Mathf.Max(0.3f, deathPosition.y - 0.65f), deathPosition.z);

            // Nếu đang trong Gameplay session và có Pool kích hoạt, lấy từ pool để triệt tiêu 100% GC
            if (gameplay != null && DefenseWoodDropPool.ActiveSessionPool != null)
            {
                var pooledPickup = DefenseWoodDropPool.ActiveSessionPool.Get();
                if (parent != null) pooledPickup.transform.SetParent(parent, false);
                pooledPickup.transform.position = spawnPos;
                pooledPickup.BindGameplay(gameplay);
                pooledPickup.Initialize(amount, collector, collectorWallet);
                return pooledPickup;
            }

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

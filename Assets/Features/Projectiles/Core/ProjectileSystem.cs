using UnityEngine;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Projectiles.Components;

namespace ProjectZombie.Features.Projectiles.Core
{
    public class ProjectileSystem : MonoBehaviour
    {
        public static ProjectileSystem Instance { get; private set; }

        private ProjectileSpawner _spawner;
        public ProjectileEventDispatcher EventDispatcher { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSystem()
        {
            _spawner = gameObject.AddComponent<ProjectileSpawner>();
            EventDispatcher = new ProjectileEventDispatcher();
        }

        /// <summary>
        /// Spawn một projectile từ Data.
        /// </summary>
        public ProjectileController Spawn(ProjectileData data, Vector2 position, Vector2 direction, GameObject owner, DamageData damageOverride = default, int generation = 0)
        {
            return _spawner.SpawnProjectile(data, position, direction, owner, damageOverride, generation);
        }

        /// <summary>
        /// Prewarm pool thủ công, thường gọi từ LevelManager hoặc WeaponManager lúc bắt đầu.
        /// </summary>
        public void PrewarmPool(ProjectileData data)
        {
            _spawner?.PrewarmPool(data);
        }

        /// <summary>
        /// Thu hồi toàn bộ đạn / pháp bảo đang bay về Pool (dùng khi kết thúc trận đấu hoặc trở về Sảnh).
        /// </summary>
        public void DespawnAllProjectiles()
        {
            _spawner?.DespawnAllProjectiles();
        }
    }
}

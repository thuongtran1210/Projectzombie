using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Projectiles.Components;

namespace ProjectZombie.Features.Projectiles.Core
{
    public class ProjectileSpawner : MonoBehaviour
    {
        private Dictionary<string, ProjectilePool> _pools = new Dictionary<string, ProjectilePool>();

        public void PrewarmPool(ProjectileData data)
        {
            if (data == null || data.LogicPrefab == null) return;
            GetOrCreatePool(data); // GetOrCreatePool will handle the prewarm count defined in data
        }

        public ProjectileController SpawnProjectile(ProjectileData data, Vector2 position, Vector2 direction, GameObject owner, DamageData damageOverride, int generation = 0)
        {
            if (data == null || data.LogicPrefab == null)
            {
                Debug.LogError("ProjectileData or LogicPrefab is null!");
                return null;
            }

            var pool = GetOrCreatePool(data);
            var obj = pool.Get();

            if (obj == null)
            {
                Debug.LogWarning($"Pool for {data.ProjectileID} has reached its maximum size of {data.MaxPoolSize}!");
                return null; // Pool full
            }

            obj.transform.position = position;

            var controller = obj.GetComponent<ProjectileController>();
            if (controller == null)
            {
                controller = obj.AddComponent<ProjectileController>();
            }

            // Final damage context
            float finalBaseDamage = damageOverride.Amount > 0 ? damageOverride.Amount : data.BaseDamage;
            ElementType finalElement = damageOverride.Element;
            bool isCrit = damageOverride.IsCritical;
            Object sourceWeapon = damageOverride.SourceWeapon;

            DamageContext context = new DamageContext(owner, finalBaseDamage, finalElement, isCrit, sourceWeapon);

            controller.Initialize(data, direction, owner, context, pool, generation);

            // Tự động đồng bộ các chỉ số bổ trợ từ WeaponBase (Pierce, Speed, Scale)
            if (sourceWeapon is Weapons.WeaponBase wb)
            {
                controller.State.BonusPierce = wb.GetFinalPierce();
                controller.State.SpeedMultiplier = 1f + (wb.GetFinalProjectileSpeed() > 0 ? wb.GetFinalProjectileSpeed() * 0.08f : 0f);
                controller.State.ScaleMultiplier = wb.GetFinalScale();

                if (wb.GetFinalScale() != 1f)
                {
                    obj.transform.localScale = Vector3.one * wb.GetFinalScale();
                }
            }

            global::Core.Audio.AudioManager.Instance?.PlayProjectileShoot(position);

            return controller;
        }

        private ProjectilePool GetOrCreatePool(ProjectileData data)
        {
            if (_pools.TryGetValue(data.ProjectileID, out var pool))
            {
                return pool;
            }

            var poolObj = new GameObject($"Pool_{data.ProjectileID}");
            poolObj.transform.SetParent(transform);
            
            var newPool = poolObj.AddComponent<ProjectilePool>();
            newPool.Initialize(data.LogicPrefab, data.PrewarmCount, data.MaxPoolSize);
            
            _pools[data.ProjectileID] = newPool;
            return newPool;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Projectiles.Core;
using ProjectZombie.Features.Projectiles.Components;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Khung vũ khí sinh ra các vật thể xoay tròn xung quanh người chơi.
    /// Đã được refactor sang sử dụng hệ thống Đạn Data-Driven (ProjectileSystem) kết hợp Object Pooling.
    /// </summary>
    public class Weapon_Orbit : Weapon_ProjectileBase
    {
        [Header("Orbit Settings")]
        [SerializeField] private float baseRadius = 2f;
        [SerializeField] private int baseOrbCount = 1;

        private readonly List<ProjectileController> _activeOrbs = new List<ProjectileController>();
        private int _lastProjectileCount = -1;
        private float _lastScaleBonus = -1f;

        protected override void PerformAttack()
        {
            if (projectileData == null || ProjectileSystem.Instance == null || CharacterStats == null) return;

            int orbCount = Mathf.Max(1, baseOrbCount + localProjectileCountBonus);
            float scale = GetFinalScale();
            float angleStep = 360f / orbCount;
            DamageData damageData = DamageUtility.CalculateDamage(GetDamage(), GetFinalCritChance());
            Vector3 center = firePoint != null ? firePoint.position : transform.position;

            _activeOrbs.Clear();

            for (int i = 0; i < orbCount; i++)
            {
                float startAngle = i * angleStep;
                float rad = startAngle * Mathf.Deg2Rad;
                Vector2 initialDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector3 spawnPos = center + (Vector3)(initialDir * baseRadius);

                var orb = ProjectileSystem.Instance.Spawn(projectileData, spawnPos, initialDir, gameObject, damageData);
                if (orb != null)
                {
                    orb.transform.localScale = Vector3.one * scale;
                    _activeOrbs.Add(orb);
                }
            }
        }

        private void OnEnable()
        {
            if (ProjectileSystem.Instance != null && ProjectileSystem.Instance.EventDispatcher != null)
            {
                ProjectileSystem.Instance.EventDispatcher.OnProjectileDespawned += HandleOrbDespawned;
            }
        }

        private void OnDisable()
        {
            if (ProjectileSystem.Instance != null && ProjectileSystem.Instance.EventDispatcher != null)
            {
                ProjectileSystem.Instance.EventDispatcher.OnProjectileDespawned -= HandleOrbDespawned;
            }
            _activeOrbs.Clear();
        }

        private void HandleOrbDespawned(ProjectileController orb)
        {
            if (orb != null)
            {
                _activeOrbs.Remove(orb);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform center = firePoint != null ? firePoint : transform;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center.position, baseRadius);

            Gizmos.color = Color.yellow;
            int previewCount = Mathf.Max(1, baseOrbCount + localProjectileCountBonus);
            float angleStep = 360f / previewCount;
            for (int i = 0; i < previewCount; i++)
            {
                float rad = i * angleStep * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * baseRadius;
                Gizmos.DrawWireSphere(center.position + offset, 0.2f);
            }
        }
#endif
    }
}

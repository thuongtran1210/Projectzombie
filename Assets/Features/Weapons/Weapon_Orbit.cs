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
        [SerializeField] private float activeShockwaveRadius = 5.5f;

        private readonly List<ProjectileController> _activeOrbs = new List<ProjectileController>();
        private static readonly Collider2D[] _shockwaveHitBuffer = new Collider2D[30];

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 9.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Thần Âm Trảm Linh";
        }

        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.RhythmPulse, 0f, activeShockwaveRadius, 0f, false);

        protected override void PerformAttack()
        {
            if (projectileData == null || ProjectileSystem.Instance == null || CharacterStats == null) return;

            int orbCount = Mathf.Max(1, baseOrbCount + localProjectileCountBonus);
            float scale = GetFinalScale();
            float angleStep = 360f / orbCount;
            DamageData damageData = CreateDamageData();
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

            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(center);
        }

        /// <summary>
        /// Kỹ năng chủ động: Thần Âm Trảm Linh — Dậm sóng âm 360 độ cực đại gây choáng cứng 1.5s và đẩy lùi toàn bộ quái xung quanh.
        /// </summary>
        protected override void PerformActiveRelicSkill()
        {
            Vector3 center = firePoint != null ? firePoint.position : transform.position;
            float radius = activeShockwaveRadius * GetFinalScale();

            // Sinh đòn Orbit hỗ trợ
            PerformAttack();

            // Quét và gây choáng bầy quái xung quanh
            int mask = TargetingUtility.EnemyLayerMask;
            int numHits = Physics2D.OverlapCircleNonAlloc(center, radius, _shockwaveHitBuffer, mask);

            DamageData shockwaveDmg = CreateDamageData();
            shockwaveDmg = new DamageData(shockwaveDmg.Amount * 1.8f, true, ElementType.Tho, shockwaveDmg.IsCounter, this);

            for (int i = 0; i < numHits; i++)
            {
                var hit = _shockwaveHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(shockwaveDmg);
                }

                if (hit.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 pushDir = ((Vector2)hit.transform.position - (Vector2)center).normalized;
                    if (pushDir == Vector2.zero) pushDir = Vector2.up;
                    rb.AddForce(pushDir * 10f, ForceMode2D.Impulse);
                }
            }

            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(center);
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
            DespawnAllOrbs();
        }

        protected override void OnDestroy()
        {
            DespawnAllOrbs();
            base.OnDestroy();
        }

        public void DespawnAllOrbs()
        {
            for (int i = _activeOrbs.Count - 1; i >= 0; i--)
            {
                var orb = _activeOrbs[i];
                if (orb != null && orb.gameObject != null && orb.gameObject.activeInHierarchy)
                {
                    orb.Despawn();
                }
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

using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// W_PIPE — Điếu Cày Cửu U (Pháp Bảo Hỗ Trợ Khống Chế & Thiêu Đốt — Hệ Hỏa).
    /// - Tự động kích hoạt định kỳ: Rít điếu cày ùng ục và nhả đám mây khói thuốc rồng cuộn phía trước mặt.
    /// - Quái vật trong vùng khói bị thiêu đốt (Burn DoT) và dính hiệu ứng Say Thuốc Lào (Stoned: Giảm 60% tốc chạy & đi lảo đảo).
    /// </summary>
    public class Weapon_Pipe : WeaponBase
    {
        [Header("Pipe Settings")]
        [SerializeField] private float smokeDuration = 3.5f;
        [SerializeField] private float smokeRadius = 1.2f; // Bán kính vừa vặn 1.2m
        [SerializeField] private GameObject smokeVfxPrefab;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicSupportAura;
            isPrimaryActiveWeapon = false;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 9.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Bão Khói Thuốc Lào";
        }

        protected override void PerformAttack()
        {
            Vector2 forwardDir = transform.right;
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null)
                {
                    forwardDir = player.FacingVector;
                }
            }

            // Đẩy tâm cụm khói ra phía trước mặt 2.0m để KHÔNG CHE LẤP NHÂN VẬT
            Vector2 spawnPos = (Vector2)transform.position + forwardDir * 2.0f;

            if (smokeVfxPrefab != null)
            {
                float rotZ = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg;
                ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(smokeVfxPrefab, spawnPos, Quaternion.Euler(0, 0, rotZ), smokeDuration);
            }

            global::Core.Audio.AudioManager.Instance?.PlayStatusBurn(spawnPos);

            StartCoroutine(RoutineSmokeCloud(spawnPos, smokeRadius, smokeDuration));
        }

        /// <summary>
        /// Kỹ năng chủ động: Bão Khói Thuốc Lào — Rít hơi dài nhả bão khói diện rộng làm quái đi giật lùi và ho nổ sát thương.
        /// </summary>
        protected override void PerformActiveRelicSkill()
        {
            Vector2 forwardDir = transform.right;
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null) forwardDir = player.FacingVector;
            }

            Vector2 spawnPos = (Vector2)transform.position + forwardDir * 2.5f;

            if (smokeVfxPrefab != null)
            {
                float rotZ = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg;
                ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(smokeVfxPrefab, spawnPos, Quaternion.Euler(0, 0, rotZ), 5.0f);
            }

            global::Core.Audio.AudioManager.Instance?.PlayStatusBurn(spawnPos);
            StartCoroutine(RoutineSmokeCloud(spawnPos, smokeRadius * 1.8f, 5.0f));
        }

        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            // Nếu Hero chém trúng quái bạo kích: Kích hoạt thêm tia tàn lửa bốc cháy
            if (heroDamage.IsCritical && enemyHit != null)
            {
                ApplyBurnToArea(enemyHit.transform.position, 2.0f, 2.5f, heroDamage.Amount * 0.4f);
            }
        }

        private void ApplyBurnToArea(Vector2 center, float radius, float duration, float dps)
        {
            int mask = TargetingUtility.EnemyLayerMask;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, mask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Burn, duration, dps, 0.5f);
                }
            }
        }

        private IEnumerator RoutineSmokeCloud(Vector2 center, float radius, float duration)
        {
            float elapsed = 0f;
            int mask = TargetingUtility.EnemyLayerMask;

            DamageData smokeTickDmg = CreateDamageData();
            smokeTickDmg = new DamageData(smokeTickDmg.Amount * 0.4f, smokeTickDmg.IsCritical, ElementType.Hoa, smokeTickDmg.IsCounter, this);

            while (elapsed < duration)
            {
                elapsed += 0.5f;
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, mask);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].TryGetComponent<HealthSystem>(out var hp))
                    {
                        hp.TakeDamage(smokeTickDmg);
                    }
                    if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                    {
                        // Giảm thời lượng Say Thuốc xuống 0.8s (làm chậm và lảo đảo trong khi đứng trong khói)
                        status.ApplyStatusEffect(StatusEffectType.Stoned, 0.8f);
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}

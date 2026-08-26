using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// W_PIPE — Điếu Cày Cửu U (Vũ Khí Phun Khói Gây Lú / Say Thuốc Lào — Hệ Hỏa).
    /// Chuỗi Combo 3 Đòn:
    /// - Hit 1 (Gõ Cán Điếu): Gõ đầu điếu cày gây 100% DMG, đẩy lùi quái 1m.
    /// - Hit 2 (Búng Tàn Lửa): Búng tia tàn thuốc cháy rực ra xa gây 140% Fire DoT trong 2s.
    /// - Hit 3 (Khói Thần Rồng Cuộn): Rít một hơi dài nhả đám mây khói thuốc dày đặc tồn tại 3.5s; quái trúng khói bị Say Thuốc Lào (Stoned).
    /// </summary>
    public class Weapon_Pipe : Weapon_MeleeBase
    {
        [Header("Pipe Settings")]
        [SerializeField] private float smokeDuration = 3.5f;
        [SerializeField] private float smokeRadius = 2.8f;

        protected override void PerformAttack()
        {
            PerformComboAttack(CurrentComboStep);
        }

        protected override void PerformComboAttack(int step)
        {
            Vector2 attackDir = transform.right;
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null)
                {
                    if (player.MovementInput.sqrMagnitude > 0.01f)
                        attackDir = player.MovementInput.normalized;
                    else
                        attackDir = player.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
                }
            }

            Vector2 frontPos = (Vector2)transform.position + attackDir * 1.2f;

            switch (step)
            {
                case 1:
                    // Hit 1: Gõ cán điếu
                    DamageData dmg1 = CreateDamageData();
                    DealDamageInArea(frontPos, new Vector2(2.0f, 1.5f), 0f, dmg1, 3.5f);
                    break;

                case 2:
                    // Hit 2: Búng tàn lửa DoT
                    DamageData dmg2 = CreateDamageData();
                    dmg2 = new DamageData(dmg2.Amount * 1.4f, dmg2.IsCritical, ElementType.Hoa, dmg2.IsCounter, this);
                    DealDamageInArea(frontPos + attackDir * 0.8f, new Vector2(2.5f, 2.0f), 0f, dmg2, 2.0f);
                    ApplyBurnToArea(frontPos + attackDir * 0.8f, 1.5f, 2.0f, dmg2.Amount * 0.3f);
                    break;

                case 3:
                    // Hit 3: Khói Thần Rồng Cuộn (Say Thuốc Lào)
                    StartCoroutine(RoutineSmokeCloud(frontPos + attackDir * 1.2f));
                    break;
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

        private IEnumerator RoutineSmokeCloud(Vector2 center)
        {
            float elapsed = 0f;
            int mask = TargetingUtility.EnemyLayerMask;

            DamageData smokeTickDmg = CreateDamageData();
            smokeTickDmg = new DamageData(smokeTickDmg.Amount * 0.4f, smokeTickDmg.IsCritical, ElementType.Hoa, smokeTickDmg.IsCounter, this);

            while (elapsed < smokeDuration)
            {
                elapsed += 0.4f;
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, smokeRadius, mask);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].TryGetComponent<HealthSystem>(out var hp))
                    {
                        hp.TakeDamage(smokeTickDmg);
                    }
                    if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                    {
                        status.ApplyStatusEffect(StatusEffectType.Stoned, 2.5f);
                    }
                }
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}

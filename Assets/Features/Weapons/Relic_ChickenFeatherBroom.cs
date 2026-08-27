using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// R008 — Chổi Lông Gà Gia Truyền (Pháp Bảo Đòn Phạt Tuổi Thơ — Hệ Kim).
    /// - Kế thừa Weapon_OnHitRelicBase theo kiến trúc Composable.
    /// - Tự động kích nổ Chổi Quét Giáng Trời khi Hero tung đòn Chí Mạng hoặc kết thúc Combo Hit 3.
    /// </summary>
    public class Relic_ChickenFeatherBroom : Weapon_OnHitRelicBase
    {
        [Header("Broom Smash Settings")]
        [SerializeField] private float smashRadius = 3.5f;
        [SerializeField] private float smashKnockbackForce = 12f;

        private static readonly Collider2D[] _smashHitBuffer = new Collider2D[30];

        protected override void ExecuteOnHitEffect(DamageData heroDamage, Collider2D enemyHit)
        {
            // Nếu đòn đánh của Hero là đòn chí mạng: Giáng ngay Chổi Lông Gà vào vị trí kẻ địch
            if (heroDamage.IsCritical && enemyHit != null)
            {
                TriggerBroomSmash(enemyHit.transform.position);
            }
        }

        protected override void ExecuteFinisherEffect(int finalStep, Vector2 attackDirection)
        {
            // Khi Hero tung ra đòn kết liễu Combo Hit 3: Giáng Chổi Lông Gà vào vị trí phía trước mặt
            if (finalStep == 3)
            {
                Vector2 targetPos = (Vector2)transform.position + attackDirection * 2.5f;
                TriggerBroomSmash(targetPos);
            }
        }

        public void TriggerBroomSmash(Vector2 targetPos)
        {
            // Sinh VFX từ Object Pool tự thu hồi
            SpawnRelicVFX(targetPos, Quaternion.identity);

            int mask = TargetingUtility.EnemyLayerMask;
            int numHits = Physics2D.OverlapCircleNonAlloc(targetPos, smashRadius, _smashHitBuffer, mask);

            DamageData dmg = CreateDamageData();
            dmg = new DamageData(dmg.Amount * 1.8f, dmg.IsCritical, ElementType.Kim, dmg.IsCounter, this);

            for (int i = 0; i < numHits; i++)
            {
                var hit = _smashHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<HealthSystem>(out var hp))
                {
                    hp.TakeDamage(dmg);
                }

                if (hit.TryGetComponent<EnemyStatusController>(out var status))
                {
                    Vector2 knockbackDir = (hit.transform.position - (Vector3)targetPos).normalized;
                    if (knockbackDir.sqrMagnitude < 0.01f) knockbackDir = Vector2.up;

                    status.ApplyKnockback(knockbackDir, smashKnockbackForce, 0.35f);
                    status.ApplyStatusEffect(StatusEffectType.Stun, 0.8f);
                }
            }
        }

        protected override void PerformAttack()
        {
            TriggerBroomSmash(transform.position);
        }
    }
}

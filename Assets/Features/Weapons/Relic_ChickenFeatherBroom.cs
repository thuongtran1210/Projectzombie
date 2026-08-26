using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// R008 — Chổi Lông Gà Gia Truyền (Pháp Bảo Đòn Phạt Tuổi Thơ — Hệ Kim).
    /// - Kích hoạt khi đòn Combo Hit 3 của Vũ Khí Chính kết thúc.
    /// - Triệu hồi Chổi Lông Gà khổng lồ giáng từ trời xuống với lực Knockback cực đại 12m/s và găm dính quái vào tường (Wall Splat).
    /// </summary>
    public class Relic_ChickenFeatherBroom : WeaponBase
    {
        [Header("Broom Settings")]
        [SerializeField] private float smashRadius = 3.5f;
        [SerializeField] private float smashKnockbackForce = 12f;
        [SerializeField] private float cooldownSeconds = 4.0f;

        private float _lastSmashTime;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicOnHitTrigger;
            isPrimaryActiveWeapon = false;
        }

        protected override void PerformAttack()
        {
            TriggerBroomSmash(transform.position);
        }

        public void TriggerBroomSmash(Vector2 targetPos)
        {
            if (Time.time < _lastSmashTime + cooldownSeconds) return;
            _lastSmashTime = Time.time;

            int mask = TargetingUtility.EnemyLayerMask;
            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, smashRadius, mask);

            DamageData dmg = CreateDamageData();
            dmg = new DamageData(dmg.Amount * 1.8f, dmg.IsCritical, ElementType.Kim, dmg.IsCounter, this);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<HealthSystem>(out var hp))
                {
                    hp.TakeDamage(dmg);
                }

                if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                {
                    Vector2 knockbackDir = (hits[i].transform.position - (Vector3)targetPos).normalized;
                    if (knockbackDir.sqrMagnitude < 0.01f) knockbackDir = Vector2.up;
                    
                    // Lực hất văng cực mạnh
                    status.ApplyKnockback(knockbackDir, smashKnockbackForce, 0.35f);
                    status.ApplyStatusEffect(StatusEffectType.Stun, 0.8f);
                }
            }
        }
    }
}

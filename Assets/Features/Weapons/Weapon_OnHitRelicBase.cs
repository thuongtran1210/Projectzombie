using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Lớp cơ sở (Base Class) chuyên dụng cho các Pháp Bảo Kích Ứng (On-Hit / Combo Finisher Relics).
    /// Tự động quản lý Cooldown kích ứng, bắt sự kiện nhịp chém của Hero và tích hợp sẵn VFX Pool.
    /// Giúp việc tạo mới các Pháp Bảo bồi đòn trở nên cực kỳ tinh gọn, không duplicate boilerplate.
    /// </summary>
    public abstract class Weapon_OnHitRelicBase : WeaponBase
    {
        [Header("Relic Trigger Cooldown")]
        [Tooltip("Thời gian hồi giữa 2 lần bồi đòn kích ứng")]
        [SerializeField] protected float triggerCooldown = 1.0f;

        [Header("Relic Visual Effect")]
        [Tooltip("Prefab hiệu ứng tung chiêu của Pháp Bảo")]
        [SerializeField] protected GameObject relicVfxPrefab;
        [SerializeField] protected float vfxLifeTime = 0.5f;

        protected float _lastTriggerTime = -999f;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicOnHitTrigger;
            isPrimaryActiveWeapon = false;
        }

        /// <summary>
        /// Kiểm tra xem Pháp Bảo đã sẵn sàng bồi đòn chưa (Cooldown check).
        /// </summary>
        protected virtual bool IsTriggerReady()
        {
            return Time.time >= _lastTriggerTime + triggerCooldown;
        }

        protected void RecordTriggerTime()
        {
            _lastTriggerTime = Time.time;
        }

        /// <summary>
        /// Sinh hiệu ứng VFX từ Object Pool tự thu hồi.
        /// </summary>
        protected GameObject SpawnRelicVFX(Vector3 position, Quaternion rotation, int level = 1)
        {
            if (relicVfxPrefab != null)
            {
                return ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(relicVfxPrefab, position, rotation, vfxLifeTime, level);
            }
            return null;
        }

        /// <summary>
        /// Lắng nghe đòn đánh tay thường của Hero chém trúng quái.
        /// </summary>
        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            if (!IsTriggerReady() || enemyHit == null) return;
            
            RecordTriggerTime();
            ExecuteOnHitEffect(heroDamage, enemyHit);
        }

        /// <summary>
        /// Lắng nghe đòn chém kết thúc Combo (Hit 3 Finisher) của Hero.
        /// </summary>
        public override void OnHeroComboFinished(int finalStep, Vector2 attackDirection)
        {
            if (!IsTriggerReady()) return;

            RecordTriggerTime();
            ExecuteFinisherEffect(finalStep, attackDirection);
        }

        /// <summary>
        /// Override phương thức này để xử lý hành vi bồi đòn khi chém trúng quái.
        /// </summary>
        protected virtual void ExecuteOnHitEffect(DamageData heroDamage, Collider2D enemyHit) { }

        /// <summary>
        /// Override phương thức này để xử lý hành vi kích nổ khi kết thúc Combo 3.
        /// </summary>
        protected virtual void ExecuteFinisherEffect(int finalStep, Vector2 attackDirection) { }
    }
}

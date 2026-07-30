using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    using ProjectZombie.Features.Upgrades;

    /// <summary>
    /// Lớp gốc cho mọi loại vũ khí. Quản lý Object Pool, Thời gian hồi chiêu và thông số cơ bản.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Base Weapon Settings")]
        [SerializeField] protected Transform firePoint;
        
        [Header("Weapon Identity")]
        public string weaponId;
        public ElementType element = ElementType.None;

        public int WeaponLevel { get; private set; } = 1;
        public virtual int MaxLevel => 6;
        
        // Local bonuses applied from upgrades
        protected float localDamageBonus = 0f;
        protected float localAttackSpeedBonus = 0f;
        protected int localProjectileCountBonus = 0;
        protected int localPierceBonus = 0;
        protected float localScaleBonus = 0f;
        protected float localCritChanceBonus = 0f;
        protected float localCritDamageBonus = 0f;
        protected float localProjectileSpeedBonus = 0f;

        protected ICharacterStats CharacterStats;
        private float _lastAttackTime;

        public virtual void Initialize(ICharacterStats stats)
        {
            CharacterStats = stats;
            if (firePoint == null) firePoint = transform;
        }

        /// <summary>
        /// Giảm % thời gian hồi chiêu hiện tại của vũ khí (VD: 0.2f = giảm 20%).
        /// </summary>
        public void ReduceCurrentCooldown(float percentage)
        {
            float totalAttackSpeed = CharacterStats != null ? CharacterStats.AttackSpeed + localAttackSpeedBonus : 1f;
            float attackCooldown = 1f / Mathf.Max(0.01f, totalAttackSpeed);
            float remaining = (_lastAttackTime + attackCooldown) - Time.time;

            if (remaining > 0)
            {
                float reduction = remaining * Mathf.Clamp01(percentage);
                _lastAttackTime -= reduction;
            }
        }

        public void Tick()
        {
            if (CharacterStats == null) return;

            // Attack Speed calculation includes local bonus
            float totalAttackSpeed = CharacterStats.AttackSpeed + localAttackSpeedBonus;
            // Cooldown = 1 / AttackSpeed
            float attackCooldown = 1f / Mathf.Max(0.01f, totalAttackSpeed);

            if (Time.time >= _lastAttackTime + attackCooldown)
            {
                if (CanAttack())
                {
                    PerformAttack();
                    _lastAttackTime = Time.time;
                }
            }
        }


        public virtual float GetDamage()
        {
            return CharacterStats.GetTotalDamage() + localDamageBonus;
        }

        // --- Final Stat Getters for Projectiles ---
        
        public virtual float GetFinalDamage() => GetDamage();
        
        public virtual float GetFinalCritChance() => CharacterStats.CritChance + localCritChanceBonus;
        
        public virtual float GetFinalCritDamage() => 2.0f + localCritDamageBonus; // Mặc định 2.0 (200%)
        
        public virtual float GetFinalProjectileSpeed() => localProjectileSpeedBonus;
        
        public virtual int GetFinalProjectileCount() => 1 + localProjectileCountBonus; // Cơ bản bắn 1 viên
        
        public virtual int GetFinalPierce() => localPierceBonus;
        
        public virtual float GetFinalScale() => 1f + localScaleBonus; // Cơ bản scale 1
        
        // ------------------------------------------

        public virtual void ApplyStatModifier(WeaponStatModifier modifier)
        {
            if (WeaponLevel < MaxLevel)
            {
                WeaponLevel++;
            }
            
            localDamageBonus += modifier.damageBonus;
            localAttackSpeedBonus += modifier.attackSpeedBonus;
            localProjectileCountBonus += modifier.projectileCountBonus;
            localPierceBonus += modifier.pierceBonus;
            localScaleBonus += modifier.scaleBonus;
            localCritChanceBonus += modifier.critChanceBonus;
            localCritDamageBonus += modifier.critDamageBonus;
            localProjectileSpeedBonus += modifier.projectileSpeedBonus;
        }

        /// <summary>
        /// Gọi khi vũ khí thăng cấp. Các lớp con có thể override để thực hiện logic đặc biệt (như thay đổi đạn ở Level 4, 6).
        /// </summary>
        public virtual void OnLevelUp(int newLevel, UpgradeData appliedUpgrade)
        {
            // Mặc định không làm gì, để cho Weapon_RangedBase hoặc các vũ khí cụ thể xử lý
        }

        /// <summary>
        /// Logic tấn công chính. Các vũ khí con phải implement hàm này.
        /// </summary>
        protected abstract void PerformAttack();

        /// <summary>
        /// Cho phép vũ khí kiểm tra điều kiện (VD: có quái trong tầm hay không) trước khi tấn công.
        /// </summary>
        protected virtual bool CanAttack()
        {
            return true;
        }
    }
}

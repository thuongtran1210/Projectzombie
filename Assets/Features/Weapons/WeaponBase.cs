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
        public string displayName;
        public Sprite icon;
        [TextArea] public string description;
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
        /// Thời gian hồi chiêu còn lại hiện tại của vũ khí (giây).
        /// </summary>
        public float RemainingCooldown
        {
            get
            {
                float totalAttackSpeed = CharacterStats != null ? CharacterStats.AttackSpeed + localAttackSpeedBonus : 1f;
                float attackCooldown = 1f / Mathf.Max(0.01f, totalAttackSpeed);
                float remaining = (_lastAttackTime + attackCooldown) - Time.time;
                return Mathf.Max(0f, remaining);
            }
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

        [Header("Active / Passive Mode (Action RPG)")]
        [Tooltip("Vai trò chiến đấu của vũ khí/pháp bảo")]
        public WeaponRole weaponRole = WeaponRole.PrimaryWeapon;

        [Tooltip("Nếu là true: Vũ khí chính, chỉ xuất chiêu khi bấm Nút Đánh. Nếu là false: Pháp bảo hộ thân tự động kích hoạt.")]
        public bool isPrimaryActiveWeapon = false;

        [Header("Combo System (Action RPG)")]
        public int currentComboStep = 1;
        public virtual int MaxComboSteps => 3;
        public float comboResetWindow = 1.0f;
        private float _lastComboHitTime;

        public int CurrentComboStep => currentComboStep;

        /// <summary>
        /// Sự kiện phát ra khi đòn đánh của vũ khí trúng kẻ địch. Dành cho các Pháp bảo (Relics) On-Hit lắng nghe.
        /// </summary>
        public event System.Action<DamageData, Collider2D> OnHitEnemy;

        /// <summary>
        /// Gọi từ các đòn tấn công (Melee / Projectile) khi va chạm trúng kẻ địch.
        /// </summary>
        public void NotifyHitEnemy(DamageData damageData, Collider2D enemyCol)
        {
            OnHitEnemy?.Invoke(damageData, enemyCol);
        }

        public float GetTotalAttackSpeed()
        {
            return CharacterStats != null ? CharacterStats.AttackSpeed + localAttackSpeedBonus : 1f;
        }

        public void Tick()
        {
            if (CharacterStats == null) return;

            // Tự động reset combo về nhát 1 nếu quá thời gian chờ (Combo Window)
            if (currentComboStep > 1 && Time.time >= _lastComboHitTime + comboResetWindow)
            {
                currentComboStep = 1;
            }

            // Nếu là vũ khí chính chủ động, không tự động kích hoạt trong Tick
            if (isPrimaryActiveWeapon) return;

            // Attack Speed calculation includes local bonus
            float totalAttackSpeed = GetTotalAttackSpeed();
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

        /// <summary>
        /// Kích hoạt đòn đánh chủ động từ Nút Tấn Công theo chuỗi Combo.
        /// </summary>
        public virtual bool TriggerActiveComboAttack()
        {
            if (CharacterStats == null) return false;

            // Kiểm tra Cooldown cơ bản giữa các nhát chém (phụ thuộc AttackSpeed)
            float totalAttackSpeed = GetTotalAttackSpeed();
            float attackCooldown = (1f / Mathf.Max(0.01f, totalAttackSpeed)) * 0.4f; // Nhịp combo nhanh hơn 40% cooldown gốc

            if (Time.time < _lastAttackTime + attackCooldown)
            {
                return false;
            }

            // Kiểm tra reset combo
            if (Time.time >= _lastComboHitTime + comboResetWindow)
            {
                currentComboStep = 1;
            }

            if (!CanAttack()) return false;

            int executedStep = currentComboStep;
            PerformComboAttack(executedStep);

            _lastAttackTime = Time.time;
            _lastComboHitTime = Time.time;

            // Tăng bước combo tiếp theo
            currentComboStep = (currentComboStep % MaxComboSteps) + 1;
            return true;
        }

        /// <summary>
        /// Kích hoạt đòn đánh đơn lẻ (Tương thích ngược).
        /// </summary>
        public bool TriggerActiveAttack()
        {
            return TriggerActiveComboAttack();
        }

        /// <summary>
        /// Thực thi đòn đánh theo bước combo (1, 2, 3). Mặc định gọi lại PerformAttack().
        /// </summary>
        protected virtual void PerformComboAttack(int step)
        {
            PerformAttack();
        }


        public virtual float GetDamage()
        {
            return CharacterStats.GetTotalDamage() + localDamageBonus;
        }

        // --- Final Stat Getters for Projectiles ---
        
        public virtual float GetFinalDamage() => GetDamage();
        
        public virtual float GetFinalCritChance() => CharacterStats != null ? CharacterStats.CritChance + localCritChanceBonus : localCritChanceBonus;
        
        public virtual float GetFinalCritDamage() => 2.0f + localCritDamageBonus; // Mặc định 2.0 (200%)
        
        public virtual float GetFinalProjectileSpeed() => localProjectileSpeedBonus;
        
        public virtual int GetFinalProjectileCount() => 1 + localProjectileCountBonus; // Cơ bản bắn 1 viên
        
        public virtual int GetFinalPierce() => localPierceBonus;
        
        public virtual float GetFinalScale() => 1f + localScaleBonus; // Cơ bản scale 1
        
        public virtual ElementType AttackElement => element;

        public virtual DamageData CreateDamageData()
        {
            return DamageUtility.CalculateDamage(
                GetFinalDamage(), 
                GetFinalCritChance(), 
                GetFinalCritDamage(), 
                attackerElement: AttackElement,
                defenderElement: ElementType.None,
                sourceWeapon: this
            );
        }

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

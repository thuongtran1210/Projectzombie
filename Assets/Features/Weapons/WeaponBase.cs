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

        [Header("Active / Passive Mode (Hybrid Relic System v6.0)")]
        [Tooltip("Vai trò chiến đấu của vũ khí/pháp bảo")]
        public WeaponRole weaponRole = WeaponRole.PrimaryWeapon;

        [Tooltip("Nếu là true: Vũ khí chính, chỉ xuất chiêu khi bấm Nút Đánh. Nếu là false: Pháp bảo hộ thân.")]
        public bool isPrimaryActiveWeapon = false;

        [Tooltip("True: Pháp bảo tự động (Passive - Không hiện nút); False: Pháp bảo chủ động (Active - Hiện nút bấm kỹ năng)")]
        public bool isPassiveRelic = false;

        [Tooltip("Thời gian hồi chiêu khi kích hoạt chủ động (giây)")]
        public float activeCooldown = 8.0f;

        [Tooltip("Thời gian hiệu lực kỹ năng chủ động (giây). 0 nếu là chiêu thức tức thời (Instant Cast)")]
        public float activeDuration = 0f;

        [Tooltip("Tên chiêu thức chủ động của Pháp Bảo")]
        public string skillActionName;

        private float _lastRelicSkillCastTime = -999f;
        private float _relicSkillDurationEndTime = -999f;
        private float _lastEmittedRelicCd = -1f;

        public bool IsRelicSkillActive => Time.time < _relicSkillDurationEndTime;
        public float RelicRemainingCooldown => Mathf.Max(0f, (_lastRelicSkillCastTime + activeCooldown) - Time.time);
        public float RelicMaxCooldown => Mathf.Max(0.1f, activeCooldown);
        public bool IsRelicSkillReady => RelicRemainingCooldown <= 0f;

        public event System.Action<float, float> OnRelicCooldownUpdated;
        public event System.Action OnRelicSkillReady;
        public event System.Action OnRelicSkillExecuted;

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

        /// <summary>
        /// Lắng nghe khi đòn đánh tay của Hero (CharacterCombat) đánh trúng kẻ địch để Pháp Bảo kích ứng bồi đòn lập tức.
        /// </summary>
        public virtual void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            // Các Pháp bảo On-Hit có thể override để bồi đòn thêm
        }

        /// <summary>
        /// Lắng nghe khi Hero kết thúc chuỗi Combo (Đòn Hit 3) để kích hoạt hiệu ứng Finisher của Pháp Bảo.
        /// </summary>
        public virtual void OnHeroComboFinished(int finalStep, Vector2 attackDirection)
        {
            // Các Pháp bảo Finisher có thể override để tung chiêu lớn
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

            // Xử lý cho Pháp Bảo Chủ Động (Active Relic)
            if (!isPassiveRelic)
            {
                float remainingCd = RelicRemainingCooldown;
                if (Mathf.Abs(remainingCd - _lastEmittedRelicCd) > 0.05f || (remainingCd <= 0f && _lastEmittedRelicCd > 0f))
                {
                    _lastEmittedRelicCd = remainingCd;
                    OnRelicCooldownUpdated?.Invoke(remainingCd, RelicMaxCooldown);
                    if (remainingCd <= 0f)
                    {
                        OnRelicSkillReady?.Invoke();
                    }
                }

                // Nếu đang trong thời gian duy trì hiệu lực kỹ năng (Duration Buff)
                if (IsRelicSkillActive)
                {
                    TickRelicSkillDuration();
                }
                return;
            }

            // Xử lý cho Pháp Bảo Bị Động (Passive Relic) - Tự động bắn theo nhịp
            float totalAttackSpeed = GetTotalAttackSpeed();
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
        /// Kích hoạt Kỹ năng Chủ Động của Pháp Bảo khi người chơi nhấn nút Kỹ Năng Pháp Bảo.
        /// </summary>
        public virtual bool TriggerActiveRelicSkill()
        {
            if (CharacterStats == null) return false;
            if (isPassiveRelic) return false; // Pháp bảo bị động không thể kích hoạt bằng nút
            if (RelicRemainingCooldown > 0f) return false;

            _lastRelicSkillCastTime = Time.time;
            if (activeDuration > 0f)
            {
                _relicSkillDurationEndTime = Time.time + activeDuration;
            }

            PerformActiveRelicSkill();
            OnRelicSkillExecuted?.Invoke();
            OnRelicCooldownUpdated?.Invoke(RelicRemainingCooldown, RelicMaxCooldown);
            return true;
        }

        /// <summary>
        /// Thực thi chiêu thức chủ động khi bấm nút. Các Pháp bảo chủ động con có thể override hàm này.
        /// Mặc định nếu không override sẽ gọi PerformAttack().
        /// </summary>
        protected virtual void PerformActiveRelicSkill()
        {
            PerformAttack();
        }

        /// <summary>
        /// Gọi mỗi frame trong Tick khi kỹ năng chủ động đang trong thời gian duy trì hiệu lực (activeDuration > 0).
        /// </summary>
        protected virtual void TickRelicSkillDuration()
        {
            // Các pháp bảo dạng hào quang / bộc phát kéo dài có thể override
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

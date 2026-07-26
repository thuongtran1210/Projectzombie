using ProjectZombie.Features.Shared;
using System;
using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Quản lý toàn bộ chỉ số thực tế của Player trong lúc chơi.
    /// Khởi tạo dữ liệu từ HeroStatsConfig và cung cấp các hàm để thay đổi chỉ số (Buff/Passive).
    /// </summary>
    public class PlayerStats : MonoBehaviour, ICharacterStats
    {
        [SerializeField] private HeroStatsConfig baseStatsConfig;

        // Các chỉ số đang active
        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float DashCooldown { get; private set; }
        public float BaseDamage { get; private set; }
        public float AttackSpeed { get; private set; }
        public float CritChance { get; private set; }
        public float PickupRange { get; private set; }
        public float ExpMultiplier { get; private set; } = 1f;

        public float AttackRange => PickupRange; // Tái sử dụng PickupRange làm tầm đánh (hoặc cấu hình riêng)

        private float _damageMultiplier = 1f;

        // Sự kiện báo hiệu khi có chỉ số thay đổi (có thể dùng cho UI)
        public event Action OnStatsUpdated;

        private void Awake()
        {
            if (baseStatsConfig == null)
            {
                Debug.LogError("[PlayerStats] Thiếu HeroStatsConfig! Hãy kéo thả vào Inspector.");
                return;
            }

            InitStats();

            // Đồng bộ máu qua HealthSystem (Single Source of Truth)
            var healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(MaxHealth);
            }
        }

        private void Start()
        {
            ApplyCharacterPassives();
        }

        private void ApplyCharacterPassives()
        {
            if (baseStatsConfig != null && baseStatsConfig.characterPassives != null)
            {
                foreach (var passive in baseStatsConfig.characterPassives)
                {
                    if (passive != null)
                    {
                        passive.ApplyPassive(gameObject);
                    }
                }
            }
        }

        private void InitStats()
        {
            MaxHealth = baseStatsConfig.maxHealth;
            MoveSpeed = baseStatsConfig.moveSpeed;
            DashCooldown = baseStatsConfig.dashCooldown;
            BaseDamage = baseStatsConfig.baseDamage;
            AttackSpeed = baseStatsConfig.attackSpeed;
            CritChance = baseStatsConfig.critChance;
            PickupRange = baseStatsConfig.pickupRange;
            
            _damageMultiplier = 1f;
        }

        /// <summary>
        /// Sát thương cuối cùng (đã bao gồm các hệ số nhân từ buff).
        /// </summary>
        public float GetTotalDamage()
        {
            return BaseDamage * _damageMultiplier;
        }

        /// <summary>
        /// Cộng thêm hệ số nhân sát thương.
        /// (Ví dụ: +1% sát thương thì truyền 0.01f)
        /// </summary>
        public void AddDamageMultiplier(float amount)
        {
            _damageMultiplier += amount;
            OnStatsUpdated?.Invoke();
        }

        /// <summary>
        /// Đặt lại hệ số nhân sát thương.
        /// </summary>
        public void SetDamageMultiplier(float multiplier)
        {
            _damageMultiplier = multiplier;
            OnStatsUpdated?.Invoke();
        }

        public void AddExpMultiplier(float amount)
        {
            ExpMultiplier += amount;
            OnStatsUpdated?.Invoke();
        }

        public void SetExpMultiplier(float multiplier)
        {
            ExpMultiplier = multiplier;
            OnStatsUpdated?.Invoke();
        }

        public void AddMaxHealth(float amount)
        {
            MaxHealth += amount;
            // Cập nhật lên HealthSystem
            var healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(MaxHealth, false); // false để không hồi đầy máu tự động khi tăng max HP (tuỳ design)
            }
            OnStatsUpdated?.Invoke();
        }

        public void AddMoveSpeed(float amount)
        {
            MoveSpeed += amount;
            OnStatsUpdated?.Invoke();
        }

        public void AddCritChance(float amount)
        {
            CritChance += amount;
            OnStatsUpdated?.Invoke();
        }

        public void AddPickupRange(float amount)
        {
            PickupRange += amount;
            OnStatsUpdated?.Invoke();
        }

        public void AddBaseDamage(float amount)
        {
            BaseDamage += amount;
            OnStatsUpdated?.Invoke();
        }

        public void AddAttackSpeed(float amount)
        {
            AttackSpeed += amount;
            OnStatsUpdated?.Invoke();
        }
    }
}

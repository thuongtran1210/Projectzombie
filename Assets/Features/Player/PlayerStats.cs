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
        public float AttackRange { get; private set; } = 9.5f;

        private float _damageMultiplier = 1f;
        public float DamageMultiplier => _damageMultiplier;
        public float CritDamageMultiplier => 1.5f;

        // Sự kiện báo hiệu khi có chỉ số thay đổi (có thể dùng cho UI)
        public event Action OnStatsUpdated;

        private void Awake()
        {
            if (baseStatsConfig == null)
            {
                baseStatsConfig = Resources.Load<HeroStatsConfig>("HeroStatsConfig");
                #if UNITY_EDITOR
                if (baseStatsConfig == null)
                {
                    var configs = UnityEditor.AssetDatabase.FindAssets("t:HeroStatsConfig");
                    if (configs != null && configs.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(configs[0]);
                        baseStatsConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroStatsConfig>(path);
                    }
                }
                #endif
            }

            if (baseStatsConfig == null)
            {
                Debug.LogWarning("[PlayerStats] Thiếu HeroStatsConfig, sử dụng chỉ số mặc định: MoveSpeed=5, MaxHealth=100.");
                MaxHealth = 100f;
                MoveSpeed = 5f;
                DashCooldown = 2f;
                BaseDamage = 10f;
                AttackSpeed = 1f;
                CritChance = 0.05f;
                PickupRange = 2f;
                AttackRange = 9.5f;
                _damageMultiplier = 1f;
            }
            else
            {
                InitStats();
            }

            // Nạp toàn bộ chỉ số vĩnh viễn đã nâng cấp tại Miếu Tứ Bất Tử
            ApplyPermanentUpgrades();

            // Đồng bộ máu qua HealthSystem (Single Source of Truth)
            var healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(MaxHealth);
            }
        }

        private void ApplyPermanentUpgrades()
        {
            var saveData = ProjectZombie.Features.MetaProgression.MetaCurrencyManager.Instance != null 
                ? ProjectZombie.Features.MetaProgression.MetaCurrencyManager.Instance.GetSaveData() 
                : Core.Save.SaveSystem.Load();

            if (saveData == null || saveData.upgradeNodeLevels == null || saveData.upgradeNodeLevels.Length == 0)
                return;

            var treeData = Resources.Load<ProjectZombie.Features.MetaProgression.PermanentUpgradeTreeData>("PermanentUpgradeTree");
#if UNITY_EDITOR
            if (treeData == null)
            {
                treeData = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.MetaProgression.PermanentUpgradeTreeData>("Assets/_Data/Meta/PermanentUpgradeTree.asset");
            }
#endif
            if (treeData == null || treeData.nodes == null) return;

            for (int i = 0; i < treeData.nodes.Length; i++)
            {
                var node = treeData.nodes[i];
                int level = saveData.GetUpgradeLevel(i);
                if (level > 0 && node != null)
                {
                    MaxHealth += node.statBonusPerLevel.maxHealthBonus * level;
                    BaseDamage += node.statBonusPerLevel.baseDamageBonus * level;
                    MoveSpeed += node.statBonusPerLevel.moveSpeedBonus * level;
                    CritChance += node.statBonusPerLevel.critChanceBonus * level;
                    PickupRange += node.statBonusPerLevel.pickupRangeBonus * level;
                    ExpMultiplier += node.statBonusPerLevel.expMultiplierBonus * level;
                    AttackSpeed += node.statBonusPerLevel.attackSpeedBonus * level;
                    DashCooldown = Mathf.Max(0.4f, DashCooldown - node.statBonusPerLevel.dashCooldownReduction * level);
                }
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
            AttackRange = 9.5f;
            
            _damageMultiplier = 1f;
        }

        public void AddAttackRange(float amount)
        {
            AttackRange += amount;
            OnStatsUpdated?.Invoke();
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

        public void ReduceDashCooldown(float percentage)
        {
            DashCooldown = Mathf.Max(0.4f, DashCooldown * (1f - Mathf.Clamp01(percentage)));
            OnStatsUpdated?.Invoke();
        }
    }
}

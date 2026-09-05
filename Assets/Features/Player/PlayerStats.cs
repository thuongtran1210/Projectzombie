using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player.Stats;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Core.Save;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player
{
    public enum PlayerStatType
    {
        MaxHealth,
        MoveSpeed,
        DashCooldown,
        BaseDamage,
        AttackSpeed,
        CritChance,
        PickupRange,
        ExpMultiplier,
        AttackRange
    }

    /// <summary>
    /// Quản lý toàn bộ chỉ số thực tế của Player trong lúc chơi.
    /// Hỗ trợ cả cơ chế cộng trực tiếp và hệ thống StatModifier (Flat / PercentAdd / PercentMult).
    /// </summary>
    public class PlayerStats : MonoBehaviour, ICharacterStats
    {
        [SerializeField] private HeroStatsConfig baseStatsConfig;

        // Base Values (Chỉ số cơ bản chưa buff)
        private float _baseMaxHealth = 100f;
        private float _baseMoveSpeed = 5f;
        private float _baseDashCooldown = 2f;
        private float _baseDamage = 10f;
        private float _baseAttackSpeed = 1f;
        private float _baseCritChance = 0.05f;
        private float _basePickupRange = 2f;
        private float _baseExpMultiplier = 1f;
        private float _baseAttackRange = 9.5f;

        // Modifiers dictionary cho từng loại chỉ số
        private readonly Dictionary<PlayerStatType, List<StatModifier>> _statModifiers = new Dictionary<PlayerStatType, List<StatModifier>>();

        // Các chỉ số đang active (Tính toán sau khi áp dụng Modifiers)
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

        public const float MIN_MOVE_SPEED = 2.0f;
        public const float MAX_MOVE_SPEED = 9.0f;

        // Sự kiện báo hiệu khi có chỉ số thay đổi (dùng cho UI Presenters và HUD)
        public event Action OnStatsUpdated;

        private void Awake()
        {
            InitializeBaseConfig();
            InitStats();
            ApplyPermanentUpgrades();
            SyncHealthWithSystem(true);
        }

        private void Start()
        {
            ApplyCharacterPassives();
        }

        private void InitializeBaseConfig()
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
        }

        private void InitStats()
        {
            var selectedChar = RunLoadoutState.SelectedCharacter;
            if (selectedChar != null && selectedChar.baseMaxHealth > 0f)
            {
                _baseMaxHealth = selectedChar.baseMaxHealth;
                _baseMoveSpeed = selectedChar.baseMoveSpeed > 0f ? selectedChar.baseMoveSpeed : 5.0f;
                _baseDashCooldown = selectedChar.baseDashCooldown > 0f ? selectedChar.baseDashCooldown : 2.0f;
                _baseDamage = selectedChar.baseDamage > 0f ? selectedChar.baseDamage : 10.0f;
                _baseCritChance = selectedChar.baseCritChance > 0f ? selectedChar.baseCritChance : 0.05f;
                _baseAttackSpeed = (selectedChar.basicAttackConfig != null && selectedChar.basicAttackConfig.baseAttackSpeed > 0f) 
                    ? selectedChar.basicAttackConfig.baseAttackSpeed 
                    : (baseStatsConfig != null ? baseStatsConfig.attackSpeed : 1.0f);
                _basePickupRange = baseStatsConfig != null ? baseStatsConfig.pickupRange : 2.0f;
            }
            else if (baseStatsConfig != null)
            {
                _baseMaxHealth = baseStatsConfig.maxHealth;
                _baseMoveSpeed = baseStatsConfig.moveSpeed;
                _baseDashCooldown = baseStatsConfig.dashCooldown;
                _baseDamage = baseStatsConfig.baseDamage;
                _baseAttackSpeed = baseStatsConfig.attackSpeed;
                _baseCritChance = baseStatsConfig.critChance;
                _basePickupRange = baseStatsConfig.pickupRange;
            }
            else
            {
                _baseMaxHealth = 100f;
                _baseMoveSpeed = 5f;
                _baseDashCooldown = 2f;
                _baseDamage = 10f;
                _baseAttackSpeed = 1f;
                _baseCritChance = 0.05f;
                _basePickupRange = 2f;
            }

            _baseAttackRange = 9.5f;
            _baseExpMultiplier = 1f;
            _damageMultiplier = 1f;

            RecalculateAllStats();
        }

        /// <summary>
        /// Nạp toàn bộ chỉ số vĩnh viễn đã nâng cấp tại Miếu Tứ Bất Tử.
        /// </summary>
        public void ApplyPermanentUpgrades(
            MetaProgressionSaveData customSaveData = null, 
            PermanentUpgradeTreeData customTreeData = null)
        {
            var saveData = customSaveData;
            if (saveData == null)
            {
                saveData = MetaCurrencyManager.Instance != null 
                    ? MetaCurrencyManager.Instance.GetSaveData() 
                    : SaveSystem.Load();
            }

            if (saveData == null || saveData.upgradeNodeLevels == null || saveData.upgradeNodeLevels.Length == 0)
                return;

            var treeData = customTreeData;
            if (treeData == null)
            {
                treeData = Resources.Load<PermanentUpgradeTreeData>("PermanentUpgradeTree");
#if UNITY_EDITOR
                if (treeData == null)
                {
                    treeData = UnityEditor.AssetDatabase.LoadAssetAtPath<PermanentUpgradeTreeData>("Assets/_Data/Meta/PermanentUpgradeTree.asset");
                }
#endif
            }

            if (treeData == null || treeData.nodes == null) return;

            for (int i = 0; i < treeData.nodes.Length; i++)
            {
                var node = treeData.nodes[i];
                int level = saveData.GetUpgradeLevel(i);
                if (level > 0 && node != null)
                {
                    _baseMaxHealth += node.statBonusPerLevel.maxHealthBonus * level;
                    _baseDamage += node.statBonusPerLevel.baseDamageBonus * level;
                    _baseMoveSpeed += node.statBonusPerLevel.moveSpeedBonus * level;
                    _baseCritChance += node.statBonusPerLevel.critChanceBonus * level;
                    _basePickupRange += node.statBonusPerLevel.pickupRangeBonus * level;
                    _baseExpMultiplier += node.statBonusPerLevel.expMultiplierBonus * level;
                    _baseAttackSpeed += node.statBonusPerLevel.attackSpeedBonus * level;
                    _baseDashCooldown = Mathf.Max(0.4f, _baseDashCooldown - node.statBonusPerLevel.dashCooldownReduction * level);
                }
            }

            RecalculateAllStats();
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

        #region Stat Modifier Architecture

        public void AddModifier(PlayerStatType statType, StatModifier mod)
        {
            if (mod == null) return;
            if (!_statModifiers.TryGetValue(statType, out var list))
            {
                list = new List<StatModifier>();
                _statModifiers[statType] = list;
            }

            list.Add(mod);
            list.Sort((a, b) => a.Order.CompareTo(b.Order));
            RecalculateStat(statType);
            OnStatsUpdated?.Invoke();
        }

        public bool RemoveModifier(PlayerStatType statType, StatModifier mod)
        {
            if (mod == null) return false;
            if (_statModifiers.TryGetValue(statType, out var list))
            {
                if (list.Remove(mod))
                {
                    RecalculateStat(statType);
                    OnStatsUpdated?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void RemoveAllModifiersFromSource(object source)
        {
            if (source == null) return;
            bool changed = false;
            foreach (var kvp in _statModifiers)
            {
                int removed = kvp.Value.RemoveAll(mod => mod.Source == source);
                if (removed > 0)
                {
                    RecalculateStat(kvp.Key);
                    changed = true;
                }
            }

            if (changed)
            {
                OnStatsUpdated?.Invoke();
            }
        }

        private float CalculateFinalValue(float baseValue, PlayerStatType statType)
        {
            float finalValue = baseValue;
            float sumPercentAdd = 0f;

            if (_statModifiers.TryGetValue(statType, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var mod = list[i];
                    if (mod.Type == StatModType.Flat)
                    {
                        finalValue += mod.Value;
                    }
                    else if (mod.Type == StatModType.PercentAdd)
                    {
                        sumPercentAdd += mod.Value;
                        if (i + 1 >= list.Count || list[i + 1].Type != StatModType.PercentAdd)
                        {
                            finalValue *= 1f + sumPercentAdd;
                            sumPercentAdd = 0f;
                        }
                    }
                    else if (mod.Type == StatModType.PercentMult)
                    {
                        finalValue *= 1f + mod.Value;
                    }
                }
            }

            return finalValue;
        }

        private void RecalculateStat(PlayerStatType statType)
        {
            switch (statType)
            {
                case PlayerStatType.MaxHealth:
                    MaxHealth = CalculateFinalValue(_baseMaxHealth, PlayerStatType.MaxHealth);
                    SyncHealthWithSystem(false);
                    break;
                case PlayerStatType.MoveSpeed:
                    MoveSpeed = Mathf.Clamp(CalculateFinalValue(_baseMoveSpeed, PlayerStatType.MoveSpeed), MIN_MOVE_SPEED, MAX_MOVE_SPEED);
                    break;
                case PlayerStatType.DashCooldown:
                    DashCooldown = Mathf.Max(0.4f, CalculateFinalValue(_baseDashCooldown, PlayerStatType.DashCooldown));
                    break;
                case PlayerStatType.BaseDamage:
                    BaseDamage = CalculateFinalValue(_baseDamage, PlayerStatType.BaseDamage);
                    break;
                case PlayerStatType.AttackSpeed:
                    AttackSpeed = CalculateFinalValue(_baseAttackSpeed, PlayerStatType.AttackSpeed);
                    break;
                case PlayerStatType.CritChance:
                    CritChance = CalculateFinalValue(_baseCritChance, PlayerStatType.CritChance);
                    break;
                case PlayerStatType.PickupRange:
                    PickupRange = CalculateFinalValue(_basePickupRange, PlayerStatType.PickupRange);
                    break;
                case PlayerStatType.ExpMultiplier:
                    ExpMultiplier = CalculateFinalValue(_baseExpMultiplier, PlayerStatType.ExpMultiplier);
                    break;
                case PlayerStatType.AttackRange:
                    AttackRange = CalculateFinalValue(_baseAttackRange, PlayerStatType.AttackRange);
                    break;
            }
        }

        private void RecalculateAllStats()
        {
            MaxHealth = CalculateFinalValue(_baseMaxHealth, PlayerStatType.MaxHealth);
            MoveSpeed = Mathf.Clamp(CalculateFinalValue(_baseMoveSpeed, PlayerStatType.MoveSpeed), MIN_MOVE_SPEED, MAX_MOVE_SPEED);
            DashCooldown = Mathf.Max(0.4f, CalculateFinalValue(_baseDashCooldown, PlayerStatType.DashCooldown));
            BaseDamage = CalculateFinalValue(_baseDamage, PlayerStatType.BaseDamage);
            AttackSpeed = CalculateFinalValue(_baseAttackSpeed, PlayerStatType.AttackSpeed);
            CritChance = CalculateFinalValue(_baseCritChance, PlayerStatType.CritChance);
            PickupRange = CalculateFinalValue(_basePickupRange, PlayerStatType.PickupRange);
            ExpMultiplier = CalculateFinalValue(_baseExpMultiplier, PlayerStatType.ExpMultiplier);
            AttackRange = CalculateFinalValue(_baseAttackRange, PlayerStatType.AttackRange);
        }

        private void SyncHealthWithSystem(bool resetToFull)
        {
            var healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(MaxHealth, resetToFull);
            }
        }

        #endregion

        #region Backwards-Compatible Mutation Helpers

        public void AddAttackRange(float amount)
        {
            _baseAttackRange += amount;
            RecalculateStat(PlayerStatType.AttackRange);
            OnStatsUpdated?.Invoke();
        }

        public float GetTotalDamage()
        {
            return BaseDamage * _damageMultiplier;
        }

        public void AddDamageMultiplier(float amount)
        {
            _damageMultiplier += amount;
            OnStatsUpdated?.Invoke();
        }

        public void SetDamageMultiplier(float multiplier)
        {
            _damageMultiplier = multiplier;
            OnStatsUpdated?.Invoke();
        }

        public void AddExpMultiplier(float amount)
        {
            _baseExpMultiplier += amount;
            RecalculateStat(PlayerStatType.ExpMultiplier);
            OnStatsUpdated?.Invoke();
        }

        public void SetExpMultiplier(float multiplier)
        {
            _baseExpMultiplier = multiplier;
            RecalculateStat(PlayerStatType.ExpMultiplier);
            OnStatsUpdated?.Invoke();
        }

        public void AddMaxHealth(float amount)
        {
            _baseMaxHealth += amount;
            RecalculateStat(PlayerStatType.MaxHealth);
            OnStatsUpdated?.Invoke();
        }

        public void AddMoveSpeed(float amount)
        {
            _baseMoveSpeed += amount;
            RecalculateStat(PlayerStatType.MoveSpeed);
            OnStatsUpdated?.Invoke();
        }

        public void AddCritChance(float amount)
        {
            _baseCritChance += amount;
            RecalculateStat(PlayerStatType.CritChance);
            OnStatsUpdated?.Invoke();
        }

        public void AddPickupRange(float amount)
        {
            _basePickupRange += amount;
            RecalculateStat(PlayerStatType.PickupRange);
            OnStatsUpdated?.Invoke();
        }

        public void AddBaseDamage(float amount)
        {
            _baseDamage += amount;
            RecalculateStat(PlayerStatType.BaseDamage);
            OnStatsUpdated?.Invoke();
        }

        public void AddAttackSpeed(float amount)
        {
            _baseAttackSpeed += amount;
            RecalculateStat(PlayerStatType.AttackSpeed);
            OnStatsUpdated?.Invoke();
        }

        public void ReduceDashCooldown(float percentage)
        {
            _baseDashCooldown = Mathf.Max(0.4f, _baseDashCooldown * (1f - Mathf.Clamp01(percentage)));
            RecalculateStat(PlayerStatType.DashCooldown);
            OnStatsUpdated?.Invoke();
        }

        #endregion
    }
}

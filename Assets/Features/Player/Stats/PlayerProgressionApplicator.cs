using UnityEngine;
using ProjectZombie.Core.Save;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.Player.Stats
{
    /// <summary>
    /// Service chuyên trách áp dụng các chỉ số tiến trình Meta vĩnh viễn (Miếu Tứ Bất Tử & Cấp Sao Tướng)
    /// vào đối tượng PlayerStats khi bắt đầu trận đấu hoặc khi reset.
    /// Tách biệt hoàn toàn trách nhiệm nạp dữ liệu (Data Loading & Resolution) ra khỏi Core Domain Model PlayerStats (SRP & DIP).
    /// </summary>
    public static class PlayerProgressionApplicator
    {
        private static PermanentUpgradeTreeData _cachedTreeData;
        private static CharacterStarProgressionSO _cachedStarConfig;

        /// <summary>
        /// Nạp toàn bộ chỉ số vĩnh viễn đã nâng cấp tại Miếu Tứ Bất Tử vào PlayerStats.
        /// </summary>
        public static void ApplyPermanentUpgrades(
            PlayerStats stats,
            MetaProgressionSaveData customSaveData = null,
            PermanentUpgradeTreeData customTreeData = null)
        {
            if (stats == null) return;

            var saveData = customSaveData;
            if (saveData == null)
            {
                if (MetaCurrencyManager.Instance != null && MetaCurrencyManager.Instance.GetSaveData() != null)
                {
                    saveData = MetaCurrencyManager.Instance.GetSaveData();
                }
                else if (GameManager.Instance != null && GameManager.Instance.SaveData != null)
                {
                    saveData = GameManager.Instance.SaveData;
                }
                else
                {
                    saveData = SaveSystem.Load();
                }
            }

            if (saveData == null || saveData.upgradeNodeLevels == null || saveData.upgradeNodeLevels.Length == 0)
                return;

            var treeData = customTreeData ?? ResolvePermanentUpgradeTreeData();
            if (treeData == null || treeData.nodes == null) return;

            for (int i = 0; i < treeData.nodes.Length; i++)
            {
                var node = treeData.nodes[i];
                if (node == null) continue;

                int level = saveData.GetUpgradeLevel(i);
                if (level > 0)
                {
                    stats.AddMaxHealth(node.statBonusPerLevel.maxHealthBonus * level);
                    stats.AddBaseDamage(node.statBonusPerLevel.baseDamageBonus * level);
                    stats.AddMoveSpeed(node.statBonusPerLevel.moveSpeedBonus * level);
                    stats.AddCritChance(node.statBonusPerLevel.critChanceBonus * level);
                    stats.AddPickupRange(node.statBonusPerLevel.pickupRangeBonus * level);
                    stats.AddExpMultiplier(node.statBonusPerLevel.expMultiplierBonus * level);
                    stats.AddAttackSpeed(node.statBonusPerLevel.attackSpeedBonus * level);
                    if (node.statBonusPerLevel.dashCooldownReduction > 0f)
                    {
                        stats.ReduceDashCooldown(node.statBonusPerLevel.dashCooldownReduction * level);
                    }
                }
            }
        }

        /// <summary>
        /// Nạp bonus chỉ số từ Cấp Sao (1★ - 5★) của nhân vật đang được chọn vào trận.
        /// </summary>
        public static void ApplyCharacterStarProgression(
            PlayerStats stats,
            string characterId,
            CharacterStarProgressionSO customConfig = null,
            int customStar = -1)
        {
            if (stats == null) return;

            string targetHeroId = !string.IsNullOrEmpty(characterId) 
                ? characterId 
                : (RunLoadoutState.SelectedCharacter != null ? RunLoadoutState.SelectedCharacter.characterId : null);

            if (string.IsNullOrEmpty(targetHeroId)) return;

            var heroMgr = CharacterProgressionManager.Instance;
            int star = customStar >= 1 
                ? customStar 
                : (heroMgr != null ? heroMgr.GetCharacterStarLevel(targetHeroId) : 1);

            if (star <= 1) return;

            var configSO = customConfig ?? (heroMgr != null ? heroMgr.ProgressionConfig : null) ?? ResolveCharacterStarProgressionSO();
            if (configSO != null && configSO.StarSteps != null)
            {
                for (int s = 2; s <= star; s++)
                {
                    var step = configSO.GetStepConfig(s);
                    if (step != null)
                    {
                        if (step.healthMultiplierBonus > 0f) stats.AddMaxHealth(stats.MaxHealth * step.healthMultiplierBonus);
                        if (step.damageMultiplierBonus > 0f) stats.AddBaseDamage(stats.BaseDamage * step.damageMultiplierBonus);
                        if (step.moveSpeedMultiplierBonus > 0f) stats.AddMoveSpeed(stats.MoveSpeed * step.moveSpeedMultiplierBonus);
                        if (step.cooldownReductionBonus > 0f) stats.ReduceDashCooldown(step.cooldownReductionBonus);
                    }
                }
            }
        }

        private static PermanentUpgradeTreeData ResolvePermanentUpgradeTreeData()
        {
            if (_cachedTreeData != null) return _cachedTreeData;

            try
            {
                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync("PermanentUpgradeTree");
                var locations = locHandle.WaitForCompletion();
                if (locHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && locations != null && locations.Count > 0)
                {
                    var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<PermanentUpgradeTreeData>("PermanentUpgradeTree");
                    _cachedTreeData = handle.WaitForCompletion();
                }
                if (locHandle.IsValid()) UnityEngine.AddressableAssets.Addressables.Release(locHandle);
            }
            catch { }

            if (_cachedTreeData == null)
            {
                _cachedTreeData = Resources.Load<PermanentUpgradeTreeData>("PermanentUpgradeTree") 
                               ?? Resources.Load<PermanentUpgradeTreeData>("Meta/PermanentUpgradeTree");
            }
#if UNITY_EDITOR
            if (_cachedTreeData == null)
            {
                _cachedTreeData = UnityEditor.AssetDatabase.LoadAssetAtPath<PermanentUpgradeTreeData>("Assets/_Data/Meta/PermanentUpgradeTree.asset");
            }
#endif
            return _cachedTreeData;
        }

        private static CharacterStarProgressionSO ResolveCharacterStarProgressionSO()
        {
            if (_cachedStarConfig != null) return _cachedStarConfig;

            try
            {
                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync("CharacterStarProgressionConfig");
                var locations = locHandle.WaitForCompletion();
                if (locHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && locations != null && locations.Count > 0)
                {
                    var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<CharacterStarProgressionSO>("CharacterStarProgressionConfig");
                    _cachedStarConfig = handle.WaitForCompletion();
                }
                if (locHandle.IsValid()) UnityEngine.AddressableAssets.Addressables.Release(locHandle);
            }
            catch { }

            if (_cachedStarConfig == null)
            {
                _cachedStarConfig = Resources.Load<CharacterStarProgressionSO>("CharacterStarProgressionConfig");
            }
#if UNITY_EDITOR
            if (_cachedStarConfig == null)
            {
                _cachedStarConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterStarProgressionSO>("Assets/_Data/CharacterStarProgressionConfig.asset");
            }
#endif
            return _cachedStarConfig;
        }
    }
}

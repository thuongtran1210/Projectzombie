#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Editor.Studio
{
    /// <summary>
    /// Factory chuyên trách tạo nhanh các mẫu ScriptableObject Thẻ Nâng Cấp chuẩn ROGUELITE cho Game Designer.
    /// </summary>
    public static class UpgradeStudioTemplateFactory
    {
        private const string BASE_DIR = "Assets/_Data/Upgrades";

        public static UpgradeData CreateTemplate(UpgradeType type)
        {
            EnsureDirectoryExists(BASE_DIR);

            switch (type)
            {
                case UpgradeType.MythicCore:
                    return CreateMythicCoreTemplate();

                case UpgradeType.SynergyTrait:
                    return CreateSynergyTraitTemplate();

                case UpgradeType.BreakthroughUltimate:
                    return CreateMutationAugmentTemplate(AugmentTier.Silver);

                case UpgradeType.WeaponUpgrade:
                    return CreateWeaponUpgradeTemplate();

                case UpgradeType.EvolutionUpgrade:
                    return CreateEvolutionUpgradeTemplate();

                case UpgradeType.CommonUpgrade:
                case UpgradeType.RareUpgrade:
                case UpgradeType.ConditionalPassive:
                    return CreateStatMicroTemplate();

                case UpgradeType.RelicFusion:
                    return CreateFusionUpgradeTemplate();

                default:
                    return CreateStatMicroTemplate();
            }
        }

        public static MutationAugmentUpgradeData CreateMutationAugmentTemplate(AugmentTier tier)
        {
            string folder = $"{BASE_DIR}/MutationAugments";
            EnsureDirectoryExists(folder);

            string tierName = tier.ToString();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/AUGMENT_{tierName.ToUpper()}_NEW.asset");
            var asset = ScriptableObject.CreateInstance<MutationAugmentUpgradeData>();
            asset.id = $"AUG_{tierName.Substring(0, 1).ToUpper()}_NEW";
            asset.upgradeName = $"Lõi Đột Biến [{tierName}] Mới";
            asset.description = "Hiệu ứng bẻ gãy quy tắc hoặc cường hóa giao tranh bùng nổ...";
            asset.tier = tier;
            asset.upgradeType = UpgradeType.BreakthroughUltimate;
            asset.spawnWeight = tier switch
            {
                AugmentTier.Silver => 60f,
                AugmentTier.Gold => 30f,
                AugmentTier.Prismatic => 10f,
                _ => 30f
            };
            asset.maxLevel = 1;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static StatMicroUpgradeData CreateStatMicroTemplate()
        {
            string folder = $"{BASE_DIR}/MicroStats";
            EnsureDirectoryExists(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/MICRO_STAT_NEW.asset");
            var asset = ScriptableObject.CreateInstance<StatMicroUpgradeData>();
            asset.id = "STAT_MICRO_NEW";
            asset.upgradeName = "Tên Thẻ Chỉ Số";
            asset.oneLineSummary = "+15% Tốc Đánh & +5% Sát Thương";
            asset.description = "Tăng cường năng lực nền tảng cho nhân vật, thay thế trang bị.";
            asset.upgradeType = UpgradeType.CommonUpgrade;
            asset.spawnWeight = 60f;
            asset.maxLevel = 5;
            asset.statModifier = new PlayerStatModifier
            {
                attackSpeedBonus = 0.15f,
                baseDamageBonus = 5f
            };

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static MythicCoreUpgradeData CreateMythicCoreTemplate()
        {
            string folder = $"{BASE_DIR}/Mythic";
            EnsureDirectoryExists(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/CORE_NEW_MYTHIC.asset");
            var asset = ScriptableObject.CreateInstance<MythicCoreUpgradeData>();
            asset.id = "CORE_NEW";
            asset.upgradeName = "Tên Lõi Thần Thoại";
            asset.mythicTitle = "DANH HIỆU THẦN THOẠI";
            asset.description = "Mô tả nội tại uy lực của Lõi...";
            asset.upgradeType = UpgradeType.MythicCore;
            asset.spawnWeight = 1f;
            asset.maxLevel = 1;
            asset.element = ElementType.Hoa;
            asset.archetype = MythicArchetype.PhuDongThienUy;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static SynergyTraitUpgradeData CreateSynergyTraitTemplate()
        {
            string folder = $"{BASE_DIR}/Mythic/Traits";
            EnsureDirectoryExists(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/TRAIT_NEW_01.asset");
            var asset = ScriptableObject.CreateInstance<SynergyTraitUpgradeData>();
            asset.id = "TRAIT_NEW_01";
            asset.upgradeName = "Tên Thần Binh Thuật";
            asset.description = "+15% Sát thương và tăng cường năng lực đặc thù của Lõi.";
            asset.upgradeType = UpgradeType.SynergyTrait;
            asset.spawnWeight = 10f;
            asset.maxLevel = 3;
            asset.requiredArchetype = MythicArchetype.PhuDongThienUy;
            asset.playerStatModifier = new PlayerStatModifier
            {
                baseDamageBonus = 5f,
                critChanceBonus = 0.05f
            };

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static WeaponUpgradeData CreateWeaponUpgradeTemplate()
        {
            string folder = $"{BASE_DIR}/Weapons";
            EnsureDirectoryExists(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/W001_Lv2_NewUpgrade.asset");
            var asset = ScriptableObject.CreateInstance<WeaponUpgradeData>();
            asset.id = "W001_Lv2";
            asset.upgradeName = "Cường Hóa Pháp Bảo [Cấp 2]";
            asset.description = "+25% Sát thương và +1 Số lượng tia đạn.";
            asset.upgradeType = UpgradeType.WeaponUpgrade;
            asset.spawnWeight = 10f;
            asset.maxLevel = 5;
            asset.weaponId = "W001";
            asset.requiredCurrentLevel = 1;
            asset.statModifier = new WeaponStatModifier
            {
                damageBonus = 5f,
                projectileCountBonus = 1
            };

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static EvolutionUpgradeData CreateEvolutionUpgradeTemplate()
        {
            string folder = $"{BASE_DIR}/Evolutions";
            EnsureDirectoryExists(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/E001_NewEvolution.asset");
            var asset = ScriptableObject.CreateInstance<EvolutionUpgradeData>();
            asset.id = "E001";
            asset.upgradeName = "Tên Thần Binh Tiến Hóa";
            asset.description = "Bắn liên hoàn các luồng thần pháp hủy diệt quét sạch kẻ địch.";
            asset.upgradeType = UpgradeType.EvolutionUpgrade;
            asset.spawnWeight = 100f;
            asset.maxLevel = 1;
            asset.weaponId = "W001";
            asset.requiredCurrentLevel = 5;
            asset.requiredPassiveId = "P001";

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static CommonUpgradeData CreateCommonUpgradeTemplate()
        {
            string folder = $"{BASE_DIR}/Passives";
            EnsureDirectoryExists(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/P001_NewPassive.asset");
            var asset = ScriptableObject.CreateInstance<CommonUpgradeData>();
            asset.id = "P001";
            asset.upgradeName = "Tên Bổ Trợ Khí Vận";
            asset.description = "+10% Sát thương cơ bản toàn thể cho nhân vật.";
            asset.upgradeType = UpgradeType.CommonUpgrade;
            asset.spawnWeight = 10f;
            asset.maxLevel = 5;
            asset.playerStatModifier = new PlayerStatModifier
            {
                baseDamageBonus = 3f
            };

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static FusionUpgradeData CreateFusionUpgradeTemplate()
        {
            string folder = $"{BASE_DIR}/RelicFusions";
            EnsureDirectoryExists(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/FUSION_NEW.asset");
            var asset = ScriptableObject.CreateInstance<FusionUpgradeData>();
            asset.id = "F001";
            asset.upgradeName = "Dung Hợp Thần Binh";
            asset.description = "Dung hợp 2 bảo vật tạo thành thần khí vô thượng.";
            asset.upgradeType = UpgradeType.RelicFusion;
            asset.spawnWeight = 50f;
            asset.maxLevel = 1;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static void EnsureDirectoryExists(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}
#endif

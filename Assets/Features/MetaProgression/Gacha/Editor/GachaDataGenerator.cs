#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression.Gacha.Data;

namespace ProjectZombie.Features.MetaProgression.Gacha.Editor
{
    /// <summary>
    /// Editor Tool quét toàn bộ WeaponData trong dự án và tự động sinh GachaBannerConfigSO chuẩn mực.
    /// </summary>
    public static class GachaDataGenerator
    {
        [MenuItem("ProjectZombie/Gacha/Generate Default Gacha Banner SO", priority = 200)]
        public static void GenerateDefaultGachaBanner()
        {
            string outputDir = "Assets/Resources/Gacha";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string assetPath = $"{outputDir}/banner_standard.asset";
            var banner = AssetDatabase.LoadAssetAtPath<GachaBannerConfigSO>(assetPath);

            if (banner == null)
            {
                banner = ScriptableObject.CreateInstance<GachaBannerConfigSO>();
                AssetDatabase.CreateAsset(banner, assetPath);
            }

            banner.bannerId = "banner_standard";
            banner.bannerName = "Bảo Rương Vạn Cổ";
            banner.bannerDescription = "Mở rương thu thập Mảnh Pháp Bảo Thần Binh viễn cổ, gia tăng uy lực vĩnh viễn.";
            banner.singleRollCost = 100;
            banner.multiRollCost = 900;
            banner.hardPityLegendary = 50;
            banner.softPityStart = 40;
            banner.softPityRatePerRoll = 0.05f;
            banner.epicGuaranteedEvery = 10;

            var dropList = new List<GachaDropItem>();

            // Quét toàn bộ WeaponData trong dự án
            string[] guids = AssetDatabase.FindAssets("t:WeaponData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                if (weapon == null || string.IsNullOrEmpty(weapon.weaponId)) continue;

                // Thiết lập trọng số và mảnh theo phẩm chất
                float weight = 100f;
                int shardAmount = 5;

                switch (weapon.rarity)
                {
                    case ItemRarity.Common:
                        weight = 120f;
                        shardAmount = 5;
                        break;
                    case ItemRarity.Rare:
                        weight = 40f;
                        shardAmount = 8;
                        break;
                    case ItemRarity.Epic:
                        weight = 15f;
                        shardAmount = 12;
                        break;
                    case ItemRarity.Legendary:
                        weight = 3f;
                        shardAmount = 20;
                        break;
                }

                dropList.Add(new GachaDropItem
                {
                    relicId = weapon.weaponId,
                    relicName = weapon.weaponName,
                    rarity = weapon.rarity,
                    element = weapon.elementType,
                    shardAmount = shardAmount,
                    weight = weight,
                    icon = weapon.icon
                });
            }

            banner.SetDropPool(dropList);
            EditorUtility.SetDirty(banner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GachaDataGenerator] Đã tạo thành công Banner '{banner.bannerName}' tại '{assetPath}' với {dropList.Count} Pháp Bảo.");
            Selection.activeObject = banner;

            GenerateHeroGachaBanner();
        }

        [MenuItem("ProjectZombie/Gacha/Generate Hero Gacha Banner SO", priority = 201)]
        public static void GenerateHeroGachaBanner()
        {
            string outputDir = "Assets/Resources/Gacha";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string assetPath = $"{outputDir}/banner_hero.asset";
            var banner = AssetDatabase.LoadAssetAtPath<GachaBannerConfigSO>(assetPath);

            if (banner == null)
            {
                banner = ScriptableObject.CreateInstance<GachaBannerConfigSO>();
                AssetDatabase.CreateAsset(banner, assetPath);
            }

            banner.bannerId = "banner_hero";
            banner.bannerName = "Gương Chiêu Mộ Anh Hùng";
            banner.bannerDescription = "Mở gương thần chiêu mộ Mảnh Thần Tướng Vạn Cổ, quy tụ anh kiệt cứu nhân độ thế.";
            banner.singleRollCost = 200;
            banner.multiRollCost = 1800;
            banner.hardPityLegendary = 50;
            banner.softPityStart = 40;
            banner.softPityRatePerRoll = 0.05f;
            banner.epicGuaranteedEvery = 10;

            var dropList = new List<GachaDropItem>();

            // Quét toàn bộ CharacterDataSO trong dự án
            string[] guids = AssetDatabase.FindAssets("t:CharacterDataSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var hero = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>(path);
                if (hero == null || string.IsNullOrEmpty(hero.characterId)) continue;

                float weight = 100f;
                int shardAmount = 5;

                switch (hero.rarity)
                {
                    case ItemRarity.Common:
                        weight = 120f;
                        shardAmount = 5;
                        break;
                    case ItemRarity.Rare:
                        weight = 50f;
                        shardAmount = 5;
                        break;
                    case ItemRarity.Epic:
                        weight = 20f;
                        shardAmount = 10;
                        break;
                    case ItemRarity.Legendary:
                        weight = 5f;
                        shardAmount = 15;
                        break;
                }

                dropList.Add(new GachaDropItem
                {
                    dropType = GachaDropType.CharacterShard,
                    relicId = hero.characterId,
                    relicName = hero.characterName,
                    rarity = hero.rarity,
                    element = hero.element,
                    shardAmount = shardAmount,
                    weight = weight,
                    icon = hero.avatar
                });
            }

            banner.SetDropPool(dropList);
            EditorUtility.SetDirty(banner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GachaDataGenerator] Đã tạo thành công Banner Anh Hùng '{banner.bannerName}' tại '{assetPath}' với {dropList.Count} Vị Tướng.");
        }
    }
}
#endif

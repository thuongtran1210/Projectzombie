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
        }
    }
}
#endif

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.MetaProgression.Gacha.Data;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.MetaProgression.Gacha.Editor
{
    /// <summary>
    /// Editor Tool đồng bộ GachaBannerConfigSO từ Single Source of Truth (Defense3DCatalog & CharacterDatabaseSO).
    /// </summary>
    public static class GachaDataGenerator
    {
        [MenuItem("ProjectZombie/Gacha/Generate Default Gacha Banner SO", priority = 200)]
        public static void GenerateDefaultGachaBanner()
        {
            var catalog = Defense3DCatalog.CreateDefault();
            var charDb = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");

            var standardPool = BuildStandardDropPool(catalog, charDb);
            SaveBannerAsset("Assets/_Data/Gacha/banner_standard.asset", "banner_standard", "Bảo Rương Vạn Cổ",
                "Mở rương thu thập Mảnh Anh Hùng, Pháp Bảo, Quân Lính & Trụ Phòng Thủ Vọng Xuyên.",
                100, 900, 50, 40, 0.05f, 10, standardPool);
            SaveBannerAsset("Assets/Resources/Gacha/banner_standard.asset", "banner_standard", "Bảo Rương Vạn Cổ",
                "Mở rương thu thập Mảnh Anh Hùng, Pháp Bảo, Quân Lính & Trụ Phòng Thủ Vọng Xuyên.",
                100, 900, 50, 40, 0.05f, 10, standardPool);

            GenerateHeroGachaBanner();
        }

        [MenuItem("ProjectZombie/Gacha/Generate Hero Gacha Banner SO", priority = 201)]
        public static void GenerateHeroGachaBanner()
        {
            var catalog = Defense3DCatalog.CreateDefault();
            var charDb = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");

            var heroPool = BuildHeroDropPool(catalog, charDb);
            SaveBannerAsset("Assets/_Data/Gacha/banner_hero.asset", "banner_hero", "Bảo Rương Anh Hùng",
                "Tăng tỷ lệ xuất hiện Mảnh Anh Hùng & Pháp Bảo Vọng Xuyên.",
                150, 1350, 40, 30, 0.08f, 10, heroPool);
            SaveBannerAsset("Assets/Resources/Gacha/banner_hero.asset", "banner_hero", "Bảo Rương Anh Hùng",
                "Tăng tỷ lệ xuất hiện Mảnh Anh Hùng & Pháp Bảo Vọng Xuyên.",
                150, 1350, 40, 30, 0.08f, 10, heroPool);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<GachaDropItem> BuildStandardDropPool(Defense3DCatalog catalog, CharacterDatabaseSO charDb)
        {
            var pool = new List<GachaDropItem>();
            foreach (var entry in catalog.GetEntriesByType(LoadoutItemType.Hero))
            {
                var heroData = FindCharacterData(charDb, DefenseLoadoutIdMapper.GetMetaCharacterId(entry.Id));
                pool.Add(new GachaDropItem
                {
                    dropType = GachaDropType.CharacterShard,
                    relicId = DefenseLoadoutIdMapper.GetMetaCharacterId(entry.Id),
                    relicName = heroData != null ? heroData.characterName : entry.DisplayName,
                    rarity = heroData != null ? heroData.rarity : ItemRarity.Epic,
                    element = heroData != null ? heroData.element : ElementType.Kim,
                    shardAmount = 5,
                    weight = (heroData != null && heroData.rarity == ItemRarity.Legendary) ? 10f : 25f,
                    icon = heroData != null ? heroData.avatar : null
                });
            }

            LoadoutItemType[] unitTypes = { LoadoutItemType.Relic, LoadoutItemType.Minion, LoadoutItemType.Tower };
            for (int t = 0; t < unitTypes.Length; t++)
            {
                foreach (var entry in catalog.GetEntriesByType(unitTypes[t]))
                {
                    GetUnitRarityAndElement(entry, out var rarity, out var element, out int shards, out float weight);
                    pool.Add(new GachaDropItem
                    {
                        dropType = GachaDropType.RelicShard,
                        relicId = entry.Id,
                        relicName = entry.DisplayName,
                        rarity = rarity,
                        element = element,
                        shardAmount = shards,
                        weight = weight,
                        icon = null
                    });
                }
            }
            return pool;
        }

        private static List<GachaDropItem> BuildHeroDropPool(Defense3DCatalog catalog, CharacterDatabaseSO charDb)
        {
            var pool = new List<GachaDropItem>();
            foreach (var entry in catalog.GetEntriesByType(LoadoutItemType.Hero))
            {
                var heroData = FindCharacterData(charDb, DefenseLoadoutIdMapper.GetMetaCharacterId(entry.Id));
                bool isLeg = heroData != null && heroData.rarity == ItemRarity.Legendary;
                pool.Add(new GachaDropItem
                {
                    dropType = GachaDropType.CharacterShard,
                    relicId = DefenseLoadoutIdMapper.GetMetaCharacterId(entry.Id),
                    relicName = heroData != null ? heroData.characterName : entry.DisplayName,
                    rarity = heroData != null ? heroData.rarity : ItemRarity.Epic,
                    element = heroData != null ? heroData.element : ElementType.Kim,
                    shardAmount = isLeg ? 15 : 10,
                    weight = isLeg ? 18f : 40f,
                    icon = heroData != null ? heroData.avatar : null
                });
            }

            foreach (var entry in catalog.GetEntriesByType(LoadoutItemType.Relic))
            {
                pool.Add(new GachaDropItem
                {
                    dropType = GachaDropType.RelicShard,
                    relicId = entry.Id,
                    relicName = entry.DisplayName,
                    rarity = ItemRarity.Epic,
                    element = ElementType.Tho,
                    shardAmount = 8,
                    weight = 25f,
                    icon = null
                });
            }
            return pool;
        }

        private static CharacterDataSO FindCharacterData(CharacterDatabaseSO db, string characterId)
        {
            if (db == null || db.Characters == null) return null;
            for (int i = 0; i < db.Characters.Count; i++)
            {
                var c = db.Characters[i];
                if (c != null && string.Equals(c.characterId, characterId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return c;
                }
            }
            return null;
        }

        private static void GetUnitRarityAndElement(LoadoutCatalogEntry entry, out ItemRarity rarity, out ElementType element, out int shards, out float weight)
        {
            if (string.Equals(entry.Id, Defense3DCatalog.MINION_DAN_BINH, System.StringComparison.OrdinalIgnoreCase))
            {
                rarity = ItemRarity.Common; element = ElementType.Tho; shards = 8; weight = 75f;
            }
            else if (string.Equals(entry.Id, Defense3DCatalog.MINION_PHU_TAP_DICH, System.StringComparison.OrdinalIgnoreCase))
            {
                rarity = ItemRarity.Rare; element = ElementType.Kim; shards = 6; weight = 50f;
            }
            else if (string.Equals(entry.Id, Defense3DCatalog.TOWER_CUNG_TIEN, System.StringComparison.OrdinalIgnoreCase))
            {
                rarity = ItemRarity.Rare; element = ElementType.Moc; shards = 6; weight = 50f;
            }
            else if (string.Equals(entry.Id, Defense3DCatalog.TOWER_BANG_HA, System.StringComparison.OrdinalIgnoreCase))
            {
                rarity = ItemRarity.Epic; element = ElementType.Thuy; shards = 5; weight = 30f;
            }
            else
            {
                rarity = ItemRarity.Epic; element = ElementType.Moc; shards = 5; weight = 30f;
            }
        }

        private static void SaveBannerAsset(
            string assetPath,
            string bannerId,
            string bannerName,
            string description,
            int singleCost,
            int multiCost,
            int hardPity,
            int softPity,
            float softRate,
            int epicEvery,
            List<GachaDropItem> pool)
        {
            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var banner = AssetDatabase.LoadAssetAtPath<GachaBannerConfigSO>(assetPath);
            if (banner == null)
            {
                banner = ScriptableObject.CreateInstance<GachaBannerConfigSO>();
                AssetDatabase.CreateAsset(banner, assetPath);
            }

            banner.bannerId = bannerId;
            banner.bannerName = bannerName;
            banner.bannerDescription = description;
            banner.singleRollCost = singleCost;
            banner.multiRollCost = multiCost;
            banner.hardPityLegendary = hardPity;
            banner.softPityStart = softPity;
            banner.softPityRatePerRoll = softRate;
            banner.epicGuaranteedEvery = epicEvery;
            banner.SetDropPool(new List<GachaDropItem>(pool));

            EditorUtility.SetDirty(banner);
        }
    }
}
#endif

using System;
using System.Collections.Generic;

namespace ProjectZombie.EditorTools.BuildSync
{
    public static class SyncRegistry
    {
        public static readonly List<SyncRule> DirectoryRules = new List<SyncRule>
        {
            new SyncRule("Thẻ Nâng Cấp (Upgrades)", "Assets/_Data/Upgrades", "Assets/Resources/Upgrades", "*.asset"),
            new SyncRule("Âm thanh (Audios)", "Assets/_Data/Audios", "Assets/Resources/Audios", "*.*", new[] { ".wav", ".mp3", ".ogg", ".asset", ".mixer" }),
            new SyncRule("Pháp Bảo (Weapons)", "Assets/_Data/Weapons", "Assets/Resources/Weapons", "*.asset"),
            new SyncRule("Quái vật (Enemies)", "Assets/_Prefabs/Characters/Enemies", "Assets/Resources/Enemies", "*.prefab"),
            new SyncRule("Tướng (Players)", "Assets/_Prefabs/Characters/Players", "Assets/Resources/Players", "*.prefab"),
            new SyncRule("Timeline Màn Chơi (Levels)", "Assets/_Data/Levels", "Assets/Resources/Levels", "*.asset"),
            new SyncRule("Giao Diện (UI Prefabs)", "Assets/_Prefabs/UI", "Assets/Resources/UI", "*.prefab"),
            new SyncRule("UI HUD Sprites (Pins, Badges)", "Assets/Art/UI/HUD", "Assets/Resources/UI/HUD", "*.png"),
            new SyncRule("UI Vọng Xuyên Theme Sprites", "Assets/Art/UI/VongXuyen", "Assets/Resources/UI/VongXuyen", "*.png"),
            new SyncRule("UI Gacha Sprites & Art", "Assets/Art/UI/Gacha", "Assets/Resources/UI/Gacha", "*.*", new[] { ".png", ".jpg", ".asset" }),
            new SyncRule("Gacha Banner Configs", "Assets/_Data/Gacha", "Assets/Resources/Gacha", "*.asset"),
            new SyncRule("Bản Đồ Màn Chơi (Map Prefabs)", "Assets/_Prefabs/Maps", "Assets/Resources/Maps", "*.prefab"),
            new SyncRule("Tilemap Assets (Tiles)", "Assets/Art/Tilemaps/Tiles", "Assets/Resources/Tiles", "*.asset")
        };

        public static readonly List<SyncRule> SingleAssetRules = new List<SyncRule>
        {
            SyncRule.ForSingleAsset("CharacterDatabase (Dữ liệu Tướng)", "Assets/_Data/CharacterDatabase.asset", "Assets/Resources/CharacterDatabase.asset", () => ProjectZombie.Editor.CharacterDataAssetGenerator.GenerateCharacterAssets()),
            SyncRule.ForSingleAsset("CharacterStarProgressionConfig (Cấu Hình Nâng Sao Tướng)", "Assets/_Data/CharacterStarProgressionConfig.asset", "Assets/Resources/CharacterStarProgressionConfig.asset", () => ProjectZombie.Editor.CharacterDataAssetGenerator.GenerateCharacterStarProgressionConfig()),
            SyncRule.ForSingleAsset("WorldStageDatabase (Danh Sách Màn Chơi / Ải)", "Assets/_Data/Levels/WorldStageDatabase.asset", "Assets/Resources/WorldStageDatabase.asset", () => ProjectZombie.Editor.Maps.StageDataGeneratorTool.GenerateDefaultStages()),
            SyncRule.ForSingleAsset("PermanentUpgradeTree (Cây Nâng Cấp Vĩnh Viễn)", "Assets/_Data/Meta/PermanentUpgradeTree.asset", "Assets/Resources/PermanentUpgradeTree.asset"),
            SyncRule.ForSingleAsset("GachaBanner (Banner Gacha Chuẩn)", "Assets/_Data/Gacha/banner_standard.asset", "Assets/Resources/Gacha/banner_standard.asset", () => ProjectZombie.Features.MetaProgression.Gacha.Editor.GachaDataGenerator.GenerateDefaultGachaBanner())
        };

        public static readonly List<SyncRule> UIPrefabRules = new List<SyncRule>
        {
            SyncRule.ForUIPrefab("MainHubUI", () => ProjectZombie.Editor.UI.MainHubUIGenerator.GenerateMainHubPrefab()),
            SyncRule.ForUIPrefab("CharacterSelectionUI", () => ProjectZombie.Editor.UI.CharacterSelectionUIGenerator.GenerateCharacterSelectionPrefab()),
            SyncRule.ForUIPrefab("SanctuaryTreeUI", () => ProjectZombie.Editor.UI.SanctuaryTreeUIGenerator.GenerateSanctuaryTreePrefab()),
            SyncRule.ForUIPrefab("SettingsModalUI", () => ProjectZombie.Editor.UI.SettingsUIGenerator.GenerateSettingsModal()),
            SyncRule.ForUIPrefab("PlayerStatsMenuUI", () => ProjectZombie.Editor.UI.PlayerStatsMenuUIGenerator.RebuildPlayerStatsMenuUI()),
            SyncRule.ForUIPrefab("MobileControlsCustomizerUI", () => ProjectZombie.Editor.UI.MobileControlsCustomizerUIGenerator.GenerateCustomizerUI()),
            SyncRule.ForUIPrefab("WeaponLoadoutUI", () => ProjectZombie.Editor.UI.WeaponLoadoutUIGenerator.GenerateWeaponLoadoutPrefab()),
            SyncRule.ForUIPrefab("CardCodexUI", () => ProjectZombie.Editor.UI.CardCodexUIGenerator.GenerateCardCodexPrefab()),
            SyncRule.ForUIPrefab("StageSelect_Screen", () => ProjectZombie.Editor.UI.StageSelectUIGenerator.GenerateStageSelectUI()),
            SyncRule.ForUIPrefab("LoadingScreenUI", () => ProjectZombie.Editor.UI.LoadingScreenSetupTool.CreateOrUpdateLoadingPrefab()),
            SyncRule.ForUIPrefab("ResourceDownloadModalUI", () => ProjectZombie.Editor.UI.ResourceDownloadUIGenerator.GenerateResourceDownloadPrefab()),
            new SyncRule("GachaShopPanel", "Assets/_Prefabs/UI/Gacha/GachaShopPanel.prefab", "Assets/Resources/UI/Gacha/GachaShopPanel.prefab", "*.prefab")
            {
                Type = RuleType.UIPrefab,
                FallbackGenerator = () => ProjectZombie.Features.MetaProgression.Gacha.Editor.GachaUIPrefabBuilder.BuildGachaUIPrefabs()
            }
        };
    }
}

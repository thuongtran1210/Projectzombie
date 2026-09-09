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
            new SyncRule("Gacha Banner Configs", "Assets/_Data/Gacha", "Assets/Resources/Gacha", "*.asset")
        };

        public static readonly List<SyncRule> SingleAssetRules = new List<SyncRule>
        {
            SyncRule.ForSingleAsset("CharacterDatabase (Dữ liệu Tướng)", "Assets/_Data/CharacterDatabase.asset", "Assets/Resources/CharacterDatabase.asset"),
            SyncRule.ForSingleAsset("PermanentUpgradeTree (Cây Nâng Cấp Vĩnh Viễn)", "Assets/_Data/Meta/PermanentUpgradeTree.asset", "Assets/Resources/PermanentUpgradeTree.asset"),
            SyncRule.ForSingleAsset("GachaBanner (Banner Gacha Chuẩn)", "Assets/Resources/Gacha/banner_standard.asset", "Assets/_Data/Gacha/banner_standard.asset")
        };

        public static readonly List<SyncRule> UIPrefabRules = new List<SyncRule>
        {
            SyncRule.ForUIPrefab("SettingsModalUI", () => ProjectZombie.Editor.UI.SettingsUIGenerator.GenerateSettingsModal()),
            SyncRule.ForUIPrefab("PlayerStatsMenuUI", () => ProjectZombie.Editor.UI.PlayerStatsMenuUIGenerator.RebuildPlayerStatsMenuUI()),
            SyncRule.ForUIPrefab("MobileControlsCustomizerUI", () => ProjectZombie.Editor.UI.MobileControlsCustomizerUIGenerator.GenerateCustomizerUI()),
            SyncRule.ForUIPrefab("WeaponLoadoutUI", () => ProjectZombie.Editor.UI.WeaponLoadoutUIGenerator.GenerateWeaponLoadoutPrefab()),
            SyncRule.ForUIPrefab("CardCodexUI", () => ProjectZombie.Editor.UI.CardCodexUIGenerator.GenerateCardCodexPrefab()),
            new SyncRule("GachaShopPanel", "Assets/_Prefabs/UI/Gacha/GachaShopPanel.prefab", "Assets/Resources/UI/Gacha/GachaShopPanel.prefab", "*.prefab")
            {
                Type = RuleType.UIPrefab,
                FallbackGenerator = () => ProjectZombie.Features.MetaProgression.Gacha.Editor.GachaUIPrefabBuilder.BuildGachaUIPrefabs()
            }
        };
    }
}

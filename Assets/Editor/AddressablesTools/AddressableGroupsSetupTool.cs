#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace ProjectZombie.Editor.AddressablesTools
{
    /// <summary>
    /// Tool tự động thiết lập và phân chia 6 nhóm Addressables chuẩn theo tài liệu DOCS_ADDRESSABLES_CDN_FIREBASE.md.
    /// </summary>
    public static class AddressableGroupsSetupTool
    {
        [MenuItem("Tools/ProjectZombie/Addressables/Setup Standard CDN & Local Groups", priority = 100)]
        public static void SetupStandardGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressableGroupsSetupTool] Không tìm thấy AddressableAssetSettings. Hãy mở Window > Asset Management > Addressables > Groups trước.");
                return;
            }

            // 1. Nhóm Local (Nằm trong APK)
            var groupCore = CreateOrConfigureGroup(settings, "Group_Core_Preload", false, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
            var groupEnemiesStage1 = CreateOrConfigureGroup(settings, "Group_Enemies_Stage1", false, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
            var groupWeapons = CreateOrConfigureGroup(settings, "Group_Weapons_Tier1", false, BundledAssetGroupSchema.BundlePackingMode.PackTogether);

            // 2. Nhóm Remote (DLC Firebase Storage CDN)
            var groupStages = CreateOrConfigureGroup(settings, "Group_DLC_Stages_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackSeparately);
            var groupEnemiesRemote = CreateOrConfigureGroup(settings, "Group_DLC_Enemies_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
            var groupAudioRemote = CreateOrConfigureGroup(settings, "Group_DLC_Audio_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackSeparately);
            var groupUpgradesRemote = CreateOrConfigureGroup(settings, "Group_DLC_Upgrades_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
            var groupMetaConfigsRemote = CreateOrConfigureGroup(settings, "Group_DLC_MetaConfigs_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackTogether);

            // 3. Tự động thêm các Prefab Vũ Khí, Quái Màn 1, Thẻ Nâng Cấp & Cấu Hình Meta vào Groups
            AutoPopulateDefaultEntries(settings, groupCore, groupEnemiesStage1, groupWeapons, groupStages, groupUpgradesRemote, groupMetaConfigsRemote);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF88>[AddressableGroupsSetupTool]</color> Đã thiết lập thành công 8 Nhóm Addressables (3 Local + 5 Remote DLC CDN)!");
        }

        [MenuItem("Tools/ProjectZombie/Addressables/🔨 Build Addressables Content Bundles", priority = 101)]
        public static void BuildAddressablesBundles()
        {
            SetupStandardGroups();
            Debug.Log("<color=#FFAA00>[AddressableGroupsSetupTool]</color> Bắt đầu đóng gói Addressables Content Bundles...");
            AddressableAssetSettings.BuildPlayerContent(out UnityEditor.AddressableAssets.Build.AddressablesPlayerBuildResult result);
            if (string.IsNullOrEmpty(result.Error))
            {
                Debug.Log($"<color=#00FF88>[AddressableGroupsSetupTool] BUILD BUNDLE THÀNH CÔNG!</color> File đã xuất ra thư mục ServerData/Android/");
                EditorUtility.DisplayDialog("Addressables Build Thành Công", $"Đã đóng gói hoàn tất các AssetBundle và Catalog!\n\nBạn có thể vào thư mục ServerData/Android/ và kéo thả lên Firebase Storage CDN.", "OK");
            }
            else
            {
                Debug.LogError($"[AddressableGroupsSetupTool] Build Bundle thất bại: {result.Error}");
                EditorUtility.DisplayDialog("Lỗi Build Addressables", $"Build thất bại: {result.Error}", "Đóng");
            }
        }

        private static void AutoPopulateDefaultEntries(
            AddressableAssetSettings settings,
            AddressableAssetGroup groupCore,
            AddressableAssetGroup groupEnemiesStage1,
            AddressableAssetGroup groupWeapons,
            AddressableAssetGroup groupStages,
            AddressableAssetGroup groupUpgradesRemote,
            AddressableAssetGroup groupMetaConfigsRemote)
        {
            // 1. Core Database & World Stages (Hỗ trợ LiveOps Hot Update)
            AddAssetToGroup(settings, groupMetaConfigsRemote, "Assets/_Data/Levels/WorldStageDatabase.asset", "WorldStageDatabase", "Map", "RemoteDLC");
            AddAssetToGroup(settings, groupCore, "Assets/_Data/CharacterDatabase.asset", "CharacterDatabase");

            // 1.1 Đăng ký từng StageDefinitionSO vào Remote DLC
            string[] stageGuids = AssetDatabase.FindAssets("t:StageDefinitionSO", new[] { "Assets/_Data/Levels/Stages" });
            foreach (string guid in stageGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                AddAssetToGroup(settings, groupMetaConfigsRemote, path, fileName, "Map", "RemoteDLC");
            }

            // 1.2 Đăng ký Prefab Màn vào Group_DLC_Stages_Remote
            AddAssetToGroup(settings, groupStages, "Assets/_Prefabs/Maps/Map_BambooForest.prefab", "Map_BambooForest", "Map", "RemoteDLC");
            AddAssetToGroup(settings, groupStages, "Assets/_Prefabs/Maps/Map_AncientCitadel.prefab", "Map_AncientCitadel", "Map", "RemoteDLC");
            AddAssetToGroup(settings, groupStages, "Assets/_Prefabs/Maps/Map_CinnabarSwamp.prefab", "Map_CinnabarSwamp", "Map", "RemoteDLC");

            // 2. Toàn bộ 12 Pháp Bảo & Vũ Khí Khởi Đầu (Weapons & Relics - Local)
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W_POT.prefab", "Weapon_W_POT");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W_SLIPPER.prefab", "Weapon_W_SLIPPER");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W_PIPE.prefab", "Weapon_W_PIPE");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W001_NoThan.prefab", "Weapon_W001_NoThan");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W002_ButPhanQuan.prefab", "Weapon_W002_ButPhanQuan");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W003_BuaTranYeu.prefab", "Weapon_W003_BuaTranYeu");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W004_CuuViHoTrao.prefab", "Weapon_W004_CuuViHoTrao");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W005_TrongDongDongSon.prefab", "Weapon_W005_TrongDongDongSon");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W006_LuuDanThanSa.prefab", "Weapon_W006_LuuDanThanSa");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W007_CungThachSanh.prefab", "Weapon_W007_CungThachSanh");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W008_DaoCuuVi.prefab", "Weapon_W008_DaoCuuVi");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W009_TruongLongVuong.prefab", "Weapon_W009_TruongLongVuong");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W010_LinhPhuMaDa.prefab", "Weapon_W010_LinhPhuMaDa");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W011_NuocThanhChuaHuong.prefab", "Weapon_W011_NuocThanhChuaHuong");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Weapons/Weapon_W012_PhiTieuBatQuai.prefab", "Weapon_W012_PhiTieuBatQuai");

            // 3. Toàn bộ Projectiles tương ứng
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W001_NoThan.prefab", "Proj_W001_NoThan");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W002_ButPhanQuan.prefab", "Proj_W002_ButPhanQuan");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W003_BuaTranYeu.prefab", "Proj_W003_BuaTranYeu");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W004_CuuViHoTrao.prefab", "Proj_W004_CuuViHoTrao");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W005_TrongDongDongSon.prefab", "Proj_W005_TrongDongDongSon");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W006_LuuDanThanSa.prefab", "Proj_W006_LuuDanThanSa");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W007_CungThachSanh.prefab", "Proj_W007_CungThachSanh");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W008_DaoCuuVi.prefab", "Proj_W008_DaoCuuVi");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W009_TruongLongVuong.prefab", "Proj_W009_TruongLongVuong");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W010_LinhPhuMaDa.prefab", "Proj_W010_LinhPhuMaDa");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W011_NuocThanhChuaHuong.prefab", "Proj_W011_NuocThanhChuaHuong");
            AddAssetToGroup(settings, groupWeapons, "Assets/_Prefabs/Projectiles/Proj_W012_PhiTieuBatQuai.prefab", "Proj_W012_PhiTieuBatQuai");

            // 4. Gán Quái Màn 1
            AddAssetToGroup(settings, groupEnemiesStage1, "Assets/_Prefabs/Characters/Enemies/Zombie_Basic.prefab", "Zombie_Basic");

            // 5. Toàn bộ Thẻ Nâng Cấp Upgrades (Remote DLC Update)
            string[] upgradeGuids = AssetDatabase.FindAssets("t:UpgradeData", new[] { "Assets/_Data/Upgrades" });
            foreach (string guid in upgradeGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                AddAssetToGroup(settings, groupUpgradesRemote, path, fileName, "UpgradeData", "RemoteDLC");
            }

            // 6. Cấu hình Meta & LiveOps (Remote DLC Meta Configs)
            AddAssetToGroup(settings, groupMetaConfigsRemote, "Assets/_Data/Gacha/banner_standard.asset", "banner_standard", "MetaConfigs", "RemoteDLC");
            AddAssetToGroup(settings, groupMetaConfigsRemote, "Assets/_Data/Meta/PermanentUpgradeTree.asset", "PermanentUpgradeTree", "MetaConfigs", "RemoteDLC");
            AddAssetToGroup(settings, groupMetaConfigsRemote, "Assets/_Data/CharacterStarProgressionConfig.asset", "CharacterStarProgressionConfig", "MetaConfigs", "RemoteDLC");
        }

        private static void AddAssetToGroup(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath, string address, params string[] labels)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!string.IsNullOrEmpty(guid))
            {
                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                if (entry != null)
                {
                    if (!string.IsNullOrEmpty(address)) entry.address = address;
                    if (labels != null)
                    {
                        foreach (var label in labels)
                        {
                            if (!string.IsNullOrEmpty(label))
                            {
                                settings.AddLabel(label);
                                entry.SetLabel(label, true, true);
                            }
                        }
                    }
                }
            }
        }

        private static AddressableAssetGroup CreateOrConfigureGroup(
            AddressableAssetSettings settings, 
            string groupName, 
            bool isRemote, 
            BundledAssetGroupSchema.BundlePackingMode packingMode)
        {
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema != null)
            {
                schema.BundleMode = packingMode;
                schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;

                if (isRemote)
                {
                    schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
                    schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
                }
                else
                {
                    schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
                    schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
                }
                EditorUtility.SetDirty(group);
            }

            return group;
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
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
            CreateOrConfigureGroup(settings, "Group_Core_Preload", false, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
            CreateOrConfigureGroup(settings, "Group_Enemies_Stage1", false, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
            CreateOrConfigureGroup(settings, "Group_Weapons_Tier1", false, BundledAssetGroupSchema.BundlePackingMode.PackTogether);

            // 2. Nhóm Remote (DLC Firebase Storage CDN)
            CreateOrConfigureGroup(settings, "Group_DLC_Stages_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackSeparately);
            CreateOrConfigureGroup(settings, "Group_DLC_Enemies_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
            CreateOrConfigureGroup(settings, "Group_DLC_Audio_Remote", true, BundledAssetGroupSchema.BundlePackingMode.PackSeparately);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF88>[AddressableGroupsSetupTool]</color> Đã thiết lập thành công 6 Nhóm Addressables (3 Local + 3 Remote DLC CDN)!");
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

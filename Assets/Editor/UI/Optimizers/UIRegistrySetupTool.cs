#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Editor.UITools
{
    /// <summary>
    /// Công cụ Editor tự động tạo và liên kết các Prefab UI vào UIRegistrySO.
    /// Giúp Designer và Dev không phải kéo thả thủ công từng Prefab.
    /// </summary>
    [InitializeOnLoad]
    public static class UIRegistrySetupTool
    {
        private const string REGISTRY_PATH = "Assets/Resources/UI/UIRegistry.asset";
        private const string REGISTRY_DIR = "Assets/Resources/UI";

        static UIRegistrySetupTool()
        {
            EditorApplication.delayCall += () =>
            {
                CreateOrUpdateUIRegistry();
            };
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/4. Tối Ưu UI/8. Setup UIRegistry Asset", priority = 169)]
        public static void CreateOrUpdateUIRegistry()

        {
            if (!Directory.Exists(REGISTRY_DIR))
            {
                Directory.CreateDirectory(REGISTRY_DIR);
            }


            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(REGISTRY_PATH);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<UIRegistrySO>();
                AssetDatabase.CreateAsset(registry, REGISTRY_PATH);
            }

            // Danh sách ánh xạ các Prefab chuẩn trong dự án
            var mappings = new (MetaScreenType type, string hierarchyName, string[] searchPaths)[]
            {
                (MetaScreenType.MainHub, "Panel_MainHub", new[] { "Assets/Prefabs/UI/MainHubUI.prefab", "Assets/Resources/UI/MainHubUI.prefab", "Assets/_Prefabs/UI/MainHubUI.prefab" }),
                (MetaScreenType.CharacterSelect, "Panel_CharacterSelect", new[] { "Assets/Prefabs/UI/CharacterSelectionUI.prefab", "Assets/Resources/UI/CharacterSelectionUI.prefab", "Assets/_Prefabs/UI/CharacterSelectionUI.prefab" }),
                (MetaScreenType.WeaponLoadout, "Panel_WeaponLoadout", new[] { "Assets/Prefabs/UI/WeaponLoadoutUI.prefab", "Assets/Resources/UI/WeaponLoadoutUI.prefab", "Assets/_Prefabs/UI/WeaponLoadoutUI.prefab" }),
                (MetaScreenType.SanctuaryTree, "Panel_SanctuaryTree", new[] { "Assets/Prefabs/UI/SanctuaryTreeUI.prefab", "Assets/Resources/UI/SanctuaryTreeUI.prefab", "Assets/_Prefabs/UI/SanctuaryTreeUI.prefab" }),
                (MetaScreenType.Codex, "Panel_CardCodex", new[] { "Assets/Prefabs/UI/CardCodexUI.prefab", "Assets/Resources/UI/CardCodexUI.prefab", "Assets/_Prefabs/UI/CardCodexUI.prefab" }),
                (MetaScreenType.Settings, "Modal_Settings", new[] { "Assets/Prefabs/UI/SettingsModalUI.prefab", "Assets/Resources/UI/SettingsModalUI.prefab", "Assets/_Prefabs/UI/SettingsModalUI.prefab" }),
                (MetaScreenType.GachaShop, "Panel_GachaShop", new[] { "Assets/Prefabs/UI/Gacha/GachaShopPanel.prefab", "Assets/Resources/UI/Gacha/GachaShopPanel.prefab", "Assets/_Prefabs/UI/GachaShopPanel.prefab" }),
                (MetaScreenType.StageSelect, "Screen_StageSelect", new[] { "Assets/_Prefabs/UI/StageSelect_Screen.prefab", "Assets/Prefabs/UI/StageSelect_Screen.prefab", "Assets/Resources/UI/StageSelect_Screen.prefab" }),
                (MetaScreenType.ResourceDownload, "Modal_ResourceDownload", new[] { "Assets/_Prefabs/UI/ResourceDownloadModalUI.prefab", "Assets/Prefabs/UI/ResourceDownloadModalUI.prefab", "Assets/Resources/UI/ResourceDownloadModalUI.prefab" }),
                (MetaScreenType.Lobby, "Modal_Lobby", new[] { "Assets/Prefabs/UI/LobbyModalUI.prefab", "Assets/Resources/UI/LobbyModalUI.prefab", "Assets/_Prefabs/UI/LobbyModalUI.prefab" })
            };

            var serializedObj = new SerializedObject(registry);
            var screensProp = serializedObj.FindProperty("_screens");
            screensProp.ClearArray();

            int addedCount = 0;
            for (int i = 0; i < mappings.Length; i++)
            {
                var map = mappings[i];
                BaseMetaScreenView foundPrefab = null;

                foreach (var path in map.searchPaths)
                {
                    var prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefabGo != null)
                    {
                        foundPrefab = prefabGo.GetComponent<BaseMetaScreenView>();
                        if (foundPrefab != null) break;
                    }
                }

                if (foundPrefab == null)
                {
                    // Tìm kiếm toàn bộ Assets nếu đường dẫn chuẩn chưa đúng
                    string[] guids = AssetDatabase.FindAssets($"t:Prefab {map.type}");
                    foreach (var g in guids)
                    {
                        var p = AssetDatabase.GUIDToAssetPath(g);
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                        if (go != null && go.TryGetComponent<BaseMetaScreenView>(out var view) && view.ScreenType == map.type)
                        {
                            foundPrefab = view;
                            break;
                        }
                    }
                }

                screensProp.InsertArrayElementAtIndex(i);
                var element = screensProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("screenType").enumValueIndex = (int)map.type;
                element.FindPropertyRelative("screenHierarchyName").stringValue = map.hierarchyName;
                element.FindPropertyRelative("screenPrefab").objectReferenceValue = foundPrefab;

                if (foundPrefab != null) addedCount++;
            }

            serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=#00FF88>[UIRegistrySetupTool]</color> Đã tạo và cấu hình thành công '{REGISTRY_PATH}' với {addedCount}/{mappings.Length} màn hình!");
            Selection.activeObject = registry;
        }
    }
}
#endif

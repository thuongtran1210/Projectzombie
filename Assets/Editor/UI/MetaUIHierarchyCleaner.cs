#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.StageSelect;
using ProjectZombie.Features.UI.ResourceDownload;
using ProjectZombie.Features.UI.Gacha;

namespace ProjectZombie.Editor.UI
{
    public static class MetaUIHierarchyCleaner
    {
        [MenuItem("Tools/ProjectZombie/UI/Sảnh Chính (Meta Menu)/0. Dọn Dẹp & Đồng Bộ Canvas_MetaMenu (Clean & Fix All)", priority = 0)]
        public static void CleanAndSyncMetaMenu()
        {
            var metaCanvasObj = GameObject.Find("Canvas_MetaMenu");
            if (metaCanvasObj == null)
            {
                var allCanvas = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (var c in allCanvas)
                {
                    if (c.gameObject.name == "Canvas_MetaMenu" && c.gameObject.scene.IsValid())
                    {
                        metaCanvasObj = c.gameObject;
                        break;
                    }
                }
            }

            if (metaCanvasObj == null)
            {
                Debug.LogError("[MetaUIHierarchyCleaner] Không tìm thấy 'Canvas_MetaMenu' trong Scene!");
                return;
            }

            // 1. Tái tạo WeaponLoadoutUI.prefab với layout 2 cột hoàn hảo
            WeaponLoadoutUIGenerator.GenerateWeaponLoadoutPrefab();

            var metaMgr = metaCanvasObj.GetComponent<MetaUIManager>();
            if (metaMgr == null) metaMgr = metaCanvasObj.AddComponent<MetaUIManager>();

            Transform metaTransform = metaCanvasObj.transform;

            // 2. Thu thập và khử trùng lặp các GameObject con
            CleanDuplicates<MainHubView>(metaTransform, "Panel_MainHub");
            CleanDuplicates<CharacterSelectionView>(metaTransform, "Panel_CharacterSelect");
            CleanDuplicates<WeaponLoadoutView>(metaTransform, "Panel_WeaponLoadout");
            CleanDuplicates<MetaUpgradeShopView>(metaTransform, "Panel_SanctuaryTree");
            CleanDuplicates<CardCodexView>(metaTransform, "Panel_CardCodex");
            CleanDuplicates<SettingsModalView>(metaTransform, "Modal_Settings");
            CleanDuplicates<GachaChestView>(metaTransform, "Panel_GachaShop");
            CleanDuplicates<StageSelectUIView>(metaTransform, "Screen_StageSelect");
            CleanDuplicates<ResourceDownloadModalView>(metaTransform, "Modal_ResourceDownload");

            // 3. Tự động tìm reference chính xác
            var mainHub = metaTransform.GetComponentInChildren<MainHubView>(true);
            var charSelect = metaTransform.GetComponentInChildren<CharacterSelectionView>(true);
            var weaponLoadout = metaTransform.GetComponentInChildren<WeaponLoadoutView>(true);
            var sanctuary = metaTransform.GetComponentInChildren<MetaUpgradeShopView>(true);
            var codex = metaTransform.GetComponentInChildren<CardCodexView>(true);
            var settings = metaTransform.GetComponentInChildren<SettingsModalView>(true);
            var gacha = metaTransform.GetComponentInChildren<GachaChestView>(true);
            var stageSelect = metaTransform.GetComponentInChildren<StageSelectUIView>(true);
            var resourceDl = metaTransform.GetComponentInChildren<ResourceDownloadModalView>(true);

            // 4. Thiết lập trạng thái Active chuẩn:
            // Chỉ duy nhất Backdrop và Panel_MainHub được bật; tất cả sub-screen khác BẮT BUỘC TẮT (Active = false)
            for (int i = 0; i < metaTransform.childCount; i++)
            {
                var child = metaTransform.GetChild(i);
                if (child.name == "Persistent_MetaBackdrop" || child.name == "Panel_MainHub")
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }

            // 5. Wire SerializedObject cho MetaUIManager
            SerializedObject soMeta = new SerializedObject(metaMgr);
            soMeta.FindProperty("_metaCanvasGroup").objectReferenceValue = metaCanvasObj.GetComponent<CanvasGroup>();
            soMeta.FindProperty("_mainHubScreen").objectReferenceValue = mainHub;
            soMeta.FindProperty("_characterSelectScreen").objectReferenceValue = charSelect;
            soMeta.FindProperty("_weaponLoadoutScreen").objectReferenceValue = weaponLoadout;
            soMeta.FindProperty("_sanctuaryTreeScreen").objectReferenceValue = sanctuary;
            soMeta.FindProperty("_codexScreen").objectReferenceValue = codex;
            soMeta.FindProperty("_settingsScreen").objectReferenceValue = settings;
            soMeta.FindProperty("_gachaShopScreen").objectReferenceValue = gacha;
            soMeta.FindProperty("_stageSelectScreen").objectReferenceValue = stageSelect;
            soMeta.FindProperty("_resourceDownloadScreen").objectReferenceValue = resourceDl;

            var backdrop = metaTransform.Find("Persistent_MetaBackdrop");
            if (backdrop != null)
            {
                soMeta.FindProperty("_persistentBackdrop").objectReferenceValue = backdrop.gameObject;
            }

            soMeta.ApplyModifiedProperties();
            EditorUtility.SetDirty(metaMgr);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#00FF88>[MetaUIHierarchyCleaner] ĐÃ DỌN DẸP, KHỬ TRÙNG LẶP VÀ ĐỒNG BỘ TOÀN BỘ CANVAS_METAMENU THÀNH CÔNG!</color>");
        }

        private static void CleanDuplicates<T>(Transform parent, string preferredName) where T : Component
        {
            var components = Object.FindObjectsOfType<T>(true);
            if (components.Length > 1)
            {
                Debug.Log($"[MetaUIHierarchyCleaner] Phát hiện {components.Length} đối tượng {typeof(T).Name}. Giữ lại 1 bản và xóa các bản thừa.");
                for (int i = 1; i < components.Length; i++)
                {
                    if (components[i] != null && components[i].gameObject != null)
                    {
                        Object.DestroyImmediate(components[i].gameObject);
                    }
                }
            }

            if (components.Length > 0 && components[0] != null)
            {
                var go = components[0].gameObject;
                if (go.transform.parent != parent)
                {
                    go.transform.SetParent(parent, false);
                }
                go.name = preferredName;
            }
        }
    }
}
#endif

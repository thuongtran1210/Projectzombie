using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.Save;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.MetaProgression.Gacha;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Tool tự động tạo lập và cấu hình hoàn chỉnh Scene MainMenu.unity:
    /// - Main Camera & AudioListener
    /// - EventSystem kèm InputSystemUIInputModule
    /// - Root Managers (GameStateManager, GameManager, Currency, Gacha, TransitionController)
    /// - Canvas_Master chuẩn 1920x1080 Mobile
    /// - Canvas_MetaMenu với đầy đủ MainHubUI và các Modal con (CharacterSelection, WeaponLoadout, Settings, Gacha, v.v.)
    /// - Đồng bộ EditorBuildSettings (Scene 0: MainMenu, Scene 1: MapSongQue3D_Prototype)
    /// </summary>
    [InitializeOnLoad]
    public static class MainMenuSceneSetupTool
    {
        private const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";
        private const string DEFENSE_SCENE_PATH = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";

        static MainMenuSceneSetupTool()
        {
            EditorApplication.delayCall += CheckAndAutoGenerate;
        }

        private static void CheckAndAutoGenerate()
        {
            if (!File.Exists(SCENE_PATH))
            {
                Debug.Log("<color=#00FF88>[MainMenuSceneSetupTool]</color> Tự động khởi tạo Scene MainMenu.unity...");
                SetupMainMenuScene(saveAndOpen: true);
            }
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/Dựng Scene MainMenu Chuẩn (1-Click)", priority = 10)]
        public static void SetupMainMenuFromMenu()
        {
            SetupMainMenuScene(saveAndOpen: true);
        }

        public static void SetupMainMenuSceneBatch()
        {
            SetupMainMenuScene(saveAndOpen: true);
        }

        public static void SetupMainMenuScene(bool saveAndOpen = true)
        {
            // 1. Tạo mới một scene trống
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Main Camera
            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f, 1.0f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 0, -10f);

            // 3. EventSystem (New Input System)
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();

            // 4. --- GAME MANAGERS ---
            var managersObj = new GameObject("--- GAME MANAGERS ---");
            var gameStateMgr = managersObj.AddComponent<GameStateManager>();
            var gameMgr = managersObj.AddComponent<GameManager>();
            var currencyMgr = managersObj.AddComponent<MetaCurrencyManager>();
            var relicInvMgr = managersObj.AddComponent<RelicInventoryManager>();
            var gachaMgr = managersObj.AddComponent<RelicGachaManager>();
            var transitionController = managersObj.AddComponent<MetaSceneTransitionController>();

            // Gán Banner cho Gacha nếu có
            var gachaBannerSO = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.MetaProgression.Gacha.Data.GachaBannerConfigSO>("Assets/_Data/Gacha/GachaBanner_Standard_VanCo.asset");
            if (gachaBannerSO != null)
            {
                var soGacha = new SerializedObject(gachaMgr);
                var propBanner = soGacha.FindProperty("_activeBanner");
                if (propBanner != null)
                {
                    propBanner.objectReferenceValue = gachaBannerSO;
                    soGacha.ApplyModifiedProperties();
                }
            }

            // 5. Canvas_Master
            var canvasObj = new GameObject("Canvas_Master");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 6. Canvas_MetaMenu (Root UI Menu)
            var metaMenuObj = new GameObject("Canvas_MetaMenu");
            metaMenuObj.transform.SetParent(canvasObj.transform, false);
            var metaRect = metaMenuObj.AddComponent<RectTransform>();
            StretchFull(metaRect);

            var metaCanvasGroup = metaMenuObj.AddComponent<CanvasGroup>();
            metaCanvasGroup.alpha = 1f;
            metaCanvasGroup.interactable = true;
            metaCanvasGroup.blocksRaycasts = true;

            var metaUIManager = metaMenuObj.AddComponent<MetaUIManager>();

            // 6.1. Persistent_MetaBackdrop
            var backdropObj = new GameObject("Persistent_MetaBackdrop");
            backdropObj.transform.SetParent(metaMenuObj.transform, false);
            var backdropRect = backdropObj.AddComponent<RectTransform>();
            StretchFull(backdropRect);
            var backdropImg = backdropObj.AddComponent<Image>();
            backdropImg.raycastTarget = false;
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png");
            if (bgSprite != null)
            {
                backdropImg.sprite = bgSprite;
                backdropImg.color = Color.white;
            }
            else
            {
                backdropImg.color = new Color(0.06f, 0.08f, 0.11f, 1.0f);
            }

            // 6.2. MainHubUI (Sảnh chính)
            GameObject hubInstance = null;
            var hubPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/MainHubUI.prefab");
            if (hubPrefab != null)
            {
                hubInstance = (GameObject)PrefabUtility.InstantiatePrefab(hubPrefab, metaMenuObj.transform);
                hubInstance.name = "Panel_MainHub";
                hubInstance.SetActive(true);
            }

            // 6.3. Các Modal màn hình phụ (Instantiate từ Prefabs và SetActive(false))
            InstantiateModalPrefab("Assets/_Prefabs/UI/CharacterSelectionUI.prefab", "Modal_CharacterSelection", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/WeaponLoadoutUI.prefab", "Modal_WeaponLoadout", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/SettingsModalUI.prefab", "Modal_Settings", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/Gacha/GachaShopPanel.prefab", "Modal_Gacha", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/LobbyModalUI.prefab", "Modal_Lobby", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/StageSelect_Screen.prefab", "Modal_StageSelect", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/SanctuaryTreeUI.prefab", "Modal_SanctuaryTree", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/CardCodexUI.prefab", "Modal_CardCodex", metaMenuObj.transform);
            InstantiateModalPrefab("Assets/_Prefabs/UI/LoadingScreenUI.prefab", "Screen_Loading", metaMenuObj.transform);

            // 6.4. FadeTransitionOverlay
            var fadeOverlayObj = new GameObject("FadeTransitionOverlay");
            fadeOverlayObj.transform.SetParent(canvasObj.transform, false);
            var fadeRect = fadeOverlayObj.AddComponent<RectTransform>();
            StretchFull(fadeRect);
            var fadeImg = fadeOverlayObj.AddComponent<Image>();
            fadeImg.color = Color.black;
            fadeImg.raycastTarget = false;
            var fadeCg = fadeOverlayObj.AddComponent<CanvasGroup>();
            fadeCg.alpha = 0f;
            fadeCg.interactable = false;
            fadeCg.blocksRaycasts = false;

            // 7. Kết nối Serialized Properties
            var soMeta = new SerializedObject(metaUIManager);
            var propMetaGroup = soMeta.FindProperty("_metaCanvasGroup");
            if (propMetaGroup != null) propMetaGroup.objectReferenceValue = metaCanvasGroup;
            var propBackdrop = soMeta.FindProperty("_persistentBackdrop");
            if (propBackdrop != null) propBackdrop.objectReferenceValue = backdropObj;
            var propRegistry = soMeta.FindProperty("_uiRegistry");
            var uiRegistrySO = AssetDatabase.LoadAssetAtPath<UIRegistrySO>("Assets/Resources/UI/UIRegistry.asset");
            if (propRegistry != null && uiRegistrySO != null) propRegistry.objectReferenceValue = uiRegistrySO;
            if (hubInstance != null)
            {
                var hubView = hubInstance.GetComponent<BaseMetaScreenView>();
                var propHub = soMeta.FindProperty("_mainHubScreen");
                if (propHub != null && hubView != null) propHub.objectReferenceValue = hubView;
            }
            soMeta.ApplyModifiedProperties();

            // Kết nối TransitionController
            var soTrans = new SerializedObject(transitionController);
            var propFade = soTrans.FindProperty("_fadeOverlayCanvasGroup");
            if (propFade != null) propFade.objectReferenceValue = fadeCg;
            var propMetaMgr = soTrans.FindProperty("_metaUIManager");
            if (propMetaMgr != null) propMetaMgr.objectReferenceValue = metaUIManager;
            if (hubInstance != null)
            {
                var hubPresenter = hubInstance.GetComponent<MainHubPresenter>();
                var propPresenter = soTrans.FindProperty("_mainHubPresenter");
                if (propPresenter != null && hubPresenter != null) propPresenter.objectReferenceValue = hubPresenter;
            }
            soTrans.ApplyModifiedProperties();

            // 8. Đảm bảo thư mục Assets/Scenes tồn tại
            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }

            // 9. Lưu Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"<color=#00FF88>[MainMenuSceneSetupTool]</color> Đã tạo và lưu thành công Scene: {SCENE_PATH}");

            // 10. Cập nhật EditorBuildSettings (Scene 0: MainMenu, Scene 1: MapSongQue3D_Prototype)
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject InstantiateModalPrefab(string prefabPath, string instanceName, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return null;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = instanceName;
            instance.SetActive(false);
            return instance;
        }

        private static void StretchFull(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        public static void UpdateBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(SCENE_PATH, true),
                new EditorBuildSettingsScene(DEFENSE_SCENE_PATH, true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("<color=#00FF88>[MainMenuSceneSetupTool]</color> Đã cập nhật EditorBuildSettings:\n 0 -> " + SCENE_PATH + "\n 1 -> " + DEFENSE_SCENE_PATH);
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor Tool 1-Click tự động quét, chuẩn hóa và dựng cấu trúc trọn bộ UI Canvas trong Scene:
    /// - Canvas_MetaMenu (Sảnh Hoàng Tuyền, Điện Anh Hùng, Miếu Tứ Bất Tử, Screen Stack)
    /// - Canvas_Gameplay (HUD, Mobile Controls: Joystick, Attack, Dash, Skill)
    /// - Fade Transition Overlay & GameStateManager / MetaSceneTransitionController
    /// </summary>
    public class FullGameUISetupTool : EditorWindow
    {
        [MenuItem("Tools/ProjectZombie/Setup Full UI Hierarchy (1-Click)", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<FullGameUISetupTool>("Setup Game UI");
            window.minSize = new Vector2(460, 420);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Cấu Trúc Toàn Bộ UI Canvas (Hướng A - All-in-One)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Tool này sẽ tự động thiết lập hoặc chuẩn hóa Scene hiện tại:\n" +
                "1. Tạo/Chuẩn hóa Canvas_MetaMenu (Sảnh Hoàng Tuyền, Chọn Anh Hùng, Miếu Nâng Cấp).\n" +
                "2. Tạo/Chuẩn hóa Canvas_Gameplay (Run HUD, cụm nút Mobile Controls: Attack, Dash, Skill).\n" +
                "3. Thiết lập Fade Transition Overlay và kết nối MetaSceneTransitionController.\n" +
                "4. Tự động Wire toàn bộ View và Presenter theo chuẩn MVP.",
                MessageType.Info
            );

            EditorGUILayout.Space(15);

            if (GUILayout.Button("⚡ Tự Động Dựng & Chuẩn Hóa Toàn Bộ Canvas", GUILayout.Height(45)))
            {
                SetupFullUIInScene();
            }
        }

        public static void SetupFullUIInScene()
        {
            // 1. Tìm hoặc tạo Canvas Chính
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas_Master", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                mainCanvas = canvasObj.GetComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            Undo.RegisterFullObjectHierarchyUndo(mainCanvas.gameObject, "Setup Full UI Canvas");

            var scaler = mainCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // 2. Dựng Canvas_MetaMenu Root
            Transform metaRoot = mainCanvas.transform.Find("Canvas_MetaMenu");
            if (metaRoot == null)
            {
                GameObject metaObj = new GameObject("Canvas_MetaMenu", typeof(RectTransform), typeof(CanvasGroup), typeof(MetaUIManager));
                metaObj.transform.SetParent(mainCanvas.transform, false);
                metaRoot = metaObj.transform;
            }

            StretchRect(metaRoot.GetComponent<RectTransform>());
            var metaGroup = metaRoot.GetComponent<CanvasGroup>();
            var metaManager = metaRoot.GetComponent<MetaUIManager>();

            // 2.1. Panel_MainHub
            Transform hubTrans = metaRoot.Find("Panel_MainHub");
            if (hubTrans == null)
            {
                GameObject hubObj = new GameObject("Panel_MainHub", typeof(RectTransform), typeof(CanvasGroup), typeof(MainHubView), typeof(MainHubPresenter));
                hubObj.transform.SetParent(metaRoot, false);
                hubTrans = hubObj.transform;
            }
            StretchRect(hubTrans.GetComponent<RectTransform>());
            BuildMainHubHierarchy(hubTrans);

            // 2.2. Panel_CharacterSelect
            Transform heroTrans = metaRoot.Find("Panel_CharacterSelect");
            if (heroTrans == null)
            {
                GameObject heroObj = new GameObject("Panel_CharacterSelect", typeof(RectTransform), typeof(CanvasGroup), typeof(CharacterSelectionView), typeof(CharacterSelectionPresenter));
                heroObj.transform.SetParent(metaRoot, false);
                heroTrans = heroObj.transform;
            }
            StretchRect(heroTrans.GetComponent<RectTransform>());

            // 2.3. Panel_SanctuaryTree
            Transform sanctuaryTrans = metaRoot.Find("Panel_SanctuaryTree");
            if (sanctuaryTrans == null)
            {
                GameObject sanctuaryObj = new GameObject("Panel_SanctuaryTree", typeof(RectTransform), typeof(CanvasGroup), typeof(MetaUpgradeShopView), typeof(MetaUpgradeShopPresenter));
                sanctuaryObj.transform.SetParent(metaRoot, false);
                sanctuaryTrans = sanctuaryObj.transform;
            }
            StretchRect(sanctuaryTrans.GetComponent<RectTransform>());

            // Wire MetaUIManager
            var soMeta = new SerializedObject(metaManager);
            soMeta.FindProperty("_metaCanvasGroup").objectReferenceValue = metaGroup;
            soMeta.FindProperty("_mainHubScreen").objectReferenceValue = hubTrans.GetComponent<MainHubView>();
            soMeta.FindProperty("_characterSelectScreen").objectReferenceValue = heroTrans.GetComponent<CharacterSelectionView>();
            soMeta.FindProperty("_sanctuaryTreeScreen").objectReferenceValue = sanctuaryTrans.GetComponent<MetaUpgradeShopView>();
            soMeta.ApplyModifiedProperties();

            // 3. Dựng Canvas_Gameplay Root
            Transform gameRoot = mainCanvas.transform.Find("Canvas_Gameplay");
            if (gameRoot == null)
            {
                GameObject gameObj = new GameObject("Canvas_Gameplay", typeof(RectTransform), typeof(CanvasGroup));
                gameObj.transform.SetParent(mainCanvas.transform, false);
                gameRoot = gameObj.transform;
            }
            StretchRect(gameRoot.GetComponent<RectTransform>());
            var gameGroup = gameRoot.GetComponent<CanvasGroup>();

            // 4. Fade Overlay Panel
            Transform fadeTrans = mainCanvas.transform.Find("Panel_FadeOverlay");
            if (fadeTrans == null)
            {
                GameObject fadeObj = new GameObject("Panel_FadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                fadeObj.transform.SetParent(mainCanvas.transform, false);
                fadeTrans = fadeObj.transform;

                Image img = fadeObj.GetComponent<Image>();
                img.color = Color.black;
            }
            StretchRect(fadeTrans.GetComponent<RectTransform>());
            var fadeGroup = fadeTrans.GetComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeObjOrTrans(fadeTrans, false);

            // 5. MetaSceneTransitionController
            var transitionController = FindObjectOfType<MetaSceneTransitionController>();
            if (transitionController == null)
            {
                GameObject tcObj = new GameObject("MetaSceneTransitionController", typeof(MetaSceneTransitionController));
                transitionController = tcObj.GetComponent<MetaSceneTransitionController>();
            }

            var soTC = new SerializedObject(transitionController);
            soTC.FindProperty("_metaMenuCanvasGroup").objectReferenceValue = metaGroup;
            soTC.FindProperty("_gameplayCanvasGroup").objectReferenceValue = gameGroup;
            soTC.FindProperty("_fadeOverlayCanvasGroup").objectReferenceValue = fadeGroup;
            soTC.FindProperty("_mainHubPresenter").objectReferenceValue = hubTrans.GetComponent<MainHubPresenter>();
            soTC.ApplyModifiedProperties();

            // Tự động gọi MobileControlsSetupTool để dựng cụm Joystick & Attack Button
            EditorApplication.ExecuteMenuItem("Tools/ProjectZombie/Mobile Controls Setup & Auto-Wire");

            EditorUtility.SetDirty(mainCanvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCanvas.gameObject.scene);

            Debug.Log("[FullGameUISetupTool] Đã dựng thành công toàn bộ hệ thống Canvas UI All-in-One!");
        }

        private static void BuildMainHubHierarchy(Transform hubRoot)
        {
            MainHubView hubView = hubRoot.GetComponent<MainHubView>();
            var soHub = new SerializedObject(hubView);

            // Header - CoTien Text
            Transform coTienTrans = hubRoot.Find("Header_CoTien");
            if (coTienTrans == null)
            {
                GameObject coTienObj = new GameObject("Header_CoTien", typeof(RectTransform), typeof(TextMeshProUGUI));
                coTienObj.transform.SetParent(hubRoot, false);
                coTienTrans = coTienObj.transform;

                RectTransform rect = coTienObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(50, -40);
                rect.sizeDelta = new Vector2(300, 60);

                var tmp = coTienObj.GetComponent<TextMeshProUGUI>();
                tmp.fontSize = 32;
                tmp.text = "🪙 0";
            }
            soHub.FindProperty("_coTienText").objectReferenceValue = coTienTrans.GetComponent<TextMeshProUGUI>();

            // Button Start Run (Xuất Trận)
            Transform startBtnTrans = hubRoot.Find("Btn_StartRun");
            if (startBtnTrans == null)
            {
                GameObject startBtnObj = new GameObject("Btn_StartRun", typeof(RectTransform), typeof(Image), typeof(Button));
                startBtnObj.transform.SetParent(hubRoot, false);
                startBtnTrans = startBtnObj.transform;

                RectTransform rect = startBtnObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 150);
                rect.sizeDelta = new Vector2(320, 90);

                Image img = startBtnObj.GetComponent<Image>();
                img.color = new Color(0.85f, 0.25f, 0.2f, 1f);

                GameObject textObj = new GameObject("Text_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(startBtnObj.transform, false);
                StretchRect(textObj.GetComponent<RectTransform>());

                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "⚔️ XUẤT TRẬN";
                tmp.fontSize = 36;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }
            soHub.FindProperty("_startRunButton").objectReferenceValue = startBtnTrans.GetComponent<Button>();

            // Button Hero Select
            Transform heroBtnTrans = hubRoot.Find("Btn_HeroSelect");
            if (heroBtnTrans == null)
            {
                GameObject heroBtnObj = new GameObject("Btn_HeroSelect", typeof(RectTransform), typeof(Image), typeof(Button));
                heroBtnObj.transform.SetParent(hubRoot, false);
                heroBtnTrans = heroBtnObj.transform;

                RectTransform rect = heroBtnObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-220, 150);
                rect.sizeDelta = new Vector2(90, 90);

                Image img = heroBtnObj.GetComponent<Image>();
                img.color = new Color(0.2f, 0.4f, 0.6f, 1f);

                GameObject textObj = new GameObject("Text_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(heroBtnObj.transform, false);
                StretchRect(textObj.GetComponent<RectTransform>());

                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "🧑";
                tmp.fontSize = 32;
                tmp.alignment = TextAlignmentOptions.Center;
            }
            soHub.FindProperty("_heroSelectButton").objectReferenceValue = heroBtnTrans.GetComponent<Button>();

            // Button Sanctuary Tree (Miếu Tứ Bất Tử)
            Transform sanctuaryBtnTrans = hubRoot.Find("Btn_SanctuaryTree");
            if (sanctuaryBtnTrans == null)
            {
                GameObject sanctuaryBtnObj = new GameObject("Btn_SanctuaryTree", typeof(RectTransform), typeof(Image), typeof(Button));
                sanctuaryBtnObj.transform.SetParent(hubRoot, false);
                sanctuaryBtnTrans = sanctuaryBtnObj.transform;

                RectTransform rect = sanctuaryBtnObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(220, 150);
                rect.sizeDelta = new Vector2(90, 90);

                Image img = sanctuaryBtnObj.GetComponent<Image>();
                img.color = new Color(0.3f, 0.6f, 0.3f, 1f);

                GameObject textObj = new GameObject("Text_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(sanctuaryBtnObj.transform, false);
                StretchRect(textObj.GetComponent<RectTransform>());

                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "⛩️";
                tmp.fontSize = 32;
                tmp.alignment = TextAlignmentOptions.Center;
            }
            soHub.FindProperty("_sanctuaryTreeButton").objectReferenceValue = sanctuaryBtnTrans.GetComponent<Button>();

            soHub.ApplyModifiedProperties();
            EditorUtility.SetDirty(hubView);
        }

        private static void StretchRect(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void fadeObjOrTrans(Transform t, bool active)
        {
            if (t != null) t.gameObject.SetActive(active);
        }
    }
}

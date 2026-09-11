#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.StageSelect;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động tạo Prefab UI Chọn Màn Chơi (Stage Selection UI) chuẩn Đông Sơn Cổ Phong.
    /// Tích hợp đầy đủ MVP Pattern (StageSelectUIView & StageSelectUIPresenter).
    /// </summary>
    public static class StageSelectUIGenerator
    {
        private const string PREFAB_PATH = "Assets/_Prefabs/UI/StageSelect_Screen.prefab";

        [MenuItem("Tools/ProjectZombie/UI/Generate Stage Select Screen", priority = 150)]
        public static void GenerateStageSelectUI()
        {
            var vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NotoSerif-Bold SDF.asset")
                           ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            // 1. Root Screen GameObject
            GameObject root = new GameObject("Screen_StageSelect", typeof(RectTransform), typeof(CanvasRenderer));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            var view = root.AddComponent<StageSelectUIView>();
            var presenter = root.AddComponent<StageSelectUIPresenter>();

            // 2. Dark Overlay Background
            GameObject bgObj = CreateUIObject("Dark_Backdrop", root.transform);
            SetStretch(bgObj.GetComponent<RectTransform>());
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.08f, 0.95f);
            bgImg.raycastTarget = true;

            // 3. Main Center Panel (Khung Gỗ Đông Sơn)
            GameObject panelObj = CreateUIObject("Panel_StageCard", root.transform);
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(720f, 520f);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.14f, 0.18f, 1f);

            // Header Title
            var titleText = CreateTMPText("Text_StageTitle", panelObj.transform, vietFont, "Rừng Trúc Âm Ty", 32, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -40f);
            titleRect.sizeDelta = new Vector2(600f, 50f);

            // Thumbnail Image
            GameObject thumbObj = CreateUIObject("Img_StageThumbnail", panelObj.transform);
            var thumbRect = thumbObj.GetComponent<RectTransform>();
            thumbRect.anchorMin = new Vector2(0.5f, 0.5f);
            thumbRect.anchorMax = new Vector2(0.5f, 0.5f);
            thumbRect.anchoredPosition = new Vector2(0f, 60f);
            thumbRect.sizeDelta = new Vector2(400f, 180f);
            var thumbImg = thumbObj.AddComponent<Image>();
            thumbImg.color = new Color(0.2f, 0.25f, 0.3f);

            // Level Recommend & Description
            var recText = CreateTMPText("Text_RecLevel", panelObj.transform, vietFont, "<color=#FFD700>Cấp Đề Cử:</color> Lv.1", 20, TextAlignmentOptions.Center, Color.white);
            var recRect = recText.GetComponent<RectTransform>();
            recRect.anchorMin = new Vector2(0.5f, 0.5f);
            recRect.anchorMax = new Vector2(0.5f, 0.5f);
            recRect.anchoredPosition = new Vector2(0f, -50f);
            recRect.sizeDelta = new Vector2(400f, 30f);

            var descText = CreateTMPText("Text_Description", panelObj.transform, vietFont, "Nơi âm khí tích tụ ngàn năm...", 18, TextAlignmentOptions.Center, new Color(0.8f, 0.85f, 0.9f));
            var descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 0.5f);
            descRect.anchorMax = new Vector2(0.5f, 0.5f);
            descRect.anchoredPosition = new Vector2(0f, -85f);
            descRect.sizeDelta = new Vector2(560f, 40f);

            // Rewards Bar
            var coinText = CreateTMPText("Text_RewardCoin", panelObj.transform, vietFont, "<color=#FFD700>Cổ Tiền:</color> 500", 18, TextAlignmentOptions.Center, Color.white);
            var coinRect = coinText.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0.3f, 0.5f);
            coinRect.anchorMax = new Vector2(0.3f, 0.5f);
            coinRect.anchoredPosition = new Vector2(0f, -120f);
            coinRect.sizeDelta = new Vector2(200f, 30f);

            var expText = CreateTMPText("Text_RewardExp", panelObj.transform, vietFont, "<color=#00E5FF>Kinh Nghiệm:</color> 250", 18, TextAlignmentOptions.Center, Color.white);
            var expRect = expText.GetComponent<RectTransform>();
            expRect.anchorMin = new Vector2(0.7f, 0.5f);
            expRect.anchorMax = new Vector2(0.7f, 0.5f);
            expRect.anchoredPosition = new Vector2(0f, -120f);
            expRect.sizeDelta = new Vector2(200f, 30f);

            // Navigation Buttons (Prev / Next)
            var prevBtn = CreateButton("Btn_PrevStage", panelObj.transform, vietFont, "<", new Vector2(-320f, 60f), new Vector2(50f, 80f), new Color(0.2f, 0.3f, 0.4f));
            var nextBtn = CreateButton("Btn_NextStage", panelObj.transform, vietFont, ">", new Vector2(320f, 60f), new Vector2(50f, 80f), new Color(0.2f, 0.3f, 0.4f));

            // Start Battle Button (Xuất Trận - Tuân thủ quy chuẩn mục 12.9: CẤM hard-code Emoji Unicode)
            var startBtn = CreateButton("Btn_StartBattle", panelObj.transform, vietFont, "XUẤT TRẬN", new Vector2(0f, -180f), new Vector2(240f, 60f), new Color(0.85f, 0.3f, 0.15f));
            
            // Download DLC Button
            var dlcBtn = CreateButton("Btn_DownloadDLC", panelObj.transform, vietFont, "Tải Màn Chơi (12.4 MB)", new Vector2(0f, -180f), new Vector2(260f, 60f), new Color(0.15f, 0.6f, 0.85f));
            var dlcSizeTMP = dlcBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Download Progress Bar (Slider & Text)
            GameObject progressObj = CreateUIObject("Slider_DownloadProgress", panelObj.transform);
            var progressRect = progressObj.GetComponent<RectTransform>();
            progressRect.anchoredPosition = new Vector2(0f, -180f);
            progressRect.sizeDelta = new Vector2(280f, 32f);

            var sliderBg = progressObj.AddComponent<Image>();
            sliderBg.color = new Color(0.08f, 0.1f, 0.14f, 1f);

            var slider = progressObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            GameObject fillArea = CreateUIObject("Fill_Area", progressObj.transform);
            SetStretch(fillArea.GetComponent<RectTransform>());

            GameObject fillObj = CreateUIObject("Fill", fillArea.transform);
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0.15f, 0.75f, 0.95f, 1f);

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;

            var progressStatusText = CreateTMPText("Text_DownloadStatus", progressObj.transform, vietFont, "Đang tải DLC... (0%)", 16, TextAlignmentOptions.Center, Color.white);
            SetStretch(progressStatusText.GetComponent<RectTransform>());
            progressObj.SetActive(false);

            // Close / Back Button
            var backBtn = CreateButton("Btn_Back", root.transform, vietFont, "ĐÓNG", new Vector2(0f, -320f), new Vector2(160f, 50f), new Color(0.4f, 0.4f, 0.4f));

            // Wire Serialized Properties to View
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_stageTitleText").objectReferenceValue = titleText;
            soView.FindProperty("_stageDescText").objectReferenceValue = descText;
            soView.FindProperty("_recommendedLevelText").objectReferenceValue = recText;
            soView.FindProperty("_stageThumbnailImage").objectReferenceValue = thumbImg;
            soView.FindProperty("_coinsRewardText").objectReferenceValue = coinText;
            soView.FindProperty("_expRewardText").objectReferenceValue = expText;
            soView.FindProperty("_startBattleButton").objectReferenceValue = startBtn;
            soView.FindProperty("_downloadDlcButton").objectReferenceValue = dlcBtn;
            soView.FindProperty("_dlcSizeText").objectReferenceValue = dlcSizeTMP;
            soView.FindProperty("_downloadProgressBar").objectReferenceValue = slider;
            soView.FindProperty("_downloadStatusText").objectReferenceValue = progressStatusText;
            soView.FindProperty("_prevStageButton").objectReferenceValue = prevBtn;
            soView.FindProperty("_nextStageButton").objectReferenceValue = nextBtn;
            soView.FindProperty("_backButton").objectReferenceValue = backBtn;
            soView.ApplyModifiedProperties();

            // Wire Presenter Database
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            
            // Nạp sẵn WorldStageDatabase nếu có
            var db = AssetDatabase.LoadAssetAtPath<WorldStageDatabaseSO>("Assets/_Data/Levels/WorldStageDatabase.asset");
            if (db != null)
            {
                var stageListProp = soPresenter.FindProperty("_stageList");
                stageListProp.arraySize = db.Stages.Count;
                for (int i = 0; i < db.Stages.Count; i++)
                {
                    stageListProp.GetArrayElementAtIndex(i).objectReferenceValue = db.Stages[i];
                }
            }
            soPresenter.ApplyModifiedProperties();

            // Save Prefab
            System.IO.Directory.CreateDirectory("Assets/_Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88>[StageSelectUIGenerator]</color> Đã tạo thành công Prefab UI Chọn Ải tại: {PREFAB_PATH}");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, TMP_FontAsset font, string text, float size, TextAlignmentOptions align, Color color)
        {
            GameObject go = CreateUIObject(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, TMP_FontAsset font, string label, Vector2 pos, Vector2 size, Color bgColor)
        {
            GameObject go = CreateUIObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            var btn = go.AddComponent<Button>();

            var text = CreateTMPText("Text_Label", go.transform, font, label, 20, TextAlignmentOptions.Center, Color.white);
            SetStretch(text.GetComponent<RectTransform>());

            return btn;
        }
    }
}
#endif

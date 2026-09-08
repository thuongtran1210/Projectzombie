using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ProjectZombie.Features.UI.Controls.Customization;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động tạo Prefab Overlay Tùy Chỉnh Phím Ảo (Mobile Controls Customizer UI).
    /// Chuẩn Cổ Phong Vọng Xuyên 2.5D: Thanh công cụ Top Bar bo góc gỗ mun, 2 thanh trượt Scale/Opacity và 3 nút Lưu, Mặc Định, Hủy.
    /// </summary>
    public static class MobileControlsCustomizerUIGenerator
    {
        private const string SPRITES_PATH = "Assets/Art/UI/VongXuyen/";
        private const string PREFAB_OUTPUT_PATH = "Assets/_Prefabs/UI/MobileControlsCustomizerUI.prefab";

        [MenuItem("Tools/ProjectZombie/UI/⚡ Generate Mobile Controls Customizer UI", false, 110)]
        public static void GenerateCustomizerUI()
        {
            // 1. Tải Resources
            Sprite cardTotemBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite sliderTrack = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Track_9Slice.png");
            Sprite sliderFill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Fill_9Slice.png");
            Sprite sliderHandle = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Handle_Orb.png");
            Sprite btnAmber = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Battle_Hex_Amber_Glow.png");
            Sprite btnGoMun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");
            Sprite btnSonMai = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_SonMai_ChuSa.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 2. Root Overlay
            GameObject rootObj = new GameObject("MobileControlsCustomizerUI", typeof(RectTransform), typeof(CanvasGroup), typeof(MobileControlsCustomizerView), typeof(MobileControlsCustomizerPresenter));
            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // 3. Thanh Công Cụ Trên Cùng (Top Toolbar - Chiều cao 110, bo góc gỗ Cổ Phong)
            GameObject topBarObj = new GameObject("Toolbar_Top", typeof(RectTransform), typeof(Image));
            topBarObj.transform.SetParent(rootObj.transform, false);
            RectTransform tbRT = topBarObj.GetComponent<RectTransform>();
            tbRT.anchorMin = new Vector2(0.5f, 1f);
            tbRT.anchorMax = new Vector2(0.5f, 1f);
            tbRT.pivot = new Vector2(0.5f, 1f);
            tbRT.anchoredPosition = new Vector2(0, -10);
            tbRT.sizeDelta = new Vector2(1020, 110);

            Image tbImg = topBarObj.GetComponent<Image>();
            tbImg.color = new Color(0.12f, 0.08f, 0.07f, 0.95f);
            tbImg.type = Image.Type.Sliced;
            if (cardTotemBg != null) tbImg.sprite = cardTotemBg;

            // 3.1. Tiêu đề Nút Đang Chọn (Title Info)
            GameObject titleObj = new GameObject("Txt_SelectedControlTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(topBarObj.transform, false);
            RectTransform ttRT = titleObj.GetComponent<RectTransform>();
            ttRT.anchorMin = new Vector2(0, 1f);
            ttRT.anchorMax = new Vector2(0, 1f);
            ttRT.pivot = new Vector2(0, 1f);
            ttRT.anchoredPosition = new Vector2(18, -12);
            ttRT.sizeDelta = new Vector2(340, 30);
            TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) titleTMP.font = vietFont;
            titleTMP.fontSize = 17;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;
            titleTMP.text = "📐 Chạm vào một nút để chỉnh sửa";
            titleTMP.color = new Color(1f, 0.88f, 0.55f);

            // 3.2. Subtitle hướng dẫn
            GameObject guideObj = new GameObject("Txt_Guide", typeof(RectTransform), typeof(TextMeshProUGUI));
            guideObj.transform.SetParent(topBarObj.transform, false);
            RectTransform gdRT = guideObj.GetComponent<RectTransform>();
            gdRT.anchorMin = new Vector2(0, 0);
            gdRT.anchorMax = new Vector2(0, 0);
            gdRT.pivot = new Vector2(0, 0);
            gdRT.anchoredPosition = new Vector2(18, 12);
            gdRT.sizeDelta = new Vector2(340, 48);
            TextMeshProUGUI gdTMP = guideObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) gdTMP.font = vietFont;
            gdTMP.fontSize = 13;
            gdTMP.alignment = TextAlignmentOptions.Left;
            gdTMP.text = "• Kéo thả nút để đổi vị trí\n• Dùng thanh trượt giữa để chỉnh Cỡ / Mờ";
            gdTMP.color = new Color(0.82f, 0.82f, 0.82f);

            // 3.3. Sliders Area (Scale & Opacity) đặt ở vị trí X = 370 -> 640
            // Slider 1: Scale (70% - 150%)
            Slider scaleSlider = CreateMiniSlider(topBarObj.transform, "Slider_Scale", "Cỡ:", new Vector2(370, -20), 0.7f, 1.5f, 1.0f, sliderTrack, sliderFill, sliderHandle, vietFont, out TextMeshProUGUI scaleValTMP);
            
            // Slider 2: Opacity (30% - 100%)
            Slider opacitySlider = CreateMiniSlider(topBarObj.transform, "Slider_Opacity", "Mờ:", new Vector2(370, -62), 0.3f, 1.0f, 1.0f, sliderTrack, sliderFill, sliderHandle, vietFont, out TextMeshProUGUI opacityValTMP);

            // 3.4. Action Buttons (Save, Reset, Cancel) đặt ở bên phải X = 660 -> 1000
            // Nút Lưu to nổi bật (Xanh ngọc / Hổ phách kim)
            Button btnSave = CreateActionButton(topBarObj.transform, "Btn_Save", "💾 LƯU", new Vector2(720, -55), new Vector2(110, 68), btnAmber != null ? btnAmber : btnGoMun, new Color(0.2f, 1.0f, 0.5f), vietFont, 16);
            // Nút Mặc định (Gỗ mun)
            Button btnReset = CreateActionButton(topBarObj.transform, "Btn_Reset", "🔄 MẶC ĐỊNH", new Vector2(850, -55), new Vector2(115, 68), btnGoMun, new Color(0.95f, 0.95f, 0.95f), vietFont, 14);
            // Nút Hủy (Sơn mài chu sa đỏ)
            Button btnCancel = CreateActionButton(topBarObj.transform, "Btn_Cancel", "❌ HỦY", new Vector2(965, -55), new Vector2(95, 68), btnSonMai != null ? btnSonMai : btnGoMun, new Color(1f, 0.6f, 0.6f), vietFont, 14);

            // 4. Wire View & Presenter
            MobileControlsCustomizerView view = rootObj.GetComponent<MobileControlsCustomizerView>();
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_panelRoot").objectReferenceValue = topBarObj;
            soView.FindProperty("_selectedControlTitleText").objectReferenceValue = titleTMP;
            soView.FindProperty("_scaleSlider").objectReferenceValue = scaleSlider;
            soView.FindProperty("_scaleValueText").objectReferenceValue = scaleValTMP;
            soView.FindProperty("_opacitySlider").objectReferenceValue = opacitySlider;
            soView.FindProperty("_opacityValueText").objectReferenceValue = opacityValTMP;
            soView.FindProperty("_saveButton").objectReferenceValue = btnSave;
            soView.FindProperty("_resetDefaultButton").objectReferenceValue = btnReset;
            soView.FindProperty("_cancelButton").objectReferenceValue = btnCancel;
            soView.ApplyModifiedProperties();

            MobileControlsCustomizerPresenter presenter = rootObj.GetComponent<MobileControlsCustomizerPresenter>();
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            rootObj.SetActive(false);

            // 5. Save Prefabs
            string resourcesDir = "Assets/Resources/UI";
            if (!System.IO.Directory.Exists(resourcesDir)) System.IO.Directory.CreateDirectory(resourcesDir);
            string resourcesPrefabPath = $"{resourcesDir}/MobileControlsCustomizerUI.prefab";

            PrefabUtility.SaveAsPrefabAsset(rootObj, PREFAB_OUTPUT_PATH);
            PrefabUtility.SaveAsPrefabAsset(rootObj, resourcesPrefabPath);
            Object.DestroyImmediate(rootObj);

            Debug.Log($"<color=#00FF88>[MobileControlsCustomizerUIGenerator]</color> Đã tạo thành công Prefab Customizer tại: {PREFAB_OUTPUT_PATH} và {resourcesPrefabPath}");
        }

        private static Slider CreateMiniSlider(Transform parent, string name, string label, Vector2 pos, float min, float max, float val, Sprite track, Sprite fill, Sprite handle, TMP_FontAsset font, out TextMeshProUGUI valTMP)
        {
            GameObject rowObj = new GameObject(name, typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            RectTransform rRT = rowObj.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0, 1f);
            rRT.anchorMax = new Vector2(0, 1f);
            rRT.pivot = new Vector2(0, 1f);
            rRT.anchoredPosition = pos;
            rRT.sizeDelta = new Vector2(250, 32);

            // Label
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(rowObj.transform, false);
            RectTransform lRT = lblObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f);
            lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = Vector2.zero;
            lRT.sizeDelta = new Vector2(42, 28);
            TextMeshProUGUI lTMP = lblObj.GetComponent<TextMeshProUGUI>();
            if (font != null) lTMP.font = font;
            lTMP.fontSize = 15;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.text = label;
            lTMP.color = new Color(0.95f, 0.88f, 0.72f);

            // Slider
            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
            sliderObj.transform.SetParent(rowObj.transform, false);
            RectTransform sRT = sliderObj.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0, 0.5f);
            sRT.anchorMax = new Vector2(0, 0.5f);
            sRT.pivot = new Vector2(0, 0.5f);
            sRT.anchoredPosition = new Vector2(46, 0);
            sRT.sizeDelta = new Vector2(145, 18);

            Image trackImg = sliderObj.GetComponent<Image>();
            trackImg.color = Color.white;
            trackImg.type = Image.Type.Sliced;
            if (track != null) trackImg.sprite = track;

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = val;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = new Vector2(2, 2);
            faRT.offsetMax = new Vector2(-2, -2);

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fRT = fillObj.GetComponent<RectTransform>();
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.sizeDelta = Vector2.zero;
            Image fImg = fillObj.GetComponent<Image>();
            fImg.color = new Color(1f, 0.82f, 0.35f, 1f);
            fImg.type = Image.Type.Sliced;
            if (fill != null) fImg.sprite = fill;
            slider.fillRect = fRT;

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform haRT = handleArea.GetComponent<RectTransform>();
            haRT.anchorMin = Vector2.zero;
            haRT.anchorMax = Vector2.one;
            haRT.offsetMin = new Vector2(6, 0);
            haRT.offsetMax = new Vector2(-6, 0);

            GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObj.transform.SetParent(handleArea.transform, false);
            RectTransform hRT = handleObj.GetComponent<RectTransform>();
            hRT.sizeDelta = new Vector2(26, 26);
            Image hImg = handleObj.GetComponent<Image>();
            hImg.color = Color.white;
            if (handle != null) hImg.sprite = handle;
            hImg.preserveAspect = true;
            slider.handleRect = hRT;

            // Val Text
            GameObject valObj = new GameObject("Txt_Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            valObj.transform.SetParent(rowObj.transform, false);
            RectTransform vRT = valObj.GetComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0, 0.5f);
            vRT.anchorMax = new Vector2(0, 0.5f);
            vRT.pivot = new Vector2(0, 0.5f);
            vRT.anchoredPosition = new Vector2(196, 0);
            vRT.sizeDelta = new Vector2(50, 26);
            valTMP = valObj.GetComponent<TextMeshProUGUI>();
            if (font != null) valTMP.font = font;
            valTMP.fontSize = 14;
            valTMP.fontStyle = FontStyles.Bold;
            valTMP.alignment = TextAlignmentOptions.Left;
            valTMP.text = "100%";
            valTMP.color = new Color(0.98f, 0.88f, 0.60f);

            return slider;
        }

        private static Button CreateActionButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Sprite bgSprite, Color textColor, TMP_FontAsset font, float fontSize = 14f)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform bRT = btnObj.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 1f);
            bRT.anchorMax = new Vector2(0, 1f);
            bRT.pivot = new Vector2(0.5f, 0.5f);
            bRT.anchoredPosition = pos;
            bRT.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.color = Color.white;
            img.type = Image.Type.Sliced;
            if (bgSprite != null) img.sprite = bgSprite;

            Button btn = btnObj.GetComponent<Button>();

            GameObject txtObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform tRT = txtObj.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = label;
            tmp.color = textColor;

            return btn;
        }
    }
}

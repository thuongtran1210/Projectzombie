using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động tạo và cấu hình Prefab Modal Cài Đặt (Settings Modal) theo phong cách Cổ Phong Vọng Xuyên 2.5D.
    /// </summary>
    public static class SettingsUIGenerator
    {
        private const string SPRITES_PATH = "Assets/Art/UI/VongXuyen/";
        private const string PREFAB_OUTPUT_PATH = "Assets/_Prefabs/UI/SettingsModalUI.prefab";

        [MenuItem("Tools/ProjectZombie/UI/Generate Settings UI Prefab", false, 105)]
        public static void GenerateSettingsModal()
        {
            // 1. Tải Resources Sprite & Font
            Sprite modalWoodFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Frame_Modal_TangBaoCac_9Slice.png");
            Sprite bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Settings_Parchment.png");
            Sprite sliderTrack = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Track_9Slice.png");
            Sprite sliderFill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Fill_9Slice.png");
            Sprite sliderHandle = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Handle_Orb.png");
            Sprite toggleBoxOff = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Toggle_Wood_Box_Off.png");
            Sprite toggleCheckOn = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Toggle_Wood_Checkmark_On.png");
            Sprite btnWoodSub = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Upgrade_Wood_Sub_9Slice.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 2. Tạo Root Modal
            GameObject modalRoot = new GameObject("Modal_Settings", typeof(RectTransform), typeof(CanvasGroup), typeof(SettingsModalView), typeof(SettingsModalPresenter));
            RectTransform rootRT = modalRoot.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // 3. Dark Overlay
            GameObject overlayObj = new GameObject("Overlay_Dark", typeof(RectTransform), typeof(Image), typeof(Button));
            overlayObj.transform.SetParent(modalRoot.transform, false);
            RectTransform ovRT = overlayObj.GetComponent<RectTransform>();
            ovRT.anchorMin = Vector2.zero;
            ovRT.anchorMax = Vector2.one;
            ovRT.offsetMin = Vector2.zero;
            ovRT.offsetMax = Vector2.zero;
            Image ovImg = overlayObj.GetComponent<Image>();
            ovImg.color = new Color(0.04f, 0.02f, 0.06f, 0.78f);
            Button ovBtn = overlayObj.GetComponent<Button>();

            // 4. Modal Container Frame (Gỗ Mun 9-Slice)
            GameObject frameObj = new GameObject("Frame_Settings_Content", typeof(RectTransform), typeof(Image));
            frameObj.transform.SetParent(modalRoot.transform, false);
            RectTransform frRT = frameObj.GetComponent<RectTransform>();
            frRT.anchorMin = new Vector2(0.5f, 0.5f);
            frRT.anchorMax = new Vector2(0.5f, 0.5f);
            frRT.pivot = new Vector2(0.5f, 0.5f);
            frRT.anchoredPosition = Vector2.zero;
            frRT.sizeDelta = new Vector2(580, 520);
            Image frImg = frameObj.GetComponent<Image>();
            frImg.color = Color.white;
            frImg.type = Image.Type.Sliced;
            if (modalWoodFrame != null) frImg.sprite = modalWoodFrame;

            // 5. Header Banner
            GameObject bannerObj = new GameObject("Banner_Header", typeof(RectTransform), typeof(Image));
            bannerObj.transform.SetParent(frameObj.transform, false);
            RectTransform bnRT = bannerObj.GetComponent<RectTransform>();
            bnRT.anchorMin = new Vector2(0.5f, 1f);
            bnRT.anchorMax = new Vector2(0.5f, 1f);
            bnRT.pivot = new Vector2(0.5f, 0.5f);
            bnRT.anchoredPosition = new Vector2(0, 8);
            bnRT.sizeDelta = new Vector2(440, 78);
            Image bnImg = bannerObj.GetComponent<Image>();
            bnImg.color = Color.white;
            if (bannerParchment != null) bnImg.sprite = bannerParchment;

            GameObject titleTextObj = new GameObject("Txt_Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleTextObj.transform.SetParent(bannerObj.transform, false);
            RectTransform ttRT = titleTextObj.GetComponent<RectTransform>();
            ttRT.anchorMin = Vector2.zero;
            ttRT.anchorMax = Vector2.one;
            ttRT.offsetMin = Vector2.zero;
            ttRT.offsetMax = Vector2.zero;
            TextMeshProUGUI ttTMP = titleTextObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) ttTMP.font = vietFont;
            ttTMP.fontSize = 24;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.alignment = TextAlignmentOptions.Center;
            ttTMP.text = "- CÀI ĐẶT HỆ THỐNG -";
            ttTMP.color = new Color(0.24f, 0.14f, 0.08f);

            // 6. Section 1: ÂM THANH (Audio Section)
            // A. BGM Slider Row
            Slider bgmSlider = CreateSliderRow(frameObj.transform, "Row_BGM", "Nhạc Nền (BGM)", new Vector2(0, 110), sliderTrack, sliderFill, sliderHandle, vietFont, out TextMeshProUGUI bgmValTMP);

            // B. SFX Slider Row
            Slider sfxSlider = CreateSliderRow(frameObj.transform, "Row_SFX", "Hiệu Ứng (SFX)", new Vector2(0, 45), sliderTrack, sliderFill, sliderHandle, vietFont, out TextMeshProUGUI sfxValTMP);

            // 7. Section 2: TRẢI NGHIỆM CHIẾN ĐẤU (Game Feel Toggles)
            Toggle shakeToggle = CreateToggleRow(frameObj.transform, "Row_Toggle_Shake", "Rung Màn Hình", new Vector2(0, -25), toggleBoxOff, toggleCheckOn, vietFont);
            Toggle dmgToggle = CreateToggleRow(frameObj.transform, "Row_Toggle_Damage", "Hiện Số Sát Thương", new Vector2(0, -85), toggleBoxOff, toggleCheckOn, vietFont);
            Toggle fpsToggle = CreateToggleRow(frameObj.transform, "Row_Toggle_60FPS", "Mượt Mà 60 FPS", new Vector2(0, -145), toggleBoxOff, toggleCheckOn, vietFont);

            // 8. Close Button (Nút Tròn X ở Góc Trên Bên Phải)
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Nav_Close_X_Wood.png");
            GameObject btnCloseObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            btnCloseObj.transform.SetParent(frameObj.transform, false);
            RectTransform bcRT = btnCloseObj.GetComponent<RectTransform>();
            bcRT.anchorMin = new Vector2(1f, 1f);
            bcRT.anchorMax = new Vector2(1f, 1f);
            bcRT.pivot = new Vector2(0.5f, 0.5f);
            bcRT.anchoredPosition = new Vector2(-14, -14);
            bcRT.sizeDelta = new Vector2(46, 46);
            Image bcImg = btnCloseObj.GetComponent<Image>();
            bcImg.color = Color.white;
            if (btnCloseX != null) bcImg.sprite = btnCloseX;
            Button closeBtn = btnCloseObj.GetComponent<Button>();

            // 9. Wire Components into SettingsModalView (kế thừa BaseMetaScreenView)
            SettingsModalView view = modalRoot.GetComponent<SettingsModalView>();
            CanvasGroup modalCG = modalRoot.GetComponent<CanvasGroup>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_modalContainer").objectReferenceValue = frRT;
            so.FindProperty("_screenCanvasGroup").objectReferenceValue = modalCG;
            so.FindProperty("_dimBackgroundButton").objectReferenceValue = ovBtn;
            so.FindProperty("_bgmSlider").objectReferenceValue = bgmSlider;
            so.FindProperty("_sfxSlider").objectReferenceValue = sfxSlider;
            so.FindProperty("_bgmValText").objectReferenceValue = bgmValTMP;
            so.FindProperty("_sfxValText").objectReferenceValue = sfxValTMP;
            so.FindProperty("_screenShakeToggle").objectReferenceValue = shakeToggle;
            so.FindProperty("_damageNumbersToggle").objectReferenceValue = dmgToggle;
            so.FindProperty("_fps60Toggle").objectReferenceValue = fpsToggle;
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("_overlayCloseButton").objectReferenceValue = ovBtn;
            so.ApplyModifiedProperties();

            // Mặc định ẩn modal
            modalRoot.SetActive(false);

            // 10. Save Prefab
            PrefabUtility.SaveAsPrefabAsset(modalRoot, PREFAB_OUTPUT_PATH);
            Object.DestroyImmediate(modalRoot);

            Debug.Log($"<color=#00FF88>[SettingsUIGenerator]</color> Đã tạo thành công Prefab Cài Đặt tại: {PREFAB_OUTPUT_PATH}");

            // Tự động gắn vào MainHub trong Scene nếu có
            LinkToMainHubScene(PREFAB_OUTPUT_PATH);
        }

        private static Slider CreateSliderRow(Transform parent, string rowName, string label, Vector2 pos, Sprite track, Sprite fill, Sprite handle, TMP_FontAsset font, out TextMeshProUGUI valTMP)
        {
            GameObject rowObj = new GameObject(rowName, typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            RectTransform rRT = rowObj.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0.5f, 0.5f);
            rRT.anchorMax = new Vector2(0.5f, 0.5f);
            rRT.pivot = new Vector2(0.5f, 0.5f);
            rRT.anchoredPosition = pos;
            rRT.sizeDelta = new Vector2(460, 44);

            // Label Text
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(rowObj.transform, false);
            RectTransform lRT = lblObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f);
            lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(10, 0);
            lRT.sizeDelta = new Vector2(150, 36);
            TextMeshProUGUI lblTMP = lblObj.GetComponent<TextMeshProUGUI>();
            if (font != null) lblTMP.font = font;
            lblTMP.fontSize = 17;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.alignment = TextAlignmentOptions.Left;
            lblTMP.text = label;
            lblTMP.color = new Color(0.96f, 0.88f, 0.72f);

            // Slider Object
            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
            sliderObj.transform.SetParent(rowObj.transform, false);
            RectTransform sRT = sliderObj.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 0.5f);
            sRT.anchorMax = new Vector2(0.5f, 0.5f);
            sRT.pivot = new Vector2(0.5f, 0.5f);
            sRT.anchoredPosition = new Vector2(60, 0);
            sRT.sizeDelta = new Vector2(200, 24);

            Image trackImg = sliderObj.GetComponent<Image>();
            trackImg.color = Color.white;
            trackImg.type = Image.Type.Sliced;
            if (track != null) trackImg.sprite = track;

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;

            // Fill Area & Fill
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = new Vector2(4, 3);
            faRT.offsetMax = new Vector2(-4, -3);

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fRT = fillObj.GetComponent<RectTransform>();
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.sizeDelta = Vector2.zero;
            Image fImg = fillObj.GetComponent<Image>();
            fImg.color = Color.white;
            fImg.type = Image.Type.Sliced;
            if (fill != null) fImg.sprite = fill;
            slider.fillRect = fRT;

            // Handle Slide Area & Handle
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform haRT = handleArea.GetComponent<RectTransform>();
            haRT.anchorMin = Vector2.zero;
            haRT.anchorMax = Vector2.one;
            haRT.offsetMin = new Vector2(8, 0);
            haRT.offsetMax = new Vector2(-8, 0);

            GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObj.transform.SetParent(handleArea.transform, false);
            RectTransform hRT = handleObj.GetComponent<RectTransform>();
            hRT.sizeDelta = new Vector2(34, 34);
            Image hImg = handleObj.GetComponent<Image>();
            hImg.color = Color.white;
            if (handle != null) hImg.sprite = handle;
            hImg.preserveAspect = true;
            slider.handleRect = hRT;

            // Value Text (e.g. 80%)
            GameObject valObj = new GameObject("Txt_Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            valObj.transform.SetParent(rowObj.transform, false);
            RectTransform vRT = valObj.GetComponent<RectTransform>();
            vRT.anchorMin = new Vector2(1, 0.5f);
            vRT.anchorMax = new Vector2(1, 0.5f);
            vRT.pivot = new Vector2(1, 0.5f);
            vRT.anchoredPosition = new Vector2(-10, 0);
            vRT.sizeDelta = new Vector2(60, 36);
            valTMP = valObj.GetComponent<TextMeshProUGUI>();
            if (font != null) valTMP.font = font;
            valTMP.fontSize = 17;
            valTMP.fontStyle = FontStyles.Bold;
            valTMP.alignment = TextAlignmentOptions.Right;
            valTMP.text = "80%";
            valTMP.color = new Color(0.98f, 0.88f, 0.60f);

            return slider;
        }

        private static Toggle CreateToggleRow(Transform parent, string rowName, string label, Vector2 pos, Sprite boxOff, Sprite checkOn, TMP_FontAsset font)
        {
            GameObject rowObj = new GameObject(rowName, typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            RectTransform rRT = rowObj.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0.5f, 0.5f);
            rRT.anchorMax = new Vector2(0.5f, 0.5f);
            rRT.pivot = new Vector2(0.5f, 0.5f);
            rRT.anchoredPosition = pos;
            rRT.sizeDelta = new Vector2(460, 44);

            // Label Text
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(rowObj.transform, false);
            RectTransform lRT = lblObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f);
            lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(10, 0);
            lRT.sizeDelta = new Vector2(250, 36);
            TextMeshProUGUI lblTMP = lblObj.GetComponent<TextMeshProUGUI>();
            if (font != null) lblTMP.font = font;
            lblTMP.fontSize = 17;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.alignment = TextAlignmentOptions.Left;
            lblTMP.text = label;
            lblTMP.color = new Color(0.96f, 0.88f, 0.72f);

            // Toggle Object
            GameObject toggleObj = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(rowObj.transform, false);
            RectTransform tRT = toggleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(1, 0.5f);
            tRT.anchorMax = new Vector2(1, 0.5f);
            tRT.pivot = new Vector2(1, 0.5f);
            tRT.anchoredPosition = new Vector2(-15, 0);
            tRT.sizeDelta = new Vector2(40, 40);

            // Background Image
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(toggleObj.transform, false);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.color = Color.white;
            bgImg.type = Image.Type.Sliced;
            if (boxOff != null) bgImg.sprite = boxOff;

            // Checkmark Image
            GameObject chkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            chkObj.transform.SetParent(bgObj.transform, false);
            RectTransform chkRT = chkObj.GetComponent<RectTransform>();
            chkRT.anchorMin = Vector2.zero;
            chkRT.anchorMax = Vector2.one;
            chkRT.sizeDelta = Vector2.zero;
            Image chkImg = chkObj.GetComponent<Image>();
            chkImg.color = Color.white;
            if (checkOn != null) chkImg.sprite = checkOn;
            chkImg.preserveAspect = true;

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = chkImg;
            toggle.isOn = true;

            return toggle;
        }

        [MenuItem("Tools/ProjectZombie/UI/⚡ Rebuild Settings UI Modal", false, 106)]
        public static void RebuildSettingsUI()
        {
            GenerateSettingsModal();
        }

        private static void LinkToMainHubScene(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            Transform targetParent = null;

            var metaManager = Object.FindObjectOfType<MetaUIManager>(true);
            if (metaManager != null)
            {
                targetParent = metaManager.transform;
            }
            else
            {
                var mainHubView = Object.FindObjectOfType<MainHubView>(true);
                if (mainHubView != null)
                {
                    var canvas = mainHubView.GetComponentInParent<Canvas>();
                    if (canvas != null) targetParent = canvas.transform;
                }
            }

            if (targetParent != null)
            {
                Transform existingModal = targetParent.Find("Modal_Settings");
                if (existingModal != null) Object.DestroyImmediate(existingModal.gameObject);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, targetParent);
                instance.name = "Modal_Settings";
                instance.SetActive(false);

                var settingsView = instance.GetComponent<SettingsModalView>();
                if (metaManager != null && settingsView != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaManager);
                    soMeta.FindProperty("_settingsScreen").objectReferenceValue = settingsView;
                    soMeta.ApplyModifiedProperties();
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log("<color=#00FF88>[SettingsUIGenerator]</color> Đã liên kết thành công Modal_Settings vào Canvas và MetaUIManager Screen Stack!");
            }
        }
    }
}

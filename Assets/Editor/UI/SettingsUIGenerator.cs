using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.StatsAndSkills;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động tạo và cấu hình Prefab Modal Cài Đặt (Settings Modal) theo phong cách Cổ Phong Vọng Xuyên 2.5D.
    /// Đảm bảo giao diện đặc nền (Opaque Dark Wood), tương phản cao, hoạt động 100% trên thiết bị di động Android.
    /// </summary>
    public static class SettingsUIGenerator
    {
        private const string SPRITES_PATH = "Assets/Art/UI/VongXuyen/";
        private const string PREFAB_OUTPUT_PATH = "Assets/_Prefabs/UI/SettingsModalUI.prefab";

        [MenuItem("Tools/ProjectZombie/UI/Generate Settings UI Prefab", false, 105)]
        [MenuItem("Tools/ProjectZombie/UI/⚡ Rebuild Settings UI Modal", false, 106)]
        [MenuItem("ProjectZombie/⚡ Rebuild Settings UI Modal", false, 106)]
        public static void RebuildSettingsUI()
        {
            GenerateSettingsModal();
        }

        public static void GenerateSettingsModal()
        {
            // 1. Tải Resources Sprite & Font
            Sprite modalWoodFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Frame_Modal_TangBaoCac_9Slice.png");
            Sprite cardTotemBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite badgePill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Settings_Parchment.png");
            if (bannerParchment == null) bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Parchment_Scroll.png");
            
            Sprite sliderTrack = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Track_9Slice.png");
            Sprite sliderFill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Fill_9Slice.png");
            Sprite sliderHandle = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Handle_Orb.png");
            Sprite toggleBoxOff = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Toggle_Wood_Box_Off.png");
            Sprite toggleCheckOn = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Toggle_Wood_Checkmark_On.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Nav_Close_X_Wood.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
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
            ovImg.color = new Color(0.02f, 0.01f, 0.03f, 0.82f);
            Button ovBtn = overlayObj.GetComponent<Button>();

            // 4. Modal Container Frame (Kích thước chuẩn 600x560)
            GameObject frameObj = new GameObject("Frame_Settings_Content", typeof(RectTransform));
            frameObj.transform.SetParent(modalRoot.transform, false);
            RectTransform frRT = frameObj.GetComponent<RectTransform>();
            frRT.anchorMin = new Vector2(0.5f, 0.5f);
            frRT.anchorMax = new Vector2(0.5f, 0.5f);
            frRT.pivot = new Vector2(0.5f, 0.5f);
            frRT.anchoredPosition = Vector2.zero;
            frRT.sizeDelta = new Vector2(600, 560);

            // 4.1. Nền Gỗ Mun Đặc (Solid Dark Wood Background - Khắc phục tình trạng rỗng/trong suốt)
            GameObject bgObj = new GameObject("Background_Wood_Dark", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(frameObj.transform, false);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = new Vector2(8, 8);
            bgRT.offsetMax = new Vector2(-8, -8);
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.13f, 0.09f, 0.08f, 0.98f);
            if (cardTotemBg != null)
            {
                bgImg.sprite = cardTotemBg;
                bgImg.type = Image.Type.Sliced;
            }

            // 4.2. Khung Viền Rồng Vàng 4 Góc (Ornamental Dragon Frame)
            GameObject borderObj = new GameObject("Frame_Border_Dragon", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(frameObj.transform, false);
            RectTransform borderRT = borderObj.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            Image borderImg = borderObj.GetComponent<Image>();
            borderImg.color = Color.white;
            borderImg.type = Image.Type.Sliced;
            borderImg.raycastTarget = false;
            if (modalWoodFrame != null) borderImg.sprite = modalWoodFrame;

            // 5. Header Banner
            GameObject bannerObj = new GameObject("Banner_Header", typeof(RectTransform), typeof(Image));
            bannerObj.transform.SetParent(frameObj.transform, false);
            RectTransform bnRT = bannerObj.GetComponent<RectTransform>();
            bnRT.anchorMin = new Vector2(0.5f, 1f);
            bnRT.anchorMax = new Vector2(0.5f, 1f);
            bnRT.pivot = new Vector2(0.5f, 1f);
            bnRT.anchoredPosition = new Vector2(0, -12);
            bnRT.sizeDelta = new Vector2(440, 68);
            Image bnImg = bannerObj.GetComponent<Image>();
            bnImg.color = Color.white;
            if (bannerParchment != null)
            {
                bnImg.sprite = bannerParchment;
                bnImg.type = Image.Type.Sliced;
            }

            GameObject titleTextObj = new GameObject("Txt_Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleTextObj.transform.SetParent(bannerObj.transform, false);
            RectTransform ttRT = titleTextObj.GetComponent<RectTransform>();
            ttRT.anchorMin = Vector2.zero;
            ttRT.anchorMax = Vector2.one;
            ttRT.offsetMin = Vector2.zero;
            ttRT.offsetMax = Vector2.zero;
            TextMeshProUGUI ttTMP = titleTextObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) ttTMP.font = vietFont;
            ttTMP.fontSize = 22;
            ttTMP.fontStyle = FontStyles.Bold;
            ttTMP.alignment = TextAlignmentOptions.Center;
            ttTMP.text = "- CÀI ĐẶT HỆ THỐNG -";
            ttTMP.color = new Color(0.24f, 0.14f, 0.08f);

            // 6. Section 1: ÂM THANH (Audio Controls)
            // A. BGM Slider Row
            Slider bgmSlider = CreateSliderRow(frameObj.transform, "Row_BGM", "Nhạc Nền (BGM)", new Vector2(0, 105), sliderTrack, sliderFill, sliderHandle, badgePill, vietFont, out TextMeshProUGUI bgmValTMP);

            // B. SFX Slider Row
            Slider sfxSlider = CreateSliderRow(frameObj.transform, "Row_SFX", "Hiệu Ứng (SFX)", new Vector2(0, 50), sliderTrack, sliderFill, sliderHandle, badgePill, vietFont, out TextMeshProUGUI sfxValTMP);

            // 7. Section 2: TRẢI NGHIỆM CHIẾN ĐẤU (Game Feel Toggles)
            Toggle shakeToggle = CreateToggleRow(frameObj.transform, "Row_Toggle_Shake", "Rung Màn Hình", new Vector2(0, -10), toggleBoxOff, toggleCheckOn, badgePill, vietFont);
            Toggle dmgToggle = CreateToggleRow(frameObj.transform, "Row_Toggle_Damage", "Hiện Số Sát Thương", new Vector2(0, -65), toggleBoxOff, toggleCheckOn, badgePill, vietFont);
            Toggle fpsToggle = CreateToggleRow(frameObj.transform, "Row_Toggle_60FPS", "Mượt Mà 60 FPS", new Vector2(0, -120), toggleBoxOff, toggleCheckOn, badgePill, vietFont);

            // 8. Nút Tùy Chỉnh Phím Ảo (Customize Controls Button)
            Sprite btnAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");
            if (btnAmber == null) btnAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");

            GameObject btnCustomObj = new GameObject("Btn_CustomizeControls", typeof(RectTransform), typeof(Image), typeof(Button));
            btnCustomObj.transform.SetParent(frameObj.transform, false);
            RectTransform cusRT = btnCustomObj.GetComponent<RectTransform>();
            cusRT.anchorMin = new Vector2(0.5f, 0.5f);
            cusRT.anchorMax = new Vector2(0.5f, 0.5f);
            cusRT.pivot = new Vector2(0.5f, 0.5f);
            cusRT.anchoredPosition = new Vector2(0, -185);
            cusRT.sizeDelta = new Vector2(510, 48);

            Image cusImg = btnCustomObj.GetComponent<Image>();
            cusImg.color = Color.white;
            cusImg.type = Image.Type.Sliced;
            if (btnAmber != null) cusImg.sprite = btnAmber;
            Button customBtn = btnCustomObj.GetComponent<Button>();

            GameObject cusTextObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            cusTextObj.transform.SetParent(btnCustomObj.transform, false);
            RectTransform cstRT = cusTextObj.GetComponent<RectTransform>();
            cstRT.anchorMin = Vector2.zero;
            cstRT.anchorMax = Vector2.one;
            cstRT.offsetMin = Vector2.zero;
            cstRT.offsetMax = Vector2.zero;
            TextMeshProUGUI cstTMP = cusTextObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) cstTMP.font = vietFont;
            cstTMP.fontSize = 18;
            cstTMP.fontStyle = FontStyles.Bold;
            cstTMP.alignment = TextAlignmentOptions.Center;
            cstTMP.text = "TÙY CHỈNH PHÍM ĐIỀU KHIỂN";
            cstTMP.color = new Color(1f, 0.92f, 0.65f);

            // 9. Close Button (Nút Đóng X)
            GameObject btnCloseObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            btnCloseObj.transform.SetParent(frameObj.transform, false);
            RectTransform bcRT = btnCloseObj.GetComponent<RectTransform>();
            bcRT.anchorMin = new Vector2(1f, 1f);
            bcRT.anchorMax = new Vector2(1f, 1f);
            bcRT.pivot = new Vector2(0.5f, 0.5f);
            bcRT.anchoredPosition = new Vector2(-16, -16);
            bcRT.sizeDelta = new Vector2(46, 46);
            Image bcImg = btnCloseObj.GetComponent<Image>();
            bcImg.color = Color.white;
            if (btnCloseX != null) bcImg.sprite = btnCloseX;
            Button closeBtn = btnCloseObj.GetComponent<Button>();

            // 10. Wire Components into SettingsModalView
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
            so.FindProperty("_customizeControlsButton").objectReferenceValue = customBtn;
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("_overlayCloseButton").objectReferenceValue = ovBtn;
            so.ApplyModifiedProperties();

            // Wire Presenter
            SettingsModalPresenter presenter = modalRoot.GetComponent<SettingsModalPresenter>();
            if (presenter != null)
            {
                SerializedObject soPresenter = new SerializedObject(presenter);
                soPresenter.FindProperty("_view").objectReferenceValue = view;
                soPresenter.ApplyModifiedProperties();
            }

            modalRoot.SetActive(false);

            // 10. Save Prefab to _Prefabs and Resources
            string resourcesDir = "Assets/Resources/UI";
            if (!System.IO.Directory.Exists(resourcesDir))
            {
                System.IO.Directory.CreateDirectory(resourcesDir);
            }
            string resourcesPrefabPath = $"{resourcesDir}/SettingsModalUI.prefab";

            PrefabUtility.SaveAsPrefabAsset(modalRoot, PREFAB_OUTPUT_PATH);
            PrefabUtility.SaveAsPrefabAsset(modalRoot, resourcesPrefabPath);
            Object.DestroyImmediate(modalRoot);

            Debug.Log($"<color=#00FF88>[SettingsUIGenerator]</color> Đã tạo thành công Prefab Cài Đặt tại: {PREFAB_OUTPUT_PATH} và {resourcesPrefabPath}");

            LinkToMainHubScene(PREFAB_OUTPUT_PATH);
        }

        private static Slider CreateSliderRow(Transform parent, string rowName, string label, Vector2 pos, Sprite track, Sprite fill, Sprite handle, Sprite pillBg, TMP_FontAsset font, out TextMeshProUGUI valTMP)
        {
            GameObject rowObj = new GameObject(rowName, typeof(RectTransform), typeof(Image));
            rowObj.transform.SetParent(parent, false);
            RectTransform rRT = rowObj.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0.5f, 0.5f);
            rRT.anchorMax = new Vector2(0.5f, 0.5f);
            rRT.pivot = new Vector2(0.5f, 0.5f);
            rRT.anchoredPosition = pos;
            rRT.sizeDelta = new Vector2(510, 52);

            Image rowBg = rowObj.GetComponent<Image>();
            rowBg.color = new Color(0.20f, 0.14f, 0.11f, 0.95f);
            rowBg.type = Image.Type.Sliced;
            if (pillBg != null) rowBg.sprite = pillBg;

            // Label Text
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(rowObj.transform, false);
            RectTransform lRT = lblObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f);
            lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(16, 0);
            lRT.sizeDelta = new Vector2(170, 36);
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
            sRT.anchoredPosition = new Vector2(80, 0);
            sRT.sizeDelta = new Vector2(180, 24);

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
            fImg.color = new Color(1f, 0.82f, 0.35f, 1f);
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
            vRT.anchoredPosition = new Vector2(-14, 0);
            vRT.sizeDelta = new Vector2(56, 36);
            valTMP = valObj.GetComponent<TextMeshProUGUI>();
            if (font != null) valTMP.font = font;
            valTMP.fontSize = 17;
            valTMP.fontStyle = FontStyles.Bold;
            valTMP.alignment = TextAlignmentOptions.Right;
            valTMP.text = "80%";
            valTMP.color = new Color(0.98f, 0.88f, 0.60f);

            return slider;
        }

        private static Toggle CreateToggleRow(Transform parent, string rowName, string label, Vector2 pos, Sprite boxOff, Sprite checkOn, Sprite pillBg, TMP_FontAsset font)
        {
            GameObject rowObj = new GameObject(rowName, typeof(RectTransform), typeof(Image));
            rowObj.transform.SetParent(parent, false);
            RectTransform rRT = rowObj.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0.5f, 0.5f);
            rRT.anchorMax = new Vector2(0.5f, 0.5f);
            rRT.pivot = new Vector2(0.5f, 0.5f);
            rRT.anchoredPosition = pos;
            rRT.sizeDelta = new Vector2(510, 48);

            Image rowBg = rowObj.GetComponent<Image>();
            rowBg.color = new Color(0.18f, 0.12f, 0.10f, 0.95f);
            rowBg.type = Image.Type.Sliced;
            if (pillBg != null) rowBg.sprite = pillBg;

            // Label Text
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(rowObj.transform, false);
            RectTransform lRT = lblObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f);
            lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(16, 0);
            lRT.sizeDelta = new Vector2(280, 36);
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
            tRT.anchoredPosition = new Vector2(-16, 0);
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
                else
                {
                    var statsMenuView = Object.FindObjectOfType<PlayerStatsMenuUIView>(true);
                    if (statsMenuView != null)
                    {
                        var canvas = statsMenuView.GetComponentInParent<Canvas>();
                        if (canvas != null) targetParent = canvas.transform;
                    }
                    else
                    {
                        var anyCanvas = Object.FindObjectOfType<Canvas>(true);
                        if (anyCanvas != null) targetParent = anyCanvas.transform;
                    }
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
                Debug.Log("<color=#00FF88>[SettingsUIGenerator]</color> Đã liên kết thành công Modal_Settings vào Canvas hiện tại!");
            }
        }
    }
}

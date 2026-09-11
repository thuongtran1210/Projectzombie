using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.ResourceDownload;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động tạo và cấu hình Prefab Modal Quản Lý Tải Xuống (Resource Download Modal)
    /// và Prefab Item (ResourcePackageItem_Prefab) theo phong cách Cổ Phong Vọng Xuyên 2.5D.
    /// </summary>
    public static class ResourceDownloadUIGenerator
    {
        private const string SPRITES_PATH = "Assets/Art/UI/VongXuyen/";
        private const string PREFAB_OUTPUT_FOLDER = "Assets/_Prefabs/UI";
        private const string RESOURCES_OUTPUT_FOLDER = "Assets/Resources/UI";

        [MenuItem("Tools/ProjectZombie/UI/Sảnh Chính (Meta Menu)/6. Tạo Modal Tải Tài Nguyên (Resource Download Modal UI)", priority = 16)]
        public static void RebuildResourceDownloadUI()
        {
            GenerateResourceDownloadPrefab();
        }

        [MenuItem("Tools/ProjectZombie/UI/Sảnh Chính (Meta Menu)/Gắn Nút Tải Tài Nguyên Vào Header (Chỉ Làm Đúng 1 Việc)", priority = 17)]
        public static void InjectOnlyHeaderDownloadButton()
        {
            InjectButtonToActiveSceneHeader();
        }

        public static void GenerateResourceDownloadPrefab()
        {
            // 1. Load Assets & Fonts
            Sprite modalWoodFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Frame_Modal_TangBaoCac_9Slice.png");
            Sprite cardTotemBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite badgePill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Settings_Parchment.png");
            if (bannerParchment == null) bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Parchment_Scroll.png");

            Sprite sliderTrack = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Track_9Slice.png");
            Sprite sliderFill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Fill_9Slice.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Nav_Close_X_Wood.png");
            Sprite btnAmber = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Battle_Hex_Amber_Glow.png");
            Sprite btnDark = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 2. Tạo Item Prefab trước (ResourcePackageItem_Prefab)
            GameObject itemPrefabObj = BuildItemPrefab(vietFont, badgePill, sliderTrack, sliderFill, btnAmber, btnDark);
            string itemPrefabPath = $"{PREFAB_OUTPUT_FOLDER}/ResourcePackageItem_Prefab.prefab";
            string itemResourcesPath = $"{RESOURCES_OUTPUT_FOLDER}/ResourcePackageItem_Prefab.prefab";
            
            if (!System.IO.Directory.Exists(PREFAB_OUTPUT_FOLDER)) System.IO.Directory.CreateDirectory(PREFAB_OUTPUT_FOLDER);
            if (!System.IO.Directory.Exists(RESOURCES_OUTPUT_FOLDER)) System.IO.Directory.CreateDirectory(RESOURCES_OUTPUT_FOLDER);

            GameObject savedItemPrefab = PrefabUtility.SaveAsPrefabAsset(itemPrefabObj, itemPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(itemPrefabObj, itemResourcesPath);
            Object.DestroyImmediate(itemPrefabObj);

            // 3. Tạo Modal Root
            GameObject modalRoot = new GameObject("Modal_ResourceDownload", typeof(RectTransform), typeof(CanvasGroup), typeof(ResourceDownloadModalView), typeof(ResourceDownloadModalPresenter));
            RectTransform rootRT = modalRoot.GetComponent<RectTransform>();
            SetStretchAnchor(rootRT);

            // 4. Dark Dim Overlay
            GameObject overlayObj = CreateUIElement("Overlay_Dark", modalRoot.transform);
            SetStretchAnchor(overlayObj.GetComponent<RectTransform>());
            var ovImg = overlayObj.AddComponent<Image>();
            ovImg.color = new Color(0.02f, 0.01f, 0.03f, 0.85f);
            var ovBtn = overlayObj.AddComponent<Button>();

            // 5. Container Frame (660 x 580)
            GameObject frameObj = CreateUIElement("Frame_Modal_Content", modalRoot.transform);
            RectTransform frRT = frameObj.GetComponent<RectTransform>();
            frRT.anchorMin = new Vector2(0.5f, 0.5f);
            frRT.anchorMax = new Vector2(0.5f, 0.5f);
            frRT.pivot = new Vector2(0.5f, 0.5f);
            frRT.anchoredPosition = Vector2.zero;
            frRT.sizeDelta = new Vector2(660, 580);

            // Background Dark Wood
            GameObject bgObj = CreateUIElement("Background_Wood_Dark", frameObj.transform);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = new Vector2(8, 8);
            bgRT.offsetMax = new Vector2(-8, -8);
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.13f, 0.09f, 0.08f, 0.98f);
            if (cardTotemBg != null)
            {
                bgImg.sprite = cardTotemBg;
                bgImg.type = Image.Type.Sliced;
            }

            // Dragon Frame Border
            GameObject borderObj = CreateUIElement("Frame_Border_Dragon", frameObj.transform);
            SetStretchAnchor(borderObj.GetComponent<RectTransform>());
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.color = Color.white;
            borderImg.type = Image.Type.Sliced;
            borderImg.raycastTarget = false;
            if (modalWoodFrame != null) borderImg.sprite = modalWoodFrame;

            // 6. Header Banner
            GameObject bannerObj = CreateUIElement("Banner_Header", frameObj.transform);
            RectTransform bnRT = bannerObj.GetComponent<RectTransform>();
            bnRT.anchorMin = new Vector2(0.5f, 1f);
            bnRT.anchorMax = new Vector2(0.5f, 1f);
            bnRT.pivot = new Vector2(0.5f, 1f);
            bnRT.anchoredPosition = new Vector2(0, -12);
            bnRT.sizeDelta = new Vector2(460, 68);
            var bnImg = bannerObj.AddComponent<Image>();
            bnImg.color = Color.white;
            if (bannerParchment != null)
            {
                bnImg.sprite = bannerParchment;
                bnImg.type = Image.Type.Sliced;
            }

            GameObject titleTextObj = CreateUIElement("Txt_Title", bannerObj.transform);
            SetStretchAnchor(titleTextObj.GetComponent<RectTransform>());
            var titleTMP = CreateTextMeshPro(titleTextObj, vietFont);
            titleTMP.fontSize = 21;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.text = "- QUẢN LÝ DỮ LIỆU TẢI VỀ -";
            titleTMP.color = new Color(0.24f, 0.14f, 0.08f);

            // 7. Sub-Header Summary Storage Bar (Thanh tóm tắt dung lượng)
            GameObject summaryObj = CreateUIElement("Bar_StorageSummary", frameObj.transform);
            RectTransform sumRT = summaryObj.GetComponent<RectTransform>();
            sumRT.anchorMin = new Vector2(0.5f, 1f);
            sumRT.anchorMax = new Vector2(0.5f, 1f);
            sumRT.pivot = new Vector2(0.5f, 1f);
            sumRT.anchoredPosition = new Vector2(0, -84);
            sumRT.sizeDelta = new Vector2(590, 34);

            var sumImg = summaryObj.AddComponent<Image>();
            sumImg.color = new Color(0.08f, 0.06f, 0.05f, 0.85f);
            sumImg.type = Image.Type.Sliced;
            if (badgePill != null) sumImg.sprite = badgePill;

            GameObject sumTextObj = CreateUIElement("Txt_Summary", summaryObj.transform);
            SetStretchAnchor(sumTextObj.GetComponent<RectTransform>());
            sumTextObj.GetComponent<RectTransform>().offsetMin = new Vector2(12, 0);
            sumTextObj.GetComponent<RectTransform>().offsetMax = new Vector2(-12, 0);
            var sumTMP = CreateTextMeshPro(sumTextObj, vietFont);
            sumTMP.fontSize = 13.5f;
            sumTMP.alignment = TextAlignmentOptions.Center;
            sumTMP.text = "Dung lượng đã lưu: <color=#00FF88>0.0 MB</color>  |  Chưa tải: <color=#FFAA00>0.0 MB</color>";
            sumTMP.color = new Color(0.85f, 0.82f, 0.78f);

            // 8. Scroll View danh sách gói
            GameObject scrollObj = CreateUIElement("Scroll_Packages", frameObj.transform);
            RectTransform scRT = scrollObj.GetComponent<RectTransform>();
            scRT.anchorMin = new Vector2(0.5f, 0.5f);
            scRT.anchorMax = new Vector2(0.5f, 0.5f);
            scRT.pivot = new Vector2(0.5f, 0.5f);
            scRT.anchoredPosition = new Vector2(0, -25);
            scRT.sizeDelta = new Vector2(590, 310);

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            GameObject viewportObj = CreateUIElement("Viewport", scrollObj.transform);
            SetStretchAnchor(viewportObj.GetComponent<RectTransform>());
            var vpImg = viewportObj.AddComponent<Image>();
            vpImg.color = Color.white;
            var vpMask = viewportObj.AddComponent<Mask>();
            vpMask.showMaskGraphic = false;

            // Content
            GameObject contentObj = CreateUIElement("Content", viewportObj.transform);
            RectTransform cRT = contentObj.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.anchoredPosition = Vector2.zero;
            cRT.sizeDelta = new Vector2(0, 0);

            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportObj.GetComponent<RectTransform>();
            scrollRect.content = cRT;

            // 9. Bottom Actions: Nút "TẢI TẤT CẢ" & "DỌN DẸP TOÀN BỘ CACHE"
            GameObject bottomBar = CreateUIElement("Bottom_Actions", frameObj.transform);
            RectTransform btmRT = bottomBar.GetComponent<RectTransform>();
            btmRT.anchorMin = new Vector2(0.5f, 0);
            btmRT.anchorMax = new Vector2(0.5f, 0);
            btmRT.pivot = new Vector2(0.5f, 0);
            btmRT.anchoredPosition = new Vector2(0, 20);
            btmRT.sizeDelta = new Vector2(590, 48);

            // Nút Dọn Dẹp Cache (Bên Trái)
            GameObject btnClearObj = CreateUIElement("Btn_ClearAllCache", bottomBar.transform);
            RectTransform clrRT = btnClearObj.GetComponent<RectTransform>();
            clrRT.anchorMin = new Vector2(0, 0.5f);
            clrRT.anchorMax = new Vector2(0, 0.5f);
            clrRT.pivot = new Vector2(0, 0.5f);
            clrRT.anchoredPosition = new Vector2(0, 0);
            clrRT.sizeDelta = new Vector2(240, 46);

            var clrImg = btnClearObj.AddComponent<Image>();
            clrImg.color = Color.white;
            clrImg.type = Image.Type.Sliced;
            if (btnDark != null) clrImg.sprite = btnDark;
            var clearAllBtn = btnClearObj.AddComponent<Button>();

            GameObject clrTxtObj = CreateUIElement("Text", btnClearObj.transform);
            SetStretchAnchor(clrTxtObj.GetComponent<RectTransform>());
            var clrTMP = CreateTextMeshPro(clrTxtObj, vietFont);
            clrTMP.text = "<color=#FF4444>DỌN DẸP CACHE</color>";
            clrTMP.fontSize = 14;
            clrTMP.fontStyle = FontStyles.Bold;
            clrTMP.alignment = TextAlignmentOptions.Center;

            // Nút Tải Tất Cả (Bên Phải)
            GameObject btnDlAllObj = CreateUIElement("Btn_DownloadAll", bottomBar.transform);
            RectTransform dlaRT = btnDlAllObj.GetComponent<RectTransform>();
            dlaRT.anchorMin = new Vector2(1, 0.5f);
            dlaRT.anchorMax = new Vector2(1, 0.5f);
            dlaRT.pivot = new Vector2(1, 0.5f);
            dlaRT.anchoredPosition = new Vector2(0, 0);
            dlaRT.sizeDelta = new Vector2(280, 46);

            var dlaImg = btnDlAllObj.AddComponent<Image>();
            dlaImg.color = Color.white;
            dlaImg.type = Image.Type.Sliced;
            if (btnAmber != null) dlaImg.sprite = btnAmber;
            var downloadAllBtn = btnDlAllObj.AddComponent<Button>();

            GameObject dlaTxtObj = CreateUIElement("Text", btnDlAllObj.transform);
            SetStretchAnchor(dlaTxtObj.GetComponent<RectTransform>());
            var dlaTMP = CreateTextMeshPro(dlaTxtObj, vietFont);
            dlaTMP.text = "TẢI TẤT CẢ";
            dlaTMP.fontSize = 15;
            dlaTMP.fontStyle = FontStyles.Bold;
            dlaTMP.alignment = TextAlignmentOptions.Center;
            dlaTMP.color = Color.white;

            // 10. Close Button (Nút Đóng X)
            GameObject btnCloseObj = CreateUIElement("Btn_Close", frameObj.transform);
            RectTransform bcRT = btnCloseObj.GetComponent<RectTransform>();
            bcRT.anchorMin = new Vector2(1f, 1f);
            bcRT.anchorMax = new Vector2(1f, 1f);
            bcRT.pivot = new Vector2(0.5f, 0.5f);
            bcRT.anchoredPosition = new Vector2(-16, -16);
            bcRT.sizeDelta = new Vector2(46, 46);
            var bcImg = btnCloseObj.AddComponent<Image>();
            bcImg.color = Color.white;
            if (btnCloseX != null) bcImg.sprite = btnCloseX;
            var closeBtn = btnCloseObj.AddComponent<Button>();

            // 11. Wire Properties into ResourceDownloadModalView
            var view = modalRoot.GetComponent<ResourceDownloadModalView>();
            var presenter = modalRoot.GetComponent<ResourceDownloadModalPresenter>();
            var itemPrefabComponent = savedItemPrefab.GetComponent<ResourcePackageItemView>();

            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_titleText").objectReferenceValue = titleTMP;
            soView.FindProperty("_summaryStorageText").objectReferenceValue = sumTMP;
            soView.FindProperty("_downloadAllButton").objectReferenceValue = downloadAllBtn;
            soView.FindProperty("_downloadAllButtonText").objectReferenceValue = dlaTMP;
            soView.FindProperty("_clearAllCacheButton").objectReferenceValue = clearAllBtn;
            soView.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            soView.FindProperty("_itemsContainer").objectReferenceValue = contentObj.transform;
            soView.FindProperty("_itemPrefab").objectReferenceValue = itemPrefabComponent;
            soView.ApplyModifiedProperties();

            SerializedObject soPres = new SerializedObject(presenter);
            soPres.FindProperty("_view").objectReferenceValue = view;
            soPres.ApplyModifiedProperties();

            modalRoot.SetActive(false);

            // 12. Save Prefab
            string modalPrefabPath = $"{PREFAB_OUTPUT_FOLDER}/ResourceDownloadModalUI.prefab";
            string modalResourcesPath = $"{RESOURCES_OUTPUT_FOLDER}/ResourceDownloadModalUI.prefab";

            PrefabUtility.SaveAsPrefabAsset(modalRoot, modalPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(modalRoot, modalResourcesPath);
            Object.DestroyImmediate(modalRoot);

            Debug.Log($"<color=#00FF88>[ResourceDownloadUIGenerator]</color> Đã tạo thành công Prefab Modal Tải Tài Nguyên tại: {modalPrefabPath} và {modalResourcesPath}");

            LinkToMainHubScene(modalPrefabPath);
        }

        private static GameObject BuildItemPrefab(TMP_FontAsset font, Sprite pillBg, Sprite track, Sprite fill, Sprite btnAmber, Sprite btnDark)
        {
            GameObject item = new GameObject("ResourcePackageItem_Prefab", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(ResourcePackageItemView));
            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(580, 92);

            var le = item.GetComponent<LayoutElement>();
            le.minHeight = 92;
            le.preferredHeight = 92;
            le.flexibleWidth = 1;

            var imgBg = item.GetComponent<Image>();
            imgBg.color = new Color(0.20f, 0.14f, 0.11f, 0.95f);
            imgBg.type = Image.Type.Sliced;
            if (pillBg != null) imgBg.sprite = pillBg;

            // Title Line (Co giãn theo chiều ngang)
            GameObject titleObj = CreateUIElement("Txt_Title", item.transform);
            RectTransform tRT = titleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 1);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.pivot = new Vector2(0, 1);
            tRT.offsetMin = new Vector2(16, -34);
            tRT.offsetMax = new Vector2(-170, -10);
            var titleTMP = CreateTextMeshPro(titleObj, font);
            titleTMP.fontSize = 15;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.text = "Tên Gói Tài Nguyên";
            titleTMP.color = new Color(0.98f, 0.90f, 0.75f);
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Description Line (Co giãn tự động)
            GameObject descObj = CreateUIElement("Txt_Description", item.transform);
            RectTransform dRT = descObj.GetComponent<RectTransform>();
            dRT.anchorMin = new Vector2(0, 0);
            dRT.anchorMax = new Vector2(1, 0);
            dRT.pivot = new Vector2(0, 0);
            dRT.offsetMin = new Vector2(16, 10);
            dRT.offsetMax = new Vector2(-170, 52);
            var descTMP = CreateTextMeshPro(descObj, font);
            descTMP.fontSize = 11.5f;
            descTMP.text = "Mô tả chi tiết nội dung gói tài nguyên...";
            descTMP.color = new Color(0.80f, 0.75f, 0.70f);
            descTMP.enableWordWrapping = true;

            // Status Tag (ĐÃ TẢI / CHƯA TẢI)
            GameObject statusObj = CreateUIElement("Txt_Status", item.transform);
            RectTransform stRT = statusObj.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(1, 1);
            stRT.anchorMax = new Vector2(1, 1);
            stRT.pivot = new Vector2(1, 1);
            stRT.anchoredPosition = new Vector2(-16, -10);
            stRT.sizeDelta = new Vector2(145, 24);
            var statusTMP = CreateTextMeshPro(statusObj, font);
            statusTMP.fontSize = 12.5f;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.alignment = TextAlignmentOptions.Right;
            statusTMP.text = "<color=#00FF88>[ĐÃ TẢI]</color>";

            // Button Download (Hiện khi CHƯA TẢI)
            GameObject btnDlObj = CreateUIElement("Btn_Download", item.transform);
            RectTransform bdlRT = btnDlObj.GetComponent<RectTransform>();
            bdlRT.anchorMin = new Vector2(1, 0);
            bdlRT.anchorMax = new Vector2(1, 0);
            bdlRT.pivot = new Vector2(1, 0);
            bdlRT.anchoredPosition = new Vector2(-16, 10);
            bdlRT.sizeDelta = new Vector2(145, 36);
            var dlImg = btnDlObj.AddComponent<Image>();
            dlImg.color = Color.white;
            dlImg.type = Image.Type.Sliced;
            if (btnAmber != null) dlImg.sprite = btnAmber;
            var dlBtn = btnDlObj.AddComponent<Button>();

            GameObject dlTxtObj = CreateUIElement("Text", btnDlObj.transform);
            SetStretchAnchor(dlTxtObj.GetComponent<RectTransform>());
            var dlTMP = CreateTextMeshPro(dlTxtObj, font);
            dlTMP.fontSize = 13;
            dlTMP.fontStyle = FontStyles.Bold;
            dlTMP.alignment = TextAlignmentOptions.Center;
            dlTMP.text = "TẢI VỀ";
            dlTMP.color = Color.white;

            // Button Delete (Hiện khi ĐÃ TẢI - người dùng yêu cầu Chưa tải thì KHÔNG có nút xóa)
            GameObject btnDelObj = CreateUIElement("Btn_Delete", item.transform);
            RectTransform bdelRT = btnDelObj.GetComponent<RectTransform>();
            bdelRT.anchorMin = new Vector2(1, 0);
            bdelRT.anchorMax = new Vector2(1, 0);
            bdelRT.pivot = new Vector2(1, 0);
            bdelRT.anchoredPosition = new Vector2(-16, 10);
            bdelRT.sizeDelta = new Vector2(120, 36);
            var delImg = btnDelObj.AddComponent<Image>();
            delImg.color = Color.white;
            delImg.type = Image.Type.Sliced;
            if (btnDark != null) delImg.sprite = btnDark;
            var delBtn = btnDelObj.AddComponent<Button>();

            GameObject delTxtObj = CreateUIElement("Text", btnDelObj.transform);
            SetStretchAnchor(delTxtObj.GetComponent<RectTransform>());
            var delTMP = CreateTextMeshPro(delTxtObj, font);
            delTMP.fontSize = 13;
            delTMP.fontStyle = FontStyles.Bold;
            delTMP.alignment = TextAlignmentOptions.Center;
            delTMP.text = "<color=#FF5555>XÓA</color>";

            // Progress Slider Bar (Hiện khi ĐANG TẢI)
            GameObject progObj = CreateUIElement("Progress_Download", item.transform);
            RectTransform pRT = progObj.GetComponent<RectTransform>();
            pRT.anchorMin = new Vector2(1, 0);
            pRT.anchorMax = new Vector2(1, 0);
            pRT.pivot = new Vector2(1, 0);
            pRT.anchoredPosition = new Vector2(-16, 12);
            pRT.sizeDelta = new Vector2(145, 22);

            var trackImg = progObj.AddComponent<Image>();
            trackImg.color = Color.white;
            trackImg.type = Image.Type.Sliced;
            if (track != null) trackImg.sprite = track;

            var slider = progObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false;

            GameObject fillArea = CreateUIElement("Fill Area", progObj.transform);
            RectTransform faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = new Vector2(3, 2);
            faRT.offsetMax = new Vector2(-3, -2);

            GameObject fillObj = CreateUIElement("Fill", fillArea.transform);
            SetStretchAnchor(fillObj.GetComponent<RectTransform>());
            var fImg = fillObj.AddComponent<Image>();
            fImg.color = new Color(0f, 1f, 0.5f, 1f);
            fImg.type = Image.Type.Sliced;
            if (fill != null) fImg.sprite = fill;
            slider.fillRect = fillObj.GetComponent<RectTransform>();

            GameObject pTxtObj = CreateUIElement("Txt_Progress", progObj.transform);
            SetStretchAnchor(pTxtObj.GetComponent<RectTransform>());
            var progTMP = CreateTextMeshPro(pTxtObj, font);
            progTMP.fontSize = 11;
            progTMP.fontStyle = FontStyles.Bold;
            progTMP.alignment = TextAlignmentOptions.Center;
            progTMP.text = "0%";
            progTMP.color = Color.white;

            progObj.SetActive(false);

            // Wire to ResourcePackageItemView
            var itemView = item.GetComponent<ResourcePackageItemView>();
            SerializedObject so = new SerializedObject(itemView);
            so.FindProperty("_titleText").objectReferenceValue = titleTMP;
            so.FindProperty("_descText").objectReferenceValue = descTMP;
            so.FindProperty("_statusText").objectReferenceValue = statusTMP;
            so.FindProperty("_downloadButton").objectReferenceValue = dlBtn;
            so.FindProperty("_downloadButtonText").objectReferenceValue = dlTMP;
            so.FindProperty("_deleteButton").objectReferenceValue = delBtn;
            so.FindProperty("_progressBar").objectReferenceValue = slider;
            so.FindProperty("_progressText").objectReferenceValue = progTMP;
            so.ApplyModifiedProperties();

            return item;
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
                var canvas = Object.FindObjectOfType<Canvas>(true);
                if (canvas != null) targetParent = canvas.transform;
            }

            if (targetParent != null)
            {
                Transform existing = targetParent.Find("Modal_ResourceDownload");
                if (existing != null) Object.DestroyImmediate(existing.gameObject);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, targetParent);
                instance.name = "Modal_ResourceDownload";
                instance.SetActive(false);

                var modalView = instance.GetComponent<ResourceDownloadModalView>();
                if (metaManager != null && modalView != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaManager);
                    var prop = soMeta.FindProperty("_resourceDownloadModalScreen");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = modalView;
                        soMeta.ApplyModifiedProperties();
                    }
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log("<color=#00FF88>[ResourceDownloadUIGenerator]</color> Đã liên kết thành công Modal_ResourceDownload vào Canvas Sảnh Chính!");
            }
        }

        public static void InjectButtonToActiveSceneHeader()
        {
            var mainHubView = Object.FindObjectOfType<MainHubView>(true);
            if (mainHubView == null)
            {
                Debug.LogError("<color=#FF4444>[ResourceDownloadUIGenerator]</color> Không tìm thấy MainHubView trong Scene hiện tại!");
                return;
            }

            Transform headerTransform = mainHubView.transform.Find("Header_TopBar");
            if (headerTransform == null)
            {
                // Tìm kiếm sâu trong các cấp con
                foreach (var t in mainHubView.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Header_TopBar")
                    {
                        headerTransform = t;
                        break;
                    }
                }
            }

            if (headerTransform == null)
            {
                Debug.LogError("<color=#FF4444>[ResourceDownloadUIGenerator]</color> Không tìm thấy Header_TopBar bên trong MainHubView!");
                return;
            }

            Transform rightGroup = headerTransform.Find("Right_Currencies");
            if (rightGroup == null)
            {
                foreach (var t in headerTransform.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Right_Currencies")
                    {
                        rightGroup = t;
                        break;
                    }
                }
            }

            if (rightGroup == null)
            {
                Debug.LogError("<color=#FF4444>[ResourceDownloadUIGenerator]</color> Không tìm thấy Right_Currencies trong Header_TopBar!");
                return;
            }

            // Mở rộng width của Right_Currencies để không bị tràn
            var rRT = rightGroup.GetComponent<RectTransform>();
            if (rRT != null && rRT.sizeDelta.x < 500f)
            {
                rRT.sizeDelta = new Vector2(520, rRT.sizeDelta.y);
            }

            Transform existingBtn = rightGroup.Find("Btn_ResourceDownload");
            Button resourceDlBtn = null;

            if (existingBtn != null)
            {
                resourceDlBtn = existingBtn.GetComponent<Button>();
                Debug.Log("<color=#00FF88>[ResourceDownloadUIGenerator]</color> Đã tìm thấy nút Btn_ResourceDownload có sẵn trên Header!");
            }
            else
            {
                Sprite pillWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Pill_Currency_Wood.png");
                Sprite downloadIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Badge_Element_Kim.png");
                if (downloadIconSprite == null) downloadIconSprite = pillWoodSprite;

                GameObject btnDlObj = CreateUIElement("Btn_ResourceDownload", rightGroup);
                btnDlObj.GetComponent<RectTransform>().sizeDelta = new Vector2(44, 44);

                var dlImg = btnDlObj.AddComponent<Image>();
                dlImg.color = Color.white;
                if (pillWoodSprite != null)
                {
                    dlImg.sprite = pillWoodSprite;
                    dlImg.type = Image.Type.Sliced;
                }

                resourceDlBtn = btnDlObj.AddComponent<Button>();
                var btnColors = resourceDlBtn.colors;
                btnColors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
                btnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
                resourceDlBtn.colors = btnColors;

                // Icon Tải Về / Đám Mây bên trong nút
                GameObject iconDlChild = CreateUIElement("Icon", btnDlObj.transform);
                RectTransform idcRT = iconDlChild.GetComponent<RectTransform>();
                idcRT.anchorMin = new Vector2(0.5f, 0.5f);
                idcRT.anchorMax = new Vector2(0.5f, 0.5f);
                idcRT.pivot = new Vector2(0.5f, 0.5f);
                idcRT.sizeDelta = new Vector2(28, 28);
                var idcImg = iconDlChild.AddComponent<Image>();
                idcImg.sprite = downloadIconSprite;
                idcImg.preserveAspect = true;
                idcImg.raycastTarget = false;

                // Đặt nút nằm trước nút Cài Đặt (Btn_Settings)
                Transform settingsBtnTransform = rightGroup.Find("Btn_Settings");
                if (settingsBtnTransform != null)
                {
                    int settingsIndex = settingsBtnTransform.GetSiblingIndex();
                    btnDlObj.transform.SetSiblingIndex(settingsIndex);
                }

                Debug.Log("<color=#00FF88>[ResourceDownloadUIGenerator]</color> Đã tạo mới nút Btn_ResourceDownload vào Header_TopBar thành công!");
            }

            // Gắn reference vào MainHubView mà KHÔNG làm thay đổi bất kỳ thành phần nào khác
            SerializedObject soView = new SerializedObject(mainHubView);
            var prop = soView.FindProperty("_resourceDownloadButton");
            if (prop != null)
            {
                prop.objectReferenceValue = resourceDlBtn;
                soView.ApplyModifiedProperties();
                EditorUtility.SetDirty(mainHubView);
            }

            // Đảm bảo Modal_ResourceDownload cũng đã được liên kết
            var metaManager = Object.FindObjectOfType<MetaUIManager>(true);
            if (metaManager != null)
            {
                var modalView = metaManager.GetComponentInChildren<ResourceDownloadModalView>(true);
                if (modalView == null)
                {
                    string modalPrefabPath = $"{PREFAB_OUTPUT_FOLDER}/ResourceDownloadModalUI.prefab";
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modalPrefabPath);
                    if (prefab != null)
                    {
                        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, metaManager.transform);
                        instance.name = "Modal_ResourceDownload";
                        instance.SetActive(false);
                        modalView = instance.GetComponent<ResourceDownloadModalView>();
                    }
                }

                if (modalView != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaManager);
                    var metaProp = soMeta.FindProperty("_resourceDownloadScreen");
                    if (metaProp != null)
                    {
                        metaProp.objectReferenceValue = modalView;
                        soMeta.ApplyModifiedProperties();
                        EditorUtility.SetDirty(metaManager);
                    }
                }
            }

            // Tự động lưu Prefab Sảnh hiện tại vào _Prefabs/UI và Resources/UI để khi build Android đồng bộ 100%
            string hubPrefabPath = $"{PREFAB_OUTPUT_FOLDER}/MainHubUI.prefab";
            string hubResourcesPath = $"{RESOURCES_OUTPUT_FOLDER}/MainHubUI.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(mainHubView.gameObject, hubPrefabPath, InteractionMode.AutomatedAction);
            if (System.IO.File.Exists(hubPrefabPath))
            {
                System.IO.File.Copy(hubPrefabPath, hubResourcesPath, true);
                AssetDatabase.ImportAsset(hubResourcesPath, ImportAssetOptions.ForceUpdate);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainHubView.gameObject.scene);
            Debug.Log("<color=#00FF88>[ResourceDownloadUIGenerator]</color> HOÀN TẤT: Đã gắn nút vào Header VÀ đồng bộ trực tiếp sang Resources/UI/MainHubUI.prefab cho Android!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TextMeshProUGUI CreateTextMeshPro(GameObject obj, TMP_FontAsset font)
        {
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void SetStretchAnchor(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}

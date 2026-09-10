using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool thiết lập toàn bộ cụm Top HUD Wave (Thẻ tre MiniBadge + Thanh tiến trình quái).
    /// </summary>
    public static class WaveBannerUISetupTool
    {
        [MenuItem("Tools/ProjectZombie/UI/Trong Trận (In-Game Gameplay)/2.2. Thiết Lập Thẻ Tre & Thanh Tiến Trình (Top-Center HUD)", priority = 24)]
        public static void SetupWaveBannerUI()
        {
            // 1. Tìm RunHUD Root
            GameObject hudRoot = GameObject.Find("UI_RunHUDRoot");
            if (hudRoot == null) hudRoot = GameObject.Find("RunHUD_Root");
            if (hudRoot == null)
            {
                var allObjs = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allObjs)
                {
                    if (go.name.Trim() == "UI_RunHUDRoot" || go.name.Trim() == "RunHUD_Root")
                    {
                        hudRoot = go;
                        break;
                    }
                }
            }

            if (hudRoot == null)
            {
                Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
                if (mainCanvas != null)
                {
                    hudRoot = mainCanvas.gameObject;
                }
                else
                {
                    Debug.LogError("[WaveBannerSetupTool] Khong tim thay UI_RunHUDRoot hoac Canvas nao trong Scene!");
                    return;
                }
            }

            Undo.RegisterFullObjectHierarchyUndo(hudRoot, "Setup Wave Banner UI");

            // Load Font & Sprite Cổ Phong Đông Sơn
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Font/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = Resources.Load<TMP_FontAsset>("Fonts/GameFont_Vietnamese_SD");

            Sprite badgePillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite timerBoxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_HUD_Timer_Kill_Wood.png");
            Sprite trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slider_Wood_Track_9Slice.png");
            Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/ExpBar_Fill_Gold.png");
            if (fillSprite == null) fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_EXP.png");
            if (fillSprite == null) fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_HP.png");

            // -----------------------------------------------------------------
            // 2. TÁI SỬ DỤNG HOẶC TẠO NODE ROOT: TopCenter_WaveBannerWidget
            // -----------------------------------------------------------------
            Transform widgetTrans = GetOrCreateChild(hudRoot.transform, "TopCenter_WaveBannerWidget");
            RectTransform wRT = EnsureComponent<RectTransform>(widgetTrans.gameObject);
            wRT.anchorMin = new Vector2(0.5f, 1f);
            wRT.anchorMax = new Vector2(0.5f, 1f);
            wRT.pivot = new Vector2(0.5f, 1f);
            wRT.anchoredPosition = new Vector2(0, -6);
            wRT.sizeDelta = new Vector2(620, 100);

            // Gắn View & Presenter
            var view = EnsureComponent<WaveBannerWidgetView>(widgetTrans.gameObject);
            var presenter = EnsureComponent<WaveBannerWidgetPresenter>(widgetTrans.gameObject);

            // -----------------------------------------------------------------
            // 3. THẺ TRE TIÊU ĐỀ RIÊNG BIỆT (MINI BADGE TOP-CENTER) - GỌN GÀNG, ĐẸP MẮT
            // -----------------------------------------------------------------
            Transform miniBadgeTrans = GetOrCreateChild(widgetTrans, "MiniBadge_Bamboo");
            RectTransform mbRT = EnsureComponent<RectTransform>(miniBadgeTrans.gameObject);
            mbRT.anchorMin = new Vector2(0.5f, 1f);
            mbRT.anchorMax = new Vector2(0.5f, 1f);
            mbRT.pivot = new Vector2(0.5f, 1f);
            mbRT.anchoredPosition = new Vector2(0, 0);
            mbRT.sizeDelta = new Vector2(320, 36);

            Image mbBg = EnsureComponent<Image>(miniBadgeTrans.gameObject);
            mbBg.color = new Color(1f, 1f, 1f, 0.96f);
            mbBg.type = Image.Type.Sliced;
            if (timerBoxSprite != null) mbBg.sprite = timerBoxSprite;
            else if (badgePillSprite != null) mbBg.sprite = badgePillSprite;

            CanvasGroup mbCG = EnsureComponent<CanvasGroup>(miniBadgeTrans.gameObject);

            // 3.1. Hàng Tiêu Đề Màn Chơi & Số Hồi (Row_Header)
            Transform headerRowTrans = GetOrCreateChild(miniBadgeTrans, "Row_Header");
            RectTransform hrRT = EnsureComponent<RectTransform>(headerRowTrans.gameObject);
            hrRT.anchorMin = Vector2.zero;
            hrRT.anchorMax = Vector2.one;
            hrRT.offsetMin = new Vector2(16, 0);
            hrRT.offsetMax = new Vector2(-16, 0);

            // Tái định vị nếu Txt_StageTitle hoặc Txt_WaveIndex cũ đang nằm trực tiếp dưới MiniBadge_Bamboo
            Transform oldStageTxt = miniBadgeTrans.Find("Txt_StageTitle");
            if (oldStageTxt != null && oldStageTxt.parent == miniBadgeTrans) oldStageTxt.SetParent(headerRowTrans, false);
            Transform oldWaveTxt = miniBadgeTrans.Find("Txt_WaveIndex");
            if (oldWaveTxt != null && oldWaveTxt.parent == miniBadgeTrans) oldWaveTxt.SetParent(headerRowTrans, false);

            // 3.1.1. Text Tên Ải (Stage Title)
            Transform stageTxtTrans = GetOrCreateChild(headerRowTrans, "Txt_StageTitle");
            RectTransform stRT = EnsureComponent<RectTransform>(stageTxtTrans.gameObject);
            stRT.anchorMin = new Vector2(0f, 0f);
            stRT.anchorMax = new Vector2(0.60f, 1f);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            TextMeshProUGUI stageTMP = EnsureComponent<TextMeshProUGUI>(stageTxtTrans.gameObject);
            if (vietFont != null) stageTMP.font = vietFont;
            stageTMP.fontSize = 14;
            stageTMP.fontStyle = FontStyles.Bold;
            stageTMP.alignment = TextAlignmentOptions.Left;
            stageTMP.text = "MAN 1: U MINH GIOI";
            stageTMP.color = new Color(1f, 0.86f, 0.45f); // Vàng Hoàng Kim

            // 3.1.2. Text Đợt Quái (Wave Index)
            Transform waveTxtTrans = GetOrCreateChild(headerRowTrans, "Txt_WaveIndex");
            RectTransform wtRT = EnsureComponent<RectTransform>(waveTxtTrans.gameObject);
            wtRT.anchorMin = new Vector2(0.60f, 0f);
            wtRT.anchorMax = new Vector2(1f, 1f);
            wtRT.offsetMin = Vector2.zero;
            wtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI waveTMP = EnsureComponent<TextMeshProUGUI>(waveTxtTrans.gameObject);
            if (vietFont != null) waveTMP.font = vietFont;
            waveTMP.fontSize = 13;
            waveTMP.fontStyle = FontStyles.Bold;
            waveTMP.alignment = TextAlignmentOptions.Right;
            waveTMP.text = "HOI 01 / 10";
            waveTMP.color = new Color(0.96f, 0.96f, 0.96f);

            // -----------------------------------------------------------------
            // 4. THANH TIẾN TRÌNH QUÁI ĐỘC LẬP (TÁCH KHỎI MINIBADGE)
            // -----------------------------------------------------------------
            // Chuyển ProgressBar_StageMonster ra làm con trực tiếp của TopCenter_WaveBannerWidget
            Transform oldProgBarInMini = miniBadgeTrans.Find("ProgressBar_StageMonster");
            Transform progBarTrans = GetOrCreateChild(widgetTrans, "ProgressBar_StageMonster");
            if (oldProgBarInMini != null && oldProgBarInMini != progBarTrans)
            {
                // Di dời con sang node mới
                while (oldProgBarInMini.childCount > 0)
                {
                    oldProgBarInMini.GetChild(0).SetParent(progBarTrans, false);
                }
                Object.DestroyImmediate(oldProgBarInMini.gameObject);
            }

            RectTransform pbRT = EnsureComponent<RectTransform>(progBarTrans.gameObject);
            pbRT.anchorMin = new Vector2(0.5f, 1f);
            pbRT.anchorMax = new Vector2(0.5f, 1f);
            pbRT.pivot = new Vector2(0.5f, 1f);
            pbRT.anchoredPosition = new Vector2(0, -42);
            pbRT.sizeDelta = new Vector2(600, 26);

            Image pbTrack = EnsureComponent<Image>(progBarTrans.gameObject);
            pbTrack.type = Image.Type.Sliced;
            pbTrack.color = new Color(0.12f, 0.09f, 0.08f, 0.96f);
            if (trackSprite != null) pbTrack.sprite = trackSprite;
            else if (badgePillSprite != null) pbTrack.sprite = badgePillSprite;

            // Fill Image (Ruột thanh tiến trình)
            Transform fillTrans = GetOrCreateChild(progBarTrans, "Img_ProgressFill");
            RectTransform fRT = EnsureComponent<RectTransform>(fillTrans.gameObject);
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.offsetMin = new Vector2(3, 3);
            fRT.offsetMax = new Vector2(-3, -3);
            Image fillImg = EnsureComponent<Image>(fillTrans.gameObject);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0.05f;
            fillImg.color = new Color(1f, 0.72f, 0.15f, 1f); // Vàng Cam Lôi Hỏa
            if (fillSprite != null) fillImg.sprite = fillSprite;

            // Boss Marker Icon ở cuối thanh
            Transform bossMarkerTrans = GetOrCreateChild(progBarTrans, "Icon_BossMarker");
            RectTransform bmRT = EnsureComponent<RectTransform>(bossMarkerTrans.gameObject);
            bmRT.anchorMin = new Vector2(1f, 0.5f);
            bmRT.anchorMax = new Vector2(1f, 0.5f);
            bmRT.pivot = new Vector2(0.5f, 0.5f);
            bmRT.anchoredPosition = new Vector2(-6, 0);
            bmRT.sizeDelta = new Vector2(24, 24);
            Image bmImg = EnsureComponent<Image>(bossMarkerTrans.gameObject);
            bmImg.color = new Color(1f, 0.25f, 0.25f, 0.95f); // Đỏ Chu Sa

            // Container chứa các mốc Icon quái Timeline (Container_TimelineMarkers)
            Transform markersContainerTrans = GetOrCreateChild(progBarTrans, "Container_TimelineMarkers");
            RectTransform mcRT = EnsureComponent<RectTransform>(markersContainerTrans.gameObject);
            mcRT.anchorMin = Vector2.zero;
            mcRT.anchorMax = Vector2.one;
            mcRT.offsetMin = new Vector2(16, 0);
            mcRT.offsetMax = new Vector2(-16, 0);

            // -----------------------------------------------------------------
            // 5. HÀNG THÔNG TIN PHỤ DƯỚI THANH (Row_Footer: Mô tả giai đoạn & Thời gian)
            // -----------------------------------------------------------------
            // Chuyển Row_Footer ra làm con trực tiếp của TopCenter_WaveBannerWidget
            Transform oldFooterInMini = miniBadgeTrans.Find("Row_Footer");
            Transform footerRowTrans = GetOrCreateChild(widgetTrans, "Row_Footer");
            if (oldFooterInMini != null && oldFooterInMini != footerRowTrans)
            {
                while (oldFooterInMini.childCount > 0)
                {
                    oldFooterInMini.GetChild(0).SetParent(footerRowTrans, false);
                }
                Object.DestroyImmediate(oldFooterInMini.gameObject);
            }

            RectTransform frRT = EnsureComponent<RectTransform>(footerRowTrans.gameObject);
            frRT.anchorMin = new Vector2(0.5f, 1f);
            frRT.anchorMax = new Vector2(0.5f, 1f);
            frRT.pivot = new Vector2(0.5f, 1f);
            frRT.anchoredPosition = new Vector2(0, -72);
            frRT.sizeDelta = new Vector2(590, 22);

            // Tái định vị nếu Txt_ProgressTime hoặc Txt_WaveDescription cũ đang nằm chỗ khác
            Transform oldTimeTxt = progBarTrans.Find("Txt_ProgressTime");
            if (oldTimeTxt != null) oldTimeTxt.SetParent(footerRowTrans, false);
            Transform oldDescTxt = miniBadgeTrans.Find("Txt_WaveDescription");
            if (oldDescTxt != null && oldDescTxt.parent == miniBadgeTrans) oldDescTxt.SetParent(footerRowTrans, false);

            // 5.1. Text Mô Tả Giai Đoạn Quái (Bên trái Footer)
            Transform waveDescTrans = GetOrCreateChild(footerRowTrans, "Txt_WaveDescription");
            RectTransform wdRT = EnsureComponent<RectTransform>(waveDescTrans.gameObject);
            wdRT.anchorMin = new Vector2(0f, 0f);
            wdRT.anchorMax = new Vector2(0.70f, 1f);
            wdRT.offsetMin = Vector2.zero;
            wdRT.offsetMax = Vector2.zero;
            TextMeshProUGUI wdTMP = EnsureComponent<TextMeshProUGUI>(waveDescTrans.gameObject);
            if (vietFont != null) wdTMP.font = vietFont;
            wdTMP.fontSize = 11;
            wdTMP.fontStyle = FontStyles.Bold;
            wdTMP.alignment = TextAlignmentOptions.Left;
            wdTMP.text = "Giai doan: Khoi dau tran chien";
            wdTMP.color = new Color(0.9f, 0.82f, 0.72f);

            // 5.2. Text Tiến độ thời gian (Bên phải Footer)
            Transform ptTrans = GetOrCreateChild(footerRowTrans, "Txt_ProgressTime");
            RectTransform ptRT = EnsureComponent<RectTransform>(ptTrans.gameObject);
            ptRT.anchorMin = new Vector2(0.70f, 0f);
            ptRT.anchorMax = new Vector2(1f, 1f);
            ptRT.offsetMin = Vector2.zero;
            ptRT.offsetMax = Vector2.zero;
            TextMeshProUGUI ptTMP = EnsureComponent<TextMeshProUGUI>(ptTrans.gameObject);
            if (vietFont != null) ptTMP.font = vietFont;
            ptTMP.fontSize = 11;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.alignment = TextAlignmentOptions.Right;
            ptTMP.text = "00:00 / 20:00";
            ptTMP.color = new Color(1f, 0.88f, 0.5f);

            // -----------------------------------------------------------------
            // 6. TÁCH BIỆT & DỰNG ĐẠI BANNER ĐỘT PHÁ CHUYỂN WAVE TRỰC TIẾP TRÊN CANVAS_GAMEPLAY
            // -----------------------------------------------------------------
            Transform gameplayRoot = hudRoot.transform.parent;
            if (gameplayRoot == null || (!gameplayRoot.name.Contains("Gameplay") && !gameplayRoot.name.Contains("Canvas")))
            {
                gameplayRoot = hudRoot.transform;
            }

            // Nếu banner cũ còn nằm dưới widgetTrans thì chuyển ra gameplayRoot
            Transform oldBannerInWidget = widgetTrans.Find("Center_WaveTransitionBanner");
            Transform bannerTrans = GetOrCreateChild(gameplayRoot, "Center_WaveTransitionBanner");
            if (oldBannerInWidget != null && oldBannerInWidget != bannerTrans)
            {
                Object.DestroyImmediate(oldBannerInWidget.gameObject);
            }

            // Đặt banner nằm ở vị trí render sau cùng (trên cùng các layer HUD khác)
            bannerTrans.SetAsLastSibling();

            RectTransform bnRT = EnsureComponent<RectTransform>(bannerTrans.gameObject);
            bnRT.anchorMin = Vector2.zero;
            bnRT.anchorMax = Vector2.one;
            bnRT.pivot = new Vector2(0.5f, 0.5f);
            bnRT.offsetMin = Vector2.zero;
            bnRT.offsetMax = Vector2.zero;

            CanvasGroup bnCG = EnsureComponent<CanvasGroup>(bannerTrans.gameObject);
            bnCG.alpha = 0f;
            bannerTrans.gameObject.SetActive(false);

            // 6.1. Backdrop mờ nền tối toàn màn hình chiến trường
            Transform dimTrans = GetOrCreateChild(bannerTrans, "Img_DimBackdrop");
            RectTransform dimRT = EnsureComponent<RectTransform>(dimTrans.gameObject);
            dimRT.anchorMin = Vector2.zero;
            dimRT.anchorMax = Vector2.one;
            dimRT.offsetMin = Vector2.zero;
            dimRT.offsetMax = Vector2.zero;
            Image dimImg = EnsureComponent<Image>(dimTrans.gameObject);
            dimImg.color = new Color(0.04f, 0.03f, 0.06f, 0.45f); // Đen khói tối
            dimImg.raycastTarget = false;

            // 6.2. Khung Nội Dung Đại Sớ Trung Tâm (Panel Content Container - KÍCH THƯỚC LỚN HOÀNH TRÁNG)
            Transform containerTrans = GetOrCreateChild(bannerTrans, "Container_EpicPanel");
            RectTransform cRT = EnsureComponent<RectTransform>(containerTrans.gameObject);
            cRT.anchorMin = new Vector2(0.5f, 0.5f);
            cRT.anchorMax = new Vector2(0.5f, 0.5f);
            cRT.pivot = new Vector2(0.5f, 0.5f);
            cRT.anchoredPosition = new Vector2(0, 0); // Đặt chính giữa trọng tâm màn hình
            cRT.sizeDelta = new Vector2(960, 220);    // Panel to hoành tráng

            // Khung Banner Nền Gỗ Mun Viền Đồng Cẩm Lai 9-Slice
            Transform bannerBgTrans = GetOrCreateChild(containerTrans, "Img_BannerBg");
            RectTransform bbgRT = EnsureComponent<RectTransform>(bannerBgTrans.gameObject);
            bbgRT.anchorMin = Vector2.zero;
            bbgRT.anchorMax = Vector2.one;
            bbgRT.sizeDelta = Vector2.zero;
            Image bbgImg = EnsureComponent<Image>(bannerBgTrans.gameObject);
            bbgImg.color = new Color(0.10f, 0.08f, 0.07f, 0.98f); // Gỗ mun sẫm
            bbgImg.type = Image.Type.Sliced;
            if (timerBoxSprite != null) bbgImg.sprite = timerBoxSprite;
            else if (badgePillSprite != null) bbgImg.sprite = badgePillSprite;

            // Icon Huy Hiệu Sự Kiện (Phía trên đỉnh Panel)
            Transform badgeTrans = GetOrCreateChild(containerTrans, "Icon_EventBadge");
            RectTransform bIconRT = EnsureComponent<RectTransform>(badgeTrans.gameObject);
            bIconRT.anchorMin = new Vector2(0.5f, 1f);
            bIconRT.anchorMax = new Vector2(0.5f, 1f);
            bIconRT.pivot = new Vector2(0.5f, 0.5f);
            bIconRT.anchoredPosition = new Vector2(0, -12);
            bIconRT.sizeDelta = new Vector2(56, 56);
            Image bIconImg = EnsureComponent<Image>(badgeTrans.gameObject);
            bIconImg.preserveAspect = true;
            bIconImg.color = new Color(1f, 0.72f, 0.15f);

            // Banner Tag Text (✦ HỒI THỨ 03 / 10 ✦)
            Transform bTagTrans = GetOrCreateChild(containerTrans, "Txt_BannerTag");
            RectTransform btagRT = EnsureComponent<RectTransform>(bTagTrans.gameObject);
            btagRT.anchorMin = new Vector2(0, 0.64f);
            btagRT.anchorMax = new Vector2(1, 0.92f);
            btagRT.offsetMin = new Vector2(30, 0);
            btagRT.offsetMax = new Vector2(-30, 0);
            TextMeshProUGUI bTagTMP = EnsureComponent<TextMeshProUGUI>(bTagTrans.gameObject);
            if (vietFont != null) bTagTMP.font = vietFont;
            bTagTMP.fontSize = 17;
            bTagTMP.fontStyle = FontStyles.Bold;
            bTagTMP.alignment = TextAlignmentOptions.Center;
            bTagTMP.text = "✦ HỒI THỨ 03 / 10 ✦";
            bTagTMP.color = new Color(1f, 0.88f, 0.5f); // Vàng Sớ Kim

            // Banner Main Title Text (TÊN ĐỢT QUÁI LỚN)
            Transform bTitleTrans = GetOrCreateChild(containerTrans, "Txt_BannerTitle");
            RectTransform btRT = EnsureComponent<RectTransform>(bTitleTrans.gameObject);
            btRT.anchorMin = new Vector2(0, 0.32f);
            btRT.anchorMax = new Vector2(1, 0.68f);
            btRT.offsetMin = new Vector2(30, 0);
            btRT.offsetMax = new Vector2(-30, 0);
            TextMeshProUGUI bTitleTMP = EnsureComponent<TextMeshProUGUI>(bTitleTrans.gameObject);
            if (vietFont != null) bTitleTMP.font = vietFont;
            bTitleTMP.fontSize = 34;
            bTitleTMP.fontStyle = FontStyles.Bold;
            bTitleTMP.alignment = TextAlignmentOptions.Center;
            bTitleTMP.text = "BẦY QUỶ XƯƠNG BAO VÂY!";
            bTitleTMP.color = new Color(1f, 0.63f, 0f); // Vàng Hổ Phách

            // Banner Subtitle Text (LỜI SẤM / CẢNH BÁO)
            Transform bSubTrans = GetOrCreateChild(containerTrans, "Txt_BannerSub");
            RectTransform bsRT = EnsureComponent<RectTransform>(bSubTrans.gameObject);
            bsRT.anchorMin = new Vector2(0, 0.08f);
            bsRT.anchorMax = new Vector2(1, 0.34f);
            bsRT.offsetMin = new Vector2(30, 0);
            bsRT.offsetMax = new Vector2(-30, 0);
            TextMeshProUGUI bSubTMP = EnsureComponent<TextMeshProUGUI>(bSubTrans.gameObject);
            if (vietFont != null) bSubTMP.font = vietFont;
            bSubTMP.fontSize = 17;
            bSubTMP.fontStyle = FontStyles.Bold;
            bSubTMP.alignment = TextAlignmentOptions.Center;
            bSubTMP.text = "BẦY QUÁI BỘC PHÁT (BURST WAVE)";
            bSubTMP.color = new Color(0.92f, 0.88f, 0.82f); // Trắng Ngà

            // -----------------------------------------------------------------
            // 7. DỌN DẸP CÁC NODE TRÙNG LẶP NẾU CÓ
            // -----------------------------------------------------------------
            CleanupDuplicateChildren(headerRowTrans);
            CleanupDuplicateChildren(miniBadgeTrans);
            CleanupDuplicateChildren(progBarTrans);
            CleanupDuplicateChildren(containerTrans);
            CleanupDuplicateChildren(bannerTrans);

            // -----------------------------------------------------------------
            // 8. SERIALIZE VIEW & PRESENTER FIELDS
            // -----------------------------------------------------------------
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_miniBadgeCanvasGroup").objectReferenceValue = mbCG;
            soView.FindProperty("_stageTitleText").objectReferenceValue = stageTMP;
            soView.FindProperty("_waveIndexText").objectReferenceValue = waveTMP;

            soView.FindProperty("_stageProgressSlider").objectReferenceValue = null;
            soView.FindProperty("_stageProgressFillImage").objectReferenceValue = fillImg;
            soView.FindProperty("_stageProgressText").objectReferenceValue = ptTMP;
            soView.FindProperty("_waveDescriptionText").objectReferenceValue = wdTMP;
            soView.FindProperty("_bossMarkerIcon").objectReferenceValue = bossMarkerTrans.gameObject;
            soView.FindProperty("_timelineMarkersContainer").objectReferenceValue = mcRT;

            soView.FindProperty("_bannerCanvasGroup").objectReferenceValue = bnCG;
            soView.FindProperty("_bannerContainer").objectReferenceValue = cRT;
            soView.FindProperty("_bannerDimBackdrop").objectReferenceValue = dimImg;
            soView.FindProperty("_bannerFrameImage").objectReferenceValue = bbgImg;
            soView.FindProperty("_bannerIconBadge").objectReferenceValue = bIconImg;
            soView.FindProperty("_bannerTagText").objectReferenceValue = bTagTMP;
            soView.FindProperty("_bannerTitleText").objectReferenceValue = bTitleTMP;
            soView.FindProperty("_bannerSubText").objectReferenceValue = bSubTMP;
            soView.ApplyModifiedProperties();

            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            EditorUtility.SetDirty(widgetTrans.gameObject);
            Debug.Log("<color=#4DEEEA><b>[WaveBannerUISetupTool]</b> Đã nâng cấp thành công Đại Banner Đột Phá chuyển Wave nằm giữa màn hình!</color>");
        }

        private static Transform GetOrCreateChild(Transform parent, string childName)
        {
            Transform found = parent.Find(childName);
            if (found != null) return found;

            GameObject go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        private static void CleanupDuplicateChildren(Transform parent)
        {
            if (parent == null) return;
            var seenNames = new System.Collections.Generic.HashSet<string>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (seenNames.Contains(child.name))
                {
                    // Trùng tên thì hủy bản sao thừa
                    Object.DestroyImmediate(child.gameObject);
                }
                else
                {
                    seenNames.Add(child.name);
                }
            }
        }
    }
}

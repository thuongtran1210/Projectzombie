using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool độc lập tạo và cấu hình Cụm Wave Banner Widget (Thẻ Tre Top-Center, Thanh tiến trình quái giai đoạn & Banner Pop-up chuyển Wave).
    /// ĐẢM BẢO TÁI SỬ DỤNG 100% CÁC GAME OBJECT CŨ, dọn dẹp các node trùng lặp, không sinh thêm đối tượng rác.
    /// HOÀN TOÀN ĐỘC LẬP: Chỉ can thiệp vào node TopCenter_WaveBannerWidget, không làm ảnh hưởng đến các UI khác.
    /// Không chứa emoji trong mã nguồn và giao diện.
    /// </summary>
    public static class WaveBannerUISetupTool
    {
        [MenuItem("Tools/Vong Xuyen/UI/Setup Wave Banner UI (Top Center)", priority = 10)]
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
            // 6. DỰNG BANNER ĐỘT PHÁ CHUYỂN WAVE (CENTER POPUP BANNER)
            // -----------------------------------------------------------------
            Transform bannerTrans = GetOrCreateChild(widgetTrans, "Center_WaveTransitionBanner");
            RectTransform bnRT = EnsureComponent<RectTransform>(bannerTrans.gameObject);
            bnRT.anchorMin = new Vector2(0.5f, 0.5f);
            bnRT.anchorMax = new Vector2(0.5f, 0.5f);
            bnRT.pivot = new Vector2(0.5f, 0.5f);
            bnRT.anchoredPosition = new Vector2(0, 160);
            bnRT.sizeDelta = new Vector2(580, 120);

            CanvasGroup bnCG = EnsureComponent<CanvasGroup>(bannerTrans.gameObject);
            bnCG.alpha = 0f;
            bannerTrans.gameObject.SetActive(false);

            // Khung Banner Background
            Transform bannerBgTrans = GetOrCreateChild(bannerTrans, "Img_BannerBg");
            RectTransform bbgRT = EnsureComponent<RectTransform>(bannerBgTrans.gameObject);
            bbgRT.anchorMin = Vector2.zero;
            bbgRT.anchorMax = Vector2.one;
            bbgRT.sizeDelta = Vector2.zero;
            Image bbgImg = EnsureComponent<Image>(bannerBgTrans.gameObject);
            bbgImg.color = new Color(0.12f, 0.09f, 0.08f, 0.92f); // Gỗ mun sẫm
            bbgImg.type = Image.Type.Sliced;
            if (badgePillSprite != null) bbgImg.sprite = badgePillSprite;
            else if (timerBoxSprite != null) bbgImg.sprite = timerBoxSprite;

            // Banner Title Text
            Transform bTitleTrans = GetOrCreateChild(bannerTrans, "Txt_BannerTitle");
            RectTransform btRT = EnsureComponent<RectTransform>(bTitleTrans.gameObject);
            btRT.anchorMin = new Vector2(0, 0.45f);
            btRT.anchorMax = new Vector2(1, 1);
            btRT.offsetMin = new Vector2(20, 0);
            btRT.offsetMax = new Vector2(-20, -10);
            TextMeshProUGUI bTitleTMP = EnsureComponent<TextMeshProUGUI>(bTitleTrans.gameObject);
            if (vietFont != null) bTitleTMP.font = vietFont;
            bTitleTMP.fontSize = 24;
            bTitleTMP.fontStyle = FontStyles.Bold;
            bTitleTMP.alignment = TextAlignmentOptions.Center;
            bTitleTMP.text = "DOT 03: BAY QUY XUONG BAO VAY!";
            bTitleTMP.color = new Color(1f, 0.63f, 0f); // Vàng Hổ Phách

            // Banner Subtitle Text
            Transform bSubTrans = GetOrCreateChild(bannerTrans, "Txt_BannerSub");
            RectTransform bsRT = EnsureComponent<RectTransform>(bSubTrans.gameObject);
            bsRT.anchorMin = new Vector2(0, 0);
            bsRT.anchorMax = new Vector2(1, 0.45f);
            bsRT.offsetMin = new Vector2(20, 10);
            bsRT.offsetMax = new Vector2(-20, 0);
            TextMeshProUGUI bSubTMP = EnsureComponent<TextMeshProUGUI>(bSubTrans.gameObject);
            if (vietFont != null) bSubTMP.font = vietFont;
            bSubTMP.fontSize = 16;
            bSubTMP.fontStyle = FontStyles.Bold;
            bSubTMP.alignment = TextAlignmentOptions.Center;
            bSubTMP.text = "BAY QUAI BAO VAY (BURST WAVE)";
            bSubTMP.color = new Color(0.3f, 0.93f, 0.92f); // Xanh Phong Lôi

            // -----------------------------------------------------------------
            // 5. DỌN DẸP CÁC NODE TRÙNG LẶP NẾU CÓ
            // -----------------------------------------------------------------
            CleanupDuplicateChildren(headerRowTrans);
            CleanupDuplicateChildren(miniBadgeTrans);
            CleanupDuplicateChildren(progBarTrans);
            CleanupDuplicateChildren(bannerTrans);

            // -----------------------------------------------------------------
            // 6. SERIALIZE VIEW & PRESENTER FIELDS
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
            soView.FindProperty("_bannerContainer").objectReferenceValue = bnRT;
            soView.FindProperty("_bannerFrameImage").objectReferenceValue = bbgImg;
            soView.FindProperty("_bannerTitleText").objectReferenceValue = bTitleTMP;
            soView.FindProperty("_bannerSubText").objectReferenceValue = bSubTMP;
            soView.ApplyModifiedProperties();

            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            EditorUtility.SetDirty(widgetTrans.gameObject);
            Debug.Log("<color=#4DEEEA><b>[WaveBannerUISetupTool]</b> Da tai su dung va cau hinh thanh cong Wave Banner Widget ma khong tao trung lap Game Object!</color>");
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

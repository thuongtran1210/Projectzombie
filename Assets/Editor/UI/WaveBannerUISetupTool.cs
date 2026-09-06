using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool độc lập tạo và cấu hình Cụm Wave Banner Widget (Thẻ Tre Top-Center & Banner Pop-up chuyển Wave).
    /// HOÀN TOÀN ĐỘC LẬP: Chỉ can thiệp vào node TopCenter_WaveBannerWidget, không làm ảnh hưởng đến các UI khác.
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
                    Debug.LogError("[WaveBannerSetupTool] Không tìm thấy UI_RunHUDRoot hoặc Canvas nào trong Scene!");
                    return;
                }
            }

            Undo.RegisterFullObjectHierarchyUndo(hudRoot, "Setup Wave Banner UI");

            // Load Font & Sprite Cổ Phong Đông Sơn
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Font/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = Resources.Load<TMP_FontAsset>("Fonts/GameFont_Vietnamese_SD");

            Sprite badgePillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite timerBoxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_HUD_Timer_Kill_Wood.png");

            // -----------------------------------------------------------------
            // 2. TẠO HOẶC TÌM NODE CON ĐỘC LẬP: TopCenter_WaveBannerWidget
            // -----------------------------------------------------------------
            Transform widgetTrans = hudRoot.transform.Find("TopCenter_WaveBannerWidget");
            if (widgetTrans == null)
            {
                GameObject wObj = new GameObject("TopCenter_WaveBannerWidget", typeof(RectTransform));
                wObj.transform.SetParent(hudRoot.transform, false);
                widgetTrans = wObj.transform;
            }

            RectTransform wRT = widgetTrans.GetComponent<RectTransform>();
            wRT.anchorMin = new Vector2(0.5f, 1f);
            wRT.anchorMax = new Vector2(0.5f, 1f);
            wRT.pivot = new Vector2(0.5f, 1f);
            wRT.anchoredPosition = new Vector2(0, -18);
            wRT.sizeDelta = new Vector2(280, 72);

            // Gắn View & Presenter
            var view = widgetTrans.GetComponent<WaveBannerWidgetView>();
            if (view == null) view = widgetTrans.gameObject.AddComponent<WaveBannerWidgetView>();

            var presenter = widgetTrans.GetComponent<WaveBannerWidgetPresenter>();
            if (presenter == null) presenter = widgetTrans.gameObject.AddComponent<WaveBannerWidgetPresenter>();

            // -----------------------------------------------------------------
            // 3. DỰNG THẺ TRE CỐ ĐỊNH (MINI BADGE TOP-CENTER)
            // -----------------------------------------------------------------
            Transform miniBadgeTrans = widgetTrans.Find("MiniBadge_Bamboo");
            if (miniBadgeTrans == null)
            {
                GameObject mbObj = new GameObject("MiniBadge_Bamboo", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                mbObj.transform.SetParent(widgetTrans, false);
                miniBadgeTrans = mbObj.transform;
            }

            RectTransform mbRT = miniBadgeTrans.GetComponent<RectTransform>();
            mbRT.anchorMin = Vector2.zero;
            mbRT.anchorMax = Vector2.one;
            mbRT.sizeDelta = Vector2.zero;

            Image mbBg = miniBadgeTrans.GetComponent<Image>();
            mbBg.color = new Color(1f, 1f, 1f, 0.95f);
            mbBg.type = Image.Type.Sliced;
            if (timerBoxSprite != null) mbBg.sprite = timerBoxSprite;
            else if (badgePillSprite != null) mbBg.sprite = badgePillSprite;

            CanvasGroup mbCG = miniBadgeTrans.GetComponent<CanvasGroup>();

            // 3.1. Text Tên Ải (Stage Title)
            Transform stageTxtTrans = miniBadgeTrans.Find("Txt_StageTitle");
            if (stageTxtTrans == null)
            {
                GameObject stObj = new GameObject("Txt_StageTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                stObj.transform.SetParent(miniBadgeTrans, false);
                stageTxtTrans = stObj.transform;
            }
            RectTransform stRT = stageTxtTrans.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0, 0.5f);
            stRT.anchorMax = new Vector2(1, 1);
            stRT.offsetMin = new Vector2(12, 0);
            stRT.offsetMax = new Vector2(-12, -6);
            TextMeshProUGUI stageTMP = stageTxtTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) stageTMP.font = vietFont;
            stageTMP.fontSize = 15;
            stageTMP.fontStyle = FontStyles.Bold;
            stageTMP.alignment = TextAlignmentOptions.Center;
            stageTMP.text = "AI 1: U MINH GIOI";
            stageTMP.color = new Color(1f, 0.85f, 0.45f); // Vàng Hoàng Kim

            // 3.2. Text Đợt Quái (Wave Index)
            Transform waveTxtTrans = miniBadgeTrans.Find("Txt_WaveIndex");
            if (waveTxtTrans == null)
            {
                GameObject wtObj = new GameObject("Txt_WaveIndex", typeof(RectTransform), typeof(TextMeshProUGUI));
                wtObj.transform.SetParent(miniBadgeTrans, false);
                waveTxtTrans = wtObj.transform;
            }
            RectTransform wtRT = waveTxtTrans.GetComponent<RectTransform>();
            wtRT.anchorMin = new Vector2(0, 0);
            wtRT.anchorMax = new Vector2(1, 0.5f);
            wtRT.offsetMin = new Vector2(12, 6);
            wtRT.offsetMax = new Vector2(-12, 0);
            TextMeshProUGUI waveTMP = waveTxtTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) waveTMP.font = vietFont;
            waveTMP.fontSize = 13;
            waveTMP.fontStyle = FontStyles.Bold;
            waveTMP.alignment = TextAlignmentOptions.Center;
            waveTMP.text = "DOT 01 / 10";
            waveTMP.color = new Color(0.95f, 0.95f, 0.95f);

            // -----------------------------------------------------------------
            // 4. DỰNG BANNER ĐỘT PHÁ CHUYỂN WAVE (CENTER POPUP BANNER)
            // -----------------------------------------------------------------
            Transform bannerTrans = widgetTrans.Find("Center_WaveTransitionBanner");
            if (bannerTrans == null)
            {
                GameObject bnObj = new GameObject("Center_WaveTransitionBanner", typeof(RectTransform), typeof(CanvasGroup));
                bnObj.transform.SetParent(widgetTrans, false);
                bannerTrans = bnObj.transform;
            }

            RectTransform bnRT = bannerTrans.GetComponent<RectTransform>();
            // Neo ở khoảng giữa màn hình phía trên Player
            bnRT.anchorMin = new Vector2(0.5f, 0.5f);
            bnRT.anchorMax = new Vector2(0.5f, 0.5f);
            bnRT.pivot = new Vector2(0.5f, 0.5f);
            bnRT.anchoredPosition = new Vector2(0, 160);
            bnRT.sizeDelta = new Vector2(580, 120);

            CanvasGroup bnCG = bannerTrans.GetComponent<CanvasGroup>();
            bnCG.alpha = 0f;
            bannerTrans.gameObject.SetActive(false);

            // Khung Banner Background
            Transform bannerBgTrans = bannerTrans.Find("Img_BannerBg");
            if (bannerBgTrans == null)
            {
                GameObject bbgObj = new GameObject("Img_BannerBg", typeof(RectTransform), typeof(Image));
                bbgObj.transform.SetParent(bannerTrans, false);
                bannerBgTrans = bbgObj.transform;
            }
            RectTransform bbgRT = bannerBgTrans.GetComponent<RectTransform>();
            bbgRT.anchorMin = Vector2.zero;
            bbgRT.anchorMax = Vector2.one;
            bbgRT.sizeDelta = Vector2.zero;
            Image bbgImg = bannerBgTrans.GetComponent<Image>();
            bbgImg.color = new Color(0.12f, 0.09f, 0.08f, 0.92f); // Gỗ mun sẫm
            bbgImg.type = Image.Type.Sliced;
            if (badgePillSprite != null) bbgImg.sprite = badgePillSprite;
            else if (timerBoxSprite != null) bbgImg.sprite = timerBoxSprite;

            // Banner Title Text
            Transform bTitleTrans = bannerTrans.Find("Txt_BannerTitle");
            if (bTitleTrans == null)
            {
                GameObject btObj = new GameObject("Txt_BannerTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                btObj.transform.SetParent(bannerTrans, false);
                bTitleTrans = btObj.transform;
            }
            RectTransform btRT = bTitleTrans.GetComponent<RectTransform>();
            btRT.anchorMin = new Vector2(0, 0.45f);
            btRT.anchorMax = new Vector2(1, 1);
            btRT.offsetMin = new Vector2(20, 0);
            btRT.offsetMax = new Vector2(-20, -10);
            TextMeshProUGUI bTitleTMP = bTitleTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) bTitleTMP.font = vietFont;
            bTitleTMP.fontSize = 24;
            bTitleTMP.fontStyle = FontStyles.Bold;
            bTitleTMP.alignment = TextAlignmentOptions.Center;
            bTitleTMP.text = "DOT 03: BAY QUY XUONG BAO VAY!";
            bTitleTMP.color = new Color(1f, 0.63f, 0f); // Vàng Hổ Phách

            // Banner Subtitle Text
            Transform bSubTrans = bannerTrans.Find("Txt_BannerSub");
            if (bSubTrans == null)
            {
                GameObject bsObj = new GameObject("Txt_BannerSub", typeof(RectTransform), typeof(TextMeshProUGUI));
                bsObj.transform.SetParent(bannerTrans, false);
                bSubTrans = bsObj.transform;
            }
            RectTransform bsRT = bSubTrans.GetComponent<RectTransform>();
            bsRT.anchorMin = new Vector2(0, 0);
            bsRT.anchorMax = new Vector2(1, 0.45f);
            bsRT.offsetMin = new Vector2(20, 10);
            bsRT.offsetMax = new Vector2(-20, 0);
            TextMeshProUGUI bSubTMP = bSubTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) bSubTMP.font = vietFont;
            bSubTMP.fontSize = 16;
            bSubTMP.fontStyle = FontStyles.Bold;
            bSubTMP.alignment = TextAlignmentOptions.Center;
            bSubTMP.text = "BAY QUAI BAO VAY (BURST WAVE)";
            bSubTMP.color = new Color(0.3f, 0.93f, 0.92f); // Xanh Phong Lôi

            // -----------------------------------------------------------------
            // 5. SERIALIZE VIEW & PRESENTER FIELDS
            // -----------------------------------------------------------------
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_miniBadgeCanvasGroup").objectReferenceValue = mbCG;
            soView.FindProperty("_stageTitleText").objectReferenceValue = stageTMP;
            soView.FindProperty("_waveIndexText").objectReferenceValue = waveTMP;

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
            Debug.Log("<color=#4DEEEA><b>[WaveBannerUISetupTool]</b> Đã thiết lập thành công Wave Banner Widget tại Top-Center mà không ảnh hưởng UI khác!</color>");
        }
    }
}

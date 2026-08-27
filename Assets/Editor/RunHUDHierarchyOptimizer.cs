using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor tool tự động tối ưu hóa, làm đẹp và gán trọn bộ Sprite Đông Sơn Cổ Phong rực rỡ vào Top Run HUD:
    /// - Khung HUD gỗ giấy dó & Chim Lạc Hoàng Kim
    /// - Tim Ruby đỏ & Thanh Máu phân đoạn
    /// - Huy hiệu EXP & Thanh Kinh Nghiệm Hoàng Kim
    /// - Bát Quái Thái Cực Âm Dương Rực Lửa
    /// </summary>
    public static class RunHUDHierarchyOptimizer
    {
        [MenuItem("Tools/ProjectZombie/UI/⚡ Optimize & Apply Dong Son Run HUD (1-Click)", priority = 2)]
        public static void OptimizeRunHUD()
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
                Debug.LogError("[RunHUDOptimizer] Không tìm thấy UI_RunHUDRoot trong Scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(hudRoot, "Optimize Dong Son Run HUD");

            // Đảm bảo có RunHUDView và RunHUDPresenter
            RunHUDView hudView = hudRoot.GetComponent<RunHUDView>();
            if (hudView == null) hudView = hudRoot.AddComponent<RunHUDView>();

            RunHUDPresenter hudPresenter = hudRoot.GetComponent<RunHUDPresenter>();
            if (hudPresenter == null) hudPresenter = hudRoot.AddComponent<RunHUDPresenter>();

            // Load tất cả Sprite Vọng Xuyên Cổ Phong
            Sprite hpExpFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Frame_VongXuyen_9Slice.png");
            Sprite hpFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_HP.png");
            Sprite expFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_EXP.png");
            Sprite levelOrbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_HUD_Player_Orb_Level.png");
            Sprite yinyangMeterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Panel_YinYang_Meter_HUD.png");
            Sprite timerBoxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_HUD_Timer_Kill_Wood.png");
            Sprite badgePillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Upgrade_Pill_Wood_9Slice.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // -------------------------------------------------------------
            // 2. DỌN SẠCH & DỰNG LẠI TopLeft_PlayerStatus
            // -------------------------------------------------------------
            Transform topLeftTrans = hudRoot.transform.Find("TopLeft_PlayerStatus");
            if (topLeftTrans == null)
            {
                GameObject tlObj = new GameObject("TopLeft_PlayerStatus", typeof(RectTransform));
                tlObj.transform.SetParent(hudRoot.transform, false);
                topLeftTrans = tlObj.transform;
            }

            // Xóa sạch con cũ để tránh đè chữ
            while (topLeftTrans.childCount > 0)
            {
                Object.DestroyImmediate(topLeftTrans.GetChild(0).gameObject);
            }

            // Xóa bỏ component Image nền cũ nếu có
            Image oldTlBg = topLeftTrans.GetComponent<Image>();
            if (oldTlBg != null) Object.DestroyImmediate(oldTlBg);

            RectTransform tlRT = topLeftTrans.GetComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0, 1);
            tlRT.anchorMax = new Vector2(0, 1);
            tlRT.pivot = new Vector2(0, 1);
            tlRT.anchoredPosition = new Vector2(25, -20);
            tlRT.sizeDelta = new Vector2(380, 100);

            // A. Khung Tròn Gỗ Mun Đính 4 Mũi La Bàn Hiển Thị Cấp Độ (Bên Trái)
            GameObject badgeObj = new GameObject("Badge_Level", typeof(RectTransform), typeof(Image));
            badgeObj.transform.SetParent(topLeftTrans, false);
            RectTransform bRT = badgeObj.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(0, 0);
            bRT.sizeDelta = new Vector2(92, 92);
            Image bImg = badgeObj.GetComponent<Image>();
            bImg.color = Color.white;
            if (levelOrbSprite != null) bImg.sprite = levelOrbSprite;
            bImg.preserveAspect = true;

            GameObject lvlTxtObj = new GameObject("Txt_Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            lvlTxtObj.transform.SetParent(badgeObj.transform, false);
            RectTransform ltRT = lvlTxtObj.GetComponent<RectTransform>();
            ltRT.anchorMin = Vector2.zero;
            ltRT.anchorMax = Vector2.one;
            ltRT.offsetMin = Vector2.zero;
            ltRT.offsetMax = Vector2.zero;
            TextMeshProUGUI lvlTMP = lvlTxtObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) lvlTMP.font = vietFont;
            lvlTMP.fontSize = 20;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.alignment = TextAlignmentOptions.Center;
            lvlTMP.text = "Lv.1";
            lvlTMP.color = new Color(0.96f, 0.88f, 0.72f);

            // B. Khung chứa 2 thanh Máu & EXP Vát Đuôi (Bên Phải Badge)
            GameObject barsContainer = new GameObject("Bars_Container", typeof(RectTransform));
            barsContainer.transform.SetParent(topLeftTrans, false);
            RectTransform bcRT = barsContainer.GetComponent<RectTransform>();
            bcRT.anchorMin = new Vector2(0, 0.5f);
            bcRT.anchorMax = new Vector2(0, 0.5f);
            bcRT.pivot = new Vector2(0, 0.5f);
            bcRT.anchoredPosition = new Vector2(86, 0);
            bcRT.sizeDelta = new Vector2(250, 72);

            // B.1. THANH MÁU (HP BAR)
            GameObject hpSliderObj = new GameObject("HP_Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
            hpSliderObj.transform.SetParent(barsContainer.transform, false);
            RectTransform hpRT = hpSliderObj.GetComponent<RectTransform>();
            hpRT.anchorMin = new Vector2(0, 1);
            hpRT.anchorMax = new Vector2(1, 1);
            hpRT.pivot = new Vector2(0.5f, 1);
            hpRT.anchoredPosition = new Vector2(0, 0);
            hpRT.sizeDelta = new Vector2(0, 34);

            Image hpBg = hpSliderObj.GetComponent<Image>();
            hpBg.color = Color.white;
            hpBg.type = Image.Type.Sliced;
            if (hpExpFrameSprite != null) hpBg.sprite = hpExpFrameSprite;

            Slider hpSlider = hpSliderObj.GetComponent<Slider>();
            hpSlider.minValue = 0;
            hpSlider.maxValue = 100;
            hpSlider.value = 100;

            GameObject hpFillArea = new GameObject("Fill Area", typeof(RectTransform));
            hpFillArea.transform.SetParent(hpSliderObj.transform, false);
            RectTransform hfaRT = hpFillArea.GetComponent<RectTransform>();
            hfaRT.anchorMin = Vector2.zero;
            hfaRT.anchorMax = Vector2.one;
            hfaRT.offsetMin = new Vector2(5, 5);
            hfaRT.offsetMax = new Vector2(-12, -5);

            GameObject hpFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            hpFill.transform.SetParent(hpFillArea.transform, false);
            RectTransform hfRT = hpFill.GetComponent<RectTransform>();
            hfRT.anchorMin = Vector2.zero;
            hfRT.anchorMax = Vector2.one;
            hfRT.sizeDelta = Vector2.zero;
            Image hpFillImg = hpFill.GetComponent<Image>();
            hpFillImg.color = Color.white;
            hpFillImg.type = Image.Type.Sliced;
            if (hpFillSprite != null) hpFillImg.sprite = hpFillSprite;
            hpSlider.fillRect = hfRT;

            // Text Máu nằm lọt giữa thanh HP
            GameObject hpTxtObj = new GameObject("Txt_HP", typeof(RectTransform), typeof(TextMeshProUGUI));
            hpTxtObj.transform.SetParent(hpSliderObj.transform, false);
            RectTransform htRT = hpTxtObj.GetComponent<RectTransform>();
            htRT.anchorMin = Vector2.zero;
            htRT.anchorMax = Vector2.one;
            htRT.offsetMin = new Vector2(0, 0);
            htRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI hpTMP = hpTxtObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) hpTMP.font = vietFont;
            hpTMP.fontSize = 14;
            hpTMP.fontStyle = FontStyles.Bold;
            hpTMP.alignment = TextAlignmentOptions.Center;
            hpTMP.text = "100 / 100";
            hpTMP.color = Color.white;

            // B.2. THANH KINH NGHIỆM (EXP BAR)
            GameObject expSliderObj = new GameObject("EXP_Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
            expSliderObj.transform.SetParent(barsContainer.transform, false);
            RectTransform expRT = expSliderObj.GetComponent<RectTransform>();
            expRT.anchorMin = new Vector2(0, 0);
            expRT.anchorMax = new Vector2(1, 0);
            expRT.pivot = new Vector2(0.5f, 0);
            expRT.anchoredPosition = new Vector2(0, 0);
            expRT.sizeDelta = new Vector2(0, 26);

            Image expBg = expSliderObj.GetComponent<Image>();
            expBg.color = Color.white;
            expBg.type = Image.Type.Sliced;
            if (hpExpFrameSprite != null) expBg.sprite = hpExpFrameSprite;

            Slider expSlider = expSliderObj.GetComponent<Slider>();
            expSlider.minValue = 0;
            expSlider.maxValue = 100;
            expSlider.value = 0;

            GameObject expFillArea = new GameObject("Fill Area", typeof(RectTransform));
            expFillArea.transform.SetParent(expSliderObj.transform, false);
            RectTransform efaRT = expFillArea.GetComponent<RectTransform>();
            efaRT.anchorMin = Vector2.zero;
            efaRT.anchorMax = Vector2.one;
            efaRT.offsetMin = new Vector2(5, 4);
            efaRT.offsetMax = new Vector2(-12, -4);

            GameObject expFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            expFill.transform.SetParent(expFillArea.transform, false);
            RectTransform efRT = expFill.GetComponent<RectTransform>();
            efRT.anchorMin = Vector2.zero;
            efRT.anchorMax = Vector2.one;
            efRT.sizeDelta = Vector2.zero;
            Image expFillImg = expFill.GetComponent<Image>();
            expFillImg.color = Color.white;
            expFillImg.type = Image.Type.Sliced;
            if (expFillSprite != null) expFillImg.sprite = expFillSprite;
            expSlider.fillRect = efRT;

            // -------------------------------------------------------------
            // 3. DỰNG LẠI Meter_Taiji_YinYang (CỤM ÂM DƯƠNG TRUNG TÂM)
            // -------------------------------------------------------------
            Transform taijiTrans = hudRoot.transform.Find("Meter_Taiji_YinYang");
            if (taijiTrans == null) taijiTrans = hudRoot.transform.Find("Panel_YinYang");
            if (taijiTrans == null)
            {
                GameObject tObj = new GameObject("Meter_Taiji_YinYang", typeof(RectTransform));
                tObj.transform.SetParent(hudRoot.transform, false);
                taijiTrans = tObj.transform;
            }
            taijiTrans.name = "Meter_Taiji_YinYang";

            while (taijiTrans.childCount > 0)
            {
                Object.DestroyImmediate(taijiTrans.GetChild(0).gameObject);
            }

            RectTransform tjRT = taijiTrans.GetComponent<RectTransform>();
            tjRT.anchorMin = new Vector2(0.5f, 1);
            tjRT.anchorMax = new Vector2(0.5f, 1);
            tjRT.pivot = new Vector2(0.5f, 1);
            tjRT.anchoredPosition = new Vector2(0, -16);
            tjRT.sizeDelta = new Vector2(340, 90);

            Image tjBg = taijiTrans.GetComponent<Image>();
            if (tjBg == null) tjBg = taijiTrans.gameObject.AddComponent<Image>();
            tjBg.color = Color.white;
            tjBg.type = Image.Type.Sliced;
            if (yinyangMeterSprite != null) tjBg.sprite = yinyangMeterSprite;

            // Header Badge Trạng Thái: [ÂM THỊNH]
            GameObject stateBadge = new GameObject("Badge_State", typeof(RectTransform), typeof(Image));
            stateBadge.transform.SetParent(taijiTrans, false);
            RectTransform sbRT = stateBadge.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(0.5f, 1f);
            sbRT.anchorMax = new Vector2(0.5f, 1f);
            sbRT.pivot = new Vector2(0.5f, 1f);
            sbRT.anchoredPosition = new Vector2(35, 6);
            sbRT.sizeDelta = new Vector2(140, 28);
            Image sbImg = stateBadge.GetComponent<Image>();
            sbImg.color = Color.white;
            sbImg.type = Image.Type.Sliced;
            if (badgePillSprite != null) sbImg.sprite = badgePillSprite;

            GameObject tjLblObj = new GameObject("Txt_StateLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            tjLblObj.transform.SetParent(stateBadge.transform, false);
            RectTransform tjlRT = tjLblObj.GetComponent<RectTransform>();
            tjlRT.anchorMin = Vector2.zero;
            tjlRT.anchorMax = Vector2.one;
            tjlRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI tjLblTMP = tjLblObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) tjLblTMP.font = vietFont;
            tjLblTMP.fontSize = 13;
            tjLblTMP.fontStyle = FontStyles.Bold;
            tjLblTMP.alignment = TextAlignmentOptions.Center;
            tjLblTMP.text = "[ÂM THỊNH]";
            tjLblTMP.color = new Color(0.24f, 0.16f, 0.10f);

            // Slider Âm Dương Nằm Giữa Khung (Gradient Fill Bar)
            GameObject tjSliderObj = new GameObject("Slider_YinYang", typeof(RectTransform), typeof(Slider));
            tjSliderObj.transform.SetParent(taijiTrans, false);
            RectTransform tjsRT = tjSliderObj.GetComponent<RectTransform>();
            tjsRT.anchorMin = new Vector2(0.5f, 0.5f);
            tjsRT.anchorMax = new Vector2(0.5f, 0.5f);
            tjsRT.pivot = new Vector2(0.5f, 0.5f);
            tjsRT.anchoredPosition = new Vector2(35, 6);
            tjsRT.sizeDelta = new Vector2(170, 20);

            Slider tjSlider = tjSliderObj.GetComponent<Slider>();
            tjSlider.minValue = 0;
            tjSlider.maxValue = 100;
            tjSlider.value = 50;

            // Fill Area & Fill Image cho Slider_YinYang
            GameObject tjFillArea = new GameObject("Fill Area", typeof(RectTransform));
            tjFillArea.transform.SetParent(tjSliderObj.transform, false);
            RectTransform tjfaRT = tjFillArea.GetComponent<RectTransform>();
            tjfaRT.anchorMin = Vector2.zero;
            tjfaRT.anchorMax = Vector2.one;
            tjfaRT.offsetMin = new Vector2(2, 2);
            tjfaRT.offsetMax = new Vector2(-2, -2);

            GameObject tjFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            tjFill.transform.SetParent(tjFillArea.transform, false);
            RectTransform tjfRT = tjFill.GetComponent<RectTransform>();
            tjfRT.anchorMin = Vector2.zero;
            tjfRT.anchorMax = Vector2.one;
            tjfRT.sizeDelta = Vector2.zero;
            Image tjFillImg = tjFill.GetComponent<Image>();
            tjFillImg.color = new Color(1.0f, 0.84f, 0.0f, 1f); // Hoàng kim Thái Cực
            tjFillImg.type = Image.Type.Sliced;
            Sprite barFill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_HP.png");
            if (barFill != null) tjFillImg.sprite = barFill;
            tjSlider.fillRect = tjfRT;

            // Sub-Title Dưới: Thái Cực Cân Bằng
            GameObject subTitleObj = new GameObject("Txt_BalanceTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subTitleObj.transform.SetParent(taijiTrans, false);
            RectTransform stRT = subTitleObj.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.5f, 0f);
            stRT.anchorMax = new Vector2(0.5f, 0f);
            stRT.pivot = new Vector2(0.5f, 0f);
            stRT.anchoredPosition = new Vector2(35, 6);
            stRT.sizeDelta = new Vector2(180, 24);
            TextMeshProUGUI stTMP = subTitleObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) stTMP.font = vietFont;
            stTMP.fontSize = 15;
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.alignment = TextAlignmentOptions.Center;
            stTMP.text = "Thái Cực Cân Bằng";
            stTMP.color = new Color(0.25f, 0.16f, 0.10f);

            // Gắn CharacterGaugeWidgetView & CharacterGaugeWidgetPresenter
            var gaugeView = taijiTrans.GetComponent<CharacterGaugeWidgetView>();
            if (gaugeView == null) gaugeView = taijiTrans.gameObject.AddComponent<CharacterGaugeWidgetView>();
            var gaugePresenter = taijiTrans.GetComponent<CharacterGaugeWidgetPresenter>();
            if (gaugePresenter == null) gaugePresenter = taijiTrans.gameObject.AddComponent<CharacterGaugeWidgetPresenter>();

            SerializedObject soGauge = new SerializedObject(gaugeView);
            soGauge.FindProperty("_gaugeSlider").objectReferenceValue = tjSlider;
            soGauge.FindProperty("_gaugeFillImage").objectReferenceValue = tjFillImg;
            soGauge.FindProperty("_gaugeTitleText").objectReferenceValue = tjLblTMP;
            soGauge.ApplyModifiedProperties();

            SerializedObject soPresenterGauge = new SerializedObject(gaugePresenter);
            soPresenterGauge.FindProperty("_view").objectReferenceValue = gaugeView;
            soPresenterGauge.ApplyModifiedProperties();

            // Mặc định ẩn widget cho đến khi Player Đạo Sĩ được spawn và kết nối
            gaugeView.SetVisible(false);

            // -------------------------------------------------------------
            // 4. DỰNG LẠI TopRight_RunStats (THỜI GIAN & SỐ DIỆT)
            // -------------------------------------------------------------
            Transform topRightTrans = hudRoot.transform.Find("TopRight_RunStats");
            if (topRightTrans == null) topRightTrans = hudRoot.transform.Find("Panel_TopRight");
            if (topRightTrans == null)
            {
                GameObject trObj = new GameObject("TopRight_RunStats", typeof(RectTransform), typeof(Image));
                trObj.transform.SetParent(hudRoot.transform, false);
                topRightTrans = trObj.transform;
            }
            topRightTrans.name = "TopRight_RunStats";

            while (topRightTrans.childCount > 0)
            {
                Object.DestroyImmediate(topRightTrans.GetChild(0).gameObject);
            }

            RectTransform trRT = topRightTrans.GetComponent<RectTransform>();
            trRT.anchorMin = new Vector2(1, 1);
            trRT.anchorMax = new Vector2(1, 1);
            trRT.pivot = new Vector2(1, 1);
            trRT.anchoredPosition = new Vector2(-25, -20);
            trRT.sizeDelta = new Vector2(200, 96);

            Image trBg = topRightTrans.GetComponent<Image>();
            if (trBg == null) trBg = topRightTrans.gameObject.AddComponent<Image>();
            trBg.color = Color.white;
            trBg.type = Image.Type.Sliced;
            if (timerBoxSprite != null) trBg.sprite = timerBoxSprite;

            // Timer Row
            GameObject tRow = new GameObject("Row_Timer", typeof(RectTransform), typeof(TextMeshProUGUI));
            tRow.transform.SetParent(topRightTrans, false);
            RectTransform trRowRT = tRow.GetComponent<RectTransform>();
            trRowRT.anchorMin = new Vector2(0, 0.5f);
            trRowRT.anchorMax = new Vector2(1, 1);
            trRowRT.offsetMin = new Vector2(16, 0);
            trRowRT.offsetMax = new Vector2(-16, -10);
            TextMeshProUGUI timerTMP = tRow.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) timerTMP.font = vietFont;
            timerTMP.fontSize = 24;
            timerTMP.fontStyle = FontStyles.Bold;
            timerTMP.alignment = TextAlignmentOptions.Center;
            timerTMP.text = "00:00";
            timerTMP.color = new Color(0.98f, 0.88f, 0.60f);

            // Kill Count Row
            GameObject kRow = new GameObject("Row_KillCount", typeof(RectTransform), typeof(TextMeshProUGUI));
            kRow.transform.SetParent(topRightTrans, false);
            RectTransform krRowRT = kRow.GetComponent<RectTransform>();
            krRowRT.anchorMin = new Vector2(0, 0);
            krRowRT.anchorMax = new Vector2(1, 0.5f);
            krRowRT.offsetMin = new Vector2(16, 10);
            krRowRT.offsetMax = new Vector2(-16, 0);
            TextMeshProUGUI killTMP = kRow.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) killTMP.font = vietFont;
            killTMP.fontSize = 16;
            killTMP.fontStyle = FontStyles.Bold;
            killTMP.alignment = TextAlignmentOptions.Center;
            killTMP.text = "Diệt: 0";
            killTMP.color = new Color(0.95f, 0.88f, 0.72f);

            // -------------------------------------------------------------
            // 7. WIRE THÔNG SỐ VÀO RUNHUDVIEW & PRESENTER
            // -------------------------------------------------------------
            SerializedObject soView = new SerializedObject(hudView);
            soView.FindProperty("_hpSlider").objectReferenceValue = hpSlider;
            soView.FindProperty("_hpFillImage").objectReferenceValue = hpFillImg;
            soView.FindProperty("_hpText").objectReferenceValue = hpTMP;
            soView.FindProperty("_expSlider").objectReferenceValue = expSlider;
            soView.FindProperty("_expFillImage").objectReferenceValue = expFillImg;
            soView.FindProperty("_levelText").objectReferenceValue = lvlTMP;
            soView.FindProperty("_timerText").objectReferenceValue = timerTMP;
            soView.FindProperty("_killCountText").objectReferenceValue = killTMP;

            soView.ApplyModifiedProperties();

            SerializedObject soPresenter = new SerializedObject(hudPresenter);
            soPresenter.FindProperty("_view").objectReferenceValue = hudView;
            soPresenter.ApplyModifiedProperties();

            EditorUtility.SetDirty(hudRoot);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(hudRoot.scene);

            Debug.Log("<color=#FFD700>[RunHUDOptimizer] 🚀 ĐÃ HOÀN TẤT NÂNG CẤP TOÀN DIỆN TOP RUN HUD ĐÔNG SƠN RỰC RỠ!</color>");
        }
    }
}

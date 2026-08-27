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

            // Load tất cả Sprite Đông Sơn
            // 1. Nạp bộ Sprite Chibi Casual Arcade mới
            Sprite hpFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Bar_HP_Chunky_Frame.png");
            Sprite hpFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Bar_HP_Chunky_Fill.png");
            Sprite expFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Bar_EXP_Chunky_Frame.png");
            Sprite expFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Bar_EXP_Chunky_Fill.png");
            Sprite levelBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Badge_Level_Chibi_Star.png");
            Sprite taijiOrbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Meter_Taiji_Orb_Chibi.png");
            Sprite yinyangBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Gauge_YinYang_Bar_Chibi.png");
            Sprite runStatsPillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Panel_RunStats_Pill_3D.png");
            Sprite heartRubySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Icon_Heart_Ruby.png");

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
            tlRT.sizeDelta = new Vector2(360, 90);

            // A. Badge Level Tròn (Bên Trái)
            GameObject badgeObj = new GameObject("Badge_Level", typeof(RectTransform), typeof(Image));
            badgeObj.transform.SetParent(topLeftTrans, false);
            RectTransform bRT = badgeObj.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(0, 0);
            bRT.sizeDelta = new Vector2(72, 72);
            Image bImg = badgeObj.GetComponent<Image>();
            bImg.color = Color.white;
            if (levelBadgeSprite != null) bImg.sprite = levelBadgeSprite;
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
            lvlTMP.fontSize = 18;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.alignment = TextAlignmentOptions.Center;
            lvlTMP.text = "Lv.1";
            lvlTMP.color = Color.white;

            // B. Khung chứa 2 thanh Máu & EXP (Bên Phải Badge)
            GameObject barsContainer = new GameObject("Bars_Container", typeof(RectTransform));
            barsContainer.transform.SetParent(topLeftTrans, false);
            RectTransform bcRT = barsContainer.GetComponent<RectTransform>();
            bcRT.anchorMin = new Vector2(0, 0.5f);
            bcRT.anchorMax = new Vector2(0, 0.5f);
            bcRT.pivot = new Vector2(0, 0.5f);
            bcRT.anchoredPosition = new Vector2(78, 0);
            bcRT.sizeDelta = new Vector2(260, 72);

            // B.1. THANH MÁU (HP BAR)
            GameObject hpSliderObj = new GameObject("HP_Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
            hpSliderObj.transform.SetParent(barsContainer.transform, false);
            RectTransform hpRT = hpSliderObj.GetComponent<RectTransform>();
            hpRT.anchorMin = new Vector2(0, 1);
            hpRT.anchorMax = new Vector2(1, 1);
            hpRT.pivot = new Vector2(0.5f, 1);
            hpRT.anchoredPosition = new Vector2(0, 0);
            hpRT.sizeDelta = new Vector2(0, 36);

            Image hpBg = hpSliderObj.GetComponent<Image>();
            hpBg.color = Color.white;
            hpBg.type = Image.Type.Sliced;
            if (hpFrameSprite != null) hpBg.sprite = hpFrameSprite;

            Slider hpSlider = hpSliderObj.GetComponent<Slider>();
            hpSlider.minValue = 0;
            hpSlider.maxValue = 100;
            hpSlider.value = 100;

            GameObject hpFillArea = new GameObject("Fill Area", typeof(RectTransform));
            hpFillArea.transform.SetParent(hpSliderObj.transform, false);
            RectTransform hfaRT = hpFillArea.GetComponent<RectTransform>();
            hfaRT.anchorMin = Vector2.zero;
            hfaRT.anchorMax = Vector2.one;
            hfaRT.offsetMin = new Vector2(4, 4);
            hfaRT.offsetMax = new Vector2(-4, -4);

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
            htRT.offsetMin = Vector2.zero;
            htRT.offsetMax = Vector2.zero;
            TextMeshProUGUI hpTMP = hpTxtObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) hpTMP.font = vietFont;
            hpTMP.fontSize = 15;
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
            if (expFrameSprite != null) expBg.sprite = expFrameSprite;

            Slider expSlider = expSliderObj.GetComponent<Slider>();
            expSlider.minValue = 0;
            expSlider.maxValue = 100;
            expSlider.value = 0;

            GameObject expFillArea = new GameObject("Fill Area", typeof(RectTransform));
            expFillArea.transform.SetParent(expSliderObj.transform, false);
            RectTransform efaRT = expFillArea.GetComponent<RectTransform>();
            efaRT.anchorMin = Vector2.zero;
            efaRT.anchorMax = Vector2.one;
            efaRT.offsetMin = new Vector2(3, 3);
            efaRT.offsetMax = new Vector2(-3, -3);

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

            Image oldTjBg = taijiTrans.GetComponent<Image>();
            if (oldTjBg != null) Object.DestroyImmediate(oldTjBg);

            RectTransform tjRT = taijiTrans.GetComponent<RectTransform>();
            tjRT.anchorMin = new Vector2(0.5f, 1);
            tjRT.anchorMax = new Vector2(0.5f, 1);
            tjRT.pivot = new Vector2(0.5f, 1);
            tjRT.anchoredPosition = new Vector2(0, -18);
            tjRT.sizeDelta = new Vector2(280, 80);

            // A. Ngọc Thái Cực
            GameObject orbObj = new GameObject("Orb_Taiji", typeof(RectTransform), typeof(Image));
            orbObj.transform.SetParent(taijiTrans, false);
            RectTransform orbRT = orbObj.GetComponent<RectTransform>();
            orbRT.anchorMin = new Vector2(0, 0.5f);
            orbRT.anchorMax = new Vector2(0, 0.5f);
            orbRT.pivot = new Vector2(0, 0.5f);
            orbRT.anchoredPosition = new Vector2(0, 0);
            orbRT.sizeDelta = new Vector2(74, 74);
            Image orbImg = orbObj.GetComponent<Image>();
            orbImg.color = Color.white;
            if (taijiOrbSprite != null) orbImg.sprite = taijiOrbSprite;
            orbImg.preserveAspect = true;

            // B. Cụm Thanh Trượt & Tiêu Đề
            GameObject tjRight = new GameObject("Group_BarAndLabel", typeof(RectTransform));
            tjRight.transform.SetParent(taijiTrans, false);
            RectTransform tjrRT = tjRight.GetComponent<RectTransform>();
            tjrRT.anchorMin = new Vector2(0, 0.5f);
            tjrRT.anchorMax = new Vector2(0, 0.5f);
            tjrRT.pivot = new Vector2(0, 0.5f);
            tjrRT.anchoredPosition = new Vector2(80, 0);
            tjrRT.sizeDelta = new Vector2(190, 64);

            // B.1. Slider Âm Dương
            GameObject tjSliderObj = new GameObject("Slider_YinYang", typeof(RectTransform), typeof(Slider), typeof(Image));
            tjSliderObj.transform.SetParent(tjRight.transform, false);
            RectTransform tjsRT = tjSliderObj.GetComponent<RectTransform>();
            tjsRT.anchorMin = new Vector2(0, 1);
            tjsRT.anchorMax = new Vector2(1, 1);
            tjsRT.pivot = new Vector2(0.5f, 1);
            tjsRT.anchoredPosition = new Vector2(0, -4);
            tjsRT.sizeDelta = new Vector2(0, 26);

            Image tjsBg = tjSliderObj.GetComponent<Image>();
            tjsBg.color = Color.white;
            tjsBg.type = Image.Type.Sliced;
            if (yinyangBarSprite != null) tjsBg.sprite = yinyangBarSprite;

            Slider tjSlider = tjSliderObj.GetComponent<Slider>();
            tjSlider.minValue = -100;
            tjSlider.maxValue = 100;
            tjSlider.value = -40;

            // B.2. Label Trạng Thái [ÂM THỊNH] / [DƯƠNG THỊNH]
            GameObject tjLblObj = new GameObject("Txt_StateLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            tjLblObj.transform.SetParent(tjRight.transform, false);
            RectTransform tjlRT = tjLblObj.GetComponent<RectTransform>();
            tjlRT.anchorMin = new Vector2(0, 0);
            tjlRT.anchorMax = new Vector2(1, 0);
            tjlRT.pivot = new Vector2(0.5f, 0);
            tjlRT.anchoredPosition = new Vector2(0, 2);
            tjlRT.sizeDelta = new Vector2(0, 24);

            TextMeshProUGUI tjLblTMP = tjLblObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) tjLblTMP.font = vietFont;
            tjLblTMP.fontSize = 15;
            tjLblTMP.fontStyle = FontStyles.Bold;
            tjLblTMP.alignment = TextAlignmentOptions.Center;
            tjLblTMP.text = "[ÂM THỊNH]";
            tjLblTMP.color = new Color(0.65f, 0.85f, 1f, 1f);

            // Gắn CharacterGaugeWidgetView nếu có
            var gaugeView = taijiTrans.GetComponent<CharacterGaugeWidgetView>();
            if (gaugeView == null) gaugeView = taijiTrans.gameObject.AddComponent<CharacterGaugeWidgetView>();
            SerializedObject soGauge = new SerializedObject(gaugeView);
            soGauge.FindProperty("_gaugeSlider").objectReferenceValue = tjSlider;
            soGauge.FindProperty("_gaugeTitleText").objectReferenceValue = tjLblTMP;
            soGauge.ApplyModifiedProperties();

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
            trRT.sizeDelta = new Vector2(210, 84);

            Image trBg = topRightTrans.GetComponent<Image>();
            if (trBg == null) trBg = topRightTrans.gameObject.AddComponent<Image>();
            trBg.color = Color.white;
            trBg.type = Image.Type.Sliced;
            if (runStatsPillSprite != null) trBg.sprite = runStatsPillSprite;

            // Timer Row
            GameObject tRow = new GameObject("Row_Timer", typeof(RectTransform), typeof(TextMeshProUGUI));
            tRow.transform.SetParent(topRightTrans, false);
            RectTransform trRowRT = tRow.GetComponent<RectTransform>();
            trRowRT.anchorMin = new Vector2(0, 0.5f);
            trRowRT.anchorMax = new Vector2(1, 1);
            trRowRT.offsetMin = new Vector2(16, 0);
            trRowRT.offsetMax = new Vector2(-16, -6);
            TextMeshProUGUI timerTMP = tRow.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) timerTMP.font = vietFont;
            timerTMP.fontSize = 20;
            timerTMP.fontStyle = FontStyles.Bold;
            timerTMP.alignment = TextAlignmentOptions.Center;
            timerTMP.text = "00:00";
            timerTMP.color = Color.white;

            // Kill Count Row
            GameObject kRow = new GameObject("Row_KillCount", typeof(RectTransform), typeof(TextMeshProUGUI));
            kRow.transform.SetParent(topRightTrans, false);
            RectTransform krRowRT = kRow.GetComponent<RectTransform>();
            krRowRT.anchorMin = new Vector2(0, 0);
            krRowRT.anchorMax = new Vector2(1, 0.5f);
            krRowRT.offsetMin = new Vector2(16, 6);
            krRowRT.offsetMax = new Vector2(-16, 0);
            TextMeshProUGUI killTMP = kRow.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) killTMP.font = vietFont;
            killTMP.fontSize = 15;
            killTMP.fontStyle = FontStyles.Bold;
            killTMP.alignment = TextAlignmentOptions.Center;
            killTMP.text = "Diệt: 0";

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

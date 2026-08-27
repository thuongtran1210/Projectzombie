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
            Sprite hudFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/HUD_Frame_DongSon.png");
            Sprite lacBirdSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Ornament_LacBird_Gold.png");
            Sprite heartRubySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Icon_Heart_Ruby.png");
            Sprite hpBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/HealthBar_BG_Frame.png");
            Sprite hpFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/HealthBar_Segment_Full.png");
            Sprite expBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/EXP_Text_Badge.png");
            Sprite expBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/ExpBar_BG_Frame.png");
            Sprite expFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/ExpBar_Fill_Gold.png");
            Sprite taijiSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Meter_Taiji_YinYang_DongSon.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 2. Dọn sạch các GameObject rác/cũ bị trùng lặp bên trong TopLeft_PlayerStatus trước khi build
            Transform topLeftTrans = hudRoot.transform.Find("TopLeft_PlayerStatus");
            if (topLeftTrans == null)
            {
                // Thử tìm con có tên TopLeft_PlayerStatus hoặc tạo mới
                GameObject tlObj = new GameObject("TopLeft_PlayerStatus", typeof(RectTransform), typeof(Image));
                tlObj.transform.SetParent(hudRoot.transform, false);
                topLeftTrans = tlObj.transform;
            }

            // Dọn sạch các text/slider cũ rải rác ngoài root hoặc trong TopLeft
            for (int i = hudRoot.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = hudRoot.transform.GetChild(i);
                if (child.name == "HP_Slider" || child.name == "EXP_Slider" || child.name == "Txt_HP" || child.name == "Txt_Level" || child.name == "HP_Group" || child.name == "EXP_Group")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            RectTransform tlRT = topLeftTrans.GetComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0, 1);
            tlRT.anchorMax = new Vector2(0, 1);
            tlRT.pivot = new Vector2(0, 1);
            tlRT.anchoredPosition = new Vector2(25, -20);
            tlRT.sizeDelta = new Vector2(430, 110);

            // Gán Khung Nền HUD Đông Sơn 9-slice
            Image tlBg = topLeftTrans.GetComponent<Image>();
            if (tlBg == null) tlBg = topLeftTrans.gameObject.AddComponent<Image>();
            tlBg.color = Color.white;
            tlBg.type = Image.Type.Sliced;
            if (hudFrameSprite != null) tlBg.sprite = hudFrameSprite;

            // Chim Lạc Trang Trí Góc
            Transform lacBirdTrans = topLeftTrans.Find("Deco_LacBird");
            if (lacBirdTrans == null)
            {
                GameObject birdObj = new GameObject("Deco_LacBird", typeof(RectTransform), typeof(Image));
                birdObj.transform.SetParent(topLeftTrans, false);
                lacBirdTrans = birdObj.transform;
            }
            RectTransform birdRT = lacBirdTrans.GetComponent<RectTransform>();
            birdRT.anchorMin = new Vector2(0, 1);
            birdRT.anchorMax = new Vector2(0, 1);
            birdRT.pivot = new Vector2(0, 1);
            birdRT.anchoredPosition = new Vector2(-12, 10);
            birdRT.sizeDelta = new Vector2(70, 70);
            Image birdImg = lacBirdTrans.GetComponent<Image>();
            birdImg.color = Color.white;
            if (lacBirdSprite != null) birdImg.sprite = lacBirdSprite;
            birdImg.preserveAspect = true;

            // -------------------------------------------------------------
            // 3. DỰNG THANH HP (MÁU ĐỎ CHU SA)
            // -------------------------------------------------------------
            Transform hpGroupTrans = topLeftTrans.Find("HP_Group");
            if (hpGroupTrans == null)
            {
                GameObject hpg = new GameObject("HP_Group", typeof(RectTransform));
                hpg.transform.SetParent(topLeftTrans, false);
                hpGroupTrans = hpg.transform;
            }
            RectTransform hpgRT = hpGroupTrans.GetComponent<RectTransform>();
            hpgRT.anchorMin = new Vector2(0, 1);
            hpgRT.anchorMax = new Vector2(0, 1);
            hpgRT.pivot = new Vector2(0, 1);
            hpgRT.anchoredPosition = new Vector2(60, -22);
            hpgRT.sizeDelta = new Vector2(350, 36);

            // Icon Tim Ruby
            Transform heartTrans = hpGroupTrans.Find("Icon_Heart");
            if (heartTrans == null)
            {
                GameObject heartObj = new GameObject("Icon_Heart", typeof(RectTransform), typeof(Image));
                heartObj.transform.SetParent(hpGroupTrans, false);
                heartTrans = heartObj.transform;
            }
            RectTransform heartRT = heartTrans.GetComponent<RectTransform>();
            heartRT.anchorMin = new Vector2(0, 0.5f);
            heartRT.anchorMax = new Vector2(0, 0.5f);
            heartRT.pivot = new Vector2(0, 0.5f);
            heartRT.anchoredPosition = new Vector2(0, 0);
            heartRT.sizeDelta = new Vector2(28, 28);
            Image heartImg = heartTrans.GetComponent<Image>();
            heartImg.color = Color.white;
            if (heartRubySprite != null) heartImg.sprite = heartRubySprite;
            heartImg.preserveAspect = true;

            // Slider HP
            Transform hpSliderTrans = hpGroupTrans.Find("HP_Slider");
            Slider hpSlider = null;
            Image hpFillImg = null;
            if (hpSliderTrans == null)
            {
                GameObject sliderObj = new GameObject("HP_Slider", typeof(RectTransform), typeof(Slider));
                sliderObj.transform.SetParent(hpGroupTrans, false);
                hpSliderTrans = sliderObj.transform;
            }

            RectTransform hpSliderRT = hpSliderTrans.GetComponent<RectTransform>();
            hpSliderRT.anchorMin = new Vector2(0, 0.5f);
            hpSliderRT.anchorMax = new Vector2(0, 0.5f);
            hpSliderRT.pivot = new Vector2(0, 0.5f);
            hpSliderRT.anchoredPosition = new Vector2(34, 0);
            hpSliderRT.sizeDelta = new Vector2(200, 22);

            hpSlider = hpSliderTrans.GetComponent<Slider>();
            if (hpSlider == null) hpSlider = hpSliderTrans.gameObject.AddComponent<Slider>();

            // Nền Background của HP Bar
            Image hpBgImg = hpSliderTrans.GetComponent<Image>();
            if (hpBgImg == null) hpBgImg = hpSliderTrans.gameObject.AddComponent<Image>();
            hpBgImg.color = Color.white;
            hpBgImg.type = Image.Type.Sliced;
            if (hpBgSprite != null) hpBgImg.sprite = hpBgSprite;

            // Fill Area & Fill Image
            Transform hpFillArea = hpSliderTrans.Find("Fill Area");
            if (hpFillArea == null)
            {
                GameObject fa = new GameObject("Fill Area", typeof(RectTransform));
                fa.transform.SetParent(hpSliderTrans, false);
                hpFillArea = fa.transform;
            }
            RectTransform faRT = hpFillArea.GetComponent<RectTransform>();
            faRT.anchorMin = Vector2.zero;
            faRT.anchorMax = Vector2.one;
            faRT.offsetMin = new Vector2(4, 3);
            faRT.offsetMax = new Vector2(-4, -3);

            Transform hpFillObj = hpFillArea.Find("Fill");
            if (hpFillObj == null)
            {
                GameObject f = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                f.transform.SetParent(hpFillArea, false);
                hpFillObj = f.transform;
            }
            RectTransform fillRT = hpFillObj.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            hpFillImg = hpFillObj.GetComponent<Image>();
            hpFillImg.color = Color.white;
            hpFillImg.type = Image.Type.Sliced;
            if (hpFillSprite != null) hpFillImg.sprite = hpFillSprite;

            hpSlider.fillRect = fillRT;
            hpSlider.targetGraphic = null;
            Transform handle = hpSliderTrans.Find("Handle Slide Area");
            if (handle != null) Object.DestroyImmediate(handle.gameObject);

            // Xóa bất kỳ text nào cũ trùng lặp trong hpGroup
            for (int i = hpGroupTrans.childCount - 1; i >= 0; i--)
            {
                var c = hpGroupTrans.GetChild(i);
                if (c.name.StartsWith("Txt_HP") && c.name != "Txt_HP") Object.DestroyImmediate(c.gameObject);
            }

            // Text Máu (vd: 100 / 100)
            Transform hpTextTrans = hpGroupTrans.Find("Txt_HP");
            if (hpTextTrans == null)
            {
                GameObject txtObj = new GameObject("Txt_HP", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(hpGroupTrans, false);
                hpTextTrans = txtObj.transform;
            }
            RectTransform hpTxtRT = hpTextTrans.GetComponent<RectTransform>();
            hpTxtRT.anchorMin = new Vector2(0, 0.5f);
            hpTxtRT.anchorMax = new Vector2(0, 0.5f);
            hpTxtRT.pivot = new Vector2(0, 0.5f);
            hpTxtRT.anchoredPosition = new Vector2(242, 0);
            hpTxtRT.sizeDelta = new Vector2(100, 24);

            TextMeshProUGUI hpTMP = hpTextTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) hpTMP.font = vietFont;
            hpTMP.fontSize = 15;
            hpTMP.fontStyle = FontStyles.Bold;
            hpTMP.alignment = TextAlignmentOptions.Left;
            hpTMP.text = "100/100";
            hpTMP.color = new Color(1f, 0.92f, 0.75f, 1f);

            // -------------------------------------------------------------
            // 4. DỰNG THANH EXP (KINH NGHIỆM VÀNG HOÀNG KIM)
            // -------------------------------------------------------------
            Transform expGroupTrans = topLeftTrans.Find("EXP_Group");
            if (expGroupTrans == null)
            {
                GameObject eg = new GameObject("EXP_Group", typeof(RectTransform));
                eg.transform.SetParent(topLeftTrans, false);
                expGroupTrans = eg.transform;
            }
            RectTransform egRT = expGroupTrans.GetComponent<RectTransform>();
            egRT.anchorMin = new Vector2(0, 1);
            egRT.anchorMax = new Vector2(0, 1);
            egRT.pivot = new Vector2(0, 1);
            egRT.anchoredPosition = new Vector2(65, -60);
            egRT.sizeDelta = new Vector2(370, 32);

            // Huy hiệu EXP Text Badge
            Transform expBadgeTrans = expGroupTrans.Find("Badge_EXP");
            if (expBadgeTrans == null)
            {
                GameObject bObj = new GameObject("Badge_EXP", typeof(RectTransform), typeof(Image));
                bObj.transform.SetParent(expGroupTrans, false);
                expBadgeTrans = bObj.transform;
            }
            RectTransform expBadgeRT = expBadgeTrans.GetComponent<RectTransform>();
            expBadgeRT.anchorMin = new Vector2(0, 0.5f);
            expBadgeRT.anchorMax = new Vector2(0, 0.5f);
            expBadgeRT.pivot = new Vector2(0, 0.5f);
            expBadgeRT.anchoredPosition = new Vector2(0, 0);
            expBadgeRT.sizeDelta = new Vector2(34, 20);
            Image expBadgeImg = expBadgeTrans.GetComponent<Image>();
            expBadgeImg.color = Color.white;
            if (expBadgeSprite != null) expBadgeImg.sprite = expBadgeSprite;
            expBadgeImg.preserveAspect = true;

            // Slider EXP
            Transform expSliderTrans = expGroupTrans.Find("EXP_Slider");
            Slider expSlider = null;
            Image expFillImg = null;
            if (expSliderTrans == null)
            {
                GameObject sliderObj = new GameObject("EXP_Slider", typeof(RectTransform), typeof(Slider));
                sliderObj.transform.SetParent(expGroupTrans, false);
                expSliderTrans = sliderObj.transform;
            }

            RectTransform expSliderRT = expSliderTrans.GetComponent<RectTransform>();
            expSliderRT.anchorMin = new Vector2(0, 0.5f);
            expSliderRT.anchorMax = new Vector2(0, 0.5f);
            expSliderRT.pivot = new Vector2(0, 0.5f);
            expSliderRT.anchoredPosition = new Vector2(38, 0);
            expSliderRT.sizeDelta = new Vector2(210, 18);

            expSlider = expSliderTrans.GetComponent<Slider>();
            if (expSlider == null) expSlider = expSliderTrans.gameObject.AddComponent<Slider>();

            Image expBgImg = expSliderTrans.GetComponent<Image>();
            if (expBgImg == null) expBgImg = expSliderTrans.gameObject.AddComponent<Image>();
            expBgImg.color = Color.white;
            expBgImg.type = Image.Type.Sliced;
            if (expBgSprite != null) expBgImg.sprite = expBgSprite;

            Transform expFillArea = expSliderTrans.Find("Fill Area");
            if (expFillArea == null)
            {
                GameObject fa = new GameObject("Fill Area", typeof(RectTransform));
                fa.transform.SetParent(expSliderTrans, false);
                expFillArea = fa.transform;
            }
            RectTransform efaRT = expFillArea.GetComponent<RectTransform>();
            efaRT.anchorMin = Vector2.zero;
            efaRT.anchorMax = Vector2.one;
            efaRT.offsetMin = new Vector2(4, 2);
            efaRT.offsetMax = new Vector2(-4, -2);

            Transform expFillObj = expFillArea.Find("Fill");
            if (expFillObj == null)
            {
                GameObject f = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                f.transform.SetParent(expFillArea, false);
                expFillObj = f.transform;
            }
            RectTransform efillRT = expFillObj.GetComponent<RectTransform>();
            efillRT.anchorMin = Vector2.zero;
            efillRT.anchorMax = Vector2.one;
            efillRT.sizeDelta = Vector2.zero;

            expFillImg = expFillObj.GetComponent<Image>();
            expFillImg.color = Color.white;
            expFillImg.type = Image.Type.Sliced;
            if (expFillSprite != null) expFillImg.sprite = expFillSprite;

            expSlider.fillRect = efillRT;
            expSlider.targetGraphic = null;
            Transform expHandle = expSliderTrans.Find("Handle Slide Area");
            if (expHandle != null) Object.DestroyImmediate(expHandle.gameObject);

            // Xóa bất kỳ text nào cũ trùng lặp trong expGroup
            for (int i = expGroupTrans.childCount - 1; i >= 0; i--)
            {
                var c = expGroupTrans.GetChild(i);
                if (c.name.StartsWith("Txt_Level") && c.name != "Txt_Level") Object.DestroyImmediate(c.gameObject);
            }

            // Text Cấp Độ (vd: Lv.1)
            Transform lvlTextTrans = expGroupTrans.Find("Txt_Level");
            if (lvlTextTrans == null)
            {
                GameObject txtObj = new GameObject("Txt_Level", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(expGroupTrans, false);
                lvlTextTrans = txtObj.transform;
            }
            RectTransform lvlTxtRT = lvlTextTrans.GetComponent<RectTransform>();
            lvlTxtRT.anchorMin = new Vector2(0, 0.5f);
            lvlTxtRT.anchorMax = new Vector2(0, 0.5f);
            lvlTxtRT.pivot = new Vector2(0, 0.5f);
            lvlTxtRT.anchoredPosition = new Vector2(242, 0);
            lvlTxtRT.sizeDelta = new Vector2(80, 24);

            TextMeshProUGUI lvlTMP = lvlTextTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) lvlTMP.font = vietFont;
            lvlTMP.fontSize = 15;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.alignment = TextAlignmentOptions.Left;
            lvlTMP.text = "Lv.1";
            lvlTMP.color = new Color(1f, 0.85f, 0.35f, 1f);

            // -------------------------------------------------------------
            // 5. CỤM TRẠNG THÁI ÂM DƯƠNG (TAIJI METER)
            // -------------------------------------------------------------
            Transform taijiTrans = hudRoot.transform.Find("Meter_Taiji_YinYang");
            if (taijiTrans == null)
            {
                GameObject tObj = new GameObject("Meter_Taiji_YinYang", typeof(RectTransform), typeof(Image));
                tObj.transform.SetParent(hudRoot.transform, false);
                taijiTrans = tObj.transform;
            }
            RectTransform tjRT = taijiTrans.GetComponent<RectTransform>();
            tjRT.anchorMin = new Vector2(0, 1);
            tjRT.anchorMax = new Vector2(0, 1);
            tjRT.pivot = new Vector2(0, 1);
            tjRT.anchoredPosition = new Vector2(465, -20);
            tjRT.sizeDelta = new Vector2(105, 105);
            Image tjImg = taijiTrans.GetComponent<Image>();
            tjImg.color = Color.white;
            if (taijiSprite != null) tjImg.sprite = taijiSprite;
            tjImg.preserveAspect = true;

            // -------------------------------------------------------------
            // 6. CỤM THỜI GIAN & SỐ DIỆT (TOP RIGHT RUN STATS ĐỒNG BỘ ĐÔNG SƠN)
            // -------------------------------------------------------------
            Transform topRightTrans = hudRoot.transform.Find("TopRight_RunStats");
            if (topRightTrans == null)
            {
                GameObject trObj = new GameObject("TopRight_RunStats", typeof(RectTransform), typeof(Image));
                trObj.transform.SetParent(hudRoot.transform, false);
                topRightTrans = trObj.transform;
            }

            RectTransform trRT = topRightTrans.GetComponent<RectTransform>();
            trRT.anchorMin = new Vector2(1, 1);
            trRT.anchorMax = new Vector2(1, 1);
            trRT.pivot = new Vector2(1, 1);
            trRT.anchoredPosition = new Vector2(-25, -20);
            trRT.sizeDelta = new Vector2(230, 95);

            Image trBg = topRightTrans.GetComponent<Image>();
            if (trBg == null) trBg = topRightTrans.gameObject.AddComponent<Image>();
            trBg.color = Color.white;
            trBg.type = Image.Type.Sliced;
            trBg.pixelsPerUnitMultiplier = 1f;
            if (hudFrameSprite != null) trBg.sprite = hudFrameSprite;

            // Text Timer
            Transform timerTrans = topRightTrans.Find("Txt_Timer");
            if (timerTrans == null)
            {
                // Thử tìm ngoài root cũ nếu có
                Transform oldTimer = hudRoot.transform.Find("Txt_Timer");
                if (oldTimer != null)
                {
                    oldTimer.SetParent(topRightTrans, false);
                    timerTrans = oldTimer;
                }
                else
                {
                    GameObject tObj = new GameObject("Txt_Timer", typeof(RectTransform), typeof(TextMeshProUGUI));
                    tObj.transform.SetParent(topRightTrans, false);
                    timerTrans = tObj.transform;
                }
            }
            RectTransform timerRT = timerTrans.GetComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(0.5f, 1);
            timerRT.anchorMax = new Vector2(0.5f, 1);
            timerRT.pivot = new Vector2(0.5f, 1);
            timerRT.anchoredPosition = new Vector2(0, -14);
            timerRT.sizeDelta = new Vector2(190, 32);

            TextMeshProUGUI timerTMP = timerTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) timerTMP.font = vietFont;
            timerTMP.fontSize = 22;
            timerTMP.fontStyle = FontStyles.Bold;
            timerTMP.alignment = TextAlignmentOptions.Center;
            timerTMP.color = new Color(1f, 0.95f, 0.8f, 1f);
            timerTMP.text = "00:00";

            // Text Kill Count
            Transform killTrans = topRightTrans.Find("Txt_KillCount");
            if (killTrans == null)
            {
                Transform oldKill = hudRoot.transform.Find("Txt_KillCount");
                if (oldKill != null)
                {
                    oldKill.SetParent(topRightTrans, false);
                    killTrans = oldKill;
                }
                else
                {
                    GameObject kObj = new GameObject("Txt_KillCount", typeof(RectTransform), typeof(TextMeshProUGUI));
                    kObj.transform.SetParent(topRightTrans, false);
                    killTrans = kObj.transform;
                }
            }
            RectTransform killRT = killTrans.GetComponent<RectTransform>();
            killRT.anchorMin = new Vector2(0.5f, 0);
            killRT.anchorMax = new Vector2(0.5f, 0);
            killRT.pivot = new Vector2(0.5f, 0);
            killRT.anchoredPosition = new Vector2(0, 14);
            killRT.sizeDelta = new Vector2(190, 26);

            TextMeshProUGUI killTMP = killTrans.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) killTMP.font = vietFont;
            killTMP.fontSize = 16;
            killTMP.fontStyle = FontStyles.Bold;
            killTMP.alignment = TextAlignmentOptions.Center;
            killTMP.color = new Color(1f, 0.75f, 0.35f, 1f);
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

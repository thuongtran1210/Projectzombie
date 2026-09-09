#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động tạo Prefab giao diện Bách Bảo Các (Thần Thẻ, Luyện Khí & Công Thức Codex UI) chuẩn Cổ Phong Đông Sơn.
    /// Menu: Tools -> ProjectZombie -> UI -> Sảnh Chính (Meta Menu) -> 5. Tạo Thư Viện Thần Thẻ (Card Codex UI)
    /// </summary>
    public static class CardCodexUIGenerator
    {
        private static readonly Color ColorBgOverlay = new Color(0.04f, 0.03f, 0.06f, 0.90f);
        private static readonly Color ColorBambooFrame = new Color(0.42f, 0.28f, 0.16f, 1f);

        [MenuItem("Tools/ProjectZombie/UI/Sảnh Chính (Meta Menu)/5. Tạo Thư Viện Thần Thẻ (Card Codex UI)", priority = 15)]
        public static GameObject GenerateCardCodexPrefab()
        {
            string prefabFolder = "Assets/_Prefabs/UI";
            if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (font == null) font = TMP_Settings.defaultFontAsset;

            Sprite modalFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Modal_TangBaoCac_9Slice.png");
            Sprite headerBar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Header_Wood_Bar_VongXuyen.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Close_X_Wood.png");
            Sprite tabWoodActive = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Active.png");
            Sprite tabWoodInactive = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Inactive.png");
            Sprite cardTotem = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite cardDetailParchment = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");
            Sprite btnAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");
            Sprite slotWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            Sprite slotSelectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
            Sprite currencyPill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pill_Currency_Wood.png");
            Sprite coTienIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Icon_CoTien_VongXuyen.png");

            // 1. Root GameObject
            GameObject root = new GameObject("Panel_CardCodex", typeof(RectTransform), typeof(CanvasGroup), typeof(CardCodexView), typeof(CardCodexPresenter));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            SetStretchAnchor(rootRT);

            var view = root.GetComponent<CardCodexView>();
            var presenter = root.GetComponent<CardCodexPresenter>();

            // 2. Dim Background
            GameObject dimObj = CreateUIElement("Dim_Background", root.transform);
            SetStretchAnchor(dimObj.GetComponent<RectTransform>());
            var dimImg = dimObj.AddComponent<Image>();
            dimImg.color = ColorBgOverlay;
            var dimBtn = dimObj.AddComponent<Button>();

            // 3. Modal Container (1760 x 960) Chuẩn Mobile Landscape
            GameObject modalObj = CreateUIElement("Modal_Container", root.transform);
            RectTransform modalRT = modalObj.GetComponent<RectTransform>();
            modalRT.anchorMin = new Vector2(0.5f, 0.5f);
            modalRT.anchorMax = new Vector2(0.5f, 0.5f);
            modalRT.pivot = new Vector2(0.5f, 0.5f);
            modalRT.sizeDelta = new Vector2(1760, 960);
            var mImg = modalObj.AddComponent<Image>();
            mImg.type = Image.Type.Sliced;
            if (modalFrame != null) mImg.sprite = modalFrame;

            // 4. Header (Top 90px)
            GameObject headerObj = CreateUIElement("Header_Top", modalObj.transform);
            RectTransform hRT = headerObj.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 1);
            hRT.anchorMax = new Vector2(1, 1);
            hRT.pivot = new Vector2(0.5f, 1);
            hRT.anchoredPosition = new Vector2(0, -18);
            hRT.sizeDelta = new Vector2(-48, 80);
            var hImg = headerObj.AddComponent<Image>();
            hImg.type = Image.Type.Sliced;
            if (headerBar != null) hImg.sprite = headerBar;

            // Header Title
            GameObject titleObj = CreateUIElement("Txt_Title", headerObj.transform);
            RectTransform tRT = titleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 0);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.offsetMin = new Vector2(32, 0);
            tRT.offsetMax = new Vector2(-320, 0);
            var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleTMP.font = font;
            titleTMP.text = "THẦN THẺ • LÒ LUYỆN KHÍ & BÁCH BẢO CÁC";
            titleTMP.fontSize = 24;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = new Color(0.98f, 0.88f, 0.50f, 1f);
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Nút Đóng (X)
            GameObject closeObj = CreateUIElement("Btn_Close", headerObj.transform);
            RectTransform cRT = closeObj.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(1, 0.5f);
            cRT.anchorMax = new Vector2(1, 0.5f);
            cRT.pivot = new Vector2(1, 0.5f);
            cRT.anchoredPosition = new Vector2(-12, 0);
            cRT.sizeDelta = new Vector2(54, 54);
            var cImg = closeObj.AddComponent<Image>();
            if (btnCloseX != null) cImg.sprite = btnCloseX;
            var closeBtn = closeObj.AddComponent<Button>();

            // Pill Tiền Tệ
            GameObject coinPill = CreateUIElement("Pill_CoTien", headerObj.transform);
            RectTransform cpRT = coinPill.GetComponent<RectTransform>();
            cpRT.anchorMin = new Vector2(1, 0.5f);
            cpRT.anchorMax = new Vector2(1, 0.5f);
            cpRT.pivot = new Vector2(1, 0.5f);
            cpRT.anchoredPosition = new Vector2(-80, 0);
            cpRT.sizeDelta = new Vector2(150, 42);
            var cpImg = coinPill.AddComponent<Image>();
            cpImg.type = Image.Type.Sliced;
            if (currencyPill != null) cpImg.sprite = currencyPill;

            GameObject coinTxtObj = CreateUIElement("Txt_Amount", coinPill.transform);
            SetStretchAnchor(coinTxtObj.GetComponent<RectTransform>());
            coinTxtObj.GetComponent<RectTransform>().offsetMin = new Vector2(40, 0);
            var coinTMP = coinTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) coinTMP.font = font;
            coinTMP.text = "<color=#FFD700>0</color>";
            coinTMP.fontSize = 18;
            coinTMP.fontStyle = FontStyles.Bold;
            coinTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // 5. Body Area (2 Cột Đối Xứng)
            GameObject bodyObj = CreateUIElement("Body_Area", modalObj.transform);
            RectTransform bRT = bodyObj.GetComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero;
            bRT.anchorMax = Vector2.one;
            bRT.offsetMin = new Vector2(36, 32);
            bRT.offsetMax = new Vector2(-36, -112);

            // --- CỘT TRÁI (DANH SÁCH THẺ + TAB) (Width 60%) ---
            GameObject leftCol = CreateUIElement("Col_Left_Grid", bodyObj.transform);
            RectTransform lRT = leftCol.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0);
            lRT.anchorMax = new Vector2(0.60f, 1);
            lRT.offsetMin = Vector2.zero;
            lRT.offsetMax = new Vector2(-12, 0);

            // Tab Bar
            GameObject tabBar = CreateUIElement("TabBar", leftCol.transform);
            RectTransform tbRT = tabBar.GetComponent<RectTransform>();
            tbRT.anchorMin = new Vector2(0, 1);
            tbRT.anchorMax = new Vector2(1, 1);
            tbRT.pivot = new Vector2(0.5f, 1);
            tbRT.anchoredPosition = Vector2.zero;
            tbRT.sizeDelta = new Vector2(0, 52);

            var tbHlg = tabBar.AddComponent<HorizontalLayoutGroup>();
            tbHlg.spacing = 10;
            tbHlg.childControlWidth = true;
            tbHlg.childControlHeight = true;

            // Tab 1: Luyện Hóa
            GameObject t1 = CreateTabBtn("Tab_Fusion", "LUYỆN HÓA THẦN BINH", tabBar.transform, font, tabWoodActive, out Button tab1Btn, out Image tab1Bg, out TextMeshProUGUI tab1Txt);
            // Tab 2: Thần Thẻ Bị Động
            GameObject t2 = CreateTabBtn("Tab_Passives", "THẦN THẺ BỊ ĐỘNG", tabBar.transform, font, tabWoodInactive, out Button tab2Btn, out Image tab2Bg, out TextMeshProUGUI tab2Txt);
            // Tab 3: Bí Kíp Đòn Chém
            GameObject t3 = CreateTabBtn("Tab_Combo", "BÍ KÍP ĐÒN CHÉM", tabBar.transform, font, tabWoodInactive, out Button tab3Btn, out Image tab3Bg, out TextMeshProUGUI tab3Txt);

            // ScrollView Thẻ Bài
            GameObject scrollObj = CreateUIElement("ScrollView_Cards", leftCol.transform);
            RectTransform scRT = scrollObj.GetComponent<RectTransform>();
            scRT.anchorMin = Vector2.zero;
            scRT.anchorMax = Vector2.one;
            scRT.offsetMin = Vector2.zero;
            scRT.offsetMax = new Vector2(0, -62);

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            GameObject viewPort = CreateUIElement("Viewport", scrollObj.transform);
            SetStretchAnchor(viewPort.GetComponent<RectTransform>());
            viewPort.AddComponent<RectMask2D>();
            scrollRect.viewport = viewPort.GetComponent<RectTransform>();

            GameObject content = CreateUIElement("Content", viewPort.transform);
            RectTransform cntRT = content.GetComponent<RectTransform>();
            cntRT.anchorMin = new Vector2(0, 1);
            cntRT.anchorMax = new Vector2(1, 1);
            cntRT.pivot = new Vector2(0.5f, 1);
            cntRT.sizeDelta = new Vector2(0, 800);

            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(110, 130);
            grid.spacing = new Vector2(16, 16);
            grid.padding = new RectOffset(16, 16, 16, 16);
            grid.childAlignment = TextAnchor.UpperLeft;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = cntRT;

            // --- CỘT PHẢI (CHI TIẾT & LÒ LUYỆN KHÍ) (Width 40%) ---
            GameObject rightCol = CreateUIElement("Col_Right_Detail", bodyObj.transform);
            RectTransform rRT = rightCol.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0.60f, 0);
            rRT.anchorMax = new Vector2(1, 1);
            rRT.offsetMin = new Vector2(12, 0);
            rRT.offsetMax = Vector2.zero;

            var rcImg = rightCol.AddComponent<Image>();
            rcImg.type = Image.Type.Sliced;
            if (cardDetailParchment != null) rcImg.sprite = cardDetailParchment;

            // Icon + Tên
            GameObject detailHeader = CreateUIElement("Detail_Header", rightCol.transform);
            RectTransform dhRT = detailHeader.GetComponent<RectTransform>();
            dhRT.anchorMin = new Vector2(0, 1);
            dhRT.anchorMax = new Vector2(1, 1);
            dhRT.pivot = new Vector2(0.5f, 1);
            dhRT.anchoredPosition = new Vector2(0, -18);
            dhRT.sizeDelta = new Vector2(-36, 100);

            GameObject dIconObj = CreateUIElement("Icon_Card", detailHeader.transform);
            RectTransform diRT = dIconObj.GetComponent<RectTransform>();
            diRT.anchorMin = new Vector2(0, 0.5f);
            diRT.anchorMax = new Vector2(0, 0.5f);
            diRT.pivot = new Vector2(0, 0.5f);
            diRT.anchoredPosition = Vector2.zero;
            diRT.sizeDelta = new Vector2(86, 86);
            var diImg = dIconObj.AddComponent<Image>();
            diImg.preserveAspect = true;

            GameObject dInfoObj = CreateUIElement("Info_Text", detailHeader.transform);
            RectTransform dfRT = dInfoObj.GetComponent<RectTransform>();
            dfRT.anchorMin = new Vector2(0, 0);
            dfRT.anchorMax = new Vector2(1, 1);
            dfRT.offsetMin = new Vector2(100, 0);
            dfRT.offsetMax = Vector2.zero;

            var dNameTMP = CreateTextMeshPro(CreateUIElement("Txt_Name", dInfoObj.transform), font);
            dNameTMP.text = "Vạn Dép Bát Quái Thần Hỏa";
            dNameTMP.fontSize = 20;
            dNameTMP.fontStyle = FontStyles.Bold;
            dNameTMP.color = new Color(0.98f, 0.88f, 0.50f, 1f);

            var dTypeTMP = CreateTextMeshPro(CreateUIElement("Txt_Type", dInfoObj.transform), font);
            dTypeTMP.text = "<color=#FFD700>[THẦN BINH & PHÁP BẢO]</color>";
            dTypeTMP.fontSize = 13;
            dTypeTMP.color = new Color(0.85f, 0.78f, 0.65f, 1f);

            var dStarTMP = CreateTextMeshPro(CreateUIElement("Txt_StarBadge", dInfoObj.transform), font);
            dStarTMP.text = "<color=#FFD700>Cấp Độ: 3 Sao</color>";
            dStarTMP.fontSize = 14;
            dStarTMP.fontStyle = FontStyles.Bold;

            var dShardTMP = CreateTextMeshPro(CreateUIElement("Txt_ShardProgress", dInfoObj.transform), font);
            dShardTMP.text = "Tiến Độ Thẻ: <color=#00FF88>15/20 Thẻ</color>";
            dShardTMP.fontSize = 13;
            dShardTMP.color = new Color(0.9f, 0.85f, 0.75f, 1f);

            // Mô Tả
            GameObject descObj = CreateUIElement("Txt_Description", rightCol.transform);
            RectTransform ddRT = descObj.GetComponent<RectTransform>();
            ddRT.anchorMin = new Vector2(0, 0);
            ddRT.anchorMax = new Vector2(1, 1);
            ddRT.offsetMin = new Vector2(24, 110);
            ddRT.offsetMax = new Vector2(-24, -130);
            var dDescTMP = descObj.AddComponent<TextMeshProUGUI>();
            if (font != null) dDescTMP.font = font;
            dDescTMP.text = "Mô tả chi tiết công thức luyện hóa thần binh và hiệu ứng uy lực.";
            dDescTMP.fontSize = 15;
            dDescTMP.color = new Color(0.90f, 0.85f, 0.75f, 1f);
            dDescTMP.enableWordWrapping = true;

            // Nút Luyện Khí
            GameObject alchemyBtnObj = CreateUIElement("Btn_AlchemyFusion", rightCol.transform);
            RectTransform abRT = alchemyBtnObj.GetComponent<RectTransform>();
            abRT.anchorMin = new Vector2(0, 0);
            abRT.anchorMax = new Vector2(1, 0);
            abRT.pivot = new Vector2(0.5f, 0);
            abRT.anchoredPosition = new Vector2(0, 20);
            abRT.sizeDelta = new Vector2(-48, 68);
            var abImg = alchemyBtnObj.AddComponent<Image>();
            abImg.type = Image.Type.Sliced;
            if (btnAmber != null) abImg.sprite = btnAmber;
            var alchemyBtn = alchemyBtnObj.AddComponent<Button>();

            var abTxt = CreateTextMeshPro(CreateUIElement("Text", alchemyBtnObj.transform), font);
            SetStretchAnchor(abTxt.GetComponent<RectTransform>());
            abTxt.text = "LUYỆN HÓA THẦN BINH";
            abTxt.fontSize = 20;
            abTxt.fontStyle = FontStyles.Bold;
            abTxt.alignment = TextAlignmentOptions.Center;
            abTxt.color = new Color(1f, 0.95f, 0.80f, 1f);

            // 6. Wire Serialized Properties to View
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_modalContainer").objectReferenceValue = modalRT;
            soView.FindProperty("_backButton").objectReferenceValue = closeBtn;
            soView.FindProperty("_dimBackgroundButton").objectReferenceValue = dimBtn;
            soView.FindProperty("_coTienText").objectReferenceValue = coinTMP;

            soView.FindProperty("_tabFusionButton").objectReferenceValue = tab1Btn;
            soView.FindProperty("_tabPassivesButton").objectReferenceValue = tab2Btn;
            soView.FindProperty("_tabComboButton").objectReferenceValue = tab3Btn;

            soView.FindProperty("_tabFusionBg").objectReferenceValue = tab1Bg;
            soView.FindProperty("_tabPassivesBg").objectReferenceValue = tab2Bg;
            soView.FindProperty("_tabComboBg").objectReferenceValue = tab3Bg;

            soView.FindProperty("_tabFusionTxt").objectReferenceValue = tab1Txt;
            soView.FindProperty("_tabPassivesTxt").objectReferenceValue = tab2Txt;
            soView.FindProperty("_tabComboTxt").objectReferenceValue = tab3Txt;

            soView.FindProperty("_cardsGridContainer").objectReferenceValue = content.transform;
            soView.FindProperty("_detailIcon").objectReferenceValue = diImg;
            soView.FindProperty("_detailName").objectReferenceValue = dNameTMP;
            soView.FindProperty("_detailType").objectReferenceValue = dTypeTMP;
            soView.FindProperty("_detailDesc").objectReferenceValue = dDescTMP;
            soView.FindProperty("_detailStarBadge").objectReferenceValue = dStarTMP;
            soView.FindProperty("_detailShardProgress").objectReferenceValue = dShardTMP;
            soView.FindProperty("_alchemyFusionButton").objectReferenceValue = alchemyBtn;
            soView.FindProperty("_fusionButtonText").objectReferenceValue = abTxt;
            soView.ApplyModifiedProperties();

            // 7. Wire Presenter
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.FindProperty("_tabActiveSprite").objectReferenceValue = tabWoodActive;
            soPresenter.FindProperty("_tabInactiveSprite").objectReferenceValue = tabWoodInactive;
            soPresenter.FindProperty("_cardSlotWoodSprite").objectReferenceValue = slotWoodSprite;
            soPresenter.FindProperty("_cardSlotSelectedSprite").objectReferenceValue = slotSelectedSprite;
            soPresenter.ApplyModifiedProperties();

            // 8. Lưu Prefab
            string prefabPath = $"{prefabFolder}/CardCodexUI.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // 9. Auto-wire vào Scene và MetaUIManager
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                var oldUI = GameObject.Find("Panel_CardCodex");
                if (oldUI != null && oldUI != root) Object.DestroyImmediate(oldUI);

                var metaCanvas = GameObject.Find("Canvas_MetaMenu");
                Transform targetParent = metaCanvas != null ? metaCanvas.transform : canvas.transform;

                root.transform.SetParent(targetParent, false);
                SetStretchAnchor(rootRT);

                var metaMgr = Object.FindAnyObjectByType<MetaUIManager>();
                if (metaMgr != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaMgr);
                    soMeta.FindProperty("_codexScreen").objectReferenceValue = view;
                    soMeta.ApplyModifiedProperties();
                    EditorUtility.SetDirty(metaMgr);
                }

                Debug.Log($"<color=#00FF88>[CardCodexUIGenerator]</color> Đã tạo Prefab Thư Viện Thần Thẻ & Luyện Khí (Codex) và kết nối MetaUIManager thành công 100%!");
            }
            else
            {
                Object.DestroyImmediate(root);
            }

            return savedPrefab;
        }

        private static GameObject CreateTabBtn(string name, string label, Transform parent, TMP_FontAsset font, Sprite bgSprite, out Button btn, out Image bgImg, out TextMeshProUGUI txt)
        {
            GameObject go = CreateUIElement(name, parent);
            bgImg = go.AddComponent<Image>();
            bgImg.type = Image.Type.Sliced;
            if (bgSprite != null) bgImg.sprite = bgSprite;
            btn = go.AddComponent<Button>();

            GameObject tObj = CreateUIElement("Text", go.transform);
            SetStretchAnchor(tObj.GetComponent<RectTransform>());
            txt = tObj.AddComponent<TextMeshProUGUI>();
            if (font != null) txt.font = font;
            txt.text = label;
            txt.fontSize = 13;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = new Color(0.98f, 0.88f, 0.50f, 1f);
            return go;
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretchAnchor(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateTextMeshPro(GameObject go, TMP_FontAsset font)
        {
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            return tmp;
        }
    }
}
#endif

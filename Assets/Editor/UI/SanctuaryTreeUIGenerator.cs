using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Editor.UI
{
    public static class SanctuaryTreeUIGenerator
    {
        [MenuItem("ProjectZombie/UI/Generate Sanctuary Tree (Mieu Tu Bat Tu)")]
        public static GameObject GenerateSanctuaryTreePrefab()
        {
            // 1. Sinh Tree Data trước nếu chưa có
            PermanentUpgradeTreeData treeData = MetaProgression.PermanentUpgradeTreeGenerator.GenerateTreeData();

            string prefabFolder = "Assets/_Prefabs/UI";
            if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Font/UTM AvoBold.ttf");
            if (font == null)
            {
                var fontGUIDs = AssetDatabase.FindAssets("t:TMP_FontAsset");
                if (fontGUIDs.Length > 0)
                    font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGUIDs[0]));
            }

            // 2. Load Visual Assets
            Sprite modalFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Modal_TangBaoCac_9Slice.png");
            Sprite headerBar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Header_Wood_Bar_VongXuyen.png");
            Sprite titleBanner = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Banner_Upgrade_Parchment.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Close_X_Wood.png");
            Sprite cardTotem = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite cardDetailParchment = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");
            Sprite btnAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");
            Sprite slotGlow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
            Sprite iconBoxFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Box_Skill_Icon_Wood_9Slice.png");
            Sprite progressFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Frame_VongXuyen_9Slice.png");
            Sprite progressFill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_HP.png");
            Sprite currencyPill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pill_Currency_Wood.png");
            Sprite tabWoodActive = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Active.png");
            Sprite tabWoodInactive = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Inactive.png");

            // 3. Root GameObject
            GameObject root = new GameObject("Panel_SanctuaryTree", typeof(RectTransform), typeof(CanvasGroup), typeof(MetaUpgradeShopView), typeof(MetaUpgradeShopPresenter));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            SetStretchAnchor(rootRT);

            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            var view = root.GetComponent<MetaUpgradeShopView>();
            var presenter = root.GetComponent<MetaUpgradeShopPresenter>();

            // Dim Background Button
            GameObject dimObj = CreateUIElement("Dim_Background", root.transform);
            SetStretchAnchor(dimObj.GetComponent<RectTransform>());
            var dimImg = dimObj.AddComponent<Image>();
            dimImg.color = new Color(0, 0, 0, 0.65f);
            var dimBtn = dimObj.AddComponent<Button>();

            // Modal Container
            GameObject modalObj = CreateUIElement("Modal_Container", root.transform);
            RectTransform modalRT = modalObj.GetComponent<RectTransform>();
            modalRT.anchorMin = new Vector2(0.5f, 0.5f);
            modalRT.anchorMax = new Vector2(0.5f, 0.5f);
            modalRT.pivot = new Vector2(0.5f, 0.5f);
            modalRT.anchoredPosition = Vector2.zero;
            modalRT.sizeDelta = new Vector2(880, 530);
            var modalImg = modalObj.AddComponent<Image>();
            modalImg.color = Color.white;
            modalImg.type = Image.Type.Sliced;
            if (modalFrame != null) modalImg.sprite = modalFrame;

            // 4. Header Row
            GameObject headerObj = CreateUIElement("Header_Row", modalObj.transform);
            RectTransform headerRT = headerObj.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = new Vector2(0, -6);
            headerRT.sizeDelta = new Vector2(-16, 56);
            var headerImg = headerObj.AddComponent<Image>();
            headerImg.color = Color.white;
            headerImg.type = Image.Type.Sliced;
            if (headerBar != null) headerImg.sprite = headerBar;

            // Header Title Banner
            GameObject titleObj = CreateUIElement("Banner_Title", headerObj.transform);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = new Vector2(0, 4);
            titleRT.sizeDelta = new Vector2(260, 46);
            var titleImg = titleObj.AddComponent<Image>();
            titleImg.color = Color.white;
            if (titleBanner != null) titleImg.sprite = titleBanner;

            GameObject titleTextObj = CreateUIElement("Txt_Title", titleObj.transform);
            SetStretchAnchor(titleTextObj.GetComponent<RectTransform>());
            var titleTMP = titleTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleTMP.font = font;
            titleTMP.text = "MIẾU TỨ BẤT TỬ";
            titleTMP.fontSize = 18;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.98f, 0.92f, 0.70f, 1f);

            // Header Currency Pill
            GameObject currPillObj = CreateUIElement("Pill_Currency", headerObj.transform);
            RectTransform cpRT = currPillObj.GetComponent<RectTransform>();
            cpRT.anchorMin = new Vector2(0, 0.5f);
            cpRT.anchorMax = new Vector2(0, 0.5f);
            cpRT.pivot = new Vector2(0, 0.5f);
            cpRT.anchoredPosition = new Vector2(20, 2);
            cpRT.sizeDelta = new Vector2(170, 36);
            var cpImg = currPillObj.AddComponent<Image>();
            cpImg.color = Color.white;
            cpImg.type = Image.Type.Sliced;
            if (currencyPill != null) cpImg.sprite = currencyPill;

            GameObject cpTextObj = CreateUIElement("Txt_CoTien", currPillObj.transform);
            SetStretchAnchor(cpTextObj.GetComponent<RectTransform>());
            var cpTMP = cpTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) cpTMP.font = font;
            cpTMP.text = "0 Cổ Tiền";
            cpTMP.fontSize = 13;
            cpTMP.fontStyle = FontStyles.Bold;
            cpTMP.alignment = TextAlignmentOptions.Center;
            cpTMP.color = new Color(1f, 0.85f, 0.3f, 1f);

            // Header Close Button
            GameObject closeObj = CreateUIElement("Btn_Close", headerObj.transform);
            RectTransform closeRT = closeObj.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1, 0.5f);
            closeRT.anchorMax = new Vector2(1, 0.5f);
            closeRT.pivot = new Vector2(1, 0.5f);
            closeRT.anchoredPosition = new Vector2(-16, 2);
            closeRT.sizeDelta = new Vector2(44, 44);
            var closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.white;
            if (btnCloseX != null)
            {
                closeImg.sprite = btnCloseX;
                closeImg.preserveAspect = true;
            }
            var closeBtn = closeObj.AddComponent<Button>();

            // 5. Tabs Row (Top of Content)
            GameObject tabsRowObj = CreateUIElement("Tabs_BranchRow", modalObj.transform);
            RectTransform trRT = tabsRowObj.GetComponent<RectTransform>();
            trRT.anchorMin = new Vector2(0, 1);
            trRT.anchorMax = new Vector2(1, 1);
            trRT.pivot = new Vector2(0.5f, 1);
            trRT.anchoredPosition = new Vector2(0, -66);
            trRT.sizeDelta = new Vector2(-36, 42);

            CreateTabButton(tabsRowObj.transform, font, "Tab_TanVien", "TẢN VIÊN", 0, tabWoodActive, out Button tab1Btn, out Image tab1Bg, out TextMeshProUGUI tab1Txt);
            CreateTabButton(tabsRowObj.transform, font, "Tab_PhuDong", "PHÙ ĐỔNG", 1, tabWoodInactive, out Button tab2Btn, out Image tab2Bg, out TextMeshProUGUI tab2Txt);
            CreateTabButton(tabsRowObj.transform, font, "Tab_LieuHanh", "LIỄU HẠNH", 2, tabWoodInactive, out Button tab3Btn, out Image tab3Bg, out TextMeshProUGUI tab3Txt);

            // 6. Left Column: 3 Upgrade Node Cards
            GameObject leftColObj = CreateUIElement("LeftColumn_Cards", modalObj.transform);
            RectTransform lcRT = leftColObj.GetComponent<RectTransform>();
            lcRT.anchorMin = new Vector2(0, 0);
            lcRT.anchorMax = new Vector2(0.52f, 1);
            lcRT.pivot = new Vector2(0, 0.5f);
            lcRT.anchoredPosition = new Vector2(18, -48);
            lcRT.sizeDelta = new Vector2(0, -120);

            UpgradeNodeCardItem[] cards = new UpgradeNodeCardItem[3];
            for (int i = 0; i < 3; i++)
            {
                cards[i] = CreateNodeCard(leftColObj.transform, font, i, cardTotem, slotGlow, iconBoxFrame);
            }

            // 7. Right Column: Details Panel
            GameObject rightColObj = CreateUIElement("RightColumn_Details", modalObj.transform);
            RectTransform rcRT = rightColObj.GetComponent<RectTransform>();
            rcRT.anchorMin = new Vector2(0.52f, 0);
            rcRT.anchorMax = new Vector2(1, 1);
            rcRT.pivot = new Vector2(1, 0.5f);
            rcRT.anchoredPosition = new Vector2(-18, -48);
            rcRT.sizeDelta = new Vector2(0, -120);

            var detailBg = rightColObj.AddComponent<Image>();
            detailBg.color = Color.white;
            detailBg.type = Image.Type.Sliced;
            if (cardDetailParchment != null) detailBg.sprite = cardDetailParchment;

            BuildDetailsPanel(rightColObj.transform, font, iconBoxFrame, progressFrame, progressFill, btnAmber,
                out Image detIcon, out TextMeshProUGUI detTitle, out TextMeshProUGUI detBranch, out TextMeshProUGUI detDesc,
                out TextMeshProUGUI detLevel, out Image detProgressBar, out TextMeshProUGUI detBonus,
                out TextMeshProUGUI detCost, out Button buyBtn, out TextMeshProUGUI buyTxt);

            // 8. Wire Serialized Properties vào View
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_coTienBalanceText").objectReferenceValue = cpTMP;
            soView.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            soView.FindProperty("_modalContainer").objectReferenceValue = modalRT;
            soView.FindProperty("_dimBackgroundButton").objectReferenceValue = dimBtn;

            soView.FindProperty("_tabTanVienButton").objectReferenceValue = tab1Btn;
            soView.FindProperty("_tabPhuDongButton").objectReferenceValue = tab2Btn;
            soView.FindProperty("_tabLieuHanhButton").objectReferenceValue = tab3Btn;

            soView.FindProperty("_tabTanVienBg").objectReferenceValue = tab1Bg;
            soView.FindProperty("_tabPhuDongBg").objectReferenceValue = tab2Bg;
            soView.FindProperty("_tabLieuHanhBg").objectReferenceValue = tab3Bg;

            soView.FindProperty("_tabTanVienText").objectReferenceValue = tab1Txt;
            soView.FindProperty("_tabPhuDongText").objectReferenceValue = tab2Txt;
            soView.FindProperty("_tabLieuHanhText").objectReferenceValue = tab3Txt;

            var cardsProp = soView.FindProperty("_nodeCards");
            cardsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                var elem = cardsProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("rootObject").objectReferenceValue = cards[i].rootObject;
                elem.FindPropertyRelative("selectButton").objectReferenceValue = cards[i].selectButton;
                elem.FindPropertyRelative("iconImage").objectReferenceValue = cards[i].iconImage;
                elem.FindPropertyRelative("titleText").objectReferenceValue = cards[i].titleText;
                elem.FindPropertyRelative("levelText").objectReferenceValue = cards[i].levelText;
                elem.FindPropertyRelative("selectionHighlight").objectReferenceValue = cards[i].selectionHighlight;
                elem.FindPropertyRelative("iconFrame").objectReferenceValue = cards[i].iconFrame;
            }

            soView.FindProperty("_detailIcon").objectReferenceValue = detIcon;
            soView.FindProperty("_detailTitleText").objectReferenceValue = detTitle;
            soView.FindProperty("_detailBranchText").objectReferenceValue = detBranch;
            soView.FindProperty("_detailDescText").objectReferenceValue = detDesc;
            soView.FindProperty("_detailLevelText").objectReferenceValue = detLevel;
            soView.FindProperty("_detailLevelProgressBar").objectReferenceValue = detProgressBar;
            soView.FindProperty("_detailBonusPreviewText").objectReferenceValue = detBonus;
            soView.FindProperty("_upgradeCostText").objectReferenceValue = detCost;
            soView.FindProperty("_buyUpgradeButton").objectReferenceValue = buyBtn;
            soView.FindProperty("_buyButtonText").objectReferenceValue = buyTxt;
            soView.ApplyModifiedProperties();

            // 9. Wire Presenter
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.FindProperty("_treeData").objectReferenceValue = treeData;
            soPresenter.FindProperty("_tabActiveSprite").objectReferenceValue = tabWoodActive;
            soPresenter.FindProperty("_tabInactiveSprite").objectReferenceValue = tabWoodInactive;
            soPresenter.ApplyModifiedProperties();

            // 10. Save Prefab
            string prefabPath = $"{prefabFolder}/SanctuaryTreeUI.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // 11. Wire vào Scene nếu có Canvas_MetaMenu
            var metaCanvas = GameObject.Find("Canvas_MetaMenu");
            if (metaCanvas != null)
            {
                var oldUI = GameObject.Find("Panel_SanctuaryTree");
                if (oldUI != null && oldUI != root) Object.DestroyImmediate(oldUI);

                root.transform.SetParent(metaCanvas.transform, false);
                SetStretchAnchor(rootRT);

                var metaMgr = Object.FindAnyObjectByType<MetaUIManager>();
                if (metaMgr != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaMgr);
                    soMeta.FindProperty("_sanctuaryTreeScreen").objectReferenceValue = view;
                    soMeta.ApplyModifiedProperties();
                    EditorUtility.SetDirty(metaMgr);
                }

                Debug.Log($"<color=#00FF88>[SanctuaryTreeUIGenerator]</color> Đã tạo Prefab Miếu Tứ Bất Tử hoàn chỉnh và kết nối thành công!");
            }
            else
            {
                Object.DestroyImmediate(root);
            }

            return savedPrefab;
        }

        private static void CreateTabButton(Transform parent, TMP_FontAsset font, string name, string label, int index, Sprite bgSprite,
            out Button btn, out Image bg, out TextMeshProUGUI txt)
        {
            GameObject obj = CreateUIElement(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            float tabWidth = 142;
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(index * (tabWidth + 8), 0);
            rt.sizeDelta = new Vector2(tabWidth, 38);

            bg = obj.AddComponent<Image>();
            bg.color = Color.white;
            bg.type = Image.Type.Sliced;
            if (bgSprite != null) bg.sprite = bgSprite;

            btn = obj.AddComponent<Button>();

            GameObject textObj = CreateUIElement("Text", obj.transform);
            SetStretchAnchor(textObj.GetComponent<RectTransform>());
            txt = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) txt.font = font;
            txt.text = label;
            txt.fontSize = 12;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = new Color(0.95f, 0.88f, 0.70f, 1f);
        }

        private static UpgradeNodeCardItem CreateNodeCard(Transform parent, TMP_FontAsset font, int index, Sprite cardBg, Sprite glowSprite, Sprite iconFrame)
        {
            var item = new UpgradeNodeCardItem();

            GameObject cardObj = CreateUIElement($"Card_Node_{index}", parent);
            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -index * 118);
            rt.sizeDelta = new Vector2(0, 110);

            var imgBg = cardObj.AddComponent<Image>();
            imgBg.color = Color.white;
            imgBg.type = Image.Type.Sliced;
            if (cardBg != null) imgBg.sprite = cardBg;

            item.rootObject = cardObj;
            item.selectButton = cardObj.AddComponent<Button>();

            // Glow Border
            GameObject glowObj = CreateUIElement("Highlight_Glow", cardObj.transform);
            SetStretchAnchor(glowObj.GetComponent<RectTransform>());
            var glowImg = glowObj.AddComponent<Image>();
            glowImg.color = Color.white;
            glowImg.type = Image.Type.Sliced;
            if (glowSprite != null) glowImg.sprite = glowSprite;
            glowObj.SetActive(index == 0);
            item.selectionHighlight = glowImg;

            // Icon Box
            GameObject iconBox = CreateUIElement("Icon_Box", cardObj.transform);
            RectTransform ibRT = iconBox.GetComponent<RectTransform>();
            ibRT.anchorMin = new Vector2(0, 0.5f);
            ibRT.anchorMax = new Vector2(0, 0.5f);
            ibRT.pivot = new Vector2(0, 0.5f);
            ibRT.anchoredPosition = new Vector2(14, 0);
            ibRT.sizeDelta = new Vector2(80, 80);
            var ibImg = iconBox.AddComponent<Image>();
            ibImg.color = Color.white;
            ibImg.type = Image.Type.Sliced;
            if (iconFrame != null) ibImg.sprite = iconFrame;
            item.iconFrame = ibImg;

            GameObject iconInner = CreateUIElement("Icon", iconBox.transform);
            RectTransform iiRT = iconInner.GetComponent<RectTransform>();
            iiRT.anchorMin = new Vector2(0.5f, 0.5f);
            iiRT.anchorMax = new Vector2(0.5f, 0.5f);
            iiRT.pivot = new Vector2(0.5f, 0.5f);
            iiRT.anchoredPosition = Vector2.zero;
            iiRT.sizeDelta = new Vector2(64, 64);
            var iconImg = iconInner.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            item.iconImage = iconImg;

            // Title Text
            GameObject titleObj = CreateUIElement("Txt_NodeTitle", cardObj.transform);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0, 1);
            titleRT.anchoredPosition = new Vector2(104, -18);
            titleRT.sizeDelta = new Vector2(-114, 28);
            var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleTMP.font = font;
            titleTMP.text = "Tên Kỹ Năng";
            titleTMP.fontSize = 15;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = new Color(0.98f, 0.92f, 0.72f, 1f);
            item.titleText = titleTMP;

            // Level Text
            GameObject levelObj = CreateUIElement("Txt_Level", cardObj.transform);
            RectTransform lvlRT = levelObj.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0, 0);
            lvlRT.anchorMax = new Vector2(1, 0);
            lvlRT.pivot = new Vector2(0, 0);
            lvlRT.anchoredPosition = new Vector2(104, 18);
            lvlRT.sizeDelta = new Vector2(-114, 24);
            var lvlTMP = levelObj.AddComponent<TextMeshProUGUI>();
            if (font != null) lvlTMP.font = font;
            lvlTMP.text = "Cấp 0 / 5";
            lvlTMP.fontSize = 13;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.color = new Color(0.0f, 1.0f, 0.55f, 1f);
            item.levelText = lvlTMP;

            return item;
        }

        private static void BuildDetailsPanel(Transform parent, TMP_FontAsset font, Sprite iconFrame, Sprite progFrame, Sprite progFill, Sprite btnAmber,
            out Image detIcon, out TextMeshProUGUI detTitle, out TextMeshProUGUI detBranch, out TextMeshProUGUI detDesc,
            out TextMeshProUGUI detLevel, out Image detProgressBar, out TextMeshProUGUI detBonus,
            out TextMeshProUGUI detCost, out Button buyBtn, out TextMeshProUGUI buyTxt)
        {
            // Big Icon Box
            GameObject iconBox = CreateUIElement("Detail_Icon_Box", parent);
            RectTransform ibRT = iconBox.GetComponent<RectTransform>();
            ibRT.anchorMin = new Vector2(0, 1);
            ibRT.anchorMax = new Vector2(0, 1);
            ibRT.pivot = new Vector2(0, 1);
            ibRT.anchoredPosition = new Vector2(20, -20);
            ibRT.sizeDelta = new Vector2(86, 86);
            var ibImg = iconBox.AddComponent<Image>();
            ibImg.color = Color.white;
            ibImg.type = Image.Type.Sliced;
            if (iconFrame != null) ibImg.sprite = iconFrame;

            GameObject iconInner = CreateUIElement("Icon", iconBox.transform);
            RectTransform iiRT = iconInner.GetComponent<RectTransform>();
            iiRT.anchorMin = new Vector2(0.5f, 0.5f);
            iiRT.anchorMax = new Vector2(0.5f, 0.5f);
            iiRT.pivot = new Vector2(0.5f, 0.5f);
            iiRT.anchoredPosition = Vector2.zero;
            iiRT.sizeDelta = new Vector2(70, 70);
            detIcon = iconInner.AddComponent<Image>();
            detIcon.color = Color.white;
            detIcon.preserveAspect = true;

            // Title
            GameObject titleObj = CreateUIElement("Txt_DetailTitle", parent);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0, 1);
            titleRT.anchoredPosition = new Vector2(116, -18);
            titleRT.sizeDelta = new Vector2(-130, 28);
            detTitle = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detTitle.font = font;
            detTitle.text = "Tên Chỉ Số";
            detTitle.fontSize = 17;
            detTitle.fontStyle = FontStyles.Bold;
            detTitle.color = new Color(0.98f, 0.90f, 0.65f, 1f);

            // Branch Badge
            GameObject branchObj = CreateUIElement("Txt_Branch", parent);
            RectTransform brRT = branchObj.GetComponent<RectTransform>();
            brRT.anchorMin = new Vector2(0, 1);
            brRT.anchorMax = new Vector2(1, 1);
            brRT.pivot = new Vector2(0, 1);
            brRT.anchoredPosition = new Vector2(116, -46);
            brRT.sizeDelta = new Vector2(-130, 22);
            detBranch = branchObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detBranch.font = font;
            detBranch.text = "Nhánh Tản Viên Sơn Thánh";
            detBranch.fontSize = 12;
            detBranch.fontStyle = FontStyles.Italic;
            detBranch.color = new Color(0.85f, 0.70f, 0.40f, 1f);

            // Level Text & Progress Bar
            GameObject lvlObj = CreateUIElement("Txt_LevelProgress", parent);
            RectTransform lvlRT = lvlObj.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0, 1);
            lvlRT.anchorMax = new Vector2(1, 1);
            lvlRT.pivot = new Vector2(0, 1);
            lvlRT.anchoredPosition = new Vector2(116, -70);
            lvlRT.sizeDelta = new Vector2(-130, 20);
            detLevel = lvlObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detLevel.font = font;
            detLevel.text = "Cấp Hiện Tại: 0 / 10";
            detLevel.fontSize = 12;
            detLevel.color = Color.white;

            GameObject progFrameObj = CreateUIElement("Bar_ProgressFrame", parent);
            RectTransform pfRT = progFrameObj.GetComponent<RectTransform>();
            pfRT.anchorMin = new Vector2(0, 1);
            pfRT.anchorMax = new Vector2(1, 1);
            pfRT.pivot = new Vector2(0, 1);
            pfRT.anchoredPosition = new Vector2(116, -92);
            pfRT.sizeDelta = new Vector2(-136, 14);
            var pfImg = progFrameObj.AddComponent<Image>();
            pfImg.color = Color.white;
            pfImg.type = Image.Type.Sliced;
            if (progFrame != null) pfImg.sprite = progFrame;

            GameObject progFillObj = CreateUIElement("Fill", progFrameObj.transform);
            SetStretchAnchor(progFillObj.GetComponent<RectTransform>());
            detProgressBar = progFillObj.AddComponent<Image>();
            detProgressBar.color = Color.white;
            detProgressBar.type = Image.Type.Filled;
            detProgressBar.fillMethod = Image.FillMethod.Horizontal;
            detProgressBar.fillAmount = 0.4f;
            if (progFill != null) detProgressBar.sprite = progFill;

            // Description
            GameObject descObj = CreateUIElement("Txt_Description", parent);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 1);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0, 1);
            descRT.anchoredPosition = new Vector2(20, -120);
            descRT.sizeDelta = new Vector2(-40, 60);
            detDesc = descObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detDesc.font = font;
            detDesc.text = "Mô tả hiệu ứng kỹ năng.";
            detDesc.fontSize = 13;
            detDesc.color = new Color(0.90f, 0.85f, 0.75f, 1f);

            // Bonus Preview
            GameObject bonusObj = CreateUIElement("Txt_BonusPreview", parent);
            RectTransform bonRT = bonusObj.GetComponent<RectTransform>();
            bonRT.anchorMin = new Vector2(0, 1);
            bonRT.anchorMax = new Vector2(1, 1);
            bonRT.pivot = new Vector2(0, 1);
            bonRT.anchoredPosition = new Vector2(20, -185);
            bonRT.sizeDelta = new Vector2(-40, 80);
            detBonus = bonusObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detBonus.font = font;
            detBonus.text = "- Máu Tối Đa: +0 HP (Cấp kế: +15 HP)";
            detBonus.fontSize = 12;
            detBonus.color = new Color(0.0f, 1.0f, 0.65f, 1f);

            // Cost Text
            GameObject costObj = CreateUIElement("Txt_UpgradeCost", parent);
            RectTransform costRT = costObj.GetComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0, 0);
            costRT.anchorMax = new Vector2(1, 0);
            costRT.pivot = new Vector2(0.5f, 0);
            costRT.anchoredPosition = new Vector2(0, 66);
            costRT.sizeDelta = new Vector2(-40, 24);
            detCost = costObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detCost.font = font;
            detCost.text = "Chi Phí: 100 Cổ Tiền";
            detCost.fontSize = 14;
            detCost.fontStyle = FontStyles.Bold;
            detCost.alignment = TextAlignmentOptions.Center;
            detCost.color = new Color(1f, 0.85f, 0.3f, 1f);

            // Buy Button
            GameObject buyObj = CreateUIElement("Btn_BuyUpgrade", parent);
            RectTransform buyRT = buyObj.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.5f, 0);
            buyRT.anchorMax = new Vector2(0.5f, 0);
            buyRT.pivot = new Vector2(0.5f, 0);
            buyRT.anchoredPosition = new Vector2(0, 14);
            buyRT.sizeDelta = new Vector2(260, 48);
            var buyImg = buyObj.AddComponent<Image>();
            buyImg.color = Color.white;
            buyImg.type = Image.Type.Sliced;
            if (btnAmber != null) buyImg.sprite = btnAmber;
            buyBtn = buyObj.AddComponent<Button>();

            GameObject buyTextObj = CreateUIElement("Text", buyObj.transform);
            SetStretchAnchor(buyTextObj.GetComponent<RectTransform>());
            buyTxt = buyTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) buyTxt.font = font;
            buyTxt.text = "CẦU PHÚC (NÂNG CẤP)";
            buyTxt.fontSize = 14;
            buyTxt.fontStyle = FontStyles.Bold;
            buyTxt.alignment = TextAlignmentOptions.Center;
            buyTxt.color = new Color(0.98f, 0.92f, 0.70f, 1f);
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetStretchAnchor(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}

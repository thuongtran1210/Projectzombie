#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tạo Prefab giao diện Tàng Bảo Các (WeaponLoadoutUI) chuẩn Cổ Phong Đông Sơn 2 Cột Đối Xứng (Khung Trúc Cổ & Khớp Gỗ Mun).
    /// Khớp 100% bố cục, màu sắc và tỷ lệ từ hình ảnh thiết kế chuẩn.
    /// </summary>
    public static class WeaponLoadoutUIGenerator
    {
        // Bảng màu Cổ Phong Đông Sơn cao cấp
        private static readonly Color ColorBgOverlay = new Color(0.04f, 0.03f, 0.06f, 0.90f);
        private static readonly Color ColorModalBg = new Color(0.09f, 0.07f, 0.12f, 0.98f);
        private static readonly Color ColorBambooFrame = new Color(0.42f, 0.28f, 0.16f, 1f); // Nâu Trúc Cổ
        private static readonly Color ColorCardBg = new Color(0.14f, 0.11f, 0.18f, 0.96f);
        private static readonly Color ColorCardInner = new Color(0.18f, 0.14f, 0.24f, 0.90f);
        private static readonly Color ColorGold = new Color(0.95f, 0.82f, 0.42f, 1f);
        private static readonly Color ColorCinnabar = new Color(0.78f, 0.22f, 0.18f, 1f);
        private static readonly Color ColorJadeCyan = new Color(0.25f, 0.85f, 0.82f, 1f);
        private static readonly Color ColorMutedText = new Color(0.80f, 0.80f, 0.85f, 1f);

        [MenuItem("Tools/ProjectZombie/UI/Generate Weapon Loadout UI Prefab", priority = 20)]
        public static void GenerateWeaponLoadoutPrefab()
        {
            string prefabFolder = "Assets/_Prefabs/UI";
            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs")) AssetDatabase.CreateFolder("Assets", "_Prefabs");
            if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder("Assets/_Prefabs", "UI");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 1. Root Screen Panel (100% Stretch)
            GameObject root = new GameObject("Panel_WeaponLoadout", typeof(RectTransform), typeof(CanvasGroup), typeof(WeaponLoadoutView), typeof(WeaponLoadoutPresenter));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            SetStretchAnchor(rootRT);

            var view = root.GetComponent<WeaponLoadoutView>();
            var presenter = root.GetComponent<WeaponLoadoutPresenter>();

            // 2. Dim Background (Bắt sự kiện click ra ngoài để đóng)
            GameObject bgDim = CreateUIElement("Dim_WeaponLoadout", root.transform);
            SetStretchAnchor(bgDim.GetComponent<RectTransform>());
            var bgDimImg = bgDim.AddComponent<Image>();
            bgDimImg.color = ColorBgOverlay;
            var bgDimBtn = bgDim.AddComponent<Button>();

            // 3. Modal Box Trung Tâm (1160 x 660)
            GameObject modal = CreateUIElement("Modal_WeaponLoadout", root.transform);
            RectTransform modalRT = modal.GetComponent<RectTransform>();
            modalRT.anchorMin = new Vector2(0.5f, 0.5f);
            modalRT.anchorMax = new Vector2(0.5f, 0.5f);
            modalRT.pivot = new Vector2(0.5f, 0.5f);
            modalRT.sizeDelta = new Vector2(1160, 660);
            modalRT.anchoredPosition = Vector2.zero;
            var modalImg = modal.AddComponent<Image>();
            modalImg.color = ColorModalBg;

            // 4. Header Section
            BuildHeader(modal.transform, vietFont, out TextMeshProUGUI heroNameTMP, out TextMeshProUGUI heroElemTMP, out Image heroAvatarImg, out Button backBtn);

            // 5. Body Section - 2 Cột Đối Xứng
            GameObject bodyObj = CreateUIElement("Container_Body2Cols", modal.transform);
            RectTransform bodyRT = bodyObj.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0, 0);
            bodyRT.anchorMax = new Vector2(1, 1);
            bodyRT.offsetMin = new Vector2(18, 16);
            bodyRT.offsetMax = new Vector2(-18, -60); // 60px cho Header

            var bodyHlg = bodyObj.AddComponent<HorizontalLayoutGroup>();
            bodyHlg.spacing = 16;
            bodyHlg.childAlignment = TextAnchor.UpperCenter;
            bodyHlg.childControlWidth = true;
            bodyHlg.childControlHeight = true;
            bodyHlg.childForceExpandWidth = true;
            bodyHlg.childForceExpandHeight = true;

            // Cột Trái: Kho Đồ Dạng Tab (Width 50%)
            BuildLeftInventoryColumn(bodyObj.transform, vietFont, 
                out Button tabPriBtn, out Button tabRelBtn, 
                out Image tabPriBg, out Image tabRelBg, 
                out TextMeshProUGUI tabPriTxt, out TextMeshProUGUI tabRelTxt, 
                out Transform inventoryGrid);

            // Cột Phải: Trang Bị Đã Chọn + Soi Chi Tiết + Nút Xuất Trận (Width 50%)
            BuildRightLoadoutColumn(bodyObj.transform, vietFont,
                out Image priSlotIcon, out TextMeshProUGUI priSlotName,
                out Image[] relicIcons, out TextMeshProUGUI[] relicNames,
                out Image detailIcon, out TextMeshProUGUI detailName, out TextMeshProUGUI detailType,
                out TextMeshProUGUI detailDmg, out TextMeshProUGUI detailCd,
                out Image dmgFill, out Image cdFill, out TextMeshProUGUI detailDesc,
                out Button startBattleBtn);

            // 6. Wire Properties to View
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_heroAvatarImage").objectReferenceValue = heroAvatarImg;
            soView.FindProperty("_heroNameText").objectReferenceValue = heroNameTMP;
            soView.FindProperty("_heroElementText").objectReferenceValue = heroElemTMP;

            soView.FindProperty("_tabPrimaryButton").objectReferenceValue = tabPriBtn;
            soView.FindProperty("_tabRelicsButton").objectReferenceValue = tabRelBtn;
            soView.FindProperty("_tabPrimaryBg").objectReferenceValue = tabPriBg;
            soView.FindProperty("_tabRelicsBg").objectReferenceValue = tabRelBg;
            soView.FindProperty("_tabPrimaryText").objectReferenceValue = tabPriTxt;
            soView.FindProperty("_tabRelicsText").objectReferenceValue = tabRelTxt;

            soView.FindProperty("_inventoryGridContainer").objectReferenceValue = inventoryGrid;

            soView.FindProperty("_primarySlotIcon").objectReferenceValue = priSlotIcon;
            soView.FindProperty("_primarySlotName").objectReferenceValue = priSlotName;

            var relicIconProp = soView.FindProperty("_relicSlotIcons");
            relicIconProp.arraySize = 3;
            for (int i = 0; i < 3; i++) relicIconProp.GetArrayElementAtIndex(i).objectReferenceValue = relicIcons[i];

            var relicNameProp = soView.FindProperty("_relicSlotNames");
            relicNameProp.arraySize = 3;
            for (int i = 0; i < 3; i++) relicNameProp.GetArrayElementAtIndex(i).objectReferenceValue = relicNames[i];

            soView.FindProperty("_detailIcon").objectReferenceValue = detailIcon;
            soView.FindProperty("_detailNameText").objectReferenceValue = detailName;
            soView.FindProperty("_detailTypeText").objectReferenceValue = detailType;
            soView.FindProperty("_detailDamageText").objectReferenceValue = detailDmg;
            soView.FindProperty("_detailCooldownText").objectReferenceValue = detailCd;
            soView.FindProperty("_damageFillBar").objectReferenceValue = dmgFill;
            soView.FindProperty("_cooldownFillBar").objectReferenceValue = cdFill;
            soView.FindProperty("_detailDescText").objectReferenceValue = detailDesc;

            soView.FindProperty("_startBattleButton").objectReferenceValue = startBattleBtn;
            soView.FindProperty("_backButton").objectReferenceValue = backBtn;
            soView.FindProperty("_modalContainer").objectReferenceValue = modalRT;
            soView.FindProperty("_dimBackgroundButton").objectReferenceValue = bgDimBtn;
            soView.ApplyModifiedProperties();

            // 7. Wire Presenter
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            // 8. Lưu Prefab
            string prefabPath = $"{prefabFolder}/WeaponLoadoutUI.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // 9. Auto-wire vào Scene nếu có Canvas_MetaMenu
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                var oldUI = GameObject.Find("Panel_WeaponLoadout");
                if (oldUI != null && oldUI != root) Object.DestroyImmediate(oldUI);

                var metaCanvas = GameObject.Find("Canvas_MetaMenu");
                Transform targetParent = metaCanvas != null ? metaCanvas.transform : canvas.transform;

                root.transform.SetParent(targetParent, false);
                SetStretchAnchor(rootRT);

                var metaMgr = Object.FindAnyObjectByType<MetaUIManager>();
                if (metaMgr != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaMgr);
                    soMeta.FindProperty("_weaponLoadoutScreen").objectReferenceValue = view;
                    soMeta.ApplyModifiedProperties();
                    EditorUtility.SetDirty(metaMgr);
                }

                Debug.Log($"<color=#00FF88>[WeaponLoadoutUIGenerator]</color> Đã tạo Prefab Tàng Bảo Các 2 Cột chuẩn ảnh thiết kế và kết nối thành công!");
            }
            else
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildHeader(Transform parent, TMP_FontAsset font, out TextMeshProUGUI heroName, out TextMeshProUGUI heroElem, out Image heroAvatar, out Button backBtn)
        {
            GameObject headerObj = CreateUIElement("Header_TopBar", parent);
            RectTransform hRT = headerObj.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 1);
            hRT.anchorMax = new Vector2(1, 1);
            hRT.pivot = new Vector2(0.5f, 1);
            hRT.anchoredPosition = new Vector2(0, 0);
            hRT.sizeDelta = new Vector2(0, 56);

            // 1. Hero Badge (Trái)
            GameObject badge = CreateUIElement("Badge_Hero", headerObj.transform);
            RectTransform bRT = badge.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(20, 0);
            bRT.sizeDelta = new Vector2(220, 42);

            GameObject avObj = CreateUIElement("Avatar", badge.transform);
            RectTransform avRT = avObj.GetComponent<RectTransform>();
            avRT.anchorMin = new Vector2(0, 0.5f);
            avRT.anchorMax = new Vector2(0, 0.5f);
            avRT.pivot = new Vector2(0, 0.5f);
            avRT.anchoredPosition = new Vector2(4, 0);
            avRT.sizeDelta = new Vector2(36, 36);
            heroAvatar = avObj.AddComponent<Image>();

            GameObject nameObj = CreateUIElement("Txt_HeroName", badge.transform);
            RectTransform nRT = nameObj.GetComponent<RectTransform>();
            nRT.anchoredPosition = new Vector2(46, 7);
            nRT.sizeDelta = new Vector2(160, 18);
            heroName = nameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroName.font = font;
            heroName.text = "ĐẠO SĨ";
            heroName.fontSize = 15;
            heroName.fontStyle = FontStyles.Bold;
            heroName.color = Color.white;

            GameObject elemObj = CreateUIElement("Txt_HeroElem", badge.transform);
            RectTransform eRT = elemObj.GetComponent<RectTransform>();
            eRT.anchoredPosition = new Vector2(46, -9);
            eRT.sizeDelta = new Vector2(160, 16);
            heroElem = elemObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroElem.font = font;
            heroElem.text = "<color=#4CAF50>Hệ Mộc</color>";
            heroElem.fontSize = 12;

            // 2. Title Header (Giữa)
            GameObject titleObj = CreateUIElement("Container_Title", headerObj.transform);
            RectTransform tRT = titleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 0.5f);
            tRT.anchorMax = new Vector2(0.5f, 0.5f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.anchoredPosition = Vector2.zero;
            tRT.sizeDelta = new Vector2(400, 48);

            GameObject mainT = CreateUIElement("Txt_MainTitle", titleObj.transform);
            RectTransform mtRT = mainT.GetComponent<RectTransform>();
            mtRT.anchoredPosition = new Vector2(0, 7);
            mtRT.sizeDelta = new Vector2(400, 24);
            var mtTMP = mainT.AddComponent<TextMeshProUGUI>();
            if (font != null) mtTMP.font = font;
            mtTMP.text = "TÀNG BẢO CÁC";
            mtTMP.fontSize = 20;
            mtTMP.fontStyle = FontStyles.Bold;
            mtTMP.alignment = TextAlignmentOptions.Center;
            mtTMP.color = ColorGold;

            GameObject subT = CreateUIElement("Txt_SubTitle", titleObj.transform);
            RectTransform stRT = subT.GetComponent<RectTransform>();
            stRT.anchoredPosition = new Vector2(0, -11);
            stRT.sizeDelta = new Vector2(400, 16);
            var stTMP = subT.AddComponent<TextMeshProUGUI>();
            if (font != null) stTMP.font = font;
            stTMP.text = "Kho Pháp Bảo";
            stTMP.fontSize = 12;
            stTMP.alignment = TextAlignmentOptions.Center;
            stTMP.color = ColorMutedText;

            // 3. Back Button (Phải)
            GameObject backObj = CreateUIElement("Btn_Back", headerObj.transform);
            RectTransform bkRT = backObj.GetComponent<RectTransform>();
            bkRT.anchorMin = new Vector2(1, 0.5f);
            bkRT.anchorMax = new Vector2(1, 0.5f);
            bkRT.pivot = new Vector2(1, 0.5f);
            bkRT.anchoredPosition = new Vector2(-20, 0);
            bkRT.sizeDelta = new Vector2(110, 36);
            var bkImg = backObj.AddComponent<Image>();
            bkImg.color = new Color(0.24f, 0.18f, 0.28f, 0.95f);
            backBtn = backObj.AddComponent<Button>();

            GameObject bkT = CreateUIElement("Text", backObj.transform);
            SetStretchAnchor(bkT.GetComponent<RectTransform>());
            var bkTMP = bkT.AddComponent<TextMeshProUGUI>();
            if (font != null) bkTMP.font = font;
            bkTMP.text = "< QUAY LẠI";
            bkTMP.fontSize = 13;
            bkTMP.fontStyle = FontStyles.Bold;
            bkTMP.alignment = TextAlignmentOptions.Center;
            bkTMP.color = Color.white;
        }

        private static void BuildLeftInventoryColumn(Transform parent, TMP_FontAsset font,
            out Button tabPriBtn, out Button tabRelBtn,
            out Image tabPriBg, out Image tabRelBg,
            out TextMeshProUGUI tabPriTxt, out TextMeshProUGUI tabRelTxt,
            out Transform inventoryGrid)
        {
            GameObject col = CreateUIElement("Col_LeftInventory", parent);
            var img = col.AddComponent<Image>();
            img.color = ColorCardBg;

            // Viền Khung Trúc Cổ bên ngoài
            GameObject borderObj = CreateUIElement("Border_Bamboo", col.transform);
            SetStretchAnchor(borderObj.GetComponent<RectTransform>());
            var bImg = borderObj.AddComponent<Image>();
            bImg.color = ColorBambooFrame;
            bImg.type = Image.Type.Sliced;

            // Container Nội Dung Bên Trong
            GameObject innerObj = CreateUIElement("Inner_Content", col.transform);
            RectTransform inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(8, 8);
            inRT.offsetMax = new Vector2(-8, -8);

            // 1. Tab Switcher (Top)
            GameObject tabContainer = CreateUIElement("Container_Tabs", innerObj.transform);
            RectTransform tcRT = tabContainer.GetComponent<RectTransform>();
            tcRT.anchorMin = new Vector2(0, 1);
            tcRT.anchorMax = new Vector2(1, 1);
            tcRT.pivot = new Vector2(0.5f, 1);
            tcRT.anchoredPosition = Vector2.zero;
            tcRT.sizeDelta = new Vector2(0, 42);

            var tcHlg = tabContainer.AddComponent<HorizontalLayoutGroup>();
            tcHlg.spacing = 10;
            tcHlg.childControlWidth = true;
            tcHlg.childControlHeight = true;
            tcHlg.childForceExpandWidth = true;
            tcHlg.childForceExpandHeight = true;

            // Tab 1: [ VŨ KHÍ CHÍNH ]
            GameObject t1 = CreateUIElement("Btn_TabPrimary", tabContainer.transform);
            tabPriBg = t1.AddComponent<Image>();
            tabPriBg.color = new Color(0.24f, 0.18f, 0.12f, 1f);
            tabPriBtn = t1.AddComponent<Button>();

            GameObject t1Txt = CreateUIElement("Text", t1.transform);
            SetStretchAnchor(t1Txt.GetComponent<RectTransform>());
            tabPriTxt = t1Txt.AddComponent<TextMeshProUGUI>();
            if (font != null) tabPriTxt.font = font;
            tabPriTxt.text = "[ VŨ KHÍ CHÍNH ]";
            tabPriTxt.fontSize = 14;
            tabPriTxt.fontStyle = FontStyles.Bold;
            tabPriTxt.alignment = TextAlignmentOptions.Center;
            tabPriTxt.color = ColorGold;

            // Tab 2: [ PHÁP BẢO ]
            GameObject t2 = CreateUIElement("Btn_TabRelics", tabContainer.transform);
            tabRelBg = t2.AddComponent<Image>();
            tabRelBg.color = new Color(0.12f, 0.10f, 0.16f, 0.9f);
            tabRelBtn = t2.AddComponent<Button>();

            GameObject t2Txt = CreateUIElement("Text", t2.transform);
            SetStretchAnchor(t2Txt.GetComponent<RectTransform>());
            tabRelTxt = t2Txt.AddComponent<TextMeshProUGUI>();
            if (font != null) tabRelTxt.font = font;
            tabRelTxt.text = "[ PHÁP BẢO ]";
            tabRelTxt.fontSize = 14;
            tabRelTxt.fontStyle = FontStyles.Bold;
            tabRelTxt.alignment = TextAlignmentOptions.Center;
            tabRelTxt.color = ColorMutedText;

            // 2. Grid 12 Ô (4 Cột x 3 Hàng)
            GameObject gridObj = CreateUIElement("Grid_Inventory12Slots", innerObj.transform);
            RectTransform gRT = gridObj.GetComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0, 0);
            gRT.anchorMax = new Vector2(1, 1);
            gRT.offsetMin = new Vector2(12, 10);
            gRT.offsetMax = new Vector2(-12, -52);

            var glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(106, 150);
            glg.spacing = new Vector2(14, 12);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            glg.childAlignment = TextAnchor.UpperCenter;
            inventoryGrid = gridObj.transform;
        }

        private static void BuildRightLoadoutColumn(Transform parent, TMP_FontAsset font,
            out Image priSlotIcon, out TextMeshProUGUI priSlotName,
            out Image[] relicIcons, out TextMeshProUGUI[] relicNames,
            out Image detailIcon, out TextMeshProUGUI detailName, out TextMeshProUGUI detailType,
            out TextMeshProUGUI detailDmg, out TextMeshProUGUI detailCd,
            out Image dmgFill, out Image cdFill, out TextMeshProUGUI detailDesc,
            out Button startBattleBtn)
        {
            GameObject col = CreateUIElement("Col_RightLoadout", parent);
            var img = col.AddComponent<Image>();
            img.color = ColorCardBg;

            // Viền Khung Trúc Cổ bên ngoài
            GameObject borderObj = CreateUIElement("Border_Bamboo", col.transform);
            SetStretchAnchor(borderObj.GetComponent<RectTransform>());
            var bImg = borderObj.AddComponent<Image>();
            bImg.color = ColorBambooFrame;
            bImg.type = Image.Type.Sliced;

            // Container Nội Dung Phải
            GameObject innerObj = CreateUIElement("Inner_Content", col.transform);
            RectTransform inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(12, 10);
            inRT.offsetMax = new Vector2(-12, -10);

            var vlg = innerObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ================= SECTION 1: TRANG BỊ ĐÃ CHỌN (LOADOUT) (Height = 160) =================
            GameObject s1 = CreateUIElement("Section_Loadout", innerObj.transform);
            s1.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 160);

            GameObject s1Title = CreateUIElement("Txt_Title", s1.transform);
            RectTransform s1tRT = s1Title.GetComponent<RectTransform>();
            s1tRT.anchorMin = new Vector2(0, 1);
            s1tRT.anchorMax = new Vector2(1, 1);
            s1tRT.pivot = new Vector2(0.5f, 1);
            s1tRT.anchoredPosition = new Vector2(0, -4);
            s1tRT.sizeDelta = new Vector2(0, 20);
            var s1TMP = s1Title.AddComponent<TextMeshProUGUI>();
            if (font != null) s1TMP.font = font;
            s1TMP.text = "TRANG BỊ ĐÃ CHỌN (LOADOUT)";
            s1TMP.fontSize = 14;
            s1TMP.fontStyle = FontStyles.Bold;
            s1TMP.alignment = TextAlignmentOptions.Center;
            s1TMP.color = Color.white;

            // Hàng 4 Ô Trang Bị
            GameObject slotsRow = CreateUIElement("Row_Slots", s1.transform);
            RectTransform srRT = slotsRow.GetComponent<RectTransform>();
            srRT.anchorMin = new Vector2(0, 0);
            srRT.anchorMax = new Vector2(1, 1);
            srRT.offsetMin = new Vector2(10, 0);
            srRT.offsetMax = new Vector2(-10, -26);

            var srHlg = slotsRow.AddComponent<HorizontalLayoutGroup>();
            srHlg.spacing = 16;
            srHlg.childAlignment = TextAnchor.MiddleCenter;
            srHlg.childControlWidth = false;
            srHlg.childControlHeight = false;
            srHlg.childForceExpandWidth = false;
            srHlg.childForceExpandHeight = false;

            // Ô 1: Vũ Khí Chính (Lục Giác Vàng 88x88)
            GameObject pSlot = CreateUIElement("Slot_PrimaryHex", slotsRow.transform);
            pSlot.GetComponent<RectTransform>().sizeDelta = new Vector2(96, 120);

            GameObject pHexFrame = CreateUIElement("HexFrame", pSlot.transform);
            RectTransform phfRT = pHexFrame.GetComponent<RectTransform>();
            phfRT.anchorMin = new Vector2(0.5f, 1);
            phfRT.anchorMax = new Vector2(0.5f, 1);
            phfRT.pivot = new Vector2(0.5f, 1);
            phfRT.anchoredPosition = Vector2.zero;
            phfRT.sizeDelta = new Vector2(88, 88);
            var phfImg = pHexFrame.AddComponent<Image>();
            phfImg.color = ColorGold;

            GameObject pInner = CreateUIElement("Inner", pHexFrame.transform);
            SetStretchAnchor(pInner.GetComponent<RectTransform>());
            pInner.GetComponent<RectTransform>().offsetMin = new Vector2(4, 4);
            pInner.GetComponent<RectTransform>().offsetMax = new Vector2(-4, -4);
            pInner.AddComponent<Image>().color = ColorCardInner;

            GameObject pIconObj = CreateUIElement("Icon", pInner.transform);
            SetStretchAnchor(pIconObj.GetComponent<RectTransform>());
            pIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(6, 6);
            pIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-6, -6);
            priSlotIcon = pIconObj.AddComponent<Image>();
            priSlotIcon.preserveAspect = true;

            GameObject pLbl = CreateUIElement("Txt_Name", pSlot.transform);
            RectTransform plRT = pLbl.GetComponent<RectTransform>();
            plRT.anchorMin = new Vector2(0, 0);
            plRT.anchorMax = new Vector2(1, 0);
            plRT.pivot = new Vector2(0.5f, 0);
            plRT.anchoredPosition = Vector2.zero;
            plRT.sizeDelta = new Vector2(0, 24);
            priSlotName = pLbl.AddComponent<TextMeshProUGUI>();
            if (font != null) priSlotName.font = font;
            priSlotName.text = "Nỏ Thần";
            priSlotName.fontSize = 11;
            priSlotName.fontStyle = FontStyles.Bold;
            priSlotName.alignment = TextAlignmentOptions.Center;
            priSlotName.color = ColorGold;

            // Ô 2, 3, 4: 3 Pháp Bảo Hộ Thân
            relicIcons = new Image[3];
            relicNames = new TextMeshProUGUI[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject rSlot = CreateUIElement($"Slot_Relic_{i + 1}", slotsRow.transform);
                rSlot.GetComponent<RectTransform>().sizeDelta = new Vector2(96, 120);

                GameObject rBox = CreateUIElement("Box", rSlot.transform);
                RectTransform rbRT = rBox.GetComponent<RectTransform>();
                rbRT.anchorMin = new Vector2(0.5f, 1);
                rbRT.anchorMax = new Vector2(0.5f, 1);
                rbRT.pivot = new Vector2(0.5f, 1);
                rbRT.anchoredPosition = Vector2.zero;
                rbRT.sizeDelta = new Vector2(80, 80);
                rBox.AddComponent<Image>().color = new Color(0.25f, 0.65f, 0.95f, 1f); // Xanh Lam Thủy mẫu

                GameObject rInner = CreateUIElement("Inner", rBox.transform);
                SetStretchAnchor(rInner.GetComponent<RectTransform>());
                rInner.GetComponent<RectTransform>().offsetMin = new Vector2(3, 3);
                rInner.GetComponent<RectTransform>().offsetMax = new Vector2(-3, -3);
                rInner.AddComponent<Image>().color = ColorCardInner;

                GameObject rIconObj = CreateUIElement("Icon", rInner.transform);
                SetStretchAnchor(rIconObj.GetComponent<RectTransform>());
                rIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(6, 6);
                rIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-6, -6);
                relicIcons[i] = rIconObj.AddComponent<Image>();
                relicIcons[i].preserveAspect = true;

                GameObject rLbl = CreateUIElement("Txt_Name", rSlot.transform);
                RectTransform rlRT = rLbl.GetComponent<RectTransform>();
                rlRT.anchorMin = new Vector2(0, 0);
                rlRT.anchorMax = new Vector2(1, 0);
                rlRT.pivot = new Vector2(0.5f, 0);
                rlRT.anchoredPosition = Vector2.zero;
                rlRT.sizeDelta = new Vector2(0, 24);
                relicNames[i] = rLbl.AddComponent<TextMeshProUGUI>();
                if (font != null) relicNames[i].font = font;
                relicNames[i].text = (i == 0) ? "Bùa Trấn Yêu" : ((i == 1) ? "Trống Đồng" : "Khóa");
                relicNames[i].fontSize = 11;
                relicNames[i].alignment = TextAlignmentOptions.Center;
                relicNames[i].color = ColorMutedText;
            }

            // Thanh Ngăn Cách 1 (Divider)
            GameObject div1 = CreateUIElement("Divider_1", innerObj.transform);
            div1.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 4);
            div1.AddComponent<Image>().color = ColorBambooFrame;

            // ================= SECTION 2: SOI CHI TIẾT PHÁP BẢO (Height = 220) =================
            GameObject s2 = CreateUIElement("Section_Detail", innerObj.transform);
            s2.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 220);

            // Khung Icon Lớn Bên Trái (92x92)
            GameObject dFrame = CreateUIElement("Frame_Icon", s2.transform);
            RectTransform dfRT = dFrame.GetComponent<RectTransform>();
            dfRT.anchorMin = new Vector2(0, 0.5f);
            dfRT.anchorMax = new Vector2(0, 0.5f);
            dfRT.pivot = new Vector2(0, 0.5f);
            dfRT.anchoredPosition = new Vector2(14, 0);
            dfRT.sizeDelta = new Vector2(96, 96);
            dFrame.AddComponent<Image>().color = ColorGold;

            GameObject dInner = CreateUIElement("Inner", dFrame.transform);
            SetStretchAnchor(dInner.GetComponent<RectTransform>());
            dInner.GetComponent<RectTransform>().offsetMin = new Vector2(4, 4);
            dInner.GetComponent<RectTransform>().offsetMax = new Vector2(-4, -4);
            dInner.AddComponent<Image>().color = ColorCardInner;

            GameObject dIconObj = CreateUIElement("Icon", dInner.transform);
            SetStretchAnchor(dIconObj.GetComponent<RectTransform>());
            detailIcon = dIconObj.AddComponent<Image>();
            detailIcon.preserveAspect = true;

            // Cụm Thông Tin Bên Phải (Tên + Vai Trò + Hệ + Stat Bars + Mô Tả)
            GameObject dInfo = CreateUIElement("Info_Block", s2.transform);
            RectTransform diRT = dInfo.GetComponent<RectTransform>();
            diRT.anchorMin = new Vector2(0, 0);
            diRT.anchorMax = new Vector2(1, 1);
            diRT.offsetMin = new Vector2(125, 0);
            diRT.offsetMax = new Vector2(-10, 0);

            // Tên Vũ Khí
            GameObject dnObj = CreateUIElement("Txt_WeaponName", dInfo.transform);
            RectTransform dnRT = dnObj.GetComponent<RectTransform>();
            dnRT.anchorMin = new Vector2(0, 1);
            dnRT.anchorMax = new Vector2(1, 1);
            dnRT.pivot = new Vector2(0, 1);
            dnRT.anchoredPosition = new Vector2(0, -6);
            dnRT.sizeDelta = new Vector2(0, 24);
            detailName = dnObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailName.font = font;
            detailName.text = "Nỏ Thần";
            detailName.fontSize = 19;
            detailName.fontStyle = FontStyles.Bold;
            detailName.color = ColorGold;

            // Vai Trò & Hệ
            GameObject dtObj = CreateUIElement("Txt_RoleAndElement", dInfo.transform);
            RectTransform dtRT = dtObj.GetComponent<RectTransform>();
            dtRT.anchorMin = new Vector2(0, 1);
            dtRT.anchorMax = new Vector2(1, 1);
            dtRT.pivot = new Vector2(0, 1);
            dtRT.anchoredPosition = new Vector2(0, -32);
            dtRT.sizeDelta = new Vector2(0, 36);
            detailType = dtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailType.font = font;
            detailType.text = "<color=#FF8800>[VŨ KHÍ CHÍNH] (ĐÁNH TAY COMBO)</color>\nHệ <color=#FFD700>Kim</color>";
            detailType.fontSize = 12;

            // Stat Bars Row (Sát Thương Đỏ & Hồi Chiêu Lam)
            GameObject statRow = CreateUIElement("Row_StatBars", dInfo.transform);
            RectTransform stRT = statRow.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0, 1);
            stRT.anchorMax = new Vector2(1, 1);
            stRT.pivot = new Vector2(0, 1);
            stRT.anchoredPosition = new Vector2(0, -74);
            stRT.sizeDelta = new Vector2(0, 42);

            // 1. Sát Thương
            GameObject dmgCol = CreateUIElement("Col_Damage", statRow.transform);
            RectTransform dcRT = dmgCol.GetComponent<RectTransform>();
            dcRT.anchorMin = new Vector2(0, 0);
            dcRT.anchorMax = new Vector2(0.48f, 1);
            dcRT.offsetMin = Vector2.zero;
            dcRT.offsetMax = Vector2.zero;

            GameObject dmgTxtObj = CreateUIElement("Txt", dmgCol.transform);
            RectTransform dtRT2 = dmgTxtObj.GetComponent<RectTransform>();
            dtRT2.anchorMin = new Vector2(0, 1);
            dtRT2.anchorMax = new Vector2(1, 1);
            dtRT2.pivot = new Vector2(0, 1);
            dtRT2.anchoredPosition = Vector2.zero;
            dtRT2.sizeDelta = new Vector2(0, 18);
            detailDmg = dmgTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailDmg.font = font;
            detailDmg.text = "Sát thương: <color=#FF5555>12</color>";
            detailDmg.fontSize = 12;

            GameObject dmgBg = CreateUIElement("Bar_Bg", dmgCol.transform);
            RectTransform dbRT = dmgBg.GetComponent<RectTransform>();
            dbRT.anchorMin = new Vector2(0, 0);
            dbRT.anchorMax = new Vector2(1, 0);
            dbRT.pivot = new Vector2(0, 0);
            dbRT.anchoredPosition = new Vector2(0, 4);
            dbRT.sizeDelta = new Vector2(0, 10);
            dmgBg.AddComponent<Image>().color = new Color(0.2f, 0.1f, 0.1f, 1f);

            GameObject dmgFillObj = CreateUIElement("Bar_Fill", dmgBg.transform);
            SetStretchAnchor(dmgFillObj.GetComponent<RectTransform>());
            dmgFill = dmgFillObj.AddComponent<Image>();
            dmgFill.color = new Color(0.95f, 0.25f, 0.20f, 1f);
            dmgFill.type = Image.Type.Filled;
            dmgFill.fillMethod = Image.FillMethod.Horizontal;
            dmgFill.fillAmount = 0.65f;

            // 2. Hồi Chiêu
            GameObject cdCol = CreateUIElement("Col_Cooldown", statRow.transform);
            RectTransform ccRT = cdCol.GetComponent<RectTransform>();
            ccRT.anchorMin = new Vector2(0.52f, 0);
            ccRT.anchorMax = new Vector2(1, 1);
            ccRT.offsetMin = Vector2.zero;
            ccRT.offsetMax = Vector2.zero;

            GameObject cdTxtObj = CreateUIElement("Txt", cdCol.transform);
            RectTransform ctRT = cdTxtObj.GetComponent<RectTransform>();
            ctRT.anchorMin = new Vector2(0, 1);
            ctRT.anchorMax = new Vector2(1, 1);
            ctRT.pivot = new Vector2(0, 1);
            ctRT.anchoredPosition = Vector2.zero;
            ctRT.sizeDelta = new Vector2(0, 18);
            detailCd = cdTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailCd.font = font;
            detailCd.text = "Hồi chiêu: <color=#4DEEEA>0.6s</color>";
            detailCd.fontSize = 12;

            GameObject cdBg = CreateUIElement("Bar_Bg", cdCol.transform);
            RectTransform cbRT = cdBg.GetComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(0, 0);
            cbRT.anchorMax = new Vector2(1, 0);
            cbRT.pivot = new Vector2(0, 0);
            cbRT.anchoredPosition = new Vector2(0, 4);
            cbRT.sizeDelta = new Vector2(0, 10);
            cdBg.AddComponent<Image>().color = new Color(0.1f, 0.15f, 0.2f, 1f);

            GameObject cdFillObj = CreateUIElement("Bar_Fill", cdBg.transform);
            SetStretchAnchor(cdFillObj.GetComponent<RectTransform>());
            cdFill = cdFillObj.AddComponent<Image>();
            cdFill.color = new Color(0.25f, 0.85f, 0.95f, 1f);
            cdFill.type = Image.Type.Filled;
            cdFill.fillMethod = Image.FillMethod.Horizontal;
            cdFill.fillAmount = 0.80f;

            // Mô Tả
            GameObject descObj = CreateUIElement("Txt_Description", dInfo.transform);
            RectTransform ddRT = descObj.GetComponent<RectTransform>();
            ddRT.anchorMin = new Vector2(0, 0);
            ddRT.anchorMax = new Vector2(1, 1);
            ddRT.offsetMin = new Vector2(0, 4);
            ddRT.offsetMax = new Vector2(0, -125);
            detailDesc = descObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailDesc.font = font;
            detailDesc.text = "Mũi tên thần An Dương Vương bắn thẳng xuyên táo 2 kẻ địch.";
            detailDesc.fontSize = 12;
            detailDesc.color = ColorMutedText;
            detailDesc.enableWordWrapping = true;

            // Thanh Ngăn Cách 2 (Divider)
            GameObject div2 = CreateUIElement("Divider_2", innerObj.transform);
            div2.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 4);
            div2.AddComponent<Image>().color = ColorBambooFrame;

            // ================= SECTION 3: NÚT XÁC NHẬN XUẤT TRẬN (Height = 60) =================
            GameObject btnObj = CreateUIElement("Btn_StartBattle", innerObj.transform);
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 58);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = ColorCinnabar;
            startBattleBtn = btnObj.AddComponent<Button>();

            // Viền vàng nút
            GameObject btnBorder = CreateUIElement("Border", btnObj.transform);
            SetStretchAnchor(btnBorder.GetComponent<RectTransform>());
            btnBorder.GetComponent<RectTransform>().offsetMin = new Vector2(2, 2);
            btnBorder.GetComponent<RectTransform>().offsetMax = new Vector2(-2, -2);
            var bbImg = btnBorder.AddComponent<Image>();
            bbImg.color = ColorGold;

            GameObject btnInner = CreateUIElement("Inner", btnBorder.transform);
            SetStretchAnchor(btnInner.GetComponent<RectTransform>());
            btnInner.GetComponent<RectTransform>().offsetMin = new Vector2(2, 2);
            btnInner.GetComponent<RectTransform>().offsetMax = new Vector2(-2, -2);
            btnInner.AddComponent<Image>().color = new Color(0.72f, 0.18f, 0.14f, 1f);

            GameObject btnTxt = CreateUIElement("Text", btnInner.transform);
            SetStretchAnchor(btnTxt.GetComponent<RectTransform>());
            var btTMP = btnTxt.AddComponent<TextMeshProUGUI>();
            if (font != null) btTMP.font = font;
            btTMP.text = "XÁC NHẬN XUẤT TRẬN";
            btTMP.fontSize = 20;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.alignment = TextAlignmentOptions.Center;
            btTMP.color = Color.white;
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
    }
}
#endif

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
            modalImg.color = Color.white;
            modalImg.type = Image.Type.Sliced;
            Sprite modalFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Modal_TangBaoCac_9Slice.png");
            if (modalFrame != null) modalImg.sprite = modalFrame;

            // 4. Header Section
            BuildHeader(modal.transform, vietFont, out TextMeshProUGUI heroNameTMP, out TextMeshProUGUI heroElemTMP, out Image heroAvatarImg, out Button backBtn);

            // 5. Body Section - 2 Cột Đối Xứng
            GameObject bodyObj = CreateUIElement("Container_Body2Cols", modal.transform);
            RectTransform bodyRT = bodyObj.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0, 0);
            bodyRT.anchorMax = new Vector2(1, 1);
            bodyRT.offsetMin = new Vector2(28, 24);
            bodyRT.offsetMax = new Vector2(-28, -72); // 72px cho Header

            var bodyHlg = bodyObj.AddComponent<HorizontalLayoutGroup>();
            bodyHlg.spacing = 20;
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
            hRT.anchoredPosition = new Vector2(0, 8);
            hRT.sizeDelta = new Vector2(0, 72);

            // 1. Cuộn Giấy Da Tiêu Đề (Giữa): TÀNG BẢO CÁC - Kho Pháp Bảo
            GameObject titleObj = CreateUIElement("Banner_Title", headerObj.transform);
            RectTransform tRT = titleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 0.5f);
            tRT.anchorMax = new Vector2(0.5f, 0.5f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.anchoredPosition = new Vector2(0, 4);
            tRT.sizeDelta = new Vector2(440, 84);

            var tImg = titleObj.AddComponent<Image>();
            tImg.color = Color.white;
            tImg.type = Image.Type.Sliced;
            Sprite bannerScroll = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Banner_Parchment_Scroll.png");
            if (bannerScroll != null) tImg.sprite = bannerScroll;

            GameObject mainT = CreateUIElement("Txt_MainTitle", titleObj.transform);
            RectTransform mtRT = mainT.GetComponent<RectTransform>();
            mtRT.anchoredPosition = new Vector2(0, 10);
            mtRT.sizeDelta = new Vector2(360, 24);
            var mtTMP = mainT.AddComponent<TextMeshProUGUI>();
            if (font != null) mtTMP.font = font;
            mtTMP.text = "- TÀNG BẢO CÁC -";
            mtTMP.fontSize = 20;
            mtTMP.fontStyle = FontStyles.Bold;
            mtTMP.alignment = TextAlignmentOptions.Center;
            mtTMP.color = new Color(0.18f, 0.12f, 0.08f, 1f);

            GameObject subT = CreateUIElement("Txt_SubTitle", titleObj.transform);
            RectTransform stRT = subT.GetComponent<RectTransform>();
            stRT.anchoredPosition = new Vector2(0, -12);
            stRT.sizeDelta = new Vector2(360, 16);
            var stTMP = subT.AddComponent<TextMeshProUGUI>();
            if (font != null) stTMP.font = font;
            stTMP.text = "Kho Pháp Bảo";
            stTMP.fontSize = 12;
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.alignment = TextAlignmentOptions.Center;
            stTMP.color = new Color(0.35f, 0.25f, 0.18f, 1f);

            // 2. Nút Đóng / Thoát Gỗ Mun Cổ (Phải) (Btn_Close)
            GameObject closeObj = CreateUIElement("Btn_Close", headerObj.transform);
            RectTransform closeRT = closeObj.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1, 0.5f);
            closeRT.anchorMax = new Vector2(1, 0.5f);
            closeRT.pivot = new Vector2(1, 0.5f);
            closeRT.anchoredPosition = new Vector2(-16, 2);
            closeRT.sizeDelta = new Vector2(46, 46);
            var closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.white;
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Close_X_Wood.png");
            if (btnCloseX != null)
            {
                closeImg.sprite = btnCloseX;
                closeImg.preserveAspect = true;
            }
            backBtn = closeObj.AddComponent<Button>();
            var closeColors = backBtn.colors;
            closeColors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            closeColors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            backBtn.colors = closeColors;

            heroName = null;
            heroElem = null;
            heroAvatar = null;
        }

        private static void BuildLeftInventoryColumn(Transform parent, TMP_FontAsset font,
            out Button tabPriBtn, out Button tabRelBtn,
            out Image tabPriBg, out Image tabRelBg,
            out TextMeshProUGUI tabPriTxt, out TextMeshProUGUI tabRelTxt,
            out Transform inventoryGrid)
        {
            GameObject col = CreateUIElement("Col_LeftInventory", parent);
            var img = col.AddComponent<Image>();
            img.color = new Color(0.18f, 0.12f, 0.08f, 0.65f); // Nền gỗ tối bên trái
            img.type = Image.Type.Sliced;

            // Container Nội Dung Bên Trong
            GameObject innerObj = CreateUIElement("Inner_Content", col.transform);
            RectTransform inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(10, 10);
            inRT.offsetMax = new Vector2(-10, -10);

            // Tab không cần hiển thị vì chỉ có 1 kho duy nhất
            GameObject tabContainer = CreateUIElement("Container_Tabs", innerObj.transform);
            tabPriBg = tabContainer.AddComponent<Image>();
            tabPriBg.enabled = false;
            tabPriBtn = tabContainer.AddComponent<Button>();
            tabPriTxt = new GameObject().AddComponent<TextMeshProUGUI>();
            tabRelBg = new GameObject().AddComponent<Image>();
            tabRelBtn = new GameObject().AddComponent<Button>();
            tabRelTxt = new GameObject().AddComponent<TextMeshProUGUI>();
            tabContainer.SetActive(false);

            // 2. Grid 20 Ô Pháp Bảo (4x5)
            GameObject gridObj = CreateUIElement("Grid_Inventory12Slots", innerObj.transform);
            RectTransform gRT = gridObj.GetComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0, 0);
            gRT.anchorMax = new Vector2(1, 1);
            gRT.offsetMin = new Vector2(6, 6);
            gRT.offsetMax = new Vector2(-6, -6);

            var glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(104, 104);
            glg.spacing = new Vector2(8, 8);
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
            img.color = Color.white;
            img.type = Image.Type.Sliced;
            Sprite colBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");
            if (colBg != null) img.sprite = colBg;

            // Container Nội Dung Phải
            GameObject innerObj = CreateUIElement("Inner_Content", col.transform);
            RectTransform inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(16, 14);
            inRT.offsetMax = new Vector2(-16, -14);

            var vlg = innerObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            Sprite slotWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            Sprite slotSelected = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
            Sprite iconOrb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Weapon_Orb_Gold.png");
            Sprite gaugeFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Gauge_Stat_Bar_Frame.png");
            Sprite gaugeDmg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Gauge_Stat_Fill_Damage.png");
            Sprite gaugeCd = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Gauge_Stat_Fill_Cooldown.png");
            Sprite btnBattleAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");

            // ================= SECTION 1: KHAY 2 TRANG BỊ XUẤT TRẬN =================
            GameObject s1 = CreateUIElement("Section_Loadout", innerObj.transform);
            s1.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);

            // Nền khay gỗ mun cho 2 slot
            var s1Img = s1.AddComponent<Image>();
            s1Img.color = new Color(0.16f, 0.11f, 0.08f, 0.60f); // Gỗ trầm làm nền tách biệt
            s1Img.type = Image.Type.Sliced;
            Sprite woodTray = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Tray_Loadout_Wood_Frame.png");
            if (woodTray == null) woodTray = slotWood;
            if (woodTray != null) s1Img.sprite = woodTray;

            // Hàng 2 Ô: [Vũ Khí Chính] ⇄ [Pháp Bảo Xuất Trận]
            GameObject slotsRow = CreateUIElement("Row_Slots", s1.transform);
            SetStretchAnchor(slotsRow.GetComponent<RectTransform>());
            slotsRow.GetComponent<RectTransform>().offsetMin = new Vector2(10, 6);
            slotsRow.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -6);

            var srHlg = slotsRow.AddComponent<HorizontalLayoutGroup>();
            srHlg.spacing = 20;
            srHlg.childAlignment = TextAnchor.MiddleCenter;
            srHlg.childControlWidth = false;
            srHlg.childControlHeight = false;

            // Ô 1: Vũ Khí Bản Mệnh
            GameObject pSlot = CreateUIElement("Slot_PrimaryHex", slotsRow.transform);
            pSlot.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 100);

            GameObject pTag = CreateUIElement("Txt_Tag", pSlot.transform);
            RectTransform ptRT = pTag.GetComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0, 1);
            ptRT.anchorMax = new Vector2(1, 1);
            ptRT.pivot = new Vector2(0.5f, 1);
            ptRT.anchoredPosition = Vector2.zero;
            ptRT.sizeDelta = new Vector2(0, 18);
            var ptTMP = pTag.AddComponent<TextMeshProUGUI>();
            if (font != null) ptTMP.font = font;
            ptTMP.text = "VŨ KHÍ CHÍNH";
            ptTMP.fontSize = 11;
            ptTMP.fontStyle = FontStyles.Bold;
            ptTMP.alignment = TextAlignmentOptions.Center;
            ptTMP.color = new Color(0.95f, 0.80f, 0.45f, 1f); // Vàng kim

            GameObject pHexFrame = CreateUIElement("HexFrame", pSlot.transform);
            RectTransform phfRT = pHexFrame.GetComponent<RectTransform>();
            phfRT.anchorMin = new Vector2(0.5f, 0);
            phfRT.anchorMax = new Vector2(0.5f, 0);
            phfRT.pivot = new Vector2(0.5f, 0);
            phfRT.anchoredPosition = new Vector2(0, 20);
            phfRT.sizeDelta = new Vector2(58, 58);
            var phfImg = pHexFrame.AddComponent<Image>();
            phfImg.color = Color.white;
            phfImg.type = Image.Type.Sliced;
            if (slotWood != null) phfImg.sprite = slotWood;

            GameObject pIconObj = CreateUIElement("Icon", pHexFrame.transform);
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
            plRT.sizeDelta = new Vector2(0, 18);
            priSlotName = pLbl.AddComponent<TextMeshProUGUI>();
            if (font != null) priSlotName.font = font;
            priSlotName.text = "Trảm Yêu Trừ Ma Kiếm";
            priSlotName.fontSize = 11;
            priSlotName.fontStyle = FontStyles.Bold;
            priSlotName.alignment = TextAlignmentOptions.Center;
            priSlotName.color = new Color(0.90f, 0.85f, 0.78f, 1f);

            // Icon Divider ⇄ ở giữa
            GameObject divObj = CreateUIElement("Icon_Divider", slotsRow.transform);
            divObj.GetComponent<RectTransform>().sizeDelta = new Vector2(28, 28);
            var divTMP = divObj.AddComponent<TextMeshProUGUI>();
            if (font != null) divTMP.font = font;
            divTMP.text = "⇄";
            divTMP.fontSize = 20;
            divTMP.alignment = TextAlignmentOptions.Center;
            divTMP.color = new Color(0.95f, 0.75f, 0.20f, 0.85f);

            // Ô 2: 1 Pháp Bảo Hộ Thân
            relicIcons = new Image[3];
            relicNames = new TextMeshProUGUI[3];

            GameObject rSlot = CreateUIElement("Slot_Relic_1", slotsRow.transform);
            rSlot.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 100);

            GameObject rTag = CreateUIElement("Txt_Tag", rSlot.transform);
            RectTransform rtRT = rTag.GetComponent<RectTransform>();
            rtRT.anchorMin = new Vector2(0, 1);
            rtRT.anchorMax = new Vector2(1, 1);
            rtRT.pivot = new Vector2(0.5f, 1);
            rtRT.anchoredPosition = Vector2.zero;
            rtRT.sizeDelta = new Vector2(0, 18);
            var rtTMP = rTag.AddComponent<TextMeshProUGUI>();
            if (font != null) rtTMP.font = font;
            rtTMP.text = "PHÁP BẢO XUẤT TRẬN";
            rtTMP.fontSize = 11;
            rtTMP.fontStyle = FontStyles.Bold;
            rtTMP.alignment = TextAlignmentOptions.Center;
            rtTMP.color = new Color(0.95f, 0.80f, 0.45f, 1f);

            GameObject rBox = CreateUIElement("Box", rSlot.transform);
            RectTransform rbRT = rBox.GetComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(0.5f, 0);
            rbRT.anchorMax = new Vector2(0.5f, 0);
            rbRT.pivot = new Vector2(0.5f, 0);
            rbRT.anchoredPosition = new Vector2(0, 20);
            rbRT.sizeDelta = new Vector2(58, 58);
            var rBoxImg = rBox.AddComponent<Image>();
            rBoxImg.color = Color.white;
            rBoxImg.type = Image.Type.Sliced;
            if (slotSelected != null) rBoxImg.sprite = slotSelected;

            GameObject rIconObj = CreateUIElement("Icon", rBox.transform);
            SetStretchAnchor(rIconObj.GetComponent<RectTransform>());
            rIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(6, 6);
            rIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-6, -6);
            relicIcons[0] = rIconObj.AddComponent<Image>();
            relicIcons[0].preserveAspect = true;

            GameObject rLbl = CreateUIElement("Txt_Name", rSlot.transform);
            RectTransform rlRT = rLbl.GetComponent<RectTransform>();
            rlRT.anchorMin = new Vector2(0, 0);
            rlRT.anchorMax = new Vector2(1, 0);
            rlRT.pivot = new Vector2(0.5f, 0);
            rlRT.anchoredPosition = Vector2.zero;
            rlRT.sizeDelta = new Vector2(0, 18);
            relicNames[0] = rLbl.AddComponent<TextMeshProUGUI>();
            if (font != null) relicNames[0].font = font;
            relicNames[0].text = "Bùa Trấn Yêu";
            relicNames[0].fontSize = 11;
            relicNames[0].fontStyle = FontStyles.Bold;
            relicNames[0].alignment = TextAlignmentOptions.Center;
            relicNames[0].color = new Color(0.90f, 0.85f, 0.78f, 1f);

            for (int i = 1; i < 3; i++) { relicIcons[i] = new GameObject().AddComponent<Image>(); relicNames[i] = new GameObject().AddComponent<TextMeshProUGUI>(); }

            // ================= SECTION 2: SOI CHI TIẾT PHÁP BẢO (Giấy Da Cao Cấp) =================
            GameObject s2 = CreateUIElement("Section_Detail", innerObj.transform);
            s2.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 260);

            // Khung Nền Thẻ Chi Tiết Bo Góc (Gỗ Mờ)
            var s2Img = s2.AddComponent<Image>();
            s2Img.color = new Color(0.96f, 0.93f, 0.87f, 0.95f); // Giấy sớ sáng màu sắc nét
            s2Img.type = Image.Type.Sliced;
            Sprite detailBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");
            if (detailBg != null) s2Img.sprite = detailBg;

            // Container Nội Dung Soi
            GameObject dContent = CreateUIElement("Detail_Content", s2.transform);
            SetStretchAnchor(dContent.GetComponent<RectTransform>());
            dContent.GetComponent<RectTransform>().offsetMin = new Vector2(16, 12);
            dContent.GetComponent<RectTransform>().offsetMax = new Vector2(-16, -12);

            // 1. Header Tên & Tag
            GameObject dHeader = CreateUIElement("Header_Weapon", dContent.transform);
            RectTransform dhRT = dHeader.GetComponent<RectTransform>();
            dhRT.anchorMin = new Vector2(0, 1);
            dhRT.anchorMax = new Vector2(1, 1);
            dhRT.pivot = new Vector2(0.5f, 1);
            dhRT.anchoredPosition = Vector2.zero;
            dhRT.sizeDelta = new Vector2(0, 48);

            GameObject dnObj = CreateUIElement("Txt_WeaponName", dHeader.transform);
            RectTransform dnRT = dnObj.GetComponent<RectTransform>();
            dnRT.anchorMin = new Vector2(0, 1);
            dnRT.anchorMax = new Vector2(1, 1);
            dnRT.pivot = new Vector2(0, 1);
            dnRT.anchoredPosition = Vector2.zero;
            dnRT.sizeDelta = new Vector2(0, 26);
            detailName = dnObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailName.font = font;
            detailName.text = "Bùa Trấn Yêu";
            detailName.fontSize = 20;
            detailName.fontStyle = FontStyles.Bold;
            detailName.color = new Color(0.18f, 0.10f, 0.05f, 1f);

            GameObject dtObj = CreateUIElement("Txt_RoleAndElement", dHeader.transform);
            RectTransform dtRT = dtObj.GetComponent<RectTransform>();
            dtRT.anchorMin = new Vector2(0, 0);
            dtRT.anchorMax = new Vector2(1, 0);
            dtRT.pivot = new Vector2(0, 0);
            dtRT.anchoredPosition = Vector2.zero;
            dtRT.sizeDelta = new Vector2(0, 20);
            detailType = dtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailType.font = font;
            detailType.text = "<color=#007A4D><b>[QUỸ ĐẠO HỘ VỆ]</b></color>  •  <color=#2E7D32><b>Hệ Mộc</b></color>";
            detailType.fontSize = 12;
            detailType.fontStyle = FontStyles.Bold;

            // 2. Thân Soi (Icon Lớn 84x84 + Cụm Chỉ Số & Mô Tả)
            GameObject dBody = CreateUIElement("Body_Weapon", dContent.transform);
            RectTransform dbRT2 = dBody.GetComponent<RectTransform>();
            dbRT2.anchorMin = new Vector2(0, 0);
            dbRT2.anchorMax = new Vector2(1, 1);
            dbRT2.offsetMin = Vector2.zero;
            dbRT2.offsetMax = new Vector2(0, -52);

            // Khung Icon 80x80
            GameObject dFrame = CreateUIElement("Frame_Icon", dBody.transform);
            RectTransform dfRT = dFrame.GetComponent<RectTransform>();
            dfRT.anchorMin = new Vector2(0, 1);
            dfRT.anchorMax = new Vector2(0, 1);
            dfRT.pivot = new Vector2(0, 1);
            dfRT.anchoredPosition = new Vector2(0, -4);
            dfRT.sizeDelta = new Vector2(80, 80);
            var dfImg = dFrame.AddComponent<Image>();
            dfImg.color = Color.white;
            dfImg.type = Image.Type.Sliced;
            Sprite skillBox = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Box_Skill_Icon_Wood_9Slice.png");
            if (skillBox == null) skillBox = slotWood;
            if (skillBox != null) dfImg.sprite = skillBox;

            GameObject dIconObj = CreateUIElement("Icon", dFrame.transform);
            SetStretchAnchor(dIconObj.GetComponent<RectTransform>());
            dIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(8, 8);
            dIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-8, -8);
            detailIcon = dIconObj.AddComponent<Image>();
            detailIcon.preserveAspect = true;

            // Cụm Bên Phải Icon (Stats & Desc)
            GameObject dRight = CreateUIElement("Right_StatsAndDesc", dBody.transform);
            RectTransform drRT = dRight.GetComponent<RectTransform>();
            drRT.anchorMin = new Vector2(0, 0);
            drRT.anchorMax = new Vector2(1, 1);
            drRT.offsetMin = new Vector2(96, 0);
            drRT.offsetMax = Vector2.zero;

            // Stat Bars Row (Sát Thương & Hồi Chiêu)
            GameObject statRow = CreateUIElement("Row_StatBars", dRight.transform);
            RectTransform stRT = statRow.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0, 1);
            stRT.anchorMax = new Vector2(1, 1);
            stRT.pivot = new Vector2(0, 1);
            stRT.anchoredPosition = Vector2.zero;
            stRT.sizeDelta = new Vector2(0, 48);

            // Sát Thương
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
            detailDmg.text = "Sát thương: <color=#A33418><b>8</b></color>";
            detailDmg.fontSize = 12;
            detailDmg.fontStyle = FontStyles.Bold;
            detailDmg.color = new Color(0.25f, 0.16f, 0.10f, 1f);

            GameObject dmgBg = CreateUIElement("Bar_Bg", dmgCol.transform);
            RectTransform dbRT = dmgBg.GetComponent<RectTransform>();
            dbRT.anchorMin = new Vector2(0, 0);
            dbRT.anchorMax = new Vector2(1, 0);
            dbRT.pivot = new Vector2(0, 0);
            dbRT.anchoredPosition = new Vector2(0, 2);
            dbRT.sizeDelta = new Vector2(0, 12);
            var dbImg = dmgBg.AddComponent<Image>();
            dbImg.color = Color.white;
            dbImg.type = Image.Type.Sliced;
            if (gaugeFrame != null) dbImg.sprite = gaugeFrame;

            GameObject dmgFillObj = CreateUIElement("Bar_Fill", dmgBg.transform);
            SetStretchAnchor(dmgFillObj.GetComponent<RectTransform>());
            dmgFill = dmgFillObj.AddComponent<Image>();
            dmgFill.color = Color.white;
            dmgFill.type = Image.Type.Filled;
            dmgFill.fillMethod = Image.FillMethod.Horizontal;
            dmgFill.fillAmount = 0.40f;
            if (gaugeDmg != null) dmgFill.sprite = gaugeDmg;

            // Hồi Chiêu
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
            detailCd.text = "Hồi chiêu: <color=#0E6073><b>0.4s</b></color>";
            detailCd.fontSize = 12;
            detailCd.fontStyle = FontStyles.Bold;
            detailCd.color = new Color(0.25f, 0.16f, 0.10f, 1f);

            GameObject cdBg = CreateUIElement("Bar_Bg", cdCol.transform);
            RectTransform cbRT = cdBg.GetComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(0, 0);
            cbRT.anchorMax = new Vector2(1, 0);
            cbRT.pivot = new Vector2(0, 0);
            cbRT.anchoredPosition = new Vector2(0, 2);
            cbRT.sizeDelta = new Vector2(0, 12);
            var cbImg = cdBg.AddComponent<Image>();
            cbImg.color = Color.white;
            cbImg.type = Image.Type.Sliced;
            if (gaugeFrame != null) cbImg.sprite = gaugeFrame;

            GameObject cdFillObj = CreateUIElement("Bar_Fill", cdBg.transform);
            SetStretchAnchor(cdFillObj.GetComponent<RectTransform>());
            cdFill = cdFillObj.AddComponent<Image>();
            cdFill.color = Color.white;
            cdFill.type = Image.Type.Filled;
            cdFill.fillMethod = Image.FillMethod.Horizontal;
            cdFill.fillAmount = 0.85f;
            if (gaugeCd != null) cdFill.sprite = gaugeCd;

            // Mô Tả
            GameObject descObj = CreateUIElement("Txt_Description", dRight.transform);
            RectTransform ddRT = descObj.GetComponent<RectTransform>();
            ddRT.anchorMin = new Vector2(0, 0);
            ddRT.anchorMax = new Vector2(1, 1);
            ddRT.offsetMin = Vector2.zero;
            ddRT.offsetMax = new Vector2(0, -56);
            detailDesc = descObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailDesc.font = font;
            detailDesc.text = "Vòng lá bùa thần xoay quanh cản đạn và đẩy lùi yêu ma.";
            detailDesc.fontSize = 13;
            detailDesc.color = new Color(0.28f, 0.18f, 0.12f, 1f);
            detailDesc.enableWordWrapping = true;

            // ================= SECTION 3: NÚT XÁC NHẬN XUẤT TRẬN =================
            GameObject btnObj = CreateUIElement("Btn_StartBattle", innerObj.transform);
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 68);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = Color.white;
            btnImg.type = Image.Type.Sliced;
            if (btnBattleAmber != null) btnImg.sprite = btnBattleAmber;
            startBattleBtn = btnObj.AddComponent<Button>();

            GameObject btnTxt = CreateUIElement("Text", btnObj.transform);
            SetStretchAnchor(btnTxt.GetComponent<RectTransform>());
            var btTMP = btnTxt.AddComponent<TextMeshProUGUI>();
            if (font != null) btTMP.font = font;
            btTMP.text = "XÁC NHẬN XUẤT TRẬN";
            btTMP.fontSize = 20;
            btTMP.fontStyle = FontStyles.Bold;
            btTMP.alignment = TextAlignmentOptions.Center;
            btTMP.color = new Color(1f, 0.95f, 0.80f, 1f);
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

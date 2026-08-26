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
    /// Generator tạo Prefab giao diện Tàng Bảo Các (WeaponLoadoutUI) chuẩn Cổ Phong Đông Sơn 3 Cột (Responsive Auto-Layout).
    /// Thiết kế theo chuẩn thẩm mỹ Hades / Honkai Star Rail: Phân cấp thị giác rõ ràng, không bị đè chữ, không vỡ layout.
    /// </summary>
    public static class WeaponLoadoutUIGenerator
    {
        // Bảng màu Cổ Phong Đông Sơn cao cấp
        private static readonly Color ColorBgOverlay = new Color(0.04f, 0.03f, 0.06f, 0.88f);
        private static readonly Color ColorModalBg = new Color(0.10f, 0.08f, 0.14f, 0.98f);
        private static readonly Color ColorCardBg = new Color(0.15f, 0.12f, 0.20f, 0.95f);
        private static readonly Color ColorCardInner = new Color(0.20f, 0.16f, 0.28f, 0.90f);
        private static readonly Color ColorGold = new Color(0.95f, 0.82f, 0.45f, 1f);
        private static readonly Color ColorCinnabar = new Color(0.85f, 0.22f, 0.18f, 1f);
        private static readonly Color ColorJadeCyan = new Color(0.35f, 0.92f, 0.90f, 1f);
        private static readonly Color ColorMutedText = new Color(0.82f, 0.82f, 0.88f, 1f);

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

            // 3. Modal Box Trung Tâm (1180 x 680 - Center Pivot)
            GameObject modal = CreateUIElement("Modal_WeaponLoadout", root.transform);
            RectTransform modalRT = modal.GetComponent<RectTransform>();
            modalRT.anchorMin = new Vector2(0.5f, 0.5f);
            modalRT.anchorMax = new Vector2(0.5f, 0.5f);
            modalRT.pivot = new Vector2(0.5f, 0.5f);
            modalRT.sizeDelta = new Vector2(1180, 680);
            modalRT.anchoredPosition = Vector2.zero;
            var modalImg = modal.AddComponent<Image>();
            modalImg.color = ColorModalBg;

            // 4. Header Section
            BuildHeader(modal.transform, vietFont, out TextMeshProUGUI heroNameTMP, out TextMeshProUGUI heroElemTMP, out Image heroAvatarImg, out Button backBtn);

            // 5. Body Section - 3 Cột Ngang Tách Biệt
            GameObject bodyObj = CreateUIElement("Container_Body3Cols", modal.transform);
            RectTransform bodyRT = bodyObj.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0, 0);
            bodyRT.anchorMax = new Vector2(1, 1);
            bodyRT.offsetMin = new Vector2(20, 20);
            bodyRT.offsetMax = new Vector2(-20, -70); // Chừa 70px cho Header

            var bodyHlg = bodyObj.AddComponent<HorizontalLayoutGroup>();
            bodyHlg.spacing = 16;
            bodyHlg.childAlignment = TextAnchor.UpperLeft;
            bodyHlg.childControlWidth = false;
            bodyHlg.childControlHeight = true;
            bodyHlg.childForceExpandWidth = false;
            bodyHlg.childForceExpandHeight = true;

            // Cột 1: Vũ Khí Chính (Width 280)
            BuildPrimaryColumn(bodyObj.transform, vietFont, out Transform primaryGrid);

            // Cột 2: Pháp Bảo Hộ Thân (Width 460)
            BuildRelicColumn(bodyObj.transform, vietFont, out Transform relicGrid);

            // Cột 3: Tóm Tắt Trang Bị + Chi Tiết + Nút Xuất Trận (Width 380)
            BuildRightDetailColumn(bodyObj.transform, vietFont,
                out Image priSlotIcon, out TextMeshProUGUI priSlotName,
                out Image[] relicIcons, out TextMeshProUGUI[] relicNames,
                out Image detailIcon, out TextMeshProUGUI detailName, out TextMeshProUGUI detailType,
                out TextMeshProUGUI detailDmg, out TextMeshProUGUI detailCd, out TextMeshProUGUI detailDesc,
                out Button startBattleBtn);

            // 6. Wire Properties to View
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_heroAvatarImage").objectReferenceValue = heroAvatarImg;
            soView.FindProperty("_heroNameText").objectReferenceValue = heroNameTMP;
            soView.FindProperty("_heroElementText").objectReferenceValue = heroElemTMP;

            soView.FindProperty("_primarySlotIcon").objectReferenceValue = priSlotIcon;
            soView.FindProperty("_primarySlotName").objectReferenceValue = priSlotName;

            var relicIconProp = soView.FindProperty("_relicSlotIcons");
            relicIconProp.arraySize = 3;
            for (int i = 0; i < 3; i++) relicIconProp.GetArrayElementAtIndex(i).objectReferenceValue = relicIcons[i];

            var relicNameProp = soView.FindProperty("_relicSlotNames");
            relicNameProp.arraySize = 3;
            for (int i = 0; i < 3; i++) relicNameProp.GetArrayElementAtIndex(i).objectReferenceValue = relicNames[i];

            soView.FindProperty("_primaryWeaponsContainer").objectReferenceValue = primaryGrid;
            soView.FindProperty("_relicWeaponsContainer").objectReferenceValue = relicGrid;

            soView.FindProperty("_detailIcon").objectReferenceValue = detailIcon;
            soView.FindProperty("_detailNameText").objectReferenceValue = detailName;
            soView.FindProperty("_detailTypeText").objectReferenceValue = detailType;
            soView.FindProperty("_detailDamageText").objectReferenceValue = detailDmg;
            soView.FindProperty("_detailCooldownText").objectReferenceValue = detailCd;
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

                Debug.Log($"<color=#00FF88>[WeaponLoadoutUIGenerator]</color> Đã tạo Prefab 3 Cột Cổ Phong và kết nối vào Canvas_MetaMenu thành công!");
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
            hRT.sizeDelta = new Vector2(0, 60);

            // 1. Hero Badge (Trái)
            GameObject badge = CreateUIElement("Badge_Hero", headerObj.transform);
            RectTransform bRT = badge.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 0.5f);
            bRT.anchorMax = new Vector2(0, 0.5f);
            bRT.pivot = new Vector2(0, 0.5f);
            bRT.anchoredPosition = new Vector2(20, 0);
            bRT.sizeDelta = new Vector2(260, 44);
            var bImg = badge.AddComponent<Image>();
            bImg.color = ColorCardBg;

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
            nRT.anchoredPosition = new Vector2(48, 8);
            nRT.sizeDelta = new Vector2(200, 20);
            heroName = nameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroName.font = font;
            heroName.text = "Thư Sinh";
            heroName.fontSize = 15;
            heroName.fontStyle = FontStyles.Bold;
            heroName.color = Color.white;

            GameObject elemObj = CreateUIElement("Txt_HeroElem", badge.transform);
            RectTransform eRT = elemObj.GetComponent<RectTransform>();
            eRT.anchoredPosition = new Vector2(48, -10);
            eRT.sizeDelta = new Vector2(200, 16);
            heroElem = elemObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroElem.font = font;
            heroElem.text = "<color=#FFD700>Hệ Kim</color>";
            heroElem.fontSize = 12;

            // 2. Title Header (Giữa)
            GameObject titleObj = CreateUIElement("Txt_Title", headerObj.transform);
            RectTransform tRT = titleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 0.5f);
            tRT.anchorMax = new Vector2(0.5f, 0.5f);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.anchoredPosition = Vector2.zero;
            tRT.sizeDelta = new Vector2(500, 40);
            var tTMP = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tTMP.font = font;
            tTMP.text = "TÀNG BẢO CÁC • KHO PHÁP BẢO";
            tTMP.fontSize = 22;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.color = ColorGold;

            // 3. Back Button (Phải)
            GameObject backObj = CreateUIElement("Btn_Back", headerObj.transform);
            RectTransform bkRT = backObj.GetComponent<RectTransform>();
            bkRT.anchorMin = new Vector2(1, 0.5f);
            bkRT.anchorMax = new Vector2(1, 0.5f);
            bkRT.pivot = new Vector2(1, 0.5f);
            bkRT.anchoredPosition = new Vector2(-20, 0);
            bkRT.sizeDelta = new Vector2(110, 38);
            var bkImg = backObj.AddComponent<Image>();
            bkImg.color = new Color(0.24f, 0.18f, 0.30f, 0.95f);
            backBtn = backObj.AddComponent<Button>();

            GameObject bkT = CreateUIElement("Text", backObj.transform);
            SetStretchAnchor(bkT.GetComponent<RectTransform>());
            var bkTMP = bkT.AddComponent<TextMeshProUGUI>();
            if (font != null) bkTMP.font = font;
            bkTMP.text = "◀ QUAY LẠI";
            bkTMP.fontSize = 14;
            bkTMP.fontStyle = FontStyles.Bold;
            bkTMP.alignment = TextAlignmentOptions.Center;
            bkTMP.color = Color.white;
        }

        private static void BuildPrimaryColumn(Transform parent, TMP_FontAsset font, out Transform primaryGrid)
        {
            GameObject col = CreateUIElement("Col_PrimaryWeapons", parent);
            RectTransform rt = col.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(270, 0);
            var img = col.AddComponent<Image>();
            img.color = ColorCardBg;

            // Header Text
            GameObject hObj = CreateUIElement("Header", col.transform);
            RectTransform hRT = hObj.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 1);
            hRT.anchorMax = new Vector2(1, 1);
            hRT.pivot = new Vector2(0.5f, 1);
            hRT.anchoredPosition = new Vector2(0, -10);
            hRT.sizeDelta = new Vector2(-20, 36);
            var hTMP = hObj.AddComponent<TextMeshProUGUI>();
            if (font != null) hTMP.font = font;
            hTMP.text = "<color=#FF8800>⚔️ VŨ KHÍ CHÍNH</color>\n<size=11><color=#AAAAAA>(Chọn 1 Vũ Khí Đánh Tay)</color></size>";
            hTMP.fontSize = 14;
            hTMP.fontStyle = FontStyles.Bold;
            hTMP.alignment = TextAlignmentOptions.Center;

            // Grid Container
            GameObject gridObj = CreateUIElement("Grid_PrimaryWeapons", col.transform);
            RectTransform gRT = gridObj.GetComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0, 0);
            gRT.anchorMax = new Vector2(1, 1);
            gRT.offsetMin = new Vector2(12, 12);
            gRT.offsetMax = new Vector2(-12, -55);

            var glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(72, 72);
            glg.spacing = new Vector2(10, 10);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.childAlignment = TextAnchor.UpperLeft;
            primaryGrid = gridObj.transform;
        }

        private static void BuildRelicColumn(Transform parent, TMP_FontAsset font, out Transform relicGrid)
        {
            GameObject col = CreateUIElement("Col_RelicWeapons", parent);
            RectTransform rt = col.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(470, 0);
            var img = col.AddComponent<Image>();
            img.color = ColorCardBg;

            // Header Text
            GameObject hObj = CreateUIElement("Header", col.transform);
            RectTransform hRT = hObj.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 1);
            hRT.anchorMax = new Vector2(1, 1);
            hRT.pivot = new Vector2(0.5f, 1);
            hRT.anchoredPosition = new Vector2(0, -10);
            hRT.sizeDelta = new Vector2(-20, 36);
            var hTMP = hObj.AddComponent<TextMeshProUGUI>();
            if (font != null) hTMP.font = font;
            hTMP.text = "<color=#00FF88>🛡️ PHÁP BẢO HỘ THÂN</color>\n<size=11><color=#AAAAAA>(Tự Động Bồi Đòn / Hộ Thể - Chọn Tối Đa 3 Món)</color></size>";
            hTMP.fontSize = 14;
            hTMP.fontStyle = FontStyles.Bold;
            hTMP.alignment = TextAlignmentOptions.Center;

            // Grid Container
            GameObject gridObj = CreateUIElement("Grid_RelicWeapons", col.transform);
            RectTransform gRT = gridObj.GetComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0, 0);
            gRT.anchorMax = new Vector2(1, 1);
            gRT.offsetMin = new Vector2(14, 14);
            gRT.offsetMax = new Vector2(-14, -55);

            var glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(74, 74);
            glg.spacing = new Vector2(12, 12);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;
            glg.childAlignment = TextAnchor.UpperLeft;
            relicGrid = gridObj.transform;
        }

        private static void BuildRightDetailColumn(Transform parent, TMP_FontAsset font,
            out Image priSlotIcon, out TextMeshProUGUI priSlotName,
            out Image[] relicIcons, out TextMeshProUGUI[] relicNames,
            out Image detailIcon, out TextMeshProUGUI detailName, out TextMeshProUGUI detailType,
            out TextMeshProUGUI detailDmg, out TextMeshProUGUI detailCd, out TextMeshProUGUI detailDesc,
            out Button startBattleBtn)
        {
            GameObject col = CreateUIElement("Col_RightSummary", parent);
            RectTransform rt = col.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(380, 0);

            var vlg = col.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 1. Card Equipped Summary (Height = 145)
            GameObject eqCard = CreateUIElement("Card_EquippedSummary", col.transform);
            eqCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 145);
            eqCard.AddComponent<Image>().color = ColorCardBg;

            GameObject eqHeader = CreateUIElement("Header", eqCard.transform);
            RectTransform eqhRT = eqHeader.GetComponent<RectTransform>();
            eqhRT.anchorMin = new Vector2(0, 1);
            eqhRT.anchorMax = new Vector2(1, 1);
            eqhRT.pivot = new Vector2(0, 1);
            eqhRT.anchoredPosition = new Vector2(12, -8);
            eqhRT.sizeDelta = new Vector2(-24, 20);
            var eqhTMP = eqHeader.AddComponent<TextMeshProUGUI>();
            if (font != null) eqhTMP.font = font;
            eqhTMP.text = "<color=#FFD700>📦 TRANG BỊ ĐÃ CHỌN (LOADOUT):</color>";
            eqhTMP.fontSize = 13;
            eqhTMP.fontStyle = FontStyles.Bold;

            // Slot Primary Weapon
            GameObject priObj = CreateUIElement("Slot_Primary", eqCard.transform);
            RectTransform priRT = priObj.GetComponent<RectTransform>();
            priRT.anchorMin = new Vector2(0, 0.5f);
            priRT.anchorMax = new Vector2(0, 0.5f);
            priRT.pivot = new Vector2(0, 0.5f);
            priRT.anchoredPosition = new Vector2(12, -8);
            priRT.sizeDelta = new Vector2(110, 85);
            priObj.AddComponent<Image>().color = new Color(0.24f, 0.18f, 0.10f, 1f);

            GameObject priIcObj = CreateUIElement("Icon", priObj.transform);
            RectTransform priIcRT = priIcObj.GetComponent<RectTransform>();
            priIcRT.anchoredPosition = new Vector2(30, -5);
            priIcRT.sizeDelta = new Vector2(46, 46);
            priSlotIcon = priIcObj.AddComponent<Image>();

            GameObject priNmObj = CreateUIElement("Name", priObj.transform);
            RectTransform priNmRT = priNmObj.GetComponent<RectTransform>();
            priNmRT.anchoredPosition = new Vector2(5, -56);
            priNmRT.sizeDelta = new Vector2(100, 24);
            priSlotName = priNmObj.AddComponent<TextMeshProUGUI>();
            if (font != null) priSlotName.font = font;
            priSlotName.text = "Bút Phán Quan";
            priSlotName.fontSize = 11;
            priSlotName.alignment = TextAlignmentOptions.Center;
            priSlotName.color = ColorGold;

            // 3 Slots Relic
            relicIcons = new Image[3];
            relicNames = new TextMeshProUGUI[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject rObj = CreateUIElement($"Slot_Relic_{i + 1}", eqCard.transform);
                RectTransform rRT = rObj.GetComponent<RectTransform>();
                rRT.anchorMin = new Vector2(0, 0.5f);
                rRT.anchorMax = new Vector2(0, 0.5f);
                rRT.pivot = new Vector2(0, 0.5f);
                rRT.anchoredPosition = new Vector2(130 + i * 78, -8);
                rRT.sizeDelta = new Vector2(72, 85);
                rObj.AddComponent<Image>().color = ColorCardInner;

                GameObject rIcObj = CreateUIElement("Icon", rObj.transform);
                RectTransform rIcRT = rIcObj.GetComponent<RectTransform>();
                rIcRT.anchoredPosition = new Vector2(15, -6);
                rIcRT.sizeDelta = new Vector2(42, 42);
                relicIcons[i] = rIcObj.AddComponent<Image>();

                GameObject rNmObj = CreateUIElement("Name", rObj.transform);
                RectTransform rNmRT = rNmObj.GetComponent<RectTransform>();
                rNmRT.anchoredPosition = new Vector2(2, -56);
                rNmRT.sizeDelta = new Vector2(68, 24);
                relicNames[i] = rNmObj.AddComponent<TextMeshProUGUI>();
                if (font != null) relicNames[i].font = font;
                relicNames[i].text = "Trống";
                relicNames[i].fontSize = 10;
                relicNames[i].alignment = TextAlignmentOptions.Center;
                relicNames[i].color = ColorMutedText;
            }

            // 2. Card Detail Inspection (Height = 330)
            GameObject dtCard = CreateUIElement("Card_DetailInspection", col.transform);
            dtCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 330);
            dtCard.AddComponent<Image>().color = ColorCardBg;

            GameObject dIcObj = CreateUIElement("Img_Icon", dtCard.transform);
            RectTransform dIcRT = dIcObj.GetComponent<RectTransform>();
            dIcRT.anchorMin = new Vector2(0, 1);
            dIcRT.anchorMax = new Vector2(0, 1);
            dIcRT.pivot = new Vector2(0, 1);
            dIcRT.anchoredPosition = new Vector2(14, -14);
            dIcRT.sizeDelta = new Vector2(58, 58);
            detailIcon = dIcObj.AddComponent<Image>();

            GameObject dNmObj = CreateUIElement("Txt_Name", dtCard.transform);
            RectTransform dNmRT = dNmObj.GetComponent<RectTransform>();
            dNmRT.anchorMin = new Vector2(0, 1);
            dNmRT.anchorMax = new Vector2(1, 1);
            dNmRT.pivot = new Vector2(0, 1);
            dNmRT.anchoredPosition = new Vector2(80, -12);
            dNmRT.sizeDelta = new Vector2(-90, 26);
            detailName = dNmObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailName.font = font;
            detailName.text = "Tên Pháp Bảo";
            detailName.fontSize = 17;
            detailName.fontStyle = FontStyles.Bold;
            detailName.color = ColorGold;

            GameObject dTyObj = CreateUIElement("Txt_Type", dtCard.transform);
            RectTransform dTyRT = dTyObj.GetComponent<RectTransform>();
            dTyRT.anchorMin = new Vector2(0, 1);
            dTyRT.anchorMax = new Vector2(1, 1);
            dTyRT.pivot = new Vector2(0, 1);
            dTyRT.anchoredPosition = new Vector2(80, -40);
            dTyRT.sizeDelta = new Vector2(-90, 20);
            detailType = dTyObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailType.font = font;
            detailType.text = "Vũ Khí Chính • Hệ Kim";
            detailType.fontSize = 12;

            // Stats Line
            GameObject dDmgObj = CreateUIElement("Txt_Damage", dtCard.transform);
            RectTransform dDmgRT = dDmgObj.GetComponent<RectTransform>();
            dDmgRT.anchorMin = new Vector2(0, 1);
            dDmgRT.anchorMax = new Vector2(0.5f, 1);
            dDmgRT.pivot = new Vector2(0, 1);
            dDmgRT.anchoredPosition = new Vector2(14, -82);
            dDmgRT.sizeDelta = new Vector2(0, 22);
            detailDmg = dDmgObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailDmg.font = font;
            detailDmg.text = "Sát thương: <color=#FFD700>20</color>";
            detailDmg.fontSize = 13;

            GameObject dCdObj = CreateUIElement("Txt_Cooldown", dtCard.transform);
            RectTransform dCdRT = dCdObj.GetComponent<RectTransform>();
            dCdRT.anchorMin = new Vector2(0.5f, 1);
            dCdRT.anchorMax = new Vector2(1, 1);
            dCdRT.pivot = new Vector2(0, 1);
            dCdRT.anchoredPosition = new Vector2(10, -82);
            dCdRT.sizeDelta = new Vector2(-20, 22);
            detailCd = dCdObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailCd.font = font;
            detailCd.text = "Hồi chiêu: <color=#4DEEEA>0.8s</color>";
            detailCd.fontSize = 13;

            // Description Box
            GameObject dDescObj = CreateUIElement("Txt_Desc", dtCard.transform);
            RectTransform dDescRT = dDescObj.GetComponent<RectTransform>();
            dDescRT.anchorMin = new Vector2(0, 0);
            dDescRT.anchorMax = new Vector2(1, 1);
            dDescRT.offsetMin = new Vector2(14, 12);
            dDescRT.offsetMax = new Vector2(-14, -112);
            detailDesc = dDescObj.AddComponent<TextMeshProUGUI>();
            if (font != null) detailDesc.font = font;
            detailDesc.text = "Mô tả chi tiết và cơ chế đặc trưng của Pháp Bảo trong trận chiến...";
            detailDesc.fontSize = 12;
            detailDesc.color = ColorMutedText;
            detailDesc.enableWordWrapping = true;

            // 3. Start Battle Button (Height = 56)
            GameObject btnObj = CreateUIElement("Btn_StartBattle", col.transform);
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 56);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = ColorCinnabar;
            startBattleBtn = btnObj.AddComponent<Button>();

            GameObject btnTxt = CreateUIElement("Text", btnObj.transform);
            SetStretchAnchor(btnTxt.GetComponent<RectTransform>());
            var btTMP = btnTxt.AddComponent<TextMeshProUGUI>();
            if (font != null) btTMP.font = font;
            btTMP.text = "⚡ XÁC NHẬN XUẤT TRẬN";
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

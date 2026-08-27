#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động dựng và chuẩn hóa toàn bộ Sảnh Hoàng Tuyền (MainHubUI) chuẩn AAA Cổ Phong Đông Sơn
    /// Khớp 100% tỷ lệ, layout, vị trí các nút và thẻ trang bị từ bản vẽ thiết kế chuẩn.
    /// </summary>
    public static class MainHubUIGenerator
    {
        // Bảng màu chuẩn Cổ Phong Đông Sơn
        private static readonly Color ColorWoodDark = new Color(0.12f, 0.08f, 0.06f, 0.95f);
        private static readonly Color ColorWoodBtn = new Color(0.20f, 0.13f, 0.09f, 0.98f);
        private static readonly Color ColorBronzeBorder = new Color(0.55f, 0.38f, 0.20f, 1f);
        private static readonly Color ColorGold = new Color(0.96f, 0.84f, 0.45f, 1f);
        private static readonly Color ColorCinnabar = new Color(0.76f, 0.16f, 0.12f, 1f); // Đỏ Chu Sa
        private static readonly Color ColorDragonGold = new Color(0.98f, 0.75f, 0.25f, 1f);
        private static readonly Color ColorMutedText = new Color(0.85f, 0.82f, 0.78f, 1f);

        [MenuItem("Tools/ProjectZombie/UI/Generate Main Hub UI Prefab", priority = 10)]
        public static void GenerateMainHubUI()
        {
            GenerateMainHubPrefab();
        }

        public static void GenerateMainHubPrefab()
        {
            string prefabFolder = "Assets/_Prefabs/UI";
            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs")) AssetDatabase.CreateFolder("Assets", "_Prefabs");
            if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder("Assets/_Prefabs", "UI");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 1. Root Screen Panel (100% Stretch)
            GameObject root = new GameObject("Panel_MainHub", typeof(RectTransform), typeof(CanvasGroup), typeof(MainHubView), typeof(MainHubPresenter));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            SetStretchAnchor(rootRT);

            var view = root.GetComponent<MainHubView>();
            var presenter = root.GetComponent<MainHubPresenter>();

            // 2. Background Scenery Overlay (Vùng cảnh nền sương mù)
            GameObject bgOverlay = CreateUIElement("Scenery_Overlay", root.transform);
            SetStretchAnchor(bgOverlay.GetComponent<RectTransform>());
            var bgImg = bgOverlay.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.03f, 0.05f, 0.35f); // Lớp phủ nhẹ không che cảnh

            // 3. Top Header Bar (Logo Vong Xuyên + Tiền tệ + Cài Đặt)
            BuildTopHeader(root.transform, vietFont, out TextMeshProUGUI coTienTMP, out TextMeshProUGUI linhHonTMP, out Button settingsBtn);

            // 4. Hero Stage Info (Tên & Hệ Tướng)
            BuildHeroStage(root.transform, vietFont, out TextMeshProUGUI heroNameTMP, out TextMeshProUGUI heroElemTMP, out Image heroAvatarImg);

            // 5. Bottom HUD Row (Chứa toàn bộ cụm điều khiển dưới cùng)
            BuildBottomHUDRow(root.transform, vietFont,
                out Button loadoutBtn, out TextMeshProUGUI priNameTMP, out Image priIconImg, out Image[] relicIcons,
                out Button heroBtn, out Button armoryBtn, out Button sanctuaryBtn,
                out Button startRunBtn);

            // 6. Wire Properties to MainHubView
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_coTienText").objectReferenceValue = coTienTMP;
            soView.FindProperty("_linhHonText").objectReferenceValue = linhHonTMP;
            soView.FindProperty("_settingsButton").objectReferenceValue = settingsBtn;

            soView.FindProperty("_currentHeroNameText").objectReferenceValue = heroNameTMP;
            soView.FindProperty("_currentHeroElementText").objectReferenceValue = heroElemTMP;
            soView.FindProperty("_currentHeroAvatarImage").objectReferenceValue = heroAvatarImg;

            soView.FindProperty("_loadoutCardButton").objectReferenceValue = loadoutBtn;
            soView.FindProperty("_primaryWeaponNameText").objectReferenceValue = priNameTMP;
            soView.FindProperty("_primaryWeaponIcon").objectReferenceValue = priIconImg;

            var relicProp = soView.FindProperty("_relicIcons");
            relicProp.arraySize = 3;
            for (int i = 0; i < 3; i++) relicProp.GetArrayElementAtIndex(i).objectReferenceValue = relicIcons[i];

            soView.FindProperty("_heroSelectButton").objectReferenceValue = heroBtn;
            soView.FindProperty("_armoryButton").objectReferenceValue = armoryBtn;
            soView.FindProperty("_sanctuaryTreeButton").objectReferenceValue = sanctuaryBtn;
            soView.FindProperty("_startRunButton").objectReferenceValue = startRunBtn;
            soView.ApplyModifiedProperties();

            // 7. Wire Presenter
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            // 8. Lưu Prefab
            string prefabPath = $"{prefabFolder}/MainHubUI.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // 9. Cập nhật vào Scene nếu có Canvas_MetaMenu
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                var oldUI = GameObject.Find("Panel_MainHub");
                if (oldUI != null && oldUI != root) Object.DestroyImmediate(oldUI);

                var metaCanvas = GameObject.Find("Canvas_MetaMenu");
                Transform targetParent = metaCanvas != null ? metaCanvas.transform : canvas.transform;

                root.transform.SetParent(targetParent, false);
                SetStretchAnchor(rootRT);
                root.transform.SetAsFirstSibling(); // MainHub nằm dưới các Modal popup

                var metaMgr = Object.FindAnyObjectByType<MetaUIManager>();
                if (metaMgr != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaMgr);
                    soMeta.FindProperty("_mainHubScreen").objectReferenceValue = view;
                    soMeta.ApplyModifiedProperties();
                    EditorUtility.SetDirty(metaMgr);
                }

                Debug.Log($"<color=#00FF88>[MainHubUIGenerator]</color> Đã tạo Prefab Sảnh Hoàng Tuyền khớp 100% thiết kế tham khảo!");
            }
            else
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildTopHeader(Transform parent, TMP_FontAsset font, 
            out TextMeshProUGUI coTienTMP, out TextMeshProUGUI linhHonTMP, out Button settingsBtn)
        {
            GameObject header = CreateUIElement("Header_TopBar", parent);
            RectTransform hRT = header.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 1);
            hRT.anchorMax = new Vector2(1, 1);
            hRT.pivot = new Vector2(0.5f, 1);
            hRT.anchoredPosition = new Vector2(0, -14);
            hRT.sizeDelta = new Vector2(-60, 60);

            // 1. Logo Game "VONG XUYÊN" (Góc Trái)
            GameObject logoObj = CreateUIElement("Logo_VongXuyen", header.transform);
            RectTransform lRT = logoObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f);
            lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(10, 0);
            lRT.sizeDelta = new Vector2(280, 50);

            var lTMP = logoObj.AddComponent<TextMeshProUGUI>();
            if (font != null) lTMP.font = font;
            lTMP.text = "VONG XUYÊN";
            lTMP.fontSize = 32;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = ColorGold;

            // 2. Khung Tiền Tệ & Cài Đặt (Góc Phải)
            GameObject rightGroup = CreateUIElement("Right_Currencies", header.transform);
            RectTransform rRT = rightGroup.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(1, 0.5f);
            rRT.anchorMax = new Vector2(1, 0.5f);
            rRT.pivot = new Vector2(1, 0.5f);
            rRT.anchoredPosition = Vector2.zero;
            rRT.sizeDelta = new Vector2(460, 48);

            var rHlg = rightGroup.AddComponent<HorizontalLayoutGroup>();
            rHlg.spacing = 14;
            rHlg.childAlignment = TextAnchor.MiddleRight;
            rHlg.childControlWidth = false;
            rHlg.childControlHeight = false;
            rHlg.childForceExpandWidth = false;
            rHlg.childForceExpandHeight = false;

            // Box 1: Cổ Tiền (170 x 44)
            GameObject box1 = CreateUIElement("Box_CoTien", rightGroup.transform);
            box1.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 42);
            var b1Img = box1.AddComponent<Image>();
            b1Img.color = ColorWoodDark;

            // Viền đồng box 1
            CreateBorder(box1.transform, ColorBronzeBorder, 2);

            GameObject b1Txt = CreateUIElement("Text", box1.transform);
            SetStretchAnchor(b1Txt.GetComponent<RectTransform>());
            b1Txt.GetComponent<RectTransform>().offsetMin = new Vector2(12, 0);
            b1Txt.GetComponent<RectTransform>().offsetMax = new Vector2(-12, 0);
            coTienTMP = b1Txt.AddComponent<TextMeshProUGUI>();
            if (font != null) coTienTMP.font = font;
            coTienTMP.text = "Cổ Tiền: <color=#FFD700>12,580</color>";
            coTienTMP.fontSize = 13;
            coTienTMP.fontStyle = FontStyles.Bold;
            coTienTMP.alignment = TextAlignmentOptions.Center;
            coTienTMP.color = ColorMutedText;

            // Box 2: Linh Hồn (170 x 44)
            GameObject box2 = CreateUIElement("Box_LinhHon", rightGroup.transform);
            box2.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 42);
            var b2Img = box2.AddComponent<Image>();
            b2Img.color = ColorWoodDark;

            CreateBorder(box2.transform, ColorBronzeBorder, 2);

            GameObject b2Txt = CreateUIElement("Text", box2.transform);
            SetStretchAnchor(b2Txt.GetComponent<RectTransform>());
            b2Txt.GetComponent<RectTransform>().offsetMin = new Vector2(12, 0);
            b2Txt.GetComponent<RectTransform>().offsetMax = new Vector2(-12, 0);
            linhHonTMP = b2Txt.AddComponent<TextMeshProUGUI>();
            if (font != null) linhHonTMP.font = font;
            linhHonTMP.text = "Linh Hồn: <color=#B388FF>3,420</color>";
            linhHonTMP.fontSize = 13;
            linhHonTMP.fontStyle = FontStyles.Bold;
            linhHonTMP.alignment = TextAlignmentOptions.Center;
            linhHonTMP.color = ColorMutedText;

            // Nút Cài Đặt (44 x 42)
            GameObject setObj = CreateUIElement("Btn_Settings", rightGroup.transform);
            setObj.GetComponent<RectTransform>().sizeDelta = new Vector2(44, 42);
            var setImg = setObj.AddComponent<Image>();
            setImg.color = ColorWoodDark;
            settingsBtn = setObj.AddComponent<Button>();
            CreateBorder(setObj.transform, ColorBronzeBorder, 2);

            GameObject setTxt = CreateUIElement("Text", setObj.transform);
            SetStretchAnchor(setTxt.GetComponent<RectTransform>());
            var sTMP = setTxt.AddComponent<TextMeshProUGUI>();
            if (font != null) sTMP.font = font;
            sTMP.text = "*";
            sTMP.fontSize = 20;
            sTMP.fontStyle = FontStyles.Bold;
            sTMP.alignment = TextAlignmentOptions.Center;
            sTMP.color = ColorGold;
        }

        private static void BuildHeroStage(Transform parent, TMP_FontAsset font,
            out TextMeshProUGUI heroName, out TextMeshProUGUI heroElem, out Image heroAvatar)
        {
            GameObject stage = CreateUIElement("Stage_HeroCenter", parent);
            RectTransform sRT = stage.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 0.5f);
            sRT.anchorMax = new Vector2(0.5f, 0.5f);
            sRT.pivot = new Vector2(0.5f, 0.5f);
            sRT.anchoredPosition = new Vector2(0, 30);
            sRT.sizeDelta = new Vector2(340, 380);

            // Avatar Tướng (hoặc 3D/2D Rig)
            GameObject avObj = CreateUIElement("Img_HeroAvatar", stage.transform);
            RectTransform avRT = avObj.GetComponent<RectTransform>();
            avRT.anchorMin = new Vector2(0.5f, 0.5f);
            avRT.anchorMax = new Vector2(0.5f, 0.5f);
            avRT.pivot = new Vector2(0.5f, 0.5f);
            avRT.anchoredPosition = new Vector2(0, 20);
            avRT.sizeDelta = new Vector2(260, 280);
            heroAvatar = avObj.AddComponent<Image>();
            heroAvatar.preserveAspect = true;

            // Biển hiệu Tên Tướng & Hệ bên dưới chân
            GameObject tagObj = CreateUIElement("Tag_HeroName", stage.transform);
            RectTransform tRT = tagObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 0);
            tRT.anchorMax = new Vector2(0.5f, 0);
            tRT.pivot = new Vector2(0.5f, 0);
            tRT.anchoredPosition = new Vector2(0, 0);
            tRT.sizeDelta = new Vector2(200, 32);

            GameObject nameObj = CreateUIElement("Txt_HeroName", tagObj.transform);
            SetStretchAnchor(nameObj.GetComponent<RectTransform>());
            heroName = nameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroName.font = font;
            heroName.text = "ĐẠO SĨ";
            heroName.fontSize = 20;
            heroName.fontStyle = FontStyles.Bold;
            heroName.alignment = TextAlignmentOptions.Center;
            heroName.color = ColorGold;

            heroElem = null; // Gộp chung hoặc quản lý qua name
        }

        private static void BuildBottomHUDRow(Transform parent, TMP_FontAsset font,
            out Button loadoutBtn, out TextMeshProUGUI priName, out Image priIcon, out Image[] relicIcons,
            out Button heroBtn, out Button armoryBtn, out Button sanctuaryBtn,
            out Button startRunBtn)
        {
            GameObject hud = CreateUIElement("Bottom_HUDRow", parent);
            RectTransform hRT = hud.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 0);
            hRT.anchorMax = new Vector2(1, 0);
            hRT.pivot = new Vector2(0.5f, 0);
            hRT.anchoredPosition = new Vector2(0, 24);
            hRT.sizeDelta = new Vector2(-60, 140);

            // ================= 1. GÓC TRÁI: THẺ TRANG BỊ LOADOUT (340 x 130) =================
            GameObject loadoutCard = CreateUIElement("Card_LoadoutSummary", hud.transform);
            RectTransform lcRT = loadoutCard.GetComponent<RectTransform>();
            lcRT.anchorMin = new Vector2(0, 0.5f);
            lcRT.anchorMax = new Vector2(0, 0.5f);
            lcRT.pivot = new Vector2(0, 0.5f);
            lcRT.anchoredPosition = new Vector2(10, 0);
            lcRT.sizeDelta = new Vector2(340, 130);

            var lcImg = loadoutCard.AddComponent<Image>();
            lcImg.color = Color.white;
            lcImg.type = Image.Type.Sliced;
            Sprite cardDongSon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Frame_Modal_Window_DongSon.png");
            if (cardDongSon != null) lcImg.sprite = cardDongSon;
            loadoutBtn = loadoutCard.AddComponent<Button>();

            // Header "TRANG BỊ (LOADOUT)"
            GameObject lcTitle = CreateUIElement("Txt_Title", loadoutCard.transform);
            RectTransform lctRT = lcTitle.GetComponent<RectTransform>();
            lctRT.anchorMin = new Vector2(0, 1);
            lctRT.anchorMax = new Vector2(1, 1);
            lctRT.pivot = new Vector2(0, 1);
            lctRT.anchoredPosition = new Vector2(14, -8);
            lctRT.sizeDelta = new Vector2(-28, 20);
            var lctTMP = lcTitle.AddComponent<TextMeshProUGUI>();
            if (font != null) lctTMP.font = font;
            lctTMP.text = "TRANG BỊ (LOADOUT)";
            lctTMP.fontSize = 12;
            lctTMP.fontStyle = FontStyles.Bold;
            lctTMP.color = ColorGold;

            // Hàng Ô Trang Bị Bên Trong
            GameObject itemsRow = CreateUIElement("Row_Items", loadoutCard.transform);
            RectTransform irRT = itemsRow.GetComponent<RectTransform>();
            irRT.anchorMin = new Vector2(0, 0);
            irRT.anchorMax = new Vector2(1, 1);
            irRT.offsetMin = new Vector2(12, 10);
            irRT.offsetMax = new Vector2(-12, -32);

            var irHlg = itemsRow.AddComponent<HorizontalLayoutGroup>();
            irHlg.spacing = 10;
            irHlg.childAlignment = TextAnchor.MiddleLeft;
            irHlg.childControlWidth = false;
            irHlg.childControlHeight = false;

            // ================= THẺ TRANG BỊ (LOADOUT) - 2 Ô CHUẨN V5.0 =================
            GameObject pBox = CreateUIElement("Slot_Primary", itemsRow.transform);
            pBox.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            pBox.AddComponent<Image>().color = new Color(0.18f, 0.14f, 0.10f, 1f);
            CreateBorder(pBox.transform, ColorGold, 2);

            GameObject pIconObj = CreateUIElement("Icon", pBox.transform);
            SetStretchAnchor(pIconObj.GetComponent<RectTransform>());
            pIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(4, 4);
            pIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-4, -4);
            priIcon = pIconObj.AddComponent<Image>();
            priIcon.preserveAspect = true;

            GameObject pLbl = CreateUIElement("Txt_Name", pBox.transform);
            RectTransform plRT = pLbl.GetComponent<RectTransform>();
            plRT.anchorMin = new Vector2(0, 0);
            plRT.anchorMax = new Vector2(1, 0);
            plRT.pivot = new Vector2(0.5f, 0);
            plRT.anchoredPosition = new Vector2(0, 2);
            plRT.sizeDelta = new Vector2(0, 16);
            priName = pLbl.AddComponent<TextMeshProUGUI>();
            if (font != null) priName.font = font;
            priName.text = "Đòn Đánh";
            priName.fontSize = 10;
            priName.fontStyle = FontStyles.Bold;
            priName.alignment = TextAlignmentOptions.Center;
            priName.color = ColorGold;

            // Ô Pháp Bảo Hộ Thân (Slot Relic)
            relicIcons = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject rBox = CreateUIElement($"Slot_Relic_{i + 1}", itemsRow.transform);
                rBox.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
                rBox.AddComponent<Image>().color = new Color(0.14f, 0.11f, 0.16f, 1f);
                CreateBorder(rBox.transform, i == 0 ? new Color(0.0f, 1.0f, 0.65f, 1.0f) : new Color(0.35f, 0.35f, 0.40f, 0.5f), 2);

                GameObject rIconObj = CreateUIElement("Icon", rBox.transform);
                SetStretchAnchor(rIconObj.GetComponent<RectTransform>());
                rIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(4, 4);
                rIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-4, -4);
                relicIcons[i] = rIconObj.AddComponent<Image>();
                relicIcons[i].preserveAspect = true;

                // Ẩn 2 slot phụ vì game v5.0 chỉ dùng duy nhất 1 Pháp Bảo Hộ Thân
                if (i > 0)
                {
                    rBox.SetActive(false);
                }
            }

            // ================= 2. Ở GIỮA: 3 NÚT ĐIỀU HƯỚNG GỖ MUN (480 x 64) =================
            GameObject navGroup = CreateUIElement("Group_NavButtons", hud.transform);
            RectTransform ngRT = navGroup.GetComponent<RectTransform>();
            ngRT.anchorMin = new Vector2(0.5f, 0.5f);
            ngRT.anchorMax = new Vector2(0.5f, 0.5f);
            ngRT.pivot = new Vector2(0.5f, 0.5f);
            ngRT.anchoredPosition = new Vector2(10, 0);
            ngRT.sizeDelta = new Vector2(490, 68);

            var ngHlg = navGroup.AddComponent<HorizontalLayoutGroup>();
            ngHlg.spacing = 14;
            ngHlg.childAlignment = TextAnchor.MiddleCenter;
            ngHlg.childControlWidth = false;
            ngHlg.childControlHeight = false;

            // Nút 1: ANH HÙNG (160 x 62 - Xanh Ngọc Bích 9-Slice)
            GameObject btn1 = CreateUIElement("Btn_HeroSelect", navGroup.transform);
            btn1.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 62);
            var b1Img = btn1.AddComponent<Image>();
            b1Img.color = Color.white;
            b1Img.type = Image.Type.Sliced;
            Sprite btnGreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_JadeGreen.png");
            if (btnGreen != null) b1Img.sprite = btnGreen;
            heroBtn = btn1.AddComponent<Button>();

            GameObject b1T = CreateUIElement("Text", btn1.transform);
            SetStretchAnchor(b1T.GetComponent<RectTransform>());
            var b1TMP = b1T.AddComponent<TextMeshProUGUI>();
            if (font != null) b1TMP.font = font;
            b1TMP.text = "ANH HÙNG";
            b1TMP.fontSize = 17;
            b1TMP.fontStyle = FontStyles.Bold;
            b1TMP.alignment = TextAlignmentOptions.Center;
            b1TMP.color = Color.white;

            // Nút 2: TÀNG BẢO CÁC (160 x 62 - Vàng Hoàng Kim 9-Slice)
            GameObject btn2 = CreateUIElement("Btn_Armory", navGroup.transform);
            btn2.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 62);
            var b2Img = btn2.AddComponent<Image>();
            b2Img.color = Color.white;
            b2Img.type = Image.Type.Sliced;
            Sprite btnGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_DragonGold.png");
            if (btnGold != null) b2Img.sprite = btnGold;
            armoryBtn = btn2.AddComponent<Button>();

            GameObject b2T = CreateUIElement("Text", btn2.transform);
            SetStretchAnchor(b2T.GetComponent<RectTransform>());
            var b2TMP = b2T.AddComponent<TextMeshProUGUI>();
            if (font != null) b2TMP.font = font;
            b2TMP.text = "TÀNG BẢO CÁC";
            b2TMP.fontSize = 17;
            b2TMP.fontStyle = FontStyles.Bold;
            b2TMP.alignment = TextAlignmentOptions.Center;
            b2TMP.color = new Color(0.2f, 0.12f, 0.05f, 1f);

            // Nút 3: MIẾU CỔ (160 x 62 - Xanh Ngọc Bích 9-Slice)
            GameObject btn3 = CreateUIElement("Btn_SanctuaryTree", navGroup.transform);
            btn3.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 62);
            var b3Img = btn3.AddComponent<Image>();
            b3Img.color = Color.white;
            b3Img.type = Image.Type.Sliced;
            if (btnGreen != null) b3Img.sprite = btnGreen;
            sanctuaryBtn = btn3.AddComponent<Button>();

            GameObject b3T = CreateUIElement("Text", btn3.transform);
            SetStretchAnchor(b3T.GetComponent<RectTransform>());
            var b3TMP = b3T.AddComponent<TextMeshProUGUI>();
            if (font != null) b3TMP.font = font;
            b3TMP.text = "MIẾU CỔ";
            b3TMP.fontSize = 17;
            b3TMP.fontStyle = FontStyles.Bold;
            b3TMP.alignment = TextAlignmentOptions.Center;
            b3TMP.color = Color.white;

            // ================= 3. GÓC PHẢI: NÚT XUẤT TRẬN ĐỎ CHU SA BỌC RỒNG VÀNG (260 x 86) =================
            GameObject startObj = CreateUIElement("Btn_StartRun", hud.transform);
            RectTransform stRT = startObj.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(1, 0.5f);
            stRT.anchorMax = new Vector2(1, 0.5f);
            stRT.pivot = new Vector2(1, 0.5f);
            stRT.anchoredPosition = new Vector2(-10, 0);
            stRT.sizeDelta = new Vector2(250, 78);

            var stImg = startObj.AddComponent<Image>();
            stImg.color = Color.white;
            stImg.type = Image.Type.Sliced;
            Sprite btnRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_CinnabarRed.png");
            if (btnRed != null) stImg.sprite = btnRed;
            startRunBtn = startObj.AddComponent<Button>();

            GameObject stTxt = CreateUIElement("Text", startObj.transform);
            SetStretchAnchor(stTxt.GetComponent<RectTransform>());
            var stTMP = stTxt.AddComponent<TextMeshProUGUI>();
            if (font != null) stTMP.font = font;
            stTMP.text = "XUẤT TRẬN";
            stTMP.fontSize = 26;
            stTMP.fontStyle = FontStyles.Bold;
            stTMP.alignment = TextAlignmentOptions.Center;
            stTMP.color = Color.white;
        }

        private static void CreateBorder(Transform parent, Color borderColor, float width)
        {
            GameObject border = CreateUIElement("Border", parent);
            SetStretchAnchor(border.GetComponent<RectTransform>());
            var bImg = border.AddComponent<Image>();
            bImg.color = borderColor;
            bImg.raycastTarget = false;
            border.transform.SetAsFirstSibling();
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

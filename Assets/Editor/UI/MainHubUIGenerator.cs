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

        [MenuItem("Tools/ProjectZombie/UI/⚡ Rebuild & Sync All Vong Xuyen Menu (1-Click)", priority = 1)]
        public static void RebuildAllMenuUI()
        {
            GenerateMainHubPrefab();
            SettingsUIGenerator.GenerateSettingsModal();
            CharacterSelectionUIGenerator.GenerateCharacterSelectionPrefab();
            Debug.Log("<color=#00FF88>[MainHubUIGenerator]</color> ĐÃ ĐỒNG BỘ VÀ TÁI TẠO TOÀN BỘ SẢNH CHÍNH, MODAL CHỌN ANH HÙNG & MODAL CÀI ĐẶT THÀNH CÔNG 100%!");
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

            // 2. Background Scenery Overlay (Bức Tranh Nền Rừng Thiêng Vọng Xuyên)
            GameObject bgOverlay = CreateUIElement("Scenery_Overlay", root.transform);
            SetStretchAnchor(bgOverlay.GetComponent<RectTransform>());
            var bgImg = bgOverlay.AddComponent<Image>();
            bgImg.color = Color.white;
            Sprite bgForestSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png");
            if (bgForestSprite != null) bgImg.sprite = bgForestSprite;

            // 3. Top Header Bar (Khung Gỗ Chạm Khắc Đỉnh Màn Hình)
            BuildTopHeader(root.transform, vietFont, out TextMeshProUGUI coTienTMP, out TextMeshProUGUI linhHonTMP, out Button settingsBtn);

            // 4. Hero Stage Info (Bục Đá Lục Giác 2.5D & Tên Đạo Sĩ)
            BuildHeroStage(root.transform, vietFont, out TextMeshProUGUI heroNameTMP, out TextMeshProUGUI heroElemTMP, out Image heroAvatarImg);

            // 5. Bottom HUD Row (Bộ Bài Nan Quạt, Khay Loadout, 3 Nút Thẻ Gỗ, Nút Xuất Trận Lục Giác Ngọc Hổ Phách)
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

            // 9. Cập nhật vào Scene nếu có Canvas
            var metaCanvasObj = GameObject.Find("Canvas_MetaMenu");
            Canvas targetCanvas = metaCanvasObj != null ? metaCanvasObj.GetComponent<Canvas>() : Object.FindAnyObjectByType<Canvas>();
            if (targetCanvas != null)
            {
                var oldUI = GameObject.Find("Panel_MainHub");
                if (oldUI != null && oldUI != root) Object.DestroyImmediate(oldUI);

                root.transform.SetParent(targetCanvas.transform, false);
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

                EditorUtility.SetDirty(root);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetCanvas.gameObject.scene);
                Debug.Log($"<color=#00FF88>[MainHubUIGenerator]</color> Đã tạo Prefab Sảnh Hoàng Tuyền khớp 100% thiết kế tham khảo và đồng bộ vào Scene!");
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
            hRT.anchoredPosition = new Vector2(0, 0);
            hRT.sizeDelta = new Vector2(0, 78);

            // Thanh Khung Gỗ Chạm Khắc Đỉnh Màn Hình
            Sprite headerWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Header_Wood_Bar_VongXuyen.png");
            if (headerWoodSprite != null)
            {
                var hImg = header.AddComponent<Image>();
                hImg.sprite = headerWoodSprite;
                hImg.type = Image.Type.Simple;
                hImg.color = Color.white;
            }

            // 1. Chữ "VONG XUYÊN" (Góc Trái)
            GameObject logoObj = CreateUIElement("Logo_VongXuyen", header.transform);
            RectTransform lRT = logoObj.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0.5f);
            lRT.anchorMax = new Vector2(0, 0.5f);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(32, 4);
            lRT.sizeDelta = new Vector2(280, 50);

            var lTMP = logoObj.AddComponent<TextMeshProUGUI>();
            if (font != null) lTMP.font = font;
            lTMP.text = "VONG XUYÊN";
            lTMP.fontSize = 26;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = new Color(0.96f, 0.84f, 0.55f, 1f);

            // 2. Khung Tiền Tệ & Cài Đặt (Góc Phải)
            GameObject rightGroup = CreateUIElement("Right_Currencies", header.transform);
            RectTransform rRT = rightGroup.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(1, 0.5f);
            rRT.anchorMax = new Vector2(1, 0.5f);
            rRT.pivot = new Vector2(1, 0.5f);
            rRT.anchoredPosition = new Vector2(-24, 0);
            rRT.sizeDelta = new Vector2(420, 46);

            var rHlg = rightGroup.AddComponent<HorizontalLayoutGroup>();
            rHlg.spacing = 14;
            rHlg.childAlignment = TextAnchor.MiddleRight;
            rHlg.childControlWidth = false;
            rHlg.childControlHeight = false;

            Sprite pillWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pill_Currency_Wood.png");

            // Box 1: Cổ Tiền
            GameObject box1 = CreateUIElement("Box_CoTien", rightGroup.transform);
            box1.GetComponent<RectTransform>().sizeDelta = new Vector2(124, 36);
            var b1Img = box1.AddComponent<Image>();
            b1Img.color = Color.white;
            b1Img.type = Image.Type.Sliced;
            if (pillWoodSprite != null) b1Img.sprite = pillWoodSprite;

            GameObject b1Txt = CreateUIElement("Text", box1.transform);
            SetStretchAnchor(b1Txt.GetComponent<RectTransform>());
            b1Txt.GetComponent<RectTransform>().offsetMin = new Vector2(28, 0);
            b1Txt.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
            coTienTMP = b1Txt.AddComponent<TextMeshProUGUI>();
            if (font != null) coTienTMP.font = font;
            coTienTMP.text = "311";
            coTienTMP.fontSize = 15;
            coTienTMP.fontStyle = FontStyles.Bold;
            coTienTMP.alignment = TextAlignmentOptions.Center;
            coTienTMP.color = new Color(0.98f, 0.88f, 0.60f);

            // Box 2: Linh Hồn / Phù Lục
            GameObject box2 = CreateUIElement("Box_LinhHon", rightGroup.transform);
            box2.GetComponent<RectTransform>().sizeDelta = new Vector2(110, 36);
            var b2Img = box2.AddComponent<Image>();
            b2Img.color = Color.white;
            b2Img.type = Image.Type.Sliced;
            if (pillWoodSprite != null) b2Img.sprite = pillWoodSprite;

            GameObject b2Txt = CreateUIElement("Text", box2.transform);
            SetStretchAnchor(b2Txt.GetComponent<RectTransform>());
            b2Txt.GetComponent<RectTransform>().offsetMin = new Vector2(28, 0);
            b2Txt.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
            linhHonTMP = b2Txt.AddComponent<TextMeshProUGUI>();
            if (font != null) linhHonTMP.font = font;
            linhHonTMP.text = "0";
            linhHonTMP.fontSize = 15;
            linhHonTMP.fontStyle = FontStyles.Bold;
            linhHonTMP.alignment = TextAlignmentOptions.Center;
            linhHonTMP.color = new Color(0.85f, 0.75f, 1f);

            // Nút Bánh Răng Cài Đặt (Btn_Settings)
            Sprite gearSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Settings_Gear_Wood.png");
            GameObject btnSetObj = CreateUIElement("Btn_Settings", rightGroup.transform);
            btnSetObj.GetComponent<RectTransform>().sizeDelta = new Vector2(42, 42);
            var setImg = btnSetObj.AddComponent<Image>();
            setImg.color = Color.white;
            if (gearSprite != null) setImg.sprite = gearSprite;
            setImg.preserveAspect = true;

            settingsBtn = btnSetObj.AddComponent<Button>();
            var setColors = settingsBtn.colors;
            setColors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            setColors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            settingsBtn.colors = setColors;
        }

        private static void BuildHeroStage(Transform parent, TMP_FontAsset font,
            out TextMeshProUGUI heroName, out TextMeshProUGUI heroElem, out Image heroAvatar)
        {
            GameObject stage = CreateUIElement("Stage_HeroCenter", parent);
            RectTransform sRT = stage.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.5f, 0.5f);
            sRT.anchorMax = new Vector2(0.5f, 0.5f);
            sRT.pivot = new Vector2(0.5f, 0.5f);
            sRT.anchoredPosition = new Vector2(0, -30);
            sRT.sizeDelta = new Vector2(320, 320);

            // Bục Đá Lục Giác 2.5D Cổ Khảm Hổ Phách
            Sprite pedestalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pedestal_Hexagon_2_5D_WoodStone.png");
            if (pedestalSprite != null)
            {
                GameObject pedObj = CreateUIElement("Pedestal_Hexagon_2_5D", stage.transform);
                RectTransform pRT = pedObj.GetComponent<RectTransform>();
                pRT.anchorMin = new Vector2(0.5f, 0.5f);
                pRT.anchorMax = new Vector2(0.5f, 0.5f);
                pRT.pivot = new Vector2(0.5f, 0.25f);
                pRT.anchoredPosition = new Vector2(0, -40);
                pRT.sizeDelta = new Vector2(256, 138);
                var pImg = pedObj.AddComponent<Image>();
                pImg.sprite = pedestalSprite;
                pImg.color = Color.white;
                pImg.raycastTarget = false;
            }

            // Avatar Tướng Đứng Trên Bục (Để màu trong suốt nếu chưa có sprite để không hiện ô chữ nhật trắng)
            GameObject avObj = CreateUIElement("Img_HeroAvatar", stage.transform);
            RectTransform avRT = avObj.GetComponent<RectTransform>();
            avRT.anchorMin = new Vector2(0.5f, 0.5f);
            avRT.anchorMax = new Vector2(0.5f, 0.5f);
            avRT.pivot = new Vector2(0.5f, 0.2f);
            avRT.anchoredPosition = new Vector2(0, 15);
            avRT.sizeDelta = new Vector2(160, 180);
            heroAvatar = avObj.AddComponent<Image>();
            heroAvatar.color = new Color(1, 1, 1, 0);
            heroAvatar.enabled = false;
            heroAvatar.preserveAspect = true;

            // Tên Tướng: Đạo Sĩ
            GameObject tagObj = CreateUIElement("Tag_HeroName", stage.transform);
            RectTransform tRT = tagObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 0);
            tRT.anchorMax = new Vector2(0.5f, 0);
            tRT.pivot = new Vector2(0.5f, 0);
            tRT.anchoredPosition = new Vector2(0, -75);
            tRT.sizeDelta = new Vector2(200, 32);

            GameObject nameObj = CreateUIElement("Txt_HeroName", tagObj.transform);
            SetStretchAnchor(nameObj.GetComponent<RectTransform>());
            heroName = nameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroName.font = font;
            heroName.text = "Đạo Sĩ";
            heroName.fontSize = 22;
            heroName.fontStyle = FontStyles.Bold;
            heroName.alignment = TextAlignmentOptions.Center;
            heroName.color = new Color(0.96f, 0.88f, 0.72f);

            heroElem = null;
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
            hRT.anchoredPosition = new Vector2(0, 20);
            hRT.sizeDelta = new Vector2(-40, 150);

            // ================= 1. GÓC TRÁI: BỘ BÀI NAN QUẠT & KHAY LOADOUT GỖ =================
            GameObject loadoutContainer = CreateUIElement("Container_LoadoutDeck", hud.transform);
            RectTransform lcRT = loadoutContainer.GetComponent<RectTransform>();
            lcRT.anchorMin = new Vector2(0, 0);
            lcRT.anchorMax = new Vector2(0, 0);
            lcRT.pivot = new Vector2(0, 0);
            lcRT.anchoredPosition = new Vector2(15, 0);
            lcRT.sizeDelta = new Vector2(230, 180);

            // Bộ Bài Xòe Nan Quạt Phía Sau
            Sprite cardDeckSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Relic_Fan_Deck.png");
            if (cardDeckSprite != null)
            {
                GameObject deckObj = CreateUIElement("Card_Relic_Fan_Deck", loadoutContainer.transform);
                RectTransform dRT = deckObj.GetComponent<RectTransform>();
                dRT.anchorMin = new Vector2(0, 0);
                dRT.anchorMax = new Vector2(0, 0);
                dRT.pivot = new Vector2(0, 0);
                dRT.anchoredPosition = new Vector2(10, 48);
                dRT.sizeDelta = new Vector2(174, 106);
                var dImg = deckObj.AddComponent<Image>();
                dImg.sprite = cardDeckSprite;
                dImg.color = Color.white;
            }

            // Khung Gỗ Loadout Phía Trước
            GameObject loadoutCard = CreateUIElement("Tray_Loadout_Wood_Frame", loadoutContainer.transform);
            RectTransform tcRT = loadoutCard.GetComponent<RectTransform>();
            tcRT.anchorMin = new Vector2(0, 0);
            tcRT.anchorMax = new Vector2(0, 0);
            tcRT.pivot = new Vector2(0, 0);
            tcRT.anchoredPosition = new Vector2(0, 0);
            tcRT.sizeDelta = new Vector2(215, 96);

            var lcImg = loadoutCard.AddComponent<Image>();
            lcImg.color = Color.white;
            lcImg.type = Image.Type.Sliced;
            Sprite trayWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Tray_Loadout_Wood_Frame.png");
            if (trayWoodSprite != null) lcImg.sprite = trayWoodSprite;
            loadoutBtn = loadoutCard.AddComponent<Button>();

            // Header "TRANG BỊ (LOADOUT)"
            GameObject lcTitle = CreateUIElement("Txt_Title", loadoutCard.transform);
            RectTransform lctRT = lcTitle.GetComponent<RectTransform>();
            lctRT.anchorMin = new Vector2(0, 1);
            lctRT.anchorMax = new Vector2(1, 1);
            lctRT.pivot = new Vector2(0, 1);
            lctRT.anchoredPosition = new Vector2(16, -6);
            lctRT.sizeDelta = new Vector2(-32, 16);
            var lctTMP = lcTitle.AddComponent<TextMeshProUGUI>();
            if (font != null) lctTMP.font = font;
            lctTMP.text = "TRANG BỊ (LOADOUT)";
            lctTMP.fontSize = 10;
            lctTMP.fontStyle = FontStyles.Bold;
            lctTMP.color = new Color(0.85f, 0.75f, 0.55f, 1f);

            // Hàng 2 Ô Trang Bị
            GameObject itemsRow = CreateUIElement("Row_Items", loadoutCard.transform);
            RectTransform irRT = itemsRow.GetComponent<RectTransform>();
            irRT.anchorMin = new Vector2(0, 0);
            irRT.anchorMax = new Vector2(1, 1);
            irRT.offsetMin = new Vector2(16, 8);
            irRT.offsetMax = new Vector2(-16, -26);

            var irHlg = itemsRow.AddComponent<HorizontalLayoutGroup>();
            irHlg.spacing = 14;
            irHlg.childAlignment = TextAnchor.MiddleLeft;
            irHlg.childControlWidth = false;
            irHlg.childControlHeight = false;

            Sprite boxSlotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Box_Skill_Icon_Wood_9Slice.png");

            // Slot 1: Primary Weapon
            GameObject pBox = CreateUIElement("Slot_Primary", itemsRow.transform);
            pBox.GetComponent<RectTransform>().sizeDelta = new Vector2(46, 46);
            var pbImg = pBox.AddComponent<Image>();
            pbImg.color = Color.white;
            pbImg.type = Image.Type.Sliced;
            if (boxSlotSprite != null) pbImg.sprite = boxSlotSprite;

            GameObject pIconObj = CreateUIElement("Icon", pBox.transform);
            SetStretchAnchor(pIconObj.GetComponent<RectTransform>());
            pIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
            pIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);
            priIcon = pIconObj.AddComponent<Image>();
            priIcon.preserveAspect = true;

            priName = null;

            // Slot 2: Pháp Bảo Hộ Thân
            relicIcons = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject rBox = CreateUIElement($"Slot_Relic_{i + 1}", itemsRow.transform);
                rBox.GetComponent<RectTransform>().sizeDelta = new Vector2(46, 46);
                var rbImg = rBox.AddComponent<Image>();
                rbImg.color = Color.white;
                rbImg.type = Image.Type.Sliced;
                if (boxSlotSprite != null) rbImg.sprite = boxSlotSprite;

                GameObject rIconObj = CreateUIElement("Icon", rBox.transform);
                SetStretchAnchor(rIconObj.GetComponent<RectTransform>());
                rIconObj.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
                rIconObj.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);
                relicIcons[i] = rIconObj.AddComponent<Image>();
                relicIcons[i].preserveAspect = true;

                if (i > 0) rBox.SetActive(false);
            }

            // ================= 2. Ở GIỮA: 3 NÚT THẺ GỖ KHÂU CHỈ =================
            GameObject navGroup = CreateUIElement("Group_NavButtons", hud.transform);
            RectTransform ngRT = navGroup.GetComponent<RectTransform>();
            ngRT.anchorMin = new Vector2(0.5f, 0);
            ngRT.anchorMax = new Vector2(0.5f, 0);
            ngRT.pivot = new Vector2(0.5f, 0);
            ngRT.anchoredPosition = new Vector2(0, 8);
            ngRT.sizeDelta = new Vector2(340, 52);

            var ngHlg = navGroup.AddComponent<HorizontalLayoutGroup>();
            ngHlg.spacing = 10;
            ngHlg.childAlignment = TextAnchor.MiddleCenter;
            ngHlg.childControlWidth = false;
            ngHlg.childControlHeight = false;

            Sprite btnNavWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Wood_Stitched.png");

            // Nút 1: ANH HÙNG
            GameObject btn1 = CreateUIElement("Btn_HeroSelect", navGroup.transform);
            btn1.GetComponent<RectTransform>().sizeDelta = new Vector2(98, 44);
            var b1Img = btn1.AddComponent<Image>();
            b1Img.color = Color.white;
            b1Img.type = Image.Type.Sliced;
            if (btnNavWoodSprite != null) b1Img.sprite = btnNavWoodSprite;
            heroBtn = btn1.AddComponent<Button>();

            GameObject b1T = CreateUIElement("Text", btn1.transform);
            SetStretchAnchor(b1T.GetComponent<RectTransform>());
            var b1TMP = b1T.AddComponent<TextMeshProUGUI>();
            if (font != null) b1TMP.font = font;
            b1TMP.text = "ANH HÙNG";
            b1TMP.fontSize = 12;
            b1TMP.fontStyle = FontStyles.Bold;
            b1TMP.alignment = TextAlignmentOptions.Center;
            b1TMP.color = new Color(0.95f, 0.88f, 0.70f, 1f);

            // Nút 2: TÀNG BẢO CÁC
            GameObject btn2 = CreateUIElement("Btn_Armory", navGroup.transform);
            btn2.GetComponent<RectTransform>().sizeDelta = new Vector2(106, 44);
            var b2Img = btn2.AddComponent<Image>();
            b2Img.color = Color.white;
            b2Img.type = Image.Type.Sliced;
            if (btnNavWoodSprite != null) b2Img.sprite = btnNavWoodSprite;
            armoryBtn = btn2.AddComponent<Button>();

            GameObject b2T = CreateUIElement("Text", btn2.transform);
            SetStretchAnchor(b2T.GetComponent<RectTransform>());
            var b2TMP = b2T.AddComponent<TextMeshProUGUI>();
            if (font != null) b2TMP.font = font;
            b2TMP.text = "TÀNG BẢO CÁC";
            b2TMP.fontSize = 12;
            b2TMP.fontStyle = FontStyles.Bold;
            b2TMP.alignment = TextAlignmentOptions.Center;
            b2TMP.color = new Color(0.95f, 0.88f, 0.70f, 1f);

            // Nút 3: MIẾU CỔ
            GameObject btn3 = CreateUIElement("Btn_SanctuaryTree", navGroup.transform);
            btn3.GetComponent<RectTransform>().sizeDelta = new Vector2(98, 44);
            var b3Img = btn3.AddComponent<Image>();
            b3Img.color = Color.white;
            b3Img.type = Image.Type.Sliced;
            if (btnNavWoodSprite != null) b3Img.sprite = btnNavWoodSprite;
            sanctuaryBtn = btn3.AddComponent<Button>();

            GameObject b3T = CreateUIElement("Text", btn3.transform);
            SetStretchAnchor(b3T.GetComponent<RectTransform>());
            var b3TMP = b3T.AddComponent<TextMeshProUGUI>();
            if (font != null) b3TMP.font = font;
            b3TMP.text = "MIẾU CỔ";
            b3TMP.fontSize = 12;
            b3TMP.fontStyle = FontStyles.Bold;
            b3TMP.alignment = TextAlignmentOptions.Center;
            b3TMP.color = new Color(0.95f, 0.88f, 0.70f, 1f);

            // ================= 3. GÓC PHẢI: NÚT XUẤT TRẬN LỤC GIÁC NGỌC HỔ PHÁCH =================
            GameObject startObj = CreateUIElement("Btn_StartRun", hud.transform);
            RectTransform stRT = startObj.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(1, 0);
            stRT.anchorMax = new Vector2(1, 0);
            stRT.pivot = new Vector2(1, 0);
            stRT.anchoredPosition = new Vector2(-15, 6);
            stRT.sizeDelta = new Vector2(165, 86);

            var stImg = startObj.AddComponent<Image>();
            stImg.color = Color.white;
            stImg.type = Image.Type.Sliced;
            Sprite btnBattleAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");
            if (btnBattleAmber != null) stImg.sprite = btnBattleAmber;
            startRunBtn = startObj.AddComponent<Button>();

            GameObject stTxt = CreateUIElement("Text", startObj.transform);
            SetStretchAnchor(stTxt.GetComponent<RectTransform>());
            stTxt.GetComponent<RectTransform>().offsetMin = new Vector2(10, 8);
            stTxt.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -8);
            var stTMP = stTxt.AddComponent<TextMeshProUGUI>();
            if (font != null) stTMP.font = font;
            stTMP.text = "XUẤT TRẬN";
            stTMP.fontSize = 22;
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

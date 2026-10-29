#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Defense3D.UI;
using ProjectZombie.Editor.UITools;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động tạo và cấu hình Prefab Màn Hình Chuẩn Bị Xuất Trận (Defense Preparation UI)
    /// theo phong cách Cổ Phong Đông Sơn 2.5D, tuân thủ nghiêm ngặt quy chuẩn Android Mobile trong AGENTS.md:
    /// - Sử dụng độc quyền TextMeshProUGUI, không dùng Text Legacy.
    /// - Tuyệt đối không hard-code Unicode Emoji (dùng Rich Text màu sắc và TMP Sprite Asset).
    /// - Tắt raycastTarget trên các thành phần trang trí tĩnh (tối ưu CPU EventSystem).
    /// - Tự động liên kết toàn bộ serialized fields vào DefensePreparationView.
    /// - Tự động ghi nhận Prefab vào UIRegistry.asset.
    /// </summary>
    [InitializeOnLoad]
    public static class DefensePreparationUIGenerator
    {
        private const string SPRITES_PATH = "Assets/Art/UI/VongXuyen/";
        private const string RESOURCES_OUTPUT_PATH = "Assets/Resources/UI/DefensePreparationUI.prefab";
        private const string PREFAB_OUTPUT_PATH = "Assets/_Prefabs/UI/DefensePreparationUI.prefab";

        static DefensePreparationUIGenerator()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(RESOURCES_OUTPUT_PATH))
                {
                    GenerateDefensePreparationUI();
                }
            };
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/1. Sảnh Chính (Meta Menu)/12. ⚡ Tạo Màn Hình Chuẩn Bị Xuất Trận (Defense Preparation UI)", priority = 115)]
        public static void GenerateDefensePreparationUI()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[DefensePreparationUIGenerator] Vui lòng tắt Play Mode trước khi tạo UI!");
                return;
            }

            Debug.Log("<color=#00FF88>[DefensePreparationUIGenerator]</color> Bắt đầu khởi tạo Prefab Chuẩn Bị Xuất Trận Cổ Phong...");

            // 1. Tải Tài nguyên Hình ảnh Cổ Phong
            Sprite modalWoodFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Frame_Modal_TangBaoCac_9Slice.png")
                                    ?? AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite cardTotemBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite btnWoodStitched = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Nav_Wood_Stitched.png")
                                     ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");
            Sprite badgePill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Settings_Parchment.png")
                                     ?? AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Parchment_Scroll.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Nav_Close_X_Wood.png");
            Sprite btnBattleHex = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Battle_Hex_Amber_Glow.png")
                                  ?? AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_StartRun_Amber.png");
            Sprite boxSkillIcon = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Box_Skill_Icon_Wood_9Slice.png");
            Sprite toggleBoxOff = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Toggle_Wood_Box_Off.png");
            Sprite toggleCheckOn = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Toggle_Wood_Checkmark_On.png");
            Sprite sliderTrack = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Track_9Slice.png");
            Sprite sliderFill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Fill_9Slice.png");

            // 2. Tải Font tiếng Việt
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset")
                                     ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset")
                                     ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset")
                                     ?? TMP_Settings.defaultFontAsset;

            // 3. Tạo Root Modal GameObject
            GameObject modalRoot = new GameObject("Modal_DefensePreparation", typeof(RectTransform), typeof(CanvasGroup), typeof(DefensePreparationView));
            RectTransform rootRT = modalRoot.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            CanvasGroup rootCG = modalRoot.GetComponent<CanvasGroup>();
            rootCG.alpha = 1f;
            rootCG.interactable = true;
            rootCG.blocksRaycasts = true;

            DefensePreparationView view = modalRoot.GetComponent<DefensePreparationView>();

            // 4. Lớp nền mờ (Dark Dim Overlay)
            GameObject overlayObj = new GameObject("Overlay_Dark", typeof(RectTransform), typeof(Image), typeof(Button));
            overlayObj.transform.SetParent(modalRoot.transform, false);
            RectTransform ovRT = overlayObj.GetComponent<RectTransform>();
            SetStretch(ovRT);
            Image ovImg = overlayObj.GetComponent<Image>();
            ovImg.color = new Color(0.02f, 0.01f, 0.03f, 0.88f);
            ovImg.raycastTarget = true;
            Button ovBtn = overlayObj.GetComponent<Button>();

            // 5. Khung Gỗ Chính (Modal Container Frame: 960x620)
            GameObject frameObj = new GameObject("Frame_Defense_Content", typeof(RectTransform), typeof(Image));
            frameObj.transform.SetParent(modalRoot.transform, false);
            RectTransform frRT = frameObj.GetComponent<RectTransform>();
            frRT.anchorMin = new Vector2(0.5f, 0.5f);
            frRT.anchorMax = new Vector2(0.5f, 0.5f);
            frRT.pivot = new Vector2(0.5f, 0.5f);
            frRT.sizeDelta = new Vector2(960f, 620f);

            Image frImg = frameObj.GetComponent<Image>();
            if (modalWoodFrame != null)
            {
                frImg.sprite = modalWoodFrame;
                frImg.type = Image.Type.Sliced;
                frImg.color = Color.white;
            }
            else
            {
                frImg.color = new Color(0.12f, 0.08f, 0.06f, 0.98f);
            }

            // Nền tối bên trong khung gỗ (Solid Inner Dark Background)
            GameObject innerBg = new GameObject("Bg_Inner_Wood", typeof(RectTransform), typeof(Image));
            innerBg.transform.SetParent(frameObj.transform, false);
            RectTransform inRT = innerBg.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(12f, 12f);
            inRT.offsetMax = new Vector2(-12f, -12f);
            Image inImg = innerBg.GetComponent<Image>();
            inImg.color = new Color(0.08f, 0.06f, 0.07f, 0.96f);
            inImg.raycastTarget = false;

            // 6. Header Banner & Tiêu Đề
            GameObject headerBannerObj = new GameObject("Banner_Header", typeof(RectTransform), typeof(Image));
            headerBannerObj.transform.SetParent(frameObj.transform, false);
            RectTransform hdRT = headerBannerObj.GetComponent<RectTransform>();
            hdRT.anchorMin = new Vector2(0.5f, 1f);
            hdRT.anchorMax = new Vector2(0.5f, 1f);
            hdRT.pivot = new Vector2(0.5f, 0.5f);
            hdRT.anchoredPosition = new Vector2(0f, 4f);
            hdRT.sizeDelta = new Vector2(460f, 64f);

            Image hdImg = headerBannerObj.GetComponent<Image>();
            if (bannerParchment != null)
            {
                hdImg.sprite = bannerParchment;
                hdImg.type = Image.Type.Sliced;
                hdImg.color = Color.white;
            }
            else
            {
                hdImg.color = new Color(0.2f, 0.14f, 0.1f, 1f);
            }
            hdImg.raycastTarget = false;

            TextMeshProUGUI titleTMP = CreateTMPText("Txt_Title", headerBannerObj.transform, vietFont, "CHUẨN BỊ XUẤT TRẬN", 22, TextAlignmentOptions.Center, new Color(0.24f, 0.14f, 0.08f));
            titleTMP.fontStyle = FontStyles.Bold;
            SetStretch(titleTMP.GetComponent<RectTransform>());

            TextMeshProUGUI subtitleTMP = CreateTMPText("Txt_Subtitle", frameObj.transform, vietFont, "Thiết lập Đội hình, Pháp Bảo và Trụ trước khi tiến vào Vực Âm Ty", 13, TextAlignmentOptions.Center, new Color(0.85f, 0.8f, 0.72f));
            RectTransform subRT = subtitleTMP.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 1f);
            subRT.anchorMax = new Vector2(0.5f, 1f);
            subRT.pivot = new Vector2(0.5f, 1f);
            subRT.anchoredPosition = new Vector2(0f, -38f);
            subRT.sizeDelta = new Vector2(700f, 24f);

            // Nút Đóng (Btn_Close)
            GameObject closeBtnObj = new GameObject("Btn_Close_X", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(frameObj.transform, false);
            RectTransform cbRT = closeBtnObj.GetComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(1f, 1f);
            cbRT.anchorMax = new Vector2(1f, 1f);
            cbRT.pivot = new Vector2(1f, 1f);
            cbRT.anchoredPosition = new Vector2(-16f, -16f);
            cbRT.sizeDelta = new Vector2(44f, 44f);
            Image cbImg = closeBtnObj.GetComponent<Image>();
            if (btnCloseX != null)
            {
                cbImg.sprite = btnCloseX;
                cbImg.color = Color.white;
            }
            else
            {
                cbImg.color = new Color(0.85f, 0.25f, 0.2f, 1f);
            }
            cbImg.raycastTarget = true;
            Button closeBtn = closeBtnObj.GetComponent<Button>();

            // 7. Khu Vực 4 Cột Thẻ Dàn Ngang (Hero, Relic, Minions, Towers)
            // Center Y: 12f, Height: 350f. Spacing: 16f, Width mỗi thẻ: 212f
            float cardY = 14f;
            float cardHeight = 356f;
            float cardWidth = 212f;

            // -------------------------------------------------------------
            // CỘT 1: ANH HÙNG (Hero Slot) - X = -342
            // -------------------------------------------------------------
            GameObject heroCardObj = CreateCardFrame("Card_Hero", frameObj.transform, new Vector2(-342f, cardY), new Vector2(cardWidth, cardHeight), cardTotemBg);
            CreateHeaderPill("Pill_Hero", heroCardObj.transform, badgePill, vietFont, "<color=#FFD700>ANH HÙNG</color>");

            // Khung Avatar
            GameObject heroAvatarBox = CreateUIObject("Box_Avatar", heroCardObj.transform);
            RectTransform habRT = heroAvatarBox.GetComponent<RectTransform>();
            habRT.anchoredPosition = new Vector2(0f, 65f);
            habRT.sizeDelta = new Vector2(96f, 96f);
            Image habImg = heroAvatarBox.AddComponent<Image>();
            if (boxSkillIcon != null)
            {
                habImg.sprite = boxSkillIcon;
                habImg.type = Image.Type.Sliced;
            }
            else
            {
                habImg.color = new Color(0.2f, 0.16f, 0.14f, 1f);
            }
            habImg.raycastTarget = false;

            GameObject heroAvatarImgObj = CreateUIObject("Img_HeroAvatar", heroAvatarBox.transform);
            RectTransform haiRT = heroAvatarImgObj.GetComponent<RectTransform>();
            haiRT.anchorMin = new Vector2(0.5f, 0.5f);
            haiRT.anchorMax = new Vector2(0.5f, 0.5f);
            haiRT.sizeDelta = new Vector2(84f, 84f);
            Image heroAvatarImg = heroAvatarImgObj.AddComponent<Image>();
            heroAvatarImg.color = Color.white;
            heroAvatarImg.raycastTarget = false;

            TextMeshProUGUI heroNameTMP = CreateTMPText("Txt_HeroName", heroCardObj.transform, vietFont, "<color=#00E5FF>Tướng:</color> <b>Thư Sinh</b>", 15, TextAlignmentOptions.Center, Color.white);
            RectTransform hnRT = heroNameTMP.GetComponent<RectTransform>();
            hnRT.anchoredPosition = new Vector2(0f, -2f);
            hnRT.sizeDelta = new Vector2(196f, 26f);

            TextMeshProUGUI heroRoleTMP = CreateTMPText("Txt_HeroRole", heroCardObj.transform, vietFont, "<color=#00FF88>Anh Hùng Phòng Thủ</color>", 12, TextAlignmentOptions.Center, Color.white);
            RectTransform hrRT = heroRoleTMP.GetComponent<RectTransform>();
            hrRT.anchoredPosition = new Vector2(0f, -32f);
            hrRT.sizeDelta = new Vector2(196f, 32f);

            Button heroSlotBtn = CreateActionButton("Btn_HeroSlot", heroCardObj.transform, btnWoodStitched, vietFont, "ĐỔI TƯỚNG", new Vector2(0f, -125f), new Vector2(170f, 40f));

            // -------------------------------------------------------------
            // CỘT 2: PHÁP BẢO (Relic Slot) - X = -114
            // -------------------------------------------------------------
            GameObject relicCardObj = CreateCardFrame("Card_Relic", frameObj.transform, new Vector2(-114f, cardY), new Vector2(cardWidth, cardHeight), cardTotemBg);
            CreateHeaderPill("Pill_Relic", relicCardObj.transform, badgePill, vietFont, "<color=#FFD700>PHÁP BẢO</color>");

            // Khung Icon Pháp Bảo
            GameObject relicIconBox = CreateUIObject("Box_RelicIcon", relicCardObj.transform);
            RectTransform ribRT = relicIconBox.GetComponent<RectTransform>();
            ribRT.anchoredPosition = new Vector2(0f, 65f);
            ribRT.sizeDelta = new Vector2(96f, 96f);
            Image ribImg = relicIconBox.AddComponent<Image>();
            if (boxSkillIcon != null)
            {
                ribImg.sprite = boxSkillIcon;
                ribImg.type = Image.Type.Sliced;
            }
            else
            {
                ribImg.color = new Color(0.2f, 0.16f, 0.14f, 1f);
            }
            ribImg.raycastTarget = false;

            GameObject relicIconImgObj = CreateUIObject("Img_RelicIcon", relicIconBox.transform);
            RectTransform riiRT = relicIconImgObj.GetComponent<RectTransform>();
            riiRT.anchorMin = new Vector2(0.5f, 0.5f);
            riiRT.anchorMax = new Vector2(0.5f, 0.5f);
            riiRT.sizeDelta = new Vector2(84f, 84f);
            Image relicIconImg = relicIconImgObj.AddComponent<Image>();
            relicIconImg.color = Color.white;
            relicIconImg.raycastTarget = false;

            TextMeshProUGUI relicNameTMP = CreateTMPText("Txt_RelicName", relicCardObj.transform, vietFont, "<color=#FFD700>Pháp Bảo:</color> <b>Dép Tổ Ong</b>", 15, TextAlignmentOptions.Center, Color.white);
            RectTransform rnRT = relicNameTMP.GetComponent<RectTransform>();
            rnRT.anchoredPosition = new Vector2(0f, -2f);
            rnRT.sizeDelta = new Vector2(196f, 26f);

            TextMeshProUGUI relicDescTMP = CreateTMPText("Txt_RelicDesc", relicCardObj.transform, vietFont, "Kích hoạt ném dép gây choáng và đẩy lùi quái.", 12, TextAlignmentOptions.Center, new Color(0.85f, 0.82f, 0.75f));
            RectTransform rdRT = relicDescTMP.GetComponent<RectTransform>();
            rdRT.anchoredPosition = new Vector2(0f, -40f);
            rdRT.sizeDelta = new Vector2(196f, 50f);

            Button relicSlotBtn = CreateActionButton("Btn_RelicSlot", relicCardObj.transform, btnWoodStitched, vietFont, "ĐỔI PHÁP BẢO", new Vector2(0f, -125f), new Vector2(170f, 40f));

            // -------------------------------------------------------------
            // CỘT 3: LÍNH TRỢ CHIẾN (Minion Slots - 3 slots) - X = 114
            // -------------------------------------------------------------
            GameObject minionCardObj = CreateCardFrame("Card_Minions", frameObj.transform, new Vector2(114f, cardY), new Vector2(cardWidth, cardHeight), cardTotemBg);
            CreateHeaderPill("Pill_Minions", minionCardObj.transform, badgePill, vietFont, "<color=#FFD700>LÍNH TRỢ CHIẾN</color>");

            TextMeshProUGUI minionSubTMP = CreateTMPText("Txt_MinionSub", minionCardObj.transform, vietFont, "Tối đa 3 Lính xuất trận", 11, TextAlignmentOptions.Center, new Color(0.7f, 0.65f, 0.6f));
            RectTransform msubRT = minionSubTMP.GetComponent<RectTransform>();
            msubRT.anchoredPosition = new Vector2(0f, 108f);
            msubRT.sizeDelta = new Vector2(196f, 18f);

            Button[] minionButtons = new Button[3];
            Image[] minionCheckmarks = new Image[3];
            TextMeshProUGUI[] minionNames = new TextMeshProUGUI[3];

            string[] defaultMinionNames = new[] { "Dân Binh", "Phu Tạp Dịch", "Thầy Lang" };
            float[] minionRowY = new[] { 60f, 0f, -60f };

            for (int i = 0; i < 3; i++)
            {
                CreateToggleSlotRow($"Slot_Minion_{i}", minionCardObj.transform, btnWoodStitched, toggleBoxOff, toggleCheckOn, vietFont,
                    defaultMinionNames[i], new Vector2(0f, minionRowY[i]), new Vector2(192f, 48f),
                    out minionButtons[i], out minionCheckmarks[i], out minionNames[i]);
            }

            TextMeshProUGUI minionHintTMP = CreateTMPText("Txt_MinionHint", minionCardObj.transform, vietFont, "Nhấn thẻ để bật / tắt", 11, TextAlignmentOptions.Center, new Color(0.6f, 0.55f, 0.5f));
            RectTransform mhRT = minionHintTMP.GetComponent<RectTransform>();
            mhRT.anchoredPosition = new Vector2(0f, -125f);
            mhRT.sizeDelta = new Vector2(180f, 20f);

            // -------------------------------------------------------------
            // CỘT 4: TRỤ PHÒNG THỦ (Tower Slots - 1 slot) - X = 342
            // -------------------------------------------------------------
            GameObject towerCardObj = CreateCardFrame("Card_Towers", frameObj.transform, new Vector2(342f, cardY), new Vector2(cardWidth, cardHeight), cardTotemBg);
            CreateHeaderPill("Pill_Towers", towerCardObj.transform, badgePill, vietFont, "<color=#FFD700>TRỤ THỦ THÀNH</color>");

            TextMeshProUGUI towerSubTMP = CreateTMPText("Txt_TowerSub", towerCardObj.transform, vietFont, "Công sự phòng vệ", 11, TextAlignmentOptions.Center, new Color(0.7f, 0.65f, 0.6f));
            RectTransform tsubRT = towerSubTMP.GetComponent<RectTransform>();
            tsubRT.anchoredPosition = new Vector2(0f, 108f);
            tsubRT.sizeDelta = new Vector2(196f, 18f);

            Button[] towerButtons = new Button[1];
            Image[] towerCheckmarks = new Image[1];
            TextMeshProUGUI[] towerNames = new TextMeshProUGUI[1];

            CreateToggleSlotRow("Slot_Tower_0", towerCardObj.transform, btnWoodStitched, toggleBoxOff, toggleCheckOn, vietFont,
                "Tháp Cung Tiễn", new Vector2(0f, 45f), new Vector2(192f, 52f),
                out towerButtons[0], out towerCheckmarks[0], out towerNames[0]);

            TextMeshProUGUI towerDescTMP = CreateTMPText("Txt_TowerDesc", towerCardObj.transform, vietFont, "<color=#FFCC00>Tháp Cung Tiễn:</color>\nBắn liên tục tiễn gỗ tầm xa cản bước yêu ma tràn bờ.", 11, TextAlignmentOptions.Center, new Color(0.8f, 0.75f, 0.7f));
            RectTransform tdRT = towerDescTMP.GetComponent<RectTransform>();
            tdRT.anchoredPosition = new Vector2(0f, -40f);
            tdRT.sizeDelta = new Vector2(192f, 70f);

            TextMeshProUGUI towerHintTMP = CreateTMPText("Txt_TowerHint", towerCardObj.transform, vietFont, "Trụ tự động xây dựng", 11, TextAlignmentOptions.Center, new Color(0.6f, 0.55f, 0.5f));
            RectTransform thRT = towerHintTMP.GetComponent<RectTransform>();
            thRT.anchoredPosition = new Vector2(0f, -125f);
            thRT.sizeDelta = new Vector2(180f, 20f);

            // 8. Khu Vực Dưới (Footer: Thông Báo Hợp Lệ & Nút Xuất Trận)
            TextMeshProUGUI validationTMP = CreateTMPText("Txt_ValidationMessage", frameObj.transform, vietFont, "<color=#00FF88>Đội hình đã hợp lệ, sẵn sàng tham chiến!</color>", 14, TextAlignmentOptions.Center, Color.white);
            RectTransform valRT = validationTMP.GetComponent<RectTransform>();
            valRT.anchorMin = new Vector2(0.5f, 0f);
            valRT.anchorMax = new Vector2(0.5f, 0f);
            valRT.pivot = new Vector2(0.5f, 0.5f);
            valRT.anchoredPosition = new Vector2(0f, 85f);
            valRT.sizeDelta = new Vector2(600f, 26f);

            // Nút Xuất Trận (Btn_StartBattle)
            GameObject startBattleBtnObj = new GameObject("Btn_StartBattle", typeof(RectTransform), typeof(Image), typeof(Button));
            startBattleBtnObj.transform.SetParent(frameObj.transform, false);
            RectTransform sbRT = startBattleBtnObj.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(0.5f, 0f);
            sbRT.anchorMax = new Vector2(0.5f, 0f);
            sbRT.pivot = new Vector2(0.5f, 0.5f);
            sbRT.anchoredPosition = new Vector2(0f, 44f);
            sbRT.sizeDelta = new Vector2(280f, 56f);

            Image sbImg = startBattleBtnObj.GetComponent<Image>();
            if (btnBattleHex != null)
            {
                sbImg.sprite = btnBattleHex;
                sbImg.type = Image.Type.Sliced;
                sbImg.color = Color.white;
            }
            else
            {
                sbImg.color = new Color(0.9f, 0.45f, 0.1f, 1f);
            }
            sbImg.raycastTarget = true;
            Button startBattleBtn = startBattleBtnObj.GetComponent<Button>();

            TextMeshProUGUI startBattleTMP = CreateTMPText("Txt_Label", startBattleBtnObj.transform, vietFont, "<b>XUẤT TRẬN</b>", 20, TextAlignmentOptions.Center, Color.white);
            SetStretch(startBattleTMP.GetComponent<RectTransform>());

            // 9. Lớp Phủ Tải Trận (Loading Overlay)
            GameObject loadingOverlayObj = new GameObject("Loading_Overlay", typeof(RectTransform), typeof(Image));
            loadingOverlayObj.transform.SetParent(modalRoot.transform, false);
            RectTransform loRT = loadingOverlayObj.GetComponent<RectTransform>();
            SetStretch(loRT);
            Image loImg = loadingOverlayObj.GetComponent<Image>();
            loImg.color = new Color(0.04f, 0.03f, 0.05f, 0.94f);
            loImg.raycastTarget = true; // Chặn tương tác bên dưới khi tải trận

            TextMeshProUGUI loadingStatusTMP = CreateTMPText("Txt_LoadingStatus", loadingOverlayObj.transform, vietFont, "Đang khởi tạo trận địa Sông Quê...", 18, TextAlignmentOptions.Center, Color.white);
            RectTransform lsRT = loadingStatusTMP.GetComponent<RectTransform>();
            lsRT.anchorMin = new Vector2(0.5f, 0.5f);
            lsRT.anchorMax = new Vector2(0.5f, 0.5f);
            lsRT.anchoredPosition = new Vector2(0f, 32f);
            lsRT.sizeDelta = new Vector2(500f, 30f);

            // Loading Slider
            GameObject sliderObj = new GameObject("Slider_LoadingProgress", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(loadingOverlayObj.transform, false);
            RectTransform slRT = sliderObj.GetComponent<RectTransform>();
            slRT.anchorMin = new Vector2(0.5f, 0.5f);
            slRT.anchorMax = new Vector2(0.5f, 0.5f);
            slRT.anchoredPosition = new Vector2(0f, -16f);
            slRT.sizeDelta = new Vector2(400f, 24f);

            Image slTrackImg = sliderObj.AddComponent<Image>();
            if (sliderTrack != null)
            {
                slTrackImg.sprite = sliderTrack;
                slTrackImg.type = Image.Type.Sliced;
            }
            else
            {
                slTrackImg.color = new Color(0.12f, 0.1f, 0.14f, 1f);
            }
            slTrackImg.raycastTarget = false;

            Slider loadingSlider = sliderObj.GetComponent<Slider>();
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;
            loadingSlider.interactable = false;

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRT = fillArea.GetComponent<RectTransform>();
            SetStretch(faRT);

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fRT = fillObj.GetComponent<RectTransform>();
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.sizeDelta = Vector2.zero;

            Image fImg = fillObj.GetComponent<Image>();
            if (sliderFill != null)
            {
                fImg.sprite = sliderFill;
                fImg.type = Image.Type.Sliced;
                fImg.color = Color.white;
            }
            else
            {
                fImg.color = new Color(0f, 0.9f, 1f, 1f);
            }
            fImg.raycastTarget = false;

            loadingSlider.fillRect = fRT;
            loadingSlider.targetGraphic = slTrackImg;

            loadingOverlayObj.SetActive(false); // Ẩn mặc định

            // 10. Liên kết toàn bộ SerializedProperties vào DefensePreparationView
            SerializedObject soView = new SerializedObject(view);

            // 1. Header
            soView.FindProperty("_titleText").objectReferenceValue = titleTMP;
            soView.FindProperty("_subtitleText").objectReferenceValue = subtitleTMP;
            soView.FindProperty("_validationMessageText").objectReferenceValue = validationTMP;

            // 2. Hero
            soView.FindProperty("_heroNameText").objectReferenceValue = heroNameTMP;
            soView.FindProperty("_heroRoleText").objectReferenceValue = heroRoleTMP;
            soView.FindProperty("_heroAvatarImage").objectReferenceValue = heroAvatarImg;
            soView.FindProperty("_heroSlotButton").objectReferenceValue = heroSlotBtn;

            // 3. Relic
            soView.FindProperty("_relicNameText").objectReferenceValue = relicNameTMP;
            soView.FindProperty("_relicDescText").objectReferenceValue = relicDescTMP;
            soView.FindProperty("_relicIconImage").objectReferenceValue = relicIconImg;
            soView.FindProperty("_relicSlotButton").objectReferenceValue = relicSlotBtn;

            // 4. Minions (3 slots)
            SerializedProperty minionBtnsProp = soView.FindProperty("_minionSlotButtons");
            SerializedProperty minionChecksProp = soView.FindProperty("_minionSlotCheckmarks");
            SerializedProperty minionNamesProp = soView.FindProperty("_minionSlotNameTexts");

            minionBtnsProp.arraySize = 3;
            minionChecksProp.arraySize = 3;
            minionNamesProp.arraySize = 3;

            for (int i = 0; i < 3; i++)
            {
                minionBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = minionButtons[i];
                minionChecksProp.GetArrayElementAtIndex(i).objectReferenceValue = minionCheckmarks[i];
                minionNamesProp.GetArrayElementAtIndex(i).objectReferenceValue = minionNames[i];
            }

            // 5. Towers (1 slot)
            SerializedProperty towerBtnsProp = soView.FindProperty("_towerSlotButtons");
            SerializedProperty towerChecksProp = soView.FindProperty("_towerSlotCheckmarks");
            SerializedProperty towerNamesProp = soView.FindProperty("_towerSlotNameTexts");

            towerBtnsProp.arraySize = 1;
            towerChecksProp.arraySize = 1;
            towerNamesProp.arraySize = 1;

            towerBtnsProp.GetArrayElementAtIndex(0).objectReferenceValue = towerButtons[0];
            towerChecksProp.GetArrayElementAtIndex(0).objectReferenceValue = towerCheckmarks[0];
            towerNamesProp.GetArrayElementAtIndex(0).objectReferenceValue = towerNames[0];

            // 6. Actions
            soView.FindProperty("_startBattleButton").objectReferenceValue = startBattleBtn;
            soView.FindProperty("_startBattleButtonText").objectReferenceValue = startBattleTMP;
            soView.FindProperty("_closeButton").objectReferenceValue = closeBtn;

            // 7. Loading Overlay
            soView.FindProperty("_loadingOverlay").objectReferenceValue = loadingOverlayObj;
            soView.FindProperty("_loadingProgressBar").objectReferenceValue = loadingSlider;
            soView.FindProperty("_loadingStatusText").objectReferenceValue = loadingStatusTMP;

            soView.ApplyModifiedProperties();

            DefensePreparationUIStyler.Apply(modalRoot);

            // 11. Lưu Prefab vào Resources/UI và _Prefabs/UI
            EnsureDirectoryExists(Path.GetDirectoryName(RESOURCES_OUTPUT_PATH));
            EnsureDirectoryExists(Path.GetDirectoryName(PREFAB_OUTPUT_PATH));

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(modalRoot, RESOURCES_OUTPUT_PATH);
            PrefabUtility.SaveAsPrefabAsset(modalRoot, PREFAB_OUTPUT_PATH);

            Object.DestroyImmediate(modalRoot);
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88>[DefensePreparationUIGenerator]</color> Đã tạo thành công Prefab tại: {RESOURCES_OUTPUT_PATH}");

            // 12. Tự động đồng bộ vào UIRegistrySO
            UIRegistrySetupTool.CreateOrUpdateUIRegistry();

            if (savedPrefab != null)
            {
                Selection.activeObject = savedPrefab;
            }
        }

        // ====================================================================
        // HELPER BUILDERS
        // ====================================================================

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureDirectoryExists(string dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, TMP_FontAsset font, string text, float size, TextAlignmentOptions align, Color color)
        {
            GameObject go = CreateUIObject(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false; // Tắt raycastTarget trên text tĩnh theo AGENTS.md
            return tmp;
        }

        private static GameObject CreateCardFrame(string name, Transform parent, Vector2 pos, Vector2 size, Sprite bgSprite)
        {
            GameObject cardObj = CreateUIObject(name, parent);
            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = cardObj.AddComponent<Image>();
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.12f, 0.09f, 0.08f, 0.95f);
            }
            img.raycastTarget = false;

            return cardObj;
        }

        private static void CreateHeaderPill(string name, Transform parent, Sprite pillSprite, TMP_FontAsset font, string text)
        {
            GameObject pillObj = CreateUIObject(name, parent);
            RectTransform rt = pillObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 150f);
            rt.sizeDelta = new Vector2(184f, 32f);

            Image img = pillObj.AddComponent<Image>();
            if (pillSprite != null)
            {
                img.sprite = pillSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.18f, 0.12f, 0.09f, 1f);
            }
            img.raycastTarget = false;

            var tmp = CreateTMPText("Txt_Label", pillObj.transform, font, text, 14, TextAlignmentOptions.Center, Color.white);
            tmp.fontStyle = FontStyles.Bold;
            SetStretch(tmp.GetComponent<RectTransform>());
        }

        private static Button CreateActionButton(string name, Transform parent, Sprite btnSprite, TMP_FontAsset font, string label, Vector2 pos, Vector2 size)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.24f, 0.18f, 0.14f, 1f);
            }
            img.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();

            var tmp = CreateTMPText("Txt_Label", btnObj.transform, font, label, 14, TextAlignmentOptions.Center, Color.white);
            SetStretch(tmp.GetComponent<RectTransform>());

            return btn;
        }

        private static void CreateToggleSlotRow(string name, Transform parent, Sprite rowBg, Sprite boxOff, Sprite checkOn, TMP_FontAsset font,
            string labelText, Vector2 pos, Vector2 size, out Button slotBtn, out Image checkmarkImg, out TextMeshProUGUI labelTMP)
        {
            GameObject rowObj = CreateUIObject(name, parent);
            RectTransform rt = rowObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = rowObj.AddComponent<Image>();
            if (rowBg != null)
            {
                img.sprite = rowBg;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.16f, 0.12f, 0.1f, 1f);
            }
            img.raycastTarget = true;

            slotBtn = rowObj.AddComponent<Button>();

            // Khung Checkbox (Bên trái)
            GameObject checkObj = CreateUIObject("Box_Check", rowObj.transform);
            RectTransform cRT = checkObj.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0f, 0.5f);
            cRT.anchorMax = new Vector2(0f, 0.5f);
            cRT.pivot = new Vector2(0.5f, 0.5f);
            cRT.anchoredPosition = new Vector2(22f, 0f);
            cRT.sizeDelta = new Vector2(26f, 26f);

            Image cImg = checkObj.AddComponent<Image>();
            if (boxOff != null)
            {
                cImg.sprite = boxOff;
            }
            else
            {
                cImg.color = new Color(0.25f, 0.2f, 0.16f, 1f);
            }
            cImg.raycastTarget = false;

            // Dấu tích xanh (Checkmark bên trong)
            GameObject checkmarkObj = CreateUIObject("Img_Checkmark", checkObj.transform);
            RectTransform cmRT = checkmarkObj.GetComponent<RectTransform>();
            cmRT.anchorMin = new Vector2(0.5f, 0.5f);
            cmRT.anchorMax = new Vector2(0.5f, 0.5f);
            cmRT.sizeDelta = new Vector2(22f, 22f);

            checkmarkImg = checkmarkObj.AddComponent<Image>();
            if (checkOn != null)
            {
                checkmarkImg.sprite = checkOn;
                checkmarkImg.color = Color.white;
            }
            else
            {
                checkmarkImg.color = new Color(0f, 0.9f, 0.4f, 1f);
            }
            checkmarkImg.raycastTarget = false;
            checkmarkObj.SetActive(true); // Mặc định bật

            // Text tên slot (Bên phải)
            labelTMP = CreateTMPText("Txt_SlotName", rowObj.transform, font, labelText, 14, TextAlignmentOptions.Left, Color.white);
            RectTransform lRT = labelTMP.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0f, 0.5f);
            lRT.anchorMax = new Vector2(1f, 0.5f);
            lRT.pivot = new Vector2(0f, 0.5f);
            lRT.anchoredPosition = new Vector2(44f, 0f);
            lRT.sizeDelta = new Vector2(-48f, 32f);
        }
    }
}
#endif

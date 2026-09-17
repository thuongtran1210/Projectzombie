#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.Lobby;
using ProjectZombie.Editor.UITools;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Tool độc lập (Non-destructive) tự động tạo và cấu hình Prefab Modal Sảnh Đồng Đội (Lobby Co-op UI).
    /// Đảm bảo không làm thay đổi, ghi đè hoặc ảnh hưởng tới các UI khác trong Scene và Project.
    /// </summary>
    public static class LobbyUIGenerator
    {
        private const string SPRITES_PATH = "Assets/Art/UI/VongXuyen/";
        private const string PREFAB_OUTPUT_PATH = "Assets/_Prefabs/UI/LobbyModalUI.prefab";
        private const string RESOURCES_OUTPUT_PATH = "Assets/Resources/UI/LobbyModalUI.prefab";

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/1. Sảnh Chính (Meta Menu)/8. ⚡ Tạo Modal Sảnh Đồng Đội (Lobby Co-op UI)", priority = 109)]
        public static void GenerateLobbyModal()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[LobbyUIGenerator] Vui lòng tắt Play Mode trước khi tạo UI!");
                return;
            }

            Debug.Log("<color=#00FF88>[LobbyUIGenerator]</color> Đang phân tích Scene và nạp tài nguyên Cổ Phong...");

            // 1. Nạp Tài nguyên Hình ảnh & Font
            Sprite modalWoodFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Frame_Modal_TangBaoCac_9Slice.png");
            if (modalWoodFrame == null) modalWoodFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Card_Upgrade_Wood_Totem_9Slice.png");

            Sprite cardTotemBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite btnWoodStitched = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Nav_Wood_Stitched.png");
            if (btnWoodStitched == null) btnWoodStitched = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");

            Sprite badgePill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Settings_Parchment.png");
            if (bannerParchment == null) bannerParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Banner_Parchment_Scroll.png");

            Sprite sliderTrack = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Slider_Wood_Track_9Slice.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITES_PATH + "Btn_Nav_Close_X_Wood.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 2. Tạo Root Modal
            GameObject modalRoot = new GameObject("Modal_Lobby", typeof(RectTransform), typeof(CanvasGroup), typeof(LobbyView), typeof(LobbyPresenter));
            RectTransform rootRT = modalRoot.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            CanvasGroup rootCG = modalRoot.GetComponent<CanvasGroup>();
            rootCG.alpha = 1f;
            rootCG.interactable = true;
            rootCG.blocksRaycasts = true;

            // 3. Dark Dim Overlay
            GameObject overlayObj = new GameObject("Overlay_Dark", typeof(RectTransform), typeof(Image), typeof(Button));
            overlayObj.transform.SetParent(modalRoot.transform, false);
            RectTransform ovRT = overlayObj.GetComponent<RectTransform>();
            ovRT.anchorMin = Vector2.zero;
            ovRT.anchorMax = Vector2.one;
            ovRT.offsetMin = Vector2.zero;
            ovRT.offsetMax = Vector2.zero;
            Image ovImg = overlayObj.GetComponent<Image>();
            ovImg.color = new Color(0.02f, 0.01f, 0.03f, 0.85f);
            Button ovBtn = overlayObj.GetComponent<Button>();

            // 4. Modal Container Frame (Kích thước 680x560)
            GameObject frameObj = new GameObject("Frame_Lobby_Content", typeof(RectTransform), typeof(Image));
            frameObj.transform.SetParent(modalRoot.transform, false);
            RectTransform frRT = frameObj.GetComponent<RectTransform>();
            frRT.anchorMin = new Vector2(0.5f, 0.5f);
            frRT.anchorMax = new Vector2(0.5f, 0.5f);
            frRT.pivot = new Vector2(0.5f, 0.5f);
            frRT.sizeDelta = new Vector2(680f, 560f);

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

            // 5. Nút Đóng (X Button)
            GameObject closeBtnObj = new GameObject("Btn_Close_X", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(frameObj.transform, false);
            RectTransform cbRT = closeBtnObj.GetComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(1f, 1f);
            cbRT.anchorMax = new Vector2(1f, 1f);
            cbRT.pivot = new Vector2(1f, 1f);
            cbRT.anchoredPosition = new Vector2(-12f, -12f);
            cbRT.sizeDelta = new Vector2(44f, 44f);
            Image cbImg = closeBtnObj.GetComponent<Image>();
            if (btnCloseX != null)
            {
                cbImg.sprite = btnCloseX;
                cbImg.color = Color.white;
            }
            else
            {
                cbImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            }
            Button closeBtn = closeBtnObj.GetComponent<Button>();

            // 6. Header Banner Tiêu Đề
            GameObject headerObj = new GameObject("Banner_Header", typeof(RectTransform), typeof(Image));
            headerObj.transform.SetParent(frameObj.transform, false);
            RectTransform hdRT = headerObj.GetComponent<RectTransform>();
            hdRT.anchorMin = new Vector2(0.5f, 1f);
            hdRT.anchorMax = new Vector2(0.5f, 1f);
            hdRT.pivot = new Vector2(0.5f, 0.5f);
            hdRT.anchoredPosition = new Vector2(0f, 6f);
            hdRT.sizeDelta = new Vector2(340f, 56f);
            Image hdImg = headerObj.GetComponent<Image>();
            if (bannerParchment != null)
            {
                hdImg.sprite = bannerParchment;
                hdImg.type = Image.Type.Sliced;
                hdImg.color = Color.white;
            }
            else
            {
                hdImg.color = new Color(0.18f, 0.12f, 0.08f, 1f);
            }

            GameObject titleTextObj = CreateTMPText(headerObj.transform, "Text_Title", "<color=#FFD700>SẢNH ĐỒNG ĐỘI (CO-OP)</color>", 18f, TextAlignmentOptions.Center, vietFont);
            RectTransform ttRT = titleTextObj.GetComponent<RectTransform>();
            ttRT.anchorMin = Vector2.zero;
            ttRT.anchorMax = Vector2.one;
            ttRT.offsetMin = Vector2.zero;
            ttRT.offsetMax = Vector2.zero;

            // ==========================================
            // 7. ENTRY PANEL (Panel chọn Tạo phòng / Nhập mã)
            // ==========================================
            GameObject entryPanel = new GameObject("Panel_Entry", typeof(RectTransform));
            entryPanel.transform.SetParent(frameObj.transform, false);
            RectTransform epRT = entryPanel.GetComponent<RectTransform>();
            epRT.anchorMin = Vector2.zero;
            epRT.anchorMax = Vector2.one;
            epRT.offsetMin = new Vector2(30f, 30f);
            epRT.offsetMax = new Vector2(-30f, -40f);

            // Subtitle
            GameObject subText = CreateTMPText(entryPanel.transform, "Text_Subtitle", "<color=#CCCCCC>Tạo phòng mới hoặc nhập mã 6 ký tự để cùng săn Boss</color>", 14f, TextAlignmentOptions.Center, vietFont);
            RectTransform stRT = subText.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0f, 1f);
            stRT.anchorMax = new Vector2(1f, 1f);
            stRT.pivot = new Vector2(0.5f, 1f);
            stRT.anchoredPosition = new Vector2(0f, -10f);
            stRT.sizeDelta = new Vector2(0f, 30f);

            // Nút Tạo Phòng Mới
            GameObject createRoomBtnObj = CreateButtonWithText(entryPanel.transform, "Btn_CreateRoom", "<color=#FFD700>TẠO PHÒNG MỚI</color>", new Vector2(0f, 80f), new Vector2(280f, 52f), btnWoodStitched, vietFont, 16f);
            Button createRoomBtn = createRoomBtnObj.GetComponent<Button>();

            // Phân cách (Separator)
            GameObject sepText = CreateTMPText(entryPanel.transform, "Text_Separator", "<color=#777777>——— HOẶC NHẬP MÃ PHÒNG ———</color>", 13f, TextAlignmentOptions.Center, vietFont);
            RectTransform spRT = sepText.GetComponent<RectTransform>();
            spRT.anchorMin = new Vector2(0.5f, 0.5f);
            spRT.anchorMax = new Vector2(0.5f, 0.5f);
            spRT.pivot = new Vector2(0.5f, 0.5f);
            spRT.anchoredPosition = new Vector2(0f, 20f);
            spRT.sizeDelta = new Vector2(320f, 30f);

            // Input Field Nhập Mã Phòng
            GameObject inputObj = new GameObject("InputField_RoomCode", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputObj.transform.SetParent(entryPanel.transform, false);
            RectTransform inRT = inputObj.GetComponent<RectTransform>();
            inRT.anchorMin = new Vector2(0.5f, 0.5f);
            inRT.anchorMax = new Vector2(0.5f, 0.5f);
            inRT.pivot = new Vector2(0.5f, 0.5f);
            inRT.anchoredPosition = new Vector2(0f, -30f);
            inRT.sizeDelta = new Vector2(260f, 48f);

            Image inBg = inputObj.GetComponent<Image>();
            if (sliderTrack != null)
            {
                inBg.sprite = sliderTrack;
                inBg.type = Image.Type.Sliced;
                inBg.color = Color.white;
            }
            else
            {
                inBg.color = new Color(0.08f, 0.05f, 0.04f, 0.95f);
            }

            TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
            inputField.characterLimit = 6;
            inputField.contentType = TMP_InputField.ContentType.Alphanumeric;

            // Input Text Area
            GameObject textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRT = textArea.GetComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero;
            taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(16f, 6f);
            taRT.offsetMax = new Vector2(-16f, -6f);

            // Placeholder Text
            GameObject placeholderObj = CreateTMPText(textArea.transform, "Placeholder", "Nhập mã 6 ký tự...", 16f, TextAlignmentOptions.Center, vietFont);
            var phTMP = placeholderObj.GetComponent<TextMeshProUGUI>();
            phTMP.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            phTMP.fontStyle = FontStyles.Italic;
            RectTransform phRT = placeholderObj.GetComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero;
            phRT.offsetMax = Vector2.zero;

            // Main Text Component
            GameObject textComponentObj = CreateTMPText(textArea.transform, "Text", "", 18f, TextAlignmentOptions.Center, vietFont);
            var textTMP = textComponentObj.GetComponent<TextMeshProUGUI>();
            textTMP.color = new Color(1f, 0.84f, 0f, 1f); // Gold
            textTMP.fontStyle = FontStyles.Bold;
            RectTransform tcRT = textComponentObj.GetComponent<RectTransform>();
            tcRT.anchorMin = Vector2.zero;
            tcRT.anchorMax = Vector2.one;
            tcRT.offsetMin = Vector2.zero;
            tcRT.offsetMax = Vector2.zero;

            inputField.textViewport = taRT;
            inputField.textComponent = textTMP;
            inputField.placeholder = phTMP;

            // Nút Vào Phòng (Join Room)
            GameObject joinRoomBtnObj = CreateButtonWithText(entryPanel.transform, "Btn_JoinRoom", "<color=#00FF88>VÀO PHÒNG</color>", new Vector2(0f, -95f), new Vector2(280f, 52f), btnWoodStitched, vietFont, 16f);
            Button joinRoomBtn = joinRoomBtnObj.GetComponent<Button>();

            // Nút Quay Lại
            GameObject backMenuBtnObj = CreateButtonWithText(entryPanel.transform, "Btn_BackToMenu", "<color=#CCCCCC>Quay Lại</color>", new Vector2(0f, -155f), new Vector2(160f, 38f), btnWoodStitched, vietFont, 13f);
            Button backMenuBtn = backMenuBtnObj.GetComponent<Button>();

            // ==========================================
            // 8. ROOM PANEL (Panel khi đã ở trong phòng chờ)
            // ==========================================
            GameObject roomPanel = new GameObject("Panel_Room", typeof(RectTransform));
            roomPanel.transform.SetParent(frameObj.transform, false);
            RectTransform rpRT = roomPanel.GetComponent<RectTransform>();
            rpRT.anchorMin = Vector2.zero;
            rpRT.anchorMax = Vector2.one;
            rpRT.offsetMin = new Vector2(24f, 24f);
            rpRT.offsetMax = new Vector2(-24f, -40f);
            roomPanel.SetActive(false); // Mặc định ẩn khi mới mở

            // Top Bar: Mã phòng + Nút Sao Chép + Số người
            GameObject topBar = new GameObject("TopBar_Info", typeof(RectTransform));
            topBar.transform.SetParent(roomPanel.transform, false);
            RectTransform tbRT = topBar.GetComponent<RectTransform>();
            tbRT.anchorMin = new Vector2(0f, 1f);
            tbRT.anchorMax = new Vector2(1f, 1f);
            tbRT.pivot = new Vector2(0.5f, 1f);
            tbRT.anchoredPosition = new Vector2(0f, 0f);
            tbRT.sizeDelta = new Vector2(0f, 44f);

            GameObject roomCodeTextObj = CreateTMPText(topBar.transform, "Text_RoomCode", "<color=#888888>Mã Phòng:</color> <color=#FFD700><b>PZ8899</b></color>", 16f, TextAlignmentOptions.Left, vietFont);
            RectTransform rcRT = roomCodeTextObj.GetComponent<RectTransform>();
            rcRT.anchorMin = new Vector2(0f, 0.5f);
            rcRT.anchorMax = new Vector2(0f, 0.5f);
            rcRT.pivot = new Vector2(0f, 0.5f);
            rcRT.anchoredPosition = new Vector2(10f, 0f);
            rcRT.sizeDelta = new Vector2(220f, 36f);
            TextMeshProUGUI roomCodeTMP = roomCodeTextObj.GetComponent<TextMeshProUGUI>();

            GameObject copyBtnObj = CreateButtonWithText(topBar.transform, "Btn_CopyCode", "<color=#CCCCCC>Sao Chép Mã</color>", new Vector2(280f, 0f), new Vector2(130f, 34f), btnWoodStitched, vietFont, 12f);
            RectTransform cpyRT = copyBtnObj.GetComponent<RectTransform>();
            cpyRT.anchorMin = new Vector2(0f, 0.5f);
            cpyRT.anchorMax = new Vector2(0f, 0.5f);
            cpyRT.pivot = new Vector2(0f, 0.5f);
            Button copyBtn = copyBtnObj.GetComponent<Button>();

            GameObject playerCountTextObj = CreateTMPText(topBar.transform, "Text_PlayerCount", "<color=#CCCCCC>Số Người:</color> <color=#00FF88>1/4</color>", 15f, TextAlignmentOptions.Right, vietFont);
            RectTransform pcRT = playerCountTextObj.GetComponent<RectTransform>();
            pcRT.anchorMin = new Vector2(1f, 0.5f);
            pcRT.anchorMax = new Vector2(1f, 0.5f);
            pcRT.pivot = new Vector2(1f, 0.5f);
            pcRT.anchoredPosition = new Vector2(-10f, 0f);
            pcRT.sizeDelta = new Vector2(160f, 36f);
            TextMeshProUGUI playerCountTMP = playerCountTextObj.GetComponent<TextMeshProUGUI>();

            // Slots Container (4 Slots)
            GameObject slotsContainer = new GameObject("Container_PlayerSlots", typeof(RectTransform), typeof(VerticalLayoutGroup));
            slotsContainer.transform.SetParent(roomPanel.transform, false);
            RectTransform scRT = slotsContainer.GetComponent<RectTransform>();
            scRT.anchorMin = new Vector2(0f, 0f);
            scRT.anchorMax = new Vector2(1f, 1f);
            scRT.offsetMin = new Vector2(10f, 70f);
            scRT.offsetMax = new Vector2(-10f, -50f);

            var vlg = slotsContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            LobbyPlayerSlotView[] slotViews = new LobbyPlayerSlotView[4];
            for (int i = 0; i < 4; i++)
            {
                slotViews[i] = CreatePlayerSlot(slotsContainer.transform, $"Slot_Player_{i + 1}", cardTotemBg, badgePill, vietFont);
            }

            // Bottom Action Bar: Ready + Start + Leave Buttons
            GameObject bottomBar = new GameObject("BottomBar_Actions", typeof(RectTransform));
            bottomBar.transform.SetParent(roomPanel.transform, false);
            RectTransform btmRT = bottomBar.GetComponent<RectTransform>();
            btmRT.anchorMin = new Vector2(0f, 0f);
            btmRT.anchorMax = new Vector2(1f, 0f);
            btmRT.pivot = new Vector2(0.5f, 0f);
            btmRT.anchoredPosition = new Vector2(0f, 5f);
            btmRT.sizeDelta = new Vector2(0f, 56f);

            // Nút Rời Phòng
            GameObject leaveBtnObj = CreateButtonWithText(bottomBar.transform, "Btn_LeaveRoom", "<color=#FF4444>Rời Phòng</color>", new Vector2(-220f, 0f), new Vector2(140f, 44f), btnWoodStitched, vietFont, 14f);
            Button leaveBtn = leaveBtnObj.GetComponent<Button>();

            // Nút Sẵn Sàng (Client)
            GameObject readyBtnObj = CreateButtonWithText(bottomBar.transform, "Btn_Ready", "<color=#00FF88>Sẵn Sàng</color>", new Vector2(0f, 0f), new Vector2(160f, 46f), btnWoodStitched, vietFont, 15f);
            Button readyBtn = readyBtnObj.GetComponent<Button>();
            TextMeshProUGUI readyBtnTMP = readyBtnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Nút Bắt Đầu Trận (Host)
            GameObject startMatchBtnObj = CreateButtonWithText(bottomBar.transform, "Btn_StartMatch", "<color=#FFD700>BẮT ĐẦU TRẬN</color>", new Vector2(180f, 0f), new Vector2(200f, 48f), btnWoodStitched, vietFont, 15f);
            Button startMatchBtn = startMatchBtnObj.GetComponent<Button>();

            // Status Message Text
            GameObject statusTextObj = CreateTMPText(frameObj.transform, "Text_StatusMessage", "", 13f, TextAlignmentOptions.Center, vietFont);
            RectTransform statRT = statusTextObj.GetComponent<RectTransform>();
            statRT.anchorMin = new Vector2(0f, 0f);
            statRT.anchorMax = new Vector2(1f, 0f);
            statRT.pivot = new Vector2(0.5f, 0f);
            statRT.anchoredPosition = new Vector2(0f, 8f);
            statRT.sizeDelta = new Vector2(0f, 24f);
            TextMeshProUGUI statusTMP = statusTextObj.GetComponent<TextMeshProUGUI>();

            // ==========================================
            // 9. WIRE REFERENCES VÀO LOBBY VIEW & PRESENTER
            // ==========================================
            LobbyView lobbyView = modalRoot.GetComponent<LobbyView>();
            LobbyPresenter lobbyPresenter = modalRoot.GetComponent<LobbyPresenter>();

            SerializedObject soView = new SerializedObject(lobbyView);
            soView.FindProperty("_modalContainer").objectReferenceValue = frRT;
            soView.FindProperty("_dimBackgroundButton").objectReferenceValue = ovBtn;
            soView.FindProperty("_screenCanvasGroup").objectReferenceValue = rootCG;

            soView.FindProperty("_entryPanel").objectReferenceValue = entryPanel;
            soView.FindProperty("_roomPanel").objectReferenceValue = roomPanel;

            soView.FindProperty("_roomCodeInputField").objectReferenceValue = inputField;
            soView.FindProperty("_createRoomButton").objectReferenceValue = createRoomBtn;
            soView.FindProperty("_joinRoomButton").objectReferenceValue = joinRoomBtn;
            soView.FindProperty("_backToMenuButton").objectReferenceValue = backMenuBtn;
            soView.FindProperty("_closeXButton").objectReferenceValue = closeBtn;

            soView.FindProperty("_roomCodeDisplayText").objectReferenceValue = roomCodeTMP;
            soView.FindProperty("_playerCountText").objectReferenceValue = playerCountTMP;
            soView.FindProperty("_copyCodeButton").objectReferenceValue = copyBtn;
            soView.FindProperty("_readyButton").objectReferenceValue = readyBtn;
            soView.FindProperty("_readyButtonText").objectReferenceValue = readyBtnTMP;
            soView.FindProperty("_startGameButton").objectReferenceValue = startMatchBtn;
            soView.FindProperty("_leaveRoomButton").objectReferenceValue = leaveBtn;

            soView.FindProperty("_playerSlotsContainer").objectReferenceValue = scRT;
            var slotsProp = soView.FindProperty("_playerSlots");
            slotsProp.ClearArray();
            for (int i = 0; i < 4; i++)
            {
                slotsProp.InsertArrayElementAtIndex(i);
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotViews[i];
            }

            soView.FindProperty("_statusMessageText").objectReferenceValue = statusTMP;
            soView.ApplyModifiedProperties();

            SerializedObject soPres = new SerializedObject(lobbyPresenter);
            soPres.FindProperty("_view").objectReferenceValue = lobbyView;
            soPres.ApplyModifiedProperties();

            // 10. Lưu Prefab Output
            EnsureDirectoryExists("Assets/_Prefabs/UI");
            EnsureDirectoryExists("Assets/Resources/UI");

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(modalRoot, PREFAB_OUTPUT_PATH);
            PrefabUtility.SaveAsPrefabAsset(modalRoot, RESOURCES_OUTPUT_PATH);

            Object.DestroyImmediate(modalRoot);

            // 11. Cập nhật UIRegistrySO tự động
            UIRegistrySetupTool.CreateOrUpdateUIRegistry();

            Debug.Log($"<color=#00FF88>[LobbyUIGenerator] THÀNH CÔNG!</color> Đã tạo xong Prefab '{PREFAB_OUTPUT_PATH}' và liên kết tự động vào UIRegistrySO mà KHÔNG ảnh hưởng bất kỳ UI nào khác!");
            Selection.activeObject = savedPrefab;
        }

        private static LobbyPlayerSlotView CreatePlayerSlot(Transform parent, string name, Sprite cardBg, Sprite badgePill, TMP_FontAsset font)
        {
            GameObject slotObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LobbyPlayerSlotView));
            slotObj.transform.SetParent(parent, false);

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 64f);

            Image img = slotObj.GetComponent<Image>();
            if (cardBg != null)
            {
                img.sprite = cardBg;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.16f, 0.11f, 0.08f, 0.95f);
            }

            // Tên người chơi
            GameObject nameTextObj = CreateTMPText(slotObj.transform, "Text_PlayerName", "Người Chơi", 15f, TextAlignmentOptions.Left, font);
            RectTransform ntRT = nameTextObj.GetComponent<RectTransform>();
            ntRT.anchorMin = new Vector2(0f, 0.5f);
            ntRT.anchorMax = new Vector2(0f, 0.5f);
            ntRT.pivot = new Vector2(0f, 0.5f);
            ntRT.anchoredPosition = new Vector2(16f, 10f);
            ntRT.sizeDelta = new Vector2(240f, 26f);
            var nameTMP = nameTextObj.GetComponent<TextMeshProUGUI>();
            nameTMP.fontStyle = FontStyles.Bold;

            // Tên Tướng
            GameObject heroTextObj = CreateTMPText(slotObj.transform, "Text_CharacterName", "<color=#CCCCCC>Tướng:</color> Đạo Sĩ", 13f, TextAlignmentOptions.Left, font);
            RectTransform htRT = heroTextObj.GetComponent<RectTransform>();
            htRT.anchorMin = new Vector2(0f, 0.5f);
            htRT.anchorMax = new Vector2(0f, 0.5f);
            htRT.pivot = new Vector2(0f, 0.5f);
            htRT.anchoredPosition = new Vector2(16f, -12f);
            htRT.sizeDelta = new Vector2(240f, 24f);
            var heroTMP = heroTextObj.GetComponent<TextMeshProUGUI>();

            // Huy hiệu Host Badge
            GameObject hostBadgeObj = new GameObject("Badge_Host", typeof(RectTransform), typeof(Image));
            hostBadgeObj.transform.SetParent(slotObj.transform, false);
            RectTransform hbRT = hostBadgeObj.GetComponent<RectTransform>();
            hbRT.anchorMin = new Vector2(0.5f, 0.5f);
            hbRT.anchorMax = new Vector2(0.5f, 0.5f);
            hbRT.pivot = new Vector2(0.5f, 0.5f);
            hbRT.anchoredPosition = new Vector2(30f, 0f);
            hbRT.sizeDelta = new Vector2(90f, 26f);
            Image hbImg = hostBadgeObj.GetComponent<Image>();
            if (badgePill != null)
            {
                hbImg.sprite = badgePill;
                hbImg.type = Image.Type.Sliced;
                hbImg.color = Color.white;
            }
            else
            {
                hbImg.color = new Color(0.3f, 0.2f, 0.05f, 0.9f);
            }

            GameObject hostBadgeText = CreateTMPText(hostBadgeObj.transform, "Text", "<color=#FFD700>CHỦ PHÒNG</color>", 11f, TextAlignmentOptions.Center, font);
            RectTransform hbtRT = hostBadgeText.GetComponent<RectTransform>();
            hbtRT.anchorMin = Vector2.zero;
            hbtRT.anchorMax = Vector2.one;
            hbtRT.offsetMin = Vector2.zero;
            hbtRT.offsetMax = Vector2.zero;

            // Huy hiệu Ready Badge
            GameObject readyBadgeObj = new GameObject("Badge_Ready", typeof(RectTransform), typeof(Image));
            readyBadgeObj.transform.SetParent(slotObj.transform, false);
            RectTransform rbRT = readyBadgeObj.GetComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(0.5f, 0.5f);
            rbRT.anchorMax = new Vector2(0.5f, 0.5f);
            rbRT.pivot = new Vector2(0.5f, 0.5f);
            rbRT.anchoredPosition = new Vector2(30f, 0f);
            rbRT.sizeDelta = new Vector2(90f, 26f);
            Image rbImg = readyBadgeObj.GetComponent<Image>();
            if (badgePill != null)
            {
                rbImg.sprite = badgePill;
                rbImg.type = Image.Type.Sliced;
                rbImg.color = Color.white;
            }
            else
            {
                rbImg.color = new Color(0.05f, 0.3f, 0.15f, 0.9f);
            }

            GameObject readyBadgeText = CreateTMPText(readyBadgeObj.transform, "Text", "<color=#00FF88>SẴN SÀNG</color>", 11f, TextAlignmentOptions.Center, font);
            RectTransform rbtRT = readyBadgeText.GetComponent<RectTransform>();
            rbtRT.anchorMin = Vector2.zero;
            rbtRT.anchorMax = Vector2.one;
            rbtRT.offsetMin = Vector2.zero;
            rbtRT.offsetMax = Vector2.zero;

            // Status Text
            GameObject statusTextObj = CreateTMPText(slotObj.transform, "Text_Status", "<color=#888888>[Chờ...]</color>", 13f, TextAlignmentOptions.Right, font);
            RectTransform stRT = statusTextObj.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(1f, 0.5f);
            stRT.anchorMax = new Vector2(1f, 0.5f);
            stRT.pivot = new Vector2(1f, 0.5f);
            stRT.anchoredPosition = new Vector2(-80f, 0f);
            stRT.sizeDelta = new Vector2(140f, 26f);
            var statusTMP = statusTextObj.GetComponent<TextMeshProUGUI>();

            // Ping Text
            GameObject pingTextObj = CreateTMPText(slotObj.transform, "Text_Ping", "<color=#00FF88>20ms</color>", 12f, TextAlignmentOptions.Right, font);
            RectTransform ptRT = pingTextObj.GetComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(1f, 0.5f);
            ptRT.anchorMax = new Vector2(1f, 0.5f);
            ptRT.pivot = new Vector2(1f, 0.5f);
            ptRT.anchoredPosition = new Vector2(-16f, 0f);
            ptRT.sizeDelta = new Vector2(60f, 26f);
            var pingTMP = pingTextObj.GetComponent<TextMeshProUGUI>();

            // Wire vào LobbyPlayerSlotView
            LobbyPlayerSlotView slotView = slotObj.GetComponent<LobbyPlayerSlotView>();
            SerializedObject soSlot = new SerializedObject(slotView);
            soSlot.FindProperty("_playerNameText").objectReferenceValue = nameTMP;
            soSlot.FindProperty("_characterNameText").objectReferenceValue = heroTMP;
            soSlot.FindProperty("_statusText").objectReferenceValue = statusTMP;
            soSlot.FindProperty("_pingText").objectReferenceValue = pingTMP;
            soSlot.FindProperty("_hostBadge").objectReferenceValue = hostBadgeObj;
            soSlot.FindProperty("_readyBadge").objectReferenceValue = readyBadgeObj;
            soSlot.ApplyModifiedProperties();

            return slotView;
        }

        private static GameObject CreateButtonWithText(Transform parent, string name, string text, Vector2 anchoredPos, Vector2 size, Sprite btnSprite, TMP_FontAsset font, float fontSize)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.2f, 0.14f, 0.1f, 1f);
            }

            GameObject txtObj = CreateTMPText(btnObj.transform, "Text", text, fontSize, TextAlignmentOptions.Center, font);
            RectTransform txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(10f, 4f);
            txtRT.offsetMax = new Vector2(-10f, -4f);

            return btnObj;
        }

        private static GameObject CreateTMPText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, TMP_FontAsset font)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.raycastTarget = false; // Tối ưu CPU EventSystem (AGENTS.md)
            tmp.richText = true;

            return go;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
#endif

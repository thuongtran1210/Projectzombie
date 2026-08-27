#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tạo Prefab giao diện Chọn Nhân Vật MVP (CharacterSelectionUI).
    /// Tuân thủ chuẩn thẩm mỹ Cổ Phong Đông Sơn - Anime URP và Mô hình MVP (Mục 12 Guidelines).
    /// </summary>
    public static class CharacterSelectionUIGenerator
    {
        [MenuItem("Tools/ProjectZombie/UI/Generate Character Selection UI Prefab", priority = 10)]
        public static void GenerateCharacterSelectionPrefab()
        {
            string prefabFolder = "Assets/_Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Prefabs", "UI");
            }

            // 1. Root Modal Panel
            GameObject root = new GameObject("Panel_CharacterSelect", typeof(RectTransform), typeof(CanvasGroup), typeof(CharacterSelectionView), typeof(CharacterSelectionPresenter));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            SetStretchAnchor(rootRT);

            // Presenter & View
            var view = root.GetComponent<CharacterSelectionView>();
            var presenter = root.GetComponent<CharacterSelectionPresenter>();
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            // 2. Dim Overlay Background (Bắt sự kiện click ra ngoài để đóng)
            GameObject bgDim = CreateUIElement("Dim_CharacterSelect", root.transform);
            SetStretchAnchor(bgDim.GetComponent<RectTransform>());
            var bgDimImg = bgDim.AddComponent<Image>();
            bgDimImg.color = new Color(0.04f, 0.03f, 0.06f, 0.88f); // Đen khói huyền ảo
            var bgDimBtn = bgDim.AddComponent<Button>();

            // 3. Main Center Modal Panel (Khung Gỗ Mun Cổ 9-Slice)
            GameObject panel = CreateUIElement("Modal_CharacterSelect", root.transform);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(1120, 640);
            panelRT.anchoredPosition = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = Color.white;
            panelImg.type = Image.Type.Sliced;

            Sprite modalFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Modal_TangBaoCac_9Slice.png");
            if (modalFrame != null) panelImg.sprite = modalFrame;

            // Load Font tiếng Việt
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // Load Sprites Vọng Xuyên Cổ Phong
            Sprite ribbonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Banner_Ribbon_Hero_Amber.png");
            Sprite parchmentCard = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");
            Sprite totemFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Avatar_Totem_Wood.png");
            Sprite btnNavArrow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Arrow_Hex_Wood.png");
            Sprite btnBattleAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");
            Sprite skillBoxWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Box_Skill_Icon_Wood_9Slice.png");
            Sprite heroNameBadge = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Hero_Name_Wood.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Close_X_Wood.png");

            // 4. Header Title: Băng Rôn Đỏ Cam Viền Vàng "CHỌN ANH HÙNG XUẤT TRẬN"
            GameObject ribbonObj = CreateUIElement("Ribbon_Title", panel.transform);
            RectTransform ribbonRT = ribbonObj.GetComponent<RectTransform>();
            ribbonRT.anchorMin = new Vector2(0.5f, 1.0f);
            ribbonRT.anchorMax = new Vector2(0.5f, 1.0f);
            ribbonRT.pivot = new Vector2(0.5f, 1.0f);
            ribbonRT.anchoredPosition = new Vector2(0, 16);
            ribbonRT.sizeDelta = new Vector2(500, 78);
            var ribbonImg = ribbonObj.AddComponent<Image>();
            ribbonImg.color = Color.white;
            ribbonImg.type = Image.Type.Sliced;
            if (ribbonSprite != null) ribbonImg.sprite = ribbonSprite;

            GameObject titleObj = CreateUIElement("Text_Title", ribbonObj.transform);
            SetStretchAnchor(titleObj.GetComponent<RectTransform>());
            titleObj.GetComponent<RectTransform>().offsetMin = new Vector2(50, 8);
            titleObj.GetComponent<RectTransform>().offsetMax = new Vector2(-50, -14);
            var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) titleTMP.font = vietFont;
            titleTMP.text = "CHỌN ANH HÙNG XUẤT TRẬN";
            titleTMP.fontSize = 22;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;

            // 4.5. Nút Thoát / Đóng Modal Góc Trên Bên Phải (Btn_Close / Btn_Back)
            GameObject closeBtnObj = CreateUIElement("Btn_Close", panel.transform);
            RectTransform closeRT = closeBtnObj.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1.0f, 1.0f);
            closeRT.anchorMax = new Vector2(1.0f, 1.0f);
            closeRT.pivot = new Vector2(1.0f, 1.0f);
            closeRT.anchoredPosition = new Vector2(-12, -12);
            closeRT.sizeDelta = new Vector2(48, 48);
            var closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = Color.white;
            if (btnCloseX != null) closeImg.sprite = btnCloseX;
            closeImg.preserveAspect = true;
            var closeBtn = closeBtnObj.AddComponent<Button>();
            var closeColors = closeBtn.colors;
            closeColors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            closeColors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            closeBtn.colors = closeColors;

            // 5. Left Column: Khung Totem Gỗ Mun Chạm Sừng Thú & Character Preview
            GameObject leftCol = CreateUIElement("LeftColumn_Avatar", panel.transform);
            RectTransform leftRT = leftCol.GetComponent<RectTransform>();
            leftRT.anchorMin = new Vector2(0f, 0.5f);
            leftRT.anchorMax = new Vector2(0f, 0.5f);
            leftRT.pivot = new Vector2(0.5f, 0.5f);
            leftRT.anchoredPosition = new Vector2(255, -28);
            leftRT.sizeDelta = new Vector2(380, 460);

            // Totem Wood Frame bao quanh Avatar
            GameObject orbFrame = CreateUIElement("Avatar_Totem_Frame", leftCol.transform);
            RectTransform ofRT = orbFrame.GetComponent<RectTransform>();
            ofRT.anchorMin = new Vector2(0.5f, 0.5f);
            ofRT.anchorMax = new Vector2(0.5f, 0.5f);
            ofRT.pivot = new Vector2(0.5f, 0.5f);
            ofRT.anchoredPosition = new Vector2(0, 36);
            ofRT.sizeDelta = new Vector2(300, 300);
            var ofImg = orbFrame.AddComponent<Image>();
            ofImg.color = Color.white;
            if (totemFrame != null) ofImg.sprite = totemFrame;

            // 1. RawImage nhận RenderTexture Animation thời gian thực
            GameObject rawPreviewObj = CreateUIElement("CharacterPreview_RT", orbFrame.transform);
            RectTransform rawPreviewRT = rawPreviewObj.GetComponent<RectTransform>();
            rawPreviewRT.anchorMin = new Vector2(0.5f, 0.5f);
            rawPreviewRT.anchorMax = new Vector2(0.5f, 0.5f);
            rawPreviewRT.pivot = new Vector2(0.5f, 0.45f);
            rawPreviewRT.sizeDelta = new Vector2(220, 220);
            rawPreviewRT.anchoredPosition = Vector2.zero;
            var rawPreviewImg = rawPreviewObj.AddComponent<RawImage>();
            rawPreviewImg.color = Color.white;
            rawPreviewImg.enabled = false;
            rawPreviewImg.raycastTarget = false;

            // 2. Avatar Sprite Display (Fallback)
            GameObject avatarImgObj = CreateUIElement("CharacterAvatarImage", orbFrame.transform);
            RectTransform avatarRT = avatarImgObj.GetComponent<RectTransform>();
            avatarRT.anchorMin = new Vector2(0.5f, 0.5f);
            avatarRT.anchorMax = new Vector2(0.5f, 0.5f);
            avatarRT.pivot = new Vector2(0.5f, 0.45f);
            avatarRT.sizeDelta = new Vector2(175, 175);
            avatarRT.anchoredPosition = Vector2.zero;
            var avatarImg = avatarImgObj.AddComponent<Image>();
            avatarImg.preserveAspect = true;

            // Cặp Nút Chuyển Tướng Gỗ (< và >)
            GameObject prevBtnObj = CreateButton("Btn_Prev", leftCol.transform, new Vector2(-75, -155), new Vector2(88, 48), "<", vietFont);
            var pImg = prevBtnObj.GetComponent<Image>();
            pImg.color = Color.white;
            pImg.type = Image.Type.Sliced;
            if (btnNavArrow != null) pImg.sprite = btnNavArrow;
            var pTxt = prevBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (pTxt != null) { pTxt.fontSize = 24; pTxt.color = new Color(0.95f, 0.88f, 0.70f, 1f); }

            GameObject nextBtnObj = CreateButton("Btn_Next", leftCol.transform, new Vector2(75, -155), new Vector2(88, 48), ">", vietFont);
            var nImg = nextBtnObj.GetComponent<Image>();
            nImg.color = Color.white;
            nImg.type = Image.Type.Sliced;
            if (btnNavArrow != null) nImg.sprite = btnNavArrow;
            var nTxt = nextBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nTxt != null) { nTxt.fontSize = 24; nTxt.color = new Color(0.95f, 0.88f, 0.70f, 1f); }

            // 6. Right Column: Tờ Giấy Da Cổ Chứa Thông Tin & Kỹ Năng
            GameObject rightCol = CreateUIElement("RightColumn_Info", panel.transform);
            RectTransform rightRT = rightCol.GetComponent<RectTransform>();
            rightRT.anchorMin = new Vector2(1f, 0.5f);
            rightRT.anchorMax = new Vector2(1f, 0.5f);
            rightRT.pivot = new Vector2(0.5f, 0.5f);
            rightRT.anchoredPosition = new Vector2(-320, -28);
            rightRT.sizeDelta = new Vector2(530, 460);

            var rightBgImg = rightCol.AddComponent<Image>();
            rightBgImg.color = Color.white;
            rightBgImg.type = Image.Type.Sliced;
            if (parchmentCard != null) rightBgImg.sprite = parchmentCard;

            // Container Nội Dung Bên Trong Right Column
            GameObject rightInner = CreateUIElement("Inner_Content", rightCol.transform);
            RectTransform riRT = rightInner.GetComponent<RectTransform>();
            riRT.anchorMin = Vector2.zero;
            riRT.anchorMax = Vector2.one;
            riRT.offsetMin = new Vector2(18, 16);
            riRT.offsetMax = new Vector2(-18, -16);

            // Biển Tên Tướng (Badge Gỗ Cổ)
            GameObject nameBadgeObj = CreateUIElement("Badge_HeroName", rightInner.transform);
            RectTransform nbRT = nameBadgeObj.GetComponent<RectTransform>();
            nbRT.anchorMin = new Vector2(0.5f, 1);
            nbRT.anchorMax = new Vector2(0.5f, 1);
            nbRT.pivot = new Vector2(0.5f, 1);
            nbRT.anchoredPosition = new Vector2(0, -6);
            nbRT.sizeDelta = new Vector2(220, 44);
            var nbImg = nameBadgeObj.AddComponent<Image>();
            nbImg.color = Color.white;
            nbImg.type = Image.Type.Sliced;
            if (heroNameBadge != null) nbImg.sprite = heroNameBadge;

            GameObject nameObj = CreateUIElement("Text_CharacterName", nameBadgeObj.transform);
            SetStretchAnchor(nameObj.GetComponent<RectTransform>());
            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) nameTMP.font = vietFont;
            nameTMP.text = "Thư Sinh";
            nameTMP.fontSize = 20;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = new Color(0.96f, 0.88f, 0.72f);

            GameObject elemObj = new GameObject("Text_Element", typeof(RectTransform));
            elemObj.transform.SetParent(rightInner.transform, false);
            var elemTMP = elemObj.AddComponent<TextMeshProUGUI>();
            elemObj.SetActive(false);

            // Description
            GameObject descObj = CreateUIElement("Text_Description", rightInner.transform);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 1);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0.5f, 1);
            descRT.anchoredPosition = new Vector2(0, -56);
            descRT.sizeDelta = new Vector2(0, 48);
            var descTMP = descObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) descTMP.font = vietFont;
            descTMP.text = "Vị học sĩ cầm bút như kiếm, lấy trời đất làm nghiên. Từ Vọng Xuyên xa xôi, mượn mực thần kể chuyện anh hùng.";
            descTMP.fontSize = 12;
            descTMP.alignment = TextAlignmentOptions.Center;
            descTMP.color = new Color(0.25f, 0.18f, 0.12f);
            descTMP.enableWordWrapping = true;

            // Signature Skill Block (Kỹ Năng Chủ Động)
            GameObject skillCard = CreateUIElement("Card_SignatureSkill", rightInner.transform);
            RectTransform skillRT = skillCard.GetComponent<RectTransform>();
            skillRT.anchorMin = new Vector2(0, 1);
            skillRT.anchorMax = new Vector2(1, 1);
            skillRT.pivot = new Vector2(0.5f, 1);
            skillRT.anchoredPosition = new Vector2(0, -112);
            skillRT.sizeDelta = new Vector2(0, 84);

            // Khung Icon Kỹ Năng Bên Trái
            GameObject skBox = CreateUIElement("Box_Icon", skillCard.transform);
            RectTransform skbRT = skBox.GetComponent<RectTransform>();
            skbRT.anchorMin = new Vector2(0, 0.5f);
            skbRT.anchorMax = new Vector2(0, 0.5f);
            skbRT.pivot = new Vector2(0, 0.5f);
            skbRT.anchoredPosition = new Vector2(6, 0);
            skbRT.sizeDelta = new Vector2(72, 72);
            var skbImg = skBox.AddComponent<Image>();
            skbImg.color = Color.white;
            skbImg.type = Image.Type.Sliced;
            if (skillBoxWood != null) skbImg.sprite = skillBoxWood;

            GameObject skillHeader = CreateUIElement("Text_SkillHeader", skillCard.transform);
            RectTransform shRT = skillHeader.GetComponent<RectTransform>();
            shRT.anchorMin = new Vector2(0, 1);
            shRT.anchorMax = new Vector2(1, 1);
            shRT.pivot = new Vector2(0, 1);
            shRT.anchoredPosition = new Vector2(88, -4);
            shRT.sizeDelta = new Vector2(-88, 20);
            var shTMP = skillHeader.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) shTMP.font = vietFont;
            shTMP.text = "KỸ NĂNG CHỦ ĐỘNG (SIGNATURE SKILL)";
            shTMP.fontSize = 12;
            shTMP.fontStyle = FontStyles.Bold;
            shTMP.color = new Color(0.20f, 0.14f, 0.10f);

            GameObject skillTextObj = CreateUIElement("Text_SignatureSkill", skillCard.transform);
            RectTransform stRT = skillTextObj.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0, 0);
            stRT.anchorMax = new Vector2(1, 1);
            stRT.offsetMin = new Vector2(88, 4);
            stRT.offsetMax = new Vector2(-6, -24);
            var stTMP = skillTextObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) stTMP.font = vietFont;
            stTMP.text = "Phán Quyết Tiên Định: Chọn 1 kẻ địch. Họa nên Thiên Thư, gây [Lượng] sát thương Phép và làm chậm 30% trong 5 giây.";
            stTMP.fontSize = 11;
            stTMP.color = new Color(0.30f, 0.22f, 0.16f);
            stTMP.enableWordWrapping = true;

            // Passive Trait Block (Nội Tại Độc Quyền)
            GameObject passiveCard = CreateUIElement("Card_PassiveTrait", rightInner.transform);
            RectTransform passiveRT = passiveCard.GetComponent<RectTransform>();
            passiveRT.anchorMin = new Vector2(0, 1);
            passiveRT.anchorMax = new Vector2(1, 1);
            passiveRT.pivot = new Vector2(0.5f, 1);
            passiveRT.anchoredPosition = new Vector2(0, -204);
            passiveRT.sizeDelta = new Vector2(0, 84);

            // Khung Icon Nội Tại Bên Trái
            GameObject psBox = CreateUIElement("Box_Icon", passiveCard.transform);
            RectTransform psbRT = psBox.GetComponent<RectTransform>();
            psbRT.anchorMin = new Vector2(0, 0.5f);
            psbRT.anchorMax = new Vector2(0, 0.5f);
            psbRT.pivot = new Vector2(0, 0.5f);
            psbRT.anchoredPosition = new Vector2(6, 0);
            psbRT.sizeDelta = new Vector2(72, 72);
            var psbImg = psBox.AddComponent<Image>();
            psbImg.color = Color.white;
            psbImg.type = Image.Type.Sliced;
            if (skillBoxWood != null) psbImg.sprite = skillBoxWood;

            GameObject passiveHeader = CreateUIElement("Text_PassiveHeader", passiveCard.transform);
            RectTransform phRT = passiveHeader.GetComponent<RectTransform>();
            phRT.anchorMin = new Vector2(0, 1);
            phRT.anchorMax = new Vector2(1, 1);
            phRT.pivot = new Vector2(0, 1);
            phRT.anchoredPosition = new Vector2(88, -4);
            phRT.sizeDelta = new Vector2(-88, 20);
            var phTMP = passiveHeader.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) phTMP.font = vietFont;
            phTMP.text = "NỘI TẠI ĐỘC QUYỀN (PASSIVE TRAIT)";
            phTMP.fontSize = 12;
            phTMP.fontStyle = FontStyles.Bold;
            phTMP.color = new Color(0.20f, 0.14f, 0.10f);

            GameObject passiveTextObj = CreateUIElement("Text_PassiveTrait", passiveCard.transform);
            RectTransform ptRT = passiveTextObj.GetComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0, 0);
            ptRT.anchorMax = new Vector2(1, 1);
            ptRT.offsetMin = new Vector2(88, 4);
            ptRT.offsetMax = new Vector2(-6, -24);
            var ptTMP = passiveTextObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) ptTMP.font = vietFont;
            ptTMP.text = "Sơn Hà Bồi Hộ: Cứ mỗi 10 giây, tạo ra 1 Bamboo Barrier nhỏ chặn đòn tấn công cơ bản tiếp theo. Tăng 10% Tốc độ di chuyển.";
            ptTMP.fontSize = 11;
            ptTMP.color = new Color(0.30f, 0.22f, 0.16f);
            ptTMP.enableWordWrapping = true;

            // 7. Select Button: "XÁC NHẬN CHỌN TƯỚNG" (Nút Hổ Phách 3D)
            GameObject selectBtnObj = CreateButton("Btn_Select", rightInner.transform, new Vector2(0, 8), new Vector2(460, 72), "XÁC NHẬN CHỌN TƯỚNG", vietFont);
            RectTransform sbrt = selectBtnObj.GetComponent<RectTransform>();
            sbrt.anchorMin = new Vector2(0.5f, 0f);
            sbrt.anchorMax = new Vector2(0.5f, 0f);
            sbrt.pivot = new Vector2(0.5f, 0f);
            sbrt.anchoredPosition = new Vector2(0, 6);
            sbrt.sizeDelta = new Vector2(460, 72);

            var btnImg = selectBtnObj.GetComponent<Image>();
            btnImg.color = Color.white;
            btnImg.type = Image.Type.Sliced;
            if (btnBattleAmber != null) btnImg.sprite = btnBattleAmber;

            var btnTxt = selectBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            btnTxt.fontSize = 20;
            btnTxt.fontStyle = FontStyles.Bold;
            btnTxt.color = Color.white;

            // 8. Wire References to View Component
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_characterNameText").objectReferenceValue = nameTMP;
            soView.FindProperty("_elementText").objectReferenceValue = elemTMP;
            soView.FindProperty("_descriptionText").objectReferenceValue = descTMP;
            soView.FindProperty("_signatureSkillText").objectReferenceValue = stTMP;
            soView.FindProperty("_passiveTraitText").objectReferenceValue = ptTMP;
            soView.FindProperty("_characterAvatarImage").objectReferenceValue = avatarImg;
            soView.FindProperty("_characterPreviewRawImage").objectReferenceValue = rawPreviewImg;

            soView.FindProperty("_selectButton").objectReferenceValue = selectBtnObj.GetComponent<Button>();
            soView.FindProperty("_prevButton").objectReferenceValue = prevBtnObj.GetComponent<Button>();
            soView.FindProperty("_nextButton").objectReferenceValue = nextBtnObj.GetComponent<Button>();
            soView.FindProperty("_backButton").objectReferenceValue = closeBtn;
            soView.FindProperty("_modalContainer").objectReferenceValue = panelRT;
            soView.FindProperty("_dimBackgroundButton").objectReferenceValue = bgDimBtn;
            soView.ApplyModifiedProperties();

            // 8.5. Wire SelectionData and Hero Prefabs to Presenter
            string dataPath = "Assets/_Data/CharacterSelectionData.asset";
            var selData = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterSelectionData>(dataPath);
            if (selData == null)
            {
                selData = ScriptableObject.CreateInstance<ProjectZombie.Features.Player.CharacterSelectionData>();
                AssetDatabase.CreateAsset(selData, dataPath);
            }

            var pThuSinh = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/Thu Sinh.prefab");
            var pDaoSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/Dao Si.prefab");
            var pThanhDong = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/Thanh Dong.prefab");
            var pAnSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/An Si.prefab");

            // Load VFX Prefabs cho 4 đòn đánh bản thể
            var vfxThuSinh = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_ThuSinh_InkSlash.prefab");
            var vfxDaoSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_DaoSi_SwordSlash.prefab");
            var vfxThanhDong = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/Projectile_ThanhDong_AirWave.prefab");
            var vfxAnSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_AnSi_EarthImpactSlash.prefab");

            // Khởi tạo Database nhân vật mẫu
            var charList = new System.Collections.Generic.List<ProjectZombie.Features.Player.CharacterEntry>
            {
                new ProjectZombie.Features.Player.CharacterEntry
                {
                    characterId = "C001_ThuSinh",
                    characterName = "Thư Sinh",
                    element = ElementType.Kim,
                    elementHexColor = "#FFD700",
                    description = "Được anh linh liệt tổ & Đức Thánh Trần điểm hóa. Tay cầm bút lệnh khí thiêng sông núi phán định tà ma.",
                    signatureSkillName = "Phán Quyết Tiền Định",
                    signatureSkillDesc = "Chèn 1 hit ảo Ngũ Hành vào Queue Tương Sinh, kích hoạt giảm 20% Cooldown cho vũ khí khớp lệnh.",
                    passiveTraitName = "Văn Khí Hộ Thể",
                    passiveTraitDesc = "Khi kích hoạt Tương Sinh Ngũ Hành, tăng 15% Tốc độ di chuyển và hồi 5% HP tối đa.",
                    playerPrefab = pThuSinh,
                    basicAttackConfig = new ProjectZombie.Features.Player.CharacterAttackConfig
                    {
                        attackType = ProjectZombie.Features.Player.CharacterAttackType.MeleeSlash,
                        slashVfxPrefab = vfxThuSinh,
                        attackName = "Vung Bút Phán Quan",
                        meleeAreaSize = new Vector2(3.5f, 2.6f),
                        meleeOffset = 1.3f,
                        baseAttackSpeed = 1.8f
                    },
                    isUnlocked = true
                },
                new ProjectZombie.Features.Player.CharacterEntry
                {
                    characterId = "C002_DaoSi",
                    characterName = "Đạo Sĩ",
                    element = ElementType.Moc,
                    elementHexColor = "#9B51E0",
                    description = "Đạo nhân tinh thông Tiên Đạo Bát Quái. Vận hành Cán Cân Âm Dương (Âm Thịnh / Dương Thịnh / Thái Cực).",
                    signatureSkillName = "Bát Quái Trận Đồ",
                    signatureSkillDesc = "Dậm chân tạo vùng Bát Quái làm chậm và gây sát thương yêu ma, ép Cán Cân Âm Dương về 50 (Thái Cực) trong 4s.",
                    passiveTraitName = "Cán Cân Âm Dương",
                    passiveTraitDesc = "Trạng thái Thái Cực (Cân bằng) tăng 25% Sát thương toàn thể và giảm 20% Sát thương nhận vào.",
                    playerPrefab = pDaoSi,
                    basicAttackConfig = new ProjectZombie.Features.Player.CharacterAttackConfig
                    {
                        attackType = ProjectZombie.Features.Player.CharacterAttackType.MeleeSlash,
                        slashVfxPrefab = vfxDaoSi,
                        attackName = "Trảm Yêu Trừ Ma Kiếm",
                        meleeAreaSize = new Vector2(3.6f, 2.5f),
                        meleeOffset = 1.35f,
                        baseAttackSpeed = 2.0f
                    },
                    isUnlocked = true
                },
                new ProjectZombie.Features.Player.CharacterEntry
                {
                    characterId = "C003_ThanhDong",
                    characterName = "Thanh Đồng",
                    element = ElementType.Moc,
                    elementHexColor = "#4C7A3D",
                    description = "Cô Đồng / Thầy Pháp Đạo Mẫu Tứ Phủ (Thiên, Nhạc, Thoải, Địa). Tay mang Chuỗi Linh Phù Tứ Phủ hộ thân trừ tà.",
                    signatureSkillName = "Giá Đồng Tứ Phủ",
                    signatureSkillDesc = "Thỉnh nhập Thánh thần Tứ Phủ ban hào quang 4 cõi (Tăng công / Tăng tốc / Giảm hồi chiêu / Giáp hộ thân) trong 5s.",
                    passiveTraitName = "Linh Lực Tứ Phủ",
                    passiveTraitDesc = "Thu thập Linh Khí tích lũy thanh Linh Lực Tứ Phủ. Khi kích hoạt Giá Đồng, nhận đồng thời hiệu ứng hộ trì của cả 4 cõi thần linh.",
                    playerPrefab = pThanhDong,
                    basicAttackConfig = new ProjectZombie.Features.Player.CharacterAttackConfig
                    {
                        attackType = ProjectZombie.Features.Player.CharacterAttackType.RangedProjectile,
                        projectilePrefab = vfxThanhDong,
                        attackName = "Khí Ba Đạo Mẫu",
                        baseAttackSpeed = 2.2f,
                        projectileSpeed = 9.0f
                    },
                    isUnlocked = true
                },
                new ProjectZombie.Features.Player.CharacterEntry
                {
                    characterId = "C004_AnSi",
                    characterName = "Ẩn Sĩ Sơn Lâm",
                    element = ElementType.Tho,
                    elementHexColor = "#8A6A3E",
                    description = "Kỳ nhân tự tu nội lực chốn thâm sơn, hòa hợp làm một với núi rừng bản địa. Dồn lực bộc phát địa khí.",
                    signatureSkillName = "Thập Phương Chấn Thế",
                    signatureSkillDesc = "Trừ 30% HP hiện tại bộc phát địa khí chấn nứt đất đá, gây sát thương + Choáng 1.2s và đẩy lùi 8m/s.",
                    passiveTraitName = "Bàn Thạch Chi Khu",
                    passiveTraitDesc = "Máu càng thấp thủ càng cao. Khi HP dưới 50%, nhận thêm 30% Kháng sát thương và miễn nhiễm Đẩy lùi.",
                    playerPrefab = pAnSi,
                    basicAttackConfig = new ProjectZombie.Features.Player.CharacterAttackConfig
                    {
                        attackType = ProjectZombie.Features.Player.CharacterAttackType.MeleeSlash,
                        slashVfxPrefab = vfxAnSi,
                        attackName = "Thạch Quyền Phá Địa",
                        meleeAreaSize = new Vector2(3.6f, 2.7f),
                        meleeOffset = 1.35f,
                        baseAttackSpeed = 1.6f
                    },
                    isUnlocked = true
                }
            };

            selData.SetCharacters(charList);
            selData.SelectCharacter(0);
            EditorUtility.SetDirty(selData);

            soPresenter.Update();
            soPresenter.FindProperty("_selectionData").objectReferenceValue = selData;
            var prefabsProp = soPresenter.FindProperty("_characterPrefabs");
            if (prefabsProp != null)
            {
                prefabsProp.arraySize = 4;
                prefabsProp.GetArrayElementAtIndex(0).objectReferenceValue = pThuSinh;
                prefabsProp.GetArrayElementAtIndex(1).objectReferenceValue = pDaoSi;
                prefabsProp.GetArrayElementAtIndex(2).objectReferenceValue = pThanhDong;
                prefabsProp.GetArrayElementAtIndex(3).objectReferenceValue = pAnSi;
            }
            soPresenter.ApplyModifiedProperties();

            // 9. Save as Prefab
            string prefabPath = $"{prefabFolder}/CharacterSelectionUI.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Kiểm tra xem trong Scene có Canvas không để đặt làm con của Canvas_MetaMenu
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                // Xóa instance cũ nếu có
                var oldUI = GameObject.Find("CharacterSelectionUI");
                if (oldUI != null && oldUI != root) Object.DestroyImmediate(oldUI);

                var oldPanel = GameObject.Find("Panel_CharacterSelect");
                if (oldPanel != null && oldPanel != root) Object.DestroyImmediate(oldPanel);

                var metaCanvas = GameObject.Find("Canvas_MetaMenu");
                Transform targetParent = metaCanvas != null ? metaCanvas.transform : canvas.transform;

                root.transform.SetParent(targetParent, false);
                SetStretchAnchor(rootRT);

                var metaMgr = Object.FindAnyObjectByType<MetaUIManager>();
                if (metaMgr != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaMgr);
                    soMeta.FindProperty("_characterSelectScreen").objectReferenceValue = view;
                    soMeta.ApplyModifiedProperties();
                    EditorUtility.SetDirty(metaMgr);
                }

                Debug.Log("<color=#00FF88>[CharacterSelectionUIGenerator]</color> Đã đưa Panel_CharacterSelect vào trong Canvas_MetaMenu thành công!");
            }
            else
            {
                Object.DestroyImmediate(root);
            }

            // Tự động gán Prefab vào GameplayBootstrapper trong Scene nếu có
            var bootstrapper = Object.FindObjectOfType<ProjectZombie.Features.Player.GameplayBootstrapper>();
            if (bootstrapper != null && savedPrefab != null)
            {
                SerializedObject soBoot = new SerializedObject(bootstrapper);
                var prop = soBoot.FindProperty("characterSelectionUIPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = savedPrefab;
                    soBoot.ApplyModifiedProperties();
                    EditorUtility.SetDirty(bootstrapper);
                    Debug.Log("[CharacterSelectionUIGenerator] Đã tự động liên kết UI Prefab vào GameplayBootstrapper trong Scene!");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CharacterSelectionUIGenerator] Đã sinh thành công UI Prefab: {prefabPath}");
            EditorUtility.DisplayDialog("Character Selection UI", $"Đã tạo thành công UI Chọn Nhân Vật và đặt nổi lên trên cùng của Canvas Scene!\n\nBạn có thể bấm Play để trải nghiệm ngay.", "Tuyệt vời!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, string text, TMP_FontAsset font = null)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.18f, 0.25f, 0.95f);

            var btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = cb;

            GameObject txtObj = CreateUIElement("Text", btnObj.transform);
            SetStretchAnchor(txtObj.GetComponent<RectTransform>());
            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.9f, 0.7f);

            return btnObj;
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

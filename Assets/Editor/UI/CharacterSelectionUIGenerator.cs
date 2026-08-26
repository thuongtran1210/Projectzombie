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

            // 3. Main Center Modal Panel (Khung Đồng Cổ)
            GameObject panel = CreateUIElement("Modal_CharacterSelect", root.transform);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(1100, 720);
            panelRT.anchoredPosition = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.15f, 0.95f); // Gỗ mun thau cổ

            // Thử load Sprite Frame nếu có
            Sprite cardFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Frame_Card_Evolution.png");
            if (cardFrame != null)
            {
                panelImg.sprite = cardFrame;
                panelImg.type = Image.Type.Sliced;
            }

            // 4. Header Title: "CHỌN ANH HÙNG XUẤT TRẬN"
            GameObject titleObj = CreateUIElement("Text_Title", panel.transform);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1.0f);
            titleRT.anchorMax = new Vector2(0.5f, 1.0f);
            titleRT.pivot = new Vector2(0.5f, 1.0f);
            titleRT.anchoredPosition = new Vector2(0, -35);
            titleRT.sizeDelta = new Vector2(800, 60);

            // Tải font tiếng Việt đã bake sẵn
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) titleTMP.font = vietFont;
            titleTMP.text = "<color=#FFD700>CHON ANH HUNG XUAT TRAN</color>";
            titleTMP.fontSize = 38;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;

            // 5. Left Column: Avatar & Character Preview
            GameObject leftCol = CreateUIElement("LeftColumn_Avatar", panel.transform);
            RectTransform leftRT = leftCol.GetComponent<RectTransform>();
            leftRT.anchorMin = new Vector2(0f, 0.5f);
            leftRT.anchorMax = new Vector2(0f, 0.5f);
            leftRT.pivot = new Vector2(0.5f, 0.5f);
            leftRT.anchoredPosition = new Vector2(250, -20);
            leftRT.sizeDelta = new Vector2(360, 480);

            // Avatar Frame Background
            var avatarBgImg = leftCol.AddComponent<Image>();
            avatarBgImg.color = new Color(0.08f, 0.07f, 0.10f, 0.9f);

            // Avatar Sprite Display
            GameObject avatarImgObj = CreateUIElement("CharacterAvatarImage", leftCol.transform);
            RectTransform avatarRT = avatarImgObj.GetComponent<RectTransform>();
            avatarRT.sizeDelta = new Vector2(240, 240);
            avatarRT.anchoredPosition = new Vector2(0, 30);
            var avatarImg = avatarImgObj.AddComponent<Image>();
            avatarImg.preserveAspect = true;

            // Navigation Buttons (Prev < / Next >)
            GameObject prevBtnObj = CreateButton("Btn_Prev", leftCol.transform, new Vector2(-130, -160), new Vector2(70, 60), "<", vietFont);
            GameObject nextBtnObj = CreateButton("Btn_Next", leftCol.transform, new Vector2(130, -160), new Vector2(70, 60), ">", vietFont);

            // 6. Right Column: Character Info & Skills
            GameObject rightCol = CreateUIElement("RightColumn_Info", panel.transform);
            RectTransform rightRT = rightCol.GetComponent<RectTransform>();
            rightRT.anchorMin = new Vector2(1f, 0.5f);
            rightRT.anchorMax = new Vector2(1f, 0.5f);
            rightRT.pivot = new Vector2(0.5f, 0.5f);
            rightRT.anchoredPosition = new Vector2(-330, -20);
            rightRT.sizeDelta = new Vector2(560, 480);

            // Character Name & Element
            GameObject nameObj = CreateUIElement("Text_CharacterName", rightCol.transform);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 1);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.pivot = new Vector2(0, 1);
            nameRT.anchoredPosition = new Vector2(10, -10);
            nameRT.sizeDelta = new Vector2(0, 50);
            var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) nameTMP.font = vietFont;
            nameTMP.fontSize = 38;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color = new Color(1f, 0.85f, 0.4f);

            GameObject elemObj = CreateUIElement("Text_Element", rightCol.transform);
            RectTransform elemRT = elemObj.GetComponent<RectTransform>();
            elemRT.anchorMin = new Vector2(0, 1);
            elemRT.anchorMax = new Vector2(1, 1);
            elemRT.pivot = new Vector2(0, 1);
            elemRT.anchoredPosition = new Vector2(10, -65);
            elemRT.sizeDelta = new Vector2(0, 35);
            var elemTMP = elemObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) elemTMP.font = vietFont;
            elemTMP.fontSize = 24;
            elemTMP.fontStyle = FontStyles.Bold;

            // Description
            GameObject descObj = CreateUIElement("Text_Description", rightCol.transform);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 1);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0, 1);
            descRT.anchoredPosition = new Vector2(10, -110);
            descRT.sizeDelta = new Vector2(0, 100);
            var descTMP = descObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) descTMP.font = vietFont;
            descTMP.fontSize = 20;
            descTMP.color = new Color(0.85f, 0.85f, 0.85f);
            descTMP.enableWordWrapping = true;

            // Signature Skill Block
            GameObject skillCard = CreateUIElement("Card_SignatureSkill", rightCol.transform);
            RectTransform skillRT = skillCard.GetComponent<RectTransform>();
            skillRT.anchorMin = new Vector2(0, 1);
            skillRT.anchorMax = new Vector2(1, 1);
            skillRT.pivot = new Vector2(0, 1);
            skillRT.anchoredPosition = new Vector2(0, -210);
            skillRT.sizeDelta = new Vector2(0, 95);
            var skillBg = skillCard.AddComponent<Image>();
            skillBg.color = new Color(0.08f, 0.08f, 0.12f, 0.8f);

            GameObject skillHeader = CreateUIElement("Text_SkillHeader", skillCard.transform);
            RectTransform shRT = skillHeader.GetComponent<RectTransform>();
            shRT.anchorMin = new Vector2(0, 1);
            shRT.anchorMax = new Vector2(1, 1);
            shRT.pivot = new Vector2(0, 1);
            shRT.anchoredPosition = new Vector2(10, -6);
            shRT.sizeDelta = new Vector2(0, 24);
            var shTMP = skillHeader.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) shTMP.font = vietFont;
            shTMP.text = "<color=#FFD700>KY NANG CHU DONG (SIGNATURE SKILL):</color>";
            shTMP.fontSize = 16;
            shTMP.fontStyle = FontStyles.Bold;

            GameObject skillTextObj = CreateUIElement("Text_SignatureSkill", skillCard.transform);
            RectTransform stRT = skillTextObj.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0, 0);
            stRT.anchorMax = new Vector2(1, 1);
            stRT.offsetMin = new Vector2(10, 6);
            stRT.offsetMax = new Vector2(-10, -28);
            var stTMP = skillTextObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) stTMP.font = vietFont;
            stTMP.fontSize = 16;
            stTMP.color = new Color(0.9f, 0.9f, 0.9f);
            stTMP.enableWordWrapping = true;

            // Passive Trait Block
            GameObject passiveCard = CreateUIElement("Card_PassiveTrait", rightCol.transform);
            RectTransform passiveRT = passiveCard.GetComponent<RectTransform>();
            passiveRT.anchorMin = new Vector2(0, 1);
            passiveRT.anchorMax = new Vector2(1, 1);
            passiveRT.pivot = new Vector2(0, 1);
            passiveRT.anchoredPosition = new Vector2(0, -315);
            passiveRT.sizeDelta = new Vector2(0, 85);
            var passiveBg = passiveCard.AddComponent<Image>();
            passiveBg.color = new Color(0.08f, 0.10f, 0.12f, 0.8f);

            GameObject passiveHeader = CreateUIElement("Text_PassiveHeader", passiveCard.transform);
            RectTransform phRT = passiveHeader.GetComponent<RectTransform>();
            phRT.anchorMin = new Vector2(0, 1);
            phRT.anchorMax = new Vector2(1, 1);
            phRT.pivot = new Vector2(0, 1);
            phRT.anchoredPosition = new Vector2(10, -6);
            phRT.sizeDelta = new Vector2(0, 24);
            var phTMP = passiveHeader.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) phTMP.font = vietFont;
            phTMP.text = "<color=#4DEEEA>NOI TAI DOC QUYEN (PASSIVE TRAIT):</color>";
            phTMP.fontSize = 16;
            phTMP.fontStyle = FontStyles.Bold;

            GameObject passiveTextObj = CreateUIElement("Text_PassiveTrait", passiveCard.transform);
            RectTransform ptRT = passiveTextObj.GetComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0, 0);
            ptRT.anchorMax = new Vector2(1, 1);
            ptRT.offsetMin = new Vector2(10, 6);
            ptRT.offsetMax = new Vector2(-10, -28);
            var ptTMP = passiveTextObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) ptTMP.font = vietFont;
            ptTMP.fontSize = 16;
            ptTMP.color = new Color(0.9f, 0.9f, 0.9f);
            ptTMP.enableWordWrapping = true;

            // 7. Select Button: "XÁC NHẬN CHỌN TƯỚNG"
            GameObject selectBtnObj = CreateButton("Btn_Select", rightCol.transform, new Vector2(0, -345), new Vector2(560, 56), "XAC NHAN CHON TUONG", vietFont);
            var btnImg = selectBtnObj.GetComponent<Image>();
            btnImg.color = new Color(0.85f, 0.45f, 0.15f); // Cam Đồng sáng
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

            soView.FindProperty("_selectButton").objectReferenceValue = selectBtnObj.GetComponent<Button>();
            soView.FindProperty("_prevButton").objectReferenceValue = prevBtnObj.GetComponent<Button>();
            soView.FindProperty("_nextButton").objectReferenceValue = nextBtnObj.GetComponent<Button>();
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
                    playerPrefab = pThuSinh, // Fallback
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
                prefabsProp.GetArrayElementAtIndex(3).objectReferenceValue = pThuSinh;
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

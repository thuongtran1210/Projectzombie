#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.Gacha;
using ProjectZombie.Features.MetaProgression.Gacha;

namespace ProjectZombie.Features.MetaProgression.Gacha.Editor
{
    /// <summary>
    /// Builder chuyên nghiệp dựng toàn bộ Modal Gacha Bảo Rương Vạn Cổ chuẩn Cổ Phong Đông Sơn
    /// Khớp 100% tỷ lệ Modal (1760 x 960), Cuộn Sớ Header, Nút Close gỗ mun [X], Bố cục 2 Cột và Nút Quay căn chuẩn.
    /// </summary>
    public static class GachaUIPrefabBuilder
    {
        private static readonly Color ColorBgOverlay = new Color(0.04f, 0.03f, 0.06f, 0.90f);
        private static readonly Color ColorWoodDark = new Color(0.12f, 0.08f, 0.06f, 0.95f);
        private static readonly Color ColorGold = new Color(0.96f, 0.84f, 0.45f, 1f);

        [MenuItem("ProjectZombie/Gacha/Build Gacha UI Prefabs (1-Click)", priority = 203)]
        public static void BuildGachaUIPrefabs()
        {
            // 1. Cấu hình Sprite & Animation
            GachaSpriteImporterSetup.ConfigureSprites();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GachaChestAnimationGenerator.CreateChestAnimation();

            string prefabDir = "Assets/_Prefabs/UI/Gacha";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            // Nạp Font Tiếng Việt chuẩn
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 2. Tạo GachaCardRewardView Prefab
            string cardPrefabPath = $"{prefabDir}/GachaCardRewardItem.prefab";
            var cardGo = CreateCardRewardObject(vietFont);
            var cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardGo, cardPrefabPath);
            GameObject.DestroyImmediate(cardGo);
            Debug.Log($"[GachaUIPrefabBuilder] Đã tạo GachaCardRewardItem Prefab tại '{cardPrefabPath}'.");

            // 3. Tạo GachaShopPanel Prefab (Modal Cổ Phong Cao Cấp)
            string shopPrefabPath = $"{prefabDir}/GachaShopPanel.prefab";
            var shopGo = CreateShopPanelObject(cardPrefab.GetComponent<GachaCardRewardView>(), vietFont);
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(shopGo, shopPrefabPath);
            Debug.Log($"[GachaUIPrefabBuilder] Đã tạo GachaShopPanel Prefab tại '{shopPrefabPath}'.");

            // 4. Đồng bộ trực tiếp vào Scene (Canvas_MetaMenu)
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                var oldUI = GameObject.Find("Panel_GachaShop");
                if (oldUI != null && oldUI != shopGo) Object.DestroyImmediate(oldUI);

                var metaCanvas = GameObject.Find("Canvas_MetaMenu");
                Transform targetParent = metaCanvas != null ? metaCanvas.transform : canvas.transform;

                shopGo.transform.SetParent(targetParent, false);
                var cg = shopGo.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;
                }

                var metaMgr = Object.FindAnyObjectByType<MetaUIManager>();
                if (metaMgr != null)
                {
                    SerializedObject soMeta = new SerializedObject(metaMgr);
                    soMeta.FindProperty("_gachaShopScreen").objectReferenceValue = shopGo.GetComponent<GachaChestView>();
                    soMeta.ApplyModifiedProperties();
                    EditorUtility.SetDirty(metaMgr);
                }

                EditorUtility.SetDirty(shopGo);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetParent.gameObject.scene);
                Debug.Log($"<color=#00FF88>[GachaUIPrefabBuilder]</color> Đã đồng bộ Panel_GachaShop vào Scene (Canvas_MetaMenu) thành công!");
            }
            else
            {
                GameObject.DestroyImmediate(shopGo);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateCardRewardObject(TMP_FontAsset font)
        {
            var go = new GameObject("GachaCardRewardItem", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 220);

            // Nền thẻ
            var bgGo = new GameObject("Card_BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.color = new Color(0.12f, 0.09f, 0.14f, 0.95f);

            // Khung viền Rarity 9-Slice
            var borderGo = new GameObject("Rarity_Border", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(go.transform, false);
            var borderRect = borderGo.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            var borderImg = borderGo.GetComponent<Image>();
            borderImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Border_Rarity_Common.png");
            borderImg.type = Image.Type.Sliced;

            // Icon Pháp Bảo
            var iconGo = new GameObject("Relic_Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(0, 30);
            iconRect.sizeDelta = new Vector2(90, 90);
            iconGo.GetComponent<Image>().preserveAspect = true;

            // Tên Pháp Bảo
            var nameGo = CreateTextMeshProGo("Relic_Name_Text", go.transform, font, 14);
            var nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -35);
            nameRect.sizeDelta = new Vector2(150, 30);
            var nameText = nameGo.GetComponent<TextMeshProUGUI>();

            // Rarity Text
            var rarityGo = CreateTextMeshProGo("Rarity_Text", go.transform, font, 12);
            var rarityRect = rarityGo.GetComponent<RectTransform>();
            rarityRect.anchoredPosition = new Vector2(0, -60);
            rarityRect.sizeDelta = new Vector2(150, 25);
            var rarityText = rarityGo.GetComponent<TextMeshProUGUI>();

            // Shard Count Text
            var shardGo = CreateTextMeshProGo("Shard_Count_Text", go.transform, font, 13);
            var shardRect = shardGo.GetComponent<RectTransform>();
            shardRect.anchoredPosition = new Vector2(0, -85);
            shardRect.sizeDelta = new Vector2(150, 25);
            var shardText = shardGo.GetComponent<TextMeshProUGUI>();

            // Star Level Text
            var starGo = CreateTextMeshProGo("Star_Level_Text", go.transform, font, 13);
            var starRect = starGo.GetComponent<RectTransform>();
            starRect.anchoredPosition = new Vector2(0, 80);
            starRect.sizeDelta = new Vector2(150, 25);
            var starText = starGo.GetComponent<TextMeshProUGUI>();

            // Badge MỚI
            var badgeGo = new GameObject("New_Badge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(go.transform, false);
            var badgeRect = badgeGo.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0, 1);
            badgeRect.anchorMax = new Vector2(0, 1);
            badgeRect.anchoredPosition = new Vector2(25, -20);
            badgeRect.sizeDelta = new Vector2(40, 20);
            var badgeImg = badgeGo.GetComponent<Image>();
            badgeImg.color = new Color(0.82f, 0.22f, 0.22f, 1f);

            var badgeTextGo = CreateTextMeshProGo("Text", badgeGo.transform, font, 10);
            var bText = badgeTextGo.GetComponent<TextMeshProUGUI>();
            bText.text = "<b>MỚI</b>";

            // Gắn Component GachaCardRewardView
            var cardView = go.AddComponent<GachaCardRewardView>();
            var so = new SerializedObject(cardView);
            so.FindProperty("_iconImage").objectReferenceValue = iconGo.GetComponent<Image>();
            so.FindProperty("_rarityBorderImage").objectReferenceValue = borderImg;
            so.FindProperty("_nameText").objectReferenceValue = nameText;
            so.FindProperty("_rarityText").objectReferenceValue = rarityText;
            so.FindProperty("_shardCountText").objectReferenceValue = shardText;
            so.FindProperty("_starLevelText").objectReferenceValue = starText;
            so.FindProperty("_newBadgeObject").objectReferenceValue = badgeGo;
            so.ApplyModifiedProperties();

            return go;
        }

        private static GameObject CreateTextMeshProGo(string name, Transform parent, TMP_FontAsset font, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return go;
        }

        private static GameObject CreateShopPanelObject(GachaCardRewardView cardPrefab, TMP_FontAsset font)
        {
            // 1. Root Screen Panel (100% Stretch bao trọn màn hình)
            var panelGo = new GameObject("Panel_GachaShop", typeof(RectTransform), typeof(CanvasGroup));
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // 2. Dim Background (Lớp nền tối che sảnh)
            var dimGo = new GameObject("Dim_Background", typeof(RectTransform), typeof(Image), typeof(Button));
            dimGo.transform.SetParent(panelGo.transform, false);
            var dimRect = dimGo.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;
            dimGo.GetComponent<Image>().color = ColorBgOverlay;
            var dimBtn = dimGo.GetComponent<Button>();

            // 3. Modal Box Trung Tâm (1760 x 960) Chuẩn Cổ Phong
            var modalGo = new GameObject("Modal_Gacha", typeof(RectTransform), typeof(Image));
            modalGo.transform.SetParent(panelGo.transform, false);
            var modalRect = modalGo.GetComponent<RectTransform>();
            modalRect.anchorMin = new Vector2(0.5f, 0.5f);
            modalRect.anchorMax = new Vector2(0.5f, 0.5f);
            modalRect.pivot = new Vector2(0.5f, 0.5f);
            modalRect.sizeDelta = new Vector2(1760, 960);
            modalRect.anchoredPosition = Vector2.zero;

            var modalImg = modalGo.GetComponent<Image>();
            modalImg.color = Color.white;
            modalImg.type = Image.Type.Sliced;
            Sprite modalFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Modal_TangBaoCac_9Slice.png");
            if (modalFrame != null) modalImg.sprite = modalFrame;

            // 4. Header Top Bar (Cuộn Giấy Da Tiêu Đề + Nút Đóng [X])
            var headerGo = new GameObject("Header_TopBar", typeof(RectTransform));
            headerGo.transform.SetParent(modalGo.transform, false);
            var headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = new Vector2(0, 20);
            headerRect.sizeDelta = new Vector2(0, 88);

            // 4.1. Cuộn Giấy Da Tiêu Đề
            var titleScrollGo = new GameObject("Banner_Parchment_Title", typeof(RectTransform), typeof(Image));
            titleScrollGo.transform.SetParent(headerGo.transform, false);
            var tsRect = titleScrollGo.GetComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0.5f, 0.5f);
            tsRect.anchorMax = new Vector2(0.5f, 0.5f);
            tsRect.pivot = new Vector2(0.5f, 0.5f);
            tsRect.anchoredPosition = new Vector2(0, 4);
            tsRect.sizeDelta = new Vector2(680, 96);
            var tsImg = titleScrollGo.GetComponent<Image>();
            tsImg.type = Image.Type.Sliced;
            Sprite bannerScroll = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Banner_Parchment_Scroll.png");
            if (bannerScroll != null) tsImg.sprite = bannerScroll;

            var titleTextGo = new GameObject("Txt_MainTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleTextGo.transform.SetParent(titleScrollGo.transform, false);
            var ttRect = titleTextGo.GetComponent<RectTransform>();
            ttRect.anchoredPosition = new Vector2(0, 14);
            ttRect.sizeDelta = new Vector2(500, 32);
            var titleText = titleTextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) titleText.font = font;
            titleText.text = "- BẢO RƯƠNG VẠN CỔ -";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.18f, 0.12f, 0.08f, 1f);

            var subTitleGo = new GameObject("Txt_SubTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subTitleGo.transform.SetParent(titleScrollGo.transform, false);
            var stRect = subTitleGo.GetComponent<RectTransform>();
            stRect.anchoredPosition = new Vector2(0, -16);
            stRect.sizeDelta = new Vector2(500, 24);
            var subTitleText = subTitleGo.GetComponent<TextMeshProUGUI>();
            if (font != null) subTitleText.font = font;
            subTitleText.text = "Thu Thập Pháp Bảo Thần Binh Viễn Cổ";
            subTitleText.fontSize = 15;
            subTitleText.fontStyle = FontStyles.Bold;
            subTitleText.alignment = TextAlignmentOptions.Center;
            subTitleText.color = new Color(0.35f, 0.25f, 0.18f, 1f);

            // 4.2. Hiển thị Cổ Tiền góc trái Header
            var coinBoxGo = new GameObject("Box_CoTien", typeof(RectTransform), typeof(Image));
            coinBoxGo.transform.SetParent(headerGo.transform, false);
            var cbRect = coinBoxGo.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0, 0.5f);
            cbRect.anchorMax = new Vector2(0, 0.5f);
            cbRect.pivot = new Vector2(0, 0.5f);
            cbRect.anchoredPosition = new Vector2(30, 0);
            cbRect.sizeDelta = new Vector2(200, 42);
            var cbImg = coinBoxGo.GetComponent<Image>();
            cbImg.type = Image.Type.Sliced;
            Sprite pillWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pill_Currency_Wood.png");
            if (pillWood != null) cbImg.sprite = pillWood;

            var coinTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            coinTextGo.transform.SetParent(coinBoxGo.transform, false);
            var ctRect = coinTextGo.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.offsetMin = new Vector2(16, 0);
            ctRect.offsetMax = new Vector2(-16, 0);
            var coinText = coinTextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) coinText.font = font;
            coinText.text = "Cổ Tiền: <color=#FFD700><b>0</b></color>";
            coinText.fontSize = 16;
            coinText.alignment = TextAlignmentOptions.Center;

            // 4.3. Nút Đóng / Thoát Gỗ Mun [X] Góc Phải
            var closeGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(headerGo.transform, false);
            var closeRect = closeGo.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.anchoredPosition = new Vector2(-20, 0);
            closeRect.sizeDelta = new Vector2(64, 64);
            var closeImg = closeGo.GetComponent<Image>();
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Close_X_Wood.png");
            if (btnCloseX != null)
            {
                closeImg.sprite = btnCloseX;
                closeImg.preserveAspect = true;
            }
            var closeBtn = closeGo.GetComponent<Button>();

            // 5. Body Section (Chia 2 Cột Cân Đối)
            var bodyGo = new GameObject("Container_Body2Cols", typeof(RectTransform));
            bodyGo.transform.SetParent(modalGo.transform, false);
            var bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(36, 120); // 120px đáy chừa chỗ cho Nút Quay
            bodyRect.offsetMax = new Vector2(-36, -88);

            var bodyHlg = bodyGo.AddComponent<HorizontalLayoutGroup>();
            bodyHlg.spacing = 30;
            bodyHlg.childControlWidth = true;
            bodyHlg.childControlHeight = true;

            // 5.1. Cột Trái: Danh Sách Các Loại Rương (Chest Selection List)
            var leftColGo = new GameObject("Col_Left_Banner", typeof(RectTransform), typeof(Image));
            leftColGo.transform.SetParent(bodyGo.transform, false);
            var leftColImg = leftColGo.GetComponent<Image>();
            leftColImg.color = new Color(0.10f, 0.08f, 0.07f, 0.9f);
            leftColImg.type = Image.Type.Sliced;
            Sprite leftColBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");
            if (leftColBg != null) leftColImg.sprite = leftColBg;

            // 5.1.1. Header Tiêu Đề Danh Sách Rương
            var listHeaderGo = new GameObject("Header_ChestList", typeof(RectTransform), typeof(Image));
            listHeaderGo.transform.SetParent(leftColGo.transform, false);
            var lhRect = listHeaderGo.GetComponent<RectTransform>();
            lhRect.anchorMin = new Vector2(0, 1);
            lhRect.anchorMax = new Vector2(1, 1);
            lhRect.pivot = new Vector2(0.5f, 1);
            lhRect.anchoredPosition = new Vector2(0, -10);
            lhRect.sizeDelta = new Vector2(-24, 46);
            var lhImg = listHeaderGo.GetComponent<Image>();
            lhImg.type = Image.Type.Sliced;
            Sprite listHeaderSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Header_Wood_Bar_VongXuyen.png");
            if (listHeaderSprite != null) lhImg.sprite = listHeaderSprite;

            var lhTextGo = new GameObject("Txt_Header", typeof(RectTransform), typeof(TextMeshProUGUI));
            lhTextGo.transform.SetParent(listHeaderGo.transform, false);
            var lhtRect = lhTextGo.GetComponent<RectTransform>();
            lhtRect.anchorMin = Vector2.zero;
            lhtRect.anchorMax = Vector2.one;
            lhtRect.sizeDelta = Vector2.zero;
            var lhText = lhTextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) lhText.font = font;
            lhText.text = "DANH SÁCH BẢO RƯƠNG";
            lhText.fontSize = 17;
            lhText.fontStyle = FontStyles.Bold;
            lhText.alignment = TextAlignmentOptions.Center;
            lhText.color = new Color(0.98f, 0.88f, 0.50f, 1f);

            // 5.1.2. ScrollView Danh Sách Rương
            var scrollListGo = new GameObject("ScrollView_ChestList", typeof(RectTransform), typeof(ScrollRect));
            scrollListGo.transform.SetParent(leftColGo.transform, false);
            var slRect = scrollListGo.GetComponent<RectTransform>();
            slRect.anchorMin = Vector2.zero;
            slRect.anchorMax = Vector2.one;
            slRect.offsetMin = new Vector2(12, 12);
            slRect.offsetMax = new Vector2(-12, -64);

            var scrollRect = scrollListGo.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(scrollListGo.transform, false);
            var vpRect = viewportGo.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.sizeDelta = Vector2.zero;
            scrollRect.viewport = vpRect;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var cntRect = contentGo.GetComponent<RectTransform>();
            cntRect.anchorMin = new Vector2(0, 1);
            cntRect.anchorMax = new Vector2(1, 1);
            cntRect.pivot = new Vector2(0.5f, 1);
            cntRect.sizeDelta = new Vector2(0, 500);
            scrollRect.content = cntRect;

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(4, 4, 6, 6);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Sprites cho Item Card
            Sprite slotBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            Sprite selectedGlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
            Sprite badgePillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite chestBronzeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Chest_Frame_01.png");

            // 1. Rương Đồng (Bảo Rương Vạn Cổ - KHẢ DỤNG)
            CreateChestSelectionItem(
                contentGo.transform, font, "Item_Chest_Bronze",
                "Bảo Rương Đồng",
                "Bảo Rương Vạn Cổ - 100 Cổ Tiền/lượt",
                "<color=#00FF88>ĐANG MỞ</color>",
                isSelected: true, isUnlocked: true,
                chestBronzeSprite, slotBgSprite, selectedGlowSprite, badgePillSprite);

            // 2. Gương Chiêu Mộ Anh Hùng (Thẻ Tướng - KHẢ DỤNG)
            CreateChestSelectionItem(
                contentGo.transform, font, "Item_Chest_Hero",
                "Gương Anh Hùng",
                "Chiêu Mộ Tướng - 200 Cổ Tiền/lượt",
                "<color=#00FF88>ĐANG MỞ</color>",
                isSelected: false, isUnlocked: true,
                chestBronzeSprite, slotBgSprite, selectedGlowSprite, badgePillSprite);

            // 3. Rương Huyền Thiết (Bạc - KHÓA)
            CreateChestSelectionItem(
                contentGo.transform, font, "Item_Chest_Silver",
                "Bảo Rương Bạc",
                "<color=#888888>Mở tại Cảnh Giới Trúc Cơ (Tầng 2)</color>",
                "<color=#FFAA00>CHƯA MỞ</color>",
                isSelected: false, isUnlocked: false,
                chestBronzeSprite, slotBgSprite, selectedGlowSprite, badgePillSprite);

            // 3. Rương Hoàng Kim (Vàng - KHÓA)
            CreateChestSelectionItem(
                contentGo.transform, font, "Item_Chest_Gold",
                "Bảo Rương Vàng",
                "<color=#888888>Mở khi vượt Ải Vong Xuyên Hà</color>",
                "<color=#FFAA00>CHƯA MỞ</color>",
                isSelected: false, isUnlocked: false,
                chestBronzeSprite, slotBgSprite, selectedGlowSprite, badgePillSprite);

            // 4. Rương Tiên Ngọc (Ngọc - KHÓA)
            CreateChestSelectionItem(
                contentGo.transform, font, "Item_Chest_Jade",
                "Bảo Rương Ngọc",
                "<color=#888888>Sự Kiện Đặc Biệt (Sắp Ra Mắt)</color>",
                "<color=#FFAA00>CHƯA MỞ</color>",
                isSelected: false, isUnlocked: false,
                chestBronzeSprite, slotBgSprite, selectedGlowSprite, badgePillSprite);

            // 5.2. Cột Phải: Vùng Rương Bát Quái & Hào Quang & Pity
            var rightColGo = new GameObject("Col_Right_ChestStage", typeof(RectTransform), typeof(Image));
            rightColGo.transform.SetParent(bodyGo.transform, false);
            var rightColImg = rightColGo.GetComponent<Image>();
            rightColImg.color = new Color(0.14f, 0.10f, 0.08f, 0.8f);
            rightColImg.type = Image.Type.Sliced;

            // Hào quang Sunburst trong suốt xoay tròn sau rương
            var sunburstGo = new GameObject("Sunburst_VFX", typeof(RectTransform), typeof(Image));
            sunburstGo.transform.SetParent(rightColGo.transform, false);
            var sunRect = sunburstGo.GetComponent<RectTransform>();
            sunRect.anchoredPosition = new Vector2(0, 50);
            sunRect.sizeDelta = new Vector2(460, 460);
            var sunImg = sunburstGo.GetComponent<Image>();
            sunImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/UI_Gacha_Sunburst_VFX.png") ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/UI_Gacha_Sunburst_VFX.jpg");
            sunImg.color = new Color(1f, 0.85f, 0.35f, 0.75f);
            sunImg.preserveAspect = true;

            // Rương Sprite 2D & Animator
            var chestGo = new GameObject("Chest_Sprite", typeof(RectTransform), typeof(Image), typeof(Animator));
            chestGo.transform.SetParent(rightColGo.transform, false);
            var cRect = chestGo.GetComponent<RectTransform>();
            cRect.anchoredPosition = new Vector2(0, 30);
            cRect.sizeDelta = new Vector2(360, 260);
            var cImg = chestGo.GetComponent<Image>();
            Sprite defaultChestSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Chest_Frame_01.png");
            if (defaultChestSprite == null)
            {
                var all = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/Gacha/Chest_Frame_01.png");
                foreach (var a in all) if (a is Sprite s) { defaultChestSprite = s; break; }
            }
            cImg.sprite = defaultChestSprite;
            cImg.preserveAspect = true;
            var cAnim = chestGo.GetComponent<Animator>();
            cAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/UI/Gacha/Animations/Chest_AnimatorController.controller");
            cAnim.updateMode = AnimatorUpdateMode.UnscaledTime;

            // Pity Texts
            var pityLegGo = new GameObject("Legendary_Pity_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            pityLegGo.transform.SetParent(rightColGo.transform, false);
            var pLegRect = pityLegGo.GetComponent<RectTransform>();
            pLegRect.anchoredPosition = new Vector2(0, -150);
            pLegRect.sizeDelta = new Vector2(500, 30);
            var pLegText = pityLegGo.GetComponent<TextMeshProUGUI>();
            if (font != null) pLegText.font = font;
            pLegText.alignment = TextAlignmentOptions.Center;
            pLegText.fontSize = 15;

            var pityEpicGo = new GameObject("Epic_Pity_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            pityEpicGo.transform.SetParent(rightColGo.transform, false);
            var pEpicRect = pityEpicGo.GetComponent<RectTransform>();
            pEpicRect.anchoredPosition = new Vector2(0, -185);
            pEpicRect.sizeDelta = new Vector2(500, 30);
            var pEpicText = pityEpicGo.GetComponent<TextMeshProUGUI>();
            if (font != null) pEpicText.font = font;
            pEpicText.alignment = TextAlignmentOptions.Center;
            pEpicText.fontSize = 14;

            // Status text thông báo (Không đủ tiền, lỗi...)
            var statusMsgGo = new GameObject("Txt_StatusMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusMsgGo.transform.SetParent(modalGo.transform, false);
            var statusMsgRect = statusMsgGo.GetComponent<RectTransform>();
            statusMsgRect.anchorMin = new Vector2(0.5f, 0);
            statusMsgRect.anchorMax = new Vector2(0.5f, 0);
            statusMsgRect.pivot = new Vector2(0.5f, 0);
            statusMsgRect.anchoredPosition = new Vector2(0, 96);
            statusMsgRect.sizeDelta = new Vector2(600, 30);
            var statusMsgText = statusMsgGo.GetComponent<TextMeshProUGUI>();
            if (font != null) statusMsgText.font = font;
            statusMsgText.text = "";
            statusMsgText.fontSize = 15;
            statusMsgText.alignment = TextAlignmentOptions.Center;
            statusMsgText.raycastTarget = false;

            // 6. Nút Quay Đáy Modal (Quay 1x & Quay 10x)
            var bottomActionsGo = new GameObject("Bottom_Action_Bar", typeof(RectTransform));
            bottomActionsGo.transform.SetParent(modalGo.transform, false);
            var baActionRect = bottomActionsGo.GetComponent<RectTransform>();
            baActionRect.anchorMin = new Vector2(0.5f, 0);
            baActionRect.anchorMax = new Vector2(0.5f, 0);
            baActionRect.pivot = new Vector2(0.5f, 0);
            baActionRect.anchoredPosition = new Vector2(0, 24);
            baActionRect.sizeDelta = new Vector2(640, 75);

            var baHlg = bottomActionsGo.AddComponent<HorizontalLayoutGroup>();
            baHlg.spacing = 30;
            baHlg.childAlignment = TextAnchor.MiddleCenter;
            baHlg.childControlWidth = false;
            baHlg.childControlHeight = false;

            // Nút Quay 1x
            var btn1Go = new GameObject("Btn_Roll_1x", typeof(RectTransform), typeof(Image), typeof(Button));
            btn1Go.transform.SetParent(bottomActionsGo.transform, false);
            btn1Go.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 68);
            var b1Img = btn1Go.GetComponent<Image>();
            b1Img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Btn_Gacha_Wood_Single.png");
            b1Img.type = Image.Type.Sliced;
            var btn1 = btn1Go.GetComponent<Button>();

            var b1TextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            b1TextGo.transform.SetParent(btn1Go.transform, false);
            var b1tRect = b1TextGo.GetComponent<RectTransform>();
            b1tRect.anchorMin = Vector2.zero;
            b1tRect.anchorMax = Vector2.one;
            b1tRect.offsetMin = new Vector2(10, 4);
            b1tRect.offsetMax = new Vector2(-10, -4);
            var b1Text = b1TextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) b1Text.font = font;
            b1Text.text = "Quay 1x\n<color=#FFD700>100 Cổ Tiền</color>";
            b1Text.fontSize = 15;
            b1Text.alignment = TextAlignmentOptions.Center;
            b1Text.raycastTarget = false;

            // Nút Quay 10x
            var btn10Go = new GameObject("Btn_Roll_10x", typeof(RectTransform), typeof(Image), typeof(Button));
            btn10Go.transform.SetParent(bottomActionsGo.transform, false);
            btn10Go.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 68);
            var b10Img = btn10Go.GetComponent<Image>();
            b10Img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Btn_Gacha_Red_Multi.png");
            b10Img.type = Image.Type.Sliced;
            var btn10 = btn10Go.GetComponent<Button>();

            var b10TextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            b10TextGo.transform.SetParent(btn10Go.transform, false);
            var b10tRect = b10TextGo.GetComponent<RectTransform>();
            b10tRect.anchorMin = Vector2.zero;
            b10tRect.anchorMax = Vector2.one;
            b10tRect.offsetMin = new Vector2(10, 4);
            b10tRect.offsetMax = new Vector2(-10, -4);
            var b10Text = b10TextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) b10Text.font = font;
            b10Text.text = "Quay 10x\n<color=#FFD700>900 Cổ Tiền</color>";
            b10Text.fontSize = 15;
            b10Text.alignment = TextAlignmentOptions.Center;
            b10Text.raycastTarget = false;

            // 7. Result Modal Popup
            var resultPopupGo = new GameObject("Result_Popup_Panel", typeof(RectTransform), typeof(Image));
            resultPopupGo.transform.SetParent(panelGo.transform, false);
            var rRect = resultPopupGo.GetComponent<RectTransform>();
            rRect.anchorMin = Vector2.zero;
            rRect.anchorMax = Vector2.one;
            rRect.sizeDelta = Vector2.zero;
            resultPopupGo.GetComponent<Image>().color = new Color(0.04f, 0.03f, 0.06f, 0.96f);

            var gridGo = new GameObject("Cards_Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(resultPopupGo.transform, false);
            var gRect = gridGo.GetComponent<RectTransform>();
            gRect.anchoredPosition = new Vector2(0, 30);
            gRect.sizeDelta = new Vector2(900, 480);
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 220);
            grid.spacing = new Vector2(16, 16);
            grid.childAlignment = TextAnchor.MiddleCenter;

            var closeResultBtnGo = new GameObject("Btn_Close_Result", typeof(RectTransform), typeof(Image), typeof(Button));
            closeResultBtnGo.transform.SetParent(resultPopupGo.transform, false);
            var closeResultRect = closeResultBtnGo.GetComponent<RectTransform>();
            closeResultRect.anchoredPosition = new Vector2(0, -380);
            closeResultRect.sizeDelta = new Vector2(240, 60);
            var crImg = closeResultBtnGo.GetComponent<Image>();
            crImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Btn_Gacha_Wood_Single.png");
            crImg.type = Image.Type.Sliced;

            var crTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            crTextGo.transform.SetParent(closeResultBtnGo.transform, false);
            var crText = crTextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) crText.font = font;
            crText.text = "<b>XÁC NHẬN</b>";
            crText.fontSize = 18;
            crText.alignment = TextAlignmentOptions.Center;

            // 8. Gắn View và Presenter
            var view = panelGo.AddComponent<GachaChestView>();
            var presenter = panelGo.AddComponent<GachaChestPresenter>();

            var vSo = new SerializedObject(view);
            vSo.FindProperty("_modalContainer").objectReferenceValue = modalRect;
            vSo.FindProperty("_dimBackgroundButton").objectReferenceValue = dimBtn;
            vSo.FindProperty("_screenCanvasGroup").objectReferenceValue = panelGo.GetComponent<CanvasGroup>();
            vSo.FindProperty("_backButton").objectReferenceValue = closeBtn;
            vSo.FindProperty("_currencyBalanceText").objectReferenceValue = coinText;
            vSo.FindProperty("_bannerTitleText").objectReferenceValue = titleText;
            vSo.FindProperty("_bannerDescriptionText").objectReferenceValue = subTitleText;
            vSo.FindProperty("_statusMessageText").objectReferenceValue = statusMsgText;
            vSo.FindProperty("_singleCostText").objectReferenceValue = b1Text;
            vSo.FindProperty("_multiCostText").objectReferenceValue = b10Text;
            vSo.FindProperty("_legendaryPityText").objectReferenceValue = pLegText;
            vSo.FindProperty("_epicPityText").objectReferenceValue = pEpicText;
            vSo.FindProperty("_singleRollButton").objectReferenceValue = btn1;
            vSo.FindProperty("_multiRollButton").objectReferenceValue = btn10;
            vSo.FindProperty("_closeResultButton").objectReferenceValue = closeResultBtnGo.GetComponent<Button>();
            vSo.FindProperty("_chestAnimator").objectReferenceValue = cAnim;
            vSo.FindProperty("_sunburstTransform").objectReferenceValue = sunRect;
            vSo.FindProperty("_sunburstImage").objectReferenceValue = sunImg;
            vSo.FindProperty("_resultPopupPanel").objectReferenceValue = resultPopupGo;
            vSo.FindProperty("_cardsContainer").objectReferenceValue = gridGo.transform;
            vSo.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
            vSo.ApplyModifiedProperties();

            resultPopupGo.SetActive(false);

            return panelGo;
        }

        private static GameObject CreateChestSelectionItem(
            Transform parent, 
            TMP_FontAsset font, 
            string itemName, 
            string title, 
            string subTitle, 
            string statusBadgeText, 
            bool isSelected, 
            bool isUnlocked,
            Sprite chestIcon,
            Sprite slotBg,
            Sprite selectedGlow,
            Sprite badgePill)
        {
            var itemGo = new GameObject(itemName, typeof(RectTransform), typeof(Image), typeof(Button));
            itemGo.transform.SetParent(parent, false);
            var itemRect = itemGo.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, 108);

            var itemImg = itemGo.GetComponent<Image>();
            itemImg.type = Image.Type.Sliced;
            if (slotBg != null) itemImg.sprite = slotBg;
            itemImg.color = isUnlocked ? Color.white : new Color(0.45f, 0.40f, 0.38f, 0.85f);

            var btn = itemGo.GetComponent<Button>();
            btn.interactable = isUnlocked;

            // Viền sáng vàng khi được chọn (Selected Glow)
            if (isSelected && selectedGlow != null)
            {
                var glowGo = new GameObject("Selected_Glow", typeof(RectTransform), typeof(Image));
                glowGo.transform.SetParent(itemGo.transform, false);
                var gRect = glowGo.GetComponent<RectTransform>();
                gRect.anchorMin = Vector2.zero;
                gRect.anchorMax = Vector2.one;
                gRect.sizeDelta = Vector2.zero;
                var gImg = glowGo.GetComponent<Image>();
                gImg.type = Image.Type.Sliced;
                gImg.sprite = selectedGlow;
                gImg.raycastTarget = false;
            }

            // Icon Rương bên trái
            var iconBoxGo = new GameObject("Icon_Box", typeof(RectTransform), typeof(Image));
            iconBoxGo.transform.SetParent(itemGo.transform, false);
            var ibRect = iconBoxGo.GetComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0, 0.5f);
            ibRect.anchorMax = new Vector2(0, 0.5f);
            ibRect.pivot = new Vector2(0, 0.5f);
            ibRect.anchoredPosition = new Vector2(16, 0);
            ibRect.sizeDelta = new Vector2(76, 76);
            var ibImg = iconBoxGo.GetComponent<Image>();
            ibImg.color = new Color(0.08f, 0.06f, 0.05f, 0.8f);

            var iconGo = new GameObject("Chest_Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(iconBoxGo.transform, false);
            var iRect = iconGo.GetComponent<RectTransform>();
            iRect.anchorMin = Vector2.zero;
            iRect.anchorMax = Vector2.one;
            iRect.offsetMin = new Vector2(4, 4);
            iRect.offsetMax = new Vector2(-4, -4);
            var iconImg = iconGo.GetComponent<Image>();
            if (chestIcon != null) iconImg.sprite = chestIcon;
            iconImg.preserveAspect = true;
            iconImg.color = isUnlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.6f);
            iconImg.raycastTarget = false;

            // Vùng Chữ (Tiêu đề + Mô tả)
            var infoGo = new GameObject("Info_Container", typeof(RectTransform));
            infoGo.transform.SetParent(itemGo.transform, false);
            var infoRect = infoGo.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(1, 1);
            infoRect.offsetMin = new Vector2(104, 10);
            infoRect.offsetMax = new Vector2(-120, -10);

            var titleGo = new GameObject("Txt_Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(infoGo.transform, false);
            var tRect = titleGo.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0, 0.5f);
            tRect.anchorMax = new Vector2(1, 1);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var tText = titleGo.GetComponent<TextMeshProUGUI>();
            if (font != null) tText.font = font;
            tText.text = title;
            tText.fontSize = 17;
            tText.fontStyle = FontStyles.Bold;
            tText.alignment = TextAlignmentOptions.MidlineLeft;
            tText.color = isSelected ? new Color(1f, 0.92f, 0.55f, 1f) : (isUnlocked ? new Color(0.9f, 0.85f, 0.75f, 1f) : new Color(0.6f, 0.55f, 0.5f, 1f));

            var subGo = new GameObject("Txt_Sub", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(infoGo.transform, false);
            var sRect = subGo.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0, 0);
            sRect.anchorMax = new Vector2(1, 0.5f);
            sRect.offsetMin = Vector2.zero;
            sRect.offsetMax = Vector2.zero;
            var sText = subGo.GetComponent<TextMeshProUGUI>();
            if (font != null) sText.font = font;
            sText.text = subTitle;
            sText.fontSize = 12.5f;
            sText.alignment = TextAlignmentOptions.MidlineLeft;
            sText.color = isUnlocked ? new Color(0.75f, 0.70f, 0.62f, 1f) : new Color(0.5f, 0.45f, 0.42f, 1f);

            // Badge Trạng Thái Bên Phải
            var badgeGo = new GameObject("Badge_Status", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(itemGo.transform, false);
            var bRect = badgeGo.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(1, 0.5f);
            bRect.anchorMax = new Vector2(1, 0.5f);
            bRect.pivot = new Vector2(1, 0.5f);
            bRect.anchoredPosition = new Vector2(-16, 0);
            bRect.sizeDelta = new Vector2(100, 32);
            var bImg = badgeGo.GetComponent<Image>();
            bImg.type = Image.Type.Sliced;
            if (badgePill != null) bImg.sprite = badgePill;
            bImg.color = isUnlocked ? (isSelected ? new Color(0.15f, 0.45f, 0.25f, 0.9f) : new Color(0.2f, 0.15f, 0.12f, 0.8f)) : new Color(0.25f, 0.20f, 0.18f, 0.8f);

            var btGo = new GameObject("Txt_Badge", typeof(RectTransform), typeof(TextMeshProUGUI));
            btGo.transform.SetParent(badgeGo.transform, false);
            var btRect = btGo.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.sizeDelta = Vector2.zero;
            var btText = btGo.GetComponent<TextMeshProUGUI>();
            if (font != null) btText.font = font;
            btText.text = statusBadgeText;
            btText.fontSize = 11.5f;
            btText.fontStyle = FontStyles.Bold;
            btText.alignment = TextAlignmentOptions.Center;

            return itemGo;
        }
    }
}
#endif

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.Gacha;
using ProjectZombie.Features.MetaProgression.Gacha;

namespace ProjectZombie.Features.MetaProgression.Gacha.Editor
{
    /// <summary>
    /// Editor Tool 1-Click tự động dựng hoàn chỉnh Prefab Gacha Shop Panel & Card Reward UI.
    /// </summary>
    public static class GachaUIPrefabBuilder
    {
        [MenuItem("ProjectZombie/Gacha/Build Gacha UI Prefabs (1-Click)", priority = 203)]
        public static void BuildGachaUIPrefabs()
        {
            // Đảm bảo cấu hình TextureImporter và Animation trước
            GachaSpriteImporterSetup.ConfigureSprites();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GachaChestAnimationGenerator.CreateChestAnimation();

            string prefabDir = "Assets/_Prefabs/UI/Gacha";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            // 1. Tạo GachaCardRewardView Prefab
            string cardPrefabPath = $"{prefabDir}/GachaCardRewardItem.prefab";
            var cardGo = CreateCardRewardObject();
            var cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardGo, cardPrefabPath);
            GameObject.DestroyImmediate(cardGo);
            Debug.Log($"[GachaUIPrefabBuilder] Đã tạo GachaCardRewardItem Prefab tại '{cardPrefabPath}'.");

            // 2. Tạo GachaShopPanel Prefab
            string shopPrefabPath = $"{prefabDir}/GachaShopPanel.prefab";
            var shopGo = CreateShopPanelObject(cardPrefab.GetComponent<GachaCardRewardView>());
            PrefabUtility.SaveAsPrefabAsset(shopGo, shopPrefabPath);
            GameObject.DestroyImmediate(shopGo);
            Debug.Log($"[GachaUIPrefabBuilder] Đã tạo GachaShopPanel Prefab tại '{shopPrefabPath}'.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateCardRewardObject()
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
            bgImg.color = new Color(0.12f, 0.10f, 0.14f, 0.95f);

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

            // Tên Pháp Bảo
            var nameGo = new GameObject("Relic_Name_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(go.transform, false);
            var nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -35);
            nameRect.sizeDelta = new Vector2(150, 30);
            var nameText = nameGo.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontSize = 14;

            // Rarity Text
            var rarityGo = new GameObject("Rarity_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            rarityGo.transform.SetParent(go.transform, false);
            var rarityRect = rarityGo.GetComponent<RectTransform>();
            rarityRect.anchoredPosition = new Vector2(0, -60);
            rarityRect.sizeDelta = new Vector2(150, 25);
            var rarityText = rarityGo.GetComponent<TextMeshProUGUI>();
            rarityText.alignment = TextAlignmentOptions.Center;
            rarityText.fontSize = 12;

            // Shard Count Text
            var shardGo = new GameObject("Shard_Count_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            shardGo.transform.SetParent(go.transform, false);
            var shardRect = shardGo.GetComponent<RectTransform>();
            shardRect.anchoredPosition = new Vector2(0, -85);
            shardRect.sizeDelta = new Vector2(150, 25);
            var shardText = shardGo.GetComponent<TextMeshProUGUI>();
            shardText.alignment = TextAlignmentOptions.Center;
            shardText.fontSize = 13;

            // Star Level Text
            var starGo = new GameObject("Star_Level_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            starGo.transform.SetParent(go.transform, false);
            var starRect = starGo.GetComponent<RectTransform>();
            starRect.anchoredPosition = new Vector2(0, 80);
            starRect.sizeDelta = new Vector2(150, 25);
            var starText = starGo.GetComponent<TextMeshProUGUI>();
            starText.alignment = TextAlignmentOptions.Center;
            starText.fontSize = 13;

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

            var badgeTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeTextGo.transform.SetParent(badgeGo.transform, false);
            var bText = badgeTextGo.GetComponent<TextMeshProUGUI>();
            bText.text = "<b>MỚI</b>";
            bText.fontSize = 10;
            bText.alignment = TextAlignmentOptions.Center;

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

        private static GameObject CreateShopPanelObject(GachaCardRewardView cardPrefab)
        {
            var panelGo = new GameObject("GachaShopPanel", typeof(RectTransform));
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(1920, 1080);

            // 1. Background
            var bgGo = new GameObject("Dark_Backdrop", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(panelGo.transform, false);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(0.08f, 0.07f, 0.10f, 0.98f);

            // 2. Banner Artwork (Cột Trái)
            var bannerGo = new GameObject("Banner_Artwork", typeof(RectTransform), typeof(Image));
            bannerGo.transform.SetParent(panelGo.transform, false);
            var bannerRect = bannerGo.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.05f, 0.2f);
            bannerRect.anchorMax = new Vector2(0.48f, 0.85f);
            bannerRect.sizeDelta = Vector2.zero;
            var bannerImg = bannerGo.GetComponent<Image>();
            bannerImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/UI_Gacha_Banner_Artwork.jpg");

            // 3. Vùng Rương & Hào quang (Cột Phải)
            var chestAreaGo = new GameObject("Chest_Area", typeof(RectTransform));
            chestAreaGo.transform.SetParent(panelGo.transform, false);
            var chestAreaRect = chestAreaGo.GetComponent<RectTransform>();
            chestAreaRect.anchorMin = new Vector2(0.52f, 0.25f);
            chestAreaRect.anchorMax = new Vector2(0.95f, 0.85f);
            chestAreaRect.sizeDelta = Vector2.zero;

            // Hào quang Sunburst phía sau
            var sunburstGo = new GameObject("Sunburst_VFX", typeof(RectTransform), typeof(Image));
            sunburstGo.transform.SetParent(chestAreaGo.transform, false);
            var sunRect = sunburstGo.GetComponent<RectTransform>();
            sunRect.anchoredPosition = new Vector2(0, 40);
            sunRect.sizeDelta = new Vector2(480, 480);
            var sunImg = sunburstGo.GetComponent<Image>();
            sunImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/UI_Gacha_Sunburst_VFX.jpg");
            sunImg.color = new Color(1f, 0.85f, 0.3f, 0.6f);

            // Rương Sprite 2D & Animator
            var chestGo = new GameObject("Chest_Sprite", typeof(RectTransform), typeof(Image), typeof(Animator));
            chestGo.transform.SetParent(chestAreaGo.transform, false);
            var cRect = chestGo.GetComponent<RectTransform>();
            cRect.anchoredPosition = new Vector2(0, 30);
            cRect.sizeDelta = new Vector2(340, 240);
            var cImg = chestGo.GetComponent<Image>();
            cImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Chest_Frame_01.png");
            var cAnim = chestGo.GetComponent<Animator>();
            cAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/UI/Gacha/Animations/Chest_AnimatorController.controller");
            cAnim.updateMode = AnimatorUpdateMode.UnscaledTime;

            // Pity Texts
            var pityLegGo = new GameObject("Legendary_Pity_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            pityLegGo.transform.SetParent(chestAreaGo.transform, false);
            var pLegRect = pityLegGo.GetComponent<RectTransform>();
            pLegRect.anchoredPosition = new Vector2(0, -110);
            pLegRect.sizeDelta = new Vector2(400, 30);
            var pLegText = pityLegGo.GetComponent<TextMeshProUGUI>();
            pLegText.alignment = TextAlignmentOptions.Center;
            pLegText.fontSize = 15;

            var pityEpicGo = new GameObject("Epic_Pity_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            pityEpicGo.transform.SetParent(chestAreaGo.transform, false);
            var pEpicRect = pityEpicGo.GetComponent<RectTransform>();
            pEpicRect.anchoredPosition = new Vector2(0, -140);
            pEpicRect.sizeDelta = new Vector2(400, 30);
            var pEpicText = pityEpicGo.GetComponent<TextMeshProUGUI>();
            pEpicText.alignment = TextAlignmentOptions.Center;
            pEpicText.fontSize = 14;

            // 4. Header Top Bar
            var topBarGo = new GameObject("Top_Bar", typeof(RectTransform));
            topBarGo.transform.SetParent(panelGo.transform, false);
            var topRect = topBarGo.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 0.88f);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.sizeDelta = Vector2.zero;

            var titleGo = new GameObject("Title_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(topBarGo.transform, false);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 0);
            titleRect.sizeDelta = new Vector2(500, 50);
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.text = "<color=#FFD700><b>✦ BẢO RƯƠNG VẠN CỔ ✦</b></color>";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;

            var coinGo = new GameObject("Currency_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            coinGo.transform.SetParent(topBarGo.transform, false);
            var coinRect = coinGo.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0.8f, 0.2f);
            coinRect.anchorMax = new Vector2(0.96f, 0.8f);
            coinRect.sizeDelta = Vector2.zero;
            var coinText = coinGo.GetComponent<TextMeshProUGUI>();
            coinText.text = "🪙 <b>12,500</b>";
            coinText.fontSize = 22;
            coinText.alignment = TextAlignmentOptions.Right;

            // 5. Nút Quay 1x và 10x Đáy Màn Hình
            var btn1Go = new GameObject("Btn_Roll_1x", typeof(RectTransform), typeof(Image), typeof(Button));
            btn1Go.transform.SetParent(panelGo.transform, false);
            var b1Rect = btn1Go.GetComponent<RectTransform>();
            b1Rect.anchoredPosition = new Vector2(-160, -460);
            b1Rect.sizeDelta = new Vector2(280, 80);
            var b1Img = btn1Go.GetComponent<Image>();
            b1Img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Btn_Gacha_Wood_Single.png");
            b1Img.type = Image.Type.Sliced;

            var b1TextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            b1TextGo.transform.SetParent(btn1Go.transform, false);
            var b1Text = b1TextGo.GetComponent<TextMeshProUGUI>();
            b1Text.text = "Quay 1x\n<color=#FFD700>100 Cổ Tiền</color>";
            b1Text.fontSize = 16;
            b1Text.alignment = TextAlignmentOptions.Center;

            var btn10Go = new GameObject("Btn_Roll_10x", typeof(RectTransform), typeof(Image), typeof(Button));
            btn10Go.transform.SetParent(panelGo.transform, false);
            var b10Rect = btn10Go.GetComponent<RectTransform>();
            b10Rect.anchoredPosition = new Vector2(160, -460);
            b10Rect.sizeDelta = new Vector2(280, 80);
            var b10Img = btn10Go.GetComponent<Image>();
            b10Img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Btn_Gacha_Red_Multi.png");
            b10Img.type = Image.Type.Sliced;

            var b10TextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            b10TextGo.transform.SetParent(btn10Go.transform, false);
            var b10Text = b10TextGo.GetComponent<TextMeshProUGUI>();
            b10Text.text = "Quay 10x\n<color=#FFD700>900 Cổ Tiền</color>";
            b10Text.fontSize = 16;
            b10Text.alignment = TextAlignmentOptions.Center;

            // 6. Result Modal Popup
            var resultPopupGo = new GameObject("Result_Popup_Panel", typeof(RectTransform), typeof(Image));
            resultPopupGo.transform.SetParent(panelGo.transform, false);
            var rRect = resultPopupGo.GetComponent<RectTransform>();
            rRect.anchorMin = Vector2.zero;
            rRect.anchorMax = Vector2.one;
            rRect.sizeDelta = Vector2.zero;
            resultPopupGo.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f, 0.95f);

            var gridGo = new GameObject("Cards_Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(resultPopupGo.transform, false);
            var gRect = gridGo.GetComponent<RectTransform>();
            gRect.anchoredPosition = new Vector2(0, 30);
            gRect.sizeDelta = new Vector2(900, 480);
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 220);
            grid.spacing = new Vector2(16, 16);
            grid.childAlignment = TextAnchor.MiddleCenter;

            var closeBtnGo = new GameObject("Btn_Close_Result", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(resultPopupGo.transform, false);
            var closeRect = closeBtnGo.GetComponent<RectTransform>();
            closeRect.anchoredPosition = new Vector2(0, -380);
            closeRect.sizeDelta = new Vector2(240, 60);
            var closeImg = closeBtnGo.GetComponent<Image>();
            closeImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Gacha/Btn_Gacha_Wood_Single.png");
            closeImg.type = Image.Type.Sliced;

            var closeTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeTextGo.transform.SetParent(closeBtnGo.transform, false);
            var closeText = closeTextGo.GetComponent<TextMeshProUGUI>();
            closeText.text = "<b>XÁC NHẬN</b>";
            closeText.fontSize = 18;
            closeText.alignment = TextAlignmentOptions.Center;

            // Gắn View và Presenter
            var view = panelGo.AddComponent<GachaChestView>();
            var presenter = panelGo.AddComponent<GachaChestPresenter>();

            var vSo = new SerializedObject(view);
            vSo.FindProperty("_currencyBalanceText").objectReferenceValue = coinText;
            vSo.FindProperty("_bannerTitleText").objectReferenceValue = titleText;
            vSo.FindProperty("_singleCostText").objectReferenceValue = b1Text;
            vSo.FindProperty("_multiCostText").objectReferenceValue = b10Text;
            vSo.FindProperty("_legendaryPityText").objectReferenceValue = pLegText;
            vSo.FindProperty("_epicPityText").objectReferenceValue = pEpicText;
            vSo.FindProperty("_singleRollButton").objectReferenceValue = btn1Go.GetComponent<Button>();
            vSo.FindProperty("_multiRollButton").objectReferenceValue = btn10Go.GetComponent<Button>();
            vSo.FindProperty("_closeResultButton").objectReferenceValue = closeBtnGo.GetComponent<Button>();
            vSo.FindProperty("_chestAnimator").objectReferenceValue = cAnim;
            vSo.FindProperty("_resultPopupPanel").objectReferenceValue = resultPopupGo;
            vSo.FindProperty("_cardsContainer").objectReferenceValue = gridGo.transform;
            vSo.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
            vSo.ApplyModifiedProperties();

            resultPopupGo.SetActive(false);

            return panelGo;
        }
    }
}
#endif

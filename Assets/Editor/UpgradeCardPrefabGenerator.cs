using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor tool tái tạo UpgradeCard_Template Prefab chuẩn Art Master DNA Đông Sơn & 9-Slice Sliced Image.
    /// </summary>
    public static class UpgradeCardPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Prefabs/UI/UpgradeCard_Template.prefab";

        [MenuItem("Tools/ProjectZombie/UI/⚡ Generate UpgradeCard Template Prefab", priority = 3)]
        public static void GenerateCardPrefab()
        {
            // 1. Tạo Root GameObject
            GameObject root = new GameObject("UpgradeCard_Template", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UpgradeCardView));
            root.layer = LayerMask.NameToLayer("UI");

            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(280, 420);

            // Sprite Card Frame Common 9-slice
            AssetDatabase.ImportAsset("Assets/Art/UI/VongXuyen/Frame_Card_Wood_9Slice.png", ImportAssetOptions.ForceUpdate);
            Sprite cardFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Wood_9Slice.png");
            if (cardFrame == null) cardFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Cards/Frame_Card_Wood_9Slice.png");
            
            Sprite iconOrbBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Weapon_Orb_Gold.png");
            Sprite cardDetailBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");

            Image rootImg = root.GetComponent<Image>();
            rootImg.color = Color.white;
            rootImg.type = Image.Type.Sliced;
            rootImg.pixelsPerUnitMultiplier = 1f;
            if (cardFrame != null) rootImg.sprite = cardFrame;

            Button rootBtn = root.GetComponent<Button>();
            rootBtn.targetGraphic = rootImg;

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // 2. Category Text (Top-Left với Padding an toàn cách mép 28px)
            GameObject catObj = new GameObject("Category_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            catObj.layer = LayerMask.NameToLayer("UI");
            catObj.transform.SetParent(root.transform, false);
            RectTransform catRT = catObj.GetComponent<RectTransform>();
            catRT.anchorMin = new Vector2(0, 1);
            catRT.anchorMax = new Vector2(0, 1);
            catRT.pivot = new Vector2(0, 1);
            catRT.anchoredPosition = new Vector2(24, -28);
            catRT.sizeDelta = new Vector2(150, 26);
            TextMeshProUGUI catTMP = catObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) catTMP.font = vietFont;
            catTMP.fontSize = 13;
            catTMP.fontStyle = FontStyles.Bold;
            catTMP.color = new Color(0.10f, 0.38f, 0.45f, 1f); // Xanh ngọc đậm
            catTMP.text = "[PHÁP BẢO]";

            // 3. Level Text (Top-Right với Padding an toàn cách mép 28px)
            GameObject lvlObj = new GameObject("Level_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            lvlObj.layer = LayerMask.NameToLayer("UI");
            lvlObj.transform.SetParent(root.transform, false);
            RectTransform lvlRT = lvlObj.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(1, 1);
            lvlRT.anchorMax = new Vector2(1, 1);
            lvlRT.pivot = new Vector2(1, 1);
            lvlRT.anchoredPosition = new Vector2(-24, -28);
            lvlRT.sizeDelta = new Vector2(90, 26);
            TextMeshProUGUI lvlTMP = lvlObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) lvlTMP.font = vietFont;
            lvlTMP.fontSize = 13;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.alignment = TextAlignmentOptions.Right;
            lvlTMP.color = new Color(0.55f, 0.30f, 0.08f, 1f); // Nâu đồng trầm
            lvlTMP.text = "Cấp 1/5";

            // 4. Icon Frame / Container (Center Upper - Kèm khung bệ đỡ Tròn Hoàng Kim)
            GameObject iconBgObj = new GameObject("Icon_Slot_Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconBgObj.layer = LayerMask.NameToLayer("UI");
            iconBgObj.transform.SetParent(root.transform, false);
            RectTransform iconBgRT = iconBgObj.GetComponent<RectTransform>();
            iconBgRT.anchorMin = new Vector2(0.5f, 1);
            iconBgRT.anchorMax = new Vector2(0.5f, 1);
            iconBgRT.pivot = new Vector2(0.5f, 1);
            iconBgRT.anchoredPosition = new Vector2(0, -62);
            iconBgRT.sizeDelta = new Vector2(96, 96);
            Image iconBgImg = iconBgObj.GetComponent<Image>();
            iconBgImg.preserveAspect = true;
            if (iconOrbBg != null) iconBgImg.sprite = iconOrbBg;

            GameObject iconObj = new GameObject("Icon_Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.layer = LayerMask.NameToLayer("UI");
            iconObj.transform.SetParent(iconBgObj.transform, false);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = new Vector2(72, 72);
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.preserveAspect = true;

            // 5. Name Text (Center Middle - Tên pháp bảo Nâu Đậm #2B1506)
            GameObject nameObj = new GameObject("Name_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameObj.layer = LayerMask.NameToLayer("UI");
            nameObj.transform.SetParent(root.transform, false);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 1);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.pivot = new Vector2(0.5f, 1);
            nameRT.anchoredPosition = new Vector2(0, -168);
            nameRT.sizeDelta = new Vector2(-40, 42);
            TextMeshProUGUI nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) nameTMP.font = vietFont;
            nameTMP.fontSize = 19;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.enableWordWrapping = true;
            nameTMP.color = new Color(0.20f, 0.10f, 0.04f, 1f); // Nâu gốm sớ đen cực rõ
            nameTMP.text = "Trống Đồng Đông Sơn";

            // 6. Description Text (Center Lower - Màu nâu mực sẫm #2A1A0F siêu nét trên nền kem)
            GameObject descObj = new GameObject("Description_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            descObj.layer = LayerMask.NameToLayer("UI");
            descObj.transform.SetParent(root.transform, false);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0.5f, 0.5f);
            descRT.offsetMin = new Vector2(24, 75);
            descRT.offsetMax = new Vector2(-24, -215);
            TextMeshProUGUI descTMP = descObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) descTMP.font = vietFont;
            descTMP.fontSize = 13.5f;
            descTMP.alignment = TextAlignmentOptions.TopLeft;
            descTMP.enableWordWrapping = true;
            descTMP.lineSpacing = 10f;
            descTMP.color = new Color(0.18f, 0.12f, 0.08f, 1f); // Nâu mực sẫm tương phản cao
            descTMP.text = "Phóng ra sóng âm ngọc lũ công kích yêu ma diện rộng.";

            // 7. Stat Diff Text (Bottom Upper)
            GameObject statObj = new GameObject("StatDiff_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            statObj.layer = LayerMask.NameToLayer("UI");
            statObj.transform.SetParent(root.transform, false);
            RectTransform statRT = statObj.GetComponent<RectTransform>();
            statRT.anchorMin = new Vector2(0, 0);
            statRT.anchorMax = new Vector2(1, 0);
            statRT.pivot = new Vector2(0.5f, 0);
            statRT.anchoredPosition = new Vector2(0, 42);
            statRT.sizeDelta = new Vector2(-40, 28);
            TextMeshProUGUI statTMP = statObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) statTMP.font = vietFont;
            statTMP.fontSize = 13;
            statTMP.fontStyle = FontStyles.Bold;
            statTMP.alignment = TextAlignmentOptions.Center;
            statTMP.enableWordWrapping = true;
            statTMP.color = new Color(0.06f, 0.38f, 0.45f, 1f); // Xanh ngọc đậm
            statTMP.text = "+25% Sát thương";

            // 8. Evolution Synergy Container & Label (Bottom Lower)
            GameObject synObj = new GameObject("Synergy_Container", typeof(RectTransform));
            synObj.layer = LayerMask.NameToLayer("UI");
            synObj.transform.SetParent(root.transform, false);
            RectTransform synRT = synObj.GetComponent<RectTransform>();
            synRT.anchorMin = new Vector2(0, 0);
            synRT.anchorMax = new Vector2(1, 0);
            synRT.pivot = new Vector2(0.5f, 0);
            synRT.anchoredPosition = new Vector2(0, 14);
            synRT.sizeDelta = new Vector2(-40, 24);

            GameObject synTextObj = new GameObject("Synergy_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            synTextObj.layer = LayerMask.NameToLayer("UI");
            synTextObj.transform.SetParent(synObj.transform, false);
            RectTransform synTextRT = synTextObj.GetComponent<RectTransform>();
            synTextRT.anchorMin = Vector2.zero;
            synTextRT.anchorMax = Vector2.one;
            synTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI synTMP = synTextObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) synTMP.font = vietFont;
            synTMP.fontSize = 12;
            synTMP.fontStyle = FontStyles.Bold;
            synTMP.alignment = TextAlignmentOptions.Center;
            synTMP.enableWordWrapping = true;
            synTMP.color = new Color(0.55f, 0.15f, 0.10f, 1f); // Đỏ chu sa trầm
            synTMP.text = "Duyên Phận: Cần P001";

            // 9. Wire SerializedFields vào UpgradeCardView
            UpgradeCardView cardView = root.GetComponent<UpgradeCardView>();
            SerializedObject so = new SerializedObject(cardView);
            so.FindProperty("_iconImage").objectReferenceValue = iconImg;
            so.FindProperty("_cardFrameImage").objectReferenceValue = rootImg;

            Sprite fWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Wood_9Slice.png");
            Sprite fJade = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Jade_9Slice.png");
            Sprite fGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Evolution_Gold_9Slice.png");
            Sprite fAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Synergy_9Slice.png");

            so.FindProperty("_frameCommonWood").objectReferenceValue = fWood;
            so.FindProperty("_frameRareJade").objectReferenceValue = fJade;
            so.FindProperty("_frameEvolutionGold").objectReferenceValue = fGold;
            so.FindProperty("_frameSynergyAmber").objectReferenceValue = fAmber;

            so.FindProperty("_nameText").objectReferenceValue = nameTMP;
            so.FindProperty("_descriptionText").objectReferenceValue = descTMP;
            so.FindProperty("_categoryText").objectReferenceValue = catTMP;
            so.FindProperty("_levelText").objectReferenceValue = lvlTMP;
            so.FindProperty("_statDiffText").objectReferenceValue = statTMP;
            so.FindProperty("_synergyContainer").objectReferenceValue = synObj;
            so.FindProperty("_synergyLabelText").objectReferenceValue = synTMP;
            so.FindProperty("_selectButton").objectReferenceValue = rootBtn;
            so.ApplyModifiedProperties();

            // 10. Lưu Prefab
            System.IO.Directory.CreateDirectory("Assets/_Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);

            Debug.Log($"<color=#FFD700>[UpgradeCardGenerator] Đã tự động tạo và wire thành công Card Template Prefab tại: {PREFAB_PATH}</color>");
        }
    }
}

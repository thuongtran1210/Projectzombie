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
            Sprite cardFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Frame_Card_Common.png");
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

            // 2. Category Text (Top-Left)
            GameObject catObj = new GameObject("Category_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            catObj.layer = LayerMask.NameToLayer("UI");
            catObj.transform.SetParent(root.transform, false);
            RectTransform catRT = catObj.GetComponent<RectTransform>();
            catRT.anchorMin = new Vector2(0, 1);
            catRT.anchorMax = new Vector2(0, 1);
            catRT.pivot = new Vector2(0, 1);
            catRT.anchoredPosition = new Vector2(24, -20);
            catRT.sizeDelta = new Vector2(130, 24);
            TextMeshProUGUI catTMP = catObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) catTMP.font = vietFont;
            catTMP.fontSize = 13;
            catTMP.fontStyle = FontStyles.Bold;
            catTMP.color = new Color(0.7f, 0.7f, 0.75f, 1f);
            catTMP.text = "Pháp Bảo";

            // 3. Level Text (Top-Right)
            GameObject lvlObj = new GameObject("Level_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            lvlObj.layer = LayerMask.NameToLayer("UI");
            lvlObj.transform.SetParent(root.transform, false);
            RectTransform lvlRT = lvlObj.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(1, 1);
            lvlRT.anchorMax = new Vector2(1, 1);
            lvlRT.pivot = new Vector2(1, 1);
            lvlRT.anchoredPosition = new Vector2(-24, -20);
            lvlRT.sizeDelta = new Vector2(80, 24);
            TextMeshProUGUI lvlTMP = lvlObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) lvlTMP.font = vietFont;
            lvlTMP.fontSize = 13;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.alignment = TextAlignmentOptions.Right;
            lvlTMP.color = new Color(1f, 0.84f, 0.3f, 1f);
            lvlTMP.text = "Lv.1/5";

            // 4. Icon Frame / Container (Center Upper)
            GameObject iconObj = new GameObject("Icon_Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.layer = LayerMask.NameToLayer("UI");
            iconObj.transform.SetParent(root.transform, false);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 1);
            iconRT.anchorMax = new Vector2(0.5f, 1);
            iconRT.pivot = new Vector2(0.5f, 1);
            iconRT.anchoredPosition = new Vector2(0, -56);
            iconRT.sizeDelta = new Vector2(110, 110);
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.preserveAspect = true;

            // 5. Name Text (Center Middle)
            GameObject nameObj = new GameObject("Name_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameObj.layer = LayerMask.NameToLayer("UI");
            nameObj.transform.SetParent(root.transform, false);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 1);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.pivot = new Vector2(0.5f, 1);
            nameRT.anchoredPosition = new Vector2(0, -178);
            nameRT.sizeDelta = new Vector2(-36, 46);
            TextMeshProUGUI nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) nameTMP.font = vietFont;
            nameTMP.fontSize = 18;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.enableWordWrapping = true;
            nameTMP.color = new Color(1f, 0.92f, 0.65f, 1f);
            nameTMP.text = "Trống Đồng Đông Sơn";

            // 6. Description Text (Center Lower)
            GameObject descObj = new GameObject("Description_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            descObj.layer = LayerMask.NameToLayer("UI");
            descObj.transform.SetParent(root.transform, false);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 1);
            descRT.pivot = new Vector2(0.5f, 0.5f);
            descRT.offsetMin = new Vector2(22, 60);
            descRT.offsetMax = new Vector2(-22, -230);
            TextMeshProUGUI descTMP = descObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) descTMP.font = vietFont;
            descTMP.fontSize = 13;
            descTMP.alignment = TextAlignmentOptions.TopLeft;
            descTMP.enableWordWrapping = true;
            descTMP.color = new Color(0.9f, 0.88f, 0.85f, 1f);
            descTMP.text = "Phóng ra sóng âm ngọc lũ công kích yêu ma diện rộng.";

            // 7. Stat Diff Text (Bottom)
            GameObject statObj = new GameObject("StatDiff_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            statObj.layer = LayerMask.NameToLayer("UI");
            statObj.transform.SetParent(root.transform, false);
            RectTransform statRT = statObj.GetComponent<RectTransform>();
            statRT.anchorMin = new Vector2(0, 0);
            statRT.anchorMax = new Vector2(1, 0);
            statRT.pivot = new Vector2(0.5f, 0);
            statRT.anchoredPosition = new Vector2(0, 18);
            statRT.sizeDelta = new Vector2(-36, 40);
            TextMeshProUGUI statTMP = statObj.GetComponent<TextMeshProUGUI>();
            if (vietFont != null) statTMP.font = vietFont;
            statTMP.fontSize = 13;
            statTMP.fontStyle = FontStyles.Bold;
            statTMP.alignment = TextAlignmentOptions.Center;
            statTMP.enableWordWrapping = true;
            statTMP.text = "<color=#4DEEEA>+25% Sát thương</color>";

            // 8. Wire SerializedFields vào UpgradeCardView
            UpgradeCardView cardView = root.GetComponent<UpgradeCardView>();
            SerializedObject so = new SerializedObject(cardView);
            so.FindProperty("_iconImage").objectReferenceValue = iconImg;
            so.FindProperty("_nameText").objectReferenceValue = nameTMP;
            so.FindProperty("_descriptionText").objectReferenceValue = descTMP;
            so.FindProperty("_categoryText").objectReferenceValue = catTMP;
            so.FindProperty("_levelText").objectReferenceValue = lvlTMP;
            so.FindProperty("_statDiffText").objectReferenceValue = statTMP;
            so.FindProperty("_selectButton").objectReferenceValue = rootBtn;
            so.ApplyModifiedProperties();

            // 9. Lưu Prefab
            System.IO.Directory.CreateDirectory("Assets/_Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);

            Debug.Log($"<color=#FFD700>[UpgradeCardGenerator] Đã tạo thành công Card Template Prefab tại: {PREFAB_PATH}</color>");
        }
    }
}

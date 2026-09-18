#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.Features.UI.Editor
{
    /// <summary>
    /// Editor Tool 1-click tự động tạo Prefab CoopReviveOverheadHUD.prefab
    /// và cho phép gắn trực tiếp vào Player Prefab đang chọn trong Unity Editor.
    /// Tuân thủ chuẩn MVP, 60 FPS Mobile và thẩm mỹ Cổ Phong Đông Sơn.
    /// </summary>
    public static class CoopRevivePrefabGenerator
    {
        private const string PREFAB_DIR = "Assets/Features/UI/Prefabs";
        private const string PREFAB_PATH = "Assets/Features/UI/Prefabs/CoopReviveOverheadHUD.prefab";
        private const string RESOURCE_DIR = "Assets/Resources/UI";
        private const string RESOURCE_PATH = "Assets/Resources/UI/CoopReviveOverheadHUD.prefab";

        [MenuItem("ProjectZombie/UI/Generate Coop Revive Overhead Prefab", priority = 30)]
        public static void GeneratePrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Features/UI"))
            {
                AssetDatabase.CreateFolder("Assets/Features", "UI");
            }
            if (!AssetDatabase.IsValidFolder(PREFAB_DIR))
            {
                AssetDatabase.CreateFolder("Assets/Features/UI", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder(RESOURCE_DIR))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "UI");
            }

            GameObject root = CreateReviveHUDHierarchy();

            // Lưu thành Prefab trong Features/UI/Prefabs
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);

            // Đồng bộ thêm vào Assets/Resources/UI để Resources.Load hoạt động trên cả Runtime APK Android
            PrefabUtility.SaveAsPrefabAsset(root, RESOURCE_PATH);

            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = prefab;
            Debug.Log($"<color=#00FF88>[Coop Revive UI]</color> Đã tạo và đồng bộ thành công Prefab tại:\n- <b>{PREFAB_PATH}</b>\n- <b>{RESOURCE_PATH}</b> (Resources.Load)");
        }

        [MenuItem("GameObject/ProjectZombie/Attach Coop Revive HUD", false, 11)]
        [MenuItem("ProjectZombie/UI/Attach Coop Revive HUD to Selected Player", priority = 31)]
        public static void AttachToSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn GameObject hoặc Prefab Player trong Hierarchy / Project để gắn Coop Revive HUD.", "OK");
                return;
            }

            // Kiểm tra xem đã có CoopReviveWorldView chưa
            var existingView = selected.GetComponentInChildren<CoopReviveWorldView>(true);
            if (existingView != null)
            {
                bool overwrite = EditorUtility.DisplayDialog("Cảnh báo", $"{selected.name} đã có CoopReviveWorldView. Bạn có muốn thay thế không?", "Thay thế", "Hủy");
                if (!overwrite) return;
                Undo.DestroyObjectImmediate(existingView.gameObject);
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Attach Coop Revive HUD");
            int group = Undo.GetCurrentGroup();

            GameObject hudObj = CreateReviveHUDHierarchy();
            hudObj.name = "CoopReviveOverheadHUD";
            hudObj.transform.SetParent(selected.transform, false);
            hudObj.transform.localPosition = new Vector3(0f, 1.85f, 0f);

            Undo.RegisterCreatedObjectUndo(hudObj, "Create Coop Revive HUD");
            Undo.CollapseUndoOperations(group);

            Selection.activeGameObject = hudObj;
            Debug.Log($"<color=#00FF88>[Coop Revive UI]</color> Đã gắn thành công Coop Revive HUD vào: <b>{selected.name}</b>");
        }

        public static GameObject CreateReviveHUDHierarchy()
        {
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null)
            {
                vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            }

            Sprite hpFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_HP.png");
            Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (defaultSprite == null) defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            // 1. Root GameObject
            GameObject root = new GameObject("CoopReviveOverheadHUD");

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = 120;

            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0f; // Mặc định ẩn khi chưa gục ngã

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(280f, 75f);
            rootRect.localScale = new Vector3(0.012f, 0.012f, 1f);

            // 2. Badge Root Container
            GameObject badgeRoot = new GameObject("ReviveBadgeRoot");
            badgeRoot.transform.SetParent(root.transform, false);
            RectTransform badgeRt = badgeRoot.AddComponent<RectTransform>();
            badgeRt.localPosition = Vector3.zero;
            badgeRt.sizeDelta = new Vector2(280f, 75f);
            badgeRt.localScale = Vector3.one;

            // 3. Status Label (SOS Title)
            GameObject statusLabelObj = new GameObject("StatusLabel");
            statusLabelObj.transform.SetParent(badgeRoot.transform, false);
            RectTransform statusRt = statusLabelObj.AddComponent<RectTransform>();
            statusRt.anchoredPosition = new Vector2(0f, 16f);
            statusRt.sizeDelta = new Vector2(280f, 32f);

            TextMeshProUGUI statusTmp = statusLabelObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) statusTmp.font = vietFont;
            statusTmp.text = "<color=#FF4444><b>[CẦN CỨU VIỆN!]</b></color>";
            statusTmp.fontSize = 24f;
            statusTmp.fontStyle = FontStyles.Bold;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.richText = true;
            statusTmp.raycastTarget = false;

            // 4. Prompt Sub Label (Hướng dẫn đứng gần để cứu)
            GameObject promptLabelObj = new GameObject("PromptSubLabel");
            promptLabelObj.transform.SetParent(badgeRoot.transform, false);
            RectTransform promptRt = promptLabelObj.AddComponent<RectTransform>();
            promptRt.anchoredPosition = new Vector2(0f, -8f);
            promptRt.sizeDelta = new Vector2(280f, 24f);

            TextMeshProUGUI promptTmp = promptLabelObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) promptTmp.font = vietFont;
            promptTmp.text = "<color=#FFD700>Đứng gần để Cứu</color>";
            promptTmp.fontSize = 18f;
            promptTmp.alignment = TextAlignmentOptions.Center;
            promptTmp.richText = true;
            promptTmp.raycastTarget = false;

            // 5. Revive Progress Slider (Thanh nạp hồi sinh)
            GameObject sliderObj = new GameObject("ReviveSlider");
            sliderObj.transform.SetParent(badgeRoot.transform, false);
            RectTransform sliderRt = sliderObj.AddComponent<RectTransform>();
            sliderRt.anchoredPosition = new Vector2(0f, -22f);
            sliderRt.sizeDelta = new Vector2(220f, 10f);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;

            // Slider Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = defaultSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
            bgImg.raycastTarget = false;

            // Slider Fill Area
            GameObject fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = new Vector2(-4f, -4f);
            fillAreaRt.anchoredPosition = Vector2.zero;

            // Slider Fill Image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.sprite = hpFillSprite != null ? hpFillSprite : defaultSprite;
            fillImg.color = new Color(0f, 1f, 0.55f, 1f); // Màu xanh ngọc hồi sinh
            fillImg.raycastTarget = false;

            slider.fillRect = fillRt;
            sliderObj.SetActive(false); // Ẩn khi chưa có ai cứu

            // 6. Gắn View & Presenter Components
            CoopReviveWorldView view = root.AddComponent<CoopReviveWorldView>();
            CoopReviveHUDPresenter presenter = root.AddComponent<CoopReviveHUDPresenter>();

            // 7. Gán Serialized Fields
            SerializedObject viewSO = new SerializedObject(view);
            viewSO.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            viewSO.FindProperty("_badgeRoot").objectReferenceValue = badgeRt;
            viewSO.FindProperty("_reviveProgressBar").objectReferenceValue = slider;
            viewSO.FindProperty("_progressBarFill").objectReferenceValue = fillImg;
            viewSO.FindProperty("_statusLabel").objectReferenceValue = statusTmp;
            viewSO.FindProperty("_promptSubLabel").objectReferenceValue = promptTmp;
            viewSO.ApplyModifiedProperties();

            SerializedObject presenterSO = new SerializedObject(presenter);
            presenterSO.FindProperty("_view").objectReferenceValue = view;
            presenterSO.ApplyModifiedProperties();

            return root;
        }
    }
}
#endif

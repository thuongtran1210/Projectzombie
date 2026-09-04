#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.Overhead;

namespace ProjectZombie.Features.UI.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo Prefab Overhead Status UI (Thanh Máu & Level World-Space)
    /// hoặc gắn trực tiếp vào GameObject / Prefab đang chọn chỉ với 1 click.
    /// </summary>
    public static class OverheadStatusPrefabGenerator
    {
        private const string PREFAB_DIR = "Assets/Features/UI/Prefabs";
        private const string PREFAB_PATH = "Assets/Features/UI/Prefabs/Overhead_Status_Bar.prefab";

        [MenuItem("ProjectZombie/UI/Generate Overhead Status Prefab", priority = 20)]
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

            GameObject root = CreateOverheadHierarchy();

            // Lưu thành Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = prefab;
            Debug.Log($"<color=green>[Overhead UI]</color> Đã tạo thành công Prefab tại: {PREFAB_PATH}");
        }

        [MenuItem("GameObject/ProjectZombie/Attach Overhead Status Bar", false, 10)]
        [MenuItem("ProjectZombie/UI/Attach Overhead Status to Selected Object", priority = 21)]
        public static void AttachToSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn GameObject (Player hoặc Enemy) trong Hierarchy / Project để gắn Overhead Status Bar.", "OK");
                return;
            }

            // Kiểm tra xem đã có OverheadStatusView chưa
            var existingView = selected.GetComponentInChildren<OverheadStatusView>(true);
            if (existingView != null)
            {
                bool overwrite = EditorUtility.DisplayDialog("Cảnh báo", $"{selected.name} đã có OverheadStatusView. Bạn có muốn thay thế cái cũ không?", "Thay thế", "Hủy");
                if (!overwrite) return;
                Undo.DestroyObjectImmediate(existingView.gameObject);
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Attach Overhead Status Bar");
            int group = Undo.GetCurrentGroup();

            GameObject overheadObj = CreateOverheadHierarchy();
            overheadObj.name = "Overhead_Status_Bar";
            overheadObj.transform.SetParent(selected.transform, false);
            overheadObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            Undo.RegisterCreatedObjectUndo(overheadObj, "Create Overhead Status Bar");
            Undo.CollapseUndoOperations(group);

            Selection.activeGameObject = overheadObj;
            Debug.Log($"<color=green>[Overhead UI]</color> Đã gắn thành công Overhead Status Bar vào: {selected.name}");
        }

        [MenuItem("ProjectZombie/UI/Clean Old Screen HUD (Hide Old HP & EXP)", priority = 22)]
        public static void CleanOldHUD()
        {
            var hudView = Object.FindObjectOfType<HUD.RunHUDView>(true);
            if (hudView == null)
            {
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy RunHUDView trong scene hiện tại.", "OK");
                return;
            }

            SerializedObject so = new SerializedObject(hudView);
            var hpSliderProp = so.FindProperty("_hpSlider");
            var expSliderProp = so.FindProperty("_expSlider");
            var levelTextProp = so.FindProperty("_levelText");

            int disabledCount = 0;
            if (hpSliderProp != null && hpSliderProp.objectReferenceValue != null)
            {
                var hpObj = ((Slider)hpSliderProp.objectReferenceValue).gameObject;
                Undo.RecordObject(hpObj, "Disable Old HP Bar");
                hpObj.SetActive(false);
                disabledCount++;
            }

            if (expSliderProp != null && expSliderProp.objectReferenceValue != null)
            {
                var expObj = ((Slider)expSliderProp.objectReferenceValue).gameObject;
                Undo.RecordObject(expObj, "Disable Old EXP Bar");
                expObj.SetActive(false);
                disabledCount++;
            }

            if (levelTextProp != null && levelTextProp.objectReferenceValue != null)
            {
                var lvlObj = ((TMP_Text)levelTextProp.objectReferenceValue).gameObject;
                Undo.RecordObject(lvlObj, "Disable Old Level Text");
                lvlObj.SetActive(false);
                disabledCount++;
            }

            EditorUtility.SetDirty(hudView.gameObject);
            Debug.Log($"<color=green>[HUD Cleanup]</color> Đã tự động ẩn {disabledCount} phần tử HUD HP/EXP/Level cũ trong RunHUDView để chuyển trọn vẹn sang Overhead UI!");
            EditorUtility.DisplayDialog("Thành công", $"Đã tự động ẩn các thanh HP/EXP/Level cũ trên màn hình HUD ({disabledCount} objects).", "OK");
        }

        private static GameObject CreateOverheadHierarchy()
        {
            // 1. Root World-Space Canvas
            GameObject root = new GameObject("Overhead_Status_Bar");
            
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50; // Đảm bảo nổi trên các sprite nhân vật
            
            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(100f, 24f);
            rootRect.localScale = new Vector3(0.01f, 0.01f, 1f); // Chuẩn tỉ lệ World Space

            // Lấy sprite trắng mặc định của Unity UI (Image Type Filled yêu cầu phải có sprite)
            Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (defaultSprite == null)
            {
                defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            }

            // 2. Health Bar Background Container
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(root.transform, false);
            RectTransform hbRect = healthBarObj.AddComponent<RectTransform>();
            hbRect.anchorMin = new Vector2(0.28f, 0.15f);
            hbRect.anchorMax = new Vector2(1f, 0.85f);
            hbRect.offsetMin = Vector2.zero;
            hbRect.offsetMax = Vector2.zero;

            Image bgImage = healthBarObj.AddComponent<Image>();
            bgImage.sprite = defaultSprite;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.9f); // Dark Charcoal Background

            // 3. Delay Health Fill (Vệt máu trễ / White damage)
            GameObject delayFillObj = new GameObject("DelayFill");
            delayFillObj.transform.SetParent(healthBarObj.transform, false);
            RectTransform delayRect = delayFillObj.AddComponent<RectTransform>();
            delayRect.anchorMin = Vector2.zero;
            delayRect.anchorMax = Vector2.one;
            delayRect.offsetMin = Vector2.zero;
            delayRect.offsetMax = Vector2.zero;

            Image delayImage = delayFillObj.AddComponent<Image>();
            delayImage.sprite = defaultSprite;
            delayImage.color = new Color(1f, 0.85f, 0.3f, 0.9f); // Gold/White warning
            delayImage.type = Image.Type.Filled;
            delayImage.fillMethod = Image.FillMethod.Horizontal;
            delayImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            delayImage.fillAmount = 1f;

            // 4. Primary Health Fill (Thanh máu chính)
            GameObject healthFillObj = new GameObject("HealthFill");
            healthFillObj.transform.SetParent(healthBarObj.transform, false);
            RectTransform fillRect = healthFillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image healthImage = healthFillObj.AddComponent<Image>();
            healthImage.sprite = defaultSprite;
            healthImage.color = new Color(0.2f, 0.85f, 0.3f, 1f); // Vibrant Emerald Green
            healthImage.type = Image.Type.Filled;
            healthImage.fillMethod = Image.FillMethod.Horizontal;
            healthImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthImage.fillAmount = 1f;

            // 5. Level Badge Root (Badge hình vuông / tròn chứa level và viền EXP Ring)
            GameObject levelBadgeObj = new GameObject("LevelBadge");
            levelBadgeObj.transform.SetParent(root.transform, false);
            RectTransform badgeRect = levelBadgeObj.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0f, 0f);
            badgeRect.anchorMax = new Vector2(0.25f, 1f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;

            // Background Badge nền tối
            Image badgeBg = levelBadgeObj.AddComponent<Image>();
            badgeBg.sprite = defaultSprite;
            badgeBg.type = Image.Type.Sliced;
            badgeBg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

            // 5.1 EXP Ring (Vòng tròn bo quanh viền Level Badge)
            GameObject expRingObj = new GameObject("ExpRing");
            expRingObj.transform.SetParent(levelBadgeObj.transform, false);
            RectTransform expRingRect = expRingObj.AddComponent<RectTransform>();
            expRingRect.anchorMin = Vector2.zero;
            expRingRect.anchorMax = Vector2.one;
            expRingRect.offsetMin = Vector2.zero;
            expRingRect.offsetMax = Vector2.zero;

            Image expRingImage = expRingObj.AddComponent<Image>();
            expRingImage.sprite = defaultSprite;
            expRingImage.color = new Color(0.3f, 0.65f, 1f, 1f); // Vibrant Arcane Cyan / Sky Blue
            expRingImage.type = Image.Type.Filled;
            expRingImage.fillMethod = Image.FillMethod.Radial360;
            expRingImage.fillOrigin = (int)Image.Origin360.Top;
            expRingImage.fillClockwise = true;
            expRingImage.fillAmount = 0f;

            // 6. Level Text (TextMeshPro)
            GameObject levelTextObj = new GameObject("LevelText");
            levelTextObj.transform.SetParent(levelBadgeObj.transform, false);
            RectTransform textRect = levelTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmpText = levelTextObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "1";
            tmpText.fontSize = 12f;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            tmpText.enableAutoSizing = false;

            // 7. Gắn View & Presenter Components và liên kết Inspector fields
            OverheadStatusView view = root.AddComponent<OverheadStatusView>();
            OverheadStatusPresenter presenter = root.AddComponent<OverheadStatusPresenter>();

            // Gán Serialized Fields thông qua SerializedObject để đảm bảo lưu đúng trong Prefab
            SerializedObject viewSO = new SerializedObject(view);
            viewSO.FindProperty("_healthFillImage").objectReferenceValue = healthImage;
            viewSO.FindProperty("_healthDelayFillImage").objectReferenceValue = delayImage;
            viewSO.FindProperty("_levelBadgeRoot").objectReferenceValue = levelBadgeObj;
            viewSO.FindProperty("_levelText").objectReferenceValue = tmpText;
            viewSO.FindProperty("_expRingFillImage").objectReferenceValue = expRingImage;
            viewSO.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            viewSO.FindProperty("_keepUpright").boolValue = true;
            viewSO.ApplyModifiedProperties();

            SerializedObject presenterSO = new SerializedObject(presenter);
            presenterSO.FindProperty("_view").objectReferenceValue = view;
            presenterSO.ApplyModifiedProperties();

            return root;
        }
    }
}
#endif

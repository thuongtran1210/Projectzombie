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
            // Load trọn bộ Sprite Vọng Xuyên Cổ Phong từ Bars_Container
            Sprite hpExpFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Frame_VongXuyen_9Slice.png");
            Sprite hpFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_HP.png");
            Sprite expFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_EXP.png");
            Sprite levelOrbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_HUD_Player_Orb_Level.png");
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");

            // Lấy sprite trắng dự phòng nếu asset chưa có
            Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (defaultSprite == null) defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            // 1. Root World-Space Canvas
            GameObject root = new GameObject("Overhead_Status_Bar");
            
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "UI_World"; // Đúng Sorting Layer cao nhất trong Project
            canvas.sortingOrder = 100; // Đảm bảo nổi tuyệt đối trên tất cả Entities, Skill và Tilemap
            
            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(160f, 36f);
            rootRect.localScale = new Vector3(0.01f, 0.01f, 1f); // Chuẩn tỉ lệ World Space

            // 2. Health Bar Background Container (Khung Gỗ Vát Đuôi Vọng Xuyên)
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(root.transform, false);
            RectTransform hbRect = healthBarObj.AddComponent<RectTransform>();
            hbRect.anchorMin = new Vector2(0.24f, 0.15f);
            hbRect.anchorMax = new Vector2(1f, 0.85f);
            hbRect.offsetMin = Vector2.zero;
            hbRect.offsetMax = Vector2.zero;

            Image bgImage = healthBarObj.AddComponent<Image>();
            bgImage.sprite = hpExpFrameSprite != null ? hpExpFrameSprite : defaultSprite;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Color.white;
            bgImage.raycastTarget = false;

            // 3. Delay Health Fill (Vệt máu trễ / White damage)
            GameObject delayFillObj = new GameObject("DelayFill");
            delayFillObj.transform.SetParent(healthBarObj.transform, false);
            RectTransform delayRect = delayFillObj.AddComponent<RectTransform>();
            delayRect.anchorMin = Vector2.zero;
            delayRect.anchorMax = Vector2.one;
            delayRect.offsetMin = new Vector2(3f, 3f);
            delayRect.offsetMax = new Vector2(-6f, -3f);

            Image delayImage = delayFillObj.AddComponent<Image>();
            delayImage.sprite = hpFillSprite != null ? hpFillSprite : defaultSprite;
            delayImage.color = new Color(1f, 0.85f, 0.3f, 0.85f); // Gold warning
            delayImage.type = Image.Type.Filled;
            delayImage.fillMethod = Image.FillMethod.Horizontal;
            delayImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            delayImage.fillAmount = 1f;
            delayImage.raycastTarget = false;

            // 4. Primary Health Fill (Thanh máu đỏ chu sa / huyết ngọc Cổ Phong)
            GameObject healthFillObj = new GameObject("HealthFill");
            healthFillObj.transform.SetParent(healthBarObj.transform, false);
            RectTransform fillRect = healthFillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-6f, -3f);

            Image healthImage = healthFillObj.AddComponent<Image>();
            healthImage.sprite = hpFillSprite != null ? hpFillSprite : defaultSprite;
            healthImage.color = Color.white;
            healthImage.type = Image.Type.Filled;
            healthImage.fillMethod = Image.FillMethod.Horizontal;
            healthImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthImage.fillAmount = 1f;
            healthImage.raycastTarget = false;

            // 5. Level Badge Root (Huy hiệu Ngọc Bát Quái Cổ Phong Vọng Xuyên)
            GameObject levelBadgeObj = new GameObject("LevelBadge");
            levelBadgeObj.transform.SetParent(root.transform, false);
            RectTransform badgeRect = levelBadgeObj.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0f, 0f);
            badgeRect.anchorMax = new Vector2(0.24f, 1f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;

            Image badgeBg = levelBadgeObj.AddComponent<Image>();
            badgeBg.sprite = levelOrbSprite != null ? levelOrbSprite : defaultSprite;
            badgeBg.preserveAspect = true;
            badgeBg.color = Color.white;
            badgeBg.raycastTarget = false;

            // 5.1 EXP Ring (Vòng tròn hoàng kim EXP bao quanh huy hiệu)
            GameObject expRingObj = new GameObject("ExpRing");
            expRingObj.transform.SetParent(levelBadgeObj.transform, false);
            RectTransform expRingRect = expRingObj.AddComponent<RectTransform>();
            expRingRect.anchorMin = Vector2.zero;
            expRingRect.anchorMax = Vector2.one;
            expRingRect.offsetMin = new Vector2(2f, 2f);
            expRingRect.offsetMax = new Vector2(-2f, -2f);

            Image expRingImage = expRingObj.AddComponent<Image>();
            expRingImage.sprite = expFillSprite != null ? expFillSprite : defaultSprite;
            expRingImage.color = new Color(0.35f, 0.75f, 1f, 0.95f); // Arcane Cyan
            expRingImage.type = Image.Type.Filled;
            expRingImage.fillMethod = Image.FillMethod.Radial360;
            expRingImage.fillOrigin = (int)Image.Origin360.Top;
            expRingImage.fillClockwise = true;
            expRingImage.fillAmount = 0f;
            expRingImage.raycastTarget = false;

            // 6. Level Text (TextMeshPro Cổ Phong)
            GameObject levelTextObj = new GameObject("LevelText");
            levelTextObj.transform.SetParent(levelBadgeObj.transform, false);
            RectTransform textRect = levelTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmpText = levelTextObj.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) tmpText.font = vietFont;
            tmpText.text = "1";
            tmpText.fontSize = 13f;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = new Color(0.98f, 0.92f, 0.76f); // Vàng kim cổ phong
            tmpText.enableAutoSizing = false;
            tmpText.raycastTarget = false;

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

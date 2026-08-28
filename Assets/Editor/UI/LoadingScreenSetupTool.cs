#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using UnityEditor.SceneManagement;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Editor Tool 1-Click tự động tạo Prefab LoadingScreenUI và cài đặt vào Scene đang mở.
    /// Menu: ProjectZombie > UI > Setup Loading Screen UI in Scene
    /// </summary>
    public static class LoadingScreenSetupTool
    {
        private const string PREFAB_PATH = "Assets/_Prefabs/UI/LoadingScreenUI.prefab";

        [MenuItem("ProjectZombie/UI/Setup Loading Screen UI in Scene", priority = 10)]
        [MenuItem("Tools/ProjectZombie/Setup Loading Screen UI", priority = 10)]
        public static void SetupLoadingScreen()
        {
            // 1. Tạo Prefab LoadingScreenUI
            GameObject loadingPrefab = CreateOrUpdateLoadingPrefab();

            // 2. Cài đặt vào Scene hiện tại
            InstantiateOrUpdateInScene(loadingPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LoadingScreenSetupTool] Đã khởi tạo và cài đặt thành công Giao diện Loading Screen vào Scene!");
        }

        public static GameObject CreateOrUpdateLoadingPrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs")) AssetDatabase.CreateFolder("Assets", "_Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs/UI")) AssetDatabase.CreateFolder("Assets/_Prefabs", "UI");

            // Nạp sprites
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png");
            Sprite frameBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Frame_VongXuyen_9Slice.png");
            Sprite fillBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Bar_HUD_Fill_EXP.png");
            Sprite spinnerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Weapon_Orb_Gold.png") 
                                ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Panel_YinYang_Meter_HUD.png");
            Sprite tipBannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png")
                                  ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Banner_Upgrade_Parchment.png");

            // Root
            GameObject root = new GameObject("LoadingScreenUI", typeof(RectTransform), typeof(CanvasGroup), typeof(LoadingScreenView), typeof(LoadingScreenPresenter));
            var rootRect = root.GetComponent<RectTransform>();
            StretchRect(rootRect);

            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            var view = root.GetComponent<LoadingScreenView>();
            var presenter = root.GetComponent<LoadingScreenPresenter>();

            // 1. Background Image
            GameObject bgObj = new GameObject("Background_DarkAtmosphere", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(root.transform, false);
            StretchRect(bgObj.GetComponent<RectTransform>());
            var bgImg = bgObj.GetComponent<Image>();
            if (bgSprite != null)
            {
                bgImg.sprite = bgSprite;
                bgImg.color = new Color(0.45f, 0.45f, 0.55f, 1f); // Tối màu cõi âm
            }
            else
            {
                bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            }

            // 2. Animated Spinner (Bát Quái / Trống Đồng)
            GameObject spinnerObj = new GameObject("Spinner_YinYang", typeof(RectTransform), typeof(Image));
            spinnerObj.transform.SetParent(root.transform, false);
            var spinRect = spinnerObj.GetComponent<RectTransform>();
            spinRect.anchorMin = new Vector2(0.5f, 0.38f);
            spinRect.anchorMax = new Vector2(0.5f, 0.38f);
            spinRect.pivot = new Vector2(0.5f, 0.5f);
            spinRect.anchoredPosition = Vector2.zero;
            spinRect.sizeDelta = new Vector2(90, 90);
            var spinImg = spinnerObj.GetComponent<Image>();
            if (spinnerSprite != null) spinImg.sprite = spinnerSprite;
            spinImg.color = new Color(1f, 0.85f, 0.35f, 1f);

            // 3. Status Message Text
            GameObject statusObj = new GameObject("Txt_StatusMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusObj.transform.SetParent(root.transform, false);
            var statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.28f);
            statusRect.anchorMax = new Vector2(0.5f, 0.28f);
            statusRect.pivot = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.sizeDelta = new Vector2(800, 40);
            var statusTmp = statusObj.GetComponent<TextMeshProUGUI>();
            statusTmp.text = "Đang khai mở cửa Hoàng Tuyền...";
            statusTmp.fontSize = 22;
            statusTmp.fontStyle = FontStyles.Bold;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = new Color(1f, 0.95f, 0.8f, 1f);

            // 4. Progress Bar Container
            GameObject barFrameObj = new GameObject("ProgressBar_Frame", typeof(RectTransform), typeof(Image));
            barFrameObj.transform.SetParent(root.transform, false);
            var barFrameRect = barFrameObj.GetComponent<RectTransform>();
            barFrameRect.anchorMin = new Vector2(0.5f, 0.22f);
            barFrameRect.anchorMax = new Vector2(0.5f, 0.22f);
            barFrameRect.pivot = new Vector2(0.5f, 0.5f);
            barFrameRect.anchoredPosition = Vector2.zero;
            barFrameRect.sizeDelta = new Vector2(750, 36);
            var barFrameImg = barFrameObj.GetComponent<Image>();
            if (frameBarSprite != null)
            {
                barFrameImg.sprite = frameBarSprite;
                barFrameImg.type = Image.Type.Sliced;
            }
            barFrameImg.color = Color.white;

            // 4.1. Progress Bar Fill
            GameObject barFillObj = new GameObject("ProgressBar_Fill", typeof(RectTransform), typeof(Image));
            barFillObj.transform.SetParent(barFrameObj.transform, false);
            var barFillRect = barFillObj.GetComponent<RectTransform>();
            barFillRect.anchorMin = new Vector2(0f, 0f);
            barFillRect.anchorMax = new Vector2(1f, 1f);
            barFillRect.pivot = new Vector2(0f, 0.5f);
            barFillRect.offsetMin = new Vector2(6, 6);
            barFillRect.offsetMax = new Vector2(-6, -6);
            var barFillImg = barFillObj.GetComponent<Image>();
            if (fillBarSprite != null) barFillImg.sprite = fillBarSprite;
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillAmount = 0.65f;
            barFillImg.color = new Color(0.3f, 0.9f, 0.85f, 1f); // Lam ngọc phát sáng

            // 4.2. Percentage Text
            GameObject percentObj = new GameObject("Txt_Percent", typeof(RectTransform), typeof(TextMeshProUGUI));
            percentObj.transform.SetParent(barFrameObj.transform, false);
            StretchRect(percentObj.GetComponent<RectTransform>());
            var percentTmp = percentObj.GetComponent<TextMeshProUGUI>();
            percentTmp.text = "65%";
            percentTmp.fontSize = 18;
            percentTmp.fontStyle = FontStyles.Bold;
            percentTmp.alignment = TextAlignmentOptions.Center;
            percentTmp.color = Color.white;

            // 5. Tips Box
            GameObject tipBoxObj = new GameObject("Tip_Box", typeof(RectTransform), typeof(Image));
            tipBoxObj.transform.SetParent(root.transform, false);
            var tipBoxRect = tipBoxObj.GetComponent<RectTransform>();
            tipBoxRect.anchorMin = new Vector2(0.5f, 0.10f);
            tipBoxRect.anchorMax = new Vector2(0.5f, 0.10f);
            tipBoxRect.pivot = new Vector2(0.5f, 0.5f);
            tipBoxRect.anchoredPosition = Vector2.zero;
            tipBoxRect.sizeDelta = new Vector2(900, 75);
            var tipBoxImg = tipBoxObj.GetComponent<Image>();
            if (tipBannerSprite != null)
            {
                tipBoxImg.sprite = tipBannerSprite;
                tipBoxImg.type = Image.Type.Sliced;
                tipBoxImg.color = new Color(1f, 1f, 1f, 0.92f);
            }
            else
            {
                tipBoxImg.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
            }

            // Tip Title Text
            GameObject tipTitleObj = new GameObject("Txt_TipTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            tipTitleObj.transform.SetParent(tipBoxObj.transform, false);
            var tipTitleRect = tipTitleObj.GetComponent<RectTransform>();
            tipTitleRect.anchorMin = new Vector2(0f, 0.55f);
            tipTitleRect.anchorMax = new Vector2(1f, 1f);
            tipTitleRect.pivot = new Vector2(0.5f, 0.5f);
            tipTitleRect.anchoredPosition = Vector2.zero;
            tipTitleRect.sizeDelta = Vector2.zero;
            var tipTitleTmp = tipTitleObj.GetComponent<TextMeshProUGUI>();
            // Nạp font tiếng Việt
            TMP_FontAsset vietnameseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset")
                                        ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/GameFont_Vietnamese_SD.asset")
                                        ?? TMP_Settings.defaultFontAsset;

            if (vietnameseFont != null)
            {
                statusTmp.font = vietnameseFont;
                percentTmp.font = vietnameseFont;
            }

            tipTitleTmp.text = "[ BÍ KÍP CÕI ÂM ]";
            tipTitleTmp.fontSize = 17;
            tipTitleTmp.fontStyle = FontStyles.Bold;
            tipTitleTmp.alignment = TextAlignmentOptions.Center;
            tipTitleTmp.color = new Color(1f, 0.84f, 0f, 1f); // Gold
            if (vietnameseFont != null) tipTitleTmp.font = vietnameseFont;

            // Tip Body Text
            GameObject tipBodyObj = new GameObject("Txt_TipBody", typeof(RectTransform), typeof(TextMeshProUGUI));
            tipBodyObj.transform.SetParent(tipBoxObj.transform, false);
            var tipBodyRect = tipBodyObj.GetComponent<RectTransform>();
            tipBodyRect.anchorMin = new Vector2(0f, 0f);
            tipBodyRect.anchorMax = new Vector2(1f, 0.55f);
            tipBodyRect.pivot = new Vector2(0.5f, 0.5f);
            tipBodyRect.anchoredPosition = Vector2.zero;
            tipBodyRect.sizeDelta = Vector2.zero;
            var tipBodyTmp = tipBodyObj.GetComponent<TextMeshProUGUI>();
            tipBodyTmp.text = "Đánh trúng hệ tương khắc gây thêm +30% Sát thương và kích hoạt hiệu ứng suy yếu!";
            tipBodyTmp.fontSize = 15;
            tipBodyTmp.alignment = TextAlignmentOptions.Center;
            tipBodyTmp.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            if (vietnameseFont != null) tipBodyTmp.font = vietnameseFont;

            // Wire Serialized Properties on View
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("_canvasGroup").objectReferenceValue = cg;
            soView.FindProperty("_panelRoot").objectReferenceValue = root;
            soView.FindProperty("_progressBarFill").objectReferenceValue = barFillImg;
            soView.FindProperty("_progressPercentText").objectReferenceValue = percentTmp;
            soView.FindProperty("_statusMessageText").objectReferenceValue = statusTmp;
            soView.FindProperty("_tipTitleText").objectReferenceValue = tipTitleTmp;
            soView.FindProperty("_tipBodyText").objectReferenceValue = tipBodyTmp;
            soView.FindProperty("_yinYangSpinner").objectReferenceValue = spinRect;
            soView.ApplyModifiedProperties();

            // Wire Serialized Properties on Presenter
            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            // Save as Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static void InstantiateOrUpdateInScene(GameObject prefab)
        {
            if (prefab == null) return;

            // Tìm Canvas_Master trong Scene
            Canvas masterCanvas = null;
            var masterObj = GameObject.Find("Canvas_Master");
            if (masterObj != null) masterCanvas = masterObj.GetComponent<Canvas>();

            if (masterCanvas == null)
            {
                masterCanvas = Object.FindObjectOfType<Canvas>();
            }

            if (masterCanvas == null)
            {
                Debug.LogWarning("[LoadingScreenSetupTool] Không tìm thấy Canvas trong Scene. Hãy chạy FullGameUISetupTool trước!");
                return;
            }

            // Kiểm tra xem đã có LoadingScreenUI trong Canvas chưa
            Transform existing = masterCanvas.transform.Find("LoadingScreenUI");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, masterCanvas.transform);
            instance.name = "LoadingScreenUI";
            instance.transform.SetAsLastSibling(); // Đặt trên cùng để đè lên mọi UI khác khi nạp

            // Đăng ký Undo và báo Scene thay đổi
            Undo.RegisterCreatedObjectUndo(instance, "Create LoadingScreenUI in Scene");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[LoadingScreenSetupTool] Đã gắn LoadingScreenUI vào Canvas_Master trên Scene thành công!");
        }

        private static void StretchRect(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif

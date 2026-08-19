using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor tool tự động tối ưu hóa và cấu trúc lại UpgradeUI_Root trong Scene chuẩn MVP.
    /// </summary>
    public static class UpgradeUIHierarchyOptimizer
    {
        [MenuItem("Tools/ProjectZombie/UI/Optimize UpgradeUI Hierarchy")]
        public static void OptimizeUpgradeUI()
        {
            GameObject rootObj = GameObject.Find("UpgradeUI_Root");
            if (rootObj == null)
            {
                Debug.LogError("[UpgradeUIOptimizer] Không tìm thấy GameObject 'UpgradeUI_Root' trong Scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(rootObj, "Optimize UpgradeUI Hierarchy");

            // 1. Lấy hoặc gắn UpgradeUIView & UpgradeUIPresenter trên Root
            UpgradeUIView uiView = rootObj.GetComponent<UpgradeUIView>();
            if (uiView == null) uiView = rootObj.AddComponent<UpgradeUIView>();

            UpgradeUIPresenter presenter = rootObj.GetComponent<UpgradeUIPresenter>();
            if (presenter == null) presenter = rootObj.AddComponent<UpgradeUIPresenter>();

            // 2. Tìm hoặc chuẩn hóa Upgrade_Panel (Con của UpgradeUI_Root)
            Transform panelTrans = rootObj.transform.Find("Upgrade_Panel");
            if (panelTrans == null && rootObj.transform.childCount > 0)
            {
                panelTrans = rootObj.transform.GetChild(0);
                panelTrans.name = "Upgrade_Panel";
            }

            if (panelTrans == null)
            {
                GameObject newPanel = new GameObject("Upgrade_Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                newPanel.transform.SetParent(rootObj.transform, false);
                panelTrans = newPanel.transform;
            }

            // Cấu hình Stretch cho Upgrade_Panel
            RectTransform panelRect = panelTrans.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.sizeDelta = new Vector2(1100, 650);
                panelRect.anchoredPosition = Vector2.zero;
            }

            Image panelBg = panelTrans.GetComponent<Image>();
            if (panelBg != null)
            {
                panelBg.color = new Color(0.05f, 0.08f, 0.12f, 0.85f); // Tối nền cổ phong
            }

            // 3. Chuẩn hóa Cards_Container (Con của Upgrade_Panel)
            Transform cardsContainerTrans = panelTrans.Find("Cards_Container");
            if (cardsContainerTrans == null)
            {
                // Thử tìm object con cũ có gắn Layout
                Transform oldContainer = panelTrans.Find("Upgrade_Panel"); // trường hợp tên trùng lặp cũ
                if (oldContainer != null)
                {
                    oldContainer.name = "Cards_Container";
                    cardsContainerTrans = oldContainer;
                }
                else
                {
                    GameObject newContainer = new GameObject("Cards_Container", typeof(RectTransform));
                    newContainer.transform.SetParent(panelTrans, false);
                    cardsContainerTrans = newContainer.transform;
                }
            }

            RectTransform containerRect = cardsContainerTrans.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.sizeDelta = new Vector2(1000, 480);
                containerRect.anchoredPosition = new Vector2(0, 30);
            }

            // Đảm bảo có HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = cardsContainerTrans.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                // Nếu đang có GridLayoutGroup cũ thì xóa bớt để chuyển sang Horizontal sạch sẽ
                GridLayoutGroup oldGrid = cardsContainerTrans.GetComponent<GridLayoutGroup>();
                if (oldGrid != null) Object.DestroyImmediate(oldGrid);

                layoutGroup = cardsContainerTrans.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 30f;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            // 4. Tạo Header_Title nếu chưa có
            Transform headerTrans = panelTrans.Find("Header_Title");
            if (headerTrans == null)
            {
                GameObject headerObj = new GameObject("Header_Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                headerObj.transform.SetParent(panelTrans, false);
                headerTrans = headerObj.transform;

                RectTransform hRect = headerObj.GetComponent<RectTransform>();
                hRect.anchorMin = new Vector2(0.5f, 1f);
                hRect.anchorMax = new Vector2(0.5f, 1f);
                hRect.pivot = new Vector2(0.5f, 1f);
                hRect.anchoredPosition = new Vector2(0, -20);
                hRect.sizeDelta = new Vector2(600, 50);

                TextMeshProUGUI hText = headerObj.GetComponent<TextMeshProUGUI>();
                hText.text = "<color=#FFD700>✦ LỰA CHỌN PHÁP BẢO ✦</color>";
                hText.fontSize = 32;
                hText.alignment = TextAlignmentOptions.Center;
            }

            // 5. Chuẩn hóa Footer_Controls (Vùng chứa Reroll & Skip dưới đáy)
            Transform footerTrans = panelTrans.Find("Footer_Controls");
            if (footerTrans == null)
            {
                GameObject footerObj = new GameObject("Footer_Controls", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                footerObj.transform.SetParent(panelTrans, false);
                footerTrans = footerObj.transform;

                RectTransform fRect = footerObj.GetComponent<RectTransform>();
                fRect.anchorMin = new Vector2(0.5f, 0f);
                fRect.anchorMax = new Vector2(0.5f, 0f);
                fRect.pivot = new Vector2(0.5f, 0f);
                fRect.anchoredPosition = new Vector2(0, 20);
                fRect.sizeDelta = new Vector2(500, 60);

                HorizontalLayoutGroup fLayout = footerObj.GetComponent<HorizontalLayoutGroup>();
                fLayout.childAlignment = TextAnchor.MiddleCenter;
                fLayout.spacing = 40f;
                fLayout.childControlWidth = false;
                fLayout.childControlHeight = false;
                fLayout.childForceExpandWidth = false;
                fLayout.childForceExpandHeight = false;
            }

            // 6. Tạo/Tìm Button_Reroll
            Transform rerollBtnTrans = footerTrans.Find("Button_Reroll");
            Button rerollBtn = null;
            TextMeshProUGUI rerollText = null;
            if (rerollBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Reroll", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(footerTrans, false);
                rerollBtnTrans = btnObj.transform;

                RectTransform bRect = btnObj.GetComponent<RectTransform>();
                bRect.sizeDelta = new Vector2(180, 50);

                Image bImg = btnObj.GetComponent<Image>();
                bImg.color = Color.white;
                bImg.type = Image.Type.Sliced;
                Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Primary_Normal.png");
                if (btnSprite != null) bImg.sprite = btnSprite;

                rerollBtn = btnObj.GetComponent<Button>();

                // Text bên trong nút Reroll
                GameObject textObj = new GameObject("Txt_RerollCount", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(btnObj.transform, false);

                RectTransform tRect = textObj.GetComponent<RectTransform>();
                tRect.anchorMin = Vector2.zero;
                tRect.anchorMax = Vector2.one;
                tRect.sizeDelta = Vector2.zero;

                rerollText = textObj.GetComponent<TextMeshProUGUI>();
                rerollText.text = "Đổi Thẻ (3)";
                rerollText.fontSize = 22;
                rerollText.alignment = TextAlignmentOptions.Center;
                rerollText.color = new Color(1f, 0.9f, 0.6f, 1f); // Màu chữ vàng đồng sáng
            }
            else
            {
                rerollBtn = rerollBtnTrans.GetComponent<Button>();
                rerollText = rerollBtnTrans.GetComponentInChildren<TextMeshProUGUI>();
            }

            // 7. Tạo/Tìm Button_Skip
            Transform skipBtnTrans = footerTrans.Find("Button_Skip");
            Button skipBtn = null;
            if (skipBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Skip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(footerTrans, false);
                skipBtnTrans = btnObj.transform;

                RectTransform bRect = btnObj.GetComponent<RectTransform>();
                bRect.sizeDelta = new Vector2(140, 50);

                Image bImg = btnObj.GetComponent<Image>();
                bImg.color = Color.white;
                bImg.type = Image.Type.Sliced;
                Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Primary_Normal.png");
                if (btnSprite != null) bImg.sprite = btnSprite;

                skipBtn = btnObj.GetComponent<Button>();

                GameObject textObj = new GameObject("Txt_Skip", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(btnObj.transform, false);

                RectTransform tRect = textObj.GetComponent<RectTransform>();
                tRect.anchorMin = Vector2.zero;
                tRect.anchorMax = Vector2.one;
                tRect.sizeDelta = Vector2.zero;

                TextMeshProUGUI skipText = textObj.GetComponent<TextMeshProUGUI>();
                skipText.text = "Bỏ Qua";
                skipText.fontSize = 22;
                skipText.alignment = TextAlignmentOptions.Center;
                skipText.color = new Color(0.9f, 0.8f, 0.8f, 1f);
            }
            else
            {
                skipBtn = skipBtnTrans.GetComponent<Button>();
            }

            // 8. Tự động liên kết các SerializedField vào UpgradeUIView & UpgradeUIPresenter
            SerializedObject soView = new SerializedObject(uiView);
            soView.FindProperty("_upgradePanel").objectReferenceValue = panelTrans.gameObject;
            soView.FindProperty("_cardsContainer").objectReferenceValue = cardsContainerTrans;
            soView.FindProperty("_rerollButton").objectReferenceValue = rerollBtn;
            soView.FindProperty("_skipButton").objectReferenceValue = skipBtn;
            soView.FindProperty("_rerollCountText").objectReferenceValue = rerollText;

            // Load Card Prefab từ Assets/_Prefabs/UI/
            UpgradeCardView cardPrefab = AssetDatabase.LoadAssetAtPath<UpgradeCardView>("Assets/_Prefabs/UI/UpgradeCard_Template.prefab");
            if (cardPrefab != null)
            {
                soView.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
            }

            soView.ApplyModifiedProperties();

            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = uiView;
            soPresenter.ApplyModifiedProperties();

            EditorUtility.SetDirty(rootObj);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(rootObj.scene);

            Debug.Log("<color=#4DEEEA>[UpgradeUIOptimizer] Đã tối ưu hóa hoàn thiện Hierarchy của UpgradeUI_Root theo đúng chuẩn MVP & Clean Architecture!</color>");
        }
    }
}

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

            // Cấu hình Stretch cho Upgrade_Panel (Tỉ lệ chuẩn 9-Slice)
            RectTransform panelRect = panelTrans.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.sizeDelta = new Vector2(1080, 640);
                panelRect.anchoredPosition = Vector2.zero;
            }

            Image panelBg = panelTrans.GetComponent<Image>();
            if (panelBg != null)
            {
                panelBg.color = Color.white;
                panelBg.type = Image.Type.Sliced;
                panelBg.pixelsPerUnitMultiplier = 1f;
                Sprite modalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Frame_Modal_Window_DongSon.png");
                if (modalSprite != null) panelBg.sprite = modalSprite;
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
                containerRect.sizeDelta = new Vector2(960, 430);
                containerRect.anchoredPosition = new Vector2(0, 15);
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
            }

            RectTransform hRect = headerTrans.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.5f, 1f);
            hRect.anchorMax = new Vector2(0.5f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.anchoredPosition = new Vector2(0, -32);
            hRect.sizeDelta = new Vector2(600, 45);

            TextMeshProUGUI hText = headerTrans.GetComponent<TextMeshProUGUI>();
            hText.text = "<color=#FFD700><b>LỰA CHỌN PHÁP BẢO</b></color>";
            hText.fontSize = 28;
            hText.alignment = TextAlignmentOptions.Center;

            // 5. Chuẩn hóa Footer_Controls (Vùng chứa Reroll & Skip dưới đáy)
            Transform footerTrans = panelTrans.Find("Footer_Controls");
            if (footerTrans == null)
            {
                GameObject footerObj = new GameObject("Footer_Controls", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                footerObj.transform.SetParent(panelTrans, false);
                footerTrans = footerObj.transform;
            }

            RectTransform fRect = footerTrans.GetComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0.5f, 0f);
            fRect.anchorMax = new Vector2(0.5f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.anchoredPosition = new Vector2(0, 36);
            fRect.sizeDelta = new Vector2(500, 60);

            HorizontalLayoutGroup fLayout = footerTrans.GetComponent<HorizontalLayoutGroup>();
            if (fLayout == null) fLayout = footerTrans.gameObject.AddComponent<HorizontalLayoutGroup>();
            fLayout.childAlignment = TextAnchor.MiddleCenter;
            fLayout.spacing = 35f;
            fLayout.childControlWidth = false;
            fLayout.childControlHeight = false;
            fLayout.childForceExpandWidth = false;
            fLayout.childForceExpandHeight = false;

            // 6. Tạo/Tìm Button_Reroll (Áp dụng Nút Vàng Hoàng Kim 9-Slice)
            Transform rerollBtnTrans = footerTrans.Find("Button_Reroll");
            Button rerollBtn = null;
            TextMeshProUGUI rerollText = null;
            if (rerollBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Reroll", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnObj.layer = LayerMask.NameToLayer("UI");
                btnObj.transform.SetParent(footerTrans, false);
                rerollBtnTrans = btnObj.transform;
            }

            RectTransform bRectReroll = rerollBtnTrans.GetComponent<RectTransform>();
            bRectReroll.sizeDelta = new Vector2(210, 58);

            Image bImgReroll = rerollBtnTrans.GetComponent<Image>();
            if (bImgReroll == null) bImgReroll = rerollBtnTrans.gameObject.AddComponent<Image>();
            bImgReroll.color = Color.white;
            bImgReroll.type = Image.Type.Sliced;
            bImgReroll.pixelsPerUnitMultiplier = 1f;
            Sprite btnSpriteGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_DragonGold.png");
            if (btnSpriteGold != null) bImgReroll.sprite = btnSpriteGold;

            rerollBtn = rerollBtnTrans.GetComponent<Button>();
            if (rerollBtn == null) rerollBtn = rerollBtnTrans.gameObject.AddComponent<Button>();

            // Text bên trong nút Reroll
            Transform textTransReroll = rerollBtnTrans.Find("Txt_RerollCount");
            if (textTransReroll == null)
            {
                GameObject textObj = new GameObject("Txt_RerollCount", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textObj.layer = LayerMask.NameToLayer("UI");
                textObj.transform.SetParent(rerollBtnTrans, false);
                textTransReroll = textObj.transform;
            }

            RectTransform tRectReroll = textTransReroll.GetComponent<RectTransform>();
            tRectReroll.anchorMin = Vector2.zero;
            tRectReroll.anchorMax = Vector2.one;
            tRectReroll.sizeDelta = Vector2.zero;

            rerollText = textTransReroll.GetComponent<TextMeshProUGUI>();
            rerollText.text = "Đổi Thẻ (3)";
            rerollText.fontSize = 20;
            rerollText.fontStyle = FontStyles.Bold;
            rerollText.alignment = TextAlignmentOptions.Center;
            rerollText.color = new Color(0.2f, 0.12f, 0.05f, 1f); // Chữ nâu gỗ trên nền vàng sáng
            rerollText.raycastTarget = false;

            // 7. Tạo/Tìm Button_Skip (Áp dụng Nút Đỏ Chu Sa 9-Slice)
            Transform skipBtnTrans = footerTrans.Find("Button_Skip");
            Button skipBtn = null;
            if (skipBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Skip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(footerTrans, false);
                skipBtnTrans = btnObj.transform;
            }

            RectTransform bRectSkip = skipBtnTrans.GetComponent<RectTransform>();
            bRectSkip.sizeDelta = new Vector2(170, 58);

            Image bImgSkip = skipBtnTrans.GetComponent<Image>();
            if (bImgSkip == null) bImgSkip = skipBtnTrans.gameObject.AddComponent<Image>();
            bImgSkip.color = Color.white;
            bImgSkip.type = Image.Type.Sliced;
            bImgSkip.pixelsPerUnitMultiplier = 1f;
            Sprite btnSpriteRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_CinnabarRed.png");
            if (btnSpriteRed != null) bImgSkip.sprite = btnSpriteRed;

            skipBtn = skipBtnTrans.GetComponent<Button>();
            if (skipBtn == null) skipBtn = skipBtnTrans.gameObject.AddComponent<Button>();

            Transform textTransSkip = skipBtnTrans.Find("Txt_Skip");
            if (textTransSkip == null)
            {
                GameObject textObj = new GameObject("Txt_Skip", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(skipBtnTrans, false);
                textTransSkip = textObj.transform;
            }

            RectTransform tRectSkip = textTransSkip.GetComponent<RectTransform>();
            tRectSkip.anchorMin = Vector2.zero;
            tRectSkip.anchorMax = Vector2.one;
            tRectSkip.sizeDelta = Vector2.zero;

            TextMeshProUGUI skipText = textTransSkip.GetComponent<TextMeshProUGUI>();
            skipText.text = "Bỏ Qua";
            skipText.fontSize = 20;
            skipText.fontStyle = FontStyles.Bold;
            skipText.alignment = TextAlignmentOptions.Center;
            skipText.color = Color.white;

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

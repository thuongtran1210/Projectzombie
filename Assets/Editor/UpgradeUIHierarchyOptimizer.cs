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
                var allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var r in allRoots)
                {
                    var found = r.transform.Find("UpgradeUI_Root");
                    if (found != null) { rootObj = found.gameObject; break; }
                    var foundNested = r.GetComponentsInChildren<UpgradeUIView>(true);
                    if (foundNested != null && foundNested.Length > 0) { rootObj = foundNested[0].gameObject; break; }
                }
            }

            if (rootObj == null)
            {
                var canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    rootObj = new GameObject("UpgradeUI_Root", typeof(RectTransform), typeof(CanvasGroup), typeof(UpgradeUIView), typeof(UpgradeUIPresenter));
                    rootObj.transform.SetParent(canvas.transform, false);
                    RectTransform rt = rootObj.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
                else
                {
                    Debug.LogWarning("[UpgradeUIOptimizer] Không tìm thấy Canvas hoặc UpgradeUI_Root trong Scene.");
                    return;
                }
            }

            Undo.RegisterFullObjectHierarchyUndo(rootObj, "Optimize UpgradeUI Hierarchy");

            // 1. Lấy hoặc gắn UpgradeUIView & UpgradeUIPresenter trên Root
            UpgradeUIView uiView = rootObj.GetComponent<UpgradeUIView>();
            if (uiView == null) uiView = rootObj.AddComponent<UpgradeUIView>();

            UpgradeUIPresenter presenter = rootObj.GetComponent<UpgradeUIPresenter>();
            if (presenter == null) presenter = rootObj.AddComponent<UpgradeUIPresenter>();

            // Nạp bộ Sprite Chibi Casual Arcade 3D mới
            Sprite modalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Frame_Upgrade_Modal_Chunky_3D.png");
            Sprite ribbonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Banner_Title_Ribbon_3D.png");
            Sprite cardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Card_Upgrade_Chunky_9Slice.png");
            Sprite iconOrbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Frame_Skill_Icon_Orb_3D.png");
            Sprite btnGoldSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Nav_3D_Gold.png");
            Sprite btnRedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Battle_3D_Red.png");

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

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
                panelRect.sizeDelta = new Vector2(1020, 620);
                panelRect.anchoredPosition = Vector2.zero;
            }

            Image panelBg = panelTrans.GetComponent<Image>();
            if (panelBg == null) panelBg = panelTrans.gameObject.AddComponent<Image>();
            if (panelBg != null)
            {
                panelBg.color = Color.white;
                panelBg.type = Image.Type.Sliced;
                if (modalSprite != null) panelBg.sprite = modalSprite;
            }

            // 3. Chuẩn hóa Cards_Container (Chứa 3 Card)
            Transform cardsContainerTrans = panelTrans.Find("Cards_Container");
            if (cardsContainerTrans == null)
            {
                Transform oldContainer = panelTrans.Find("Upgrade_Panel");
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
                containerRect.sizeDelta = new Vector2(940, 410);
                containerRect.anchoredPosition = new Vector2(0, 10);
            }

            HorizontalLayoutGroup layoutGroup = cardsContainerTrans.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                GridLayoutGroup oldGrid = cardsContainerTrans.GetComponent<GridLayoutGroup>();
                if (oldGrid != null) Object.DestroyImmediate(oldGrid);
                layoutGroup = cardsContainerTrans.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 24f;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            // 4. Tạo/Nâng cấp Header_Title (Dạng Băng Rôn Đỏ Ruby 3D)
            Transform headerTrans = panelTrans.Find("Header_Title");
            if (headerTrans == null)
            {
                GameObject headerObj = new GameObject("Header_Title", typeof(RectTransform), typeof(Image));
                headerObj.transform.SetParent(panelTrans, false);
                headerTrans = headerObj.transform;
            }

            RectTransform hRect = headerTrans.GetComponent<RectTransform>();
            if (hRect == null) hRect = headerTrans.gameObject.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.5f, 1f);
            hRect.anchorMax = new Vector2(0.5f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.anchoredPosition = new Vector2(0, -18);
            hRect.sizeDelta = new Vector2(440, 68);

            Image hImg = headerTrans.GetComponent<Image>();
            if (hImg == null) hImg = headerTrans.gameObject.AddComponent<Image>();
            hImg.color = Color.white;
            hImg.type = Image.Type.Sliced;
            if (ribbonSprite != null) hImg.sprite = ribbonSprite;

            Transform hTxtTrans = headerTrans.Find("Txt_Title");
            if (hTxtTrans == null)
            {
                GameObject txtObj = new GameObject("Txt_Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(headerTrans, false);
                hTxtTrans = txtObj.transform;
            }
            RectTransform htRT = hTxtTrans.GetComponent<RectTransform>();
            htRT.anchorMin = Vector2.zero;
            htRT.anchorMax = Vector2.one;
            htRT.offsetMin = Vector2.zero;
            htRT.offsetMax = Vector2.zero;

            TextMeshProUGUI hText = hTxtTrans.GetComponent<TextMeshProUGUI>();
            if (hText == null) hText = hTxtTrans.gameObject.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) hText.font = vietFont;
            hText.text = "LỰA CHỌN PHÁP BẢO";
            hText.fontSize = 24;
            hText.fontStyle = FontStyles.Bold;
            hText.alignment = TextAlignmentOptions.Center;
            hText.color = Color.white;

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
            fRect.anchoredPosition = new Vector2(0, 24);
            fRect.sizeDelta = new Vector2(460, 64);

            HorizontalLayoutGroup fLayout = footerTrans.GetComponent<HorizontalLayoutGroup>();
            if (fLayout == null) fLayout = footerTrans.gameObject.AddComponent<HorizontalLayoutGroup>();
            fLayout.childAlignment = TextAnchor.MiddleCenter;
            fLayout.spacing = 28f;
            fLayout.childControlWidth = false;
            fLayout.childControlHeight = false;

            // 6. Button_Reroll (Nút Vàng Hoàng Kim 3D)
            Transform rerollBtnTrans = footerTrans.Find("Button_Reroll");
            if (rerollBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Reroll", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(footerTrans, false);
                rerollBtnTrans = btnObj.transform;
            }

            RectTransform bRectReroll = rerollBtnTrans.GetComponent<RectTransform>();
            bRectReroll.sizeDelta = new Vector2(190, 56);

            Image bImgReroll = rerollBtnTrans.GetComponent<Image>();
            if (bImgReroll == null) bImgReroll = rerollBtnTrans.gameObject.AddComponent<Image>();
            bImgReroll.color = Color.white;
            bImgReroll.type = Image.Type.Sliced;
            if (btnGoldSprite != null) bImgReroll.sprite = btnGoldSprite;

            Button rerollBtn = rerollBtnTrans.GetComponent<Button>();
            if (rerollBtn == null) rerollBtn = rerollBtnTrans.gameObject.AddComponent<Button>();

            Transform textTransReroll = rerollBtnTrans.Find("Txt_RerollCount");
            if (textTransReroll == null)
            {
                GameObject textObj = new GameObject("Txt_RerollCount", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(rerollBtnTrans, false);
                textTransReroll = textObj.transform;
            }

            RectTransform tRectReroll = textTransReroll.GetComponent<RectTransform>();
            tRectReroll.anchorMin = Vector2.zero;
            tRectReroll.anchorMax = Vector2.one;
            tRectReroll.sizeDelta = Vector2.zero;

            TextMeshProUGUI rerollText = textTransReroll.GetComponent<TextMeshProUGUI>();
            if (rerollText == null) rerollText = textTransReroll.gameObject.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) rerollText.font = vietFont;
            rerollText.text = "Đổi Thẻ (3)";
            rerollText.fontSize = 17;
            rerollText.fontStyle = FontStyles.Bold;
            rerollText.alignment = TextAlignmentOptions.Center;
            rerollText.color = new Color(0.2f, 0.12f, 0.05f, 1f);

            // 7. Button_Skip (Nút Đỏ 3D)
            Transform skipBtnTrans = footerTrans.Find("Button_Skip");
            if (skipBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Skip", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(footerTrans, false);
                skipBtnTrans = btnObj.transform;
            }

            RectTransform bRectSkip = skipBtnTrans.GetComponent<RectTransform>();
            bRectSkip.sizeDelta = new Vector2(160, 56);

            Image bImgSkip = skipBtnTrans.GetComponent<Image>();
            if (bImgSkip == null) bImgSkip = skipBtnTrans.gameObject.AddComponent<Image>();
            bImgSkip.color = Color.white;
            bImgSkip.type = Image.Type.Sliced;
            if (btnRedSprite != null) bImgSkip.sprite = btnRedSprite;

            Button skipBtn = skipBtnTrans.GetComponent<Button>();
            if (skipBtn == null) skipBtn = skipBtnTrans.gameObject.AddComponent<Button>();

            Transform textTransSkip = skipBtnTrans.Find("Txt_Skip");
            if (textTransSkip == null)
            {
                GameObject textObj = new GameObject("Txt_Skip", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(skipBtnTrans, false);
                textTransSkip = textObj.transform;
            }

            RectTransform tRectSkip = textTransSkip.GetComponent<RectTransform>();
            tRectSkip.anchorMin = Vector2.zero;
            tRectSkip.anchorMax = Vector2.one;
            tRectSkip.sizeDelta = Vector2.zero;

            TextMeshProUGUI skipText = textTransSkip.GetComponent<TextMeshProUGUI>();
            if (skipText == null) skipText = textTransSkip.gameObject.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) skipText.font = vietFont;
            skipText.text = "Bỏ Qua";
            skipText.fontSize = 17;
            skipText.fontStyle = FontStyles.Bold;
            skipText.alignment = TextAlignmentOptions.Center;
            skipText.color = Color.white;

            // 8. Cấu hình Thẻ Card trong Container (Nếu đang có sẵn con)
            for (int i = 0; i < cardsContainerTrans.childCount; i++)
            {
                Transform card = cardsContainerTrans.GetChild(i);
                if (card == null) continue;
                RectTransform cardRT = card.GetComponent<RectTransform>();
                if (cardRT != null) cardRT.sizeDelta = new Vector2(270, 390);

                Image cardImg = card.GetComponent<Image>();
                if (cardImg != null && cardBgSprite != null)
                {
                    cardImg.sprite = cardBgSprite;
                    cardImg.type = Image.Type.Sliced;
                    cardImg.color = Color.white;
                }

                // Gắn khung viền Icon Orb tròn nếu có icon
                Transform iconTrans = card.Find("Icon");
                if (iconTrans == null) iconTrans = card.Find("Img_Icon");
                if (iconTrans != null && iconOrbSprite != null)
                {
                    Transform orbTrans = card.Find("Frame_Icon_Orb");
                    if (orbTrans == null)
                    {
                        GameObject orbObj = new GameObject("Frame_Icon_Orb", typeof(RectTransform), typeof(Image));
                        orbObj.transform.SetParent(card, false);
                        orbObj.transform.SetSiblingIndex(iconTrans.GetSiblingIndex());
                        orbTrans = orbObj.transform;
                    }
                    RectTransform orbRT = orbTrans.GetComponent<RectTransform>();
                    orbRT.anchorMin = new Vector2(0.5f, 0.5f);
                    orbRT.anchorMax = new Vector2(0.5f, 0.5f);
                    orbRT.pivot = new Vector2(0.5f, 0.5f);
                    orbRT.anchoredPosition = new Vector2(0, 60);
                    orbRT.sizeDelta = new Vector2(100, 100);
                    Image oImg = orbTrans.GetComponent<Image>();
                    if (oImg != null)
                    {
                        oImg.sprite = iconOrbSprite;
                        oImg.preserveAspect = true;
                    }

                    // Icon nằm trong Orb
                    iconTrans.SetParent(orbTrans, false);
                    RectTransform iRT = iconTrans.GetComponent<RectTransform>();
                    if (iRT != null)
                    {
                        iRT.anchorMin = new Vector2(0.5f, 0.5f);
                        iRT.anchorMax = new Vector2(0.5f, 0.5f);
                        iRT.pivot = new Vector2(0.5f, 0.5f);
                        iRT.anchoredPosition = Vector2.zero;
                        iRT.sizeDelta = new Vector2(64, 64);
                    }
                }
            }

            // 9. Tự động liên kết các SerializedField vào UpgradeUIView & UpgradeUIPresenter
            SerializedObject soView = new SerializedObject(uiView);
            var pProp = soView.FindProperty("_upgradePanel");
            if (pProp != null) pProp.objectReferenceValue = panelTrans.gameObject;

            var cProp = soView.FindProperty("_cardsContainer");
            if (cProp != null) cProp.objectReferenceValue = cardsContainerTrans;

            var rProp = soView.FindProperty("_rerollButton");
            if (rProp != null) rProp.objectReferenceValue = rerollBtn;

            var sProp = soView.FindProperty("_skipButton");
            if (sProp != null) sProp.objectReferenceValue = skipBtn;

            var rcProp = soView.FindProperty("_rerollCountText");
            if (rcProp != null) rcProp.objectReferenceValue = rerollText;

            UpgradeCardView cardPrefab = AssetDatabase.LoadAssetAtPath<UpgradeCardView>("Assets/_Prefabs/UI/UpgradeCard_Template.prefab");
            if (cardPrefab != null)
            {
                var cpProp = soView.FindProperty("_cardPrefab");
                if (cpProp != null) cpProp.objectReferenceValue = cardPrefab;
            }

            soView.ApplyModifiedProperties();

            SerializedObject soPresenter = new SerializedObject(presenter);
            var vProp = soPresenter.FindProperty("_view");
            if (vProp != null) vProp.objectReferenceValue = uiView;
            soPresenter.ApplyModifiedProperties();

            EditorUtility.SetDirty(rootObj);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(rootObj.scene);

            Debug.Log("<color=#FFD700>[UpgradeUIOptimizer] 🚀 ĐÃ HOÀN TẤT NÂNG CẤP TOÀN DIỆN UPGRADE PANEL CHIBI CASUAL ARCADE 3D!</color>");
        }
    }
}

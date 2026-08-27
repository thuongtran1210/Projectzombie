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

            // Nạp bộ Sprite Vọng Xuyên Cổ Phong mới
            Sprite modalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Modal_TangBaoCac_9Slice.png");
            Sprite bannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Banner_Upgrade_Parchment.png");
            Sprite cardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite parchmentSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Parchment_Detail_9Slice.png");
            Sprite badgePillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite btnSubWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Upgrade_Wood_Sub_9Slice.png");
            Sprite iconOrbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Weapon_Orb_Gold.png");

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
                panelRect.sizeDelta = new Vector2(1100, 640);
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
                containerRect.sizeDelta = new Vector2(980, 430);
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
            layoutGroup.spacing = 26f;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            // 4. Tạo/Nâng cấp Header_Title (Dạng Băng Rôn Cuộn Giấy Da Cổ)
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
            hRect.anchoredPosition = new Vector2(0, 18);
            hRect.sizeDelta = new Vector2(500, 78);

            Image hImg = headerTrans.GetComponent<Image>();
            if (hImg == null) hImg = headerTrans.gameObject.AddComponent<Image>();
            hImg.color = Color.white;
            hImg.type = Image.Type.Sliced;
            if (bannerSprite != null) hImg.sprite = bannerSprite;

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
            htRT.offsetMin = new Vector2(40, 8);
            htRT.offsetMax = new Vector2(-40, -12);

            TextMeshProUGUI hText = hTxtTrans.GetComponent<TextMeshProUGUI>();
            if (hText == null) hText = hTxtTrans.gameObject.AddComponent<TextMeshProUGUI>();
            if (vietFont != null) hText.font = vietFont;
            hText.text = "LỰA CHỌN PHÁP BẢO";
            hText.fontSize = 24;
            hText.fontStyle = FontStyles.Bold;
            hText.alignment = TextAlignmentOptions.Center;
            hText.color = new Color(0.25f, 0.16f, 0.10f, 1f);

            // 5. Chuẩn hóa Footer_Controls (Vùng chứa Làm Mới & Bỏ Qua dưới đáy)
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
            fRect.anchoredPosition = new Vector2(0, 14);
            fRect.sizeDelta = new Vector2(480, 56);

            HorizontalLayoutGroup fLayout = footerTrans.GetComponent<HorizontalLayoutGroup>();
            if (fLayout == null) fLayout = footerTrans.gameObject.AddComponent<HorizontalLayoutGroup>();
            fLayout.childAlignment = TextAnchor.MiddleCenter;
            fLayout.spacing = 30f;
            fLayout.childControlWidth = false;
            fLayout.childControlHeight = false;

            // 6. Button_Reroll (Nút Làm Mới Thẻ Gỗ)
            Transform rerollBtnTrans = footerTrans.Find("Button_Reroll");
            if (rerollBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Reroll", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(footerTrans, false);
                rerollBtnTrans = btnObj.transform;
            }

            RectTransform bRectReroll = rerollBtnTrans.GetComponent<RectTransform>();
            bRectReroll.sizeDelta = new Vector2(190, 50);

            Image bImgReroll = rerollBtnTrans.GetComponent<Image>();
            if (bImgReroll == null) bImgReroll = rerollBtnTrans.gameObject.AddComponent<Image>();
            bImgReroll.color = Color.white;
            bImgReroll.type = Image.Type.Sliced;
            if (btnSubWoodSprite != null) bImgReroll.sprite = btnSubWoodSprite;

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
            rerollText.text = "Làm Mới (3)";
            rerollText.fontSize = 17;
            rerollText.fontStyle = FontStyles.Bold;
            rerollText.alignment = TextAlignmentOptions.Center;
            rerollText.color = new Color(0.24f, 0.16f, 0.10f, 1f);

            // 7. Button_Skip (Nút Bỏ Qua Thẻ Gỗ)
            Transform skipBtnTrans = footerTrans.Find("Button_Skip");
            if (skipBtnTrans == null)
            {
                GameObject btnObj = new GameObject("Button_Skip", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(footerTrans, false);
                skipBtnTrans = btnObj.transform;
            }

            RectTransform bRectSkip = skipBtnTrans.GetComponent<RectTransform>();
            bRectSkip.sizeDelta = new Vector2(170, 50);

            Image bImgSkip = skipBtnTrans.GetComponent<Image>();
            if (bImgSkip == null) bImgSkip = skipBtnTrans.gameObject.AddComponent<Image>();
            bImgSkip.color = Color.white;
            bImgSkip.type = Image.Type.Sliced;
            if (btnSubWoodSprite != null) bImgSkip.sprite = btnSubWoodSprite;

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
            skipText.color = new Color(0.24f, 0.16f, 0.10f, 1f);

            // 8. Cấu hình Thẻ Card trong Container
            for (int i = 0; i < cardsContainerTrans.childCount; i++)
            {
                Transform card = cardsContainerTrans.GetChild(i);
                if (card == null) continue;
                RectTransform cardRT = card.GetComponent<RectTransform>();
                if (cardRT != null) cardRT.sizeDelta = new Vector2(290, 410);

                Image cardImg = card.GetComponent<Image>();
                if (cardImg != null && cardBgSprite != null)
                {
                    cardImg.sprite = cardBgSprite;
                    cardImg.type = Image.Type.Sliced;
                    cardImg.color = Color.white;
                }

                // Tờ Giấy Da Lồng Bên Trong Card
                Transform parchmentTrans = card.Find("Inner_Parchment");
                if (parchmentTrans == null)
                {
                    GameObject pObj = new GameObject("Inner_Parchment", typeof(RectTransform), typeof(Image));
                    pObj.transform.SetParent(card, false);
                    pObj.transform.SetAsFirstSibling();
                    parchmentTrans = pObj.transform;
                }
                RectTransform pRT = parchmentTrans.GetComponent<RectTransform>();
                pRT.anchorMin = Vector2.zero;
                pRT.anchorMax = Vector2.one;
                pRT.offsetMin = new Vector2(18, 16);
                pRT.offsetMax = new Vector2(-18, -20);
                Image pImg = parchmentTrans.GetComponent<Image>();
                if (pImg != null && parchmentSprite != null)
                {
                    pImg.sprite = parchmentSprite;
                    pImg.type = Image.Type.Sliced;
                }

                // Gắn Thẻ Gỗ Đỉnh (Rarity / Level)
                Transform badgeTrans = card.Find("Badge_Level");
                if (badgeTrans == null)
                {
                    GameObject bObj = new GameObject("Badge_Level", typeof(RectTransform), typeof(Image));
                    bObj.transform.SetParent(card, false);
                    badgeTrans = bObj.transform;
                }
                RectTransform bgRT = badgeTrans.GetComponent<RectTransform>();
                bgRT.anchorMin = new Vector2(0.5f, 1f);
                bgRT.anchorMax = new Vector2(0.5f, 1f);
                bgRT.pivot = new Vector2(0.5f, 1f);
                bgRT.anchoredPosition = new Vector2(0, 8);
                bgRT.sizeDelta = new Vector2(130, 34);
                Image bgImg = badgeTrans.GetComponent<Image>();
                if (bgImg != null && badgePillSprite != null)
                {
                    bgImg.sprite = badgePillSprite;
                    bgImg.type = Image.Type.Sliced;
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
                    orbRT.anchoredPosition = new Vector2(0, 50);
                    orbRT.sizeDelta = new Vector2(105, 105);
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
                        iRT.sizeDelta = new Vector2(70, 70);
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

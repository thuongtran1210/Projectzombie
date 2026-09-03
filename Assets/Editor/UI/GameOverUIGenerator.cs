#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Generator tự động dựng và chuẩn hóa toàn bộ Giao diện Game Over (GameOverUI_Root) chuẩn AAA Cổ Phong Đông Sơn
    /// Khắc phục triệt để lỗi dính chữ, tràn nút và thiếu lớp phủ mờ cảnh chiến trường.
    /// </summary>
    public static class GameOverUIGenerator
    {
        private static readonly Color ColorWoodDark = new Color(0.12f, 0.08f, 0.06f, 0.95f);
        private static readonly Color ColorGold = new Color(0.96f, 0.84f, 0.45f, 1f);
        private static readonly Color ColorBronzeBorder = new Color(0.55f, 0.38f, 0.20f, 1f);
        private static readonly Color ColorMutedText = new Color(0.85f, 0.82f, 0.78f, 1f);
        private static readonly Color ColorValueText = new Color(1.0f, 0.95f, 0.8f, 1f);

        [MenuItem("Tools/ProjectZombie/UI/⚡ Rebuild GameOver UI (Chuẩn Cổ Phong)", priority = 20)]
        public static void RebuildGameOverUI()
        {
            Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("[GameOverUIGenerator] Không tìm thấy Canvas chính trong Scene!");
                return;
            }

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (vietFont == null) vietFont = TMP_Settings.defaultFontAsset;

            // Load Sprites
            Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Panel_DongSon_GameOver.png");
            Sprite bannerDefeatSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Banner_GameOver_Defeat.png");
            Sprite cardSubBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Card_Stat_Sub_Bg.png");
            Sprite btnSonMaiSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_SonMai_ChuSa.png");
            Sprite btnGoMunSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");
            Sprite coinIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Icon_CoTien_VongXuyen.png");

            Sprite iconTime = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Icon_Stat_Time.png");
            Sprite iconKill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Icon_Stat_Kill.png");
            Sprite iconLevel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Icon_Stat_Level.png");
            Sprite iconDamage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Icon_Stat_Damage.png");

            // 1. Tìm hoặc tạo GameOverUI_Root
            Transform rootTrans = mainCanvas.transform.Find("GameOverUI_Root");
            if (rootTrans == null)
            {
                var existing = GameObject.Find("GameOverUI_Root");
                if (existing != null) rootTrans = existing.transform;
            }

            GameObject rootObj;
            if (rootTrans != null)
            {
                rootObj = rootTrans.gameObject;
                // Xóa toàn bộ con cũ để tạo mới hoàn toàn chuẩn
                for (int i = rootObj.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(rootObj.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                rootObj = new GameObject("GameOverUI_Root", typeof(RectTransform));
                rootObj.transform.SetParent(mainCanvas.transform, false);
            }

            Undo.RegisterFullObjectHierarchyUndo(rootObj, "Rebuild GameOver UI");

            // Cấu hình Stretch cho Root
            RectTransform rootRect = rootObj.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var view = rootObj.GetComponent<GameOverScreenView>();
            if (view == null) view = rootObj.AddComponent<GameOverScreenView>();

            var presenter = rootObj.GetComponent<GameOverScreenPresenter>();
            if (presenter == null) presenter = rootObj.AddComponent<GameOverScreenPresenter>();

            var canvasGroup = rootObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = rootObj.AddComponent<CanvasGroup>();

            // 2. Màn tối toàn cảnh (Backdrop Dim Overlay)
            GameObject dimObj = new GameObject("Backdrop_Dim", typeof(RectTransform), typeof(Image));
            dimObj.transform.SetParent(rootObj.transform, false);
            RectTransform dimRect = dimObj.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero;
            dimRect.offsetMax = Vector2.zero;
            Image dimImg = dimObj.GetComponent<Image>();
            dimImg.color = new Color(0.04f, 0.03f, 0.05f, 0.85f);
            dimImg.raycastTarget = true;

            // 3. Khung Gỗ Mun Viền Đồng Lớn (Background_Panel)
            GameObject panelObj = new GameObject("Background_Panel", typeof(RectTransform), typeof(Image));
            panelObj.transform.SetParent(rootObj.transform, false);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(580f, 520f);
            panelRect.anchoredPosition = new Vector2(0f, 0f);

            Image panelImg = panelObj.GetComponent<Image>();
            if (panelSprite != null)
            {
                panelImg.sprite = panelSprite;
                panelImg.type = Image.Type.Sliced;
                panelImg.color = Color.white;
            }
            else
            {
                panelImg.color = ColorWoodDark;
            }

            // 4. Banner Tiêu Đề (Banner_Title)
            GameObject bannerObj = new GameObject("Banner_Title", typeof(RectTransform), typeof(Image));
            bannerObj.transform.SetParent(panelObj.transform, false);
            RectTransform bannerRect = bannerObj.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 1f);
            bannerRect.anchorMax = new Vector2(0.5f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.sizeDelta = new Vector2(400f, 75f);
            bannerRect.anchoredPosition = new Vector2(0f, -10f);

            Image bannerImg = bannerObj.GetComponent<Image>();
            if (bannerDefeatSprite != null)
            {
                bannerImg.sprite = bannerDefeatSprite;
                bannerImg.type = Image.Type.Sliced;
            }
            else
            {
                bannerImg.color = new Color(0.6f, 0.15f, 0.12f, 1f);
            }

            GameObject titleObj = new GameObject("Title_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(bannerObj.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
            titleTMP.font = vietFont;
            titleTMP.text = "ĐÃ NGÃ XUỐNG";
            titleTMP.fontSize = 28;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(1f, 0.95f, 0.85f, 1f);

            // 5. Khung Lưới Thống Kê (Stats_Grid) 2 cột x 2 dòng
            GameObject gridObj = new GameObject("Stats_Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(panelObj.transform, false);
            RectTransform gridRect = gridObj.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 1f);
            gridRect.anchorMax = new Vector2(0.5f, 1f);
            gridRect.pivot = new Vector2(0.5f, 1f);
            gridRect.sizeDelta = new Vector2(500f, 150f);
            gridRect.anchoredPosition = new Vector2(0f, -105f);

            GridLayoutGroup grid = gridObj.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(240f, 65f);
            grid.spacing = new Vector2(20f, 12f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            // 4 Thẻ con
            var timeCard = CreateStatCard(gridObj.transform, "Card_Time", iconTime, "Thời gian", "00:00", cardSubBgSprite, vietFont);
            var killCard = CreateStatCard(gridObj.transform, "Card_Kill", iconKill, "Diệt quái", "0", cardSubBgSprite, vietFont);
            var levelCard = CreateStatCard(gridObj.transform, "Card_Level", iconLevel, "Cấp độ", "Lv.1", cardSubBgSprite, vietFont);
            var damageCard = CreateStatCard(gridObj.transform, "Card_Damage", iconDamage, "Sát thương", "0", cardSubBgSprite, vietFont);

            // 6. Khung Cổ Tiền Nhận Được (Currency_Container)
            GameObject curObj = new GameObject("Currency_Container", typeof(RectTransform), typeof(Image));
            curObj.transform.SetParent(panelObj.transform, false);
            RectTransform curRect = curObj.GetComponent<RectTransform>();
            curRect.anchorMin = new Vector2(0.5f, 0f);
            curRect.anchorMax = new Vector2(0.5f, 0f);
            curRect.pivot = new Vector2(0.5f, 0.5f);
            curRect.sizeDelta = new Vector2(400f, 60f);
            curRect.anchoredPosition = new Vector2(0f, 155f);

            Image curBg = curObj.GetComponent<Image>();
            if (cardSubBgSprite != null)
            {
                curBg.sprite = cardSubBgSprite;
                curBg.type = Image.Type.Sliced;
                curBg.color = new Color(1f, 0.9f, 0.7f, 1f);
            }
            else
            {
                curBg.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);
            }

            // Coin Icon
            GameObject coinIconObj = new GameObject("Icon_Coin", typeof(RectTransform), typeof(Image));
            coinIconObj.transform.SetParent(curObj.transform, false);
            RectTransform coinIconRect = coinIconObj.GetComponent<RectTransform>();
            coinIconRect.anchorMin = new Vector2(0f, 0.5f);
            coinIconRect.anchorMax = new Vector2(0f, 0.5f);
            coinIconRect.pivot = new Vector2(0.5f, 0.5f);
            coinIconRect.sizeDelta = new Vector2(40f, 40f);
            coinIconRect.anchoredPosition = new Vector2(35f, 0f);

            Image coinImg = coinIconObj.GetComponent<Image>();
            if (coinIconSprite != null) coinImg.sprite = coinIconSprite;

            // Coin Text
            GameObject coinTextObj = new GameObject("Currency_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            coinTextObj.transform.SetParent(curObj.transform, false);
            RectTransform coinTextRect = coinTextObj.GetComponent<RectTransform>();
            coinTextRect.anchorMin = new Vector2(0f, 0f);
            coinTextRect.anchorMax = new Vector2(1f, 1f);
            coinTextRect.offsetMin = new Vector2(65f, 0f);
            coinTextRect.offsetMax = new Vector2(-15f, 0f);

            TextMeshProUGUI coinTMP = coinTextObj.GetComponent<TextMeshProUGUI>();
            coinTMP.font = vietFont;
            coinTMP.text = "+0 Cổ Tiền";
            coinTMP.fontSize = 24;
            coinTMP.fontStyle = FontStyles.Bold;
            coinTMP.alignment = TextAlignmentOptions.MidlineLeft;
            coinTMP.color = ColorGold;

            // 7. Cụm 2 Nút Bấm: TÁI CHIẾN & HỒI QUY
            GameObject btnContainer = new GameObject("Buttons_Container", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            btnContainer.transform.SetParent(panelObj.transform, false);
            RectTransform btnContainerRect = btnContainer.GetComponent<RectTransform>();
            btnContainerRect.anchorMin = new Vector2(0.5f, 0f);
            btnContainerRect.anchorMax = new Vector2(0.5f, 0f);
            btnContainerRect.pivot = new Vector2(0.5f, 0.5f);
            btnContainerRect.sizeDelta = new Vector2(500f, 65f);
            btnContainerRect.anchoredPosition = new Vector2(0f, 60f);

            HorizontalLayoutGroup btnLayout = btnContainer.GetComponent<HorizontalLayoutGroup>();
            btnLayout.childControlWidth = false;
            btnLayout.childControlHeight = false;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childForceExpandHeight = false;
            btnLayout.spacing = 30f;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;

            // Nút Tái Chiến
            GameObject playBtnObj = CreateButton(btnContainer.transform, "PlayAgain_Button", "TÁI CHIẾN", btnSonMaiSprite, new Color(1f, 0.95f, 0.8f, 1f), vietFont);
            // Nút Hồi Quy
            GameObject menuBtnObj = CreateButton(btnContainer.transform, "MainMenu_Button", "HỒI QUY", btnGoMunSprite, new Color(0.88f, 0.86f, 0.92f, 1f), vietFont);

            // Wire Serialized Properties cho GameOverScreenView
            var soView = new SerializedObject(view);
            soView.FindProperty("panel").objectReferenceValue = panelObj;
            soView.FindProperty("backdropDim").objectReferenceValue = dimImg;
            soView.FindProperty("bannerImage").objectReferenceValue = bannerImg;
            soView.FindProperty("titleText").objectReferenceValue = titleTMP;
            soView.FindProperty("timeAliveText").objectReferenceValue = timeCard;
            soView.FindProperty("killCountText").objectReferenceValue = killCard;
            soView.FindProperty("maxLevelText").objectReferenceValue = levelCard;
            soView.FindProperty("damageDealtText").objectReferenceValue = damageCard;
            soView.FindProperty("currencyEarnedText").objectReferenceValue = coinTMP;
            soView.FindProperty("currencyIcon").objectReferenceValue = coinImg;
            soView.FindProperty("playAgainButton").objectReferenceValue = playBtnObj.GetComponent<Button>();
            soView.FindProperty("mainMenuButton").objectReferenceValue = menuBtnObj.GetComponent<Button>();
            soView.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            soView.ApplyModifiedProperties();

            // Wire GameOverScreenPresenter
            var soPres = new SerializedObject(presenter);
            soPres.FindProperty("view").objectReferenceValue = view;
            soPres.ApplyModifiedProperties();

            EditorUtility.SetDirty(rootObj);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("<color=#00FF88>[GameOverUIGenerator]</color> ĐÃ DỰNG THÀNH CÔNG GIAO DIỆN GAME OVER CHUẨN CỔ PHONG ĐÔNG SƠN!");
        }

        private static TextMeshProUGUI CreateStatCard(Transform parent, string name, Sprite icon, string label, string defaultValue, Sprite bgSprite, TMP_FontAsset font)
        {
            GameObject cardObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            cardObj.transform.SetParent(parent, false);
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(240f, 65f);

            Image cardImg = cardObj.GetComponent<Image>();
            if (bgSprite != null)
            {
                cardImg.sprite = bgSprite;
                cardImg.type = Image.Type.Sliced;
            }
            else
            {
                cardImg.color = new Color(0.18f, 0.13f, 0.1f, 0.9f);
            }

            // Icon
            if (icon != null)
            {
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(cardObj.transform, false);
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(36f, 36f);
                iconRect.anchoredPosition = new Vector2(26f, 0f);

                Image img = iconObj.GetComponent<Image>();
                img.sprite = icon;
            }

            // Label Text (vd: Thời gian)
            GameObject labelObj = new GameObject("Label_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(cardObj.transform, false);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(50f, 0f);
            labelRect.offsetMax = new Vector2(-10f, -5f);

            TextMeshProUGUI labelTMP = labelObj.GetComponent<TextMeshProUGUI>();
            labelTMP.font = font;
            labelTMP.text = label;
            labelTMP.fontSize = 13;
            labelTMP.color = ColorMutedText;
            labelTMP.alignment = TextAlignmentOptions.BottomLeft;

            // Value Text (vd: 03:45)
            GameObject valObj = new GameObject("Value_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            valObj.transform.SetParent(cardObj.transform, false);
            RectTransform valRect = valObj.GetComponent<RectTransform>();
            valRect.anchorMin = new Vector2(0f, 0f);
            valRect.anchorMax = new Vector2(1f, 0.5f);
            valRect.offsetMin = new Vector2(50f, 5f);
            valRect.offsetMax = new Vector2(-10f, 0f);

            TextMeshProUGUI valTMP = valObj.GetComponent<TextMeshProUGUI>();
            valTMP.font = font;
            valTMP.text = defaultValue;
            valTMP.fontSize = 17;
            valTMP.fontStyle = FontStyles.Bold;
            valTMP.color = ColorValueText;
            valTMP.alignment = TextAlignmentOptions.TopLeft;

            return valTMP;
        }

        private static GameObject CreateButton(Transform parent, string name, string label, Sprite btnSprite, Color textColor, TMP_FontAsset font)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(210f, 56f);

            Image img = btnObj.GetComponent<Image>();
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
            }
            else
            {
                img.color = new Color(0.3f, 0.2f, 0.15f, 1f);
            }

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = img;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;

            return btnObj;
        }
    }
}
#endif

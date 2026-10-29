using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor.SceneManagement;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseMatchUIInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Install Match UI In Open Scene")]
        public static void InstallMatchUI()
        {
            if (EditorApplication.isPlaying)
                throw new System.InvalidOperationException("Exit Play Mode before installing Match UI.");

            var existingCanvas = Object.FindObjectOfType<DefenseMatchPresenter>(true);
            if (existingCanvas != null)
            {
                BindSceneReferences(existingCanvas);
                EnsureEventSystem();
                Debug.Log("[TREBRAVE Defense3D] Existing Match UI references refreshed; authored layout preserved.");
                Selection.activeGameObject = existingCanvas.gameObject;
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseMatchUICanvas.prefab");
            if (prefab == null) prefab = CreateOrUpdateMatchUIPrefab();
            var canvasGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(canvasGo, "Install Defense Match UI Canvas");
            BindSceneReferences(canvasGo.GetComponent<DefenseMatchPresenter>());
            EnsureEventSystem();
            Selection.activeGameObject = canvasGo;
            Debug.Log("<color=#00FF88>[TREBRAVE Defense3D]</color> Đã cài đặt hoàn tất [TREBRAVE] Defense Match UI Canvas vào Scene!");
        }

        private static void BindSceneReferences(DefenseMatchPresenter presenter)
        {
            var run = Object.FindObjectOfType<DefenseRun6003D>();
            var data = new SerializedObject(presenter);
            data.FindProperty("_run").objectReferenceValue = run;
            data.FindProperty("_woodWallet").objectReferenceValue = Object.FindObjectOfType<DefenseWoodWallet3D>();
            data.ApplyModifiedProperties();
            if (run != null)
            {
                var runData = new SerializedObject(run);
                runData.FindProperty("matchUI").objectReferenceValue = presenter;
                runData.ApplyModifiedProperties();
            }
            EditorSceneManager.MarkSceneDirty(presenter.gameObject.scene);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>(true) != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(go, "Install Defense UI EventSystem");
        }

        [MenuItem("TREBRAVE/Defense 3D/Create or Update Match UI Prefab in Resources")]
        public static GameObject CreateOrUpdateMatchUIPrefab()
        {
            var go = CreateMatchUICanvasGameObject(false);
            string prefabPath = "Assets/Resources/Defense3D/DefenseMatchUICanvas.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF88>[TREBRAVE Defense3D]</color> Match UI Prefab saved to: " + prefabPath);
            return prefab;
        }

        public static GameObject CreateMatchUICanvasGameObject(bool registerUndo = true)
        {
            // 1. Root Canvas
            var canvasGo = new GameObject("[TREBRAVE] Defense Match UI Canvas", typeof(RectTransform));
            if (registerUndo)
                Undo.RegisterCreatedObjectUndo(canvasGo, "Install Defense Match UI Canvas");

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // 2. HUD View
            var hudGo = CreateChild(canvasGo, "MatchHUD");
            var hudView = hudGo.AddComponent<DefenseMatchHUDView>();

            // Top Info Bar
            var topBar = CreateChild(hudGo, "TopBar");
            var timerText = CreateTMP(topBar, "TimerText", "00:00", 32, TextAlignmentOptions.TopLeft);
            SetRect(timerText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), new Vector2(300, 45));

            var eventText = CreateTMP(topBar, "EventText", "Đợt: 0/34", 24, TextAlignmentOptions.TopLeft);
            SetRect(eventText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -75), new Vector2(300, 35));

            // Base HP
            var baseHpGo = CreateChild(hudGo, "BaseHP_Panel");
            var baseSlider = CreateSimpleSlider(baseHpGo, "BaseHealthSlider", new Color(0f, 0.8f, 0.4f));
            SetRect(baseSlider.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -120), new Vector2(320, 24));
            var baseText = CreateTMP(baseHpGo, "BaseHealthText", "Căn Cứ: 200/200", 20, TextAlignmentOptions.Left);
            SetRect(baseText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(35, -120), new Vector2(310, 24));

            // Hero HP
            var heroHpGo = CreateChild(hudGo, "HeroHP_Panel");
            var heroSlider = CreateSimpleSlider(heroHpGo, "HeroHealthSlider", new Color(0f, 0.7f, 1f));
            SetRect(heroSlider.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -155), new Vector2(320, 24));
            var heroText = CreateTMP(heroHpGo, "HeroHealthText", "Tướng: 100/100", 20, TextAlignmentOptions.Left);
            SetRect(heroText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(35, -155), new Vector2(310, 24));

            // Wood
            var woodText = CreateTMP(hudGo, "WoodText", "Gỗ: 0", 28, TextAlignmentOptions.TopRight);
            SetRect(woodText.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(250, 45));

            // EXP
            var expGo = CreateChild(hudGo, "EXP_Panel");
            var expSlider = CreateSimpleSlider(expGo, "ExpSlider", new Color(1f, 0.8f, 0f));
            SetRect(expSlider.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -85), new Vector2(250, 20));
            var expText = CreateTMP(expGo, "ExpText", "Cấp 1 (0/30)", 18, TextAlignmentOptions.Right);
            SetRect(expText.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-35, -85), new Vector2(240, 20));

            // Boss HP (Ẩn mặc định)
            var bossGo = CreateChild(hudGo, "BossHP_Panel");
            var bossSlider = CreateSimpleSlider(bossGo, "BossHealthSlider", new Color(0.9f, 0.1f, 0.3f));
            SetRect(bossSlider.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(600, 32));
            var bossText = CreateTMP(bossGo, "BossHealthText", "Dạ Xoa: 3200/3200", 22, TextAlignmentOptions.Center);
            SetRect(bossText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(580, 32));
            bossGo.SetActive(false);

            // Wire HUD View
            var serializedHud = new SerializedObject(hudView);
            serializedHud.FindProperty("_timerText").objectReferenceValue = timerText;
            serializedHud.FindProperty("_eventStatusText").objectReferenceValue = eventText;
            serializedHud.FindProperty("_baseHealthSlider").objectReferenceValue = baseSlider;
            serializedHud.FindProperty("_baseHealthText").objectReferenceValue = baseText;
            serializedHud.FindProperty("_heroHealthSlider").objectReferenceValue = heroSlider;
            serializedHud.FindProperty("_heroHealthText").objectReferenceValue = heroText;
            serializedHud.FindProperty("_woodCountText").objectReferenceValue = woodText;
            serializedHud.FindProperty("_expSlider").objectReferenceValue = expSlider;
            serializedHud.FindProperty("_expLevelText").objectReferenceValue = expText;
            serializedHud.FindProperty("_bossHealthRoot").objectReferenceValue = bossGo;
            serializedHud.FindProperty("_bossHealthSlider").objectReferenceValue = bossSlider;
            serializedHud.FindProperty("_bossHealthText").objectReferenceValue = bossText;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            // 3. Upgrade Modal View
            var upgradeModalGo = CreateChild(canvasGo, "UpgradeModal");
            var upgradeModalView = upgradeModalGo.AddComponent<DefenseUpgradeModalView>();
            var upgradeBg = CreateImage(upgradeModalGo, "OverlayBg", new Color(0f, 0f, 0f, 0.75f));
            upgradeBg.raycastTarget = true;
            SetRect(upgradeBg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var upgradeHeader = CreateTMP(upgradeModalGo, "HeaderText", "CHỌN THẺ NÂNG CẤP", 40, TextAlignmentOptions.Center);
            SetRect(upgradeHeader.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(600, 60));

            var cardContainer = CreateChild(upgradeModalGo, "CardsContainer");
            var slotProps = new DefenseUpgradeCardSlotUI[3];
            float[] cardX = { -350f, 0f, 350f };
            for (int i = 0; i < 3; i++)
            {
                var cardBtnGo = CreateChild(cardContainer, "CardButton_" + (i + 1));
                var btnImg = cardBtnGo.AddComponent<Image>();
                btnImg.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);
                var btn = cardBtnGo.AddComponent<Button>();
                SetRect(cardBtnGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(cardX[i], -20), new Vector2(300, 380));

                var title = CreateTMP(cardBtnGo, "CardTitle", "Tên Thẻ", 26, TextAlignmentOptions.Top);
                SetRect(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -35), new Vector2(260, 60));

                var desc = CreateTMP(cardBtnGo, "CardDesc", "Mô tả hiệu ứng buff...", 20, TextAlignmentOptions.Center);
                SetRect(desc.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(250, 180));

                slotProps[i] = new DefenseUpgradeCardSlotUI
                {
                    CardButton = btn,
                    TitleText = title,
                    DescriptionText = desc
                };
            }

            var serializedUpgrade = new SerializedObject(upgradeModalView);
            serializedUpgrade.FindProperty("_modalRoot").objectReferenceValue = upgradeModalGo;
            var slotsArray = serializedUpgrade.FindProperty("_cardSlots");
            slotsArray.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                var el = slotsArray.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("CardButton").objectReferenceValue = slotProps[i].CardButton;
                el.FindPropertyRelative("TitleText").objectReferenceValue = slotProps[i].TitleText;
                el.FindPropertyRelative("DescriptionText").objectReferenceValue = slotProps[i].DescriptionText;
            }
            serializedUpgrade.ApplyModifiedPropertiesWithoutUndo();
            upgradeModalGo.SetActive(false);

            // 4. Result Modal View
            var resultModalGo = CreateChild(canvasGo, "ResultModal");
            var resultModalView = resultModalGo.AddComponent<DefenseMatchResultModalView>();
            var resultBg = CreateImage(resultModalGo, "ResultBg", new Color(0f, 0f, 0f, 0.85f));
            resultBg.raycastTarget = true;
            SetRect(resultBg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var resBox = CreateImage(resultModalGo, "BoxContainer", new Color(0.12f, 0.15f, 0.2f, 0.98f));
            SetRect(resBox.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 450));

            var titleRes = CreateTMP(resBox.gameObject, "ResultTitle", "KẾT QUẢ", 48, TextAlignmentOptions.Center);
            SetRect(titleRes.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(650, 65));

            var msgRes = CreateTMP(resBox.gameObject, "ResultMessage", "Mô tả kết thúc trận...", 24, TextAlignmentOptions.Center);
            SetRect(msgRes.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(600, 80));

            var statsRes = CreateTMP(resBox.gameObject, "StatsSummary", "Thời gian: 00:00 | Gỗ: 0", 22, TextAlignmentOptions.Center);
            SetRect(statsRes.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -45), new Vector2(600, 50));

            // Restart Button
            var restartBtnGo = CreateChild(resBox.gameObject, "RestartButton");
            var restartBtnImg = restartBtnGo.AddComponent<Image>();
            restartBtnImg.color = new Color(0.15f, 0.6f, 0.3f);
            var restartBtn = restartBtnGo.AddComponent<Button>();
            SetRect(restartBtnGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-120, 60), new Vector2(200, 60));
            var restartBtnText = CreateTMP(restartBtnGo, "Text", "Chơi Lại", 24, TextAlignmentOptions.Center);
            SetRect(restartBtnText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Main Menu Button
            var menuBtnGo = CreateChild(resBox.gameObject, "MainMenuButton");
            var menuBtnImg = menuBtnGo.AddComponent<Image>();
            menuBtnImg.color = new Color(0.4f, 0.45f, 0.55f);
            var menuBtn = menuBtnGo.AddComponent<Button>();
            SetRect(menuBtnGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(120, 60), new Vector2(200, 60));
            var menuBtnText = CreateTMP(menuBtnGo, "Text", "Menu Chính", 24, TextAlignmentOptions.Center);
            SetRect(menuBtnText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var serializedResult = new SerializedObject(resultModalView);
            serializedResult.FindProperty("_modalRoot").objectReferenceValue = resultModalGo;
            serializedResult.FindProperty("_titleText").objectReferenceValue = titleRes;
            serializedResult.FindProperty("_messageText").objectReferenceValue = msgRes;
            serializedResult.FindProperty("_statsSummaryText").objectReferenceValue = statsRes;
            serializedResult.FindProperty("_restartButton").objectReferenceValue = restartBtn;
            serializedResult.FindProperty("_mainMenuButton").objectReferenceValue = menuBtn;
            serializedResult.ApplyModifiedPropertiesWithoutUndo();
            resultModalGo.SetActive(false);

            // 5. Presenter
            var presenter = canvasGo.AddComponent<DefenseMatchPresenter>();
            var serializedPresenter = new SerializedObject(presenter);
            serializedPresenter.FindProperty("_hudView").objectReferenceValue = hudView;
            serializedPresenter.FindProperty("_upgradeModalView").objectReferenceValue = upgradeModalView;
            serializedPresenter.FindProperty("_resultModalView").objectReferenceValue = resultModalView;
            serializedPresenter.FindProperty("_run").objectReferenceValue = Object.FindObjectOfType<DefenseRun6003D>();
            serializedPresenter.FindProperty("_woodWallet").objectReferenceValue = Object.FindObjectOfType<DefenseWoodWallet3D>();
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            return canvasGo;
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetRect(go.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return go;
        }

        private static TextMeshProUGUI CreateTMP(GameObject parent, string name, string text, float fontSize, TextAlignmentOptions align)
        {
            var go = CreateChild(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.raycastTarget = false; // Tối ưu theo rule 6.3
            return tmp;
        }

        private static Image CreateImage(GameObject parent, string name, Color color)
        {
            var go = CreateChild(parent, name);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false; // Tối ưu theo rule 6.3
            return img;
        }

        private static Slider CreateSimpleSlider(GameObject parent, string name, Color fillColor)
        {
            var sliderGo = CreateChild(parent, name);
            var slider = sliderGo.AddComponent<Slider>();

            var bg = CreateImage(sliderGo, "Background", new Color(0.15f, 0.15f, 0.15f, 0.8f));
            SetRect(bg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var fillArea = CreateChild(sliderGo, "FillArea");
            SetRect(fillArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var fill = CreateImage(fillArea, "Fill", fillColor);
            SetRect(fill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            slider.fillRect = fill.rectTransform;
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            return slider;
        }

        private static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
        }
    }
}

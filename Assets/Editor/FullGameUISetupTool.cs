using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Editor.UI;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor Tool 1-Click tự động quét, chuẩn hóa và dựng cấu trúc trọn bộ UI Canvas trong Scene:
    /// - Canvas_MetaMenu (Sảnh Hoàng Tuyền, Điện Anh Hùng, Miếu Tứ Bất Tử, Screen Stack)
    /// - Canvas_Gameplay (HUD, Mobile Controls: Joystick, Attack, Dash, Skill)
    /// - Fade Transition Overlay & GameStateManager / MetaSceneTransitionController
    /// </summary>
    public class FullGameUISetupTool : EditorWindow
    {
        [MenuItem("Tools/ProjectZombie/Setup Full UI Hierarchy (1-Click)", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<FullGameUISetupTool>("Setup Game UI");
            window.minSize = new Vector2(460, 420);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Cấu Trúc Toàn Bộ UI Canvas (Hướng A - All-in-One)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Tool này sẽ tự động thiết lập hoặc chuẩn hóa Scene hiện tại:\n" +
                "1. Tạo/Chuẩn hóa Canvas_MetaMenu (Sảnh Hoàng Tuyền, Chọn Anh Hùng, Miếu Nâng Cấp).\n" +
                "2. Tạo/Chuẩn hóa Canvas_Gameplay (Run HUD, cụm nút Mobile Controls: Attack, Dash, Skill).\n" +
                "3. Thiết lập Fade Transition Overlay và kết nối MetaSceneTransitionController.\n" +
                "4. Tự động Wire toàn bộ View và Presenter theo chuẩn MVP.",
                MessageType.Info
            );

            EditorGUILayout.Space(15);

            if (GUILayout.Button("⚡ Tự Động Dựng & Chuẩn Hóa Toàn Bộ Canvas", GUILayout.Height(45)))
            {
                SetupFullUIInScene();
            }
        }

        public static void SetupFullUIInScene()
        {
            // 1. Tìm hoặc tạo Canvas Chính
            Canvas mainCanvas = null;
            var masterObj = GameObject.Find("Canvas_Master");
            if (masterObj != null) mainCanvas = masterObj.GetComponent<Canvas>();

            if (mainCanvas == null)
            {
                var gameUICanvasObj = GameObject.Find("GameUICanvas");
                if (gameUICanvasObj != null)
                {
                    mainCanvas = gameUICanvasObj.GetComponent<Canvas>();
                    if (mainCanvas != null)
                    {
                        mainCanvas.gameObject.name = "Canvas_Master";
                    }
                }
            }

            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas != null)
                {
                    mainCanvas.gameObject.name = "Canvas_Master";
                }
            }

            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas_Master", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                mainCanvas = canvasObj.GetComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            Undo.RegisterFullObjectHierarchyUndo(mainCanvas.gameObject, "Setup Full UI Canvas");

            var scaler = mainCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // 1.1. Đảm bảo quy hoạch toàn bộ Manager về root --- GAME MANAGER ---
            GameObject managerRoot = GameObject.Find("--- GAME MANAGER ---");
            if (managerRoot == null)
            {
                var stateMgr = Object.FindObjectOfType<ProjectZombie.Features.Shared.GameStateManager>();
                if (stateMgr != null) managerRoot = stateMgr.gameObject;
            }

            if (managerRoot == null)
            {
                managerRoot = new GameObject("--- GAME MANAGER ---", typeof(ProjectZombie.Features.Shared.GameStateManager));
            }

            // Gắn GameManager vào root --- GAME MANAGER --- nếu chưa có
            var gameMgrComponent = managerRoot.GetComponent<ProjectZombie.Core.Save.GameManager>();
            if (gameMgrComponent == null)
            {
                gameMgrComponent = managerRoot.AddComponent<ProjectZombie.Core.Save.GameManager>();
            }

            // Gắn MetaCurrencyManager vào root --- GAME MANAGER --- nếu chưa có
            var currencyMgr = managerRoot.GetComponent<ProjectZombie.Features.MetaProgression.MetaCurrencyManager>();
            if (currencyMgr == null)
            {
                currencyMgr = managerRoot.AddComponent<ProjectZombie.Features.MetaProgression.MetaCurrencyManager>();
            }

            // Dọn dẹp component trùng thừa trên Canvas (nếu có)
            var duplicateCanvasCurrencyMgr = mainCanvas.GetComponent<ProjectZombie.Features.MetaProgression.MetaCurrencyManager>();
            if (duplicateCanvasCurrencyMgr != null && duplicateCanvasCurrencyMgr != currencyMgr)
            {
                Object.DestroyImmediate(duplicateCanvasCurrencyMgr);
            }

            // Dọn dẹp GameObject "GameManager" rời rạc nếu tồn tại
            var standaloneGameMgr = GameObject.Find("GameManager");
            if (standaloneGameMgr != null && standaloneGameMgr != managerRoot)
            {
                // Di chuyển con của standaloneGameMgr sang managerRoot trước khi xóa
                while (standaloneGameMgr.transform.childCount > 0)
                {
                    standaloneGameMgr.transform.GetChild(0).SetParent(managerRoot.transform);
                }
                Object.DestroyImmediate(standaloneGameMgr);
            }

            // Quản lý CoinPoolManager làm con của --- GAME MANAGER ---
            var coinPoolMgr = Object.FindObjectOfType<ProjectZombie.Features.Collectibles.CoinPoolManager>();
            if (coinPoolMgr == null)
            {
                var coinPoolObj = new GameObject("[CoinPoolManager]", typeof(ProjectZombie.Features.Collectibles.CoinPoolManager));
                coinPoolObj.transform.SetParent(managerRoot.transform);
                coinPoolMgr = coinPoolObj.GetComponent<ProjectZombie.Features.Collectibles.CoinPoolManager>();
            }
            else if (coinPoolMgr.transform.parent != managerRoot.transform)
            {
                coinPoolMgr.transform.SetParent(managerRoot.transform);
            }

            // Quản lý ExpGemPoolManager làm con của --- GAME MANAGER ---
            var expGemPoolMgr = Object.FindObjectOfType<ProjectZombie.Features.Collectibles.ExpGemPoolManager>();
            if (expGemPoolMgr == null)
            {
                var expGemPoolObj = new GameObject("[ExpGemPoolManager]", typeof(ProjectZombie.Features.Collectibles.ExpGemPoolManager));
                expGemPoolObj.transform.SetParent(managerRoot.transform);
                expGemPoolMgr = expGemPoolObj.GetComponent<ProjectZombie.Features.Collectibles.ExpGemPoolManager>();
            }
            else if (expGemPoolMgr.transform.parent != managerRoot.transform)
            {
                expGemPoolMgr.transform.SetParent(managerRoot.transform);
            }

            // Đảm bảo Coin_Drop.prefab luôn được tạo và gán sẵn
            string coinPrefabPath = "Assets/_Prefabs/Collectibles/Coin_Drop.prefab";
            GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coinPrefabPath);
            if (coinPrefab == null)
            {
                Projectzombie.Editor.CollectiblesTools.CoinPrefabBuilder.CreateCoinPrefab();
                coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coinPrefabPath);
            }
            if (coinPoolMgr != null && coinPrefab != null)
            {
                var soPool = new SerializedObject(coinPoolMgr);
                var propPrefab = soPool.FindProperty("defaultCoinPrefab");
                if (propPrefab != null && propPrefab.objectReferenceValue == null)
                {
                    propPrefab.objectReferenceValue = coinPrefab;
                    soPool.ApplyModifiedProperties();
                    EditorUtility.SetDirty(coinPoolMgr);
                }
            }

            // 2. Dựng Canvas_MetaMenu Root
            Transform metaRoot = mainCanvas.transform.Find("Canvas_MetaMenu");
            if (metaRoot == null)
            {
                GameObject metaObj = new GameObject("Canvas_MetaMenu", typeof(RectTransform), typeof(CanvasGroup), typeof(MetaUIManager));
                metaObj.transform.SetParent(mainCanvas.transform, false);
                metaRoot = metaObj.transform;
            }
            
            // Instantiate MainHubUI từ Prefab chuẩn
            MainHubView mainHub = null;
            GameObject mainHubPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/MainHubUI.prefab");
            if (mainHubPrefab != null)
            {
                Transform oldHub = metaRoot.transform.Find("Panel_MainHub");
                if (oldHub != null) Object.DestroyImmediate(oldHub.gameObject);

                GameObject hubInstance = (GameObject)PrefabUtility.InstantiatePrefab(mainHubPrefab, metaRoot.transform);
                hubInstance.name = "Panel_MainHub";
                hubInstance.transform.SetAsFirstSibling();
                mainHub = hubInstance.GetComponent<MainHubView>();
            }

            // Instantiate SettingsModalUI từ Prefab chuẩn
            SettingsModalView settingsView = null;
            GameObject settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/SettingsModalUI.prefab");
            if (settingsPrefab != null)
            {
                Transform oldSet = metaRoot.transform.Find("Modal_Settings");
                if (oldSet != null) Object.DestroyImmediate(oldSet.gameObject);

                GameObject setInstance = (GameObject)PrefabUtility.InstantiatePrefab(settingsPrefab, metaRoot.transform);
                setInstance.name = "Modal_Settings";
                setInstance.SetActive(false);
                settingsView = setInstance.GetComponent<SettingsModalView>();
            }

            StretchRect(metaRoot.GetComponent<RectTransform>());
            var metaGroup = metaRoot.GetComponent<CanvasGroup>();
            var metaManager = metaRoot.GetComponent<MetaUIManager>();

            // 2.1. Panel_MainHub (Sảnh Hoàng Tuyền Chuẩn AAA)
            ProjectZombie.Editor.UI.MainHubUIGenerator.GenerateMainHubPrefab();
            Transform hubTrans = metaRoot.Find("Panel_MainHub");

            // 2.2. Panel_CharacterSelect (Chọn Anh Hùng)
            ProjectZombie.Editor.UI.CharacterSelectionUIGenerator.GenerateCharacterSelectionPrefab();
            Transform heroTrans = metaRoot.Find("Panel_CharacterSelect");

            // 2.3. Panel_WeaponLoadout (Tàng Bảo Các)
            ProjectZombie.Editor.UI.WeaponLoadoutUIGenerator.GenerateWeaponLoadoutPrefab();
            Transform loadoutTrans = metaRoot.Find("Panel_WeaponLoadout");

            // 2.4. Panel_SanctuaryTree (Miếu Cổ)
            Transform sanctuaryTrans = metaRoot.Find("Panel_SanctuaryTree");
            if (sanctuaryTrans == null)
            {
                GameObject sanctuaryObj = new GameObject("Panel_SanctuaryTree", typeof(RectTransform), typeof(CanvasGroup), typeof(MetaUpgradeShopView), typeof(MetaUpgradeShopPresenter));
                sanctuaryObj.transform.SetParent(metaRoot, false);
                sanctuaryTrans = sanctuaryObj.transform;
            }
            StretchRect(sanctuaryTrans.GetComponent<RectTransform>());
            BuildSanctuaryHierarchy(sanctuaryTrans);

            // Wire MetaUIManager
            var soMeta = new SerializedObject(metaManager);
            soMeta.FindProperty("_metaCanvasGroup").objectReferenceValue = metaGroup;
            soMeta.FindProperty("_mainHubScreen").objectReferenceValue = hubTrans.GetComponent<MainHubView>();
            soMeta.FindProperty("_characterSelectScreen").objectReferenceValue = heroTrans.GetComponent<CharacterSelectionView>();
            soMeta.FindProperty("_weaponLoadoutScreen").objectReferenceValue = loadoutTrans.GetComponent<WeaponLoadoutView>();
            soMeta.FindProperty("_sanctuaryTreeScreen").objectReferenceValue = sanctuaryTrans.GetComponent<MetaUpgradeShopView>();
            if (settingsView != null)
            {
                soMeta.FindProperty("_settingsScreen").objectReferenceValue = settingsView;
            }
            soMeta.ApplyModifiedProperties();

            // 2.5. Tạo CharacterPreviewStage (Sân khấu chiếu Animation tướng lên UI)
            var previewStage = Object.FindAnyObjectByType<CharacterPreviewStage>();
            if (previewStage == null)
            {
                GameObject stageObj = new GameObject("CharacterPreviewStage");
                stageObj.transform.position = new Vector3(2000f, 2000f, 0f);
                stageObj.AddComponent<CharacterPreviewStage>();
            }

            // 3. Dựng Canvas_Gameplay Root
            Transform gameRoot = mainCanvas.transform.Find("Canvas_Gameplay");
            if (gameRoot == null)
            {
                GameObject gameObj = new GameObject("Canvas_Gameplay", typeof(RectTransform), typeof(CanvasGroup), typeof(GameplayUIManager));
                gameObj.transform.SetParent(mainCanvas.transform, false);
                gameRoot = gameObj.transform;
            }
            StretchRect(gameRoot.GetComponent<RectTransform>());
            var gameGroup = gameRoot.GetComponent<CanvasGroup>();
            var gameplayManager = gameRoot.GetComponent<GameplayUIManager>();
            if (gameplayManager == null) gameplayManager = gameRoot.gameObject.AddComponent<GameplayUIManager>();

            // 4. Fade Overlay Panel
            Transform fadeTrans = mainCanvas.transform.Find("Panel_FadeOverlay");
            if (fadeTrans == null)
            {
                GameObject fadeObj = new GameObject("Panel_FadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                fadeObj.transform.SetParent(mainCanvas.transform, false);
                fadeTrans = fadeObj.transform;

                Image img = fadeObj.GetComponent<Image>();
                img.color = Color.black;
            }
            StretchRect(fadeTrans.GetComponent<RectTransform>());
            var fadeGroup = fadeTrans.GetComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeObjOrTrans(fadeTrans, false);

            // 5. Tìm và di chuyển UI_RunHUDRoot & Panel_MobileControls vào Canvas_Gameplay
            Transform hudTrans = gameRoot.Find("UI_RunHUDRoot");
            if (hudTrans == null) hudTrans = gameRoot.Find("RunHUD_Root");
            if (hudTrans == null)
            {
                // Tìm kiếm cả trường hợp có khoảng trắng thừa trong tên (vd: " UI_RunHUDRoot")
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allObjects)
                {
                    if (go.name.Trim() == "UI_RunHUDRoot" || go.name.Trim() == "RunHUD_Root")
                    {
                        go.name = "UI_RunHUDRoot";
                        go.transform.SetParent(gameRoot, false);
                        hudTrans = go.transform;
                        break;
                    }
                }
            }

            // Tự động nâng cấp và gắn trọn bộ Sprite Đông Sơn cho Top Run HUD
            RunHUDHierarchyOptimizer.OptimizeRunHUD();

            // Tự động sinh Card Template Prefab và tối ưu toàn diện Level Up Selection Modal
            UpgradeCardPrefabGenerator.GenerateCardPrefab();
            UpgradeUIHierarchyOptimizer.OptimizeUpgradeUI();

            Transform mobileTrans = gameRoot.Find("Panel_MobileControls");
            if (mobileTrans == null)
            {
                var existingMobile = GameObject.Find("Panel_MobileControls");
                if (existingMobile == null)
                {
                    var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var go in allObjects)
                    {
                        if (go.name.Trim() == "Panel_MobileControls")
                        {
                            existingMobile = go;
                            break;
                        }
                    }
                }

                if (existingMobile != null)
                {
                    existingMobile.transform.SetParent(gameRoot, false);
                    mobileTrans = existingMobile.transform;
                }
            }

            // Tự động gọi MobileControlsSetupTool để dựng cụm Joystick & Attack Button nếu chưa có
            EditorApplication.ExecuteMenuItem("Tools/ProjectZombie/Mobile Controls Setup & Auto-Wire");

            if (mobileTrans == null)
            {
                var createdMobile = GameObject.Find("Panel_MobileControls");
                if (createdMobile != null)
                {
                    createdMobile.transform.SetParent(gameRoot, false);
                    mobileTrans = createdMobile.transform;
                }
            }

            // Wire GameplayUIManager
            var soGameplay = new SerializedObject(gameplayManager);
            soGameplay.FindProperty("_gameplayCanvasGroup").objectReferenceValue = gameGroup;
            if (hudTrans != null)
            {
                soGameplay.FindProperty("_runHUDPanel").objectReferenceValue = hudTrans.gameObject;
            }
            if (mobileTrans != null)
            {
                soGameplay.FindProperty("_mobileControlsPanel").objectReferenceValue = mobileTrans.gameObject;
            }
            soGameplay.ApplyModifiedProperties();

            // 6. MetaSceneTransitionController
            var transitionController = FindObjectOfType<MetaSceneTransitionController>();
            if (transitionController == null)
            {
                GameObject tcObj = new GameObject("MetaSceneTransitionController", typeof(MetaSceneTransitionController));
                transitionController = tcObj.GetComponent<MetaSceneTransitionController>();
            }

            var soTC = new SerializedObject(transitionController);
            soTC.FindProperty("_fadeOverlayCanvasGroup").objectReferenceValue = fadeGroup;
            soTC.FindProperty("_metaUIManager").objectReferenceValue = metaManager;
            soTC.FindProperty("_gameplayUIManager").objectReferenceValue = gameplayManager;
            soTC.FindProperty("_mainHubPresenter").objectReferenceValue = hubTrans.GetComponent<MainHubPresenter>();
            soTC.ApplyModifiedProperties();

            EditorUtility.SetDirty(mainCanvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCanvas.gameObject.scene);

            Debug.Log("[FullGameUISetupTool] Đã dựng thành công toàn bộ hệ thống Canvas UI All-in-One!");
        }

        private static void BuildMainHubHierarchy(Transform hubRoot)
        {
            // Sử dụng MainHubUIGenerator để dựng giao diện MainHub theo chuẩn Art DNA Đông Sơn
            MainHubUIGenerator.GenerateMainHubUI();
        }

        private static void BuildCharacterSelectHierarchy(Transform root)
        {
            var view = root.GetComponent<CharacterSelectionView>();
            var presenter = root.GetComponent<CharacterSelectionPresenter>();
            var so = new SerializedObject(view);

            // Nền tối
            Image bg = root.GetComponent<Image>();
            if (bg == null) bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // 1. Tên nhân vật
            Transform nameTrans = root.Find("Txt_HeroName");
            if (nameTrans == null)
            {
                var obj = new GameObject("Txt_HeroName", typeof(RectTransform), typeof(TextMeshProUGUI));
                obj.transform.SetParent(root, false);
                nameTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0, -60);
                rect.sizeDelta = new Vector2(500, 70);
                var tmp = obj.GetComponent<TextMeshProUGUI>();
                tmp.fontSize = 42;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.text = "THƯ SINH";
            }
            so.FindProperty("_characterNameText").objectReferenceValue = nameTrans.GetComponent<TextMeshProUGUI>();

            // 2. Avatar
            Transform avatarTrans = root.Find("Img_Avatar");
            if (avatarTrans == null)
            {
                var obj = new GameObject("Img_Avatar", typeof(RectTransform), typeof(Image));
                obj.transform.SetParent(root, false);
                avatarTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 50);
                rect.sizeDelta = new Vector2(260, 260);
            }
            so.FindProperty("_characterAvatarImage").objectReferenceValue = avatarTrans.GetComponent<Image>();

            // 3. Hệ Ngũ Hành
            Transform elemTrans = root.Find("Txt_Element");
            if (elemTrans == null)
            {
                var obj = new GameObject("Txt_Element", typeof(RectTransform), typeof(TextMeshProUGUI));
                obj.transform.SetParent(root, false);
                elemTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, -110);
                rect.sizeDelta = new Vector2(400, 50);
                var tmp = obj.GetComponent<TextMeshProUGUI>();
                tmp.fontSize = 28;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.text = "<color=#E8C468>HỆ KIM</color>";
            }
            else
            {
                var tmp = elemTrans.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = "<color=#E8C468>HỆ KIM</color>";
            }
            so.FindProperty("_elementText").objectReferenceValue = elemTrans.GetComponent<TextMeshProUGUI>();

            // 4. Nút Chọn (Nút Xanh Ngọc Bích 9-Slice)
            Transform selectBtnTrans = root.Find("Btn_SelectHero");
            if (selectBtnTrans == null)
            {
                var obj = new GameObject("Btn_SelectHero", typeof(RectTransform), typeof(Image), typeof(Button));
                obj.transform.SetParent(root, false);
                selectBtnTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 100);
                rect.sizeDelta = new Vector2(280, 65);
                var img = obj.GetComponent<Image>();
                img.color = Color.white;
                img.type = Image.Type.Sliced;
                Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_JadeGreen.png");
                if (btnSprite != null) img.sprite = btnSprite;

                var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(obj.transform, false);
                StretchRect(txtObj.GetComponent<RectTransform>());
                var tmp = txtObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "CHỌN ANH HÙNG";
                tmp.fontSize = 24;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }
            so.FindProperty("_selectButton").objectReferenceValue = selectBtnTrans.GetComponent<Button>();

            // 5. Nút Prev & Next
            Transform prevBtnTrans = root.Find("Btn_Prev");
            if (prevBtnTrans == null)
            {
                var obj = new GameObject("Btn_Prev", typeof(RectTransform), typeof(Image), typeof(Button));
                obj.transform.SetParent(root, false);
                prevBtnTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-220, 50);
                rect.sizeDelta = new Vector2(70, 90);
                var img = obj.GetComponent<Image>();
                img.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
            }
            so.FindProperty("_prevButton").objectReferenceValue = prevBtnTrans.GetComponent<Button>();

            Transform nextBtnTrans = root.Find("Btn_Next");
            if (nextBtnTrans == null)
            {
                var obj = new GameObject("Btn_Next", typeof(RectTransform), typeof(Image), typeof(Button));
                obj.transform.SetParent(root, false);
                nextBtnTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(220, 50);
                rect.sizeDelta = new Vector2(70, 90);
                var img = obj.GetComponent<Image>();
                img.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
            }
            so.FindProperty("_nextButton").objectReferenceValue = nextBtnTrans.GetComponent<Button>();

            // 6. Nút Back (Nút Đỏ Chu Sa 9-Slice)
            Transform backBtnTrans = root.Find("Btn_Back");
            if (backBtnTrans == null)
            {
                var obj = new GameObject("Btn_Back", typeof(RectTransform), typeof(Image), typeof(Button));
                obj.transform.SetParent(root, false);
                backBtnTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(50, -40);
                rect.sizeDelta = new Vector2(120, 50);
                var img = obj.GetComponent<Image>();
                img.color = Color.white;
                img.type = Image.Type.Sliced;
                Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_CinnabarRed.png");
                if (btnSprite != null) img.sprite = btnSprite;

                var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(obj.transform, false);
                StretchRect(txtObj.GetComponent<RectTransform>());
                var tmp = txtObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "QUAY LẠI";
                tmp.fontSize = 18;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }
            so.FindProperty("_backButton").objectReferenceValue = backBtnTrans.GetComponent<Button>();

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(view);

            // Wire Presenter
            if (presenter != null)
            {
                var soPresenter = new SerializedObject(presenter);
                soPresenter.FindProperty("_view").objectReferenceValue = view;
                var selectionData = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterSelectionData>("Assets/_Data/CharacterSelectionData.asset");
                if (selectionData != null)
                {
                    soPresenter.FindProperty("_selectionData").objectReferenceValue = selectionData;
                }
                soPresenter.ApplyModifiedProperties();
                EditorUtility.SetDirty(presenter);
            }
        }

        private static void BuildSanctuaryHierarchy(Transform root)
        {
            var view = root.GetComponent<MetaUpgradeShopView>();
            var presenter = root.GetComponent<MetaUpgradeShopPresenter>();
            var so = new SerializedObject(view);

            // Nền tối
            Image bg = root.GetComponent<Image>();
            if (bg == null) bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.1f, 0.08f, 0.95f);

            // 1. Tiêu đề
            Transform titleTrans = root.Find("Txt_Title");
            if (titleTrans == null)
            {
                var obj = new GameObject("Txt_Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                obj.transform.SetParent(root, false);
                titleTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0, -50);
                rect.sizeDelta = new Vector2(600, 70);
                var tmp = obj.GetComponent<TextMeshProUGUI>();
                tmp.fontSize = 38;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.text = "MIẾU TỨ BẤT TỬ";
            }
            else
            {
                var tmp = titleTrans.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = "MIẾU TỨ BẤT TỬ";
            }
            // 2. Số dư Cổ Tiền
            Transform balanceTrans = root.Find("Txt_Balance");
            if (balanceTrans == null)
            {
                var obj = new GameObject("Txt_Balance", typeof(RectTransform), typeof(TextMeshProUGUI));
                obj.transform.SetParent(root, false);
                balanceTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-50, -50);
                rect.sizeDelta = new Vector2(250, 60);
                var tmp = obj.GetComponent<TextMeshProUGUI>();
                tmp.fontSize = 28;
                tmp.alignment = TextAlignmentOptions.Right;
                tmp.text = "0 Cổ Tiền";
            }
            else
            {
                var tmp = balanceTrans.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = "0 Cổ Tiền";
            }
            so.FindProperty("_coTienBalanceText").objectReferenceValue = balanceTrans.GetComponent<TextMeshProUGUI>();
            // 2.1. Chi tiết gói Nâng Cấp (Ở giữa màn hình)
            Transform cardTrans = root.Find("Card_UpgradeDetail");
            if (cardTrans == null)
            {
                var cardObj = new GameObject("Card_UpgradeDetail", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(root, false);
                cardTrans = cardObj.transform;

                var rect = cardObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 40);
                rect.sizeDelta = new Vector2(550, 260);

                var cardImg = cardObj.GetComponent<Image>();
                cardImg.color = new Color(0.12f, 0.18f, 0.15f, 0.95f);

                // Tên nâng cấp
                var nameObj = new GameObject("Txt_UpgradeName", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameObj.transform.SetParent(cardTrans, false);
                var nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.5f, 1f);
                nameRect.anchorMax = new Vector2(0.5f, 1f);
                nameRect.pivot = new Vector2(0.5f, 1f);
                nameRect.anchoredPosition = new Vector2(0, -25);
                nameRect.sizeDelta = new Vector2(500, 50);
                var nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
                nameTmp.fontSize = 28;
                nameTmp.fontStyle = FontStyles.Bold;
                nameTmp.alignment = TextAlignmentOptions.Center;
                nameTmp.text = "Kim Cang Thể (+15 HP Máu)";

                // Cấp độ
                var lvlObj = new GameObject("Txt_Level", typeof(RectTransform), typeof(TextMeshProUGUI));
                lvlObj.transform.SetParent(cardTrans, false);
                var lvlRect = lvlObj.GetComponent<RectTransform>();
                lvlRect.anchorMin = new Vector2(0.5f, 0.5f);
                lvlRect.anchorMax = new Vector2(0.5f, 0.5f);
                lvlRect.pivot = new Vector2(0.5f, 0.5f);
                lvlRect.anchoredPosition = new Vector2(0, 0);
                lvlRect.sizeDelta = new Vector2(400, 40);
                var lvlTmp = lvlObj.GetComponent<TextMeshProUGUI>();
                lvlTmp.fontSize = 22;
                lvlTmp.alignment = TextAlignmentOptions.Center;
                lvlTmp.text = "Cấp hiện tại: 1 / 10";

                // Giá tiền
                var costObj = new GameObject("Txt_Cost", typeof(RectTransform), typeof(TextMeshProUGUI));
                costObj.transform.SetParent(cardTrans, false);
                var costRect = costObj.GetComponent<RectTransform>();
                costRect.anchorMin = new Vector2(0.5f, 0f);
                costRect.anchorMax = new Vector2(0.5f, 0f);
                costRect.pivot = new Vector2(0.5f, 0f);
                costRect.anchoredPosition = new Vector2(0, 25);
                costRect.sizeDelta = new Vector2(400, 45);
                var costTmp = costObj.GetComponent<TextMeshProUGUI>();
                costTmp.fontSize = 24;
                costTmp.alignment = TextAlignmentOptions.Center;
                costTmp.text = "Giá: <color=#FFD700>100 Cổ Tiền</color>";
            }

            var txtName = cardTrans.Find("Txt_UpgradeName")?.GetComponent<TextMeshProUGUI>();
            var txtLvl = cardTrans.Find("Txt_Level")?.GetComponent<TextMeshProUGUI>();
            var txtCost = cardTrans.Find("Txt_Cost")?.GetComponent<TextMeshProUGUI>();

            if (txtName != null) so.FindProperty("_upgradeTitleText").objectReferenceValue = txtName;
            if (txtLvl != null) so.FindProperty("_upgradeLevelText").objectReferenceValue = txtLvl;
            if (txtCost != null) so.FindProperty("_upgradeCostText").objectReferenceValue = txtCost;

            // 3. Nút Mua Nâng Cấp
            Transform buyBtnTrans = root.Find("Btn_BuyUpgrade");
            if (buyBtnTrans == null)
            {
                var obj = new GameObject("Btn_BuyUpgrade", typeof(RectTransform), typeof(Image), typeof(Button));
                obj.transform.SetParent(root, false);
                buyBtnTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 100);
                rect.sizeDelta = new Vector2(280, 65);
                var img = obj.GetComponent<Image>();
                img.color = Color.white;
                img.type = Image.Type.Sliced;
                Sprite btnGreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_JadeGreen.png");
                if (btnGreen != null) img.sprite = btnGreen;

                var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(obj.transform, false);
                StretchRect(txtObj.GetComponent<RectTransform>());
                var tmp = txtObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "NÂNG CẤP";
                tmp.fontSize = 24;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }
            so.FindProperty("_buyUpgradeButton").objectReferenceValue = buyBtnTrans.GetComponent<Button>();

            // Wire Presenter
            if (presenter != null)
            {
                var soPresenter = new SerializedObject(presenter);
                soPresenter.FindProperty("_view").objectReferenceValue = view;
                soPresenter.ApplyModifiedProperties();
                EditorUtility.SetDirty(presenter);
            }

            // 4. Nút Đóng / Back (Nút Đỏ Chu Sa 9-Slice)
            Transform closeBtnTrans = root.Find("Btn_Close");
            if (closeBtnTrans == null)
            {
                var obj = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
                obj.transform.SetParent(root, false);
                closeBtnTrans = obj.transform;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(50, -40);
                rect.sizeDelta = new Vector2(120, 50);
                var img = obj.GetComponent<Image>();
                img.color = Color.white;
                img.type = Image.Type.Sliced;
                Sprite btnRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Action_CinnabarRed.png");
                if (btnRed != null) img.sprite = btnRed;

                var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(obj.transform, false);
                StretchRect(txtObj.GetComponent<RectTransform>());
                var tmp = txtObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "ĐÓNG";
                tmp.fontSize = 18;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }
            so.FindProperty("_closeButton").objectReferenceValue = closeBtnTrans.GetComponent<Button>();

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(view);
        }

        private static void StretchRect(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void fadeObjOrTrans(Transform t, bool active)
        {
            if (t != null) t.gameObject.SetActive(active);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Quản lý tập trung các màn hình UI Ngoài Game theo cơ chế Navigation Screen Stack (Hướng A - All-in-One).
    /// Đảm bảo không chồng đè giao diện và hỗ trợ nút Back phần cứng của thiết bị di động Android.
    /// </summary>
    public class MetaUIManager : MonoBehaviour
    {
        public static MetaUIManager Instance { get; private set; }

        [Header("Root Meta Canvas")]
        [SerializeField] private CanvasGroup _metaCanvasGroup;

        [Header("Registered Screens")]
        [SerializeField] private BaseMetaScreenView _mainHubScreen;
        [SerializeField] private BaseMetaScreenView _characterSelectScreen;
        [SerializeField] private BaseMetaScreenView _weaponLoadoutScreen;
        [SerializeField] private BaseMetaScreenView _sanctuaryTreeScreen;
        [SerializeField] private BaseMetaScreenView _codexScreen;
        [SerializeField] private BaseMetaScreenView _settingsScreen;
        [SerializeField] private BaseMetaScreenView _gachaShopScreen;
        [SerializeField] private BaseMetaScreenView _stageSelectScreen;
        [SerializeField] private BaseMetaScreenView _resourceDownloadScreen;

        [Header("Persistent Backdrop")]
        [Tooltip("Ảnh nền cố định che 100% Tilemap và Player bên dưới khi ở trong Menu")]
        [SerializeField] private GameObject _persistentBackdrop;

        private readonly Stack<BaseMetaScreenView> _screenStack = new Stack<BaseMetaScreenView>();

        public bool IsInMetaMenu => _metaCanvasGroup != null && _metaCanvasGroup.gameObject.activeSelf;
        public BaseMetaScreenView WeaponLoadoutScreen => _weaponLoadoutScreen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (_metaCanvasGroup == null)
            {
                _metaCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (_metaCanvasGroup != null)
            {
                _metaCanvasGroup.alpha = 1f;
                _metaCanvasGroup.interactable = true;
                _metaCanvasGroup.blocksRaycasts = true;
            }

            EnsurePersistentBackdrop();

            AutoResolveMissingScreens();

            // Mặc định ẩn và tắt toàn bộ màn hình phụ ngay trong Awake
            foreach (Transform child in transform)
            {
                if (child.name == "Persistent_MetaBackdrop") continue;
                if (_mainHubScreen != null && child == _mainHubScreen.transform) continue;
                child.gameObject.SetActive(false);
            }

            if (_characterSelectScreen != null) _characterSelectScreen.gameObject.SetActive(false);
            if (_weaponLoadoutScreen != null) _weaponLoadoutScreen.gameObject.SetActive(false);
            if (_sanctuaryTreeScreen != null) _sanctuaryTreeScreen.gameObject.SetActive(false);
            if (_codexScreen != null) _codexScreen.gameObject.SetActive(false);
            if (_settingsScreen != null) _settingsScreen.gameObject.SetActive(false);
            if (_gachaShopScreen != null) _gachaShopScreen.gameObject.SetActive(false);
            if (_stageSelectScreen != null) _stageSelectScreen.gameObject.SetActive(false);
            if (_resourceDownloadScreen != null) _resourceDownloadScreen.gameObject.SetActive(false);

            // Mở màn hình Sảnh Chính (Main Hub) đầu tiên
            if (_mainHubScreen != null)
            {
                _mainHubScreen.gameObject.SetActive(true);
                PushScreen(_mainHubScreen);
            }
        }

        private void EnsurePersistentBackdrop()
        {
            if (_persistentBackdrop == null)
            {
                var existing = transform.Find("Persistent_MetaBackdrop");
                if (existing != null)
                {
                    _persistentBackdrop = existing.gameObject;
                }
                else
                {
                    Sprite bgForest = Resources.Load<Sprite>("UI/VongXuyen/BG_VongXuyen_Forest_Hub") 
                                   ?? Resources.Load<Sprite>("BG_VongXuyen_Forest_Hub");
#if UNITY_EDITOR
                    if (bgForest == null)
                        bgForest = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png");
#endif
                    if (bgForest != null)
                    {
                        GameObject backdropObj = new GameObject("Persistent_MetaBackdrop", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                        backdropObj.transform.SetParent(transform, false);
                        backdropObj.transform.SetAsFirstSibling();

                        RectTransform rt = backdropObj.GetComponent<RectTransform>();
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.sizeDelta = Vector2.zero;
                        rt.anchoredPosition = Vector2.zero;

                        var img = backdropObj.GetComponent<UnityEngine.UI.Image>();
                        img.color = Color.white;
                        img.raycastTarget = false;
                        img.sprite = bgForest;
                        _persistentBackdrop = backdropObj;
                    }
                }
            }

            if (_persistentBackdrop != null)
            {
                _persistentBackdrop.transform.SetAsFirstSibling();
                _persistentBackdrop.SetActive(true);
            }
        }

        private void Start()
        {
            AutoResolveMissingScreens();
            if (_mainHubScreen != null && (_screenStack.Count == 0 || _screenStack.Peek() != _mainHubScreen))
            {
                PushScreen(_mainHubScreen);
            }
        }

        private void Update()
        {
            // Bắt phím Escape / Back button trên điện thoại Android chuẩn New Input System
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleHardwareBackPressed();
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleHardwareBackPressed();
            }
#endif
        }

        public void PushScreen(BaseMetaScreenView screen)
        {
            if (screen == null)
            {
                Debug.LogError("[MetaUIManager] Không thể PushScreen vì Screen truyền vào bị NULL!");
                return;
            }

            Debug.Log($"[MetaUIManager] PushScreen: Đang hiển thị {screen.gameObject.name} (ScreenType: {screen.ScreenType})");

            // Tắt tất cả các màn hình khác để triệt tiêu hiện tượng đè giao diện
            var allScreens = new BaseMetaScreenView[]
            {
                _mainHubScreen, _characterSelectScreen, _weaponLoadoutScreen,
                _sanctuaryTreeScreen, _codexScreen, _settingsScreen,
                _gachaShopScreen, _stageSelectScreen, _resourceDownloadScreen
            };

            foreach (var s in allScreens)
            {
                if (s != null && s != screen)
                {
                    s.Hide();
                    s.gameObject.SetActive(false);
                }
            }

            if (_screenStack.Count == 0 || _screenStack.Peek() != screen)
            {
                _screenStack.Push(screen);
            }

            screen.gameObject.SetActive(true);
            screen.Show();
        }

        public void PopScreen()
        {
            if (_screenStack.Count > 1) // Giữ lại màn hình gốc (Main Hub)
            {
                var poppedScreen = _screenStack.Pop();
                if (poppedScreen != null)
                {
                    poppedScreen.Hide();
                    poppedScreen.gameObject.SetActive(false);
                }

                var previousScreen = _screenStack.Peek();
                if (previousScreen != null)
                {
                    previousScreen.gameObject.SetActive(true);
                    previousScreen.Show();
                }
            }
            else if (_screenStack.Count == 1 && _mainHubScreen != null)
            {
                _mainHubScreen.gameObject.SetActive(true);
                _mainHubScreen.Show();
            }
        }

        private void AutoResolveMissingScreens()
        {
            if (_mainHubScreen == null)
            {
                _mainHubScreen = GetComponentInChildren<MainHubView>(true);
                if (_mainHubScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/MainHubUI") ?? Resources.Load<GameObject>("MainHubUI");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Panel_MainHub";
                        _mainHubScreen = inst.GetComponent<MainHubView>();
                    }
                }
            }

            if (_characterSelectScreen == null)
            {
                _characterSelectScreen = GetComponentInChildren<CharacterSelectionView>(true);
                if (_characterSelectScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/CharacterSelectionUI") ?? Resources.Load<GameObject>("CharacterSelectionUI");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Panel_CharacterSelect";
                        _characterSelectScreen = inst.GetComponent<CharacterSelectionView>();
                    }
                }
            }

            if (_weaponLoadoutScreen == null)
            {
                _weaponLoadoutScreen = GetComponentInChildren<WeaponLoadoutView>(true);
                if (_weaponLoadoutScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/WeaponLoadoutUI") ?? Resources.Load<GameObject>("WeaponLoadoutUI");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Panel_WeaponLoadout";
                        _weaponLoadoutScreen = inst.GetComponent<WeaponLoadoutView>();
                    }
                }
            }

            if (_sanctuaryTreeScreen == null)
            {
                _sanctuaryTreeScreen = GetComponentInChildren<MetaUpgradeShopView>(true);
                if (_sanctuaryTreeScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/SanctuaryTreeUI") ?? Resources.Load<GameObject>("SanctuaryTreeUI");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Panel_SanctuaryTree";
                        _sanctuaryTreeScreen = inst.GetComponent<MetaUpgradeShopView>();
                    }
                }
            }

            if (_codexScreen == null)
            {
                _codexScreen = GetComponentInChildren<CardCodexView>(true);
                if (_codexScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/CardCodexUI") ?? Resources.Load<GameObject>("CardCodexUI");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Panel_CardCodex";
                        _codexScreen = inst.GetComponent<CardCodexView>();
                    }
                }
            }

            if (_settingsScreen == null)
            {
                _settingsScreen = GetComponentInChildren<SettingsModalView>(true);
                if (_settingsScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/SettingsModalUI") ?? Resources.Load<GameObject>("SettingsModalUI");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Modal_Settings";
                        _settingsScreen = inst.GetComponent<SettingsModalView>();
                    }
                }
            }

            if (_gachaShopScreen == null)
            {
                _gachaShopScreen = GetComponentInChildren<ProjectZombie.Features.UI.Gacha.GachaChestView>(true);
                if (_gachaShopScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/Gacha/GachaShopPanel") ?? Resources.Load<GameObject>("GachaShopPanel");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Panel_GachaShop";
                        _gachaShopScreen = inst.GetComponent<ProjectZombie.Features.UI.Gacha.GachaChestView>();
                    }
                }
            }

            if (_stageSelectScreen == null)
            {
                _stageSelectScreen = GetComponentInChildren<StageSelect.StageSelectUIView>(true);
                if (_stageSelectScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/StageSelect_Screen") ?? Resources.Load<GameObject>("StageSelect_Screen");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Screen_StageSelect";
                        _stageSelectScreen = inst.GetComponent<StageSelect.StageSelectUIView>();
                    }
                }
            }

            if (_resourceDownloadScreen == null)
            {
                _resourceDownloadScreen = GetComponentInChildren<ResourceDownload.ResourceDownloadModalView>(true);
                if (_resourceDownloadScreen == null)
                {
                    var p = Resources.Load<GameObject>("UI/ResourceDownloadModalUI") ?? Resources.Load<GameObject>("ResourceDownloadModalUI");
                    if (p != null)
                    {
                        var inst = Instantiate(p, transform);
                        inst.name = "Modal_ResourceDownload";
                        _resourceDownloadScreen = inst.GetComponent<ResourceDownload.ResourceDownloadModalView>();
                    }
                }
            }
        }

        public void OpenScreen(MetaScreenType screenType)
        {
            // Tự động đóng sub-screen đang mở trước khi chuyển sang màn hình mới
            while (_screenStack.Count > 1)
            {
                var popped = _screenStack.Pop();
                if (popped != null)
                {
                    popped.Hide();
                    popped.gameObject.SetActive(false);
                }
            }

            switch (screenType)
            {
                case MetaScreenType.MainHub:
                    if (_mainHubScreen == null) AutoResolveMissingScreens();
                    PushScreen(_mainHubScreen);
                    break;
                case MetaScreenType.CharacterSelect:
                    if (_characterSelectScreen == null) AutoResolveMissingScreens();
                    PushScreen(_characterSelectScreen);
                    break;
                case MetaScreenType.WeaponLoadout:
                    if (_weaponLoadoutScreen == null) AutoResolveMissingScreens();
                    PushScreen(_weaponLoadoutScreen);
                    break;
                case MetaScreenType.SanctuaryTree:
                    if (_sanctuaryTreeScreen == null) AutoResolveMissingScreens();
                    PushScreen(_sanctuaryTreeScreen);
                    break;
                case MetaScreenType.Codex:
                    if (_codexScreen == null) AutoResolveMissingScreens();
                    PushScreen(_codexScreen);
                    break;
                case MetaScreenType.Settings:
                    if (_settingsScreen == null) AutoResolveMissingScreens();
                    PushScreen(_settingsScreen);
                    break;
                case MetaScreenType.StageSelect:
                    if (_stageSelectScreen == null)
                    {
                        AutoResolveMissingScreens();
                        if (_stageSelectScreen == null)
                        {
                            var stagePrefab = Resources.Load<GameObject>("UI/StageSelect_Screen") ?? Resources.Load<GameObject>("StageSelect_Screen");
#if UNITY_EDITOR
                            if (stagePrefab == null)
                            {
                                stagePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/StageSelect_Screen.prefab");
                            }
#endif
                            if (stagePrefab != null)
                            {
                                var instance = Instantiate(stagePrefab, transform);
                                instance.name = "Screen_StageSelect";
                                _stageSelectScreen = instance.GetComponent<StageSelect.StageSelectUIView>();
                            }
                        }
                    }

                    if (_stageSelectScreen != null)
                    {
                        PushScreen(_stageSelectScreen);
                    }
                    else
                    {
                        Debug.LogWarning("[MetaUIManager] Không tìm thấy StageSelect_Screen trong Scene/Prefab. Tự động chuyển thẳng vào trận đấu!");
                        MetaSceneTransitionController.Instance?.StartRun();
                    }
                    break;
                case MetaScreenType.GachaShop:
                    if (_gachaShopScreen == null)
                    {
                        AutoResolveMissingScreens();
                        if (_gachaShopScreen == null)
                        {
                            // Tự động nạp Prefab nếu trong Scene chưa có sẵn
                            var gachaPrefab = Resources.Load<GameObject>("UI/Gacha/GachaShopPanel") ?? Resources.Load<GameObject>("GachaShopPanel");
#if UNITY_EDITOR
                            if (gachaPrefab == null)
                            {
                                gachaPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/Gacha/GachaShopPanel.prefab");
                            }
#endif
                            if (gachaPrefab != null)
                            {
                                var instance = Instantiate(gachaPrefab, transform);
                                instance.name = "Panel_GachaShop";
                                _gachaShopScreen = instance.GetComponent<ProjectZombie.Features.UI.Gacha.GachaChestView>();
                            }
                        }
                    }
                    PushScreen(_gachaShopScreen);
                    break;
                case MetaScreenType.ResourceDownload:
                    if (_resourceDownloadScreen == null)
                    {
                        AutoResolveMissingScreens();
                        if (_resourceDownloadScreen == null)
                        {
                            var resPrefab = Resources.Load<GameObject>("UI/ResourceDownloadModalUI") ?? Resources.Load<GameObject>("ResourceDownloadModalUI");
#if UNITY_EDITOR
                            if (resPrefab == null)
                            {
                                resPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/ResourceDownloadModalUI.prefab");
                            }
#endif
                            if (resPrefab != null)
                            {
                                var instance = Instantiate(resPrefab, transform);
                                instance.name = "Modal_ResourceDownload";
                                _resourceDownloadScreen = instance.GetComponent<ResourceDownload.ResourceDownloadModalView>();
                            }
                        }
                    }
                    if (_resourceDownloadScreen != null)
                    {
                        PushScreen(_resourceDownloadScreen);
                    }
                    break;
            }
        }

        public void HandleHardwareBackPressed()
        {
            if (!IsInMetaMenu) return;

            if (_screenStack.Count > 1)
            {
                var current = _screenStack.Peek();
                current.OnBackPressed();
            }
        }

        public void SetMetaCanvasActive(bool isActive)
        {
            if (_metaCanvasGroup != null)
            {
                _metaCanvasGroup.gameObject.SetActive(isActive);
                _metaCanvasGroup.alpha = isActive ? 1f : 0f;
                _metaCanvasGroup.blocksRaycasts = isActive;
                _metaCanvasGroup.interactable = isActive;
            }
        }
    }
}

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

            EnsurePersistentBackdrop();

            AutoResolveMissingScreens();

            // Mặc định ẩn tất cả màn hình phụ ngay trong Awake
            if (_characterSelectScreen != null) _characterSelectScreen.Hide();
            if (_weaponLoadoutScreen != null) _weaponLoadoutScreen.Hide();
            if (_sanctuaryTreeScreen != null) _sanctuaryTreeScreen.Hide();
            if (_codexScreen != null) _codexScreen.Hide();
            if (_settingsScreen != null) _settingsScreen.Hide();
            if (_gachaShopScreen != null) _gachaShopScreen.Hide();

            // Mở màn hình Sảnh Chính (Main Hub) đầu tiên
            if (_mainHubScreen != null)
            {
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
                    GameObject backdropObj = new GameObject("Persistent_MetaBackdrop", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                    backdropObj.transform.SetParent(transform, false);
                    backdropObj.transform.SetAsFirstSibling();

                    RectTransform rt = backdropObj.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    rt.anchoredPosition = Vector2.zero;

                    var img = backdropObj.GetComponent<UnityEngine.UI.Image>();
                    img.color = new Color(0.05f, 0.04f, 0.08f, 1.0f); // Tối sang trọng 100% không xuyên thấu
                    img.raycastTarget = true; // Chặn click lọt xuống map

                    Sprite bgForest = Resources.Load<Sprite>("UI/VongXuyen/BG_VongXuyen_Forest_Hub") ?? Resources.Load<Sprite>("BG_VongXuyen_Forest_Hub");
#if UNITY_EDITOR
                    if (bgForest == null)
                        bgForest = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png");
#endif
                    if (bgForest != null)
                    {
                        img.sprite = bgForest;
                        img.color = Color.white;
                    }
                    _persistentBackdrop = backdropObj;
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

            if (_screenStack.Count > 0)
            {
                var currentTop = _screenStack.Peek();
                if (currentTop == screen) return; // Đang ở màn hình này rồi
                Debug.Log($"[MetaUIManager] Ẩn màn hình cũ: {currentTop.gameObject.name}");
                currentTop.Hide();
            }

            _screenStack.Push(screen);
            screen.Show();
        }

        public void PopScreen()
        {
            if (_screenStack.Count > 1) // Giữ lại màn hình gốc (Main Hub)
            {
                var poppedScreen = _screenStack.Pop();
                poppedScreen.Hide();

                var previousScreen = _screenStack.Peek();
                if (previousScreen != null)
                {
                    previousScreen.Show();
                }
            }
        }

        private void AutoResolveMissingScreens()
        {
            if (_mainHubScreen == null) _mainHubScreen = GetComponentInChildren<MainHubView>(true);
            if (_characterSelectScreen == null) _characterSelectScreen = GetComponentInChildren<CharacterSelectionView>(true);
            if (_weaponLoadoutScreen == null) _weaponLoadoutScreen = GetComponentInChildren<WeaponLoadoutView>(true);
            if (_sanctuaryTreeScreen == null) _sanctuaryTreeScreen = GetComponentInChildren<MetaUpgradeShopView>(true);
            if (_codexScreen == null) _codexScreen = GetComponentInChildren<CardCodexView>(true);
            if (_settingsScreen == null) _settingsScreen = GetComponentInChildren<SettingsModalView>(true);
            if (_gachaShopScreen == null) _gachaShopScreen = GetComponentInChildren<ProjectZombie.Features.UI.Gacha.GachaChestView>(true);
        }

        public void OpenScreen(MetaScreenType screenType)
        {
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

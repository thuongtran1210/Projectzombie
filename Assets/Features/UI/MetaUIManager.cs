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

        [Header("Data-Driven UI Registry")]
        [Tooltip("Bảng đăng ký Prefab màn hình tập trung")]
        [SerializeField] private UIRegistrySO _uiRegistry;

        [Header("Persistent Backdrop")]
        [Tooltip("Ảnh nền cố định che 100% Tilemap và Player bên dưới khi ở trong Menu")]
        [SerializeField] private GameObject _persistentBackdrop;

        [Header("Cached Scene Screens (Optional - Pre-baked in Scene)")]
        [SerializeField] private BaseMetaScreenView _mainHubScreen;

        private readonly Stack<BaseMetaScreenView> _screenStack = new Stack<BaseMetaScreenView>();
        private UIScreenFactory _screenFactory;

        public bool IsInMetaMenu => _metaCanvasGroup != null && _metaCanvasGroup.gameObject.activeSelf;
        public BaseMetaScreenView WeaponLoadoutScreen => _screenFactory != null ? _screenFactory.GetOrCreateScreen(MetaScreenType.WeaponLoadout) : null;

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

            // Tự động fallback nạp UIRegistry từ Resources nếu Inspector chưa được kéo thả
            if (_uiRegistry == null)
            {
                _uiRegistry = Resources.Load<UIRegistrySO>("UI/UIRegistry");
            }

            // Khởi tạo UIScreenFactory chuyên trách nạp và quản lý vòng đời màn hình
            _screenFactory = new UIScreenFactory(_uiRegistry, transform);


            // Đăng ký các màn hình đã có sẵn trong Hierarchy của Scene vào Factory
            var existingInScene = GetComponentsInChildren<BaseMetaScreenView>(true);
            foreach (var scr in existingInScene)
            {
                _screenFactory.RegisterExistingScreen(scr);
                if (scr.ScreenType == MetaScreenType.MainHub)
                {
                    _mainHubScreen = scr;
                }
                else
                {
                    scr.gameObject.SetActive(false);
                }
            }

            // Đảm bảo backdrop hiển thị ở lớp dưới cùng
            if (_persistentBackdrop != null)
            {
                _persistentBackdrop.transform.SetAsFirstSibling();
                _persistentBackdrop.SetActive(true);
            }

            // Mở màn hình Sảnh Chính (Main Hub) đầu tiên
            if (_mainHubScreen == null)
            {
                _mainHubScreen = _screenFactory.GetOrCreateScreen(MetaScreenType.MainHub);
            }

            if (_mainHubScreen != null)
            {
                _mainHubScreen.gameObject.SetActive(true);
                PushScreen(_mainHubScreen);
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

            // Tắt tất cả các màn hình đã nạp khác để triệt tiêu hiện tượng đè giao diện
            if (_screenFactory != null)
            {
                foreach (var s in _screenFactory.GetAllLoadedScreens())
                {
                    if (s != null && s != screen && s.gameObject.activeSelf)
                    {
                        s.Hide();
                        s.gameObject.SetActive(false);
                    }
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

        /// <summary>
        /// Mở màn hình theo ScreenType thông qua UIScreenFactory (Hỗ trợ Lazy Loading mượt mà).
        /// </summary>
        public void OpenScreen(MetaScreenType screenType)
        {
            var totalSw = System.Diagnostics.Stopwatch.StartNew();

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

            if (_screenFactory == null)
            {
                _screenFactory = new UIScreenFactory(_uiRegistry, transform);
            }

            var targetScreen = _screenFactory.GetOrCreateScreen(screenType);
            if (targetScreen != null)
            {
                PushScreen(targetScreen);
                totalSw.Stop();
                Debug.Log($"<color=#FFFF00>[MetaUIManager.OpenScreen] TỔNG THỜI GIAN MỞ '{screenType}': {totalSw.ElapsedMilliseconds} ms (Bao gồm Factory, Awake, OnEnable, Render Grid)</color>");
            }
            else
            {
                totalSw.Stop();
                Debug.LogError($"[MetaUIManager] Không tìm thấy hoặc không thể nạp màn hình: {screenType}! (Thất bại sau {totalSw.ElapsedMilliseconds} ms)");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Editor Tool: Tự động phát hiện và liên kết màn hình vào Scene")]
        public void AutoResolveMissingScreensInEditor()
        {
            Debug.Log("<color=#00FF88>[MetaUIManager]</color> Đang quét và kiểm tra các màn hình trong Scene...");
            var screens = GetComponentsInChildren<BaseMetaScreenView>(true);
            foreach (var s in screens)
            {
                Debug.Log($"-> Phát hiện màn hình: {s.name} ({s.ScreenType})");
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

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

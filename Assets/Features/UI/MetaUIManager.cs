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
        [SerializeField] private BaseMetaScreenView _sanctuaryTreeScreen;
        [SerializeField] private BaseMetaScreenView _codexScreen;
        [SerializeField] private BaseMetaScreenView _settingsScreen;

        private readonly Stack<BaseMetaScreenView> _screenStack = new Stack<BaseMetaScreenView>();

        public bool IsInMetaMenu => _metaCanvasGroup != null && _metaCanvasGroup.gameObject.activeSelf;

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

            // Mặc định ẩn tất cả màn hình phụ ngay trong Awake
            if (_characterSelectScreen != null) _characterSelectScreen.Hide();
            if (_sanctuaryTreeScreen != null) _sanctuaryTreeScreen.Hide();
            if (_codexScreen != null) _codexScreen.Hide();
            if (_settingsScreen != null) _settingsScreen.Hide();

            // Mở màn hình Sảnh Chính (Main Hub) đầu tiên
            if (_mainHubScreen != null)
            {
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
            // Bắt phím Escape / Back button trên điện thoại Android
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleHardwareBackPressed();
            }
        }

        public void PushScreen(BaseMetaScreenView screen)
        {
            if (screen == null) return;

            if (_screenStack.Count > 0)
            {
                var currentTop = _screenStack.Peek();
                if (currentTop == screen) return; // Đang ở màn hình này rồi
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

        public void OpenScreen(MetaScreenType screenType)
        {
            switch (screenType)
            {
                case MetaScreenType.MainHub:
                    PushScreen(_mainHubScreen);
                    break;
                case MetaScreenType.CharacterSelect:
                    PushScreen(_characterSelectScreen);
                    break;
                case MetaScreenType.SanctuaryTree:
                    PushScreen(_sanctuaryTreeScreen);
                    break;
                case MetaScreenType.Codex:
                    PushScreen(_codexScreen);
                    break;
                case MetaScreenType.Settings:
                    PushScreen(_settingsScreen);
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

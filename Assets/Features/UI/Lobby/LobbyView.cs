using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Multiplayer.Core;

namespace ProjectZombie.Features.UI.Lobby
{
    /// <summary>
    /// View thụ động quản lý toàn bộ giao diện Phòng Chờ Đa Người Chơi (Lobby UI).
    /// Tuân thủ quy chuẩn MVP và tối ưu UI Android (AGENTS.md).
    /// </summary>
    public class LobbyView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.Lobby;

        [Header("Main Panels")]
        [SerializeField] private GameObject _entryPanel;
        [SerializeField] private GameObject _roomPanel;

        [Header("Entry Panel Controls")]
        [SerializeField] private TMP_InputField _roomCodeInputField;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinRoomButton;
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _closeXButton;

        [Header("Room Panel Controls")]
        [SerializeField] private TextMeshProUGUI _roomCodeDisplayText;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        [SerializeField] private Button _copyCodeButton;
        [SerializeField] private Button _readyButton;
        [SerializeField] private TextMeshProUGUI _readyButtonText;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _leaveRoomButton;

        [Header("Player Slots Container")]
        [SerializeField] private Transform _playerSlotsContainer;
        [SerializeField] private LobbyPlayerSlotView[] _playerSlots;

        [Header("Status & Notification")]
        [SerializeField] private TextMeshProUGUI _statusMessageText;

        public event Action OnCreateRoomClicked;
        public event Action<string> OnJoinRoomClicked;
        public event Action OnBackToMenuClicked;
        public event Action OnCopyCodeClicked;
        public event Action OnReadyToggleClicked;
        public event Action OnStartGameClicked;
        public event Action OnLeaveRoomClicked;

        protected override void Awake()
        {
            base.Awake();

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            SetupButtons();
        }

        public override void OnBackPressed()
        {
            if (_roomPanel != null && _roomPanel.activeSelf)
            {
                OnLeaveRoomClicked?.Invoke();
            }
            else
            {
                CloseAndReturnToHub();
            }
        }

        public void CloseAndReturnToHub()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
            else
            {
                Hide();
                gameObject.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            if (_createRoomButton != null)
            {
                _createRoomButton.onClick.AddListener(() => OnCreateRoomClicked?.Invoke());
            }

            if (_joinRoomButton != null)
            {
                _joinRoomButton.onClick.AddListener(() =>
                {
                    string code = _roomCodeInputField != null ? _roomCodeInputField.text : string.Empty;
                    OnJoinRoomClicked?.Invoke(code);
                });
            }

            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.AddListener(() => OnBackToMenuClicked?.Invoke());
            }

            if (_closeXButton != null)
            {
                _closeXButton.onClick.AddListener(() => OnBackToMenuClicked?.Invoke());
            }

            if (_copyCodeButton != null)
            {
                _copyCodeButton.onClick.AddListener(() => OnCopyCodeClicked?.Invoke());
            }

            if (_readyButton != null)
            {
                _readyButton.onClick.AddListener(() => OnReadyToggleClicked?.Invoke());
            }

            if (_startGameButton != null)
            {
                _startGameButton.onClick.AddListener(() => OnStartGameClicked?.Invoke());
            }

            if (_leaveRoomButton != null)
            {
                _leaveRoomButton.onClick.AddListener(() => OnLeaveRoomClicked?.Invoke());
            }
        }

        public void ShowEntryPanel()
        {
            if (_entryPanel != null) _entryPanel.SetActive(true);
            if (_roomPanel != null) _roomPanel.SetActive(false);
            ClearStatus();
        }

        public void ShowRoomPanel()
        {
            if (_entryPanel != null) _entryPanel.SetActive(false);
            if (_roomPanel != null) _roomPanel.SetActive(true);
            ClearStatus();
        }

        public void SetRoomCode(string code)
        {
            if (_roomCodeDisplayText != null)
            {
                _roomCodeDisplayText.text = $"<color=#888888>Mã Phòng:</color> <color=#FFD700><b>{code}</b></color>";
            }
        }

        public void SetPlayerCount(int current, int max)
        {
            if (_playerCountText != null)
            {
                _playerCountText.text = $"<color=#CCCCCC>Số Người:</color> <color=#00FF88>{current}/{max}</color>";
            }
        }

        public void SetHostControls(bool isHost, bool canStartGame)
        {
            if (_startGameButton != null)
            {
                _startGameButton.gameObject.SetActive(isHost);
                _startGameButton.interactable = canStartGame;
            }

            if (_readyButton != null)
            {
                // Host luôn luôn mặc định Ready nên có thể ẩn nút Ready cho Host hoặc đổi nhãn
                _readyButton.gameObject.SetActive(!isHost);
            }
        }

        public void SetReadyButtonState(bool isReady)
        {
            if (_readyButtonText != null)
            {
                _readyButtonText.text = isReady ? "<color=#FF4444>Hủy Sẵn Sàng</color>" : "<color=#00FF88>Sẵn Sàng</color>";
            }
        }

        public void UpdatePlayerList(IReadOnlyList<NetworkPlayerData> players)
        {
            if (_playerSlots == null || _playerSlots.Length == 0) return;

            int count = players != null ? players.Count : 0;
            for (int i = 0; i < _playerSlots.Length; i++)
            {
                if (i < count)
                {
                    _playerSlots[i].Bind(players[i]);
                }
                else
                {
                    _playerSlots[i].Bind(null);
                }
            }
        }

        public void SetStatusMessage(string message, bool isError = false)
        {
            if (_statusMessageText != null)
            {
                string color = isError ? "#FF4444" : "#00FF88";
                _statusMessageText.text = $"<color={color}>{message}</color>";
            }
        }

        public void ClearStatus()
        {
            if (_statusMessageText != null)
            {
                _statusMessageText.text = string.Empty;
            }
        }
    }
}

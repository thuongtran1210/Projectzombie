using System;
using UnityEngine;
using ProjectZombie.Features.Multiplayer.Core;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.UI.Lobby
{
    /// <summary>
    /// Presenter điều phối toàn bộ luồng Phòng Chờ Đa Người Chơi (Lobby Presenter).
    /// Kết nối INetworkSessionService với LobbyView theo chuẩn MVP (AGENTS.md).
    /// </summary>
    public class LobbyPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private LobbyView _view;

        private INetworkSessionService _sessionService;
        private bool _isLocalReady = false;

        public event Action OnMatchStartedTransition;

        private void Start()
        {
            // Lấy Session Service từ ServiceContext hoặc tìm PhotonFusionSessionService, fallback sang Mock
            if (!ServiceContext.TryGet(out _sessionService))
            {
                var photonService = GetComponent<PhotonFusionSessionService>();
                if (photonService == null)
                {
                    photonService = FindObjectOfType<PhotonFusionSessionService>();
                }

                if (photonService != null)
                {
                    _sessionService = photonService;
                }
                else
                {
                    _sessionService = new MockNetworkSessionService();
                }

                ServiceContext.Register(_sessionService);
            }

            SubscribeViewEvents();
            SubscribeSessionEvents();

            // Mặc định mở màn hình nhập/tạo phòng
            if (_view != null)
            {
                _view.ShowEntryPanel();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeViewEvents();
            UnsubscribeSessionEvents();
        }

        private void SubscribeViewEvents()
        {
            if (_view == null) return;

            _view.OnCreateRoomClicked += HandleCreateRoom;
            _view.OnJoinRoomClicked += HandleJoinRoom;
            _view.OnBackToMenuClicked += HandleBackToMenu;
            _view.OnCopyCodeClicked += HandleCopyCode;
            _view.OnReadyToggleClicked += HandleToggleReady;
            _view.OnStartGameClicked += HandleStartGame;
            _view.OnLeaveRoomClicked += HandleLeaveRoom;
        }

        private void UnsubscribeViewEvents()
        {
            if (_view == null) return;

            _view.OnCreateRoomClicked -= HandleCreateRoom;
            _view.OnJoinRoomClicked -= HandleJoinRoom;
            _view.OnBackToMenuClicked -= HandleBackToMenu;
            _view.OnCopyCodeClicked -= HandleCopyCode;
            _view.OnReadyToggleClicked -= HandleToggleReady;
            _view.OnStartGameClicked -= HandleStartGame;
            _view.OnLeaveRoomClicked -= HandleLeaveRoom;
        }

        private void SubscribeSessionEvents()
        {
            if (_sessionService == null) return;

            _sessionService.OnRoomUpdated += HandleRoomUpdated;
            _sessionService.OnMatchStarted += HandleMatchStarted;
            _sessionService.OnConnectionError += HandleConnectionError;
        }

        private void UnsubscribeSessionEvents()
        {
            if (_sessionService == null) return;

            _sessionService.OnRoomUpdated -= HandleRoomUpdated;
            _sessionService.OnMatchStarted -= HandleMatchStarted;
            _sessionService.OnConnectionError -= HandleConnectionError;
        }

        private async void HandleCreateRoom()
        {
            if (_view != null) _view.SetStatusMessage("Đang khởi tạo phòng Host...");

            bool success = await _sessionService.CreateHostSessionAsync();
            if (!success && _view != null)
            {
                _view.SetStatusMessage("Không thể tạo phòng. Vui lòng thử lại!", isError: true);
            }
        }

        private async void HandleJoinRoom(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                if (_view != null) _view.SetStatusMessage("Vui lòng nhập mã phòng 6 ký tự!", isError: true);
                return;
            }

            if (_view != null) _view.SetStatusMessage("Đang kết nối vào phòng...");

            bool success = await _sessionService.JoinSessionAsync(roomCode);
            if (!success && _view != null)
            {
                _view.SetStatusMessage("Vào phòng thất bại. Kiểm tra lại mã phòng!", isError: true);
            }
        }

        private void HandleBackToMenu()
        {
            if (_sessionService != null && _sessionService.IsInRoom)
            {
                HandleLeaveRoom();
            }
            if (_view != null)
            {
                _view.CloseAndReturnToHub();
            }
        }

        private void HandleCopyCode()
        {
            if (_sessionService?.CurrentRoom != null)
            {
                GUIUtility.systemCopyBuffer = _sessionService.CurrentRoom.RoomCode;
                if (_view != null) _view.SetStatusMessage("Đã sao chép mã phòng vào bộ nhớ tạm!");
            }
        }

        private void HandleToggleReady()
        {
            _isLocalReady = !_isLocalReady;
            _sessionService.SetLocalPlayerReady(_isLocalReady);
            if (_view != null) _view.SetReadyButtonState(_isLocalReady);
        }

        private async void HandleStartGame()
        {
            if (_sessionService == null || !_sessionService.IsHost) return;

            if (_view != null) _view.SetStatusMessage("Đang chuẩn bị vào trận...");
            await _sessionService.StartGameMatchAsync();
        }

        private async void HandleLeaveRoom()
        {
            if (_sessionService == null) return;

            await _sessionService.LeaveSessionAsync();
            _isLocalReady = false;
            if (_view != null)
            {
                _view.SetReadyButtonState(false);
                _view.ShowEntryPanel();
            }
        }

        private void HandleRoomUpdated(NetworkRoomInfo room)
        {
            if (_view == null) return;

            if (room == null)
            {
                _view.ShowEntryPanel();
                return;
            }

            _view.ShowRoomPanel();
            _view.SetRoomCode(room.RoomCode);
            _view.SetPlayerCount(room.CurrentPlayerCount, room.MaxPlayers);
            _view.UpdatePlayerList(room.Players);

            // Kiểm tra điều kiện bắt đầu trận của Host: Tất cả Client khác đều đã Ready
            bool allClientsReady = true;
            for (int i = 0; i < room.Players.Count; i++)
            {
                var p = room.Players[i];
                if (!p.IsHost && !p.IsReady)
                {
                    allClientsReady = false;
                    break;
                }
            }

            _view.SetHostControls(_sessionService.IsHost, canStartGame: allClientsReady);
        }

        private void HandleMatchStarted()
        {
            if (_view != null)
            {
                _view.SetStatusMessage("<color=#00FF88>Trận đấu bắt đầu! Đang tải bản đồ...</color>");
                _view.Hide();
                _view.gameObject.SetActive(false);
            }

            OnMatchStartedTransition?.Invoke();

            // 1. Nếu có MetaSceneTransitionController, chuyển cảnh mượt mà
            if (MetaSceneTransitionController.Instance != null)
            {
                MetaSceneTransitionController.Instance.TransitionToCombat(null);
            }
            else
            {
                // Fallback: Ẩn Meta Canvas và mở Gameplay Canvas
                if (MetaUIManager.Instance != null)
                {
                    MetaUIManager.Instance.SetMetaCanvasActive(false);
                }
                if (GameplayUIManager.Instance != null)
                {
                    GameplayUIManager.Instance.SetGameplayCanvasActive(true);
                }

                // Nếu GameplayBootstrapper có sẵn trong scene, chuyển trạng thái sang Playing
                if (Player.GameplayBootstrapper.Instance != null)
                {
                    Player.GameplayBootstrapper.Instance.StartMatchFlow();
                }
            }
        }

        private void HandleConnectionError(string errorMessage)
        {
            if (_view != null)
            {
                _view.SetStatusMessage(errorMessage, isError: true);
            }
        }
    }
}

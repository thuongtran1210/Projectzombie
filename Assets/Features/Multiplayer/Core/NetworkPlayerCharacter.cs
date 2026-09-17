using UnityEngine;
using Fusion;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Player.Input;
using ProjectZombie.Features.Arena;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Component NetworkBehaviour gắn trực tiếp lên Player Prefab để điều phối trạng thái Local / Remote trong phòng mạng.
    /// Tự động thiết lập Camera, UI Binder, Input Reader cho máy cục bộ hoặc Input Bridge cho máy khác.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkPlayerCharacter : NetworkBehaviour
    {
        private PlayerController _controller;
        private PlayerInputReader _localInputReader;
        private NetworkInputBridge _networkInputBridge;
        private PlayerContext _playerContext;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _localInputReader = GetComponent<PlayerInputReader>();
            _networkInputBridge = GetComponent<NetworkInputBridge>();
            if (_networkInputBridge == null)
            {
                _networkInputBridge = gameObject.AddComponent<NetworkInputBridge>();
            }
        }

        public override void Spawned()
        {
            int playerId = Object.InputAuthority.PlayerId;
            bool isLocal = Object.HasInputAuthority;

            _playerContext = PlayerContext.Create(gameObject, isLocal: isLocal, playerId: playerId);

            // Đăng ký vào PlayerRegistry tập trung (MultiplayerPlayerRegistry)
            if (ServiceContext.TryGet<IPlayerRegistry>(out var registry))
            {
                registry.Register(_playerContext);
            }

            if (isLocal)
            {
                // 1. TỰ ĐỘNG CHUYỂN GIAO DIỆN SANG GAMEPLAY HUD CHO CLIENT
                if (ProjectZombie.Features.UI.MetaUIManager.Instance != null)
                {
                    ProjectZombie.Features.UI.MetaUIManager.Instance.SetMetaCanvasActive(false);
                }
                if (ProjectZombie.Features.UI.GameplayUIManager.Instance != null)
                {
                    ProjectZombie.Features.UI.GameplayUIManager.Instance.SetGameplayCanvasActive(true);
                }
                if (ProjectZombie.Features.Shared.GameStateManager.Instance != null)
                {
                    ProjectZombie.Features.Shared.GameStateManager.Instance.ChangeState(ProjectZombie.Features.Shared.GameState.Playing);
                }

                // 2. CẤU HÌNH CHO NGƯỜI CHƠI CỤC BỘ (LOCAL PLAYER)
                if (_localInputReader != null)
                {
                    _localInputReader.enabled = true;
                    _controller.SetInputProvider(_localInputReader);
                }

                _networkInputBridge.enabled = false;

                // Kết nối Camera theo dõi
                var cameraFollow = CameraFollow.Instance;
                if (cameraFollow != null)
                {
                    cameraFollow.SetTarget(transform);
                    cameraFollow.ResetZoom(0.1f);
                }

                // Đăng ký PlayerProvider toàn cục
                PlayerProvider.RegisterPlayer(gameObject);

                Debug.Log($"<color=#00FF88>[NetworkPlayerCharacter]</color> Khởi tạo Local Player thành công (PlayerId #{playerId}) và đã chuyển sang Gameplay HUD.");
            }
            else
            {
                // 2. CẤU HÌNH CHO ĐỒNG ĐỘI TỪ XA (REMOTE PROXY)
                if (_localInputReader != null)
                {
                    _localInputReader.enabled = false;
                }

                _networkInputBridge.enabled = true;
                _controller.SetInputProvider(_networkInputBridge);

                Debug.Log($"<color=#FFFF00>[NetworkPlayerCharacter]</color> Khởi tạo Remote Proxy thành công (PlayerId #{playerId}).");
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (ServiceContext.TryGet<IPlayerRegistry>(out var registry) && _playerContext != null)
            {
                registry.Unregister(_playerContext);
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Đọc Input mạng từ Photon Runner và nạp vào NetworkInputBridge
            if (GetInput<NetworkInputData>(out var networkInput))
            {
                if (_networkInputBridge != null && _networkInputBridge.enabled)
                {
                    _networkInputBridge.ApplyNetworkInput(networkInput);
                }
            }
        }
    }
}

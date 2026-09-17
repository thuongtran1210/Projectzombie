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

            // Nếu là Scene Object chưa được gán InputAuthority trong phòng Multiplayer (PlayerId #-1)
            if (Object.InputAuthority == PlayerRef.None && Runner != null && Runner.IsRunning)
            {
                Debug.Log($"<color=#AAAAAA>[NetworkPlayerCharacter]</color> Ẩn Scene Object tĩnh (PlayerId #-1) để nhường quyền cho NetworkPlayerSpawner sinh nhân vật có Authority.");
                gameObject.SetActive(false);
                return;
            }

            _playerContext = PlayerContext.Create(gameObject, isLocal: isLocal, playerId: playerId);

            // Đăng ký vào PlayerRegistry tập trung (MultiplayerPlayerRegistry)
            if (ServiceContext.TryGet<IPlayerRegistry>(out var registry))
            {
                registry.Register(_playerContext);
            }

            if (_controller != null)
            {
                _controller.SetNetworkMovementMode(true);
            }

            if (isLocal)
            {
                // 1. CẤU HÌNH CHO NGƯỜI CHƠI CỤC BỘ (LOCAL PLAYER)
                if (_localInputReader != null)
                {
                    _localInputReader.enabled = true;
                    _localInputReader.IsNetworkMode = true;
                    _controller.SetInputProvider(_localInputReader);
                }

                _networkInputBridge.enabled = false;

                // Đăng ký PlayerProvider toàn cục (CameraFollow tự động lắng nghe OnPlayerSpawned)
                PlayerProvider.RegisterPlayer(gameObject);

                Debug.Log($"<color=#00FF88>[NetworkPlayerCharacter]</color> Khởi tạo Local Player thành công (PlayerId #{playerId}).");
            }
            else
            {
                // 2. CẤU HÌNH CHO ĐỒNG ĐỘI TỪ XA (REMOTE PROXY)
                if (_localInputReader != null)
                {
                    _localInputReader.enabled = false;
                    _localInputReader.IsNetworkMode = false;
                }

                _networkInputBridge.enabled = true;
                _controller.SetInputProvider(_networkInputBridge);

                Debug.Log($"<color=#FFFF00>[NetworkPlayerCharacter]</color> Khởi tạo Remote Proxy thành công (PlayerId #{playerId}).");
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_controller != null)
            {
                _controller.SetNetworkMovementMode(false);
            }

            if (_localInputReader != null)
            {
                _localInputReader.IsNetworkMode = false;
            }

            if (ServiceContext.TryGet<IPlayerRegistry>(out var registry) && _playerContext != null)
            {
                registry.Unregister(_playerContext);
            }
        }

        private NetworkInputButtons _previousButtons = NetworkInputButtons.None;

        // Command Registry cho các nút bấm mạng (Command Pattern & OCP)
        private static readonly System.Collections.Generic.KeyValuePair<NetworkInputButtons, Commands.INetworkActionCommand>[] _actionCommands =
        {
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, Commands.INetworkActionCommand>(NetworkInputButtons.Dash, new Commands.DashActionCommand()),
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, Commands.INetworkActionCommand>(NetworkInputButtons.Attack, new Commands.AttackActionCommand()),
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, Commands.INetworkActionCommand>(NetworkInputButtons.SignatureSkill, new Commands.SignatureSkillActionCommand()),
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, Commands.INetworkActionCommand>(NetworkInputButtons.RelicSkill, new Commands.RelicSkillActionCommand()),
        };

        public override void FixedUpdateNetwork()
        {
            // Điều phối di chuyển và hành động Network-Authoritative trên State Authority (Host)
            if (GetInput<NetworkInputData>(out var networkInput))
            {
                if (Object.HasStateAuthority)
                {
                    if (_controller != null)
                    {
                        _controller.ApplyNetworkMovement(networkInput.MoveDirection);
                    }

                    ProcessNetworkActions(networkInput.Buttons, networkInput.AimDirection);
                }

                if (_networkInputBridge != null && _networkInputBridge.enabled)
                {
                    _networkInputBridge.ApplyNetworkInput(networkInput);
                }
            }
            else
            {
                if (Object.HasStateAuthority)
                {
                    if (_controller != null)
                    {
                        _controller.ApplyNetworkMovement(Vector2.zero);
                    }
                    _previousButtons = NetworkInputButtons.None;
                }
            }
        }

        private void ProcessNetworkActions(NetworkInputButtons currentButtons, Vector2 aimDirection)
        {
            // Duyệt qua Command Registry kiểm tra Rising Edge (0 -> 1) và thực thi (0 GC allocations)
            for (int i = 0; i < _actionCommands.Length; i++)
            {
                var kvp = _actionCommands[i];
                if ((currentButtons & kvp.Key) != 0 && (_previousButtons & kvp.Key) == 0)
                {
                    kvp.Value.Execute(gameObject, aimDirection);
                }
            }

            _previousButtons = currentButtons;
        }
    }
}

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

                // Kết nối Camera theo dõi
                var cameraFollow = CameraFollow.Instance;
                if (cameraFollow != null)
                {
                    cameraFollow.SetTarget(transform);
                    cameraFollow.ResetZoom(0.1f);
                }

                // Đăng ký PlayerProvider toàn cục
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
            // Phát hiện Rising Edge (Cạnh lên: 0 -> 1) để kích hoạt đòn đánh / skill / lướt 1 lần duy nhất trên Host
            if ((currentButtons & NetworkInputButtons.Dash) != 0 && (_previousButtons & NetworkInputButtons.Dash) == 0)
            {
                if (_controller != null) _controller.PerformDash();
            }

            if ((currentButtons & NetworkInputButtons.Attack) != 0 && (_previousButtons & NetworkInputButtons.Attack) == 0)
            {
                var combat = GetComponent<CharacterCombat>();
                if (combat != null)
                {
                    if (aimDirection.sqrMagnitude > 0.001f)
                    {
                        combat.TriggerAttack(aimDirection);
                    }
                    else
                    {
                        combat.TriggerAttack();
                    }
                }
                else
                {
                    var wm = GetComponent<Weapons.WeaponManager>();
                    if (wm != null) wm.TriggerPrimaryAttack();
                }
            }

            if ((currentButtons & NetworkInputButtons.SignatureSkill) != 0 && (_previousButtons & NetworkInputButtons.SignatureSkill) == 0)
            {
                if (aimDirection.sqrMagnitude > 0.001f)
                {
                    var anim = GetComponentInChildren<PlayerAnimator>();
                    if (anim != null) anim.FlipToDirection(aimDirection.x);
                }

                var sig = GetComponent<Player.Skills.SignatureSkillManager>();
                if (sig != null) sig.TryExecuteSkill();
            }

            if ((currentButtons & NetworkInputButtons.RelicSkill) != 0 && (_previousButtons & NetworkInputButtons.RelicSkill) == 0)
            {
                var wm = GetComponent<Weapons.WeaponManager>();
                if (wm != null)
                {
                    if (aimDirection.sqrMagnitude > 0.001f)
                    {
                        wm.TriggerEquippedRelicSkill(aimDirection);
                    }
                    else
                    {
                        wm.TriggerEquippedRelicSkill();
                    }
                }
            }

            _previousButtons = currentButtons;
        }
    }
}

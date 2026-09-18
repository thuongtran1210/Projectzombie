using UnityEngine;
using Fusion;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Player.Input;
using ProjectZombie.Features.Player.Mechanics;
using ProjectZombie.Features.Shared;
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
        [Networked]
        public NetworkBool NetworkIsDowned { get; set; }

        [Networked]
        public float NetworkHealth { get; set; }

        [Networked]
        public float NetworkMaxHealth { get; set; }

        [Networked]
        public NetworkString<_32> NetworkEquippedWeaponId { get; set; }

        private PlayerController _controller;
        private PlayerInputReader _localInputReader;
        private NetworkInputBridge _networkInputBridge;
        private PlayerAnimator _playerAnimator;
        private HealthSystem _healthSystem;
        private Weapons.WeaponManager _weaponManager;
        private Combat.Coop.CoopDownedMechanic _downedMechanic;
        private PlayerContext _playerContext;
        private Vector3 _lastRenderPosition;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _localInputReader = GetComponent<PlayerInputReader>();
            _playerAnimator = GetComponentInChildren<PlayerAnimator>();
            _healthSystem = GetComponent<HealthSystem>();
            _weaponManager = GetComponent<Weapons.WeaponManager>();

            _networkInputBridge = GetComponent<NetworkInputBridge>();
            if (_networkInputBridge == null)
            {
                _networkInputBridge = gameObject.AddComponent<NetworkInputBridge>();
            }

            if (!TryGetComponent<Combat.Coop.CoopDownedMechanic>(out _downedMechanic))
            {
                _downedMechanic = gameObject.AddComponent<Combat.Coop.CoopDownedMechanic>();
            }

            if (!TryGetComponent<CoopTeamExperience>(out _))
            {
                gameObject.AddComponent<CoopTeamExperience>();
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

            // Đảm bảo nhân vật luôn ở trạng thái sống khỏe mạnh khi Spawn mới (lượt 1 hoặc các lượt chơi tiếp theo)
            if (Object.HasStateAuthority)
            {
                NetworkIsDowned = false;
                if (_healthSystem != null)
                {
                    NetworkHealth = _healthSystem.CurrentHealth;
                    NetworkMaxHealth = _healthSystem.MaxHealth;
                    _healthSystem.OnHealthChanged += HandleHostHealthChanged;
                }
            }

            if (_downedMechanic != null)
            {
                _downedMechanic.ResetDownedState();
            }
            if (_healthSystem != null)
            {
                _healthSystem.ResetHealth();
            }
            if (_playerAnimator != null)
            {
                _playerAnimator.ChangeAnimationState(PlayerAnimationState.Idle);
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

                // Gửi thông tin vũ khí/pháp bảo của Local Player lên Host
                string relicId = "";
                if (RunLoadoutState.SelectedRelic != null && !string.IsNullOrEmpty(RunLoadoutState.SelectedRelic.weaponId))
                {
                    relicId = RunLoadoutState.SelectedRelic.weaponId;
                }
                else if (_weaponManager != null && _weaponManager.EquippedRelic != null)
                {
                    relicId = _weaponManager.EquippedRelic.weaponId;
                }

                if (!string.IsNullOrEmpty(relicId))
                {
                    RpcSetEquippedWeapon(relicId);
                }

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

        private void HandleHostHealthChanged(float current, float max)
        {
            if (Object.HasStateAuthority)
            {
                NetworkHealth = current;
                NetworkMaxHealth = max;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RpcSetEquippedWeapon(string weaponId)
        {
            NetworkEquippedWeaponId = weaponId;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Object.HasStateAuthority && _healthSystem != null)
            {
                _healthSystem.OnHealthChanged -= HandleHostHealthChanged;
            }
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
            bool isDowned = (_downedMechanic != null && _downedMechanic.IsDowned) || NetworkIsDowned;
            bool isDead = _healthSystem != null && !_healthSystem.IsAlive;

            if (isDowned || isDead)
            {
                if (_controller != null)
                {
                    _controller.ApplyNetworkMovement(Vector2.zero);
                }
                _previousButtons = NetworkInputButtons.None;
                return; // Cấm nhận input, di chuyển, lướt và tấn công khi đã gục ngã hoặc chết
            }

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
                else if (Object.HasInputAuthority)
                {
                    // CLIENT-SIDE PREDICTION:
                    // Trên máy Client sở hữu nhân vật, áp dụng di chuyển cục bộ ngay lập tức để triệt tiêu độ trễ mạng
                    if (_controller != null)
                    {
                        _controller.ApplyNetworkMovement(networkInput.MoveDirection);
                    }
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

        public override void Render()
        {
            if (NetworkIsDowned)
            {
                if (_playerAnimator != null && _playerAnimator.CurrentState != PlayerAnimationState.Dead)
                {
                    _playerAnimator.ChangeAnimationState(PlayerAnimationState.Dead);
                }
                if (_downedMechanic != null && !_downedMechanic.IsDowned)
                {
                    _downedMechanic.EnterDownedState();
                }
                return;
            }
            else
            {
                if (_downedMechanic != null && _downedMechanic.IsDowned)
                {
                    _downedMechanic.CompleteRevive();
                }
            }

            // Đồng bộ hóa Animation, Hướng quay mặt, Máu và Pháp Bảo cho Đồng đội từ xa (Remote Proxy)
            if (!Object.HasInputAuthority)
            {
                if (_healthSystem != null)
                {
                    if (NetworkMaxHealth > 0 && Mathf.Abs(_healthSystem.MaxHealth - NetworkMaxHealth) > 0.5f)
                    {
                        _healthSystem.SetMaxHealth(NetworkMaxHealth, fillCurrentHealth: false);
                    }

                    if (Mathf.Abs(_healthSystem.CurrentHealth - NetworkHealth) > 0.5f)
                    {
                        _healthSystem.SetCurrentHealth(NetworkHealth);
                    }
                }

                if (_weaponManager != null)
                {
                    string netWeaponId = NetworkEquippedWeaponId.ToString();
                    if (!string.IsNullOrEmpty(netWeaponId))
                    {
                        var equipped = _weaponManager.EquippedRelic;
                        if (equipped == null || equipped.weaponId != netWeaponId)
                        {
                            _weaponManager.EquipWeaponById(netWeaponId, isPrimary: false);
                        }
                    }
                }

                if (_playerAnimator != null)
                {
                    Vector3 currentPos = transform.position;
                    Vector3 delta = currentPos - _lastRenderPosition;
                    float distSqr = delta.sqrMagnitude;
                    _lastRenderPosition = currentPos;

                    // Nếu có dịch chuyển đáng kể trong frame (đang chạy), bật Run và Flip hướng
                    if (distSqr > 0.0001f)
                    {
                        _playerAnimator.ChangeAnimationState(PlayerAnimationState.Run);
                        if (Mathf.Abs(delta.x) > 0.001f)
                        {
                            _playerAnimator.FlipToDirection(delta.x);
                        }
                    }
                    else
                    {
                        _playerAnimator.ChangeAnimationState(PlayerAnimationState.Idle);
                    }
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcSetDownedState(NetworkBool isDowned)
        {
            NetworkIsDowned = isDowned;
        }

        public void SetDownedState(bool isDowned)
        {
            if (Object != null && Object.IsValid)
            {
                if (Object.HasStateAuthority)
                {
                    NetworkIsDowned = isDowned;
                }
                else
                {
                    RpcSetDownedState(isDowned);
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

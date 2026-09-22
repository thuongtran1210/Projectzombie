using UnityEngine;
using Fusion;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Player.Mechanics;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Multiplayer.Core.Components;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Coordinator NetworkBehaviour gắn trực tiếp lên Player Prefab để điều phối vòng đời và trạng thái Local / Remote.
    /// Tuân thủ Single Responsibility (SRP): Ủy quyền chi tiết cho NetworkPlayerMovement, NetworkPlayerCombat,
    /// NetworkPlayerVitals, NetworkPlayerLoadout và NetworkPlayerPresentation (Mục 3.1 AGENTS.md).
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkPlayerMovement))]
    [RequireComponent(typeof(NetworkPlayerCombat))]
    [RequireComponent(typeof(NetworkPlayerVitals))]
    [RequireComponent(typeof(NetworkPlayerLoadout))]
    [RequireComponent(typeof(NetworkPlayerPresentation))]
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

        // Modular Sub-components
        private NetworkPlayerMovement _movement;
        private NetworkPlayerCombat _combat;
        private NetworkPlayerVitals _vitals;
        private NetworkPlayerLoadout _loadout;
        private NetworkPlayerPresentation _presentation;
        private PlayerContext _playerContext;

        private void Awake()
        {
            _movement = GetComponent<NetworkPlayerMovement>() ?? gameObject.AddComponent<NetworkPlayerMovement>();
            _combat = GetComponent<NetworkPlayerCombat>() ?? gameObject.AddComponent<NetworkPlayerCombat>();
            _vitals = GetComponent<NetworkPlayerVitals>() ?? gameObject.AddComponent<NetworkPlayerVitals>();
            _loadout = GetComponent<NetworkPlayerLoadout>() ?? gameObject.AddComponent<NetworkPlayerLoadout>();
            _presentation = GetComponent<NetworkPlayerPresentation>() ?? gameObject.AddComponent<NetworkPlayerPresentation>();

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

            _movement.SetupAuthority(isLocal);

            // Đảm bảo nhân vật luôn ở trạng thái sống khỏe mạnh khi Spawn mới
            if (Object.HasStateAuthority)
            {
                NetworkIsDowned = false;
                if (_vitals.HealthSystem != null)
                {
                    NetworkHealth = _vitals.HealthSystem.CurrentHealth;
                    NetworkMaxHealth = _vitals.HealthSystem.MaxHealth;
                    _vitals.HealthSystem.OnHealthChanged += HandleHostHealthChanged;
                }
            }

            _vitals.ResetVitals();
            _presentation.ResetToIdle();

            if (isLocal)
            {
                // Đăng ký PlayerProvider toàn cục (CameraFollow tự động lắng nghe OnPlayerSpawned)
                PlayerProvider.RegisterPlayer(gameObject);

                // Gửi thông tin vũ khí/pháp bảo của Local Player lên Host
                string relicId = _loadout.GetLocalEquippedRelicId();
                if (!string.IsNullOrEmpty(relicId))
                {
                    RpcSetEquippedWeapon(relicId);
                }

                Debug.Log($"<color=#00FF88>[NetworkPlayerCharacter]</color> Khởi tạo Local Player thành công (PlayerId #{playerId}).");
            }
            else
            {
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
            if (Object.HasStateAuthority && _vitals.HealthSystem != null)
            {
                _vitals.HealthSystem.OnHealthChanged -= HandleHostHealthChanged;
            }

            _movement.ResetMovementMode();

            if (ServiceContext.TryGet<IPlayerRegistry>(out var registry) && _playerContext != null)
            {
                registry.Unregister(_playerContext);
            }
        }

        public override void FixedUpdateNetwork()
        {
            bool isDowned = _vitals.IsDowned || NetworkIsDowned;
            bool isDead = _vitals.IsDead;

            if (isDowned || isDead)
            {
                _movement.ApplyMovement(Vector2.zero);
                _combat.ResetButtons();
                return; // Cấm nhận input, di chuyển, lướt và tấn công khi đã gục ngã hoặc chết
            }

            // Điều phối di chuyển và hành động Network-Authoritative trên State Authority (Host)
            if (GetInput<NetworkInputData>(out var networkInput))
            {
                if (Object.HasStateAuthority)
                {
                    _movement.ApplyMovement(networkInput.MoveDirection);
                    _combat.ProcessNetworkActions(networkInput.Buttons, networkInput.AimDirection);
                }
                else if (Object.HasInputAuthority)
                {
                    // CLIENT-SIDE PREDICTION: Áp dụng di chuyển cục bộ ngay lập tức để triệt tiêu độ trễ mạng
                    _movement.ApplyMovement(networkInput.MoveDirection);
                }

                _movement.FeedInputBridge(networkInput);
            }
            else
            {
                if (Object.HasStateAuthority)
                {
                    _movement.ApplyMovement(Vector2.zero);
                    _combat.ResetButtons();
                }
            }
        }

        public override void Render()
        {
            if (NetworkIsDowned)
            {
                _presentation.ForceDeadAnimation();
                _vitals.UpdateDownedState(true);
                return;
            }
            else
            {
                _vitals.UpdateDownedState(false);
            }

            // Đồng bộ hóa Animation, Máu và Pháp Bảo cho Đồng đội từ xa (Remote Proxy)
            if (!Object.HasInputAuthority)
            {
                _vitals.SyncRemoteHealth(NetworkHealth, NetworkMaxHealth);
                _loadout.SyncRemoteWeapon(NetworkEquippedWeaponId.ToString());
                _presentation.UpdateRemoteVisuals();
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
    }
}

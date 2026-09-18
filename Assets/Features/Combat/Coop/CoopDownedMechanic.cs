using System;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.Features.Combat.Coop
{
    /// <summary>
    /// Cơ chế Gục Ngã (Downed) & Cứu Trợ Hồi Sinh (Revive) trong chế độ Multiplayer Co-op.
    /// Khi nhân vật hết máu, không chết ngay mà chuyển sang trạng thái chờ đồng đội cứu.
    /// Trận đấu chỉ kết thúc khi TOÀN BỘ người chơi trong phòng đều gục ngã (Team Wipe).
    /// </summary>
    public class CoopDownedMechanic : MonoBehaviour, IDownedStateProvider
    {
        [Header("Co-op Revive Settings")]
        [Tooltip("Bán kính đồng đội cần đứng gần để hồi sinh (mét)")]
        [SerializeField] private float _reviveRadius = 2.5f;

        [Tooltip("Thời gian đứng cứu để hồi sinh hoàn toàn (giây)")]
        [SerializeField] private float _reviveDurationRequired = 3.0f;

        [Tooltip("Tỷ lệ phần trăm máu hồi phục sau khi được cứu sống")]
        [Range(0.1f, 1.0f)]
        [SerializeField] private float _reviveHealthPercentage = 0.35f;

        [Header("Visual Indicators")]
        [SerializeField] private GameObject _downedVfx;
        [Tooltip("Prefab Overhead UI tùy biến (Để trống sẽ tự sinh hoặc load từ Resources/Prefabs)")]
        [SerializeField] private GameObject _reviveHUDPrefab;

        private HealthSystem _healthSystem;
        private PlayerController _playerController;
        private PlayerAnimator _playerAnimator;
        private SpriteRenderer _spriteRenderer;
        private Weapons.WeaponManager _weaponManager;
        private CharacterCombat _characterCombat;
        private Rigidbody2D _rb;

        private CoopReviveHUDPresenter _revivePresenter;

        private bool _isDowned = false;
        private float _currentReviveProgress = 0f;

        public bool IsDowned => _isDowned;
        public float ReviveProgress01 => Mathf.Clamp01(_currentReviveProgress / _reviveDurationRequired);
        public float ReviveProgressNormalized => ReviveProgress01;
        public float ReviveRadius => _reviveRadius;

        public event Action OnPlayerDowned;
        public event Action OnPlayerRevived;
        public event Action<bool> OnDownedStateChanged;
        public event Action<float> OnReviveProgressChanged;
        public static event Action OnTeamWipe;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _playerController = GetComponent<PlayerController>();
            _playerAnimator = GetComponentInChildren<PlayerAnimator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _weaponManager = GetComponent<Weapons.WeaponManager>();
            _characterCombat = GetComponent<CharacterCombat>();
            _rb = GetComponent<Rigidbody2D>();

            // Khởi tạo Presenter & View theo chuẩn MVP & SOLID (Decoupling UI hoàn toàn khỏi logic)
            EnsurePresenterAndView();
        }

        private void EnsurePresenterAndView()
        {
            if (_revivePresenter == null)
            {
                _revivePresenter = GetComponentInChildren<CoopReviveHUDPresenter>(true);
            }

            if (_revivePresenter == null)
            {
                GameObject hudObj = null;

                // 1. Ưu tiên Prefab được gán trực tiếp qua Inspector
                if (_reviveHUDPrefab != null)
                {
                    hudObj = Instantiate(_reviveHUDPrefab, transform);
                }

                // 2. Fallback tìm trong Resources hoặc UnityEditor AssetDatabase
                if (hudObj == null)
                {
                    var loadedPrefab = Resources.Load<GameObject>("UI/CoopReviveOverheadHUD");
                    if (loadedPrefab == null)
                    {
                        loadedPrefab = Resources.Load<GameObject>("CoopReviveOverheadHUD");
                    }
#if UNITY_EDITOR
                    if (loadedPrefab == null)
                    {
                        loadedPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/CoopReviveOverheadHUD.prefab");
                    }
                    if (loadedPrefab == null)
                    {
                        loadedPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Features/UI/Prefabs/CoopReviveOverheadHUD.prefab");
                    }
#endif
                    if (loadedPrefab != null)
                    {
                        hudObj = Instantiate(loadedPrefab, transform);
                    }
                }

                // 3. Fallback cuối cùng: Tự dựng Runtime hoàn chỉnh
                if (hudObj == null)
                {
                    hudObj = new GameObject("CoopReviveOverheadHUD");
                    hudObj.transform.SetParent(transform, false);
                    hudObj.AddComponent<UI.HUD.CoopReviveWorldView>();
                    _revivePresenter = hudObj.AddComponent<CoopReviveHUDPresenter>();
                }
                else
                {
                    _revivePresenter = hudObj.GetComponent<CoopReviveHUDPresenter>();
                    if (_revivePresenter == null)
                    {
                        _revivePresenter = hudObj.AddComponent<CoopReviveHUDPresenter>();
                    }
                }
            }

            if (_revivePresenter != null)
            {
                _revivePresenter.Construct(this);
            }
        }

        private void OnEnable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnTryDie += HandleTryDie;
            }
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnTryDie -= HandleTryDie;
            }
        }

        private void Update()
        {
            if (!_isDowned) return;

            CheckTeammateReviving();
        }

        private bool HandleTryDie()
        {
            // Chỉ kích hoạt trạng thái Downed (Gục ngã chờ cứu) khi:
            // 1. Đang trong trận chiến đấu (GameState.Playing).
            // 2. Đang trong phòng Multiplayer Co-op có nhiều hơn 1 người chơi.
            bool isPlaying = GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Playing;

            int activePlayerCount = 1;
            if (ServiceContext.TryGet<ProjectZombie.Features.Multiplayer.Core.INetworkSessionService>(out var session) && session.IsInRoom)
            {
                if (session.CurrentRoom != null && session.CurrentRoom.Players != null)
                {
                    activePlayerCount = Mathf.Max(activePlayerCount, session.CurrentRoom.Players.Count);
                }
                if (ServiceContext.TryGet<IPlayerRegistry>(out var registry) && registry != null)
                {
                    activePlayerCount = Mathf.Max(activePlayerCount, registry.ActivePlayers.Count);
                }

                if (isPlaying && activePlayerCount > 1)
                {
                    EnterDownedState();
                    return true; // Chặn cái chết để HealthSystem không disable GameObject
                }
            }

            return false; // Chơi Solo / Offline / Ở Sảnh: Cho phép HealthSystem xử lý chết/GameOver thông thường
        }

        public void EnterDownedState()
        {
            if (_isDowned) return;

            _isDowned = true;
            _currentReviveProgress = 0f;

            // 1. Dừng ngay vận tốc vật lý
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_rb != null)
            {
                _rb.velocity = Vector2.zero;
            }

            // 2. Vô hiệu hóa di chuyển và khóa input
            if (_playerController != null)
            {
                _playerController.enabled = false;
                if (_playerController.InputProvider != null)
                {
                    _playerController.InputProvider.IsInputBlocked = true;
                }
            }

            // 3. Vô hiệu hóa tấn công thường và vũ khí tự động
            if (_characterCombat == null) _characterCombat = GetComponent<CharacterCombat>();
            if (_characterCombat != null) _characterCombat.enabled = false;

            if (_weaponManager == null) _weaponManager = GetComponent<Weapons.WeaponManager>();
            if (_weaponManager != null) _weaponManager.enabled = false;

            // 4. Phát hoạt ảnh Dead / Downed
            if (_playerAnimator != null)
            {
                _playerAnimator.ChangeAnimationState(PlayerAnimationState.Dead);
            }

            // 5. Làm mờ / đổi màu đỏ cảnh báo trên Sprite
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 0.8f);
            }

            if (_downedVfx != null) _downedVfx.SetActive(true);

            // 6. Đồng bộ mạng qua NetworkPlayerCharacter nếu có
            if (TryGetComponent<ProjectZombie.Features.Multiplayer.Core.NetworkPlayerCharacter>(out var netChar))
            {
                netChar.SetDownedState(true);
            }

            OnPlayerDowned?.Invoke();
            OnDownedStateChanged?.Invoke(true);
            OnReviveProgressChanged?.Invoke(0f);

            // 7. Kiểm tra xem tất cả người chơi khác có đều gục ngã hay không
            CheckTeamWipeCondition();
        }

        private void CheckTeammateReviving()
        {
            if (!ServiceContext.TryGet<IPlayerRegistry>(out var registry) || registry == null)
            {
                return;
            }

            bool hasTeammateReviving = false;
            Vector2 myPos = transform.position;

            for (int i = 0; i < registry.ActivePlayers.Count; i++)
            {
                var teammate = registry.ActivePlayers[i];
                if (teammate == null || teammate.GameObject == gameObject || !teammate.IsAlive) continue;

                // Kiểm tra nếu đồng đội cũng đang bị Downed thì không thể cứu
                if (teammate.GameObject.TryGetComponent<CoopDownedMechanic>(out var teammateDowned) && teammateDowned.IsDowned)
                {
                    continue;
                }

                float distSqr = ((Vector2)teammate.Transform.position - myPos).sqrMagnitude;
                if (distSqr <= _reviveRadius * _reviveRadius)
                {
                    hasTeammateReviving = true;
                    break;
                }
            }

            if (hasTeammateReviving)
            {
                _currentReviveProgress += Time.deltaTime;

                float norm = ReviveProgressNormalized;
                OnReviveProgressChanged?.Invoke(norm);

                if (_currentReviveProgress >= _reviveDurationRequired)
                {
                    CompleteRevive();
                }
            }
            else
            {
                // Giảm dần tiến độ cứu nếu đồng đội chạy ra ngoài vòng
                if (_currentReviveProgress > 0f)
                {
                    _currentReviveProgress = Mathf.Max(0f, _currentReviveProgress - Time.deltaTime * 1.5f);
                    float norm = ReviveProgressNormalized;
                    OnReviveProgressChanged?.Invoke(norm);
                }
            }
        }

        public void CompleteRevive()
        {
            _isDowned = false;
            _currentReviveProgress = 0f;

            // 1. Hồi phục máu và cấp bất tử ngắn
            if (_healthSystem != null)
            {
                float reviveHp = _healthSystem.MaxHealth * _reviveHealthPercentage;
                _healthSystem.Heal(reviveHp, allowRevive: true);
                _healthSystem.TriggerInvulnerability(2.5f); // 2.5s bất tử sau khi đứng dậy
            }

            // 2. Kích hoạt lại PlayerController và mở khóa input
            if (_playerController != null)
            {
                _playerController.enabled = true;
                if (_playerController.InputProvider != null)
                {
                    _playerController.InputProvider.IsInputBlocked = false;
                }
            }

            // 3. Kích hoạt lại tấn công thường và vũ khí
            if (_characterCombat != null) _characterCombat.enabled = true;
            if (_weaponManager != null) _weaponManager.enabled = true;

            // 4. Khôi phục hoạt ảnh Idle
            if (_playerAnimator != null)
            {
                _playerAnimator.ChangeAnimationState(PlayerAnimationState.Idle);
            }

            // 5. Khôi phục màu sprite gốc
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
            }

            if (_downedVfx != null) _downedVfx.SetActive(false);

            // 6. Đồng bộ mạng
            if (TryGetComponent<ProjectZombie.Features.Multiplayer.Core.NetworkPlayerCharacter>(out var netChar))
            {
                netChar.SetDownedState(false);
            }

            OnPlayerRevived?.Invoke();
            OnDownedStateChanged?.Invoke(false);
            OnReviveProgressChanged?.Invoke(0f);
        }

        private void CheckTeamWipeCondition()
        {
            if (!ServiceContext.TryGet<IPlayerRegistry>(out var registry) || registry == null)
            {
                OnTeamWipe?.Invoke();
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(GameState.GameOver);
                }
                return;
            }

            bool anyPlayerStanding = false;
            for (int i = 0; i < registry.ActivePlayers.Count; i++)
            {
                var p = registry.ActivePlayers[i];
                if (p == null || p.GameObject == null) continue;

                if (p.GameObject.TryGetComponent<CoopDownedMechanic>(out var downed) && downed.IsDowned)
                {
                    continue;
                }

                if (p.Health != null && p.Health.CurrentHealth <= 0)
                {
                    continue;
                }

                anyPlayerStanding = true;
                break;
            }

            if (!anyPlayerStanding)
            {
                Debug.Log("<color=#FF4444>[CoopDownedMechanic] Toàn đội đã gục ngã (Team Wipe)! Trận đấu kết thúc.</color>");
                OnTeamWipe?.Invoke();

                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(GameState.GameOver);
                }
            }
        }
    }
}

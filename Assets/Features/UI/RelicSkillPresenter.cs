using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối giữa WeaponManager / Active WeaponBase (Model) và RelicSkillButtonView (View).
    /// Tuân thủ Mô hình MVP: Tự động Ẩn/Hiện nút tùy theo loại Pháp Bảo (Chủ Động vs Bị Động).
    /// </summary>
    public class RelicSkillPresenter : MonoBehaviour, ProjectZombie.Core.Audio.IAudioServiceConsumer
    {
        private ProjectZombie.Core.Audio.IAudioService _audioService;

        public void InjectAudioService(ProjectZombie.Core.Audio.IAudioService audioService)
        {
            _audioService = audioService;
        }
        [Header("View Reference")]
        [SerializeField] private RelicSkillButtonView _buttonView;

        [Header("Model Reference")]
        [SerializeField] private WeaponManager _weaponManager;

        private WeaponBase _boundActiveRelic;

        public void Bind(WeaponManager manager)
        {
            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged -= HandleWeaponsChanged;
            }

            UnsubscribeRelicEvents();
            _weaponManager = manager;

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged += HandleWeaponsChanged;
            }

            RefreshRelicBinding();
        }

        public static RelicSkillPresenter Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (_buttonView == null)
            {
                _buttonView = GetComponent<RelicSkillButtonView>();
            }

            if (_buttonView != null)
            {
                _buttonView.OnButtonClicked += HandleButtonClicked;
                _buttonView.OnAimStarted += HandleAimStarted;
                _buttonView.OnAimUpdated += HandleAimUpdated;
                _buttonView.OnAimReleased += HandleAimReleased;
                _buttonView.OnAimCancelled += HandleAimCancelled;
            }

            if (_weaponManager == null)
            {
                var weaponMgr = PlayerProvider.GetPlayerComponent<WeaponManager>();
                if (weaponMgr != null)
                {
                    Bind(weaponMgr);
                }
                else
                {
                    var player = PlayerProvider.PlayerGameObject;
                    if (player != null)
                    {
                        Bind(player.GetComponent<WeaponManager>());
                    }
                }
            }
            else
            {
                RefreshRelicBinding();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged -= HandleWeaponsChanged;
            }

            if (_buttonView != null)
            {
                _buttonView.OnButtonClicked -= HandleButtonClicked;
                _buttonView.OnAimStarted -= HandleAimStarted;
                _buttonView.OnAimUpdated -= HandleAimUpdated;
                _buttonView.OnAimReleased -= HandleAimReleased;
                _buttonView.OnAimCancelled -= HandleAimCancelled;
            }

            UnsubscribeRelicEvents();
        }

        private void HandleAimStarted()
        {
            if (_boundActiveRelic == null || !_boundActiveRelic.IsRelicSkillReady) return;

            var aimConfig = _boundActiveRelic.AimConfig;
            if (aimConfig.aimType == Combat.Aiming.SkillAimType.None)
            {
                return;
            }

            Combat.Aiming.SkillAimIndicatorController.Instance?.StartAim(aimConfig);
        }

        private void HandleAimUpdated(Vector2 direction, float pullPercent, bool isCancel)
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.UpdateAim(direction, pullPercent, isCancel);
        }

        private void HandleAimReleased(Vector2 direction, bool isQuickTap)
        {
            var aimResult = Combat.Aiming.SkillAimIndicatorController.Instance != null
                ? Combat.Aiming.SkillAimIndicatorController.Instance.CurrentAimResult
                : Combat.Aiming.AimResult.FromDirection(direction, _weaponManager != null ? _weaponManager.transform.position : Vector3.zero);

            Combat.Aiming.SkillAimIndicatorController.Instance?.StopAim();
            if (_boundActiveRelic != null && _boundActiveRelic.AimConfig.aimType == Combat.Aiming.SkillAimType.None)
            {
                TriggerQuickTapSkill(default);
                return;
            }

            if (isQuickTap)
            {
                TriggerQuickTapSkill(direction);
            }
            else
            {
                if (_weaponManager != null && _boundActiveRelic != null && _boundActiveRelic.IsRelicSkillReady)
                {
                    _audioService?.PlayUIConfirm();

                    // Multiplayer cần gửi hướng qua input buffer. Chơi đơn chuyển AimResult đầy đủ
                    // trực tiếp để không làm mất vị trí ngắm của các kỹ năng chọn điểm.
                    if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null && PlayerProvider.PlayerGameObject.TryGetComponent<Player.Input.PlayerInputReader>(out var inputReader))
                    {
                        if (inputReader.IsNetworkMode)
                        {
                            inputReader.TriggerRelicSkill(aimResult.Direction);
                            return;
                        }
                    }

                    _weaponManager.TriggerEquippedRelicSkill(aimResult);
                }
            }
        }

        private void TriggerQuickTapSkill(Vector2 fallbackDirection)
        {
            if (_weaponManager == null || _boundActiveRelic == null || !_boundActiveRelic.IsRelicSkillReady) return;

            var config = _boundActiveRelic.AimConfig;
            if (config.aimType == Combat.Aiming.SkillAimType.None ||
                config.aimType == Combat.Aiming.SkillAimType.SelfAOE ||
                config.aimType == Combat.Aiming.SkillAimType.RhythmPulse)
            {
                HandleButtonClicked();
                return;
            }

            Vector3 origin = _weaponManager.transform.position;
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
                origin = PlayerProvider.PlayerTransform.position;

            Vector2 facing = fallbackDirection;
            if (facing.sqrMagnitude <= 0.001f && PlayerProvider.HasPlayer)
            {
                var player = PlayerProvider.PlayerGameObject;
                var controller = player != null ? player.GetComponent<PlayerController>() : null;
                if (controller != null && controller.MovementInput.sqrMagnitude > 0.001f)
                    facing = controller.MovementInput.normalized;
                else if (player != null)
                    facing = player.transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
            }

            Combat.Aiming.AutoTargetScanner.TryGetAutoAimDirection(
                origin, config, facing, out var aimDirection, out var targetPosition);
            float distance = Vector2.Distance(origin, targetPosition);
            var aimResult = new Combat.Aiming.AimResult(aimDirection, distance, targetPosition, 1f, true);

            _audioService?.PlayUIConfirm();
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null &&
                PlayerProvider.PlayerGameObject.TryGetComponent<Player.Input.PlayerInputReader>(out var inputReader) && inputReader.IsNetworkMode)
            {
                inputReader.TriggerRelicSkill(aimDirection);
                return;
            }

            _weaponManager.TriggerEquippedRelicSkill(aimResult);
        }

        private void HandleAimCancelled()
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.StopAim();
        }

        private void HandleWeaponsChanged()
        {
            RefreshRelicBinding();
        }

        private void RefreshRelicBinding()
        {
            if (_buttonView == null) return;

            UnsubscribeRelicEvents();

            if (_weaponManager == null || !_weaponManager.HasActiveRelic(out var activeRelic) || activeRelic == null)
            {
                // Nếu không có Pháp Bảo Chủ Động (hoặc đang mang Pháp Bảo Bị Động): Ẩn Nút
                _boundActiveRelic = null;
                _buttonView.SetVisible(false);
                return;
            }

            // Có Pháp Bảo Chủ Động: Hiện Nút và kết nối sự kiện
            _boundActiveRelic = activeRelic;
            _buttonView.SetVisible(true);
            _buttonView.SetIcon(_boundActiveRelic.icon);

            _boundActiveRelic.OnRelicCooldownUpdated += HandleRelicCooldownUpdated;
            _boundActiveRelic.OnRelicSkillReady += HandleRelicSkillReady;
            _boundActiveRelic.OnRelicSkillExecuted += HandleRelicSkillExecuted;
            _boundActiveRelic.OnRelicPhaseChanged += HandleRelicPhaseChanged;
            _boundActiveRelic.OnRelicStackBadgeUpdated += HandleRelicStackBadgeUpdated;

            RefreshUIState();
        }

        private void UnsubscribeRelicEvents()
        {
            if (_boundActiveRelic != null)
            {
                _boundActiveRelic.OnRelicCooldownUpdated -= HandleRelicCooldownUpdated;
                _boundActiveRelic.OnRelicSkillReady -= HandleRelicSkillReady;
                _boundActiveRelic.OnRelicSkillExecuted -= HandleRelicSkillExecuted;
                _boundActiveRelic.OnRelicPhaseChanged -= HandleRelicPhaseChanged;
                _boundActiveRelic.OnRelicStackBadgeUpdated -= HandleRelicStackBadgeUpdated;
                _boundActiveRelic = null;
            }
        }

        private void RefreshUIState()
        {
            if (_boundActiveRelic == null || _buttonView == null || Controls.Customization.CustomizableControlButton.IsAnyInEditMode) return;

            float rem = _boundActiveRelic.RelicRemainingCooldown;
            float max = _boundActiveRelic.RelicMaxCooldown;
            bool isReady = _boundActiveRelic.IsRelicSkillReady;
            bool isRecast = _boundActiveRelic.IsInRecastWindow;

            _buttonView.SetInteractable(isReady);
            _buttonView.SetRecastGlow(isRecast);
            _buttonView.SetStackBadge(_boundActiveRelic.RelicStackBadgeText);
            string text = rem > 0f && !isRecast ? RelicSkillButtonView.GetCachedCooldownText(rem) : string.Empty;
            _buttonView.SetCooldown(isRecast ? 0f : rem, max, text);
        }

        private void HandleRelicStackBadgeUpdated(string badgeText)
        {
            if (_buttonView != null && !Controls.Customization.CustomizableControlButton.IsAnyInEditMode)
            {
                _buttonView.SetStackBadge(badgeText);
            }
        }

        private void HandleRelicPhaseChanged(WeaponBase.RelicCastPhase phase)
        {
            RefreshUIState();
        }

        private void HandleRelicCooldownUpdated(float remaining, float max)
        {
            if (_buttonView == null || Controls.Customization.CustomizableControlButton.IsAnyInEditMode) return;

            string text = remaining > 0f ? RelicSkillButtonView.GetCachedCooldownText(remaining) : string.Empty;
            _buttonView.SetCooldown(remaining, max, text);
            RefreshUIState();
        }

        private void HandleRelicSkillReady()
        {
            RefreshUIState();
        }

        private void HandleRelicSkillExecuted()
        {
            RefreshUIState();
        }

        private void HandleButtonClicked()
        {
            if (_weaponManager == null || _boundActiveRelic == null || Controls.Customization.CustomizableControlButton.IsAnyInEditMode) return;

            if (!_boundActiveRelic.IsRelicSkillReady)
            {
                _audioService?.PlayUIError();
                return;
            }

            _audioService?.PlayUIConfirm();

            // Tuyến đường duy nhất: Gửi Intent qua PlayerInputReader
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null && PlayerProvider.PlayerGameObject.TryGetComponent<Player.Input.PlayerInputReader>(out var inputReader))
            {
                inputReader.TriggerRelicSkill();
                return;
            }

            _weaponManager.TriggerEquippedRelicSkill();
        }
    }
}

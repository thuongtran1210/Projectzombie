using UnityEngine;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối giữa WeaponManager / Active WeaponBase (Model) và RelicSkillButtonView (View).
    /// Tuân thủ Mô hình MVP: Tự động Ẩn/Hiện nút tùy theo loại Pháp Bảo (Chủ Động vs Bị Động).
    /// </summary>
    public class RelicSkillPresenter : MonoBehaviour
    {
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
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Bind(player.GetComponent<WeaponManager>());
                }
            }
            else
            {
                RefreshRelicBinding();
            }
        }

        private void OnDestroy()
        {
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

            var aimConfig = (_boundActiveRelic is Combat.Aiming.IAimableSkill aimable)
                ? aimable.AimConfig
                : new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.LineArrow, 6.5f, 1.2f, 0f, true);

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
            if (isQuickTap)
            {
                HandleButtonClicked();
            }
            else
            {
                if (_weaponManager != null && _boundActiveRelic != null && _boundActiveRelic.IsRelicSkillReady)
                {
                    global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
                    _weaponManager.TriggerEquippedRelicSkill(aimResult);
                }
            }
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
            if (_boundActiveRelic == null || _buttonView == null) return;

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
            if (_buttonView != null)
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
            if (_buttonView == null) return;

            string text = remaining > 0f ? RelicSkillButtonView.GetCachedCooldownText(remaining) : string.Empty;
            _buttonView.SetCooldown(remaining, max, text);
            _buttonView.SetInteractable(remaining <= 0f);
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
            if (_weaponManager == null || _boundActiveRelic == null) return;

            if (!_boundActiveRelic.IsRelicSkillReady)
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIError();
                return;
            }

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
            _weaponManager.TriggerEquippedRelicSkill();
        }
    }
}

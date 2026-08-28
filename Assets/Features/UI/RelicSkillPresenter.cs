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
            }

            UnsubscribeRelicEvents();
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

            RefreshUIState();
        }

        private void UnsubscribeRelicEvents()
        {
            if (_boundActiveRelic != null)
            {
                _boundActiveRelic.OnRelicCooldownUpdated -= HandleRelicCooldownUpdated;
                _boundActiveRelic.OnRelicSkillReady -= HandleRelicSkillReady;
                _boundActiveRelic.OnRelicSkillExecuted -= HandleRelicSkillExecuted;
                _boundActiveRelic = null;
            }
        }

        private void RefreshUIState()
        {
            if (_boundActiveRelic == null || _buttonView == null) return;

            float rem = _boundActiveRelic.RelicRemainingCooldown;
            float max = _boundActiveRelic.RelicMaxCooldown;
            bool isReady = _boundActiveRelic.IsRelicSkillReady;

            _buttonView.SetInteractable(isReady);
            string text = rem > 0f ? $"{Mathf.CeilToInt(rem)}s" : string.Empty;
            _buttonView.SetCooldown(rem, max, text);
        }

        private void HandleRelicCooldownUpdated(float remaining, float max)
        {
            if (_buttonView == null) return;

            string text = remaining > 0f ? $"{Mathf.CeilToInt(remaining)}s" : string.Empty;
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    /// <summary>
    /// Passive View quản lý hiển thị Bảng Thông Số Thuộc Tính & Menu Tạm Dừng trong trận (3-Column Layout).
    /// Tuân thủ nghiêm ngặt mô hình MVP: không chứa logic nghiệp vụ, chỉ nhận dữ liệu đã định dạng từ Presenter.
    /// </summary>
    public class PlayerStatsMenuUIView : MonoBehaviour
    {
        // ====================================================================
        // [INSPECTOR] — Header & Navigation
        // ====================================================================
        [Header("Header & Navigation")]
        [SerializeField] private Button _dimBackgroundButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _currencyText;

        // ====================================================================
        // [INSPECTOR] — Column 1: Hero & Active Weapon
        // ====================================================================
        [Header("Column 1: Hero & Weapon")]
        [SerializeField] private Image _heroAvatarImage;
        [SerializeField] private TextMeshProUGUI _heroNameText;
        [SerializeField] private TextMeshProUGUI _heroElementBadgeText;
        [SerializeField] private Image _weaponIconImage;
        [SerializeField] private TextMeshProUGUI _weaponNameText;
        [SerializeField] private TextMeshProUGUI _weaponLevelText;
        [SerializeField] private TextMeshProUGUI _weaponDpsText;

        // ====================================================================
        // [INSPECTOR] — Column 2: 8 Core RPG Stats
        // ====================================================================
        [Header("Column 2: Core RPG Stats")]
        [SerializeField] private StatUIEntry _healthStatEntry;
        [SerializeField] private StatUIEntry _damageStatEntry;
        [SerializeField] private StatUIEntry _critStatEntry;
        [SerializeField] private StatUIEntry _attackSpeedStatEntry;
        [SerializeField] private StatUIEntry _moveSpeedStatEntry;
        [SerializeField] private StatUIEntry _dashCooldownStatEntry;
        [SerializeField] private StatUIEntry _pickupRangeStatEntry;
        [SerializeField] private StatUIEntry _expMultiplierStatEntry;

        // ====================================================================
        // [INSPECTOR] — Column 3: Run Stats, Passives & Controls
        // ====================================================================
        [Header("Column 3: Run Stats & Passives")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _killCountText;
        [SerializeField] private Transform _passivesContainer;
        [SerializeField] private SkillUIEntry _passiveEntryPrefab;
        [SerializeField] private TooltipUI _tooltipUI;

        [Header("Pause Action Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private readonly List<SkillUIEntry> _spawnedPassives = new List<SkillUIEntry>();

        private System.Action _onResume;
        private System.Action _onSettings;
        private System.Action _onQuit;
        private System.Action _onClose;

        private void Awake()
        {
            // Đảm bảo Animator hoạt động khi Time.timeScale = 0
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_closeButton != null)
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());

            if (_dimBackgroundButton != null)
                _dimBackgroundButton.onClick.AddListener(() => _onClose?.Invoke());

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(() => _onResume?.Invoke());

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(() => _onSettings?.Invoke());

            if (_quitButton != null)
                _quitButton.onClick.AddListener(() => _onQuit?.Invoke());
        }

        // ====================================================================
        // PUBLIC API — Giao tiếp với Presenter
        // ====================================================================

        public void SetCallbacks(System.Action onClose, System.Action onResume, System.Action onSettings, System.Action onQuit)
        {
            _onClose = onClose;
            _onResume = onResume;
            _onSettings = onSettings;
            _onQuit = onQuit;
        }

        public void SetHeroInfo(Sprite avatar, string heroName, string elementBadgeFormatted)
        {
            if (_heroAvatarImage != null)
            {
                if (avatar != null)
                {
                    _heroAvatarImage.sprite = avatar;
                    _heroAvatarImage.gameObject.SetActive(true);
                }
                else
                {
                    _heroAvatarImage.gameObject.SetActive(false);
                }
            }
            if (_heroNameText != null) _heroNameText.text = heroName;
            if (_heroElementBadgeText != null) _heroElementBadgeText.text = elementBadgeFormatted;
        }

        public void SetWeaponInfo(Sprite icon, string weaponName, string levelFormatted, string dpsFormatted)
        {
            if (_weaponIconImage != null)
            {
                if (icon != null)
                {
                    _weaponIconImage.sprite = icon;
                    _weaponIconImage.gameObject.SetActive(true);
                }
                else
                {
                    _weaponIconImage.gameObject.SetActive(false);
                }
            }
            if (_weaponNameText != null) _weaponNameText.text = weaponName;
            if (_weaponLevelText != null) _weaponLevelText.text = levelFormatted;
            if (_weaponDpsText != null) _weaponDpsText.text = dpsFormatted;
        }

        public void SetCurrency(string currencyFormatted)
        {
            if (_currencyText != null) _currencyText.text = currencyFormatted;
        }

        public void SetRunStats(string timerFormatted, string killsFormatted)
        {
            if (_timerText != null) _timerText.text = timerFormatted;
            if (_killCountText != null) _killCountText.text = killsFormatted;
        }

        public void UpdateHealth(string formattedValue)
        {
            if (_healthStatEntry != null) _healthStatEntry.Setup("Sinh Lực", formattedValue);
        }

        public void UpdateDamage(string formattedValue)
        {
            if (_damageStatEntry != null) _damageStatEntry.Setup("Công Kích", formattedValue);
        }

        public void UpdateCrit(string formattedValue)
        {
            if (_critStatEntry != null) _critStatEntry.Setup("Bạo Kích", formattedValue);
        }

        public void UpdateAttackSpeed(string formattedValue)
        {
            if (_attackSpeedStatEntry != null) _attackSpeedStatEntry.Setup("Tốc Đánh", formattedValue);
        }

        public void UpdateSpeed(string formattedValue)
        {
            if (_moveSpeedStatEntry != null) _moveSpeedStatEntry.Setup("Thân Pháp", formattedValue);
        }

        public void UpdateDashCooldown(string formattedValue)
        {
            if (_dashCooldownStatEntry != null) _dashCooldownStatEntry.Setup("Phi Vân (Lướt)", formattedValue);
        }

        public void UpdatePickupRange(string formattedValue)
        {
            if (_pickupRangeStatEntry != null) _pickupRangeStatEntry.Setup("Thu Hút", formattedValue);
        }

        public void UpdateExpMultiplier(string formattedValue)
        {
            if (_expMultiplierStatEntry != null) _expMultiplierStatEntry.Setup("Ngộ Tính (EXP)", formattedValue);
        }

        public void SetPassives(IReadOnlyList<(Sprite icon, string name, string description, int level)> passives)
        {
            if (_passivesContainer == null) return;

            // Reuse or spawn entries
            int needed = passives != null ? passives.Count : 0;

            while (_spawnedPassives.Count < needed && _passiveEntryPrefab != null)
            {
                var entry = Instantiate(_passiveEntryPrefab, _passivesContainer);
                _spawnedPassives.Add(entry);
            }

            for (int i = 0; i < _spawnedPassives.Count; i++)
            {
                if (i < needed)
                {
                    _spawnedPassives[i].gameObject.SetActive(true);
                    var p = passives[i];
                    _spawnedPassives[i].Setup(p.icon, p.level, p.name, p.description, _tooltipUI);
                }
                else
                {
                    _spawnedPassives[i].gameObject.SetActive(false);
                }
            }
        }
    }
}

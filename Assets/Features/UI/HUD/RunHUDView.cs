// ============================================================================
// FILE: RunHUDView.cs — TẦNG VIEW (MVP)
// Trách nhiệm DUY NHẤT: Cập nhật các phần tử UI trên màn hình HUD trong trận.
// KHÔNG chứa logic nghiệp vụ. KHÔNG biết PlayerStats, RunStatsTracker tồn tại.
// Nhận dữ liệu ĐÃ ĐƯỢC ĐỊNH DẠNG THÀNH STRING từ Presenter.
// ============================================================================

using ProjectZombie.Features.UI.StatsAndSkills;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// View thuần túy (Passive View) cho HUD trong trận.
    /// Tất cả public method chỉ nhận string/float đã được format — không bao giờ nhận Model object.
    /// </summary>
    public class RunHUDView : MonoBehaviour
    {
        // ====================================================================
        // [INSPECTOR] — Health & EXP Bars
        // ====================================================================

        [Header("Health & EXP")]
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Image _hpFillImage;              // Image ruột của thanh HP
        [SerializeField] private TextMeshProUGUI _hpText;         // Ví dụ: "75 / 100"
        [SerializeField] private Slider _expSlider;
        [SerializeField] private Image _expFillImage;             // Image ruột của thanh EXP
        [SerializeField] private TextMeshProUGUI _levelText;      // Ví dụ: "Lv.5"

        [Header("Visual Feedback Settings")]
        [SerializeField] private Color _normalHpColor = new Color(0.82f, 0.22f, 0.22f, 1f); // #D13838 (Đỏ Chu Sa)
        [SerializeField] private Color _lowHpColor = new Color(1f, 0.15f, 0.15f, 1f);    // #FF2626 (Đỏ rực cảnh báo)
        [SerializeField] private float _lowHpThresholdRatio = 0.25f;

        // ====================================================================
        // [INSPECTOR] — Run Stats
        // ====================================================================

        [Header("Run Stats")]
        [SerializeField] private TextMeshProUGUI _timerText;      // Ví dụ: "05:42"
        [SerializeField] private TextMeshProUGUI _killCountText;  // Ví dụ: "💀 137"

        // ====================================================================
        // [INSPECTOR] — Skills Display
        // ====================================================================

        [Header("Skills Display")]
        [SerializeField] private Transform _skillsContainer;
        [SerializeField] private SkillUIEntry _skillEntryPrefab;
        [SerializeField] private TooltipUI _tooltipUI;

        private readonly System.Collections.Generic.List<SkillUIEntry> _spawnedSkills = new System.Collections.Generic.List<SkillUIEntry>();

        // ====================================================================
        // UNITY LIFECYCLE
        // ====================================================================

        private void Awake()
        {
            // Đảm bảo Animator chạy kể cả khi Time.timeScale = 0 (Level Up Pause)
            var animator = GetComponent<Animator>();
            if (animator != null)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (_hpSlider != null && _hpFillImage == null)
            {
                var fillRect = _hpSlider.fillRect;
                if (fillRect != null)
                {
                    _hpFillImage = fillRect.GetComponent<Image>();
                }
            }
        }


        // ====================================================================
        // PUBLIC API — Chỉ được gọi bởi RunHUDPresenter
        // ====================================================================

        /// <summary>Cập nhật thanh máu với cảnh báo tương phản khi HP thấp. Presenter truyền giá trị thô (float).</summary>
        public void SetHealth(float current, float max)
        {
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = max;
                _hpSlider.value = current;
            }

            float ratio = max > 0f ? (current / max) : 0f;
            bool isLowHp = ratio <= _lowHpThresholdRatio && current > 0f;

            if (_hpFillImage != null)
            {
                _hpFillImage.color = isLowHp ? _lowHpColor : _normalHpColor;
            }

            if (_hpText != null)
            {
                int curInt = Mathf.CeilToInt(current);
                int maxInt = Mathf.CeilToInt(max);
                _hpText.text = isLowHp 
                    ? $"<color=#FF3333><b>{curInt}</b></color> / {maxInt}" 
                    : $"{curInt} / {maxInt}";
            }
        }

        /// <summary>Cập nhật thanh EXP. Presenter truyền giá trị thô (float).</summary>
        public void SetExp(float current, float max)
        {
            if (_expSlider != null)
            {
                _expSlider.maxValue = max;
                _expSlider.value = current;
            }
        }

        /// <summary> Cập nhật số cấp độ. Presenter truyền string đã định dạng.</summary>
        public void SetLevel(string formattedLevel)
        {
            if (_levelText == null) return;
            _levelText.text = formattedLevel;
        }

        public void SetLevel(int level)
        {
            if (_levelText != null) _levelText.SetText("<color=#FFD700><b>Lv.{0}</b></color>", level);
        }

        /// <summary>Cập nhật đồng hồ. Presenter truyền string MM:SS đã định dạng.</summary>
        public void SetTimer(string formattedTime)
        {
            if (_timerText == null) return;
            _timerText.text = formattedTime;
        }

        public void SetTimer(int minutes, int seconds)
        {
            if (_timerText != null) _timerText.SetText("{0:00}:{1:00}", minutes, seconds);
        }

        /// <summary>Cập nhật Kill Count. Presenter truyền string đã định dạng.</summary>
        public void SetKillCount(string formattedKillCount)
        {
            if (_killCountText == null) return;
            _killCountText.text = formattedKillCount;
        }

        public void SetKillCount(int count)
        {
            if (_killCountText != null) _killCountText.SetText("Diệt: <color=#FF8C42>{0}</color>", count);
        }

        public struct SkillDisplayData
        {
            public Sprite Icon;
            public int Level;
            public string Name;
            public string Description;
        }

        /// <summary>Cập nhật danh sách icon Kỹ năng/Vũ khí sở hữu.</summary>
        public void UpdateSkills(System.Collections.Generic.IReadOnlyList<SkillDisplayData> skills)
        {
            for (int i = _spawnedSkills.Count - 1; i >= 0; i--)
            {
                if (_spawnedSkills[i] != null)
                {
                    Destroy(_spawnedSkills[i].gameObject);
                }
            }
            _spawnedSkills.Clear();

            if (_skillsContainer == null)
            {
                var found = transform.Find("Panel_Skills/ActiveSkills_Container");
                if (found != null) _skillsContainer = found;
                else
                {
                    var panelSkills = transform.Find("Panel_Skills");
                    if (panelSkills != null) _skillsContainer = panelSkills;
                }
            }

            if (_skillEntryPrefab == null)
            {
                _skillEntryPrefab = Resources.Load<SkillUIEntry>("SkillUIEntry");
            }

            if (_skillsContainer == null)
            {
                Debug.LogWarning("[RunHUDView] _skillsContainer chưa được gán và không tìm thấy trong Panel_Skills!");
                return;
            }

            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (_skillEntryPrefab == null) break;
                    SkillUIEntry newEntry = Instantiate(_skillEntryPrefab, _skillsContainer);
                    newEntry.Setup(skill.Icon, skill.Level, skill.Name, skill.Description, _tooltipUI);
                    _spawnedSkills.Add(newEntry);
                }
            }
        }
    }
}

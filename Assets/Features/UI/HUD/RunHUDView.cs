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
        [SerializeField] private TextMeshProUGUI _hpText;         // Ví dụ: "75 / 100"
        [SerializeField] private Slider _expSlider;
        [SerializeField] private TextMeshProUGUI _levelText;      // Ví dụ: "Lv.5"

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
        }


        // ====================================================================
        // PUBLIC API — Chỉ được gọi bởi RunHUDPresenter
        // ====================================================================

        /// <summary>Cập nhật thanh máu. Presenter truyền giá trị thô (float).</summary>
        public void SetHealth(float current, float max)
        {
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = max;
                _hpSlider.value = current;
            }

            if (_hpText != null)
                _hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
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
            if (_levelText == null)
            {
                Debug.LogWarning($"[{nameof(RunHUDView)}] _levelText chưa được gán trong Inspector.");
                return;
            }
            _levelText.text = formattedLevel;
        }

        /// <summary>Cập nhật đồng hồ. Presenter truyền string MM:SS đã định dạng.</summary>
        public void SetTimer(string formattedTime)
        {
            if (_timerText == null)
            {
                Debug.LogWarning($"[{nameof(RunHUDView)}] _timerText chưa được gán trong Inspector.");
                return;
            }
            _timerText.text = formattedTime;
        }

        /// <summary>Cập nhật Kill Count. Presenter truyền string đã định dạng.</summary>
        public void SetKillCount(string formattedKillCount)
        {
            if (_killCountText == null)
            {
                Debug.LogWarning($"[{nameof(RunHUDView)}] _killCountText chưa được gán trong Inspector.");
                return;
            }
            _killCountText.text = formattedKillCount;
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
            foreach (var entry in _spawnedSkills)
            {
                if (entry != null)
                {
                    Destroy(entry.gameObject);
                }
            }
            _spawnedSkills.Clear();

            if (_skillEntryPrefab == null || _skillsContainer == null)
            {
                return;
            }

            foreach (var skill in skills)
            {
                SkillUIEntry newEntry = Instantiate(_skillEntryPrefab, _skillsContainer);
                newEntry.Setup(skill.Icon, skill.Level, skill.Name, skill.Description, _tooltipUI);
                _spawnedSkills.Add(newEntry);
            }
        }

        // ====================================================================
        // [INSPECTOR] — Vong Xuyen (v4.0) Controls
        // ====================================================================

        [Header("Vong Xuyen (v4.0)")]
        [SerializeField] private TextMeshProUGUI _bossElementText;  // Ví dụ: "<color=#FF4444>[BOSS: HỎA]</color>"

        /// <summary>Cập nhật thuộc tính hiện tại của Boss. Presenter truyền string đã format TMP Rich Text.</summary>
        public void SetBossElement(string formattedBossElement)
        {
            if (_bossElementText != null)
            {
                _bossElementText.text = formattedBossElement;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


namespace ProjectZombie.Features.UI.StatsAndSkills
{
    [System.Obsolete("Class này đã bị thay thế hoàn toàn bởi RunHUDView (Features/UI/HUD/RunHUDView.cs). Hãy dùng RunHUDView.")]
    public class PlayerHUDView : MonoBehaviour
    {
        [Header("Health & EXP")]
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Slider _expSlider;

        [Header("Skills Display")]
        [SerializeField] private Transform _skillsContainer;
        [SerializeField] private SkillUIEntry _skillEntryPrefab;
        [SerializeField] private TooltipUI _tooltipUI;

        [Header("Run Stats (Timer & Kills)")]
        [Tooltip("Text hiển thị thời gian sống sót (MM:SS)")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [Tooltip("Text hiển thị số Zombie đã hạ")]
        [SerializeField] private TextMeshProUGUI _killCountText;

        private List<SkillUIEntry> _spawnedSkills = new List<SkillUIEntry>();

        public void UpdateHealth(float current, float max)
        {
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = max;
                _hpSlider.value = current;
            }
        }

        public void UpdateExp(float currentExp, float maxExp)
        {
            if (_expSlider != null)
            {
                _expSlider.maxValue = maxExp;
                _expSlider.value = currentExp;
            }
        }

        public struct SkillDisplayData
        {
            public Sprite Icon;
            public int Level;
            public string Name;
            public string Description;
        }

        public void UpdateSkills(IReadOnlyList<SkillDisplayData> skills)
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
                Debug.LogWarning($"[{nameof(PlayerHUDView)}] _skillEntryPrefab hoặc _skillsContainer chưa được gán trong Inspector.");
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
        // RUN STATS UI
        // ====================================================================

        public void SetTimer(string formattedTime)
        {
            if (_timerText != null)
                _timerText.text = formattedTime;
        }

        public void SetKillCount(string formattedKillCount)
        {
            if (_killCountText != null)
                _killCountText.text = formattedKillCount;
        }
    }
}

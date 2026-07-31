using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View màn hình tổng kết sau trận đấu (Run Summary View).
    /// </summary>
    public class RunSummaryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _killCountText;
        [SerializeField] private TextMeshProUGUI _coTienEarnedText;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _retryButton;

        public event Action OnMainMenuClicked;
        public event Action OnRetryClicked;

        private void Awake()
        {
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
            if (_retryButton != null) _retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
        }

        public void DisplaySummary(string formattedTitle, string formattedTime, string formattedKills, string formattedCoTien)
        {
            if (_titleText != null) _titleText.text = formattedTitle;
            if (_timeText != null) _timeText.text = formattedTime;
            if (_killCountText != null) _killCountText.text = formattedKills;
            if (_coTienEarnedText != null) _coTienEarnedText.text = formattedCoTien;
        }
    }
}

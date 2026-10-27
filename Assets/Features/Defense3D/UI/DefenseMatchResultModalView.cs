using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>
    /// Modal hiển thị kết quả trận đấu (Chiến Thắng / Thất Bại / Bù Giờ) và nút Chơi Lại.
    /// Tuân thủ MVP: Nhận dữ liệu string đã format từ Presenter, phát sự kiện Restart/MainMenu.
    /// </summary>
    public class DefenseMatchResultModalView : MonoBehaviour
    {
        [Header("Root Modal")]
        [SerializeField] private GameObject _modalRoot;

        [Header("Nội Dung Kết Quả")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _statsSummaryText;

        [Header("Nút Tương Tác")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        public event Action OnRestartClicked;
        public event Action OnMainMenuClicked;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
        }

        public void ShowResult(string title, string message, string statsSummary)
        {
            if (_modalRoot != null) _modalRoot.SetActive(true);
            if (_titleText != null) _titleText.text = title;
            if (_messageText != null) _messageText.text = message;
            if (_statsSummaryText != null) _statsSummaryText.text = statsSummary;
        }

        public void Hide()
        {
            if (_modalRoot != null) _modalRoot.SetActive(false);
        }
    }
}

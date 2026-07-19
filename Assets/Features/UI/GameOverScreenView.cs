using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View hiển thị thụ động cho Màn hình Kết quả Game Over / Chiến thắng.
    /// Không chứa logic game, chỉ nhận dữ liệu hiển thị từ Presenter.
    /// </summary>
    public class GameOverScreenView : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] private GameObject panel;

        [Header("Title")]
        [SerializeField] private TMP_Text titleText;

        [Header("Run Stats Display")]
        [SerializeField] private TMP_Text timeAliveText;
        [SerializeField] private TMP_Text killCountText;
        [SerializeField] private TMP_Text maxLevelText;
        [SerializeField] private TMP_Text damageDealtText;

        [Header("Currency Meta")]
        [SerializeField] private TMP_Text currencyEarnedText;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        // Sự kiện gửi lên Presenter khi bấm nút
        public event Action OnPlayAgainClicked;
        public event Action OnMainMenuClicked;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());

            // Đảm bảo Animator hoạt động khi Time.timeScale = 0 (Game Over pause)
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        public void SetActive(bool isActive)
        {
            if (panel != null) panel.SetActive(isActive);
        }

        public void SetTitle(string text, Color color)
        {
            if (titleText != null)
            {
                titleText.text = text;
                titleText.color = color;
            }
        }

        public void SetTimeAlive(string text)
        {
            if (timeAliveText != null) timeAliveText.text = text;
        }

        public void SetKillCount(string text)
        {
            if (killCountText != null) killCountText.text = text;
        }

        public void SetMaxLevel(string text)
        {
            if (maxLevelText != null) maxLevelText.text = text;
        }

        public void SetDamageDealt(string text)
        {
            if (damageDealtText != null) damageDealtText.text = text;
        }

        public void SetCurrency(string text)
        {
            if (currencyEarnedText != null) currencyEarnedText.text = text;
        }
    }
}

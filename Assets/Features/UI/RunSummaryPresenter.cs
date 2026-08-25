using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter quy đổi điểm số trận đấu thành Cổ Tiền, gọi MetaCurrencyManager và lưu đĩa.
    /// </summary>
    public class RunSummaryPresenter : MonoBehaviour
    {
        [SerializeField] private RunSummaryView _view;

        public void SetupSummary(bool isVictory, float elapsedTimeSeconds, int killCount)
        {
            if (_view != null)
            {
                _view.OnMainMenuClicked += OnReturnToMainMenu;
                _view.OnRetryClicked += OnRetryGame;
            }

            int minutes = Mathf.FloorToInt(elapsedTimeSeconds / 60f);
            int seconds = Mathf.FloorToInt(elapsedTimeSeconds % 60f);
            string formattedTime = $"Thời gian sống sót: <b>{minutes:00}:{seconds:00}</b>";
            string formattedKills = $"Số yêu ma diệt: <b>{killCount}</b>";

            // Quy đổi Cổ Tiền từ RunStatsTracker (chuẩn hóa Single Source of Truth)
            int coTienEarned = 0;
            var tracker = ProjectZombie.Features.Shared.RunStatsTracker.Instance;
            if (tracker != null)
            {
                coTienEarned = tracker.CalculateMetaCurrency(isVictory);
            }
            else
            {
                // Fallback nếu không có tracker
                coTienEarned = killCount + (minutes * 10) + (isVictory ? 500 : 0);
            }

            string title = isVictory ? "<color=#FFD700>THẮNG RUN — BÌNH YÊN U MINH</color>" : "<color=#FF4444>THẤT BẠI — DIỆM VƯƠNG TRIỆU HỒN</color>";
            string formattedCoTien = $"Cổ Tiền nhận được: <color=#FFD700>+{coTienEarned:N0}</color>";

            // Cộng Cổ Tiền & Lưu Game qua GameManager (tránh double-add nếu đã lưu)
            if (ProjectZombie.Core.Save.GameManager.Instance != null)
            {
                ProjectZombie.Core.Save.GameManager.Instance.OnRunCompleted(elapsedTimeSeconds, killCount, coTienEarned);
                Debug.Log($"[{nameof(RunSummaryPresenter)}] Đã cập nhật kết quả qua GameManager (+{coTienEarned} Cổ Tiền).");
            }
            else if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.AddCurrency(coTienEarned);
                Debug.Log($"[{nameof(RunSummaryPresenter)}] Đã cộng +{coTienEarned} Cổ Tiền!");
            }

            if (_view != null)
            {
                _view.DisplaySummary(title, formattedTime, formattedKills, formattedCoTien);
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnMainMenuClicked -= OnReturnToMainMenu;
                _view.OnRetryClicked -= OnRetryGame;
            }
        }

        private void OnReturnToMainMenu()
        {
            Debug.Log($"[{nameof(RunSummaryPresenter)}] Trở về Main Menu.");
        }

        private void OnRetryGame()
        {
            Debug.Log($"[{nameof(RunSummaryPresenter)}] Thử lại Run mới.");
        }
    }
}

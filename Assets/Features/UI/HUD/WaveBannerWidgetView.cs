using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// View quản lý hiển thị Thẻ Tre thông tin Ải/Wave (Top-Center) và Banner chuyển Wave (Center Popup).
    /// Tuân thủ MVP Pattern và thẩm mỹ Cổ Phong Đông Sơn (Anime Dark Fantasy).
    /// </summary>
    public class WaveBannerWidgetView : MonoBehaviour
    {
        [Header("Top-Center Mini Badge")]
        [SerializeField] private CanvasGroup _miniBadgeCanvasGroup;
        [SerializeField] private TextMeshProUGUI _stageTitleText;    // Ví dụ: "AI 1: U MINH GIOI"
        [SerializeField] private TextMeshProUGUI _waveIndexText;     // Ví dụ: "DOT 03 / 10"

        [Header("Center Transition Banner")]
        [SerializeField] private CanvasGroup _bannerCanvasGroup;
        [SerializeField] private RectTransform _bannerContainer;
        [SerializeField] private Image _bannerFrameImage;
        [SerializeField] private TextMeshProUGUI _bannerTitleText;   // Ví dụ: "DOT 3: BAY QUY XUONG BAO VAY!"
        [SerializeField] private TextMeshProUGUI _bannerSubText;     // Ví dụ: "BOC PHAT (BURST WAVE)"

        [Header("Visual Theme Colors")]
        [SerializeField] private Color _normalWaveColor = new Color(1f, 0.63f, 0f);      // Vàng Hổ Phách (#FFA000)
        [SerializeField] private Color _burstWaveColor = new Color(0.3f, 0.93f, 0.92f);   // Xanh Phong Lôi (#4DEEEA)
        [SerializeField] private Color _bossWaveColor = new Color(1f, 0.15f, 0.15f);      // Đỏ Chu Sa (#FF2626)

        private Coroutine _bannerCoroutine;
        private Sequence _bannerSequence;

        private void Awake()
        {
            if (_bannerCanvasGroup != null)
            {
                _bannerCanvasGroup.alpha = 0f;
                _bannerCanvasGroup.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _bannerSequence?.Kill();
        }

        /// <summary>
        /// Cập nhật nội dung hiển thị cố định trên Thẻ Tre (Top-Center Badge).
        /// </summary>
        public void UpdateMiniBadge(string stageTitle, string waveIndexStr)
        {
            if (_stageTitleText != null) _stageTitleText.text = stageTitle;
            if (_waveIndexText != null) _waveIndexText.text = waveIndexStr;

            // Hiệu ứng Punch Scale nhẹ để báo hiệu đổi wave
            if (_miniBadgeCanvasGroup != null)
            {
                _miniBadgeCanvasGroup.transform.DOKill();
                _miniBadgeCanvasGroup.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.25f, 5, 0.5f);
            }
        }

        /// <summary>
        /// Kích hoạt hoạt ảnh hiển thị Banner Đột Phá ở giữa màn hình.
        /// </summary>
        public void PlayWaveTransitionBanner(string title, string subTitle, TimelineEventType eventType, float displayDuration = 1.8f)
        {
            if (_bannerCanvasGroup == null || _bannerContainer == null) return;

            // Xác định màu sắc chủ đạo theo loại sự kiện
            Color themeColor = _normalWaveColor;
            if (eventType == TimelineEventType.BurstWave) themeColor = _burstWaveColor;
            else if (eventType == TimelineEventType.BossSpawn) themeColor = _bossWaveColor;

            if (_bannerTitleText != null)
            {
                _bannerTitleText.text = title;
                _bannerTitleText.color = themeColor;
            }

            if (_bannerSubText != null)
            {
                _bannerSubText.text = subTitle;
                _bannerSubText.color = themeColor;
            }

            // Hủy sequence hoạt ảnh cũ nếu đang chạy
            _bannerSequence?.Kill();
            _bannerSequence = DOTween.Sequence();
            _bannerSequence.SetUpdate(true); // Chạy mượt mà kể cả khi Time.timeScale = 0

            _bannerCanvasGroup.gameObject.SetActive(true);
            _bannerCanvasGroup.alpha = 0f;
            _bannerContainer.localScale = new Vector3(0.7f, 0.7f, 1f);

            // 1. Phóng to mượt mà + Fade In
            _bannerSequence.Append(_bannerCanvasGroup.DOFade(1f, 0.25f));
            _bannerSequence.Join(_bannerContainer.DOScale(1f, 0.35f).SetEase(Ease.OutBack));

            // Nếu là Boss, tạo rung lắc nhẹ (Shake) tạo cảm giác uy lực
            if (eventType == TimelineEventType.BossSpawn)
            {
                _bannerSequence.Append(_bannerContainer.DOShakePosition(0.4f, 15f, 20, 90, false, true));
            }

            // 2. Giữ nguyên trên màn hình cho người chơi đọc
            _bannerSequence.AppendInterval(displayDuration);

            // 3. Fade Out và trượt nhẹ lên trên
            _bannerSequence.Append(_bannerCanvasGroup.DOFade(0f, 0.3f));
            _bannerSequence.Join(_bannerContainer.DOScale(1.1f, 0.3f).SetEase(Ease.InQuad));

            _bannerSequence.OnComplete(() =>
            {
                if (_bannerCanvasGroup != null)
                {
                    _bannerCanvasGroup.gameObject.SetActive(false);
                }
            });
        }
    }
}

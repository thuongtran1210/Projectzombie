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
        [SerializeField] private TextMeshProUGUI _waveIndexText;     // Ví dụ: "HOI 01 / 10"

        [Header("Monster Stage Progress Bar")]
        [SerializeField] private Slider _stageProgressSlider;
        [SerializeField] private Image _stageProgressFillImage;
        [SerializeField] private TextMeshProUGUI _stageProgressText; // Ví dụ: "03:45 / 20:00 (18%)"
        [SerializeField] private TextMeshProUGUI _waveDescriptionText; // Ví dụ: "Giai doan: Ma Giap quai binh"
        [SerializeField] private GameObject _bossMarkerIcon;

        [Header("Stage Monster Timeline Markers")]
        [SerializeField] private RectTransform _timelineMarkersContainer;
        [SerializeField] private Sprite _markerBgSprite;

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
        [SerializeField] private Color _pillarWaveColor = new Color(0.85f, 0.65f, 0.15f); // Hoàng Kim Thạch (#D9A626)

        private readonly System.Collections.Generic.List<TimelineMarkerUI> _activeMarkers = new System.Collections.Generic.List<TimelineMarkerUI>();
        private Coroutine _bannerCoroutine;
        private Sequence _bannerSequence;
        private Tweener _progressTweener;
        private LevelTimelineConfig _cachedTimelineConfig;

        private class TimelineMarkerUI
        {
            public float timestamp;
            public float normalizedPos;
            public TimelineEventType eventType;
            public GameObject markerObj;
            public Image frameBg;
            public Image iconImage;
            public bool isPassed;
        }

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
            _progressTweener?.Kill();
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
        /// Cập nhật thanh tiến trình quái xuất hiện theo giai đoạn (0..1) và mô tả.
        /// </summary>
        public void UpdateStageProgress(float progress, string progressTimeStr, string waveDescription = "")
        {
            float targetValue = Mathf.Clamp01(progress);

            if (_stageProgressSlider != null)
            {
                _stageProgressSlider.value = targetValue;
            }

            if (_stageProgressFillImage != null)
            {
                _stageProgressFillImage.fillAmount = targetValue;
            }

            if (_stageProgressText != null && !string.IsNullOrEmpty(progressTimeStr))
            {
                _stageProgressText.text = progressTimeStr;
            }

            if (_waveDescriptionText != null && !string.IsNullOrEmpty(waveDescription))
            {
                _waveDescriptionText.text = waveDescription;
            }
        }

        /// <summary>
        /// Khởi tạo và phân bổ các mốc Icon quái trên thanh tiến trình theo đúng cấu hình Timeline Config.
        /// </summary>
        public void SetupTimelineMarkers(LevelTimelineConfig config)
        {
            if (config == null || config.events == null || config.events.Count == 0) return;

            // Nếu đã từng setup đúng config này rồi thì không cần tạo lại GameObject
            if (_cachedTimelineConfig == config && _activeMarkers.Count > 0) return;
            _cachedTimelineConfig = config;

            // Tìm container nếu chưa được serialize
            if (_timelineMarkersContainer == null)
            {
                var found = transform.Find("ProgressBar_StageMonster/Container_TimelineMarkers");
                if (found == null) found = transform.Find("MiniBadge_Bamboo/ProgressBar_StageMonster/Container_TimelineMarkers");
                if (found != null) _timelineMarkersContainer = found.GetComponent<RectTransform>();
            }

            if (_timelineMarkersContainer == null) return;

            // Dọn sạch marker cũ
            for (int i = _timelineMarkersContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_timelineMarkersContainer.GetChild(i).gameObject);
            }
            _activeMarkers.Clear();

            float totalDuration = Mathf.Max(1f, config.maxLevelDuration);

            // Duyệt danh sách Timeline Events để đặt Marker
            for (int i = 0; i < config.events.Count; i++)
            {
                var evt = config.events[i];
                if (evt == null) continue;

                float normX = Mathf.Clamp01(evt.timestampSeconds / totalDuration);
                Sprite iconSprite = evt.GetIcon();

                // Tạo Marker GameObject
                GameObject markerObj = new GameObject($"Marker_{i:D2}_{evt.eventType}", typeof(RectTransform));
                markerObj.transform.SetParent(_timelineMarkersContainer, false);

                RectTransform mRT = markerObj.GetComponent<RectTransform>();
                mRT.anchorMin = new Vector2(normX, 0.5f);
                mRT.anchorMax = new Vector2(normX, 0.5f);
                mRT.pivot = new Vector2(0.5f, 0.5f);
                mRT.anchoredPosition = Vector2.zero;

                float markerSize = (evt.eventType == TimelineEventType.BossSpawn) ? 28f : 22f;
                mRT.sizeDelta = new Vector2(markerSize, markerSize);

                // Khung viền Marker (Gỗ Mun / Viền màu theo loại sự kiện)
                GameObject bgObj = new GameObject("Img_FrameBg", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(markerObj.transform, false);
                RectTransform bgRT = bgObj.GetComponent<RectTransform>();
                bgRT.anchorMin = Vector2.zero;
                bgRT.anchorMax = Vector2.one;
                bgRT.sizeDelta = Vector2.zero;
                Image bgImg = bgObj.GetComponent<Image>();
                if (_markerBgSprite != null) bgImg.sprite = _markerBgSprite;
                bgImg.color = GetColorForEventType(evt.eventType);

                // Icon quái / sự kiện
                GameObject iconObj = new GameObject("Img_MonsterIcon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(markerObj.transform, false);
                RectTransform iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.12f, 0.12f);
                iconRT.anchorMax = new Vector2(0.88f, 0.88f);
                iconRT.sizeDelta = Vector2.zero;
                Image iconImg = iconObj.GetComponent<Image>();
                iconImg.preserveAspect = true;

                if (iconSprite != null)
                {
                    iconImg.sprite = iconSprite;
                    iconImg.color = Color.white;
                }
                else
                {
                    // Fallback nếu chưa có sprite
                    iconImg.color = new Color(1f, 1f, 1f, 0.4f);
                }

                _activeMarkers.Add(new TimelineMarkerUI
                {
                    timestamp = evt.timestampSeconds,
                    normalizedPos = normX,
                    eventType = evt.eventType,
                    markerObj = markerObj,
                    frameBg = bgImg,
                    iconImage = iconImg,
                    isPassed = false
                });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái của các Icon quái trên Timeline khi trận đấu diễn ra.
        /// </summary>
        public void UpdateMarkerStatus(float matchTime, float progress)
        {
            if (_activeMarkers == null || _activeMarkers.Count == 0) return;

            for (int i = 0; i < _activeMarkers.Count; i++)
            {
                var marker = _activeMarkers[i];
                if (marker == null || marker.markerObj == null) continue;

                if (matchTime >= marker.timestamp)
                {
                    if (!marker.isPassed)
                    {
                        marker.isPassed = true;
                        // Hiệu ứng nảy nhẹ khi người chơi cán mốc quái này
                        marker.markerObj.transform.DOKill();
                        marker.markerObj.transform.DOPunchScale(new Vector3(0.22f, 0.22f, 0f), 0.3f, 5, 0.5f);

                        // Đổi màu viền biểu thị đã qua mốc
                        if (marker.frameBg != null)
                        {
                            marker.frameBg.color = new Color(1f, 0.85f, 0.4f, 0.85f); // Vàng Hoàng Kim
                        }
                        if (marker.iconImage != null)
                        {
                            marker.iconImage.color = new Color(0.9f, 0.9f, 0.9f, 0.7f);
                        }
                    }
                }
                else
                {
                    marker.isPassed = false;
                    if (marker.frameBg != null)
                    {
                        marker.frameBg.color = GetColorForEventType(marker.eventType);
                    }
                    if (marker.iconImage != null)
                    {
                        marker.iconImage.color = Color.white;
                    }
                }
            }
        }

        private Color GetColorForEventType(TimelineEventType eventType)
        {
            switch (eventType)
            {
                case TimelineEventType.BossSpawn:
                    return _bossWaveColor;
                case TimelineEventType.BurstWave:
                    return _burstWaveColor;
                case TimelineEventType.SpawnPillar:
                    return _pillarWaveColor;
                case TimelineEventType.Continuous:
                default:
                    return _normalWaveColor;
            }
        }

        /// <summary>
        /// Kích hoạt hoạt ảnh hiển thị Banner Đột Phá ở giữa màn hình.
        /// </summary>
        public void PlayWaveTransitionBanner(string title, string subTitle, TimelineEventType eventType, float displayDuration = 1.8f)
        {
            if (_bannerCanvasGroup == null || _bannerContainer == null) return;

            // Xác định màu sắc chủ đạo theo loại sự kiện
            Color themeColor = GetColorForEventType(eventType);

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

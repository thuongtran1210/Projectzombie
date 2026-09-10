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
        [SerializeField] private TextMeshProUGUI _stageProgressText; // Ví dụ: "03:45 / 15:00 (25%)"
        [SerializeField] private TextMeshProUGUI _waveDescriptionText; // Ví dụ: "Giai doan: Ma Giap quai binh"
        [SerializeField] private GameObject _bossMarkerIcon;

        [Header("Stage Monster Timeline Markers & Pins")]
        [SerializeField] private RectTransform _timelineMarkersContainer;
        [SerializeField] private RectTransform _playerIndicatorPin; // Con trỏ ngọc Chim Lạc chạy theo thời gian
        [SerializeField] private Sprite _playerIndicatorSprite;
        [SerializeField] private Sprite _phaseDividerSprite;       // Trụ đồng phân chia Phase
        [SerializeField] private Sprite _markerBgSprite;
        [SerializeField] private Sprite _swarmBadgeSprite;         // Badge song kiếm
        [SerializeField] private Sprite _eliteBadgeSprite;         // Badge đầu trâu
        [SerializeField] private Sprite _finalBossBadgeSprite;     // Badge Rồng Lửa Diêm Vương

        [Header("Center Transition Banner (Epic Center Panel)")]
        [SerializeField] private CanvasGroup _bannerCanvasGroup;
        [SerializeField] private RectTransform _bannerContainer;
        [SerializeField] private Image _bannerDimBackdrop;          // Lớp phủ mờ nền tối chiến trường
        [SerializeField] private Image _bannerFrameImage;
        [SerializeField] private Image _bannerIconBadge;           // Huy hiệu biểu tượng sự kiện (Song kiếm / Đầu quỷ / Trụ đá)
        [SerializeField] private TextMeshProUGUI _bannerTagText;    // Ví dụ: "✦ HỒI THỨ BA ✦"
        [SerializeField] private TextMeshProUGUI _bannerTitleText;  // Ví dụ: "BẦY QUỶ XƯƠNG BAO VÂY!"
        [SerializeField] private TextMeshProUGUI _bannerSubText;    // Ví dụ: "⚠ BỘC PHÁT (BURST WAVE) - QUÁI TĂNG TỐC"

        [Header("Visual Theme Colors")]
        [SerializeField] private Color _normalWaveColor = new Color(1f, 0.63f, 0f);      // Vàng Hổ Phách (#FFA000)
        [SerializeField] private Color _burstWaveColor = new Color(0.3f, 0.93f, 0.92f);   // Xanh Phong Lôi (#4DEEEA)
        [SerializeField] private Color _bossWaveColor = new Color(1f, 0.15f, 0.15f);      // Đỏ Chu Sa (#FF2626)
        [SerializeField] private Color _pillarWaveColor = new Color(0.85f, 0.65f, 0.15f); // Hoàng Kim Thạch (#D9A626)

        private readonly System.Collections.Generic.List<TimelineMarkerUI> _activeMarkers = new System.Collections.Generic.List<TimelineMarkerUI>();
        private readonly System.Collections.Generic.List<GameObject> _phaseDividers = new System.Collections.Generic.List<GameObject>();
        private Coroutine _bannerCoroutine;
        private Sequence _bannerSequence;
        private Tweener _progressTweener;
        private Tweener _playerPinPulseTweener;
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
            public bool isWarningPulsing;
            public Tweener warningTweener;
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
            _playerPinPulseTweener?.Kill();
            if (_activeMarkers != null)
            {
                foreach (var marker in _activeMarkers)
                {
                    marker?.warningTweener?.Kill();
                }
            }
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

            // Đồng bộ vị trí của Con trỏ người chơi (Player Indicator Pin)
            UpdatePlayerIndicatorPosition(targetValue);
        }

        private void UpdatePlayerIndicatorPosition(float normalizedProgress)
        {
            if (_playerIndicatorPin == null && _timelineMarkersContainer != null)
            {
                EnsurePlayerIndicatorCreated();
            }

            if (_playerIndicatorPin != null)
            {
                _playerIndicatorPin.anchorMin = new Vector2(normalizedProgress, 0.5f);
                _playerIndicatorPin.anchorMax = new Vector2(normalizedProgress, 0.5f);
                _playerIndicatorPin.anchoredPosition = new Vector2(0f, 14f); // Nhô nhẹ lên trên thanh bar
            }
        }

        private void EnsureSpritesLoaded()
        {
            if (_playerIndicatorSprite == null) _playerIndicatorSprite = Resources.Load<Sprite>("UI/HUD/Pin_Player_LacBird") ?? Resources.Load<Sprite>("Pin_Player_LacBird");
            if (_phaseDividerSprite == null) _phaseDividerSprite = Resources.Load<Sprite>("UI/HUD/Pin_Phase_Divider") ?? Resources.Load<Sprite>("Pin_Phase_Divider");
            if (_swarmBadgeSprite == null) _swarmBadgeSprite = Resources.Load<Sprite>("UI/HUD/Badge_Swarm_Swords") ?? Resources.Load<Sprite>("Badge_Swarm_Swords");
            if (_eliteBadgeSprite == null) _eliteBadgeSprite = Resources.Load<Sprite>("UI/HUD/Badge_Elite_OxHead") ?? Resources.Load<Sprite>("Badge_Elite_OxHead");
            if (_finalBossBadgeSprite == null) _finalBossBadgeSprite = Resources.Load<Sprite>("UI/HUD/Badge_Boss_Dragon") ?? Resources.Load<Sprite>("Badge_Boss_Dragon");

#if UNITY_EDITOR
            if (_playerIndicatorSprite == null)
                _playerIndicatorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Pin_Player_LacBird.png");
            if (_phaseDividerSprite == null)
                _phaseDividerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Pin_Phase_Divider.png");
            if (_swarmBadgeSprite == null)
                _swarmBadgeSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Badge_Swarm_Swords.png");
            if (_eliteBadgeSprite == null)
                _eliteBadgeSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Badge_Elite_OxHead.png");
            if (_finalBossBadgeSprite == null)
                _finalBossBadgeSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Badge_Boss_Dragon.png");
#endif
        }

        private void EnsurePlayerIndicatorCreated()
        {
            EnsureSpritesLoaded();
            if (_playerIndicatorPin != null || _timelineMarkersContainer == null) return;

            GameObject pinObj = new GameObject("PlayerIndicatorPin_Jade", typeof(RectTransform), typeof(Image));
            pinObj.transform.SetParent(_timelineMarkersContainer.parent != null ? _timelineMarkersContainer.parent : _timelineMarkersContainer, false);
            _playerIndicatorPin = pinObj.GetComponent<RectTransform>();
            _playerIndicatorPin.pivot = new Vector2(0.5f, 0f); // Điểm nhọn cắm xuống thanh bar
            _playerIndicatorPin.sizeDelta = new Vector2(26f, 38f);
            _playerIndicatorPin.SetAsLastSibling(); // Luôn nổi lên trên cùng

            Image pinImg = pinObj.GetComponent<Image>();
            pinImg.preserveAspect = true;
            if (_playerIndicatorSprite != null) pinImg.sprite = _playerIndicatorSprite;
            else pinImg.color = new Color(0.3f, 0.95f, 0.7f, 1f); // Xanh ngọc phát sáng

            // Hoạt ảnh bập bùng nhẹ của ngọc linh hồn
            _playerPinPulseTweener?.Kill();
            _playerPinPulseTweener = _playerIndicatorPin.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.7f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// Khởi tạo và phân bổ các mốc Icon quái trên thanh tiến trình theo đúng cấu hình Timeline Config.
        /// </summary>
        public void SetupTimelineMarkers(LevelTimelineConfig config)
        {
            if (config == null || config.events == null || config.events.Count == 0) return;

            if (_cachedTimelineConfig == config && _activeMarkers.Count > 0) return;
            _cachedTimelineConfig = config;

            EnsureSpritesLoaded();

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
                var child = _timelineMarkersContainer.GetChild(i);
                if (child != null && child.gameObject != null && child != _playerIndicatorPin)
                {
                    Destroy(child.gameObject);
                }
            }
            _activeMarkers.Clear();
            _phaseDividers.Clear();

            float totalDuration = Mathf.Max(1f, config.maxLevelDuration);

            // 1. Tạo 2 Vạch Phân Giai Đoạn (Phase Divider Pins) tại phút 05:00 (33.3%) và 10:00 (66.6%)
            CreatePhaseDivider(0.333f, "Divider_Phase1_2");
            CreatePhaseDivider(0.666f, "Divider_Phase2_3");

            // 2. Duyệt danh sách Timeline Events để đặt Marker phân cấp
            for (int i = 0; i < config.events.Count; i++)
            {
                var evt = config.events[i];
                if (evt == null) continue;

                float normX = Mathf.Clamp01(evt.timestampSeconds / totalDuration);
                Sprite iconSprite = evt.GetIcon();

                // Phân cấp kích thước hiển thị
                float markerSize = 22f;
                if (evt.eventType == TimelineEventType.BossSpawn)
                {
                    markerSize = (evt.timestampSeconds >= totalDuration - 5f) ? 38f : 32f; // Final Boss 38px, Mid-Boss 32px
                }
                else if (evt.eventType == TimelineEventType.BurstWave)
                {
                    markerSize = 26f;
                }

                GameObject markerObj = new GameObject($"Marker_{i:D2}_{evt.eventType}", typeof(RectTransform));
                markerObj.transform.SetParent(_timelineMarkersContainer, false);

                RectTransform mRT = markerObj.GetComponent<RectTransform>();
                mRT.anchorMin = new Vector2(normX, 0.5f);
                mRT.anchorMax = new Vector2(normX, 0.5f);
                mRT.pivot = new Vector2(0.5f, 0.5f);
                mRT.anchoredPosition = Vector2.zero;
                mRT.sizeDelta = new Vector2(markerSize, markerSize);

                // Khung viền Marker
                GameObject bgObj = new GameObject("Img_FrameBg", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(markerObj.transform, false);
                RectTransform bgRT = bgObj.GetComponent<RectTransform>();
                bgRT.anchorMin = Vector2.zero;
                bgRT.anchorMax = Vector2.one;
                bgRT.sizeDelta = Vector2.zero;
                Image bgImg = bgObj.GetComponent<Image>();
                
                // Gán khung đặc thù nếu có sprite riêng
                if (evt.eventType == TimelineEventType.BossSpawn && evt.timestampSeconds >= totalDuration - 5f && _finalBossBadgeSprite != null)
                {
                    bgImg.sprite = _finalBossBadgeSprite;
                }
                else if (evt.eventType == TimelineEventType.BossSpawn && _eliteBadgeSprite != null)
                {
                    bgImg.sprite = _eliteBadgeSprite;
                }
                else if (evt.eventType == TimelineEventType.BurstWave && _swarmBadgeSprite != null)
                {
                    bgImg.sprite = _swarmBadgeSprite;
                }
                else if (_markerBgSprite != null)
                {
                    bgImg.sprite = _markerBgSprite;
                }

                bgImg.color = GetColorForEventType(evt.eventType);

                // Icon quái
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
                    isPassed = false,
                    isWarningPulsing = false
                });
            }

            EnsurePlayerIndicatorCreated();
        }

        private void CreatePhaseDivider(float normalizedX, string name)
        {
            GameObject divObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            divObj.transform.SetParent(_timelineMarkersContainer, false);
            RectTransform divRT = divObj.GetComponent<RectTransform>();
            divRT.anchorMin = new Vector2(normalizedX, 0.5f);
            divRT.anchorMax = new Vector2(normalizedX, 0.5f);
            divRT.pivot = new Vector2(0.5f, 0.5f);
            divRT.anchoredPosition = Vector2.zero;
            divRT.sizeDelta = new Vector2(10f, 28f);

            Image divImg = divObj.GetComponent<Image>();
            divImg.preserveAspect = true;
            if (_phaseDividerSprite != null) divImg.sprite = _phaseDividerSprite;
            else divImg.color = new Color(0.85f, 0.7f, 0.3f, 0.85f); // Trụ đồng

            _phaseDividers.Add(divObj);
        }

        /// <summary>
        /// Cập nhật trạng thái của các Icon quái trên Timeline (Đã qua mốc / Cảnh báo radar sắp tới).
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
                    if (marker.isWarningPulsing)
                    {
                        marker.isWarningPulsing = false;
                        marker.warningTweener?.Kill();
                        marker.markerObj.transform.localScale = Vector3.one;
                    }

                    if (!marker.isPassed)
                    {
                        marker.isPassed = true;
                        marker.markerObj.transform.DOKill();
                        marker.markerObj.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.35f, 5, 0.5f);

                        if (marker.frameBg != null)
                        {
                            marker.frameBg.color = new Color(1f, 0.85f, 0.4f, 0.85f);
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
                    float timeRemaining = marker.timestamp - matchTime;

                    // Hiệu ứng Cảnh báo Radar (Warning Pulse) khi còn 20s trước mốc Boss hoặc Burst Wave
                    if (timeRemaining <= 20f && (marker.eventType == TimelineEventType.BossSpawn || marker.eventType == TimelineEventType.BurstWave))
                    {
                        if (!marker.isWarningPulsing)
                        {
                            marker.isWarningPulsing = true;
                            marker.warningTweener?.Kill();
                            marker.warningTweener = marker.markerObj.transform.DOScale(1.25f, 0.4f)
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetEase(Ease.InOutSine);

                            if (marker.frameBg != null)
                            {
                                marker.frameBg.color = new Color(1f, 0.2f, 0.2f, 1f); // Rực đỏ cảnh báo
                            }
                        }
                    }
                    else
                    {
                        if (marker.isWarningPulsing)
                        {
                            marker.isWarningPulsing = false;
                            marker.warningTweener?.Kill();
                            marker.markerObj.transform.localScale = Vector3.one;
                        }

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
        /// Kích hoạt hoạt ảnh hiển thị Đại Banner Thông Báo Đột Phá ở chính giữa màn hình.
        /// Tự động sử dụng trực tiếp Icon có sẵn của Đợt quái / Boss từ Wave Timeline Config.
        /// </summary>
        public void PlayWaveTransitionBanner(string tagText, string title, string subTitle, TimelineEventType eventType, Sprite waveIcon = null, float displayDuration = 2.0f)
        {
            if (_bannerCanvasGroup == null || _bannerContainer == null) return;

            EnsureSpritesLoaded();

            // Xác định màu sắc chủ đạo theo loại sự kiện
            Color themeColor = GetColorForEventType(eventType);

            if (_bannerTagText != null)
            {
                _bannerTagText.text = tagText;
                _bannerTagText.color = new Color(1f, 0.88f, 0.5f); // Vàng Sớ Kim
            }

            if (_bannerTitleText != null)
            {
                _bannerTitleText.text = title;
                _bannerTitleText.color = themeColor;
            }

            if (_bannerSubText != null)
            {
                _bannerSubText.text = subTitle;
                _bannerSubText.color = new Color(0.95f, 0.92f, 0.88f);
            }

            // Gán Icon Badge: Ưu tiên 100% icon quái/Boss có sẵn của đợt quái hiện tại
            if (_bannerIconBadge != null)
            {
                Sprite badgeSp = waveIcon;

                // Fallback nếu đợt quái không gán icon riêng
                if (badgeSp == null)
                {
                    switch (eventType)
                    {
                        case TimelineEventType.BossSpawn:
                            badgeSp = _finalBossBadgeSprite ?? _eliteBadgeSprite;
                            break;
                        case TimelineEventType.BurstWave:
                            badgeSp = _swarmBadgeSprite;
                            break;
                        case TimelineEventType.SpawnPillar:
                            badgeSp = _eliteBadgeSprite;
                            break;
                        default:
                            badgeSp = _playerIndicatorSprite;
                            break;
                    }
                }

                if (badgeSp != null)
                {
                    _bannerIconBadge.sprite = badgeSp;
                    _bannerIconBadge.color = (waveIcon != null) ? Color.white : themeColor; // Giữ nguyên màu thật của icon quái
                    _bannerIconBadge.gameObject.SetActive(true);
                }
                else
                {
                    _bannerIconBadge.gameObject.SetActive(false);
                }
            }

            // Hủy sequence hoạt ảnh cũ nếu đang chạy
            _bannerSequence?.Kill();
            _bannerSequence = DOTween.Sequence();
            _bannerSequence.SetUpdate(true); // Chạy mượt mà kể cả khi Time.timeScale = 0

            _bannerCanvasGroup.gameObject.SetActive(true);
            _bannerCanvasGroup.alpha = 0f;

            // Đặt trạng thái ban đầu cho Container & Backdrop
            _bannerContainer.localScale = new Vector3(1.35f, 1.35f, 1f);
            _bannerContainer.anchoredPosition = new Vector2(0f, 0f);

            if (_bannerDimBackdrop != null)
            {
                Color bdCol = _bannerDimBackdrop.color;
                bdCol.a = 0f;
                _bannerDimBackdrop.color = bdCol;
            }

            // GIAI ĐOẠN 1: KHỞI PHÁT & VA ĐẬP (IMPACT)
            _bannerSequence.Append(_bannerCanvasGroup.DOFade(1f, 0.22f));
            if (_bannerDimBackdrop != null)
            {
                _bannerSequence.Join(_bannerDimBackdrop.DOFade(0.45f, 0.25f));
            }
            _bannerSequence.Join(_bannerContainer.DOScale(1f, 0.32f).SetEase(Ease.OutBack));

            // Nếu là Boss, tạo chấn động rung lắc dữ dội
            if (eventType == TimelineEventType.BossSpawn)
            {
                _bannerSequence.Append(_bannerContainer.DOShakePosition(0.45f, 20f, 25, 90, false, true));
            }

            // GIAI ĐOẠN 2: LƯU GIỮ & TỎA SÁNG (HOLD & PULSE)
            _bannerSequence.AppendInterval(displayDuration);

            // GIAI ĐOẠN 3: BỐC HƠI & GIẢI TỎA CHIẾN TRẬN (FADE & FLOAT UP)
            _bannerSequence.Append(_bannerCanvasGroup.DOFade(0f, 0.3f));
            if (_bannerDimBackdrop != null)
            {
                _bannerSequence.Join(_bannerDimBackdrop.DOFade(0f, 0.25f));
            }
            _bannerSequence.Join(_bannerContainer.DOAnchorPosY(80f, 0.3f).SetEase(Ease.InQuad));
            _bannerSequence.Join(_bannerContainer.DOScale(1.08f, 0.3f).SetEase(Ease.InQuad));

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

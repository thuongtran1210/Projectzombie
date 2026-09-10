using UnityEngine;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// Presenter điều phối hiển thị thông tin Ải, Đợt quái và Thanh tiến trình quái xuất hiện theo giai đoạn.
    /// Độc lập 100% với RunHUDPresenter, tự động đồng bộ ngay khi khởi tạo và cập nhật thời gian thực.
    /// </summary>
    public class WaveBannerWidgetPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private WaveBannerWidgetView _view;

        private string _lastWaveDesc = "";

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<WaveBannerWidgetView>();
            }
        }

        private void Start()
        {
            SyncCurrentStageData();
        }

        private void OnEnable()
        {
            SpawnManager.OnWaveTriggered += HandleWaveTriggered;
            SpawnManager.OnTimelineProgressUpdated += HandleTimelineProgressUpdated;
            SyncCurrentStageData();
        }

        private void OnDisable()
        {
            SpawnManager.OnWaveTriggered -= HandleWaveTriggered;
            SpawnManager.OnTimelineProgressUpdated -= HandleTimelineProgressUpdated;
        }

        private void Update()
        {
            // Dự phòng cập nhật mượt mà khi SpawnManager đang chạy
            if (SpawnManager.Instance != null && SpawnManager.Instance.IsMatchActive)
            {
                float time = SpawnManager.Instance.MatchTime;
                float maxTime = SpawnManager.Instance.LevelDuration;
                float progress = SpawnManager.Instance.MatchProgress;
                UpdateProgressView(time, maxTime, progress);
            }
        }

        /// <summary>
        /// Đồng bộ trực tiếp dữ liệu số màn chơi và đợt quái từ SpawnManager (tránh bị rỗng hoặc lệch nhịp khi khởi động).
        /// </summary>
        public void SyncCurrentStageData()
        {
            if (_view == null) return;

            var spawner = SpawnManager.Instance;
            if (spawner != null)
            {
                string stageTitle = spawner.CurrentStageName.ToUpper();
                int currentWave = spawner.CurrentWaveIndex;
                int totalWaves = spawner.TotalWaves;
                string waveIndexStr = $"HOI {currentWave:D2} / {totalWaves:D2}";

                _view.UpdateMiniBadge(stageTitle, waveIndexStr);

                if (spawner.TimelineConfig != null)
                {
                    _view.SetupTimelineMarkers(spawner.TimelineConfig);
                }

                var activeEvt = spawner.CurrentActiveEvent;
                if (activeEvt != null && !string.IsNullOrEmpty(activeEvt.eventName))
                {
                    _lastWaveDesc = activeEvt.eventName;
                }

                UpdateProgressView(spawner.MatchTime, spawner.LevelDuration, spawner.MatchProgress);
            }
            else
            {
                _view.UpdateMiniBadge("MAN 1: U MINH GIOI", "HOI 01 / 10");
                _view.UpdateStageProgress(0f, "00:00 / 15:00 (0%)", "Chuan bi chien dau");
            }
        }

        private void HandleWaveTriggered(WaveInfo info)
        {
            if (_view == null) return;

            // 1. Format dữ liệu cho Thẻ Tre Mini (Top-Center)
            string stageTitle = $"{info.stageName.ToUpper()}";
            string waveIndexStr = $"HOI {info.currentWaveIndex:D2} / {info.totalWaves:D2}";
            _view.UpdateMiniBadge(stageTitle, waveIndexStr);

            _lastWaveDesc = info.waveTitle;

            if (SpawnManager.Instance != null && SpawnManager.Instance.TimelineConfig != null)
            {
                _view.SetupTimelineMarkers(SpawnManager.Instance.TimelineConfig);
            }

            // 2. Format tiêu đề và phụ đề cho Banner Pop-up chuyển Wave
            string bannerTag = $"HỒI THỨ {info.currentWaveIndex:D2} / {info.totalWaves:D2}";
            
            // Tối ưu chuỗi tiêu đề: Lọc bỏ prefix kỹ thuật thời gian (vd: 'Phút 00:00 - ') để tiêu đề ngắn gọn, uy lực
            string cleanTitle = info.waveTitle ?? "";
            int dashIdx = cleanTitle.IndexOf(" - ");
            if (dashIdx >= 0 && dashIdx < 20)
            {
                cleanTitle = cleanTitle.Substring(dashIdx + 3).Trim();
            }
            string bannerTitle = cleanTitle.ToUpper();
            string bannerSubText = GetSubTextForEventType(info.eventType);

            // Kích hoạt hoạt ảnh Banner ở giữa màn hình (Ưu tiên lấy icon quái/Boss có sẵn của Wave)
            _view.PlayWaveTransitionBanner(bannerTag, bannerTitle, bannerSubText, info.eventType, info.eventIcon);
        }

        private void HandleTimelineProgressUpdated(float matchTime, float maxDuration, float progress)
        {
            UpdateProgressView(matchTime, maxDuration, progress);
        }

        private int _lastCachedSecond = -1;
        private int _lastCachedProgressPercent = -1;
        private string _lastCachedTimeStr = string.Empty;

        private void UpdateProgressView(float matchTime, float maxDuration, float progress)
        {
            if (_view == null) return;

            int curSec = Mathf.FloorToInt(matchTime);
            int curPercent = Mathf.FloorToInt(progress * 100f);

            if (curSec != _lastCachedSecond || curPercent != _lastCachedProgressPercent)
            {
                _lastCachedSecond = curSec;
                _lastCachedProgressPercent = curPercent;

                int curMin = curSec / 60;
                int remSec = curSec % 60;
                int maxMin = Mathf.FloorToInt(maxDuration / 60f);
                int maxSec = Mathf.FloorToInt(maxDuration % 60f);

                _lastCachedTimeStr = $"{curMin:D2}:{remSec:D2} / {maxMin:D2}:{maxSec:D2} ({curPercent}%)";
            }

            _view.UpdateStageProgress(progress, _lastCachedTimeStr, _lastWaveDesc);
            _view.UpdateMarkerStatus(matchTime, progress);
        }

        private string GetSubTextForEventType(TimelineEventType eventType)
        {
            switch (eventType)
            {
                case TimelineEventType.BurstWave:
                    return "BẦY QUÁI BỘC PHÁT (BURST WAVE)";
                case TimelineEventType.BossSpawn:
                    return "MA VƯƠNG XUẤT THẾ (BOSS SPAWN)";
                case TimelineEventType.SpawnPillar:
                    return "TRỤ MA THẠCH XUẤT THẾ";
                case TimelineEventType.Continuous:
                default:
                    return "TIẾN VÀO TRẬN ĐỊA";
            }
        }
    }
}

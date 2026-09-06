using UnityEngine;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// Presenter điều phối hiển thị thông tin Ải và Đợt quái (Wave Progress).
    /// Độc lập 100% với RunHUDPresenter, tự động đăng ký và hủy đăng ký event an toàn.
    /// </summary>
    public class WaveBannerWidgetPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private WaveBannerWidgetView _view;

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<WaveBannerWidgetView>();
            }
        }

        private void OnEnable()
        {
            SpawnManager.OnWaveTriggered += HandleWaveTriggered;
        }

        private void OnDisable()
        {
            SpawnManager.OnWaveTriggered -= HandleWaveTriggered;
        }

        private void HandleWaveTriggered(WaveInfo info)
        {
            if (_view == null) return;

            // 1. Format dữ liệu cho Thẻ Tre Mini (Top-Center)
            string stageTitle = $"{info.stageName.ToUpper()}";
            string waveIndexStr = $"DOT {info.currentWaveIndex:D2} / {info.totalWaves:D2}";
            _view.UpdateMiniBadge(stageTitle, waveIndexStr);

            // 2. Format tiêu đề và phụ đề cho Banner Pop-up chuyển Wave
            string bannerTitle = $"DOT {info.currentWaveIndex}: {info.waveTitle.ToUpper()}";
            string bannerSubText = GetSubTextForEventType(info.eventType);

            // Kích hoạt hoạt ảnh Banner ở giữa màn hình
            _view.PlayWaveTransitionBanner(bannerTitle, bannerSubText, info.eventType);
        }

        private string GetSubTextForEventType(TimelineEventType eventType)
        {
            switch (eventType)
            {
                case TimelineEventType.BurstWave:
                    return "BAY QUAI BAO VAY (BURST WAVE)";
                case TimelineEventType.BossSpawn:
                    return "MA VUONG XUAT HIEN (BOSS SPAWN)";
                case TimelineEventType.SpawnPillar:
                    return "TRU MA THACH XUAT THE";
                case TimelineEventType.Continuous:
                default:
                    return "TIEN VAO TRAN DIA";
            }
        }
    }
}

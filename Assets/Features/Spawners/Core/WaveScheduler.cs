using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Spawners.Core
{
    /// <summary>
    /// Thuật toán pure C# chịu trách nhiệm quản lý thời gian trận đấu, tính toán tiến trình MatchProgress,
    /// và xác định các mốc đợt quái (TimelineEvent) đến hạn kích hoạt.
    /// Tách rời khỏi MonoBehaviour để phục vụ Unit Testing và tuân thủ SRP.
    /// </summary>
    public class WaveScheduler
    {
        private LevelTimelineConfig _config;
        private float _matchTime;
        private bool _isMatchActive;
        private int _nextEventIndex;

        public float MatchTime => _matchTime;
        public bool IsMatchActive => _isMatchActive;
        public int NextEventIndex => _nextEventIndex;

        public float LevelDuration => (_config != null && _config.maxLevelDuration > 0) ? _config.maxLevelDuration : 900f;
        public float MatchProgress => Mathf.Clamp01(_matchTime / Mathf.Max(1f, LevelDuration));
        public int CurrentWaveIndex => Mathf.Max(1, _nextEventIndex);
        public int TotalWaves => (_config != null && _config.events != null) ? Mathf.Max(1, _config.events.Count) : 1;

        public void Initialize(LevelTimelineConfig config)
        {
            _config = config;
            _matchTime = 0f;
            _isMatchActive = false;
            _nextEventIndex = 0;
        }

        public void StartMatch()
        {
            _matchTime = 0f;
            _isMatchActive = true;
            _nextEventIndex = 0;
        }

        public void StopMatch()
        {
            _isMatchActive = false;
        }

        public void SetMatchTime(float time)
        {
            _matchTime = Mathf.Max(0f, time);
        }

        /// <summary>
        /// Cập nhật thời gian từng frame. Trả về danh sách các sự kiện TimelineEvent đến hạn kích hoạt trong tick này.
        /// </summary>
        public List<TimelineEvent> Tick(float deltaTime, out bool isTimelineEnded)
        {
            isTimelineEnded = false;
            var dueEvents = new List<TimelineEvent>();

            if (!_isMatchActive || _config == null || _config.events == null)
            {
                return dueEvents;
            }

            _matchTime += deltaTime;

            // Duyệt danh sách các sự kiện TimelineEvent có mốc thời gian <= matchTime
            while (_nextEventIndex < _config.events.Count)
            {
                var evt = _config.events[_nextEventIndex];
                if (evt != null && _matchTime >= evt.timestampSeconds)
                {
                    dueEvents.Add(evt);
                    _nextEventIndex++;
                }
                else
                {
                    break;
                }
            }

            if (_matchTime >= LevelDuration)
            {
                isTimelineEnded = true;
            }

            return dueEvents;
        }
    }
}

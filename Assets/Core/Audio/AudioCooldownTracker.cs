using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    /// Theo dõi và kiểm soát tần suất phát âm thanh (Cooldown / Voice limit) để tối ưu cho Android.
    /// Giúp tránh hiện tượng xé âm (audio clipping) khi hàng chục kẻ địch phát cùng 1 âm thanh.
    /// </summary>
    public class AudioCooldownTracker
    {
        private readonly Dictionary<AudioConfigSO, float> _lastPlayedTimes = new Dictionary<AudioConfigSO, float>();
        private readonly Dictionary<AudioConfigSO, int> _activeVoiceCounts = new Dictionary<AudioConfigSO, int>();

        public bool CanPlay(AudioConfigSO config)
        {
            if (config == null) return false;

            float currentTime = Time.unscaledTime;

            // Kiểm tra Cooldown Time
            if (_lastPlayedTimes.TryGetValue(config, out float lastTime))
            {
                if (currentTime - lastTime < config.CooldownTime)
                {
                    return false;
                }
            }

            // Kiểm tra Max Concurrent Voices
            if (_activeVoiceCounts.TryGetValue(config, out int activeCount))
            {
                if (activeCount >= config.MaxConcurrentVoices)
                {
                    return false;
                }
            }

            return true;
        }

        public void RecordPlay(AudioConfigSO config)
        {
            if (config == null) return;
            
            _lastPlayedTimes[config] = Time.unscaledTime;
            
            if (_activeVoiceCounts.ContainsKey(config))
                _activeVoiceCounts[config]++;
            else
                _activeVoiceCounts[config] = 1;
        }

        public void RecordStop(AudioConfigSO config)
        {
            if (config == null) return;

            if (_activeVoiceCounts.ContainsKey(config) && _activeVoiceCounts[config] > 0)
            {
                _activeVoiceCounts[config]--;
            }
        }
    }
}

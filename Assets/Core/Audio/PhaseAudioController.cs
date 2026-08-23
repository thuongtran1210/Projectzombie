using UnityEngine;
using ProjectZombie.Features.Spawners;

namespace Core.Audio
{
    /// <summary>
    /// Điều phối Nhạc nền và Stinger báo hiệu chuyển Phase theo mốc thời gian trận đấu (Atmosphere Palette-Swap).
    /// </summary>
    public class PhaseAudioController : MonoBehaviour
    {
        [System.Serializable]
        public struct PhaseAudioData
        {
            public string phaseName;
            public float timestampSeconds;
            public AudioConfigSO bgmConfig;
            public AudioConfigSO phaseStingerConfig;
        }

        [Header("Phase Audio Configurations")]
        [SerializeField] private PhaseAudioData[] _phaseAudioList;

        private int _currentPhaseIndex = -1;

        private void Start()
        {
            if (ProjectZombie.Features.Shared.GameStateManager.Instance != null && 
                ProjectZombie.Features.Shared.GameStateManager.Instance.CurrentState != ProjectZombie.Features.Shared.GameState.Playing)
            {
                return; // Đang ở Sảnh Main Menu ngoài game, không phát BGM trận đấu
            }

            if (SpawnManager.Instance != null && _phaseAudioList != null && _phaseAudioList.Length > 0)
            {
                CheckPhaseTransition(SpawnManager.Instance.MatchTime);
            }
        }

        private void Update()
        {
            if (ProjectZombie.Features.Shared.GameStateManager.Instance != null && 
                ProjectZombie.Features.Shared.GameStateManager.Instance.CurrentState != ProjectZombie.Features.Shared.GameState.Playing)
            {
                return;
            }

            if (SpawnManager.Instance == null || _phaseAudioList == null) return;

            float currentMatchTime = SpawnManager.Instance.MatchTime;
            CheckPhaseTransition(currentMatchTime);
        }

        private void CheckPhaseTransition(float matchTime)
        {
            int targetIndex = -1;

            for (int i = 0; i < _phaseAudioList.Length; i++)
            {
                if (matchTime >= _phaseAudioList[i].timestampSeconds)
                {
                    targetIndex = i;
                }
            }

            if (targetIndex != _currentPhaseIndex && targetIndex >= 0)
            {
                _currentPhaseIndex = targetIndex;
                TriggerPhaseAudio(_phaseAudioList[_currentPhaseIndex]);
            }
        }

        private void TriggerPhaseAudio(PhaseAudioData phaseData)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning($"[{nameof(PhaseAudioController)}] Không tìm thấy AudioManager.Instance trong Scene!");
                return;
            }

            Debug.Log($"[{nameof(PhaseAudioController)}] Trigger Phase Audio: '{phaseData.phaseName}' tại mốc thời gian {phaseData.timestampSeconds}s.");

            // 1. Phát Stinger báo hiệu chuyển Phase (nếu có)
            if (phaseData.phaseStingerConfig != null)
            {
                AudioManager.Instance.PlayPhaseStinger(phaseData.phaseStingerConfig);
            }

            // 2. Chuyển Nhạc nền (BGM) tương ứng với Phase
            if (phaseData.bgmConfig != null)
            {
                AudioManager.Instance.PlayBGM(phaseData.bgmConfig);
            }
            else
            {
                Debug.LogWarning($"[{nameof(PhaseAudioController)}] Phase '{phaseData.phaseName}' chưa được gán BGMConfig (AudioConfigSO)!");
            }
        }
    }
}

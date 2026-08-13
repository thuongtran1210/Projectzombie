using UnityEngine;
using UnityEngine.Audio;
using ProjectZombie.Features.YinYang;

namespace Core.Audio
{
    /// <summary>
    /// Điều phối Nhạc động (Adaptive/Layered Music) crossfade mượt mà theo biến Cán cân Âm Dương (yinYangValue).
    /// </summary>
    public class AdaptiveBGMController : MonoBehaviour
    {
        [Header("Mixer Snapshots")]
        [SerializeField] private AudioMixerSnapshot _neutralSnapshot;      // Thái Cực (40 - 60)
        [SerializeField] private AudioMixerSnapshot _yangDominantSnapshot; // Dương Thịnh (> 80)
        [SerializeField] private AudioMixerSnapshot _yinDominantSnapshot;  // Âm Thịnh (< 20)

        [Header("Transition Settings")]
        [SerializeField] private float _transitionDuration = 1.5f;

        private YinYangState _lastState = YinYangState.Balanced;

        private void Start()
        {
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.OnYinYangValueChanged += OnYinYangValueChanged;
                // Transition trạng thái ban đầu
                OnYinYangValueChanged(YinYangManager.Instance.CurrentValue, YinYangManager.Instance.GetState());
            }
        }

        private void OnDestroy()
        {
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.OnYinYangValueChanged -= OnYinYangValueChanged;
            }
        }

        private void OnYinYangValueChanged(float value, YinYangState state)
        {
            if (state == _lastState) return;
            _lastState = state;

            AudioMixerSnapshot targetSnapshot = null;

            switch (state)
            {
                case YinYangState.YangDominant:
                    targetSnapshot = _yangDominantSnapshot;
                    break;

                case YinYangState.YinDominant:
                    targetSnapshot = _yinDominantSnapshot;
                    break;

                case YinYangState.Balanced:
                default:
                    targetSnapshot = _neutralSnapshot;
                    break;
            }

            if (targetSnapshot != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.TransitionToSnapshot(targetSnapshot, _transitionDuration);
            }
        }
    }
}

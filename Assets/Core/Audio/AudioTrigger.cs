using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    /// Component hỗ trợ gắn vào các GameObject (Button UI, Animation Event, Trigger) để kích hoạt âm thanh dễ dàng.
    /// </summary>
    public class AudioTrigger : MonoBehaviour
    {
        [SerializeField] private AudioConfigSO _audioConfig;
        [SerializeField] private bool _playOnStart;

        private void Start()
        {
            if (_playOnStart)
            {
                PlayAudio();
            }
        }

        public void PlayAudio()
        {
            if (_audioConfig == null) return;
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(_audioConfig, transform.position);
            }
        }
    }
}

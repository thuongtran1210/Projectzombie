using UnityEngine;
using ProjectZombie.Core.Events;
using Core.Audio;

namespace ProjectZombie.Core.Audio
{
    /// <summary>
    /// Component lắng nghe các Domain Events từ GameEventBus và tự động phát Audio SFX tương ứng.
    /// Giúp Decouple hoàn toàn AudioManager khỏi gameplay code (Enemy, ExpGem, Weapons).
    /// </summary>
    public class AudioEventListener : MonoBehaviour
    {
        [Header("Audio Configs (ScriptableObject)")]
        [SerializeField] private AudioConfigSO _enemyDieSFX;
        [SerializeField] private AudioConfigSO _playerLevelUpSFX;
        [SerializeField] private AudioConfigSO _expCollectSFX;

        private int _expComboCount = 0;
        private float _lastExpCollectTime = -1f;
        private const float COMBO_RESET_DELAY = 0.35f;
        private const float PITCH_STEP = 0.04f;
        private const float MAX_PITCH = 1.5f;

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            GameEventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
            GameEventBus.Subscribe<ExpCollectedEvent>(OnExpCollected);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            GameEventBus.Unsubscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
            GameEventBus.Unsubscribe<ExpCollectedEvent>(OnExpCollected);
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (AudioManager.Instance != null && _enemyDieSFX != null)
            {
                AudioManager.Instance.PlaySound(_enemyDieSFX, evt.Position);
            }
        }

        private void OnPlayerLevelUp(PlayerLevelUpEvent evt)
        {
            if (AudioManager.Instance != null && _playerLevelUpSFX != null)
            {
                AudioManager.Instance.PlaySound(_playerLevelUpSFX);
            }
        }

        private void OnExpCollected(ExpCollectedEvent evt)
        {
            if (AudioManager.Instance != null && _expCollectSFX != null)
            {
                float currentTime = Time.unscaledTime;
                if (currentTime - _lastExpCollectTime < COMBO_RESET_DELAY)
                {
                    _expComboCount++;
                }
                else
                {
                    _expComboCount = 0;
                }
                _lastExpCollectTime = currentTime;

                float calculatedPitch = Mathf.Min(1.0f + _expComboCount * PITCH_STEP, MAX_PITCH);
                AudioManager.Instance.PlaySound(_expCollectSFX, evt.Position, calculatedPitch);
            }
        }
    }
}

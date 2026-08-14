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
                AudioManager.Instance.PlaySound(_expCollectSFX, evt.Position);
            }
        }
    }
}

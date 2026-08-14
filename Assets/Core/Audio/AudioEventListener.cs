using UnityEngine;
using ProjectZombie.Core.Events;

namespace ProjectZombie.Core.Audio
{
    /// <summary>
    /// Component lắng nghe các Domain Events từ GameEventBus và tự động phát Audio SFX tương ứng.
    /// Giúp Decouple hoàn toàn AudioManager khỏi gameplay code (Enemy, ExpGem, Weapons).
    /// </summary>
    public class AudioEventListener : MonoBehaviour
    {
        [Header("SFX Names (Khớp với AudioManager)")]
        [SerializeField] private string _enemyDieSFX = "zombie_hit";
        [SerializeField] private string _playerLevelUpSFX = "level_up";
        [SerializeField] private string _expCollectSFX = "exp_collect";

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
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(_enemyDieSFX))
            {
                AudioManager.Instance.PlaySFX(_enemyDieSFX);
            }
        }

        private void OnPlayerLevelUp(PlayerLevelUpEvent evt)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(_playerLevelUpSFX))
            {
                AudioManager.Instance.PlaySFX(_playerLevelUpSFX);
            }
        }

        private void OnExpCollected(ExpCollectedEvent evt)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(_expCollectSFX))
            {
                AudioManager.Instance.PlaySFX(_expCollectSFX);
            }
        }
    }
}

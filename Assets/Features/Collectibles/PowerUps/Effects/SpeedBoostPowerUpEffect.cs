using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Stats;
using ProjectZombie.Features.Shared.VFX;

namespace ProjectZombie.Features.Collectibles.PowerUps.Effects
{
    /// <summary>
    /// Hiệu ứng Thần Hành Phù: Tăng tốc độ di chuyển của người chơi thêm X% trong thời gian Y giây.
    /// Tích hợp trực tiếp với hệ thống StatModifier của PlayerStats.
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedBoostPowerUpEffect", menuName = "ProjectZombie/Collectibles/PowerUps/Effects/SpeedBoost")]
    public class SpeedBoostPowerUpEffect : ScriptableObject, IPowerUpEffect
    {
        [Header("Speed Boost Settings")]
        [Tooltip("Hệ số tăng tốc độ di chuyển (0.4 = +40% tốc độ chạy cơ bản)")]
        [Range(0.1f, 1.5f)]
        [SerializeField] private float _speedMultiplier = 0.4f;

        [Tooltip("Thời gian duy trì hiệu ứng (tính bằng giây)")]
        [Range(2f, 30f)]
        [SerializeField] private float _durationSeconds = 6f;

        [Header("Feedback")]
        [Tooltip("Prefab hiệu ứng VFX kích hoạt khi nhặt")]
        [SerializeField] private GameObject _boostVfxPrefab;

        public void Apply(GameObject player)
        {
            if (player == null) return;

            if (player.TryGetComponent<PlayerStats>(out var playerStats))
            {
                // Sử dụng Coroutine trên chính Player để tự động dọn dẹp khi Player chết/hết hiệp
                playerStats.StartCoroutine(ApplyTemporarySpeedRoutine(playerStats));

                if (_boostVfxPrefab != null && GlobalVFXPoolManager.Instance != null)
                {
                    GlobalVFXPoolManager.Instance.PlayEffectAttached(_boostVfxPrefab, player.transform, _durationSeconds);
                }
            }
        }

        private IEnumerator ApplyTemporarySpeedRoutine(PlayerStats playerStats)
        {
            var modifier = new StatModifier(_speedMultiplier, StatModType.PercentAdd, this);
            playerStats.AddModifier(PlayerStatType.MoveSpeed, modifier);

            yield return new WaitForSeconds(_durationSeconds);

            if (playerStats != null)
            {
                playerStats.RemoveModifier(PlayerStatType.MoveSpeed, modifier);
            }
        }
    }
}

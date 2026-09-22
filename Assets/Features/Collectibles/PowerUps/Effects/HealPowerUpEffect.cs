using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;

namespace ProjectZombie.Features.Collectibles.PowerUps.Effects
{
    /// <summary>
    /// Hiệu ứng Tiên Đan: Hồi phục máu tức thì cho người chơi dựa trên tỉ lệ % Max HP hoặc giá trị cố định.
    /// </summary>
    [CreateAssetMenu(fileName = "HealPowerUpEffect", menuName = "ProjectZombie/Collectibles/PowerUps/Effects/Heal")]
    public class HealPowerUpEffect : ScriptableObject, IPowerUpEffect
    {
        [Header("Healing Settings")]
        [Tooltip("Tỉ lệ % Max HP được hồi phục (0.35 = 35% máu tối đa)")]
        [Range(0.05f, 1.0f)]
        [SerializeField] private float _healPercent = 0.35f;

        [Tooltip("Lượng máu hồi tối thiểu")]
        [SerializeField] private float _minFlatHeal = 25f;

        [Header("Feedback")]
        [Tooltip("Prefab hiệu ứng VFX bùng nổ khi nhặt")]
        [SerializeField] private GameObject _healVfxPrefab;

        public void Apply(GameObject player)
        {
            if (player == null) return;

            if (player.TryGetComponent<HealthSystem>(out var healthSystem))
            {
                if (!healthSystem.IsAlive) return;

                float healAmount = Mathf.Max(healthSystem.MaxHealth * _healPercent, _minFlatHeal);
                healthSystem.Heal(healAmount);

                if (_healVfxPrefab != null && GlobalVFXPoolManager.Instance != null)
                {
                    GlobalVFXPoolManager.Instance.PlayEffect(_healVfxPrefab, player.transform.position, Quaternion.identity, 0.8f);
                }
            }
        }
    }
}

using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí đánh ra 2 phía cùng lúc (trái và phải) sử dụng OverlapBox (Tối ưu).
    /// </summary>
    public class Weapon_DualSlash : Weapon_MeleeBase
    {
        [Header("Omni Slash Hitbox")]
        [SerializeField] private Vector2 hitboxSize = new Vector2(3f, 2f);
        [SerializeField] private float forwardOffset = 2f;

        [Range(1, 12)]
        [Tooltip("Số lượng hướng chém. Tăng cấp vũ khí sẽ tăng số này lên.")]
        [SerializeField] public int slashCount = 2; // Public để hệ thống nâng cấp dễ dàng can thiệp

        private PlayerController _playerController;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            _playerController = GetComponentInParent<PlayerController>();
        }

        protected override bool CanAttack()
        {
            return true; 
        }

        protected override void PerformAttack()
        {
            Vector2 center = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            DamageData damageData = DamageUtility.CalculateDamage(CharacterStats.GetTotalDamage(), CharacterStats.CritChance);

            // Xác định góc gốc dựa trên hướng mặt của Player
            float baseAngle = 0f;
            if (_playerController != null && _playerController.transform.localScale.x < 0)
            {
                baseAngle = 180f;
            }

            float angleStep = 360f / Mathf.Max(1, slashCount);

            for (int i = 0; i < slashCount; i++)
            {
                float angle = baseAngle + (i * angleStep);
                float rad = angle * Mathf.Deg2Rad;
                
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 hitCenter = center + (direction * forwardOffset);

                // Gây sát thương bằng OverlapBox xoay theo đúng góc độ
                DealDamageInArea(hitCenter, hitboxSize, angle, damageData);
            }

            // Bật hiệu ứng chớp nhoáng (VFX sẽ phát ra đa hướng dựa vào Burst Count = slashCount)
            PlaySlashVFX();
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Vector2 center = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            
            float angleStep = 360f / Mathf.Max(1, slashCount);

            for (int i = 0; i < slashCount; i++)
            {
                float angle = i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 hitCenter = center + (direction * forwardOffset);

                // Unity Gizmos DrawCube không hỗ trợ xoay. Dùng DrawSphere để đánh dấu tâm nhát chém.
                Gizmos.DrawSphere(hitCenter, 0.3f);
                Gizmos.DrawWireCube(hitCenter, hitboxSize);
            }
        }
    }
}

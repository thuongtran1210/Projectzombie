using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Boomerang Cắt Chéo Cánh Cung (W012):
    /// Bắn phi tiêu theo từng Cặp Đối Xứng (Symmetric Pair), xòe rộng 2 bên và cắt chéo nhau ở phía trước
    /// rồi tự động bẻ lái quy hồi về vị trí Player, gây sát thương xuyên thấu toàn bộ quái vật.
    /// </summary>
    public class Weapon_Boomerang : Weapon_RangedBase
    {
        [Header("Curved Crescent Settings")]
        [SerializeField] private float baseSpreadAngle = 30f;

        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 9f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            Vector2 direction = _currentTarget != null 
                ? (Vector2)(_currentTarget.position - firePoint.position).normalized 
                : (Vector2)transform.right;

            DamageData damageData = CreateDamageData();
            
            // Số lượng cặp bắn ra (mặc định 1 cặp = 2 phi tiêu cắt chéo trái/phải)
            int extraProjectiles = GetFinalProjectileCount() - 1;
            int pairsCount = 1 + (extraProjectiles / 2);
            bool hasOddSingle = (extraProjectiles % 2 != 0);

            for (int p = 0; p < pairsCount; p++)
            {
                float spread = baseSpreadAngle + (p * 14f);

                // 1. Phi tiêu cánh Trái (xòe sang trái, uốn cong sang phải cắt chéo)
                Vector2 leftDir = Quaternion.Euler(0, 0, spread) * direction;
                SpawnCrescentDart(leftDir, damageData, 1f);

                // 2. Phi tiêu cánh Phải (xòe sang phải, uốn cong sang trái cắt chéo)
                Vector2 rightDir = Quaternion.Euler(0, 0, -spread) * direction;
                SpawnCrescentDart(rightDir, damageData, -1f);
            }

            // Nếu nâng cấp thêm 1 tia đạn lẻ, bắn thêm 1 phi tiêu trục giữa
            if (hasOddSingle)
            {
                SpawnCrescentDart(direction, damageData, 0f);
            }
        }

        private void SpawnCrescentDart(Vector2 spawnDir, DamageData damageData, float curveSign)
        {
            var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, spawnDir, gameObject, damageData);
            if (proj != null)
            {
                if (GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }

                var curved = proj.GetBehavior<Projectiles.Behaviors.CurvedBoomerangBehavior>();
                if (curved != null)
                {
                    curved.SetCurveSign(curveSign);
                }
            }
        }
    }
}

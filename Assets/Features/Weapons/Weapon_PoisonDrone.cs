using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Linh Phù Ma Da (W010): Triệu hồi đầm lầy khói độc âm ty AoE tại vị trí quái vật / dưới chân kẻ địch, gây sát thương độc liên tục.
    /// </summary>
    public class Weapon_PoisonDrone : Weapon_RangedBase
    {
        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 9f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return true; // Cho phép thả bùa độc dù không có mục tiêu gần
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            DamageData damageData = CreateDamageData();
            int count = GetFinalProjectileCount();

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos;
                if (_currentTarget != null)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * 1.5f;
                    spawnPos = _currentTarget.position + (Vector3)randomOffset;
                }
                else
                {
                    Vector2 randomOffset = Random.insideUnitCircle * 3.5f;
                    spawnPos = transform.position + (Vector3)randomOffset;
                }

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, spawnPos, Vector2.zero, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}


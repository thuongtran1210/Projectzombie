using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Holy Water (W011): Thả bãi nước thánh ngẫu nhiên trong bán kính xung quanh Player (Ground Zone).
    /// Đòi hỏi người chơi di chuyển chiến thuật (Kite) để lùa quái vật bước vào các vũng nước thánh.
    /// </summary>
    public class Weapon_HolyWater : Weapon_RangedBase
    {
        [Header("Holy Water Settings")]
        [Tooltip("Bán kính tối thiểu rơi bãi nước (tránh rơi đè lên tâm người chơi)")]
        [SerializeField] private float minDropDistance = 1.2f;

        [Tooltip("Bán kính tối đa rơi bãi nước xung quanh người chơi")]
        [SerializeField] private float baseDropRadius = 5.5f;

        protected override bool CanAttack()
        {
            return true; // Luôn luôn thả nước thánh định kỳ xung quanh người chơi
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            DamageData damageData = CreateDamageData();
            int count = GetFinalProjectileCount();
            float maxRadius = CharacterStats != null ? Mathf.Max(baseDropRadius, CharacterStats.AttackRange * 0.65f) : baseDropRadius;

            for (int i = 0; i < count; i++)
            {
                // Sinh ngẫu nhiên góc và khoảng cách xung quanh người chơi (Random Annular Distribution)
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float randomDist = Random.Range(minDropDistance, maxRadius);

                Vector2 offset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDist;
                Vector3 spawnPos = transform.position + (Vector3)offset;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, spawnPos, Vector2.zero, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}

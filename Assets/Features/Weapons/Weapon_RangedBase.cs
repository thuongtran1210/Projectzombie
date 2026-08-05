using UnityEngine;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Lớp cơ sở cho các vũ khí bắn xa theo hướng (Ranged Fired Weapons).
    /// Áp dụng các chỉ số riêng cho đạn bắn đi như tốc độ bay (Projectile Speed).
    /// </summary>
    public abstract class Weapon_RangedBase : Weapon_ProjectileBase
    {
        public override void ApplyStatModifier(Upgrades.WeaponStatModifier modifier)
        {
            base.ApplyStatModifier(modifier);

            // Vì projectileData đã được clone độc lập cho vũ khí này, 
            // tăng tốc độ bay của đạn bắn xa theo modifier.
            if (projectileData != null)
            {
                projectileData.Speed += modifier.projectileSpeedBonus;
            }
        }
    }
}

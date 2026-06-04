using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Giao diện chung cho các chỉ số của nhân vật (Player, Enemy).
    /// Giúp hệ thống vũ khí (WeaponBase) không bị bó buộc vào PlayerStats.
    /// </summary>
    public interface ICharacterStats
    {
        float AttackSpeed { get; }
        float CritChance { get; }
        float AttackRange { get; }
        float GetTotalDamage();
    }
}

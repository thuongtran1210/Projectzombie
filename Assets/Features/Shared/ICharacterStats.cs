using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Giao diện chung cho các chỉ số của nhân vật (Player, Enemy, Boss).
    /// Hợp nhất đầy đủ các chỉ số tấn công, phòng thủ, tốc độ và nguyên tố.
    /// Giúp hệ thống vũ khí (WeaponBase) và chiến đấu không bị bó buộc vào PlayerStats.
    /// </summary>
    public interface ICharacterStats
    {
        float MaxHealth { get; }
        float BaseDamage { get; }
        /// <summary>Damage relative to this character's starting base damage, including progression and temporary buffs.</summary>
        float DamageScale { get; }
        float AttackSpeed { get; }
        float CritChance { get; }
        float CritDamageMultiplier { get; }
        float AttackRange { get; }
        float MoveSpeed { get; }
        float AreaScale { get; }
        float Armor { get; }
        ElementType CurrentElement { get; }
        float GetTotalDamage();
    }
}


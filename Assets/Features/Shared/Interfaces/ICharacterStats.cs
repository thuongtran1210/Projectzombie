using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Shared.Interfaces
{
    /// <summary>
    /// Contract chung để đọc các chỉ số cơ bản của nhân vật (Player, Pet, Enemy, Boss).
    /// </summary>
    public interface ICharacterStats
    {
        float MaxHealth { get; }
        float MoveSpeed { get; }
        float BaseDamage { get; }
        float AttackSpeed { get; }
        float CritChance { get; }
        float CritDamageMultiplier { get; }
        float Armor { get; }
        ElementType CurrentElement { get; }
    }
}

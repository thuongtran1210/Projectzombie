using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Shared.Interfaces
{
    /// <summary>
    /// Contract chung cho mọi thực thể có khả năng gây sát thương (Player, Vũ khí, Đạn, Kẻ địch).
    /// </summary>
    public interface IDamageDealer
    {
        /// <summary>
        /// Sát thương cơ bản.
        /// </summary>
        float BaseDamage { get; }

        /// <summary>
        /// Tỉ lệ chí mạng (0.0 -> 1.0).
        /// </summary>
        float CritChance { get; }

        /// <summary>
        /// Hệ số nhân sát thương khi chí mạng.
        /// </summary>
        float CritMultiplier { get; }

        /// <summary>
        /// Hệ nguyên tố của nguồn sát thương.
        /// </summary>
        ElementType AttackElement { get; }
    }
}

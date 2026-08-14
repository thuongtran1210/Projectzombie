using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Shared.Interfaces
{
    /// <summary>
    /// Contract chung cho mọi thực thể có thể nhận sát thương (Player, Enemy, Boss, Destructible Object).
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Vị trí của thực thể trong không gian World.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Hệ nguyên tố của thực thể (Kim, Mộc, Thủy, Hỏa, Thổ, None).
        /// </summary>
        ElementType CurrentElement { get; }

        /// <summary>
        /// Thực thể còn sống hay đã tử vong.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Nhận sát thương được tính toán trước.
        /// </summary>
        /// <param name="damageData">Dữ liệu sát thương chứa giá trị, chí mạng, nguyên tố</param>
        void TakeDamage(DamageData damageData);
    }
}

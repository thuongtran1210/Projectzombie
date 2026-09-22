using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Interface đại diện cho bất kỳ đối tượng nào có thể nhận sát thương (Player, Enemy, Breakable Obstacles, Boss).
    /// Tuân thủ Dependency Inversion Principle (DIP) và Rule 3.4 trong AGENTS.md.
    /// Hợp nhất đầy đủ các thuộc tính kiểm tra vị trí, hệ nguyên tố và trạng thái sinh tồn.
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

        void TakeDamage(float amount);
        void TakeDamage(DamageData damageData);
        void TakeDamage(DamageContext context);
    }
}


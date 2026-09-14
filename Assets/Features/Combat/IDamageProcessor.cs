using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Combat
{
    /// <summary>
    /// Hợp đồng xử lý sát thương trừ máu (Decoupled Damage Processing).
    /// Cho phép chuyển đổi liền mạch giữa Xử lý cục bộ (Local Single-Player) và Xử lý Thẩm quyền Máy chủ (Photon Host Authoritative).
    /// </summary>
    public interface IDamageProcessor
    {
        /// <summary>
        /// Xử lý áp dụng sát thương lên mục tiêu HealthSystem.
        /// </summary>
        /// <param name="target">Mục tiêu nhận sát thương</param>
        /// <param name="damageAmount">Lượng sát thương thuần</param>
        /// <param name="instigator">Người/thực thể gây sát thương</param>
        void ApplyDamage(HealthSystem target, float damageAmount, object instigator = null);
    }
}

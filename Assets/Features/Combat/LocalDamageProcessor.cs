using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Combat
{
    /// <summary>
    /// Xử lý sát thương trực tiếp trên máy cục bộ cho chế độ Vượt Ải Đơn (Solo) và Offline.
    /// Triệt tiêu hoàn toàn overhead mạng, xử lý tức thì với 0 GC Allocations.
    /// </summary>
    public class LocalDamageProcessor : IDamageProcessor
    {
        public static readonly LocalDamageProcessor SharedInstance = new LocalDamageProcessor();

        public void ApplyDamage(HealthSystem target, float damageAmount, object instigator = null)
        {
            if (target == null || damageAmount <= 0f) return;
            target.TakeDamage(damageAmount);
        }
    }
}

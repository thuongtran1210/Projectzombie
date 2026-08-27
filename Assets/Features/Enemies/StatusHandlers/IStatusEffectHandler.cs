using System;

namespace ProjectZombie.Features.Enemies.StatusHandlers
{
    /// <summary>
    /// Interface định nghĩa hợp đồng Strategy Pattern cho từng loại Hiệu Ứng Bất Lợi (Status Effect).
    /// Tuân thủ nguyên tắc Open-Closed (OCP), cho phép bổ sung thêm hiệu ứng mới mà không cần chỉnh sửa controller lõi.
    /// </summary>
    public interface IStatusEffectHandler
    {
        StatusEffectType Type { get; }
        void OnApplied(Enemy enemy, ActiveStatusEffect effectData);
        void OnTick(Enemy enemy, ActiveStatusEffect effectData, float deltaTime);
        void OnExpired(Enemy enemy, ActiveStatusEffect effectData);
        void OnRemoved(Enemy enemy, ActiveStatusEffect effectData);
    }
}

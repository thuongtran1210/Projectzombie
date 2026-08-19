using System;
using UnityEngine;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Giao diện chuẩn cho mọi thực thể có thể chịu các hiệu ứng trạng thái bất lợi (Slow, Stun, Freeze, Burn, Knockback).
    /// Tuân thủ nguyên tắc Dependency Inversion (DIP) và Interface Segregation (ISP) - Mục 3.2 & 3.4 AGENTS.md.
    /// </summary>
    public interface IStatusReceiver
    {
        /// <summary>
        /// Chỉ số kháng khống chế (0.0 = 0% không kháng, 0.3 = 30% Elite, 0.7 = 70% Boss).
        /// Giảm trực tiếp thời lượng của các hiệu ứng Stun, Slow, Freeze.
        /// </summary>
        float Tenacity { get; }

        /// <summary>
        /// Kiểm tra xem thực thể hiện tại có thể di chuyển hay đang bị khóa (Stun, Freeze, Knockback).
        /// </summary>
        bool CanMove { get; }

        /// <summary>
        /// Áp dụng hiệu ứng trạng thái bất lợi lên thực thể.
        /// </summary>
        /// <param name="type">Loại hiệu ứng (Slow, Stun, Freeze, Burn)</param>
        /// <param name="duration">Thời lượng cơ bản (giây)</param>
        /// <param name="value">Giá trị bổ trợ (tỉ lệ làm chậm ví dụ 0.3f = 30%, hoặc sát thương DoT mỗi tick)</param>
        /// <param name="tickInterval">Khoảng thời gian giữa các lần kích hoạt DoT</param>
        /// <param name="onTickDamage">Callback gây sát thương mỗi tick (nếu có)</param>
        void ApplyStatusEffect(StatusEffectType type, float duration, float value = 0f, float tickInterval = 0.5f, Action<float> onTickDamage = null);

        /// <summary>
        /// Gây hiệu ứng Đẩy lùi (Knockback) có suy giảm theo thời gian.
        /// </summary>
        void ApplyKnockback(Vector2 direction, float force, float duration);

        /// <summary>
        /// Kiểm tra xem thực thể có đang chịu một loại hiệu ứng cụ thể hay không.
        /// </summary>
        bool HasStatus(StatusEffectType type);

        /// <summary>
        /// Xóa bỏ một loại hiệu ứng trạng thái đang áp dụng.
        /// </summary>
        void RemoveStatus(StatusEffectType type);
    }
}

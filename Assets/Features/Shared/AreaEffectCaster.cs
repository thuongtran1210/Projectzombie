using System;
using UnityEngine;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Tiện ích hỗ trợ quét vùng (AoE) và áp dụng hiệu ứng trạng thái bất lợi / đẩy lùi tập trung.
    /// Đảm bảo Zero GC Allocation (sử dụng _hitBuffer tĩnh) và Decoupling qua Interface IStatusReceiver.
    /// Tuân thủ Mục 6 & Mục 3.4 AGENTS.md.
    /// </summary>
    public static class AreaEffectCaster
    {
        private static readonly Collider2D[] _hitBuffer = new Collider2D[60];

        /// <summary>
        /// Quét vùng hình tròn và áp dụng hiệu ứng trạng thái bất lợi (Slow, Stun, Freeze, Burn) và Đẩy lùi lên các thực thể trúng chiêu.
        /// </summary>
        /// <param name="center">Tâm quét vùng</param>
        /// <param name="radius">Bán kính quét</param>
        /// <param name="layerMask">Mặt nạ Layer mục tiêu</param>
        /// <param name="statusType">Loại hiệu ứng trạng thái</param>
        /// <param name="duration">Thời lượng hiệu ứng (giây)</param>
        /// <param name="statusValue">Giá trị phụ (tỉ lệ slow hoặc sát thương DoT)</param>
        /// <param name="knockbackForce">Lực đẩy lùi (0 nếu không đẩy lùi)</param>
        /// <param name="knockbackDuration">Thời lượng đẩy lùi</param>
        /// <param name="onTargetHit">Callback tùy chọn khi một mục tiêu trúng chiêu</param>
        /// <returns>Số lượng mục tiêu hợp lệ trúng chiêu</returns>
        public static int CastStatusAoE(
            Vector2 center,
            float radius,
            LayerMask layerMask,
            StatusEffectType statusType,
            float duration,
            float statusValue = 0f,
            float knockbackForce = 0f,
            float knockbackDuration = 0.25f,
            Action<IStatusReceiver, Collider2D> onTargetHit = null)
        {
            int filterMask = layerMask.value != 0 ? layerMask.value : LayerMask.GetMask("Enemy");
            if (filterMask == 0) filterMask = ~0;

            int hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, _hitBuffer, filterMask);
            int affectedCount = 0;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null) continue;

                // Tìm IStatusReceiver trên collider hoặc parent
                if (col.TryGetComponent<IStatusReceiver>(out var receiver) ||
                    (receiver = col.GetComponentInParent<IStatusReceiver>()) != null)
                {
                    // 1. Áp dụng hiệu ứng trạng thái
                    if (duration > 0f)
                    {
                        receiver.ApplyStatusEffect(statusType, duration, statusValue);
                    }

                    // 2. Áp dụng đẩy lùi nếu có lực
                    if (knockbackForce > 0f)
                    {
                        Vector2 pushDir = ((Vector2)col.transform.position - center).normalized;
                        if (pushDir == Vector2.zero) pushDir = Vector2.up;
                        receiver.ApplyKnockback(pushDir, knockbackForce, knockbackDuration);
                    }

                    onTargetHit?.Invoke(receiver, col);
                    affectedCount++;
                }
            }

            return affectedCount;
        }

        /// <summary>
        /// Quét vùng và chỉ áp dụng lực Đẩy lùi (Knockback AoE).
        /// </summary>
        public static int CastKnockbackAoE(Vector2 center, float radius, LayerMask layerMask, float force, float duration = 0.25f)
        {
            return CastStatusAoE(center, radius, layerMask, StatusEffectType.Slow, 0f, 0f, force, duration);
        }
    }
}

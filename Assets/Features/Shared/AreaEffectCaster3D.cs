using System;
using UnityEngine;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Tiện ích hỗ trợ quét vùng (AoE 3D) và áp dụng hiệu ứng trạng thái bất lợi / đẩy lùi tập trung trong không gian 3D.
    /// Đảm bảo Zero GC Allocation (sử dụng _hitBuffer tĩnh) và Decoupling qua Interface IStatusReceiver.
    /// Tuân thủ quy tắc 6.1 & 3.4 AGENTS.md.
    /// </summary>
    public static class AreaEffectCaster3D
    {
        private static readonly Collider[] _hitBuffer = new Collider[80];

        /// <summary>
        /// Quét vùng hình cầu 3D và áp dụng hiệu ứng trạng thái bất lợi (Slow, Stun, Freeze, Burn) và Đẩy lùi theo phương ngang X-Z.
        /// </summary>
        public static int CastStatusAoE(
            Vector3 center,
            float radius,
            LayerMask layerMask,
            StatusEffectType statusType,
            float duration,
            float statusValue = 0f,
            float knockbackForce = 0f,
            float knockbackDuration = 0.25f,
            Action<IStatusReceiver, Collider> onTargetHit = null)
        {
            int filterMask = layerMask.value != 0 ? layerMask.value : LayerMask.GetMask("Enemy");
            if (filterMask == 0) filterMask = ~0;

            int hitCount = Physics.OverlapSphereNonAlloc(center, radius, _hitBuffer, filterMask);
            int affectedCount = 0;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _hitBuffer[i];
                if (col == null) continue;

                if (col.TryGetComponent<IStatusReceiver>(out var receiver) ||
                    (receiver = col.GetComponentInParent<IStatusReceiver>()) != null)
                {
                    // 1. Áp dụng hiệu ứng trạng thái
                    if (duration > 0f)
                    {
                        receiver.ApplyStatusEffect(statusType, duration, statusValue);
                    }

                    // 2. Áp dụng lực đẩy lùi theo phương ngang X-Z
                    if (knockbackForce > 0f)
                    {
                        Vector3 targetPos = col.transform.position;
                        Vector3 knockbackDir = targetPos - center;
                        knockbackDir.y = 0f; // Luôn đẩy lùi ngang, không hất văng lên trời

                        if (knockbackDir.sqrMagnitude < 0.001f)
                        {
                            knockbackDir = Vector3.forward;
                        }
                        else
                        {
                            knockbackDir.Normalize();
                        }

                        // Nếu target có Rigidbody 3D
                        if (col.TryGetComponent<Rigidbody>(out var rb))
                        {
                            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                        }
                        else
                        {
                            receiver.ApplyKnockback(knockbackDir, knockbackForce, knockbackDuration);
                        }
                    }

                    onTargetHit?.Invoke(receiver, col);
                    affectedCount++;
                }
            }

            return affectedCount;
        }
    }
}

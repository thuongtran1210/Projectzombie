using System;
using UnityEngine;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>
    /// Cổng phát sự kiện sát thương toàn cục (Decoupled Event-Driven Bus).
    /// Cho phép mọi hệ thống Gameplay (Tướng, Trụ, Quái, Kỹ năng) phát số sát thương
    /// mà không cần phụ thuộc hay tiêm instance của ICombatTextService.
    /// </summary>
    public static class CombatDamageEvents
    {
        public static event Action<CombatTextRequest> OnDamageDealt;

        /// <summary>
        /// Phát sự kiện yêu cầu hiển thị chữ sát thương qua struct CombatTextRequest.
        /// </summary>
        public static void RaiseDamageDealt(in CombatTextRequest request)
        {
            OnDamageDealt?.Invoke(request);
        }

        /// <summary>
        /// Tiện ích phát sự kiện sát thương nhanh gọn với các tham số cơ bản.
        /// </summary>
        public static void RaiseDamageDealt(
            float amount,
            Vector3 position,
            bool isCritical = false,
            CombatTextTarget target = CombatTextTarget.Enemy,
            Color? colorOverride = null)
        {
            OnDamageDealt?.Invoke(new CombatTextRequest(amount, position, target, isCritical, color: colorOverride));
        }
    }
}

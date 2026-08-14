using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Báo cáo chi tiết thông tin sát thương dùng cho hệ thống Floating Damage Text và Stats Tracker.
    /// </summary>
    public struct DamageReport
    {
        public float Amount;
        public bool IsCritical;
        public ElementType Element;
        public Vector3 Position;
        public bool IsPlayerTarget;
        public bool IsCounter;

        public DamageReport(float amount, bool isCritical, ElementType element, Vector3 position, bool isPlayerTarget = false, bool isCounter = false)
        {
            Amount = amount;
            IsCritical = isCritical;
            Element = element;
            Position = position;
            IsPlayerTarget = isPlayerTarget;
            IsCounter = isCounter;
        }
    }
}


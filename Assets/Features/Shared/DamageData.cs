using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Struct chứa dữ liệu sát thương truyền qua lại giữa các hệ thống (Weapon -> Projectile -> HealthSystem).
    /// </summary>
    public struct DamageData
    {
        public float Amount;
        public bool IsCritical;
        public ElementType Element;
        
        public DamageData(float amount, bool isCritical = false, ElementType element = ElementType.None)
        {
            Amount = amount;
            IsCritical = isCritical;
            Element = element;
        }
    }
}

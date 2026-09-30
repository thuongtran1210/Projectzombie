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
        public bool IsCounter;
        public Object SourceWeapon; // Reference đến WeaponBase nếu có
        public bool CanTriggerReaction;
        
        public DamageData(float amount, bool isCritical = false, ElementType element = ElementType.None, bool isCounter = false, Object sourceWeapon = null, bool canTriggerReaction = true)
        {
            Amount = amount;
            IsCritical = isCritical;
            Element = element;
            IsCounter = isCounter;
            SourceWeapon = sourceWeapon;
            CanTriggerReaction = canTriggerReaction;
        }
    }
}


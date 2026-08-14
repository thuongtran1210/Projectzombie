using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    public struct DamageContext
    {
        public GameObject Source;
        public float BaseDamage;
        public ElementType Element;
        public bool IsCritical;
        public Object SourceWeapon;
        
        public DamageContext(GameObject source, float baseDamage, ElementType element = ElementType.None, bool isCritical = false, Object sourceWeapon = null)
        {
            Source = source;
            BaseDamage = baseDamage;
            Element = element;
            IsCritical = isCritical;
            SourceWeapon = sourceWeapon;
        }
    }
}


using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    public struct DamageContext
    {
        public GameObject Source;
        public float BaseDamage;
        // In the future, you could add:
        // public DamageType Element;
        // public bool IsCritical;
        
        public DamageContext(GameObject source, float baseDamage)
        {
            Source = source;
            BaseDamage = baseDamage;
        }
    }
}

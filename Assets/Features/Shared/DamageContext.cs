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
        public ulong AttackId;
        public ElementHitSource HitSource;
        
        public DamageContext(GameObject source, float baseDamage, ElementType element = ElementType.None, bool isCritical = false, Object sourceWeapon = null)
        {
            Source = source;
            BaseDamage = baseDamage;
            Element = element;
            IsCritical = isCritical;
            SourceWeapon = sourceWeapon;
            AttackId = ElementSynergyRules.NextAttackId();
            HitSource = sourceWeapon is Weapons.WeaponBase weapon && weapon != null
                ? (weapon.isPrimaryActiveWeapon ? ElementHitSource.HeroBasicAttack : ElementHitSource.Relic)
                : ElementHitSource.Unknown;
        }

        /// <summary>Returns raw damage for one target without losing attack provenance.</summary>
        public DamageData ToDamageData()
        {
            return new DamageData(BaseDamage, IsCritical, Element, sourceWeapon: SourceWeapon)
            {
                Owner = Source,
                AttackId = AttackId,
                HitSource = HitSource
            };
        }
    }
}


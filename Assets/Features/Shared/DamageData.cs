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
        public bool ElementMultiplierApplied;
        public ElementAttackRecord AttackRecord;
        public GameObject Owner;
        
        public DamageData(float amount, bool isCritical = false, ElementType element = ElementType.None, bool isCounter = false, Object sourceWeapon = null, bool canTriggerReaction = true)
        {
            Amount = amount;
            IsCritical = isCritical;
            Element = element;
            IsCounter = isCounter;
            SourceWeapon = sourceWeapon;
            CanTriggerReaction = canTriggerReaction;
            ElementMultiplierApplied = isCounter;
            AttackRecord = null;
            Owner = null;
        }
    }

    // Shared by all targets/projectiles of one basic attack, including pooled projectiles.
    public sealed class ElementAttackRecord
    {
        private static readonly UnityEngine.Pool.ObjectPool<ElementAttackRecord> POOL =
            new UnityEngine.Pool.ObjectPool<ElementAttackRecord>(() => new ElementAttackRecord(), defaultCapacity: 32);
        private GameObject _owner;
        private bool _registered;
        private int _references;

        private ElementAttackRecord() { }

        public static ElementAttackRecord Acquire(GameObject owner)
        {
            var record = POOL.Get();
            record._owner = owner;
            record._registered = false;
            record._references = 1;
            return record;
        }

        public void Retain() => _references++;

        public void Release()
        {
            if (--_references != 0) return;
            _owner = null;
            POOL.Release(this);
        }

        public void RegisterSuccessfulHit(ElementType element)
        {
            if (_registered || _owner == null || element == ElementType.None) return;
            _registered = true;
            Elements.ElementCycleManager.Instance?.RegisterHit(element, null, _owner);
        }
    }
}


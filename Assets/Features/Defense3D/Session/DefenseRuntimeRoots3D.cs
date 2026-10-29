using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Các root sở hữu object runtime; mọi reference được gán sẵn trong scene.</summary>
    public sealed class DefenseRuntimeRoots3D : MonoBehaviour
    {
        [SerializeField] private Transform _actors;
        [SerializeField] private Transform _staging;
        [SerializeField] private Transform _pickups;
        [SerializeField] private Transform _pools;
        [SerializeField] private Transform _enemyActors;
        [SerializeField] private Transform _allies;
        [SerializeField] private Transform _effects;
        [SerializeField] private Transform _worldEffects;
        [SerializeField] private Transform _attachedEffects;
        [SerializeField] private Transform _combatText;
        [SerializeField] private Transform _enemyPools;
        [SerializeField] private Transform _pickupPools;
        [SerializeField] private Transform _effectPools;
        [SerializeField] private DefenseWaterNavigation3D _waterNavigation;
        [SerializeField] private DefenseAirNavigation3D _airNavigation;
        [SerializeField] private DefenseCannonBattery3D _cannonBattery;
        [SerializeField] private DefenseFriendlyTargets3D _friendlyTargets;
        public DefenseFriendlyTargets3D FriendlyTargets => _friendlyTargets;
        public DefenseCannonBattery3D CannonBattery => _cannonBattery;
        public DefenseAirNavigation3D AirNavigation => _airNavigation;
        public DefenseWaterNavigation3D WaterNavigation => _waterNavigation;
        public Transform Actors => _actors;
        public Transform Staging => _staging;
        public Transform Pickups => _pickups;
        public Transform Pools => _pools;
        public Transform EnemyActors => _enemyActors != null ? _enemyActors : _actors;
        public Transform EnemyPools => _enemyPools != null ? _enemyPools : _pools;
        public Transform PickupPools => _pickupPools != null ? _pickupPools : _pools;
        public Transform EffectPools => _effectPools;
        public Transform WorldEffects => _worldEffects;
        public Transform AttachedEffects => _attachedEffects;
        public Transform CombatText => _combatText;

        public void ValidatePresentationBindings()
        {
            RequireChild(_enemyActors, _actors, true);
            RequireChild(_allies, _actors, true);
            RequireChild(_effects, transform, true);
            RequireChild(_worldEffects, _effects, true);
            RequireChild(_attachedEffects, _effects, true);
            RequireChild(_combatText, transform, true);
            RequireChild(_enemyPools, _pools, false);
            RequireChild(_pickupPools, _pools, false);
            RequireChild(_effectPools, _pools, false);
            var roots = new[] { _enemyActors, _allies, _effects, _worldEffects, _attachedEffects,
                _combatText, _enemyPools, _pickupPools, _effectPools };
            for (int i = 0; i < roots.Length; i++)
                for (int j = 0; j < i; j++)
                    if (roots[i] == roots[j]) throw new InvalidOperationException("Runtime root references must be distinct.");
        }
        private void RequireChild(Transform child, Transform parent, bool active)
        {
            if (child == null || child.gameObject.scene != gameObject.scene || child.parent != parent ||
                child.localPosition != Vector3.zero || child.localRotation != Quaternion.identity || child.localScale != Vector3.one ||
                child.gameObject.activeInHierarchy != active || !child.gameObject.activeSelf)
                throw new InvalidOperationException("Assign identity runtime roots with the expected parent and activation state.");
        }

        public void ValidateBindings()
        {
            if (transform.position != Vector3.zero || transform.rotation != Quaternion.identity || transform.lossyScale != Vector3.one)
                throw new InvalidOperationException("Runtime Roots must have identity world transform.");
            if (transform.parent != null && (transform.parent.position != Vector3.zero || transform.parent.rotation != Quaternion.identity || transform.parent.lossyScale != Vector3.one))
                throw new InvalidOperationException("Runtime Roots parent container must also have identity world transform.");
            if (_waterNavigation == null || _waterNavigation.gameObject.scene != gameObject.scene)
                throw new InvalidOperationException("Assign the scene-authored water navigation.");
            _waterNavigation.ValidateBindings();
            if (_airNavigation == null || _airNavigation.gameObject.scene != gameObject.scene)
                throw new InvalidOperationException("Assign the scene-authored Air navigation.");
            _airNavigation.ValidateBindings();
            if (_cannonBattery == null || _cannonBattery.gameObject.scene != gameObject.scene)
                throw new InvalidOperationException("Assign the scene-authored cannon battery.");
            _cannonBattery.ValidateBindings();
            if (_friendlyTargets == null || _friendlyTargets.gameObject.scene != gameObject.scene)
                throw new InvalidOperationException("Assign the scene-authored friendly target roster.");
            _friendlyTargets.ValidateBindings();
            Transform[] roots = { _actors,_staging,_pickups,_pools };
            for (int i=0;i<roots.Length;i++)
            {
                if (roots[i] == null || roots[i].gameObject.scene != gameObject.scene || roots[i].parent != transform ||
                    roots[i].localPosition != Vector3.zero || roots[i].localRotation != Quaternion.identity || roots[i].localScale != Vector3.one)
                    throw new InvalidOperationException("Assign distinct identity Actors/Staging/Pickups/Pools children in the runtime root's scene.");
                for (int j=0;j<i;j++) if (roots[i] == roots[j]) throw new InvalidOperationException("Runtime roots cannot share a Transform.");
            }
            if (!gameObject.activeInHierarchy || !_actors.gameObject.activeInHierarchy || !_pickups.gameObject.activeInHierarchy ||
                _staging.gameObject.activeSelf || _pools.gameObject.activeSelf)
                throw new InvalidOperationException("Actors/Pickups must be active; Staging/Pools must be authored inactive.");
        }
    }
}

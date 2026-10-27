using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scene-authored ownership containers; staging is inactive, pools are reserved for the pooling phase.</summary>
    public sealed class DefenseRuntimeRoots3D : MonoBehaviour
    {
        [SerializeField] private Transform _actors;
        [SerializeField] private Transform _staging;
        [SerializeField] private Transform _pickups;
        [SerializeField] private Transform _pools;
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

        public void ValidateBindings()
        {
            if (transform.parent != null || transform.position != Vector3.zero || transform.rotation != Quaternion.identity || transform.lossyScale != Vector3.one)
                throw new InvalidOperationException("Runtime Roots must be an independent identity scene root.");
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

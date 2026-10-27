using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authored canal bounds and paired water/shore anchors. Never samples water onto NavMesh.</summary>
    public sealed class DefenseWaterNavigation3D : MonoBehaviour
    {
        [Serializable]
        public struct Exit
        {
            public Transform Water;
            public Transform Shore;
        }
        [SerializeField] private Collider _waterBounds;
        [SerializeField] private Exit[] _exits;
        [SerializeField, Min(0.01f)] private float _shoreSampleRadius = 0.3f;
        [SerializeField] private LayerMask _groundMask = 4096;
        private NavMeshPath _path;

        // NavMeshPath creates native state and must not run during Unity serialization.
        private void Awake() => _path = new NavMeshPath();

        public void ValidateBindings()
        {
            if (_waterBounds == null || _waterBounds.gameObject.scene != gameObject.scene || _exits == null || _exits.Length == 0)
                throw new InvalidOperationException("Assign a same-scene water bounds collider and paired exits.");
            for (int i = 0; i < _exits.Length; i++)
            {
                Exit exit = _exits[i];
                if (exit.Water == null || exit.Shore == null || exit.Water.gameObject.scene != gameObject.scene ||
                    exit.Shore.gameObject.scene != gameObject.scene || !Contains(exit.Water.position) || Contains(exit.Shore.position))
                    throw new InvalidOperationException("Water exit must be inside canal bounds and its shore must be outside.");
            }
        }

        public bool Contains(Vector3 position)
        {
            if (_waterBounds == null) return false;
            Bounds bounds = _waterBounds.bounds;
            return position.x >= bounds.min.x && position.x <= bounds.max.x &&
                position.z >= bounds.min.z && position.z <= bounds.max.z;
        }

        public bool TryGetExit(Vector3 origin, out Vector3 water, out Vector3 shore)
        {
            water = shore = default;
            if (!Contains(origin) || _exits == null) return false;
            float best = float.PositiveInfinity;
            for (int i = 0; i < _exits.Length; i++)
            {
                Exit exit = _exits[i];
                if (exit.Water == null || exit.Shore == null || !Contains(exit.Water.position)) continue;
                float distance = (exit.Water.position - origin).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                water = exit.Water.position;
                shore = exit.Shore.position;
            }
            return !float.IsPositiveInfinity(best);
        }

        public bool TryResolveShore(Vector3 shore, Vector3 destination, out Vector3 landing)
        {
            landing = default;
            if (_path == null) return false;
            if (Contains(shore) || !Physics.Raycast(shore + Vector3.up * 2f, Vector3.down,
                out var ground, 4f, _groundMask, QueryTriggerInteraction.Ignore)) return false;
            if (!NavMesh.SamplePosition(ground.point, out var hit, _shoreSampleRadius, NavMesh.AllAreas) || Contains(hit.position)) return false;
            if (!NavMesh.SamplePosition(destination, out var target, 2f, NavMesh.AllAreas)) return false;
            if (!NavMesh.CalculatePath(hit.position, target.position, NavMesh.AllAreas, _path) ||
                _path.status != NavMeshPathStatus.PathComplete) return false;
            landing = hit.position;
            return true;
        }
    }
}

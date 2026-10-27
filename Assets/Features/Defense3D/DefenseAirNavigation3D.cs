using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scene-authored flight volume and conservative swept-sphere obstacle checks, independent of NavMesh.</summary>
    public sealed class DefenseAirNavigation3D : MonoBehaviour
    {
        [SerializeField] private BoxCollider _volume;
        [SerializeField] private DefenseAirProfileSO _profile;
        public DefenseAirProfileSO Profile => _profile;

        public void ValidateBindings()
        {
            if (_volume == null || _volume.gameObject.scene != gameObject.scene || !_volume.isTrigger || _profile == null)
                throw new InvalidOperationException("Assign an authored flight trigger volume and Air profile.");
            _profile.Validate();
        }

        public bool TryResolveSpawn(Vector3 requested, out Vector3 position)
        {
            position = default;
            if (_profile == null) return false;
            position = new Vector3(requested.x, _profile.CruiseHeight, requested.z);
            return CanOccupy(position);
        }

        public bool CanOccupy(Vector3 position)
        {
            if (_volume == null || _profile == null || !new DefensePoint3(position.x, position.y, position.z).IsFinite ||
                !DefenseSpatialRules.WithinHeight(position.y, _profile.MinimumHeight, _profile.MaximumHeight)) return false;
            Bounds bounds = _volume.bounds;
            bounds.Expand(-2f * _profile.ClearanceRadius);
            return bounds.Contains(position) && !Physics.CheckSphere(position, _profile.ClearanceRadius,
                _profile.ObstacleMask, QueryTriggerInteraction.Ignore);
        }

        public bool CanMove(Vector3 from, Vector3 to)
        {
            if (!CanOccupy(from) || !CanOccupy(to)) return false;
            Vector3 delta = to - from;
            float distance = delta.magnitude;
            return distance <= Mathf.Epsilon || !Physics.SphereCast(from, _profile.ClearanceRadius,
                delta / distance, out _, distance, _profile.ObstacleMask, QueryTriggerInteraction.Ignore);
        }
    }
}

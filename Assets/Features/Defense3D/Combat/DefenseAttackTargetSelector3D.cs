using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Allocation-free filtered target selection; blocked candidates cannot starve a reachable target.</summary>
    public sealed class DefenseAttackTargetSelector3D
    {
        private readonly Transform _source;
        private readonly DefenseAttackTargetingProfileSO _profile;
        private readonly Predicate<IDefenseTargetPosition> _accepts;
        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private Vector3 _origin;

        public DefenseAttackTargetSelector3D(Transform source, DefenseAttackTargetingProfileSO profile)
        {
            _source = source;
            _profile = profile;
            _accepts = Accepts;
        }

        public IDefenseEnemyTarget3D Find(IDefenseTargetQuery query, float range)
        {
            if (_source == null || _profile == null || !(query is IDefenseSpatialTargetQuery spatial)) return null;
            _origin = _source.position + _profile.OriginOffset;
            return spatial.FindNearestSpatial(new DefensePoint3(_origin.x, _origin.y, _origin.z), range,
                _profile.Targets, _accepts)?.Identity as IDefenseEnemyTarget3D;
        }

        public bool CanHit(IDefenseTargetPosition candidate, Vector3 origin)
        {
            if (_profile == null || _source == null || candidate == null) return false;
            _origin = origin + _profile.OriginOffset;
            return Accepts(candidate);
        }

        private bool Accepts(IDefenseTargetPosition candidate)
        {
            if (!(candidate.Identity is IDefenseEnemyTarget3D target) || !target.IsTargetable || target.TargetTransform == null) return false;
            Vector3 destination = target.TargetTransform.position;
            Vector3 delta = destination - _origin;
            float distance = delta.magnitude;
            if (distance <= Mathf.Epsilon) return true;
            int count = Physics.RaycastNonAlloc(_origin, delta / distance, _hits, distance,
                _profile.LineOfSightMask, QueryTriggerInteraction.Ignore);
            if (count == _hits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Transform hit = _hits[i].transform;
                if (hit != null && !hit.IsChildOf(_source) && !hit.IsChildOf(target.TargetTransform)) return false;
            }
            return true;
        }
    }
}

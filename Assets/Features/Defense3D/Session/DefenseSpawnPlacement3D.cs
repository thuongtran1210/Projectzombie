using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Resolves authored zones and validates NavMesh and Hero exclusion geometry.</summary>
    public sealed class DefenseSpawnPlacement3D
    {
        private readonly DefenseStageSpawnPlan _plan;
        private readonly Transform _eastMarker;
        private readonly Transform _hero;
        private readonly Transform _baseMarker;
        private readonly DefenseWaterNavigation3D _waterNavigation;
        private readonly DefenseAirNavigation3D _airNavigation;
        private readonly NavMeshPath _path = new NavMeshPath();

        public DefenseSpawnPlacement3D(DefenseStageSpawnPlan plan, Transform eastMarker, Transform hero,Transform baseMarker=null, DefenseWaterNavigation3D waterNavigation=null, DefenseAirNavigation3D airNavigation=null)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
            _eastMarker = eastMarker != null ? eastMarker : throw new ArgumentNullException(nameof(eastMarker));
            _hero = hero != null ? hero : throw new ArgumentNullException(nameof(hero));
            _baseMarker=baseMarker;
            _waterNavigation=waterNavigation;
            _airNavigation=airNavigation;
        }

        public bool TryResolveLanding(Vector3 ground, out DefenseSpawnLocation location)
        {
            location = default;
            if (_hero == null) return false;
            Vector3 delta = _hero.position - ground; delta.y = 0f;
            if (delta.sqrMagnitude < _plan.SafeHeroDistance * _plan.SafeHeroDistance) return false;
            location = new DefenseSpawnLocation(ground, 1);
            return true;
        }

        /// <summary>The returned position is used directly by the same synchronous spawn attempt.</summary>
        public bool TryResolve(DefenseSpawnJob job, out DefenseSpawnLocation location)
        {
            location = default;
            if (_eastMarker == null || _hero == null) return false;
            var point = _plan.Resolve(job, _eastMarker.position);
            if (_plan.GetEnemySnapshot(job.ActualSpawnType).Stats.Locomotion == DefenseEnemyLocomotionType.AmphibianWaterToShore)
            {
                if (_waterNavigation == null || _baseMarker == null ||
                    !_waterNavigation.TryGetExit(point.Position, out _, out var shore) ||
                    !_waterNavigation.TryResolveShore(shore, _baseMarker.position, out _)) return false;
                Vector3 waterDelta = _hero.position - point.Position;
                waterDelta.y = 0f;
                if (waterDelta.sqrMagnitude < _plan.SafeHeroDistance * _plan.SafeHeroDistance) return false;
                location = point;
                return true;
            }
            if (_plan.GetEnemySnapshot(job.ActualSpawnType).Stats.Locomotion == DefenseEnemyLocomotionType.AirFlight)
            {
                if (_airNavigation == null || !_airNavigation.TryResolveSpawn(point.Position, out var airborne)) return false;
                Vector3 safeDelta = _hero.position - airborne;
                safeDelta.y = 0f;
                if (safeDelta.sqrMagnitude < _plan.SafeHeroDistance * _plan.SafeHeroDistance) return false;
                location = new DefenseSpawnLocation(airborne, point.Lane);
                return true;
            }
            if (!NavMesh.SamplePosition(point.Position, out var hit, 2f, NavMesh.AllAreas)) return false;
            Vector3 delta = _hero.position - hit.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < _plan.SafeHeroDistance * _plan.SafeHeroDistance) return false;
            location = new DefenseSpawnLocation(hit.position, point.Lane);
            if (job.Kind == DefenseSpawnKind.ArenaBoss) return true;
            if (_baseMarker!=null && hit.position.x<=4.45f)
            {
                if (!NavMesh.SamplePosition(_baseMarker.position,out var destination,2f,NavMesh.AllAreas)) return false;
                return NavMesh.CalculatePath(hit.position,destination.position,NavMesh.AllAreas,_path) && _path.status==NavMeshPathStatus.PathComplete;
            }
            if (!NavMesh.SamplePosition(DefenseBridgeRoute3D.EntryPoint(point.Lane), out var entry, 1.3f, NavMesh.AllAreas))
                return false;
            return NavMesh.CalculatePath(hit.position, entry.position, NavMesh.AllAreas, _path) &&
                _path.status == NavMeshPathStatus.PathComplete;
        }
    }
}

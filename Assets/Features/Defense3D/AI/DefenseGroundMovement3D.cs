using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Follows a supplied route on NavMesh; owns no health, target or reward logic.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(NavMeshAgent))]
    public sealed class DefenseGroundMovement3D : DefenseGameplayBehaviour, IDefenseLocomotionStrategy3D
    {
        private NavMeshAgent agent;
        private DefenseEnemyStatus3D _status;
        private float _baseSpeed;
        public bool IsMoving => following;
        [SerializeField] private float _displacementSampleRadius = 0.2f;
        [SerializeField] private float _maximumDisplacementHeight = 0.25f;
        public void BindStatus(DefenseEnemyStatus3D status) => _status = status;
        public void ApplyMotionState()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            agent.speed = _baseSpeed * (_status != null ? _status.MovementMultiplier : 1f);
            agent.isStopped = !CanAdvanceGameplay || !following || agent.speed <= 0f || (_status != null && _status.BlocksActions);
        }
        public bool TryDisplace(Vector3 delta, bool apply)
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return false;
            if (!NavMesh.SamplePosition(transform.position, out var start, Mathf.Max(agent.height, Mathf.Abs(agent.baseOffset)), NavMesh.AllAreas)) return false;
            Vector3 startError = start.position - transform.position; startError.y = 0f;
            if (startError.sqrMagnitude > _displacementSampleRadius * _displacementSampleRadius) return false;
            Vector3 destination = start.position + delta;
            if (!NavMesh.SamplePosition(destination, out var end, _displacementSampleRadius, NavMesh.AllAreas) ||
                Mathf.Abs(end.position.y - start.position.y) > _maximumDisplacementHeight ||
                NavMesh.Raycast(start.position, end.position, out _, NavMesh.AllAreas)) return false;
            Vector3 error = end.position - destination; error.y = 0f;
            if (error.sqrMagnitude > _displacementSampleRadius * _displacementSampleRadius) return false;
            return !apply || agent.Warp(end.position);
        }
        public void ResumeAfterDisplacement()
        {
            if (following && agent != null && agent.enabled && agent.isOnNavMesh) SetNextDestination();
            ApplyMotionState();
        }
        private Vector3[] route;
        private int index;
        private float arrivalRadius;
        private bool following;
        private int _routeCount;
        private NavMeshPath _path;
        private readonly Vector3[] _pursuit=new Vector3[1];
        public bool HasFailed { get; private set; }
        public event Action Arrived;

        private DefenseHeroHealth3D _hero;
        public void BindHero(DefenseHeroHealth3D hero) => _hero = hero;

        public void Configure(int lane, DefenseBasePrototype3D target)
        {
            if (TryGetComponent<DefenseGroundAggro3D>(out var aggro) &&
                TryGetComponent<DefenseEnemy3D>(out var enemy) &&
                !enemy.Stats.IsBoss)
            {
                aggro.Configure(lane, target, _hero, enemy.Stats.Locomotion == DefenseEnemyLocomotionType.ParatrooperLanding);
            }
            else
            {
                var enemyStats = TryGetComponent<DefenseEnemy3D>(out var e) ? e.Stats : default;
                float speed = enemyStats.Speed > 0f ? enemyStats.Speed : 2.5f;
                var collider = target != null ? target.GetComponent<Collider>() : null;
                float extent = collider != null ? Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.z) : 0f;
                float radius = (enemyStats.AttackRange > 0f ? enemyStats.AttackRange : 1.2f) + extent;
                Vector3 targetPos = target != null ? target.transform.position : transform.position;
                Follow(DefenseBridgeRoute3D.Create(lane, transform.position, targetPos), speed, radius);
            }
        }

        public void Follow(Vector3[] waypoints, float speed, float radius)
        {
            if (waypoints == null || waypoints.Length == 0) throw new ArgumentException("Route must contain waypoints.", nameof(waypoints));
            FollowBuffer((Vector3[])waypoints.Clone(),waypoints.Length,speed,radius);
        }

        public bool CanReach(Vector3 target)
        {
            if (agent==null || !agent.enabled || !agent.isOnNavMesh) return false;
            if (!NavMesh.SamplePosition(target,out var hit,2f,NavMesh.AllAreas)) return false;
            Vector3 delta=hit.position-target; delta.y=0f;
            if (delta.sqrMagnitude>0.25f) return false;
            if (_path==null) _path=new NavMeshPath();
            return agent.CalculatePath(hit.position,_path) && _path.status==NavMeshPathStatus.PathComplete;
        }

        public void Pursue(Vector3 target,float speed,float radius)
        {
            _pursuit[0]=target;
            FollowBuffer(_pursuit,1,speed,radius);
        }

        public void FollowBuffer(Vector3[] waypoints,int count,float speed,float radius)
        {
            Stop();
            if (waypoints==null || count<1 || count>waypoints.Length) throw new ArgumentException("Route buffer/count invalid.");
            route=waypoints;
            _routeCount=count;
            index = 0;
            arrivalRadius = Mathf.Max(0.1f, radius);
            HasFailed = false;
            if (agent==null) agent = GetComponent<NavMeshAgent>();
            _baseSpeed = speed;
            agent.speed = speed * (_status != null ? _status.MovementMultiplier : 1f);
            agent.angularSpeed = 360f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.1f;
            agent.autoBraking = true;
            if (!agent.enabled || !NavMesh.SamplePosition(transform.position, out NavMeshHit start, 2f, NavMesh.AllAreas) || !agent.Warp(start.position))
            {
                Fail("spawn position is outside baked NavMesh or agent is disabled");
                return;
            }
            following = true;
            ApplyMotionState();
            SetNextDestination();
        }

        public void Stop()
        {
            following = false;
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        }
        private void OnDisable() => Stop();
        private void SetNextDestination()
        {
            if (!NavMesh.SamplePosition(route[index], out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Fail("waypoint " + index + " is not on NavMesh");
                return;
            }
            if (_path==null) _path=new NavMeshPath();
            if (!agent.CalculatePath(hit.position, _path) || _path.status != NavMeshPathStatus.PathComplete)
            {
                Fail("no complete route to waypoint " + index);
                return;
            }
            agent.stoppingDistance=index==_routeCount-1 ? Mathf.Max(0.1f,arrivalRadius*0.9f) : 0.1f;
            if (!agent.SetPath(_path)) Fail("SetPath rejected waypoint " + index);
        }
        private void Update()
        {
            ApplyMotionState();
            if (!CanAdvanceGameplay || (_status != null && _status.BlocksActions)) return;
            if (!following || agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            if (agent.pathPending) return;
            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                Fail("path became incomplete at waypoint " + index);
                return;
            }
            float reach=index==_routeCount-1 ? arrivalRadius : 0.35f;
            if (agent.remainingDistance > Mathf.Max(reach, agent.stoppingDistance + 0.1f)) return;
            // A pending destination can briefly report remainingDistance == 0.
            // Verify world position before advancing the route.
            Vector3 delta = agent.destination - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > reach * reach) return;
            if (agent.hasPath && agent.velocity.sqrMagnitude > 0.02f) return;
            if (++index >= _routeCount)
            {
                Stop();
                Arrived?.Invoke();
            }
            else SetNextDestination();
        }
        private void Fail(string reason)
        {
            Stop();
            HasFailed = true;
            Debug.LogWarning("[TREBRAVE Defense3D] Enemy route stopped: " + reason + " (" + name + ").");
        }
    }
}

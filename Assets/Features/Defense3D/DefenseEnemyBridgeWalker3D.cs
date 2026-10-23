using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Three-lane NavMesh smoke test. Each lane validates a complete path to its
    /// bridge and across the canal before proceeding. Never moves through water
    /// by transform fallback when NavMesh is absent or disconnected.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class DefenseEnemyBridgeWalker3D : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 2.5f;
        [SerializeField, Min(0f)] private float baseDamage = 25f;
        [SerializeField, Min(0.1f)] private float arrivalRadius = 1.2f;
        [SerializeField, Min(0.1f)] private float attackInterval = 2f;
        [SerializeField, Min(1f)] private float maxHP = 100f;
        [SerializeField] private DefenseBasePrototype3D targetBase;
        public float CurrentHP { get; private set; }
        public bool IsDead => CurrentHP <= 0f;
        private float nextAttackTime;
        private bool attacking;
        private NavMeshAgent agent;
        private Vector3[] route;
        private int index;
        private bool finished;

        public void Configure(int lane, DefenseBasePrototype3D target)
        {
            targetBase = target;
            CurrentHP = maxHP;
            float z = lane == 0 ? -10.5f : lane == 2 ? 10.5f : -0.5f;
            route = new[]
            {
                new Vector3(8.8f, 0f, z),
                new Vector3(6.25f, 0f, z),
                new Vector3(3.8f, 0f, z),
                new Vector3(-1.8f, 0f, z),
                new Vector3(-7.5f, 0f, z * 0.6f),
                new Vector3(-12f, 0f, 0f)
            };
            index = 0;
            finished = false;
            agent = GetComponent<NavMeshAgent>();
            agent.speed = speed;
            agent.angularSpeed = 360f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.1f;
            agent.autoBraking = true;

            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit start, 2f, NavMesh.AllAreas))
            {
                Fail("spawn position is outside baked NavMesh");
                return;
            }
            agent.Warp(start.position);
            SetNextDestination();
        }

        private void SetNextDestination()
        {
            if (finished || agent == null || !agent.isOnNavMesh || index >= route.Length)
                return;
            if (!NavMesh.SamplePosition(route[index], out NavMeshHit hit, 1.3f, NavMesh.AllAreas))
            {
                Fail("waypoint " + index + " is not on NavMesh");
                return;
            }
            var path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                Fail("no complete route to waypoint " + index + " (check bridge NavMesh connection)");
                return;
            }
            if (!agent.SetDestination(hit.position))
                Fail("SetDestination rejected waypoint " + index);
        }

        private void Update()
        {
            if (finished || IsDead || agent == null || !agent.isOnNavMesh || targetBase == null)
                return;
            if (targetBase.IsDestroyed)
            {
                finished = true;
                agent.isStopped = true;
                return;
            }
            if (attacking)
            {
                if (Time.time >= nextAttackTime)
                {
                    targetBase.ReceiveDamage(baseDamage);
                    nextAttackTime = Time.time + attackInterval;
                    Debug.Log("[TREBRAVE Defense3D] Enemy attacked Base: " + baseDamage);
                }
                return;
            }
            if (agent.pathPending) return;
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Fail("path became incomplete at waypoint " + index);
                return;
            }
            if (agent.remainingDistance > Mathf.Max(arrivalRadius, agent.stoppingDistance + 0.1f))
                return;
            if (agent.hasPath && agent.velocity.sqrMagnitude > 0.02f)
                return;
            if (++index >= route.Length)
            {
                attacking = true;
                agent.isStopped = true;
                nextAttackTime = Time.time;
                Debug.Log("[TREBRAVE Defense3D] NavMesh enemy reached Base; starting attacks.");
                return;
            }
            SetNextDestination();
        }

        public void ReceiveDamage(float damage)
        {
            if (finished || IsDead || damage <= 0f) return;
            CurrentHP = Mathf.Max(0f, CurrentHP - damage);
            if (!IsDead) return;
            finished = true;
            Debug.Log("[TREBRAVE Defense3D] Enemy defeated: " + name);
            Destroy(gameObject);
        }

        private void Fail(string reason)
        {
            finished = true;
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            Debug.LogWarning("[TREBRAVE Defense3D] Enemy route stopped: " + reason + " (" + name + ").");
        }
    }
}

using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// First bridge-path smoke test, deliberately independent of NavMesh baking.
    /// Follows one of three canonical Sông Quê lanes; does not cross the river
    /// outside a bridge. Replace with NavMeshAgent when walkability is validated.
    /// </summary>
    public sealed class DefenseEnemyBridgeWalker3D : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 2.5f;
        [SerializeField, Min(0.05f)] private float waypointTolerance = 0.2f;
        [SerializeField, Min(0f)] private float baseDamage = 25f;
        [SerializeField] private DefenseBasePrototype3D targetBase;
        private Vector3[] route;
        private int index;
        private bool finished;

        public void Configure(int lane, DefenseBasePrototype3D target)
        {
            float z = lane == 0 ? -10.5f : lane == 2 ? 10.5f : -0.5f;
            targetBase = target;
            // Route moves strictly on land -> bridge -> land. The last
            // waypoint is deliberately in front of the Base at x=-12.
            route = new[]
            {
                new Vector3(8.8f, 0f, z),
                new Vector3(8.3f, 0f, z),
                new Vector3(6.25f, 0f, z),
                new Vector3(3.8f, 0f, z),
                new Vector3(-1.8f, 0f, z),
                new Vector3(-7.5f, 0f, z * 0.6f),
                new Vector3(-12f, 0f, 0f)
            };
            index = 0;
            finished = false;
        }

        private void Update()
        {
            if (finished || route == null || route.Length == 0 || targetBase == null || targetBase.IsDestroyed)
                return;
            Vector3 destination = route[index];
            Vector3 direction = destination - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;
            if (distance <= waypointTolerance)
            {
                if (++index >= route.Length)
                {
                    finished = true;
                    targetBase.ReceiveDamage(baseDamage);
                    Debug.Log("[TREBRAVE Defense3D] Test enemy reached Base; damage: " + baseDamage);
                    Destroy(gameObject);
                }
                return;
            }

            float movement = Mathf.Min(speed * Time.deltaTime, distance);
            transform.position += direction.normalized * movement;
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction.normalized), Time.deltaTime * 8f);
        }
    }
}

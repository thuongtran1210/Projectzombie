using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Simple stationary auto-attack tower for the Defense MVP.</summary>
    public sealed class DefenseTowerPrototype3D : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float attackRange = 8f;
        [SerializeField, Min(1f)] private float damage = 22f;
        [SerializeField, Min(0.1f)] private float fireInterval = 0.9f;
        private float nextShot;

        private void Update()
        {
            if (Time.time < nextShot) return;
            DefenseEnemyBridgeWalker3D target = null;
            float best = attackRange * attackRange;
            foreach (var enemy in FindObjectsOfType<DefenseEnemyBridgeWalker3D>())
            {
                if (!enemy.isActiveAndEnabled || enemy.IsDead) continue;
                var delta = enemy.transform.position - transform.position;
                delta.y = 0f;
                float sqr = delta.sqrMagnitude;
                if (sqr >= best) continue;
                best = sqr;
                target = enemy;
            }
            if (target == null) return;
            nextShot = Time.time + fireInterval;
            target.ReceiveDamage(damage);
            Debug.DrawLine(transform.position + Vector3.up, target.transform.position + Vector3.up, Color.yellow, 0.4f);
            Debug.Log("[TREBRAVE Defense3D] Tower hit " + target.name + " for " + damage);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}

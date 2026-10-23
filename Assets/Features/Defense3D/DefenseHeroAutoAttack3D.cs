using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Opt-in basic hero attack for the Defense 3D prototype.
    /// Automatically hits the nearest living test enemy within range on a cooldown.
    /// No Physics overlap/collider required because prototype enemies have no colliders.
    /// </summary>
    public sealed class DefenseHeroAutoAttack3D : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float attackRange = 5f;
        [SerializeField, Min(1f)] private float damage = 35f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 0.65f;
        [SerializeField] private bool drawAttackRange = true;
        private float nextAttackAt;
        private int defeatedEnemies;

        private void Update()
        {
            if (Time.time < nextAttackAt) return;
            DefenseEnemyBridgeWalker3D enemy = FindNearestEnemy();
            if (enemy == null) return;
            nextAttackAt = Time.time + attackCooldown;
            enemy.ReceiveDamage(damage);
            if (enemy.IsDead) defeatedEnemies++;
            Debug.Log("[TREBRAVE Defense3D] Hero hit " + enemy.name +
                " for " + damage + ". Enemy HP: " + enemy.CurrentHP + "; defeated: " + defeatedEnemies);
        }

        private DefenseEnemyBridgeWalker3D FindNearestEnemy()
        {
            var enemies = FindObjectsOfType<DefenseEnemyBridgeWalker3D>();
            DefenseEnemyBridgeWalker3D nearest = null;
            float best = attackRange * attackRange;
            Vector3 heroPos = transform.position;
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDead || !enemy.isActiveAndEnabled) continue;
                Vector3 delta = enemy.transform.position - heroPos;
                delta.y = 0f;
                float dist = delta.sqrMagnitude;
                if (dist > best) continue;
                best = dist;
                nearest = enemy;
            }
            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawAttackRange) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}

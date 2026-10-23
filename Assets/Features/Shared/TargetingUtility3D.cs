using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Queries live 3D enemy entities, not individual hitbox colliders.
    /// On mobile, callers own the output buffer; physics query buffers are reused.
    /// </summary>
    public static class TargetingUtility3D
    {
        private const int BUFFER_SIZE = 120;
        private static readonly Collider[] _hitBuffer = new Collider[BUFFER_SIZE];
        private static readonly HealthSystem[] _randomSeenBuffer = new HealthSystem[BUFFER_SIZE];

        private static int _enemyLayerMask = -1;
        public static int EnemyLayerMask
        {
            get
            {
                if (_enemyLayerMask == -1)
                {
                    // Do not silently target objects on Default when the Enemy layer is missing.
                    _enemyLayerMask = LayerMask.GetMask("Enemy");
                }
                return _enemyLayerMask;
            }
        }

        private static HealthSystem ResolveLiveTarget(Collider hit)
        {
            if (hit == null) return null;
            // In a 3D hierarchy, hitboxes are often children and the HealthSystem
            // is attached to the logical enemy root.
            var health = hit.GetComponentInParent<HealthSystem>();
            return health != null && health.IsAlive ? health : null;
        }

        private static float HorizontalDistanceSquared(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }

        /// <summary>
        /// Returns the transform of a LIVE enemy entity root (HealthSystem host),
        /// including if a child collider was hit.
        /// </summary>
        public static Transform FindTarget(
            Vector3 origin,
            float range,
            TargetPriority priority = TargetPriority.Nearest,
            ElementType attackerElement = ElementType.None,
            int customLayerMask = 0)
        {
            if (range <= 0f) return null;
            int mask = customLayerMask != 0 ? customLayerMask : EnemyLayerMask;
            if (mask == 0) return null;

            int hitCount = Physics.OverlapSphereNonAlloc(origin, range, _hitBuffer, mask);
            Transform bestTarget = null;
            float minMetric = float.MaxValue;
            float maxMetric = float.MinValue;
            int randomUniqueCount = 0;

            for (int i = 0; i < hitCount; i++)
            {
                var health = ResolveLiveTarget(_hitBuffer[i]);
                if (health == null) continue;

                Transform root = health.transform;
                float sqrDistance = HorizontalDistanceSquared(origin, root.position);

                switch (priority)
                {
                    case TargetPriority.Nearest:
                        if (sqrDistance < minMetric)
                        {
                            minMetric = sqrDistance;
                            bestTarget = root;
                        }
                        break;

                    case TargetPriority.LowestHealth:
                        if (health.CurrentHealth < minMetric)
                        {
                            minMetric = health.CurrentHealth;
                            bestTarget = root;
                        }
                        break;

                    case TargetPriority.HighestHealth:
                        if (health.CurrentHealth > maxMetric)
                        {
                            maxMetric = health.CurrentHealth;
                            bestTarget = root;
                        }
                        break;

                    case TargetPriority.ElementalAdvantage:
                        var enemy = health.GetComponent<Enemy>();
                        var defenderElement = enemy != null ? enemy.CurrentElement : ElementType.None;
                        float multiplier = DamageUtility.GetElementMultiplier(attackerElement, defenderElement);
                        if (multiplier > 1.0f)
                        {
                            if (sqrDistance < minMetric)
                            {
                                minMetric = sqrDistance;
                                bestTarget = root;
                            }
                        }
                        else if (bestTarget == null)
                        {
                            bestTarget = root;
                        }
                        break;

                    case TargetPriority.RandomInRange:
                        // Reservoir sampling must weight ENTITIES, not their collider count.
                        bool alreadySeen = false;
                        for (int seen = 0; seen < randomUniqueCount; seen++)
                        {
                            if (_randomSeenBuffer[seen] == health)
                            {
                                alreadySeen = true;
                                break;
                            }
                        }
                        if (alreadySeen) break;
                        _randomSeenBuffer[randomUniqueCount] = health;
                        randomUniqueCount++;
                        if (Random.Range(0, randomUniqueCount) == 0)
                            bestTarget = root;
                        break;
                }
            }

            return bestTarget;
        }

        /// <summary>
        /// Fills the supplied list with distinct, live enemy entity roots.
        /// No allocations in the normal query loop when the caller pre-sizes the list.
        /// </summary>
        public static int FindTargetsNonAlloc(
            Vector3 origin,
            float range,
            List<Transform> resultsBuffer,
            int maxTargets = 5,
            int customLayerMask = 0)
        {
            if (resultsBuffer == null) return 0;
            resultsBuffer.Clear();
            if (range <= 0f || maxTargets <= 0) return 0;

            int mask = customLayerMask != 0 ? customLayerMask : EnemyLayerMask;
            if (mask == 0) return 0;

            int numHits = Physics.OverlapSphereNonAlloc(origin, range, _hitBuffer, mask);
            for (int i = 0; i < numHits && resultsBuffer.Count < maxTargets; i++)
            {
                var health = ResolveLiveTarget(_hitBuffer[i]);
                if (health == null) continue;

                Transform root = health.transform;
                if (!resultsBuffer.Contains(root))
                    resultsBuffer.Add(root);
            }
            return resultsBuffer.Count;
        }
    }
}

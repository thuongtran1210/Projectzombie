using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Chiến lược ưu tiên lựa chọn mục tiêu quái vật.
    /// </summary>
    public enum TargetPriority
    {
        Nearest,            // Gần nhất (Mặc định)
        LowestHealth,       // Ít máu nhất (Dọn quái yếu)
        HighestHealth,      // Nhiều máu nhất (Boss / Elite)
        RandomInRange,      // Ngẫu nhiên trong tầm
        ElementalAdvantage  // Ưu tiên quái bị hệ của vũ khí khắc chế (+30% dmg)
    }

    /// <summary>
    /// Lớp tiện ích tĩnh hỗ trợ quét và tìm kiếm quái vật mục tiêu trong tầm đánh.
    /// Đảm bảo 0 GC Allocation thông qua buffer tĩnh tái sử dụng.
    /// </summary>
    public static class TargetingUtility
    {
        private const int BUFFER_SIZE = 100;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[BUFFER_SIZE];

        /// <summary>
        /// Tìm 1 mục tiêu quái vật tối ưu nhất theo chiến lược lựa chọn.
        /// </summary>
        public static Transform FindTarget(
            Vector3 origin, 
            float range, 
            TargetPriority priority = TargetPriority.Nearest, 
            ElementType attackerElement = ElementType.None)
        {
            int numHits = Physics2D.OverlapCircleNonAlloc(origin, range, _hitBuffer);
            if (numHits <= 0) return null;

            Transform bestTarget = null;
            float minMetric = float.MaxValue;
            float maxMetric = float.MinValue;
            List<Transform> candidatePool = null;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null || !hit.CompareTag("Enemy")) continue;

                if (hit.TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth <= 0)
                {
                    continue; // Bỏ qua quái đã chết
                }

                Vector3 targetPos = hit.transform.position;
                float sqrDist = (origin - targetPos).sqrMagnitude;

                switch (priority)
                {
                    case TargetPriority.Nearest:
                        if (sqrDist < minMetric)
                        {
                            minMetric = sqrDist;
                            bestTarget = hit.transform;
                        }
                        break;

                    case TargetPriority.LowestHealth:
                        float curHp = health != null ? health.CurrentHealth : 0f;
                        if (curHp < minMetric)
                        {
                            minMetric = curHp;
                            bestTarget = hit.transform;
                        }
                        break;

                    case TargetPriority.HighestHealth:
                        float maxHp = health != null ? health.CurrentHealth : 0f;
                        if (maxHp > maxMetric)
                        {
                            maxMetric = maxHp;
                            bestTarget = hit.transform;
                        }
                        break;

                    case TargetPriority.ElementalAdvantage:
                        // Ưu tiên quái có hệ bị khắc chế
                        ElementType defElement = ElementType.None;
                        if (hit.TryGetComponent<Enemy>(out var enemy))
                        {
                            defElement = enemy.CurrentElement;
                        }
                        float multiplier = DamageUtility.GetElementMultiplier(attackerElement, defElement);
                        if (multiplier > 1.0f) // Khắc chế (+30%)
                        {
                            if (sqrDist < minMetric)
                            {
                                minMetric = sqrDist;
                                bestTarget = hit.transform;
                            }
                        }
                        else if (bestTarget == null)
                        {
                            // Fallback nếu chưa tìm thấy quái khắc chế
                            bestTarget = hit.transform;
                        }
                        break;

                    case TargetPriority.RandomInRange:
                        if (candidatePool == null) candidatePool = new List<Transform>();
                        candidatePool.Add(hit.transform);
                        break;
                }
            }

            if (priority == TargetPriority.RandomInRange && candidatePool != null && candidatePool.Count > 0)
            {
                int randomIndex = Random.Range(0, candidatePool.Count);
                return candidatePool[randomIndex];
            }

            return bestTarget;
        }

        /// <summary>
        /// Tìm quái vật gần nhất trong bán kính cho trước (Overload tiện dụng).
        /// </summary>
        public static Transform FindNearestEnemy(Vector3 origin, float range)
        {
            return FindTarget(origin, range, TargetPriority.Nearest);
        }

        /// <summary>
        /// Tìm danh sách N quái vật gần nhất trong tầm (Zero-Alloc nếu truyền sẵn results list).
        /// </summary>
        public static void FindNearestEnemiesNonAlloc(Vector3 origin, float range, int maxTargets, List<Transform> results)
        {
            if (results == null) return;
            results.Clear();

            int numHits = Physics2D.OverlapCircleNonAlloc(origin, range, _hitBuffer);
            if (numHits <= 0) return;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null || !hit.CompareTag("Enemy")) continue;

                if (hit.TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth <= 0) continue;

                results.Add(hit.transform);
                if (results.Count >= maxTargets) break;
            }
        }
    }
}

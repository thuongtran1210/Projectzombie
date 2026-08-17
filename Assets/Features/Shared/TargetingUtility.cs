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
    /// Tối ưu tuyệt đối cho Mobile: 0 GC Allocation, LayerMask filtering ở tầng C++ Physics.
    /// </summary>
    public static class TargetingUtility
    {
        private const int BUFFER_SIZE = 100;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[BUFFER_SIZE];

        // Cache LayerMask tầng C++ Physics Engine (Bitwise O(1))
        private static int _enemyLayerMask = -1;
        public static int EnemyLayerMask
        {
            get
            {
                if (_enemyLayerMask == -1)
                {
                    int mask = LayerMask.GetMask("Enemy");
                    _enemyLayerMask = mask != 0 ? mask : LayerMask.GetMask("Default", "Enemy");
                }
                return _enemyLayerMask;
            }
        }

        /// <summary>
        /// Tìm 1 mục tiêu quái vật tối ưu nhất theo chiến lược lựa chọn (0 GC Allocation).
        /// </summary>
        public static Transform FindTarget(
            Vector3 origin, 
            float range, 
            TargetPriority priority = TargetPriority.Nearest, 
            ElementType attackerElement = ElementType.None,
            int customLayerMask = 0)
        {
            int mask = customLayerMask != 0 ? customLayerMask : EnemyLayerMask;
            int numHits = Physics2D.OverlapCircleNonAlloc(origin, range, _hitBuffer, mask);
            if (numHits <= 0) return null;

            Transform bestTarget = null;
            float minMetric = float.MaxValue;
            float maxMetric = float.MinValue;
            int validCandidateCount = 0;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null) continue;

                // Kiểm tra nhanh tag hoặc component nếu mask rộng
                if (!hit.CompareTag("Enemy") && customLayerMask == 0 && mask == ~0) continue;

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
                            bestTarget = hit.transform;
                        }
                        break;

                    case TargetPriority.RandomInRange:
                        // Reservoir Sampling: 0 GC Alloc chọn ngẫu nhiên 1 mục tiêu
                        validCandidateCount++;
                        if (Random.Range(0, validCandidateCount) == 0)
                        {
                            bestTarget = hit.transform;
                        }
                        break;
                }
            }

            return bestTarget;
        }

        /// <summary>
        /// Tìm quái vật gần nhất trong bán kính cho trước (Overload tiện dụng).
        /// </summary>
        public static Transform FindNearestEnemy(Vector3 origin, float range, int customLayerMask = 0)
        {
            return FindTarget(origin, range, TargetPriority.Nearest, ElementType.None, customLayerMask);
        }

        /// <summary>
        /// Tìm danh sách N quái vật gần nhất trong tầm (Zero-Alloc nếu truyền sẵn results list).
        /// </summary>
        public static void FindNearestEnemiesNonAlloc(Vector3 origin, float range, int maxTargets, List<Transform> results, int customLayerMask = 0)
        {
            if (results == null) return;
            results.Clear();

            int mask = customLayerMask != 0 ? customLayerMask : EnemyLayerMask;
            int numHits = Physics2D.OverlapCircleNonAlloc(origin, range, _hitBuffer, mask);
            if (numHits <= 0) return;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth <= 0) continue;

                results.Add(hit.transform);
                if (results.Count >= maxTargets) break;
            }
        }
    }
}

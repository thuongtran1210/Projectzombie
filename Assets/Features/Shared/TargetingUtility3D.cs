using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Lớp tiện ích tĩnh hỗ trợ quét và tìm kiếm quái vật mục tiêu trong tầm đánh 3D.
    /// Tính toán khoảng cách trên mặt phẳng ngang (X-Z), bỏ qua độ chênh lệch chiều cao Y nhỏ.
    /// Tối ưu tuyệt đối cho Mobile 60 FPS: 0 GC Allocation, LayerMask filtering ở tầng C++ Physics.
    /// Tuân thủ quy tắc 6.1 & 6.2 AGENTS.md.
    /// </summary>
    public static class TargetingUtility3D
    {
        private const int BUFFER_SIZE = 120;
        private static readonly Collider[] _hitBuffer = new Collider[BUFFER_SIZE];

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
        /// Tìm 1 mục tiêu quái vật tối ưu nhất trong không gian 3D theo chiến lược lựa chọn (0 GC Allocation).
        /// </summary>
        public static Transform FindTarget(
            Vector3 origin, 
            float range, 
            TargetPriority priority = TargetPriority.Nearest, 
            ElementType attackerElement = ElementType.None,
            int customLayerMask = 0)
        {
            int mask = customLayerMask != 0 ? customLayerMask : EnemyLayerMask;
            int numHits = Physics.OverlapSphereNonAlloc(origin, range, _hitBuffer, mask);
            if (numHits <= 0) return null;

            Transform bestTarget = null;
            float minMetric = float.MaxValue;
            float maxMetric = float.MinValue;
            int validCandidateCount = 0;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null) continue;

                if (!hit.CompareTag("Enemy") && customLayerMask == 0 && mask == ~0) continue;

                if (hit.TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth <= 0)
                {
                    continue;
                }

                Vector3 targetPos = hit.transform.position;
                // Khoảng cách ngang trên mặt phẳng X-Z (bỏ qua độ dốc/chiều cao Y)
                float dx = origin.x - targetPos.x;
                float dz = origin.z - targetPos.z;
                float sqrHorizontalDist = dx * dx + dz * dz;

                switch (priority)
                {
                    case TargetPriority.Nearest:
                        if (sqrHorizontalDist < minMetric)
                        {
                            minMetric = sqrHorizontalDist;
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
                            if (sqrHorizontalDist < minMetric)
                            {
                                minMetric = sqrHorizontalDist;
                                bestTarget = hit.transform;
                            }
                        }
                        else if (bestTarget == null)
                        {
                            bestTarget = hit.transform;
                        }
                        break;

                    case TargetPriority.RandomInRange:
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
        /// Tìm nhiều mục tiêu theo số lượng tối đa maxTargets (0 GC Allocation qua danh sách buffer truyền vào).
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

            int mask = customLayerMask != 0 ? customLayerMask : EnemyLayerMask;
            int numHits = Physics.OverlapSphereNonAlloc(origin, range, _hitBuffer, mask);
            if (numHits <= 0) return 0;

            for (int i = 0; i < numHits && resultsBuffer.Count < maxTargets; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null) continue;

                if (!hit.CompareTag("Enemy") && customLayerMask == 0 && mask == ~0) continue;

                if (hit.TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth <= 0)
                {
                    continue;
                }

                resultsBuffer.Add(hit.transform);
            }

            return resultsBuffer.Count;
        }
    }
}

using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Owns bounded, one-hop target selection for Hỏa Hoạn Burn spread.</summary>
    internal sealed class FireSpreadService
    {
        private readonly FireSpreadSettings _settings;
        private readonly Collider2D[] _colliderBuffer;
        private readonly Enemy[] _targetBuffer;
        private readonly float[] _targetDistanceBuffer;

        public FireSpreadService(FireSpreadSettings settings)
        {
            _settings = settings;
            _colliderBuffer = new Collider2D[settings.colliderBufferSize];
            _targetBuffer = new Enemy[settings.secondaryTargetCap];
            _targetDistanceBuffer = new float[settings.secondaryTargetCap];
        }

        public void SpreadFrom(Enemy source, float burnDamage)
        {
            if (source == null || source.IsBoss || _settings.secondaryTargetCap == 0)
                return;

            int colliderCount = Physics2D.OverlapCircleNonAlloc(
                source.transform.position,
                _settings.spreadRadius,
                _colliderBuffer,
                TargetingUtility.EnemyLayerMask);

            // Saturation means the query may be truncated; skip deterministically rather than selecting an arbitrary subset.
            if (colliderCount >= _colliderBuffer.Length)
                return;

            ApplySpreadTargets(burnDamage, CollectTargets(source, colliderCount));
        }

        private void ApplySpreadTargets(float burnDamage, int targetCount)
        {
            for (int i = 0; i < targetCount; i++)
            {
                Enemy target = _targetBuffer[i];
                _targetBuffer[i] = null;
                if (target == null || !target.HealthSystem.IsAlive || target.HasStatus(StatusEffectType.Burn) ||
                    target.StatusController == null || target.StatusController.IsImmuneTo(StatusEffectType.Burn))
                    continue;

                ApplyBurn(target, burnDamage);
            }
        }

        private int CollectTargets(Enemy source, int colliderCount)
        {
            int targetCount = 0;
            Vector2 sourcePosition = source.transform.position;
            for (int i = 0; i < colliderCount; i++)
            {
                Collider2D collider = _colliderBuffer[i];
                if (collider == null || !collider.TryGetComponent(out Enemy target) ||
                    target == null || target == source ||
                    !target.HealthSystem.IsAlive || target.HasStatus(StatusEffectType.Burn) ||
                    target.StatusController == null || target.StatusController.IsImmuneTo(StatusEffectType.Burn) ||
                    ContainsTarget(targetCount, target))
                    continue;

                AddNearestTarget(target, (Vector2)target.transform.position - sourcePosition, ref targetCount);
            }

            return targetCount;
        }

        private void AddNearestTarget(Enemy target, Vector2 offset, ref int targetCount)
        {
            float distance = offset.sqrMagnitude;
            if (targetCount < _targetBuffer.Length)
            {
                _targetBuffer[targetCount] = target;
                _targetDistanceBuffer[targetCount++] = distance;
                return;
            }

            int farthestIndex = 0;
            for (int i = 1; i < targetCount; i++)
            {
                if (_targetDistanceBuffer[i] > _targetDistanceBuffer[farthestIndex])
                    farthestIndex = i;
            }

            if (distance < _targetDistanceBuffer[farthestIndex])
            {
                _targetBuffer[farthestIndex] = target;
                _targetDistanceBuffer[farthestIndex] = distance;
            }
        }

        private bool ContainsTarget(int count, Enemy target)
        {
            for (int i = 0; i < count; i++)
                if (_targetBuffer[i] == target)
                    return true;
            return false;
        }

        private void ApplyBurn(Enemy target, float burnDamage)
        {
            target.StatusController.ApplyDamageOverTime(
                StatusEffectType.Burn,
                _settings.burnDuration,
                burnDamage,
                _settings.burnTickInterval,
                damage => ApplyBurnDamage(target, damage));
        }

        private static void ApplyBurnDamage(Enemy target, float damage)
        {
            if (target != null && target.HealthSystem != null)
                target.HealthSystem.TakeDamage(new DamageData(damage, element: ElementType.Hoa, canTriggerReaction: false));
        }
    }
}

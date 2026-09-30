using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Applies Toái Giáp stagger and a bounded one-shot fragment hit.</summary>
    internal sealed class ToaiGiapReaction
    {
        private readonly ToaiGiapReactionSettings _settings;
        private readonly Collider2D[] _colliderBuffer;
        private readonly Enemy[] _targetBuffer;
        private readonly float[] _distanceBuffer;

        public ToaiGiapReaction(ToaiGiapReactionSettings settings)
        {
            _settings = settings;
            _colliderBuffer = new Collider2D[settings.colliderBufferSize];
            _targetBuffer = new Enemy[settings.secondaryTargetCap];
            _distanceBuffer = new float[settings.secondaryTargetCap];
        }

        public bool TryExecute(Enemy primary, DamageData triggeringDamage)
        {
            if (primary == null || primary.HealthSystem == null || !primary.HealthSystem.IsAlive)
                return false;

            primary.ApplyStatusEffect(StatusEffectType.Stun, _settings.staggerDuration);
            PlayFragmentVisual(primary.transform.position);
            global::ProjectZombie.Core.Audio.AudioService.Current?.PlayElementalReaction(primary.transform.position);

            if (_settings.secondaryTargetCap == 0 || _settings.fragmentDamageMultiplier <= 0f)
                return true;

            Vector2 center = primary.transform.position;
            int colliderCount = Physics2D.OverlapCircleNonAlloc(
                center,
                _settings.fragmentRadius,
                _colliderBuffer,
                TargetingUtility.EnemyLayerMask);

            // A saturated query may omit nearer enemies; skip rather than choose a partial, unstable set.
            if (colliderCount >= _colliderBuffer.Length)
                return true;

            int targetCount = CollectNearestTargets(primary, center, colliderCount);
            float fragmentDamage = Mathf.Max(0f, triggeringDamage.Amount * _settings.fragmentDamageMultiplier);
            DamageData fragmentHit = new DamageData(
                fragmentDamage,
                isCritical: false,
                element: ElementType.None,
                sourceWeapon: triggeringDamage.SourceWeapon,
                canTriggerReaction: false);

            for (int i = 0; i < targetCount; i++)
            {
                Enemy target = _targetBuffer[i];
                _targetBuffer[i] = null;
                if (target == null || !target.HealthSystem.IsAlive)
                    continue;

                target.HealthSystem.TakeDamage(fragmentHit);
                PlayFragmentVisual(target.transform.position);
            }

            return true;
        }

        private int CollectNearestTargets(Enemy primary, Vector2 center, int colliderCount)
        {
            int targetCount = 0;
            for (int i = 0; i < colliderCount; i++)
            {
                Collider2D collider = _colliderBuffer[i];
                if (collider == null || !collider.TryGetComponent(out Enemy target) ||
                    target == null || target == primary || target.HealthSystem == null || !target.HealthSystem.IsAlive ||
                    ContainsTarget(targetCount, target))
                    continue;

                AddNearestTarget(target, ((Vector2)target.transform.position - center).sqrMagnitude, ref targetCount);
            }

            return targetCount;
        }

        private void AddNearestTarget(Enemy target, float squaredDistance, ref int targetCount)
        {
            if (targetCount < _targetBuffer.Length)
            {
                _targetBuffer[targetCount] = target;
                _distanceBuffer[targetCount++] = squaredDistance;
                return;
            }

            int farthestIndex = 0;
            for (int i = 1; i < targetCount; i++)
            {
                if (_distanceBuffer[i] > _distanceBuffer[farthestIndex])
                    farthestIndex = i;
            }

            if (squaredDistance < _distanceBuffer[farthestIndex])
            {
                _targetBuffer[farthestIndex] = target;
                _distanceBuffer[farthestIndex] = squaredDistance;
            }
        }

        private bool ContainsTarget(int count, Enemy target)
        {
            for (int i = 0; i < count; i++)
            {
                if (_targetBuffer[i] == target)
                    return true;
            }

            return false;
        }

        private void PlayFragmentVisual(Vector3 position)
        {
            if (_settings.fragmentVisualPrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.PlayEffect(
                    _settings.fragmentVisualPrefab,
                    position,
                    Quaternion.identity,
                    0.6f);
            }
        }
    }
}

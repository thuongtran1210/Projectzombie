using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Enemies.Visuals;
using ProjectZombie.Core.ScriptableObjects;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Enemy))]
    public sealed class SteamSlipEnemyResponse : MonoBehaviour
    {
        private Enemy _enemy;
        private EnemyStatusVisuals _visuals;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _visuals = GetComponent<EnemyStatusVisuals>();
        }

        public void ApplySlip(Vector2 direction, SteamSlipReactionSettings settings)
        {
            if (_enemy == null || settings == null || _enemy.ReactionClass == EnemyReactionClass.Boss) return;

            switch (_enemy.ReactionClass)
            {
                case EnemyReactionClass.Light:
                    _enemy.ApplyKnockback(direction, settings.lightKnockbackForce, settings.lightSlideDuration);
                    GetVisuals()?.PlaySteamSlipSpin(settings.lightSlideDuration);
                    break;
                case EnemyReactionClass.Heavy:
                    ApplyStagger(settings.heavyStaggerDuration);
                    break;
                case EnemyReactionClass.Flying:
                    GetVisuals()?.PlaySteamSlipWobble(settings.flyingWobbleDuration);
                    break;
                default:
                    ApplyStagger(settings.standardStumbleDuration);
                    break;
            }
        }

        public void ApplyCollisionStagger(SteamSlipReactionSettings settings)
        {
            if (settings != null && _enemy != null && _enemy.ReactionClass != EnemyReactionClass.Boss)
                ApplyStagger(settings.collisionStaggerDuration);
        }

        private void ApplyStagger(float duration) => _enemy.ApplyStatusEffect(StatusEffectType.Stun, duration);

        private EnemyStatusVisuals GetVisuals()
        {
            if (_visuals == null)
                _visuals = GetComponent<EnemyStatusVisuals>();
            return _visuals;
        }
    }
}

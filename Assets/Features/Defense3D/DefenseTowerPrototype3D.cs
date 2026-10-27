using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Simple stationary auto-attack tower for the Defense MVP.</summary>
    public sealed class DefenseTowerPrototype3D : DefenseGameplayBehaviour
    {
        [SerializeField, Min(0.1f)] private float attackRange = 8f;
        [SerializeField, Min(1f)] private float damage = 22f;
        [SerializeField, Min(0.1f)] private float fireInterval = 0.9f;
        [SerializeField] private DefenseAttackTargetingProfileSO _targeting;
        private DefenseAttackTargetSelector3D _targetSelector;
        private float nextShot;
        public float EffectiveDamage => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerDamage, damage) : damage;
        public float EffectiveRange => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerAttackRange, attackRange) : attackRange;

        protected override void OnGameplayBound() => _targetSelector = new DefenseAttackTargetSelector3D(transform, _targeting);

        protected override void OnGameplayReset() => nextShot = 0f;

        private void Update()
        {
            if (!CanAdvanceGameplay || GameplayTime < nextShot) return;
            var target = _targetSelector?.Find(Gameplay.Targets, EffectiveRange);
            if (target == null) return;
            nextShot = GameplayTime + fireInterval;
            target.TakeDamage(new DefenseDamageContext(EffectiveDamage, this, target.TargetTransform.position,
                runGeneration: Gameplay.RunGeneration, sessionIdentity: Gameplay.SessionIdentity));
            Debug.DrawLine(transform.position + Vector3.up, target.TargetTransform.position + Vector3.up, Color.yellow, 0.4f);
#if UNITY_EDITOR && DEFENSE_TOWER_DEBUG
            Debug.Log("[TREBRAVE Defense3D] Tower hit " + target.TargetTransform.name + " for " + EffectiveDamage);
#endif
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, EffectiveRange);
        }
    }
}

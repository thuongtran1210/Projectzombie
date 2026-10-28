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
        [SerializeField] private DefenseTowerProjectileVfx3D _projectileVfx;
        [SerializeField] private MonoBehaviour _customLauncher;
        private IDefenseProjectileLauncher _launcher;
        private DefenseAttackTargetSelector3D _targetSelector;
        private float nextShot;
        private DefenseSupportTarget3D _health;
        [SerializeField] private float _damagePerLevel = 10f;
        [SerializeField] private float _rangePerLevel = 1f;
        [SerializeField] private float _fireRatePerLevel = .3f;
        public int Level { get; private set; } = 1;
        public int UpgradeCost => Level < 3 ? Level * 40 : -1;
        public float EffectiveDamage => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerDamage, damage + (Level - 1) * _damagePerLevel) : damage;
        public float EffectiveRange => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerAttackRange, attackRange + (Level - 1) * _rangePerLevel) : attackRange;

        protected override void OnGameplayBound()
        {
            _targetSelector = new DefenseAttackTargetSelector3D(transform, _targeting);
            _launcher = _customLauncher as IDefenseProjectileLauncher ?? _projectileVfx;
            if (_health != null) _health.Defeated -= OnDefeated;
            _health = GetComponent<DefenseSupportTarget3D>();
            if (_health != null) _health.Defeated += OnDefeated;
        }

        public void PrepareForBuild() { Level = 1; nextShot = 0f; _health?.ResetForRecruit(); }
        public bool TryUpgrade(DefenseWoodWallet3D wallet)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || (_health != null && _health.IsDead) || UpgradeCost < 0 || wallet == null) return false;
            if (!wallet.TrySpend(UpgradeCost)) return false;
            Level++;
            _health?.IncreaseCapacity(100f);
            return true;
        }
        private void OnDefeated() => gameObject.SetActive(false);

        protected override void OnGameplayReset() { nextShot = 0f; Level = 1; }

        private void Update()
        {
            if (!CanAdvanceGameplay || (_health != null && _health.IsDead) || GameplayTime < nextShot) return;
            var target = _targetSelector?.Find(Gameplay.Targets, EffectiveRange);
            if (target == null) return;
            nextShot = GameplayTime + fireInterval / (1f + (Level - 1) * _fireRatePerLevel);

            var request = new DefenseProjectileLaunchRequest(
                source: this,
                origin: transform.position + Vector3.up * 1.65f,
                target: target,
                damage: EffectiveDamage,
                runGeneration: Gameplay.RunGeneration,
                sessionIdentity: Gameplay.SessionIdentity);

            bool launched = _launcher != null && _launcher.TryLaunch(request);
            if (!launched)
            {
                target.TakeDamage(new DefenseDamageContext(EffectiveDamage, this, target.TargetTransform.position,
                    runGeneration: Gameplay.RunGeneration, sessionIdentity: Gameplay.SessionIdentity));
            }
#if UNITY_EDITOR && DEFENSE_TOWER_DEBUG
            Debug.Log("[TREBRAVE Defense3D] Tower hit " + target.TargetTransform.name + " for " + EffectiveDamage);
#endif
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, EffectiveRange);
        }

        protected override void OnDestroy() { if (_health != null) _health.Defeated -= OnDefeated; base.OnDestroy(); }
    }
}

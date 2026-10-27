using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Identity and health authority. No navigation, attack or reward dependencies.</summary>
    [DisallowMultipleComponent]
    public sealed class DefenseEnemy3D : DefenseGameplayBehaviour, IDefenseEnemyTarget3D, IDefenseStatusReceiver
    {
        [SerializeField] private DefenseEnemyDataSO definition;
        private DefenseHealthCore health;
        private DefenseTargetIndex _targetIndex;
        private Action<DefenseEnemy3D,DefenseActorLifetime,DefenseActorReleaseReason> _release;
        public DefenseEnemyStatus3D Status { get; private set; }
        public IDefenseLocomotionStrategy3D Locomotion { get; private set; }
        public IDefenseAttackStrategy3D Attack { get; private set; }
        public IDefenseTargetingStrategy3D Targeting { get; private set; }

        public void EnsureStrategies()
        {
            if (Locomotion == null) Locomotion = GetComponent<IDefenseLocomotionStrategy3D>();
            if (Attack == null) Attack = GetComponent<IDefenseAttackStrategy3D>();
            if (Targeting == null) Targeting = GetComponent<IDefenseTargetingStrategy3D>();
        }

        public void StopAllActions()
        {
            Locomotion?.Stop();
            Attack?.StopAttack();
            Targeting?.Stop();
        }

        protected override void OnGameplayBound()
        {
            EnsureStrategies();
            if (Status == null) Status = GetComponent<DefenseEnemyStatus3D>();
            Status?.BindGameplay(Gameplay);
        }
        public bool TryApplyStatus(DefenseStatusRequest request) => IsTargetable && Status != null && Status.TryApplyStatus(request);
        public DefenseActorLifetime Lifetime { get; private set; }
        public bool HasCurrentLifetime => Lifetime == null || Lifetime.IsCurrent(Gameplay);

        public void BindLifetime(DefenseActorLifetime lifetime,
            Action<DefenseEnemy3D,DefenseActorLifetime,DefenseActorReleaseReason> release)
        {
            if (lifetime == null || !lifetime.IsCurrent(Gameplay)) throw new ArgumentException("Actor lifetime does not belong to its current session.");
            if (Lifetime != null && !Lifetime.IsRetired) throw new InvalidOperationException("Retire the previous actor lifetime before rebinding.");
            Lifetime = lifetime;
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        public bool RequestRelease(DefenseActorReleaseReason reason)
        {
            if (Lifetime == null || _release == null) return false;
            _release(this,Lifetime,reason);
            return true;
        }
        public DefenseEnemyDataSO Definition => definition;
        public DefenseEnemyStats Stats { get; private set; }
        public Transform TargetTransform => transform;
        public bool IsTargetable => isActiveAndEnabled && CanAdvanceGameplay && HasCurrentLifetime && !IsDead;
        internal DefenseHealthCore HealthAuthority => Health;
        public float CurrentHP => Health.CurrentHp;
        public float MaxHP => Health.MaxHp;
        public bool IsDead => Health.IsDead;
        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        public event Action Defeated;
        public event Action Initialized;
        public event Action Removed;

        private DefenseHealthCore Health
        {
            get
            {
                if (health == null)
                {
                    Stats = definition != null ? definition.CreateStats() : DefenseEnemyRegistry.GetStats(DefenseEnemyRegistry.ID_MAGIAP);
                    CreateHealth(Stats.MaxHp);
                }
                return health;
            }
        }

        /// <summary>Factory assigns the index after initializing the spawn's immutable stats.</summary>
        public void BindTargets(DefenseTargetIndex index)
        {
            DefenseEnemyTargets3D.Unregister(_targetIndex, this);
            _targetIndex = index ?? throw new ArgumentNullException(nameof(index));
            if (isActiveAndEnabled) RegisterTarget();
        }

        private void RegisterTarget()
        {
            if (_targetIndex == null) return;
            _ = Health;
            DefenseEnemyTargets3D.Register(_targetIndex, this, Stats.Locomotion == DefenseEnemyLocomotionType.AirFlight);
        }

        private void OnEnable() => RegisterTarget();
        private void OnDisable()
        {
            StopAllActions();
            DefenseEnemyTargets3D.Unregister(_targetIndex, this);
            if (Lifetime != null && !Lifetime.IsRetired) RequestRelease(DefenseActorReleaseReason.ExternalDisabled);
        }
        protected override void OnDestroy()
        {
            DefenseEnemyTargets3D.Unregister(_targetIndex, this);
            try { Removed?.Invoke(); }
            finally { base.OnDestroy(); }
        }

        public void Initialize(DefenseEnemyDataSO source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            definition = source;
            Initialize(source.CreateStats());
        }

        public void Initialize(DefenseEnemyStats stats)
        {
            EnsureStrategies();
            Stats = stats;
            if (health == null) CreateHealth(stats.MaxHp);
            health.Reset(stats.MaxHp);
            Initialized?.Invoke();
        }

        public void Initialize(DefenseEnemySpawnDefinition snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            definition = snapshot.Source;
            Initialize(snapshot.Stats);
        }

        private void CreateHealth(float maximum)
        {
            health = new DefenseHealthCore(maximum);
            health.HealthChanged += (current, max) => HealthChanged?.Invoke(current, max);
            health.Damaged += context => Damaged?.Invoke(context);
            health.Defeated += () =>
            {
                StopAllActions();
                Defeated?.Invoke();
            };
        }

        public void SetMaxHealth(float hp) => Health.Reset(Mathf.Max(1f, hp));
        public bool TakeDamage(DefenseDamageContext context) => isActiveAndEnabled && HasCurrentLifetime &&
            DefenseDamageRules.CanApply(context, Gameplay, IsDead) && Health.ApplyDamage(context);
    }
}

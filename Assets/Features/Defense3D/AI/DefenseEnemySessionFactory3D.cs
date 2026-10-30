using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Sở hữu vòng đời actor; trả về đúng pool bằng token của lượt spawn.</summary>
    public sealed class DefenseEnemySessionFactory3D : IDisposable
    {
        private sealed class Entry
        {
            public DefenseActorLifetime Lifetime;
            public Action Removed;
            public bool Releasing;
            public bool Notified;
            public IDisposable CombatText;
            public IDisposable AttackFeedback;
            public UI.DefenseEnemyHealthPresenter3D HealthBar;
            public PoolLease<GameObject> PoolLease;
        }
        private readonly IDefenseGameplayContext _gameplay;
        private readonly DefenseRuntimeRoots3D _roots;
        private readonly DefenseWoodWallet3D _wallet;
        private readonly DefenseTargetIndex _targets;
        private readonly Dictionary<DefenseEnemy3D,Entry> _entries = new Dictionary<DefenseEnemy3D,Entry>();
        private readonly Action<DefenseEnemy3D,DefenseActorLifetime,DefenseActorReleaseReason> _release;
        private readonly ProjectZombie.Features.UI.DamageText.ICombatTextService _combatText;
        private readonly IDefenseEnemyPool _pool;
        private readonly IDefenseWoodDrops _woodDrops;
        private readonly IDefenseEffectService _effects;
        private readonly DefenseBasicAttackEffectSetSO _enemyAttackStyle;
        private long _nextLease;
        private bool _cleaning, _disposed;
        private readonly Camera _presentationCamera;
        public event Action<DefenseEnemy3D,DefenseActorLifetime> ActorReleased;
        public int OwnedCount => _entries.Count;
        public bool BelongsTo(IDefenseGameplayContext gameplay) => ReferenceEquals(_gameplay,gameplay);

        public DefenseEnemySessionFactory3D(IDefenseGameplayContext gameplay, DefenseRuntimeRoots3D roots,
            DefenseWoodWallet3D wallet, DefenseTargetIndex targets, ProjectZombie.Features.UI.DamageText.ICombatTextService combatText = null, IDefenseEnemyPool pool = null, IDefenseWoodDrops woodDrops = null, IDefenseEffectService effects = null, DefenseBasicAttackEffectSetSO enemyAttackStyle = null)
        {
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _roots = roots != null ? roots : throw new ArgumentNullException(nameof(roots));
            _wallet = wallet != null ? wallet : throw new ArgumentNullException(nameof(wallet));
            _targets = targets ?? throw new ArgumentNullException(nameof(targets));
            if (!ReferenceEquals(gameplay.Targets,targets)) throw new ArgumentException("Factory target index belongs to another session.");
            roots.ValidateBindings();
            _release = Release;
            _combatText = combatText;
            _pool = pool; _woodDrops = woodDrops; _effects = effects; _enemyAttackStyle = enemyAttackStyle;
            _presentationCamera = Camera.main;
        }

        public DefenseEnemy3D Spawn(DefenseEnemySpawnDefinition definition, Vector3 position, int lane,
            DefenseBasePrototype3D target, bool arenaBoss)
        {
            return SpawnInternal(definition, position, lane, target, arenaBoss, false);
        }

        /// <summary>Only a completed landing request may acquire this actor; geometry is rechecked at acquisition.</summary>
        public DefenseEnemy3D SpawnLanded(DefenseEnemySpawnDefinition definition, Vector3 position, DefenseBasePrototype3D target)
        {
            if (definition == null || definition.Stats.Locomotion != DefenseEnemyLocomotionType.ParatrooperLanding || target == null ||
                !_roots.CannonBattery.TryResolveLanding(position, target.transform.position, out var approved))
                throw new InvalidOperationException("Landing requires a valid West ground point and route to Base.");
            return SpawnInternal(definition, approved, 1, target, false, true);
        }

        private DefenseEnemy3D SpawnInternal(DefenseEnemySpawnDefinition definition, Vector3 position, int lane,
            DefenseBasePrototype3D target, bool arenaBoss, bool landed)
        {
            if (_disposed || _cleaning || !_gameplay.AdvancesGameplay || _roots == null ||
                !_roots.Actors.gameObject.activeInHierarchy || _roots.Staging.gameObject.activeInHierarchy ||
                !_roots.Pickups.gameObject.activeInHierarchy) throw new InvalidOperationException("Enemy acquisition gate or authored runtime root is closed.");
            var lifetime = new DefenseActorLifetime(_gameplay.SessionIdentity,_gameplay.RunGeneration,checked(++_nextLease));
            return DefenseEnemyFactory3D.SpawnOwnedSnapshot(definition,position,lane,target,_gameplay,_wallet,_targets,_roots,
                actor => Register(actor,lifetime),arenaBoss,landed,_pool,_woodDrops);
        }

        private void Register(DefenseEnemy3D actor, DefenseActorLifetime lifetime)
        {
            var entry = new Entry { Lifetime=lifetime };
            if (_pool != null) _pool.TryGetLease(actor.gameObject, out entry.PoolLease);
            entry.Removed = () => Release(actor,lifetime,DefenseActorReleaseReason.ExternalRemoved);
            _entries.Add(actor,entry);
            actor.Removed += entry.Removed;
            actor.BindLifetime(lifetime,_release);
            if (_effects != null && _enemyAttackStyle != null && actor.TryGetComponent<DefenseEnemyAttack3D>(out var attack))
                entry.AttackFeedback = new DefenseBasicAttackFeedbackAdapter(attack.Feedback, _gameplay, _effects, _enemyAttackStyle, actor,
                    () => actor != null && actor.isActiveAndEnabled && ReferenceEquals(actor.Lifetime, lifetime) && lifetime.IsCurrent(_gameplay));
            actor.TryGetComponent<DefenseActorAnchors3D>(out var anchors);
            if (actor.TryGetComponent<UI.DefenseEnemyHealthPresenter3D>(out var healthBar))
            {
                entry.HealthBar = healthBar;
                actor.TryGetComponent<Collider>(out var body);
                healthBar.Bind(actor, _gameplay, lifetime, anchors != null ? anchors.Head : null, _presentationCamera, body);
            }
            if (_combatText != null) entry.CombatText = new UI.DefenseCombatTextAdapter(actor, _gameplay, _combatText,
                () => anchors != null && anchors.Head != null ? anchors.Head.position : actor.transform.position + Vector3.up, ProjectZombie.Features.UI.DamageText.CombatTextTarget.Enemy);
        }

        public void Release(DefenseEnemy3D actor, DefenseActorLifetime lifetime, DefenseActorReleaseReason reason)
        {
            if (ReferenceEquals(actor,null) || !_entries.TryGetValue(actor,out var entry) ||
                !ReferenceEquals(entry.Lifetime,lifetime) || entry.Releasing) return;
            if (!lifetime.IsRetired && !lifetime.TryRetire(_gameplay.SessionIdentity,lifetime.RunGeneration,reason)) return;
            entry.Releasing = true;
            try
            {
                _effects?.StopOwner(actor);
                entry.CombatText?.Dispose();
                entry.AttackFeedback?.Dispose();
                if (entry.HealthBar != null) entry.HealthBar.Unbind();
                actor.Removed -= entry.Removed;
                if (actor != null && reason != DefenseActorReleaseReason.ExternalRemoved)
                {
                    try { actor.gameObject.SetActive(false); }
                    finally
                    {
                        if (_pool == null || !_pool.TryRelease(entry.PoolLease))
                        {
                            UnityEngine.Object.Destroy(actor.gameObject);
                        }
                    }
                }
                if (reason == DefenseActorReleaseReason.ExternalRemoved) _pool?.TryRelease(entry.PoolLease);
                _entries.Remove(actor);
            }
            finally
            {
                entry.Releasing = false;
                if (!entry.Notified)
                {
                    entry.Notified = true;
                    ActorReleased?.Invoke(actor,lifetime);
                }
            }
        }

        public void Cleanup()
        {
            if (_disposed || _cleaning) return;
            _cleaning = true;
            List<Exception> failures = null;
            try
            {
                var snapshot = new List<KeyValuePair<DefenseEnemy3D,Entry>>(_entries);
                foreach (var entry in snapshot)
                {
                    try { Release(entry.Key,entry.Value.Lifetime,DefenseActorReleaseReason.Cleanup); }
                    catch (Exception exception)
                    {
                        if (failures == null) failures = new List<Exception>();
                        failures.Add(exception);
                    }
                }
            }
            finally { _cleaning = false; }
            if (failures != null) throw new AggregateException("Some owned actors could not be released.",failures);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Cleanup();
            _disposed = true;
            ActorReleased = null;
        }
    }
}

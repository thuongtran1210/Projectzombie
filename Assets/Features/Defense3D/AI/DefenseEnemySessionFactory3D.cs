using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One owner for actor creation, lifetime identity and release. Pool reuse is not implemented here.</summary>
    public sealed class DefenseEnemySessionFactory3D : IDisposable
    {
        private sealed class Entry
        {
            public DefenseActorLifetime Lifetime;
            public Action Removed;
            public bool Releasing;
            public bool Notified;
        }
        private readonly IDefenseGameplayContext _gameplay;
        private readonly DefenseRuntimeRoots3D _roots;
        private readonly DefenseWoodWallet3D _wallet;
        private readonly DefenseTargetIndex _targets;
        private readonly Dictionary<DefenseEnemy3D,Entry> _entries = new Dictionary<DefenseEnemy3D,Entry>();
        private readonly Action<DefenseEnemy3D,DefenseActorLifetime,DefenseActorReleaseReason> _release;
        private long _nextLease;
        private bool _cleaning, _disposed;
        public event Action<DefenseEnemy3D,DefenseActorLifetime> ActorReleased;
        public int OwnedCount => _entries.Count;
        public bool BelongsTo(IDefenseGameplayContext gameplay) => ReferenceEquals(_gameplay,gameplay);

        public DefenseEnemySessionFactory3D(IDefenseGameplayContext gameplay, DefenseRuntimeRoots3D roots,
            DefenseWoodWallet3D wallet, DefenseTargetIndex targets)
        {
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _roots = roots != null ? roots : throw new ArgumentNullException(nameof(roots));
            _wallet = wallet != null ? wallet : throw new ArgumentNullException(nameof(wallet));
            _targets = targets ?? throw new ArgumentNullException(nameof(targets));
            if (!ReferenceEquals(gameplay.Targets,targets)) throw new ArgumentException("Factory target index belongs to another session.");
            roots.ValidateBindings();
            _release = Release;
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
                actor => Register(actor,lifetime),arenaBoss,landed);
        }

        private void Register(DefenseEnemy3D actor, DefenseActorLifetime lifetime)
        {
            var entry = new Entry { Lifetime=lifetime };
            entry.Removed = () => Release(actor,lifetime,DefenseActorReleaseReason.ExternalRemoved);
            _entries.Add(actor,entry);
            actor.Removed += entry.Removed;
            actor.BindLifetime(lifetime,_release);
        }

        public void Release(DefenseEnemy3D actor, DefenseActorLifetime lifetime, DefenseActorReleaseReason reason)
        {
            if (ReferenceEquals(actor,null) || !_entries.TryGetValue(actor,out var entry) ||
                !ReferenceEquals(entry.Lifetime,lifetime) || entry.Releasing) return;
            if (!lifetime.IsRetired && !lifetime.TryRetire(_gameplay.SessionIdentity,lifetime.RunGeneration,reason)) return;
            entry.Releasing = true;
            try
            {
                actor.Removed -= entry.Removed;
                if (actor != null && reason != DefenseActorReleaseReason.ExternalRemoved)
                {
                    try { actor.gameObject.SetActive(false); }
                    finally
                    {
                        if (DefenseEnemyPoolManager.ActiveSessionPool == null ||
                            !DefenseEnemyPoolManager.ActiveSessionPool.TryRelease(actor.gameObject))
                        {
                            UnityEngine.Object.Destroy(actor.gameObject);
                        }
                    }
                }
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

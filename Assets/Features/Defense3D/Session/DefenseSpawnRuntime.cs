using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Owns schedule/planner bookkeeping; the session factory owns physical actor lifetimes.</summary>
    public sealed class DefenseSpawnRuntime : IDisposable
    {
        private const float FACTORY_RETRY_DELAY_SECONDS = 1f;
        private readonly DefenseStageSpawnPlan _plan;
        private readonly DefenseSpawnPlanner _planner;
        private readonly DefenseStageSpawnScheduler _scheduler;
        private readonly DefenseSpawnPlacement3D _placement;
        private readonly IDefenseGameplayContext _gameplay;
        private readonly DefenseBasePrototype3D _base;
        private readonly DefenseEnemySessionFactory3D _factory;
        private readonly DefenseYakshaEncounter3D _encounter;
        private readonly Dictionary<DefenseEnemy3D, DefenseActorLifetime> _actors = new Dictionary<DefenseEnemy3D, DefenseActorLifetime>();
        private readonly HashSet<string> _reportedFailures = new HashSet<string>();
        private readonly Dictionary<(string EventId, int Slot), float> _retryAt = new Dictionary<(string, int), float>();
        private readonly Action<DefenseSpawnJob> _enqueue;
        private readonly Func<DefenseSpawnJob, bool> _canSpawn;
        private readonly Func<DefenseSpawnJob, bool> _trySpawn;
        private DefenseSpawnLocation _approvedLocation;
        private readonly DefenseCannonBattery3D _battery;
        private readonly HashSet<string> _acceptedLandings = new HashSet<string>();
        private readonly Dictionary<string, Vector3> _landings = new Dictionary<string, Vector3>();
        private bool _cleaning;
        private bool _disposed;

        public int ProcessedEvents => _scheduler.ProcessedEvents;
        public int TotalEvents => _plan.Events.Count;

        public DefenseSpawnRuntime(DefenseStageSpawnPlan plan, DefenseSpawnPlacement3D placement,
            IDefenseGameplayContext gameplay, DefenseBasePrototype3D defenseBase,
            DefenseWoodWallet3D wallet, DefenseYakshaEncounter3D encounter, DefenseTargetIndex targets,
            DefenseEnemySessionFactory3D factory, DefenseCannonBattery3D battery = null)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
            _placement = placement ?? throw new ArgumentNullException(nameof(placement));
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            if (targets == null || !ReferenceEquals(gameplay.Targets,targets)) throw new ArgumentException("Spawn targets must belong to this session.");
            _base = defenseBase != null ? defenseBase : throw new ArgumentNullException(nameof(defenseBase));
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            if (!factory.BelongsTo(gameplay)) throw new ArgumentException("Spawn factory must belong to this session.");
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _battery = battery;
            _planner = plan.CreatePlanner();
            _scheduler = new DefenseStageSpawnScheduler(plan.Events);
            _enqueue = job => _planner.EnqueueJob(job, _gameplay.ElapsedSeconds);
            _canSpawn = CanSpawn;
            _trySpawn = TrySpawn;
            _planner.JobExpired += OnExpired;
            _planner.JobOverflowed += OnOverflowed;
            _factory.ActorReleased += OnActorReleased;
            Debug.Log("[TREBRAVE Defense3D] Spawn profile=" + _plan.ProfileVersion + " activeLimit=" +
                _planner.ActiveLimit + " pendingLimit=" + _planner.PendingLimit);
        }

        public void Tick()
        {
            if (_disposed || !_gameplay.AdvancesGameplay) return;
            _planner.ExpirePending(_gameplay.ElapsedSeconds);
            _battery?.Tick();
            _scheduler.EnqueueDue(_gameplay.ElapsedSeconds, _enqueue);
            _planner.ProcessSpawns(_gameplay.ElapsedSeconds, _canSpawn, _trySpawn);
        }

        /// <summary>Additional source jobs share the existing pending/active/frame budget; never edit Stage events.</summary>
        public bool EnqueueLanding(string cannonId, int shot, Vector3 position, int generation)
        {
            if (_disposed || _cleaning || !_gameplay.AdvancesGameplay || generation != _gameplay.RunGeneration ||
                _battery == null || _battery.LandingEnemy == null || string.IsNullOrWhiteSpace(cannonId) || shot <= 0 ||
                !new DefensePoint3(position.x, position.y, position.z).IsFinite) return false;
            string id = "landing:" + cannonId + ":" + shot;
            if (!_acceptedLandings.Add(id)) return false;
            _landings.Add(id, position);
            var job = new DefenseSpawnJob(id, 0, _battery.LandingEnemy.EnemyId, _battery.LandingEnemy.EnemyId,
                "cannon-west-landing", _gameplay.ElapsedSeconds, DefenseSpawnKind.Landing);
            return _planner.EnqueueJob(job, _gameplay.ElapsedSeconds);
        }

        private bool CanSpawn(DefenseSpawnJob job)
        {
            if (_retryAt.TryGetValue((job.EventId, job.SlotIndex), out float retryAt) &&
                _gameplay.ElapsedSeconds < retryAt) return false;
            if (job.Kind == DefenseSpawnKind.Landing)
            {
                if (_battery == null || !_landings.TryGetValue(job.EventId, out var requested) ||
                    !_battery.TryResolveLanding(requested, _base.transform.position, out var ground)) return false;
                // Landing retains the same XZ Hero exclusion as other spawn domains.
                return _placement.TryResolveLanding(ground, out _approvedLocation);
            }
            return _placement.TryResolve(job, out _approvedLocation);
        }

        private bool TrySpawn(DefenseSpawnJob job)
        {
            DefenseEnemy3D actor = null;
            bool startedEncounter = false;
            try
            {
                var definition = job.Kind == DefenseSpawnKind.Landing ? _battery.LandingEnemy : _plan.GetEnemySnapshot(job.ActualSpawnType);
                if (job.Kind == DefenseSpawnKind.Landing)
                {
                    actor = _factory.SpawnLanded(definition, _approvedLocation.Position, _base);
                }
                else if (job.Kind == DefenseSpawnKind.ArenaBoss)
                {
                    actor = _factory.Spawn(definition,_approvedLocation.Position,_approvedLocation.Lane,_base,true);
                    _encounter.Begin(actor.GetComponent<DefenseEnemyBridgeWalker3D>(), actor.GetComponent<DefenseYakshaArenaCombat3D>());
                    startedEncounter = true;
                }
                else
                {
                    actor = _factory.Spawn(definition,_approvedLocation.Position,_approvedLocation.Lane,_base,false);
                }
                actor.name = $"{job.ActualSpawnType}_{job.EventId}_Slot{job.SlotIndex + 1}";
                Track(actor);
                _landings.Remove(job.EventId);
                _retryAt.Remove((job.EventId, job.SlotIndex));
                return true;
            }
            catch (Exception exception)
            {
                if (startedEncounter) _encounter.Reset();
                if (actor != null)
                {
                    _factory.Release(actor,actor.Lifetime,DefenseActorReleaseReason.SpawnFailed);
                }
                string key = job.EventId + "/" + job.SlotIndex;
                _retryAt[(job.EventId, job.SlotIndex)] = _gameplay.ElapsedSeconds + FACTORY_RETRY_DELAY_SECONDS;
                if (_reportedFailures.Add(key))
                    Debug.LogError("[TREBRAVE Defense3D] Spawn deferred after factory failure: " + key + ": " + exception.Message);
                return false;
            }
        }

        private void Track(DefenseEnemy3D actor)
        {
            if (actor.Lifetime == null || !actor.Lifetime.IsCurrent(_gameplay))
                throw new InvalidOperationException("Spawned actor requires a live session-owned lifetime.");
            _actors.Add(actor,actor.Lifetime);
        }

        private void OnActorReleased(DefenseEnemy3D actor, DefenseActorLifetime lifetime)
        {
            if (!_actors.TryGetValue(actor,out var tracked) || !ReferenceEquals(tracked,lifetime)) return;
            _actors.Remove(actor);
            if (!_disposed && !_cleaning && lifetime.RunGeneration == _gameplay.RunGeneration &&
                lifetime.ReleaseReason != DefenseActorReleaseReason.Cleanup && lifetime.ReleaseReason != DefenseActorReleaseReason.SpawnFailed)
                _planner.NotifyEnemyDefeatedOrRemoved();
        }

        /// <summary>Unsubscribes before deferred destruction; callbacks cannot release a new run's slots.</summary>
        public void Cleanup()
        {
            if (_disposed) return;
            _cleaning = true;
            try
            {
                try { _encounter.Reset(); }
                finally { _factory.Cleanup(); }
            }
            finally
            {
                _actors.Clear();
                _landings.Clear();
                _acceptedLandings.Clear();
                _planner.Reset();
                _scheduler.Reset();
                _reportedFailures.Clear();
                _retryAt.Clear();
                _cleaning = false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            try { Cleanup(); }
            finally
            {
                try { _factory.Dispose(); }
                finally
                {
                    _disposed = true;
                    _factory.ActorReleased -= OnActorReleased;
                    _planner.JobExpired -= OnExpired;
                    _planner.JobOverflowed -= OnOverflowed;
                }
            }
        }

        private void OnExpired(DefenseSpawnJob job)
        {
            _landings.Remove(job.EventId);
            Debug.LogWarning("[TREBRAVE Defense3D] Spawn expired: " + job.EventId + "/" + job.SlotIndex + " zone=" + job.Zone);
        }
        private void OnOverflowed(DefenseSpawnJob job)
        {
            _landings.Remove(job.EventId);
            Debug.LogWarning("[TREBRAVE Defense3D] Spawn queue overflow: " +
            job.EventId + "/" + job.SlotIndex + " profile=" + _plan.ProfileVersion + " t=" + _gameplay.ElapsedSeconds.ToString("F1") +
            " active=" + _planner.CurrentActiveCount + "/" + _planner.ActiveLimit + " owned=" + _factory.OwnedCount +
            " pending=" + _planner.PendingCount + "/" + _planner.PendingLimit + " predicateBlockedLastBatch=" + _planner.LastPredicateBlocked +
            " factoryFailedLastBatch=" + _planner.LastFactoryFailed + " expired=" + _planner.ExpiredJobCount +
            " overflowTotal=" + _planner.OverflowJobCount);
        }
    }
}

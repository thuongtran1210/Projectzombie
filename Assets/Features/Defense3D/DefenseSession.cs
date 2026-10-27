using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Owns the existing lifecycle adapter and exposes a shared gameplay gate.</summary>
    public sealed class DefenseSession : IDefenseGameplayContext, IDefenseSessionCommands, IDisposable
    {
        private bool _disposed;
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        public DefenseG1RuntimeAdapter RuntimeAdapter { get; }
        public DefenseTargetIndex TargetRegistry { get; } = new DefenseTargetIndex();
        public DefenseRunStats Modifiers { get; } = new DefenseRunStats();
        public object SessionIdentity => this;
        public IDefenseTargetQuery Targets => TargetRegistry;
        public IDefenseStatModifiers Stats => Modifiers;
        public bool IsDisposed => _disposed;
        public DefenseGamePhase Phase => RuntimeAdapter.Run.State.Phase;
        public bool IsTerminal => RuntimeAdapter.Run.State.IsTerminal;
        public bool IsOvertime => Phase == DefenseGamePhase.Overtime ||
            ((Phase == DefenseGamePhase.Paused || Phase == DefenseGamePhase.LevelUp) &&
                RuntimeAdapter.Run.State.ResumePhase == DefenseGamePhase.Overtime);
        public bool AdvancesGameplay => !_disposed && RuntimeAdapter.Run.State.AdvancesGameplay;
        public float ElapsedSeconds => RuntimeAdapter.Run.Clock.ElapsedSeconds;
        public int RunGeneration => RuntimeAdapter.Run.RunGeneration;
        public event Action<bool> GameplayGateChanged;
        public event Action RunCleanup;
        public event Action RunReset;

        public DefenseSession(float durationSeconds = DefenseGameplayClock.StandardDurationSeconds)
        {
            RuntimeAdapter = new DefenseG1RuntimeAdapter(durationSeconds);
            RuntimeAdapter.Run.State.Changed += OnPhaseChanged;
            RuntimeAdapter.RegisterOwner(CleanupActorsAndTargets, Modifiers.Reset);
            Own(TargetRegistry);
        }

        /// <summary>Owns a service until shutdown; restarting only invokes its registered round cleanup/reset.</summary>
        public void Own(IDisposable resource)
        {
            ThrowIfDisposed();
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (_resources.Contains(resource)) throw new InvalidOperationException("Service is already owned by this session.");
            _resources.Add(resource);
        }

        /// <summary>Starts only after the composition root has bound all required actors.</summary>
        public bool Start()
        {
            ThrowIfDisposed();
            return RuntimeAdapter.Start();
        }

        /// <summary>Advances the sole gameplay clock and resolves authoritative match results.</summary>
        public float Tick(float deltaSeconds, float baseHp, bool bossDefeated)
        {
            ThrowIfDisposed();
            return RuntimeAdapter.Tick(deltaSeconds, baseHp, bossDefeated);
        }

        public bool Pause()
        {
            ThrowIfDisposed();
            return RuntimeAdapter.Run.Pause();
        }

        public bool OpenLevelUp()
        {
            ThrowIfDisposed();
            return RuntimeAdapter.Run.OpenLevelUp();
        }

        public bool Resume()
        {
            ThrowIfDisposed();
            return RuntimeAdapter.Run.Resume();
        }

        /// <summary>Closes the gate before owner cleanup; old actors cannot keep attacking during reset.</summary>
        public void Restart()
        {
            ThrowIfDisposed();
            if (AdvancesGameplay) RuntimeAdapter.Run.Pause();
            RuntimeAdapter.Restart();
            RunReset?.Invoke();
            RuntimeAdapter.Start();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            RuntimeAdapter.Run.State.Changed -= OnPhaseChanged;
            var failures = new List<Exception>();
            try { NotifyGate(false); }
            catch (Exception exception) { failures.Add(exception); }
            try { RuntimeAdapter.CleanupOwners(); }
            catch (Exception exception) { failures.Add(exception); }
            for (int i = _resources.Count - 1; i >= 0; i--)
            {
                try { _resources[i].Dispose(); }
                catch (Exception exception) { failures.Add(exception); }
            }
            _resources.Clear();
            RuntimeAdapter.Dispose();
            GameplayGateChanged = null;
            RunCleanup = null;
            RunReset = null;
            if (failures.Count > 0) throw new AggregateException("Defense session shutdown failed.", failures);
        }

        private void NotifyCleanupObservers()
        {
            if (RunCleanup == null) return;
            List<Exception> failures = null;
            foreach (Action observer in RunCleanup.GetInvocationList())
            {
                try { observer(); }
                catch (Exception exception)
                {
                    if (failures == null) failures = new List<Exception>();
                    failures.Add(exception);
                }
            }
            if (failures != null) throw new AggregateException("Defense actor cleanup failed.", failures);
        }

        private void CleanupActorsAndTargets()
        {
            try { NotifyCleanupObservers(); }
            finally { TargetRegistry.Clear(); }
        }

        private void OnPhaseChanged(DefenseGamePhase previous, DefenseGamePhase current)
        {
            bool wasActive = IsActive(previous);
            bool isActive = IsActive(current);
            if (wasActive != isActive) NotifyGate(isActive);
        }

        private void NotifyGate(bool active)
        {
            if (GameplayGateChanged == null) return;
            List<Exception> failures = null;
            foreach (Action<bool> handler in GameplayGateChanged.GetInvocationList())
            {
                try { handler(active); }
                catch (Exception exception)
                {
                    if (failures == null) failures = new List<Exception>();
                    failures.Add(exception);
                }
            }
            if (failures != null) throw new AggregateException("Defense gameplay gate notification failed.", failures);
        }

        private static bool IsActive(DefenseGamePhase phase)
        {
            return phase == DefenseGamePhase.Playing || phase == DefenseGamePhase.Overtime;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DefenseSession));
        }
    }
}

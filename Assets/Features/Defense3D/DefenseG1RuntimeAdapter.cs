using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Opt-in bridge contract for the legacy DefenseRun6003D implementation.
    /// This is deliberately NOT a MonoBehaviour and cannot alter scene references.
    /// Integrators must register cleanup/reset actions for every runtime owner.
    /// </summary>
    public sealed class DefenseG1RuntimeAdapter : IDisposable
    {
        private readonly List<Action> cleanup = new List<Action>();
        private readonly List<Action> reset = new List<Action>();
        private bool disposed;

        public DefenseRunLifecycle Run { get; }

        public DefenseG1RuntimeAdapter(float durationSeconds = DefenseGameplayClock.StandardDurationSeconds)
        {
            Run = new DefenseRunLifecycle(durationSeconds);
        }

        /// <summary>
        /// Register a paired owner lifecycle. cleanup must unsubscribe listeners,
        /// destroy run-owned instances and cancel pending jobs; reset restores HP,
        /// wallet, EXP and tower/socket state. Registration itself has no side effects.
        /// </summary>
        public void RegisterOwner(Action cleanupRun, Action resetOwner)
        {
            ThrowIfDisposed();
            if (cleanupRun == null) throw new ArgumentNullException(nameof(cleanupRun));
            if (resetOwner == null) throw new ArgumentNullException(nameof(resetOwner));
            cleanup.Add(cleanupRun);
            reset.Add(resetOwner);
        }

        public bool Start()
        {
            ThrowIfDisposed();
            return Run.Start();
        }

        public float Tick(float deltaSeconds, float baseHp, bool yakshaDefeated)
        {
            ThrowIfDisposed();
            return Run.Tick(deltaSeconds, baseHp, yakshaDefeated);
        }

        /// <summary>
        /// Only restart after all owners are registered. Cleanup runs in reverse
        /// registration order, then resets in registration order. Any exception
        /// stops the restart: core generation must NOT advance on partial cleanup.
        /// </summary>
        public void Restart()
        {
            ThrowIfDisposed();
            for (int i = cleanup.Count - 1; i >= 0; --i)
                cleanup[i]();
            for (int i = 0; i < reset.Count; ++i)
                reset[i]();
            Run.ResetCore();
        }

        public void Dispose()
        {
            if (disposed) return;
            // This adapter doesn't own MonoBehaviour lifetime. Cleanup is explicit,
            // avoiding accidental destruction or event duplication on disposal.
            cleanup.Clear();
            reset.Clear();
            disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(DefenseG1RuntimeAdapter));
        }
    }
}

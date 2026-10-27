using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// G1 U3D-005 pure C# lifecycle coordinator. Does not touch scene objects,
    /// MonoBehaviour instances, static events, or the existing prototype.
    /// Runtime adapters must explicitly reset their own HP, units, slots and pickups.
    /// </summary>
    public sealed class DefenseRunLifecycle
    {
        public DefenseGameState State { get; }
        public DefenseGameplayClock Clock { get; }
        public int RunGeneration { get; private set; }
        public bool IsStarted => State.Phase != DefenseGamePhase.Ready;

        public DefenseRunLifecycle(float durationSeconds = DefenseGameplayClock.StandardDurationSeconds)
        {
            State = new DefenseGameState();
            Clock = new DefenseGameplayClock(durationSeconds);
        }

        public bool Start() => State.TryStart();
        public bool Pause() => State.TryPause();
        public bool OpenLevelUp() => State.TryOpenLevelUp();
        public bool Resume() => State.TryResume();

        /// <summary>Accept gameplay seconds, then resolve the full frame outcome.</summary>
        public float Tick(float deltaSeconds, float baseHp, bool yakshaDefeated)
        {
            // Check loss first, including while paused; same-tick loss beats victory.
            State.Resolve(baseHp, false, Clock.ElapsedSeconds);
            if (State.IsTerminal) return 0f;
            var accepted = Clock.Tick(deltaSeconds, State);
            State.Resolve(baseHp, yakshaDefeated, Clock.ElapsedSeconds);
            return accepted;
        }

        /// <summary>
        /// Explicit restart of core counters only. Caller must dispose/clear scene
        /// subscriptions and spawned objects BEFORE calling this, and separately
        /// reset Base/Hero HP, wallet, EXP, sockets, pending spawns and respawn state.
        /// </summary>
        public void ResetCore()
        {
            State.Reset();
            Clock.Reset();
            RunGeneration++;
        }
    }
}

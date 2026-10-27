using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// G1 U3D-004: deterministic gameplay-time clock. The caller supplies frame delta
    /// and the authoritative state. This class never changes Unity Time.timeScale.
    /// </summary>
    public sealed class DefenseGameplayClock
    {
        public const float StandardDurationSeconds = 600f;
        public float ElapsedSeconds { get; private set; }
        public float DurationSeconds { get; private set; }
        public float RemainingSeconds => Math.Max(0f, DurationSeconds - ElapsedSeconds);
        public bool HasReachedDuration => ElapsedSeconds >= DurationSeconds;
        public event Action<float> Advanced;
        public event Action DurationReached;
        private bool notified;

        public DefenseGameplayClock(float durationSeconds = StandardDurationSeconds)
        {
            if (float.IsNaN(durationSeconds) || float.IsInfinity(durationSeconds) || durationSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            DurationSeconds = durationSeconds;
        }

        /// <summary>
        /// Advance only while Playing or Overtime. Returns actual accepted delta;
        /// systems for AI, cooldowns, spawns, damage and hero respawn should use it,
        /// not Time.deltaTime/Time.time, after the G1 integration step.
        /// </summary>
        public float Tick(float deltaSeconds, DefenseGameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (!state.AdvancesGameplay || deltaSeconds == 0f) return 0f;
            ElapsedSeconds += deltaSeconds;
            Advanced?.Invoke(deltaSeconds);
            if (!notified && HasReachedDuration)
            {
                notified = true;
                DurationReached?.Invoke();
            }
            return deltaSeconds;
        }

        public void Reset()
        {
            ElapsedSeconds = 0f;
            notified = false;
        }
    }
}

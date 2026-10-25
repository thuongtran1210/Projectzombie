using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Pure C# coordinator for hero down & respawn timer according to D-04/A.
    /// Respawn timer only advances via gameplay delta seconds (freezes during Pause/LevelUp).
    /// Terminal states (Win/Loss) cancel pending respawn.
    /// </summary>
    public sealed class DefenseHeroRespawnTracker
    {
        public const float StandardRespawnDelay = 5f;

        public float MaxHp { get; private set; }
        public float CurrentHp { get; private set; }
        public float RespawnDelay { get; }
        public float RemainingRespawnTime { get; private set; }
        public bool IsDown => CurrentHp <= 0f;

        public event Action Downed;
        public event Action Respawned;

        public DefenseHeroRespawnTracker(float maxHp = 200f, float respawnDelay = StandardRespawnDelay)
        {
            if (maxHp <= 0f) throw new ArgumentOutOfRangeException(nameof(maxHp));
            if (respawnDelay <= 0f) throw new ArgumentOutOfRangeException(nameof(respawnDelay));

            MaxHp = maxHp;
            CurrentHp = maxHp;
            RespawnDelay = respawnDelay;
            RemainingRespawnTime = 0f;
        }

        public void TakeDamage(float amount)
        {
            if (IsDown || amount <= 0f) return;
            CurrentHp = Math.Max(0f, CurrentHp - amount);
            if (CurrentHp <= 0f)
            {
                RemainingRespawnTime = RespawnDelay;
                Downed?.Invoke();
            }
        }

        public void Tick(float gameplayDeltaSeconds, bool isTerminal)
        {
            if (!IsDown) return;

            if (isTerminal)
            {
                // Terminal Win/Loss cancels pending respawn permanently
                RemainingRespawnTime = 0f;
                return;
            }

            if (gameplayDeltaSeconds <= 0f) return;

            RemainingRespawnTime = Math.Max(0f, RemainingRespawnTime - gameplayDeltaSeconds);
            if (RemainingRespawnTime <= 0f)
            {
                CurrentHp = MaxHp;
                Respawned?.Invoke();
            }
        }

        public void Reset()
        {
            CurrentHp = MaxHp;
            RemainingRespawnTime = 0f;
        }
    }
}

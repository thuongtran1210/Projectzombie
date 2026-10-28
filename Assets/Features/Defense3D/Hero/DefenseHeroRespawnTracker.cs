using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Hero life/respawn coordinator; shared health core owns all HP and death transitions.</summary>
    public sealed class DefenseHeroRespawnTracker
    {
        public const float StandardRespawnDelay = 5f;
        private readonly DefenseHealthCore _health;
        private bool _respawnCancelled;
        public float MaxHp => _health.MaxHp;
        public float CurrentHp => _health.CurrentHp;
        public float RespawnDelay { get; }
        public float RemainingRespawnTime { get; private set; }
        public bool IsDown => _health.IsDead;
        public event Action Downed;
        public event Action Respawned;
        public event Action<float, float> HealthChanged { add => _health.HealthChanged += value; remove => _health.HealthChanged -= value; }
        public event Action<DefenseDamageContext> Damaged { add => _health.Damaged += value; remove => _health.Damaged -= value; }

        public DefenseHeroRespawnTracker(float maxHp = 200f, float respawnDelay = StandardRespawnDelay)
        {
            if (!DefenseDamageRules.IsFinite(respawnDelay) || respawnDelay <= 0f)
                throw new ArgumentOutOfRangeException(nameof(respawnDelay));
            _health = new DefenseHealthCore(maxHp);
            RespawnDelay = respawnDelay;
            _health.Defeated += () =>
            {
                RemainingRespawnTime = _respawnCancelled ? 0f : RespawnDelay;
                Downed?.Invoke();
            };
        }

        public void TakeDamage(float amount) => ApplyDamage(new DefenseDamageContext(amount));
        public bool ApplyDamage(DefenseDamageContext context) => _health.ApplyDamage(context);
        public bool Heal(float amount) => _health.Heal(amount);

        public void Tick(float gameplayDeltaSeconds, bool isTerminal)
        {
            if (!DefenseDamageRules.IsFinite(gameplayDeltaSeconds) || gameplayDeltaSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(gameplayDeltaSeconds));
            if (isTerminal)
            {
                _respawnCancelled = true;
                RemainingRespawnTime = 0f;
                return;
            }
            if (!IsDown || _respawnCancelled || gameplayDeltaSeconds <= 0f) return;
            RemainingRespawnTime = Math.Max(0f, RemainingRespawnTime - gameplayDeltaSeconds);
            if (RemainingRespawnTime > 0f) return;
            _health.Reset();
            Respawned?.Invoke();
        }

        public void Reset()
        {
            _respawnCancelled = false;
            RemainingRespawnTime = 0f;
            _health.Reset();
        }

        public void SetMaxHp(float newMaxHp)
        {
            if (newMaxHp <= 0f) return;
            _health.Reset(newMaxHp);
        }
    }
}

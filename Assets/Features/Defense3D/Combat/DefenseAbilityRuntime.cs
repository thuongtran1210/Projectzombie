using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseAbilityPhase { Ready, Aiming, Cooldown }

    /// <summary>One cooldown/aim owner per ability, driven exclusively by session gameplay time.</summary>
    public sealed class DefenseAbilityRuntime : IDisposable
    {
        private readonly IDefenseGameplayContext _gameplay;
        private int _generation;
        private float _readyAt;
        private bool _aiming;
        private bool _disposed;

        public DefenseAbilityRuntime(IDefenseGameplayContext gameplay)
        {
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            Reset();
            gameplay.GameplayGateChanged += OnGateChanged;
            gameplay.RunCleanup += CancelAim;
            gameplay.RunReset += Reset;
        }

        public float RemainingCooldown
        {
            get
            {
                SynchronizeGeneration();
                return Math.Max(0f, _readyAt - _gameplay.ElapsedSeconds);
            }
        }

        public DefenseAbilityPhase Phase => RemainingCooldown > 0f ? DefenseAbilityPhase.Cooldown :
            (_aiming ? DefenseAbilityPhase.Aiming : DefenseAbilityPhase.Ready);

        public bool BelongsTo(IDefenseGameplayContext gameplay) => ReferenceEquals(_gameplay, gameplay);

        public bool BeginAim(bool actorAvailable)
        {
            if (_disposed || !_gameplay.AdvancesGameplay || !actorAvailable || Phase != DefenseAbilityPhase.Ready) return false;
            _aiming = true;
            return true;
        }

        public void CancelAim() => _aiming = false;

        public bool TryActivate(float cooldownSeconds, bool actorAvailable)
        {
            if (_disposed || !_gameplay.AdvancesGameplay || !actorAvailable || !DefenseDamageRules.IsFinite(cooldownSeconds) ||
                cooldownSeconds <= 0f || RemainingCooldown > 0f) return false;
            float readyAt = _gameplay.ElapsedSeconds + cooldownSeconds;
            if (!DefenseDamageRules.IsFinite(readyAt)) return false;
            _readyAt = readyAt;
            _aiming = false;
            return true;
        }

        public bool ReduceRemainingCooldownFraction(float fraction)
        {
            if (_disposed || !_gameplay.AdvancesGameplay || !DefenseDamageRules.IsFinite(fraction) || fraction < 0f || fraction > 1f) return false;
            _readyAt = _gameplay.ElapsedSeconds + RemainingCooldown * (1f - fraction);
            return true;
        }

        public void Reset()
        {
            _generation = _gameplay.RunGeneration;
            _readyAt = 0f;
            _aiming = false;
        }

        private void SynchronizeGeneration()
        {
            if (_generation != _gameplay.RunGeneration) Reset();
        }

        private void OnGateChanged(bool active)
        {
            if (!active) CancelAim();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CancelAim();
            _gameplay.GameplayGateChanged -= OnGateChanged;
            _gameplay.RunCleanup -= CancelAim;
            _gameplay.RunReset -= Reset;
        }
    }
}

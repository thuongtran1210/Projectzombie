using System;

namespace ProjectZombie.Features.Defense3D
{
    public interface IDefenseHeroCommands
    {
        DefensePoint3 Movement { get; }
        bool ConsumeBuild();
    }

    /// <summary>Session-scoped frame mailbox. Movement persists; build can be consumed once per submitted frame.</summary>
    public sealed class DefenseHeroCommands : IDefenseHeroCommands, IDisposable
    {
        private readonly IDefenseGameplayContext _gameplay;
        private DefensePoint3 _movement;
        private bool _build;
        private bool _available;
        private bool _disposed;
        private int _generation;

        public DefenseHeroCommands(IDefenseGameplayContext gameplay)
        {
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _generation = gameplay.RunGeneration;
            gameplay.GameplayGateChanged += OnGateChanged;
            gameplay.RunCleanup += Clear;
            gameplay.RunReset += Clear;
        }

        public DefensePoint3 Movement => CanRead ? _movement : default;

        private bool CanRead => !_disposed && _available && _gameplay.AdvancesGameplay &&
            _generation == _gameplay.RunGeneration;

        public void Submit(float horizontal, float vertical, bool build, bool available)
        {
            Clear();
            if (_disposed || !available || !_gameplay.AdvancesGameplay ||
                !DefenseDamageRules.IsFinite(horizontal) || !DefenseDamageRules.IsFinite(vertical)) return;
            double length = Math.Sqrt((double)horizontal * horizontal + (double)vertical * vertical);
            double scale = length > 1d ? 1d / length : 1d;
            _movement = new DefensePoint3((float)(horizontal * scale), 0f, (float)(vertical * scale));
            _generation = _gameplay.RunGeneration;
            _available = true;
            _build = build;
        }

        public bool ConsumeBuild()
        {
            bool requested = CanRead && _build;
            _build = false;
            return requested;
        }

        public void Clear()
        {
            _movement = default;
            _build = false;
            _available = false;
        }

        private void OnGateChanged(bool active)
        {
            if (!active) Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Clear();
            _gameplay.GameplayGateChanged -= OnGateChanged;
            _gameplay.RunCleanup -= Clear;
            _gameplay.RunReset -= Clear;
        }
    }
}

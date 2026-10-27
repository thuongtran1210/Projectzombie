using System;
using ProjectZombie.Features.UI.Controls;

namespace ProjectZombie.Features.Defense3D
{
    public readonly struct DefenseSkillGestureSample
    {
        public readonly SkillPointerSample Pointer;
        public readonly int RunGeneration;
        public readonly object SessionIdentity;

        internal DefenseSkillGestureSample(SkillPointerSample pointer, IDefenseGameplayContext gameplay)
        {
            Pointer = pointer;
            RunGeneration = gameplay.RunGeneration;
            SessionIdentity = gameplay.SessionIdentity;
        }
    }

    /// <summary>Scopes shared pointer gestures to one session and ability; effect execution owns cooldown commit.</summary>
    public sealed class DefenseSkillGestureSession : IDisposable
    {
        private readonly IDefenseGameplayContext _gameplay;
        private readonly DefenseAbilityRuntime _ability;
        private readonly SkillPointerGesture _gesture;
        private bool _available, _ownsAim, _disposed;
        private int _generation;
        public event Action<DefenseSkillGestureSample> Changed;
        public bool IsPressed => _gesture.IsPressed;
        public bool IsDragging => _gesture.IsDragging;

        public DefenseSkillGestureSession(IDefenseGameplayContext gameplay, DefenseAbilityRuntime ability,
            SkillPointerGesture gesture)
        {
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _ability = ability ?? throw new ArgumentNullException(nameof(ability));
            _gesture = gesture ?? throw new ArgumentNullException(nameof(gesture));
            if (!ability.BelongsTo(gameplay)) throw new ArgumentException("Ability belongs to another gameplay context.", nameof(ability));
            _generation = gameplay.RunGeneration;
            gameplay.GameplayGateChanged += OnGateChanged;
            gameplay.RunCleanup += Cancel;
            gameplay.RunReset += OnReset;
        }

        public void SetAvailable(bool available)
        {
            _available = available;
            if (!available) Cancel();
        }

        public void Press(int pointer, float x, float y, double now)
        {
            if (!CanUse()) return;
            var sample = _gesture.Press(pointer, x, y, now);
            if (sample.Signal == SkillPointerSignal.None) return;
            if (!_ability.BeginAim(true))
            {
                _gesture.Cancel();
                return;
            }
            _ownsAim = true;
            Publish(sample);
        }

        public void Move(int pointer, float x, float y, double now, bool cancelHovered)
        {
            if (CanUse()) Publish(_gesture.Move(pointer, x, y, now, cancelHovered));
        }

        public void Release(int pointer, float x, float y, double now, bool cancelHovered)
        {
            if (CanUse()) Publish(_gesture.Release(pointer, x, y, now, cancelHovered));
        }

        public void Tick(double now)
        {
            if (CanUse()) Publish(_gesture.Tick(now));
        }

        public void Cancel() => Publish(_gesture.Cancel());

        private bool CanUse()
        {
            if (!_disposed && _available && _gameplay.AdvancesGameplay && _generation == _gameplay.RunGeneration)
            {
                if (!_ownsAim || _ability.Phase == DefenseAbilityPhase.Aiming) return true;
            }
            Cancel();
            return false;
        }

        private void Publish(SkillPointerSample sample)
        {
            if (sample.Signal == SkillPointerSignal.None) return;
            if (sample.Signal == SkillPointerSignal.Cancelled || sample.Signal == SkillPointerSignal.TapReleased ||
                sample.Signal == SkillPointerSignal.AimReleased)
            {
                if (_ownsAim) _ability.CancelAim();
                _ownsAim = false;
            }
            Changed?.Invoke(new DefenseSkillGestureSample(sample, _gameplay));
        }

        private void OnGateChanged(bool active)
        {
            if (!active) Cancel();
        }

        private void OnReset()
        {
            Cancel();
            _generation = _gameplay.RunGeneration;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _gameplay.GameplayGateChanged -= OnGateChanged;
            _gameplay.RunCleanup -= Cancel;
            _gameplay.RunReset -= OnReset;
            try { Cancel(); }
            finally { Changed = null; }
        }
    }
}

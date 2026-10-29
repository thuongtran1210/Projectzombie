using System;

namespace ProjectZombie.Features.UI.Controls
{
    public enum SkillPointerSignal { None, Pressed, Aiming, TapReleased, AimReleased, Cancelled }

    /// <summary>Allocation-free gesture sample in screen space; no gameplay or rendering dependencies.</summary>
    public readonly struct SkillPointerSample
    {
        public readonly SkillPointerSignal Signal;
        public readonly float DirectionX, DirectionY, Pull;
        public readonly bool CancelHovered, AimStarted;

        public SkillPointerSample(SkillPointerSignal signal, float x = 0f, float y = 0f,
            float pull = 0f, bool cancelHovered = false, bool aimStarted = false)
        {
            Signal = signal;
            DirectionX = x;
            DirectionY = y;
            Pull = pull;
            CancelHovered = cancelHovered;
            AimStarted = aimStarted;
        }
    }

    /// <summary>Shared tap/hold/drag policy. One pointer owns a gesture until release or cancellation.</summary>
    public sealed class SkillPointerGesture
    {
        private readonly float _dragThreshold, _maxDragDistance;
        private readonly double _holdSeconds;
        private readonly bool _requireHoldOrDrag;
        private readonly bool _tapUntilDrag;
        private int _pointerId;
        private float _startX, _startY, _x, _y;
        private double _startedAt, _lastTime;
        private bool _aiming, _cancelHovered;

        public bool IsPressed { get; private set; }
        public bool IsDragging { get; private set; }
        public bool IsAiming => IsPressed && _aiming;

        public SkillPointerGesture(float dragThreshold, float maxDragDistance, double holdSeconds, bool requireHoldOrDrag = true,
            bool tapUntilDrag = false)
        {
            if (!Finite(dragThreshold) || dragThreshold <= 0f || !Finite(maxDragDistance) ||
                maxDragDistance < dragThreshold || !Finite(holdSeconds) || holdSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(dragThreshold), "Gesture thresholds must be finite and ordered.");
            _dragThreshold = dragThreshold;
            _maxDragDistance = maxDragDistance;
            _holdSeconds = holdSeconds;
            _requireHoldOrDrag = requireHoldOrDrag;
            _tapUntilDrag = tapUntilDrag;
        }

        public SkillPointerSample Press(int pointerId, float x, float y, double now)
        {
            if (IsPressed || !Finite(x) || !Finite(y) || !Finite(now)) return default;
            _pointerId = pointerId;
            _startX = _x = x;
            _startY = _y = y;
            _startedAt = _lastTime = now;
            IsPressed = true;
            _aiming = !_requireHoldOrDrag;
            return new SkillPointerSample(_aiming ? SkillPointerSignal.Aiming : SkillPointerSignal.Pressed,
                aimStarted: _aiming);
        }

        public SkillPointerSample Move(int pointerId, float x, float y, double now, bool cancelHovered)
        {
            if (!IsPressed || pointerId != _pointerId) return default;
            if (!Finite(x) || !Finite(y) || !ValidTime(now)) return Cancel();
            _x = x;
            _y = y;
            _cancelHovered = cancelHovered;
            return Evaluate(now);
        }

        public SkillPointerSample Tick(double now)
        {
            if (!IsPressed) return default;
            if (!ValidTime(now)) return Cancel();
            return Evaluate(now);
        }

        public SkillPointerSample Release(int pointerId, float x, float y, double now, bool cancelHovered)
        {
            if (!IsPressed || pointerId != _pointerId) return default;
            var sample = Move(pointerId, x, y, now, cancelHovered);
            if (sample.Signal == SkillPointerSignal.Cancelled) return sample;
            var result = new SkillPointerSample(cancelHovered ? SkillPointerSignal.Cancelled :
                ((_tapUntilDrag ? IsDragging : _aiming) ? SkillPointerSignal.AimReleased : SkillPointerSignal.TapReleased),
                sample.DirectionX, sample.DirectionY, sample.Pull, cancelHovered);
            Clear();
            return result;
        }

        public SkillPointerSample Cancel()
        {
            bool hadPointer = IsPressed;
            Clear();
            return new SkillPointerSample(hadPointer ? SkillPointerSignal.Cancelled : SkillPointerSignal.None);
        }

        private SkillPointerSample Evaluate(double now)
        {
            _lastTime = now;
            double dx = (double)_x - _startX, dy = (double)_y - _startY;
            double length = Math.Sqrt(dx * dx + dy * dy);
            IsDragging |= length >= _dragThreshold;
            bool start = !_aiming && (IsDragging || now - _startedAt >= _holdSeconds);
            _aiming |= start;
            return new SkillPointerSample(_aiming ? SkillPointerSignal.Aiming : SkillPointerSignal.Pressed,
                length > 0d ? (float)(dx / length) : 0f, length > 0d ? (float)(dy / length) : 0f,
                (float)Math.Min(1d, length / _maxDragDistance), _cancelHovered, start);
        }

        private bool ValidTime(double now) => Finite(now) && now >= _lastTime;
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        private void Clear()
        {
            IsPressed = IsDragging = _aiming = _cancelHovered = false;
        }
    }
}

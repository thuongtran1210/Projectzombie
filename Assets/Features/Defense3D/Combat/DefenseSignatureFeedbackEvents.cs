using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Ảnh chụp từng giai đoạn skill; không giữ reference tới mục tiêu có thể đã trả pool.</summary>
    public readonly struct DefenseSignatureFeedback
    {
        public readonly long CastId;
        public readonly DefensePoint3 Position;
        public readonly float Radius;
        public readonly object SessionIdentity;
        public readonly int RunGeneration;
        public readonly bool Completed;
        public DefenseSignatureFeedback(long castId, DefensePoint3 position, float radius, object sessionIdentity, int runGeneration, bool completed = false)
        { CastId = castId; Position = position; Radius = radius; SessionIdentity = sessionIdentity; RunGeneration = runGeneration; Completed = completed; }
    }
    public readonly struct DefenseSignatureHitFeedback
    {
        public readonly long CastId;
        public readonly DefenseHitFeedback Hit;
        public DefenseSignatureHitFeedback(long castId, DefenseHitFeedback hit) { CastId = castId; Hit = hit; }
    }

    /// <summary>Sự kiện cục bộ dùng chung cơ chế cô lập lỗi observer; gameplay không phụ thuộc VFX.</summary>
    public sealed class DefenseSignatureFeedbackEvents
    {
        private readonly DefensePresentationSignal<DefenseSignatureFeedback> _started = new DefensePresentationSignal<DefenseSignatureFeedback>();
        private readonly DefensePresentationSignal<DefenseSignatureFeedback> _released = new DefensePresentationSignal<DefenseSignatureFeedback>();
        private readonly DefensePresentationSignal<DefenseSignatureHitFeedback> _hits = new DefensePresentationSignal<DefenseSignatureHitFeedback>();
        private readonly DefensePresentationSignal<DefenseSignatureFeedback> _ended = new DefensePresentationSignal<DefenseSignatureFeedback>();
        private readonly Action<Exception> _reportError;
        public DefenseSignatureFeedbackEvents(Action<Exception> reportError = null) { _reportError = reportError; }
        public event Action<DefenseSignatureFeedback> Started { add => _started.Add(value); remove => _started.Remove(value); }
        public event Action<DefenseSignatureFeedback> Released { add => _released.Add(value); remove => _released.Remove(value); }
        public event Action<DefenseSignatureHitFeedback> HitConfirmed { add => _hits.Add(value); remove => _hits.Remove(value); }
        public event Action<DefenseSignatureFeedback> Ended { add => _ended.Add(value); remove => _ended.Remove(value); }
        public void PublishStarted(DefenseSignatureFeedback feedback) => _started.Publish(feedback, _reportError);
        public void PublishReleased(DefenseSignatureFeedback feedback) => _released.Publish(feedback, _reportError);
        public void PublishHit(DefenseSignatureHitFeedback feedback) => _hits.Publish(feedback, _reportError);
        public void PublishEnded(DefenseSignatureFeedback feedback) => _ended.Publish(feedback, _reportError);
    }
}

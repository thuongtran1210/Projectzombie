using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Ảnh chụp một lượt dash; ID không tái sử dụng sau cancel hoặc restart.</summary>
    public readonly struct DefenseDashFeedback
    {
        public readonly long DashId;
        public readonly DefensePoint3 Position, Direction;
        public readonly object SessionIdentity;
        public readonly int RunGeneration;
        public readonly bool Completed;
        public DefenseDashFeedback(long dashId, DefensePoint3 position, DefensePoint3 direction,
            object sessionIdentity, int runGeneration, bool completed = false)
        { DashId = dashId; Position = position; Direction = direction; SessionIdentity = sessionIdentity; RunGeneration = runGeneration; Completed = completed; }
    }

    /// <summary>Motor phát sau khi chấp nhận dash; kết thúc chỉ được phát một lần cho mỗi ID.</summary>
    public sealed class DefenseDashFeedbackEvents
    {
        private readonly DefensePresentationSignal<DefenseDashFeedback> _started = new DefensePresentationSignal<DefenseDashFeedback>();
        private readonly DefensePresentationSignal<DefenseDashFeedback> _ended = new DefensePresentationSignal<DefenseDashFeedback>();
        private readonly Action<Exception> _reportError;
        public DefenseDashFeedbackEvents(Action<Exception> reportError = null) { _reportError = reportError; }
        public event Action<DefenseDashFeedback> Started { add => _started.Add(value); remove => _started.Remove(value); }
        public event Action<DefenseDashFeedback> Ended { add => _ended.Add(value); remove => _ended.Remove(value); }
        public void PublishStarted(DefenseDashFeedback feedback) => _started.Publish(feedback, _reportError);
        public void PublishEnded(DefenseDashFeedback feedback) => _ended.Publish(feedback, _reportError);
    }
}

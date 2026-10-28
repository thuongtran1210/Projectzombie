using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Ảnh chụp kết quả vung đòn; không giữ reference tới mục tiêu có thể bị trả pool.</summary>
    public readonly struct DefenseSwingFeedback
    {
        public readonly DefenseConeSnapshot Cone;
        public readonly object SessionIdentity;
        public readonly int RunGeneration;
        public DefenseSwingFeedback(DefenseConeSnapshot cone, object sessionIdentity, int runGeneration)
        { Cone = cone; SessionIdentity = sessionIdentity; RunGeneration = runGeneration; }
    }

    /// <summary>Điểm trúng được chụp trước TakeDamage để đòn kết liễu vẫn có vị trí hiển thị đúng.</summary>
    public readonly struct DefenseHitFeedback
    {
        public readonly DefensePoint3 Position, Direction;
        public readonly float Damage;
        public readonly object SessionIdentity;
        public readonly int RunGeneration;
        public DefenseHitFeedback(DefensePoint3 position, DefensePoint3 direction, float damage, object sessionIdentity, int runGeneration)
        { Position = position; Direction = direction; Damage = damage; SessionIdentity = sessionIdentity; RunGeneration = runGeneration; }
    }

    public interface IDefenseAttackFeedbackSource
    {
        DefenseAttackFeedbackEvents Feedback { get; }
    }

    /// <summary>Sự kiện cục bộ; cache danh sách observer khi đăng ký, cô lập lỗi hiển thị khỏi gameplay.</summary>
    public sealed class DefenseAttackFeedbackEvents
    {
        private readonly DefensePresentationSignal<DefenseSwingFeedback> _swings = new DefensePresentationSignal<DefenseSwingFeedback>();
        private readonly DefensePresentationSignal<DefenseHitFeedback> _hits = new DefensePresentationSignal<DefenseHitFeedback>();
        private readonly Action<Exception> _reportError;
        public DefenseAttackFeedbackEvents(Action<Exception> reportError = null) { _reportError = reportError; }
        public event Action<DefenseSwingFeedback> SwingResolved { add => _swings.Add(value); remove => _swings.Remove(value); }
        public event Action<DefenseHitFeedback> HitConfirmed { add => _hits.Add(value); remove => _hits.Remove(value); }
        public void PublishSwing(DefenseSwingFeedback swing) => _swings.Publish(swing, _reportError);
        public void PublishHit(DefenseHitFeedback hit) => _hits.Publish(hit, _reportError);
    }
}

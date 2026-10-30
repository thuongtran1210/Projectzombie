using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseSpawnNoticeState { Scheduled, Spawned, Closed }

    /// <summary>Immutable presentation snapshot; observers cannot consume or change spawn jobs.</summary>
    public readonly struct DefenseSpawnNotice
    {
        public readonly DefenseSpawnJob Job;
        public readonly DefensePoint3 Position;
        public readonly float AnnouncedAt, FinishedAt;
        public readonly bool Dynamic;
        public readonly DefenseSpawnNoticeState State;

        public DefenseSpawnNotice(DefenseSpawnJob job, DefensePoint3 position, float announcedAt,
            bool dynamic, DefenseSpawnNoticeState state = DefenseSpawnNoticeState.Scheduled, float finishedAt = 0f)
        {
            Job = job; Position = position; AnnouncedAt = announcedAt;
            Dynamic = dynamic; State = state; FinishedAt = finishedAt;
        }

        /// <summary>Due jobs remain visible until resolved; completed jobs briefly confirm actual arrival.</summary>
        public bool TryDisplay(float now, float leadSeconds, float arrivalSeconds, out float fraction, out bool pending)
        {
            fraction = 0f; pending = false;
            if (!DefenseDamageRules.IsFinite(now) || now < 0f || !DefenseDamageRules.IsFinite(leadSeconds) || leadSeconds <= 0f ||
                !DefenseDamageRules.IsFinite(arrivalSeconds) || arrivalSeconds < 0f || State == DefenseSpawnNoticeState.Closed) return false;
            if (State == DefenseSpawnNoticeState.Spawned)
            {
                if (now < FinishedAt || now - FinishedAt >= arrivalSeconds) return false;
                fraction = 1f;
                return true;
            }
            float start = Dynamic ? AnnouncedAt : Math.Max(0f, Job.ScheduledTime - leadSeconds);
            if (now < start) return false;
            pending = now >= Job.ScheduledTime;
            float duration = Dynamic ? Job.ScheduledTime - AnnouncedAt : leadSeconds;
            fraction = pending ? 0f : Math.Min(1f, (Job.ScheduledTime - now) / Math.Max(.001f, duration));
            return true;
        }
    }

    public interface IDefenseSpawnWarningSource
    {
        int Count { get; }
        DefenseSpawnNotice GetNotice(int index);
    }

    /// <summary>Runtime-owned visual ledger keyed by stable job identity, independent of scheduling decisions.</summary>
    public sealed class DefenseSpawnWarningTimeline : IDefenseSpawnWarningSource
    {
        private readonly List<DefenseSpawnNotice> _notices = new List<DefenseSpawnNotice>();
        private readonly List<DefenseSpawnNotice> _scheduled = new List<DefenseSpawnNotice>();
        private readonly Dictionary<(string EventId, int Slot), int> _indices = new Dictionary<(string, int), int>();
        public int Count => _notices.Count;
        public DefenseSpawnNotice GetNotice(int index) => _notices[index];

        public bool Add(DefenseSpawnJob job, DefensePoint3 position, float announcedAt = 0f, bool dynamic = false)
        {
            if (string.IsNullOrWhiteSpace(job.EventId) || job.SlotIndex < 0 || !position.IsFinite ||
                !DefenseDamageRules.IsFinite(job.ScheduledTime) || job.ScheduledTime < 0f ||
                !DefenseDamageRules.IsFinite(announcedAt) || announcedAt < 0f || announcedAt > job.ScheduledTime) return false;
            var key = (job.EventId, job.SlotIndex);
            if (_indices.ContainsKey(key)) return false;
            _indices.Add(key, _notices.Count);
            var notice = new DefenseSpawnNotice(job, position, announcedAt, dynamic);
            _notices.Add(notice);
            if (!dynamic) _scheduled.Add(notice);
            return true;
        }

        /// <summary>Only the owning runtime closes jobs, using actual planner/factory outcomes.</summary>
        public void Complete(DefenseSpawnJob job, float now, bool spawned, DefensePoint3? actualPosition = null)
        {
            if (!DefenseDamageRules.IsFinite(now) || now < 0f || !_indices.TryGetValue((job.EventId, job.SlotIndex), out int index)) return;
            var previous = _notices[index];
            if (previous.State != DefenseSpawnNoticeState.Scheduled) return;
            var position = actualPosition.HasValue && actualPosition.Value.IsFinite ? actualPosition.Value : previous.Position;
            _notices[index] = new DefenseSpawnNotice(previous.Job, position, previous.AnnouncedAt, previous.Dynamic,
                spawned ? DefenseSpawnNoticeState.Spawned : DefenseSpawnNoticeState.Closed, now);
        }

        public void Reset()
        {
            _indices.Clear();
            _notices.Clear();
            for (int i = 0; i < _scheduled.Count; i++)
            {
                var notice = _scheduled[i];
                _notices.Add(notice);
                _indices.Add((notice.Job.EventId, notice.Job.SlotIndex), i);
            }
        }
    }
}

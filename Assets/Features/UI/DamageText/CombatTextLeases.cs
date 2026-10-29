using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>Quản lý các lượt mượn đang hoạt động, không cấp phát item. Callback của lượt cũ không thể thu hồi item đã được mượn lại.</summary>
    public sealed class CombatTextLeases<T> : IDisposable where T : class
    {
        private readonly struct Entry
        {
            public readonly T Item;
            public readonly long Token;
            public readonly int Priority;
            public Entry(T item, long token, int priority) { Item = item; Token = token; Priority = priority; }
        }
        private readonly List<Entry> _active = new List<Entry>();
        private readonly Func<T> _get;
        private readonly Action<T> _release;
        private readonly int _capacity;
        private long _nextToken;
        private bool _disposed, _clearing;
        public int Count => _active.Count;

        public CombatTextLeases(int capacity, Func<T> get, Action<T> release)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _get = get ?? throw new ArgumentNullException(nameof(get));
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        public bool TryAcquire(int priority, out T item, out long token)
        {
            item = null; token = 0;
            if (_disposed || _clearing) return false;
            if (_active.Count >= _capacity)
            {
                int victim = -1;
                for (int i = 0; i < _active.Count; i++)
                    if (_active[i].Priority < priority && (victim < 0 || _active[i].Priority < _active[victim].Priority)) victim = i;
                if (victim < 0) return false;
                ReleaseAt(victim);
            }
            item = _get();
            if (item == null) return false;
            token = checked(++_nextToken);
            _active.Add(new Entry(item, token, priority));
            return true;
        }

        public bool Complete(T item, long token)
        {
            // Chỉ thu hồi khi cả item và mã lượt mượn đều khớp, tránh trả pool trùng hoặc trả nhầm lượt mới.
            for (int i = 0; i < _active.Count; i++)
                if (ReferenceEquals(_active[i].Item, item) && _active[i].Token == token)
                { ReleaseAt(i); return true; }
            return false;
        }
        private void ReleaseAt(int index)
        {
            var entry = _active[index];
            _active.RemoveAt(index);
            _release(entry.Item);
        }
        public void Clear()
        {
            // Chặn yêu cầu mượn phát sinh từ callback thu hồi trong lúc đang dọn dẹp.
            if (_clearing) return;
            _clearing = true;
            try { while (_active.Count > 0) ReleaseAt(_active.Count - 1); }
            finally { _clearing = false; }
        }
        public void Dispose() { if (_disposed) return; _disposed = true; Clear(); }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Định danh một lượt mượn; cùng instance ở lượt sau có token khác.</summary>
    public readonly struct PoolLease<T> where T : class
    {
        internal readonly object Owner;
        public readonly T Item;
        public readonly long Token;
        internal PoolLease(object owner, T item, long token) { Owner = owner; Item = item; Token = token; }
        public bool IsAssigned => Owner != null && Item != null && Token > 0;
    }

    /// <summary>Lõi pool C# thuần: đóng lượt mượn trước callback, thu hồi active trước khi hủy idle.</summary>
    public sealed class OwnedPool<T> : IDisposable where T : class
    {
        private sealed class ReferenceComparer : IEqualityComparer<T>
        {
            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
        private readonly Stack<T> _idle = new Stack<T>();
        private readonly Dictionary<T, long> _active = new Dictionary<T, long>(new ReferenceComparer());
        private readonly Func<T> _create;
        private readonly Action<T> _reset, _destroy;
        private readonly Func<T, bool> _isValid;
        private readonly int _maxIdle;
        private long _nextToken;
        private bool _disposed, _clearing;
        public int ActiveCount => _active.Count;
        public int InactiveCount => _idle.Count;
        public bool IsDisposed => _disposed;

        public OwnedPool(Func<T> create, Action<T> reset, Action<T> destroy, int maxIdle = 50, Func<T, bool> isValid = null)
        {
            if (maxIdle < 0) throw new ArgumentOutOfRangeException(nameof(maxIdle));
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _reset = reset ?? throw new ArgumentNullException(nameof(reset));
            _destroy = destroy ?? throw new ArgumentNullException(nameof(destroy));
            _isValid = isValid ?? (item => item != null);
            _maxIdle = maxIdle;
        }
        public PoolLease<T> Acquire()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(OwnedPool<T>));
            if (_clearing) throw new InvalidOperationException("Cannot acquire during cleanup.");
            T item = null;
            while (_idle.Count > 0 && !_isValid(item)) item = _idle.Pop();
            if (!_isValid(item)) item = _create();
            if (!_isValid(item)) throw new InvalidOperationException("Pool allocator returned an invalid item.");
            long token = checked(++_nextToken);
            _active.Add(item, token);
            return new PoolLease<T>(this, item, token);
        }
        public bool TryGetLease(T item, out PoolLease<T> lease)
        {
            if (item != null && _active.TryGetValue(item, out long token))
            { lease = new PoolLease<T>(this, item, token); return true; }
            lease = default; return false;
        }
        public bool TryReturn(PoolLease<T> lease)
        {
            if (!ReferenceEquals(lease.Owner, this) || lease.Item == null ||
                !_active.TryGetValue(lease.Item, out long token) || token != lease.Token) return false;
            _active.Remove(lease.Item);
            if (!_isValid(lease.Item)) return true;
            try { _reset(lease.Item); }
            catch { _destroy(lease.Item); throw; }
            if (_disposed || _idle.Count >= _maxIdle) _destroy(lease.Item);
            else _idle.Push(lease.Item);
            return true;
        }
        public void ClearActive()
        {
            if (_clearing) return;
            _clearing = true;
            List<Exception> failures = null;
            try
            {
                var snapshot = new List<KeyValuePair<T, long>>(_active);
                foreach (var entry in snapshot)
                    try { TryReturn(new PoolLease<T>(this, entry.Key, entry.Value)); }
                    catch (Exception error) { if (failures == null) failures = new List<Exception>(); failures.Add(error); }
            }
            finally { _clearing = false; }
            if (failures != null) throw new AggregateException(failures);
        }
        public void Prewarm(int count)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(OwnedPool<T>));
            if (_clearing || count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            while (_idle.Count < Math.Min(count, _maxIdle))
            {
                var item = _create();
                if (!_isValid(item)) throw new InvalidOperationException("Invalid prewarm item.");
                try { _reset(item); _idle.Push(item); }
                catch { _destroy(item); throw; }
            }
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            var failures = new List<Exception>();
            try { ClearActive(); }
            catch (Exception error) { failures.Add(error); }
            while (_idle.Count > 0)
            {
                var item = _idle.Pop();
                if (!_isValid(item)) continue;
                try { _destroy(item); }
                catch (Exception error) { failures.Add(error); }
            }
            if (failures.Count > 0) throw new AggregateException(failures);
        }
    }
}

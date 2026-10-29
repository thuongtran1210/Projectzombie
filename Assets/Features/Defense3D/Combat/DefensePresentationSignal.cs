using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Cache observer khi đăng ký/gỡ; lỗi trình diễn không được ngắt gameplay.</summary>
    internal sealed class DefensePresentationSignal<T>
    {
        private readonly List<Action<T>> _handlers = new List<Action<T>>();
        private Action<T>[] _snapshot = Array.Empty<Action<T>>();
        public void Add(Action<T> handler) { if (handler == null) return; _handlers.Add(handler); _snapshot = _handlers.ToArray(); }
        public void Remove(Action<T> handler)
        {
            int index = _handlers.LastIndexOf(handler);
            if (index < 0) return;
            _handlers.RemoveAt(index); _snapshot = _handlers.ToArray();
        }
        public void Publish(T value, Action<Exception> reportError)
        {
            var snapshot = _snapshot;
            foreach (var handler in snapshot)
                try { handler(value); }
                catch (Exception error)
                {
                    // Kể cả logger bị lỗi cũng không được làm mất damage hoặc observer kế tiếp.
                    try { reportError?.Invoke(error); } catch { }
                }
        }
    }
}

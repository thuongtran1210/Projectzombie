using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public interface IDefenseEnemyPool
    {
        PoolLease<GameObject> Acquire(GameObject prefab, Transform parent);
        bool TryGetLease(GameObject item, out PoolLease<GameObject> lease);
        bool TryRelease(PoolLease<GameObject> lease);
    }

    /// <summary>Pool theo prefab, thuộc một session; factory nắm token để trả đúng lượt mượn.</summary>
    public sealed class DefenseEnemyPoolManager : IDefenseEnemyPool, IDisposable
    {
        private static DefenseEnemyPoolManager _activeSessionPool;
        public static DefenseEnemyPoolManager ActiveSessionPool => _activeSessionPool;
        private readonly Transform _poolsRoot;
        private readonly Dictionary<GameObject, OwnedPool<GameObject>> _pools = new Dictionary<GameObject, OwnedPool<GameObject>>();
        private readonly Dictionary<GameObject, OwnedPool<GameObject>> _owners = new Dictionary<GameObject, OwnedPool<GameObject>>();
        private readonly Dictionary<GameObject, Transform> _storageByItem = new Dictionary<GameObject, Transform>();
        private readonly Queue<GameObject> _pendingReturns = new Queue<GameObject>();
        private readonly List<GameObject> _storageGroups = new List<GameObject>();
        private bool _disposed;
        public DefenseEnemyPoolManager(Transform poolsRoot = null) { _poolsRoot = poolsRoot; }
        // Chỉ dành cho caller cũ; session mới truyền dependency trực tiếp.
        public static void RegisterSessionPool(DefenseEnemyPoolManager pool)
        {
            if (_activeSessionPool != null && !ReferenceEquals(_activeSessionPool, pool))
                throw new InvalidOperationException("Another legacy enemy pool is registered.");
            _activeSessionPool = pool;
        }
        public static void UnregisterSessionPool(DefenseEnemyPoolManager pool)
        { if (ReferenceEquals(_activeSessionPool, pool)) _activeSessionPool = null; }
        private OwnedPool<GameObject> GetPool(GameObject prefab)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DefenseEnemyPoolManager));
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (_pools.TryGetValue(prefab, out var pool)) return pool;
            var storage = new GameObject(prefab.name).transform;
            storage.SetParent(_poolsRoot, false);
            storage.gameObject.SetActive(false);
            _storageGroups.Add(storage.gameObject);
            OwnedPool<GameObject> created = null;
            created = new OwnedPool<GameObject>(() =>
            {
                var item = DefenseEnemyPrefab3D.Instantiate(prefab, storage);
                item.SetActive(false);
                _owners.Add(item, created);
                _storageByItem[item] = storage;
                return item;
            }, item =>
            {
                item.SetActive(false);
                // Không reparent trong OnDisable của ancestor; FlushReturns xử lý ở frame an toàn.
                foreach (var participant in item.GetComponentsInChildren<DefenseGameplayBehaviour>(true)) participant.UnbindGameplay();
                _pendingReturns.Enqueue(item);
            }, item => { _owners.Remove(item); _storageByItem.Remove(item); DefensePoolObjects.Destroy(item); }, 50, item => item != null);
            _pools.Add(prefab, created);
            return created;
        }
        /// <summary>Chuyển object đã trả về nhóm lưu trữ ở frame an toàn, ngoài callback OnDisable.</summary>
        public void FlushReturns()
        {
            while (_pendingReturns.Count > 0)
            {
                var item = _pendingReturns.Dequeue();
                if (item != null && !TryGetLease(item, out _) && _storageByItem.TryGetValue(item, out var storage) && storage != null)
                    item.transform.SetParent(storage, false);
            }
        }
        public PoolLease<GameObject> Acquire(GameObject prefab, Transform parent)
        {
            var lease = GetPool(prefab).Acquire();
            lease.Item.SetActive(false);
            lease.Item.transform.SetParent(parent != null ? parent : _poolsRoot, false);
            lease.Item.transform.localRotation = Quaternion.identity;
            lease.Item.transform.localScale = prefab.transform.localScale;
            return lease;
        }
        public bool TryGetLease(GameObject item, out PoolLease<GameObject> lease)
        {
            lease = default;
            return !ReferenceEquals(item, null) && _owners.TryGetValue(item, out var pool) && pool.TryGetLease(item, out lease);
        }
        public bool TryRelease(PoolLease<GameObject> lease)
        {
            if (ReferenceEquals(lease.Item, null) || !_owners.TryGetValue(lease.Item, out var pool) || !pool.TryReturn(lease)) return false;
            if (lease.Item == null) { _owners.Remove(lease.Item); _storageByItem.Remove(lease.Item); }
            return true;
        }
        // API tương thích đồng bộ; luồng production luôn giữ token riêng.
        public GameObject Get(GameObject prefab, Transform parent = null) => Acquire(prefab, parent).Item;
        public bool TryRelease(GameObject item) => TryGetLease(item, out var lease) && TryRelease(lease);
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { foreach (var pool in _pools.Values) pool.Dispose(); }
            finally
            {
                _pools.Clear(); _owners.Clear(); _storageByItem.Clear(); _pendingReturns.Clear();
                foreach (var group in _storageGroups) if (group != null) DefensePoolObjects.Destroy(group);
                _storageGroups.Clear(); UnregisterSessionPool(this);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public interface IDefenseWoodDrops
    {
        IReadOnlyList<DefenseWoodDrop3D> ActiveDrops { get; }
        DefenseWoodDrop3D Spawn(Vector3 position, int amount, Transform collector, DefenseWoodWallet3D wallet,
            IDefenseGameplayContext gameplay, Transform parent);
    }

    /// <summary>Sở hữu pickup của một session; khởi tạo khi inactive rồi mới kích hoạt.</summary>
    public sealed class DefenseWoodDropPool : IDefenseWoodDrops, IDisposable
    {
        private static DefenseWoodDropPool _activeSessionPool;
        public static DefenseWoodDropPool ActiveSessionPool => _activeSessionPool;
        private readonly OwnedPool<DefenseWoodDrop3D> _pool;
        private readonly GameObject _prefab;
        private readonly Transform _poolRoot;
        private readonly Transform _storage;
        private readonly List<DefenseWoodDrop3D> _activeDrops = new List<DefenseWoodDrop3D>(80);
        private readonly IReadOnlyList<DefenseWoodDrop3D> _readOnlyDrops;
        private readonly IDefenseGameplayContext _gameplay;
        private readonly Queue<DefenseWoodDrop3D> _pendingReturns = new Queue<DefenseWoodDrop3D>();
        public IReadOnlyList<DefenseWoodDrop3D> ActiveDrops => _readOnlyDrops;

        public DefenseWoodDropPool(GameObject prefab, Transform poolRoot = null, int defaultCapacity = 20, int maxSize = 80,
            IDefenseGameplayContext gameplay = null)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            if (prefab.GetComponent<DefenseWoodDrop3D>() == null) throw new ArgumentException("Wood prefab requires DefenseWoodDrop3D.");
            _poolRoot = poolRoot; _gameplay = gameplay;
            _storage = new GameObject(prefab.name).transform;
            _storage.SetParent(poolRoot, false);
            _storage.gameObject.SetActive(false);
            _readOnlyDrops = _activeDrops.AsReadOnly();
            _pool = new OwnedPool<DefenseWoodDrop3D>(CreateInstance, ResetInstance, DestroyInstance, maxSize, item => item != null);
        }
        // Giữ tương thích với caller cũ nhưng không dispose tài nguyên của session khác.
        public static void RegisterSessionPool(DefenseWoodDropPool pool)
        {
            if (_activeSessionPool != null && !ReferenceEquals(_activeSessionPool, pool))
                throw new InvalidOperationException("Another legacy pickup pool is registered.");
            _activeSessionPool = pool;
        }
        public static void UnregisterSessionPool(DefenseWoodDropPool pool)
        { if (ReferenceEquals(_activeSessionPool, pool)) _activeSessionPool = null; }
        private DefenseWoodDrop3D CreateInstance()
        {
            var instance = DefenseEnemyPrefab3D.Instantiate(_prefab, _storage);
            instance.SetActive(false);
            return instance.GetComponent<DefenseWoodDrop3D>();
        }
        private void ResetInstance(DefenseWoodDrop3D drop)
        {
            _activeDrops.Remove(drop);
            drop.ResetForPool();
            drop.gameObject.SetActive(false);
            drop.UnbindGameplay();
            _pendingReturns.Enqueue(drop);
        }
        private void DestroyInstance(DefenseWoodDrop3D drop) => DefensePoolObjects.Destroy(drop.gameObject);
        private PoolLease<DefenseWoodDrop3D> Acquire()
        {
            var lease = _pool.Acquire();
            _activeDrops.Add(lease.Item);
            lease.Item.SetPoolRelease(item => _pool.TryReturn(lease));
            return lease;
        }
        public DefenseWoodDrop3D Spawn(Vector3 position, int amount, Transform collector, DefenseWoodWallet3D wallet,
            IDefenseGameplayContext gameplay, Transform parent)
        {
            if (gameplay == null || (_gameplay != null && !ReferenceEquals(gameplay, _gameplay)))
                throw new ArgumentException("Pickup session does not match its pool.");
            var lease = Acquire();
            try
            {
                var drop = lease.Item;
                drop.transform.SetParent(parent, false);
                drop.transform.SetPositionAndRotation(position, Quaternion.identity);
                drop.transform.localScale = _prefab.transform.localScale;
                drop.BindGameplay(gameplay);
                drop.Initialize(amount, collector, wallet);
                drop.gameObject.SetActive(true);
                return drop;
            }
            catch { _pool.TryReturn(lease); throw; }
        }
        // Chỉ giữ để tương thích test/caller cũ; production dùng Spawn để bind trước OnEnable.
        public DefenseWoodDrop3D Get()
        {
            var lease = Acquire();
            lease.Item.transform.SetParent(_poolRoot, false);
            lease.Item.gameObject.SetActive(true);
            return lease.Item;
        }
        public void Release(DefenseWoodDrop3D drop)
        { if (_pool.TryGetLease(drop, out var lease)) _pool.TryReturn(lease); }
        public void Clear() => _pool.ClearActive();
        public void FlushReturns()
        {
            while (_pendingReturns.Count > 0)
            {
                var drop = _pendingReturns.Dequeue();
                if (drop != null && !_pool.TryGetLease(drop, out _) && _storage != null) drop.transform.SetParent(_storage, false);
            }
        }
        public void Dispose()
        {
            try { _pool.Dispose(); }
            finally { _activeDrops.Clear(); _pendingReturns.Clear(); if (_storage != null) DefensePoolObjects.Destroy(_storage.gameObject); UnregisterSessionPool(this); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Bounded prefab projectile inventory. Exhaustion rejects the shot; it never converts to instant damage.</summary>
    public sealed class DefenseProjectilePool3D : DefenseGameplayBehaviour, IDefenseProjectileService
    {
        [SerializeField] private GameObject _defaultProjectilePrefab;
        [SerializeField, Min(1)] private int _defaultPoolCapacity = 32;
        [SerializeField, Min(1)] private int _maxPoolSize = 128;
        private sealed class Record
        {
            public ObjectPool<ProjectileCore3D> Pool;
            public Action<ProjectileCore3D> Return;
        }
        private readonly Dictionary<GameObject, Record> _records = new Dictionary<GameObject, Record>();
        private readonly List<ProjectileCore3D> _active = new List<ProjectileCore3D>(64);
        private Transform _root;
        public int CapacityPerType => _defaultPoolCapacity;
        public int ActiveCount => _active.Count;

        private void Awake()
        {
            EnsureRoot();
        }

        private Transform EnsureRoot()
        {
            if (_root == null)
            {
                var root = new GameObject("Projectile Inventory");
                root.transform.SetParent(transform, false);
                _root = root.transform;
            }
            return _root;
        }

        public ObjectPool<ProjectileCore3D> GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null || !prefab.TryGetComponent<ProjectileCore3D>(out _)) return null;
            if (_records.TryGetValue(prefab, out var found)) return found.Pool;
            if (CanAdvanceGameplay) throw new InvalidOperationException("Register projectile pools before gameplay.");
            var record = new Record();
            record.Pool = new ObjectPool<ProjectileCore3D>(
                () =>
                {
                    var instance = Instantiate(prefab, EnsureRoot());
                    instance.SetActive(false);
                    return instance.GetComponent<ProjectileCore3D>();
                },
                core => _active.Add(core),
                core =>
                {
                    int index = _active.IndexOf(core);
                    if (index >= 0) { _active[index] = _active[_active.Count - 1]; _active.RemoveAt(_active.Count - 1); }
                    if (core != null) core.gameObject.SetActive(false);
                },
                core =>
                {
                    if (core != null)
                    {
                        if (Application.isPlaying) Destroy(core.gameObject);
                        else DestroyImmediate(core.gameObject);
                    }
                },
                true, _defaultPoolCapacity, _maxPoolSize);
            record.Return = record.Pool.Release;
            _records.Add(prefab, record);
            _active.Capacity = Mathf.Max(_active.Capacity, _records.Count * _maxPoolSize);
            return record.Pool;
        }

        public async Task PrewarmAsync(GameObject prefab, int count)
        {
            count = Mathf.Clamp(count, 1, _maxPoolSize);
            var pool = GetOrCreatePool(prefab);
            if (pool == null) throw new InvalidOperationException("Projectile prefab requires ProjectileCore3D.");
            if (pool.CountAll >= count) return;
            var buffer = new ProjectileCore3D[count];
            int acquired = 0;
            try
            {
                for (; acquired < count; acquired++)
                {
                    if (this == null || (Gameplay is DefenseSession session && session.IsDisposed)) throw new OperationCanceledException();
                    buffer[acquired] = pool.Get();
                    await Task.Yield();
                }
            }
            finally
            {
                for (int i = 0; i < acquired; i++) if (buffer[i] != null) pool.Release(buffer[i]);
            }
        }

        public void Prewarm(GameObject prefab, int count)
        {
            var pool = GetOrCreatePool(prefab);
            if (pool == null || count <= 0) return;
            count = Mathf.Min(count, _maxPoolSize);
            var buffer = new ProjectileCore3D[count];
            for (int i = 0; i < count; i++) buffer[i] = pool.Get();
            for (int i = 0; i < count; i++) pool.Release(buffer[i]);
        }

        public bool LaunchProjectile(GameObject prefab, DefenseProjectileLaunchRequest request,
            IReadOnlyList<SO_AttackEffect> effects, DefenseProjectileDefinitionSO definition = null)
        {
            var key = prefab != null ? prefab : _defaultProjectilePrefab;
            if (!CanAdvanceGameplay || !isActiveAndEnabled || key == null ||
                request.RunGeneration != Gameplay.RunGeneration || !ReferenceEquals(request.SessionIdentity, Gameplay.SessionIdentity) ||
                !_records.TryGetValue(key, out var record) || record.Pool.CountInactive == 0) return false;
            var projectile = record.Pool.Get();
            projectile.SetDefinition(definition);
            projectile.Launch(request.Source, request.Origin, request.Target, request.Damage, request.Speed, request.ArcHeight,
                effects, request.RunGeneration, request.SessionIdentity, Gameplay, record.Return, request.TargetPosition);
            return true;
        }

        public bool TryLaunch(DefenseProjectileLaunchRequest request) => LaunchProjectile(_defaultProjectilePrefab, request, null);
        public bool CanLaunch(GameObject prefab, int count) => prefab != null && count > 0 &&
            _records.TryGetValue(prefab, out var record) && record.Pool.CountInactive >= count;

        private void RecallAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) if (_active[i] != null) _active[i].Despawn();
        }

        protected override void OnGameplayCleanup() => RecallAll();
        protected override void OnGameplayReset() => RecallAll();
        private void OnDisable() => RecallAll();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_records != null)
            {
                foreach (var record in _records.Values)
                {
                    record.Pool?.Clear();
                }
                _records.Clear();
            }
            if (_root != null)
            {
                if (Application.isPlaying) Destroy(_root.gameObject);
                else DestroyImmediate(_root.gameObject);
            }
        }
    }
}

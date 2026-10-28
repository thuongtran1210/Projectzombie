using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Sở hữu pool VFX theo session; coroutine chỉ chạy khi có hiệu ứng đang hiển thị.</summary>
    public sealed class DefenseEffectService3D : MonoBehaviour, IDefenseEffectService
    {
        [SerializeField, Min(1)] private int _maxActive = 32;
        [SerializeField, Min(0)] private int _maxIdlePerPrefab = 16;
        private sealed class Entry
        {
            public DefenseEffectHandle Handle;
            public OwnedPool<DefenseEffectInstance3D> Pool;
            public PoolLease<DefenseEffectInstance3D> Lease;
            public DefenseEffectLifetime Lifetime;
            public int Priority;
            public Component Owner;
            public Transform Anchor;
            public bool HasOwner, HasAnchor;
            public Vector3 Offset;
        }
        private readonly Dictionary<DefenseEffectInstance3D, OwnedPool<DefenseEffectInstance3D>> _pools = new Dictionary<DefenseEffectInstance3D, OwnedPool<DefenseEffectInstance3D>>();
        private readonly List<Entry> _active = new List<Entry>();
        private readonly List<GameObject> _storageGroups = new List<GameObject>();
        private IDefenseGameplayContext _gameplay;
        private DefenseRuntimeRoots3D _roots;
        private readonly Queue<(OwnedPool<DefenseEffectInstance3D> Pool, DefenseEffectInstance3D Item, Transform Storage)> _pendingReturns =
            new Queue<(OwnedPool<DefenseEffectInstance3D>, DefenseEffectInstance3D, Transform)>();
        private Coroutine _parking;
        private Coroutine _runner;
        private long _nextHandle;
        public int ActiveCount => _active.Count;
        public void Bind(IDefenseGameplayContext gameplay, DefenseRuntimeRoots3D roots)
        {
            Unbind();
            if (gameplay == null || roots == null) throw new ArgumentNullException(nameof(gameplay));
            if (roots.gameObject.scene != gameObject.scene) throw new ArgumentException("Effect service and roots must share a scene.");
            roots.ValidatePresentationBindings();
            _gameplay = gameplay; _roots = roots;
            _gameplay.RunCleanup += Clear;
            _gameplay.GameplayGateChanged += OnGate;
        }
        public DefenseEffectHandle Play(DefenseEffectProfileSO profile, Vector3 position, float radius = 1f, Component owner = null, Transform anchor = null)
            => Play(new DefenseEffectRequest(profile, position, radius, owner, anchor));

        public DefenseEffectHandle Play(in DefenseEffectRequest request)
        {
            var profile = request.Profile;
            var position = request.Position;
            var radius = request.Radius;
            var owner = request.Owner;
            var anchor = request.Anchor;
            var rotation = request.Rotation;
            if (!Finite(rotation)) return default;
            rotation = Quaternion.Normalize(rotation);
            if (!isActiveAndEnabled || _gameplay == null || !_gameplay.AdvancesGameplay || profile == null || !profile.IsValid ||
                !Finite(position) || !DefenseDamageRules.IsFinite(radius) || radius <= 0f) return default;
            float diameter = radius * 2f * profile.Scale;
            if (!DefenseDamageRules.IsFinite(diameter)) return default;
            if (_active.Count >= Mathf.Max(1, _maxActive))
            {
                int victim = -1;
                for (int i = 0; i < _active.Count; i++)
                    if (_active[i].Priority < profile.Priority && (victim < 0 || _active[i].Priority < _active[victim].Priority)) victim = i;
                if (victim < 0) return default;
                StopAt(victim);
            }
            if (!_pools.TryGetValue(profile.Prefab, out var pool))
            {
                var prefab = profile.Prefab;
                var storage = new GameObject(prefab.name);
                storage.transform.SetParent(_roots.EffectPools, false);
                _storageGroups.Add(storage);
                pool = new OwnedPool<DefenseEffectInstance3D>(() =>
                {
                    var instance = DefenseEnemyPrefab3D.Instantiate(prefab.gameObject, storage.transform);
                    instance.SetActive(false);
                    return instance.GetComponent<DefenseEffectInstance3D>();
                }, item =>
                {
                    item.ResetPresentation();
                    _pendingReturns.Enqueue((pool, item, storage.transform));
                    if (_parking == null && isActiveAndEnabled) _parking = StartCoroutine(ParkReturns());
                }, item => DefensePoolObjects.Destroy(item.gameObject), Mathf.Max(0, _maxIdlePerPrefab), item => item != null);
                _pools.Add(prefab, pool);
            }
            var lease = pool.Acquire();
            var handle = new DefenseEffectHandle(this, checked(++_nextHandle));
            try
            {
                var instance = lease.Item;
                instance.Prepare(profile.Color, diameter);
                instance.transform.SetParent(anchor != null ? _roots.AttachedEffects : _roots.WorldEffects, false);
                instance.transform.SetPositionAndRotation(position, rotation);
                _active.Add(new Entry { Handle = handle, Pool = pool, Lease = lease,
                    Lifetime = new DefenseEffectLifetime(_gameplay.ElapsedSeconds, profile.Duration, profile.Sustained),
                    Priority = profile.Priority, Owner = owner, Anchor = anchor, HasOwner = owner != null,
                    HasAnchor = anchor != null, Offset = anchor != null ? position - anchor.position : Vector3.zero });
                instance.Begin();
                if (_runner == null) _runner = StartCoroutine(Animate());
                return handle;
            }
            catch
            {
                for (int i = _active.Count - 1; i >= 0; i--)
                    if (_active[i].Handle.Token == handle.Token) _active.RemoveAt(i);
                pool.TryReturn(lease); throw;
            }
        }
        private IEnumerator ParkReturns()
        {
            // Đợi frame tiếp theo để không reparent trong callback disable của ancestor.
            yield return null;
            while (_pendingReturns.Count > 0)
            {
                var entry = _pendingReturns.Dequeue();
                if (entry.Item != null && entry.Storage != null && !entry.Pool.TryGetLease(entry.Item, out _))
                    entry.Item.transform.SetParent(entry.Storage, false);
            }
            _parking = null;
        }
        private void OnEnable()
        {
            if (_pendingReturns.Count > 0 && _parking == null) _parking = StartCoroutine(ParkReturns());
        }

        private IEnumerator Animate()
        {
            // Nhường frame đầu để _runner được gán trước mọi nhánh kết thúc coroutine.
            yield return null;
            while (_active.Count > 0)
            {
                for (int i = _active.Count - 1; i >= 0; i--)
                {
                    var entry = _active[i];
                    if (entry.Lease.Item == null || (entry.HasOwner && (entry.Owner == null || !entry.Owner.gameObject.activeInHierarchy)) ||
                        (entry.HasAnchor && entry.Anchor == null) || entry.Lifetime.IsExpired(_gameplay.ElapsedSeconds))
                    { StopAt(i); continue; }
                    if (!_gameplay.AdvancesGameplay) continue;
                    if (entry.HasAnchor) entry.Lease.Item.transform.position = entry.Anchor.position + entry.Offset;
                    entry.Lease.Item.Render(entry.Lifetime.Progress(_gameplay.ElapsedSeconds));
                }
                yield return null;
            }
            _runner = null;
        }
        private static bool Finite(Quaternion rotation) => DefenseDamageRules.IsFinite(rotation.x) && DefenseDamageRules.IsFinite(rotation.y) &&
            DefenseDamageRules.IsFinite(rotation.z) && DefenseDamageRules.IsFinite(rotation.w) &&
            (double)rotation.x * rotation.x + (double)rotation.y * rotation.y + (double)rotation.z * rotation.z + (double)rotation.w * rotation.w > .000001d;
        private static bool Finite(Vector3 point) => DefenseDamageRules.IsFinite(point.x) && DefenseDamageRules.IsFinite(point.y) && DefenseDamageRules.IsFinite(point.z);
        public bool Stop(DefenseEffectHandle handle)
        {
            if (!ReferenceEquals(handle.Owner, this)) return false;
            for (int i = 0; i < _active.Count; i++)
                if (_active[i].Handle.Token == handle.Token) { StopAt(i); return true; }
            return false;
        }
        private void StopAt(int index)
        {
            var entry = _active[index]; _active.RemoveAt(index);
            entry.Pool.TryReturn(entry.Lease);
        }
        public void StopOwner(Component owner)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                if (ReferenceEquals(_active[i].Owner, owner) || (owner != null && _active[i].Owner != null &&
                    _active[i].Owner.transform.IsChildOf(owner.transform))) StopAt(i);
        }
        private void OnGate(bool active)
        { foreach (var entry in _active) if (entry.Lease.Item != null) entry.Lease.Item.SetPaused(!active); }
        public void Clear()
        {
            if (_runner != null) { StopCoroutine(_runner); _runner = null; }
            while (_active.Count > 0) StopAt(_active.Count - 1);
        }
        public void Unbind()
        {
            if (_gameplay != null) { _gameplay.RunCleanup -= Clear; _gameplay.GameplayGateChanged -= OnGate; }
            _gameplay = null;
            Clear();
            foreach (var pool in _pools.Values) pool.Dispose();
            _pools.Clear();
            if (_parking != null) { StopCoroutine(_parking); _parking = null; }
            _pendingReturns.Clear();
            foreach (var group in _storageGroups) if (group != null) DefensePoolObjects.Destroy(group);
            _storageGroups.Clear(); _roots = null;
        }
        private void OnDisable()
        {
            Clear();
            if (_parking != null) { StopCoroutine(_parking); _parking = null; }
        }
        private void OnDestroy() => Unbind();
    }
}

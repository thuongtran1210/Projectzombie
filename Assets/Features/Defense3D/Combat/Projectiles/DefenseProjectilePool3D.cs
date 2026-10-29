using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Bộ quản lý Object Pool đạn đạo tập trung (Centralized Projectile Pool).
    /// Tuân thủ quy tắc 60 FPS Mobile:
    /// - Sử dụng UnityEngine.Pool.ObjectPool để tái sử dụng toàn bộ ProjectileCore3D.
    /// - 0 Instantiate / 0 Destroy trong quá trình giao tranh.
    /// - Tự động thu hồi và dọn dẹp khi Reset / Cleanup phiên chơi (triệt tiêu lỗi Phantom Damage).
    /// </summary>
    public class DefenseProjectilePool3D : DefenseGameplayBehaviour, IDefenseProjectileLauncher
    {
        [Header("Default Projectile Settings")]
        [SerializeField] private GameObject _defaultProjectilePrefab;
        [SerializeField, Min(16)] private int _defaultPoolCapacity = 32;
        [SerializeField, Min(32)] private int _maxPoolSize = 128;

        private readonly Dictionary<GameObject, ObjectPool<ProjectileCore3D>> _poolsByPrefab = new Dictionary<GameObject, ObjectPool<ProjectileCore3D>>();
        private readonly List<ProjectileCore3D> _activeProjectiles = new List<ProjectileCore3D>(64);
        private Transform _poolRoot;

        private void Awake()
        {
            var rootObj = new GameObject("[PROJECTILE_POOL_CONTAINER]");
            rootObj.transform.SetParent(transform);
            _poolRoot = rootObj.transform;

            if (_defaultProjectilePrefab != null)
            {
                GetOrCreatePool(_defaultProjectilePrefab);
            }
        }

        /// <summary>
        /// Lấy hoặc khởi tạo ObjectPool tương ứng với một Prefab Đạn.
        /// </summary>
        public ObjectPool<ProjectileCore3D> GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null) return null;

            if (!_poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<ProjectileCore3D>(
                    createFunc: () =>
                    {
                        var instance = Instantiate(prefab, _poolRoot);
                        if (!instance.TryGetComponent(out ProjectileCore3D core))
                        {
                            core = instance.AddComponent<ProjectileCore3D>();
                        }
                        instance.SetActive(false);
                        return core;
                    },
                    actionOnGet: (core) =>
                    {
                        _activeProjectiles.Add(core);
                    },
                    actionOnRelease: (core) =>
                    {
                        _activeProjectiles.Remove(core);
                        if (core != null)
                        {
                            core.gameObject.SetActive(false);
                        }
                    },
                    actionOnDestroy: (core) =>
                    {
                        if (core != null)
                        {
                            Destroy(core.gameObject);
                        }
                    },
                    collectionCheck: false,
                    defaultCapacity: _defaultPoolCapacity,
                    maxSize: _maxPoolSize
                );

                _poolsByPrefab[prefab] = pool;
            }

            return pool;
        }

        /// <summary>
        /// Phóng một viên đạn với đầy đủ cấu hình từ Prefab và danh sách Attack Effects.
        /// </summary>
        public bool LaunchProjectile(
            GameObject prefab,
            DefenseProjectileLaunchRequest request,
            IReadOnlyList<SO_AttackEffect> effects)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled) return false;

            var targetPrefab = prefab != null ? prefab : _defaultProjectilePrefab;
            if (targetPrefab == null) return false;

            var pool = GetOrCreatePool(targetPrefab);
            if (pool == null) return false;

            var projectile = pool.Get();
            if (projectile == null) return false;

            projectile.Launch(
                source: request.Source,
                origin: request.Origin,
                target: request.Target,
                damage: request.Damage,
                speed: request.Speed,
                arcHeight: request.ArcHeight,
                effects: effects,
                runGeneration: request.RunGeneration,
                sessionIdentity: request.SessionIdentity,
                gameplay: Gameplay,
                returnToPool: (core) => pool.Release(core)
            );

            return true;
        }

        /// <summary>
        /// Thực thi giao diện IDefenseProjectileLauncher chuẩn.
        /// </summary>
        public bool TryLaunch(DefenseProjectileLaunchRequest request)
        {
            return LaunchProjectile(_defaultProjectilePrefab, request, null);
        }

        /// <summary>
        /// Nạp trước (Pre-warm) một số lượng đạn vào Pool để tránh giật khung hình frame đầu.
        /// </summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0) return;

            var pool = GetOrCreatePool(prefab);
            var temp = new ProjectileCore3D[count];

            for (int i = 0; i < count; i++)
            {
                temp[i] = pool.Get();
            }

            for (int i = 0; i < count; i++)
            {
                pool.Release(temp[i]);
            }
        }

        private void RecallAllActive()
        {
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = _activeProjectiles[i];
                if (proj != null)
                {
                    proj.Despawn();
                }
            }
            _activeProjectiles.Clear();
        }

        protected override void OnGameplayCleanup() => RecallAllActive();
        protected override void OnGameplayReset() => RecallAllActive();
        private void OnDisable() => RecallAllActive();
    }
}

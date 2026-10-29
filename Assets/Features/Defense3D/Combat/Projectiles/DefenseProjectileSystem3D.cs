using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Hệ thống quản lý và mô phỏng Đạn Đạo 3D tập trung (Centralized Projectile System).
    /// Tuân thủ quy chuẩn 60 FPS Mobile:
    /// 1. Sử dụng Struct-based Buffer cố định, 0 GC Allocations trong Update loop.
    /// 2. Áp dụng Strategy Pattern cho Quỹ đạo bay (IProjectileTrajectory) và Sát thương (IProjectilePayload).
    /// 3. Hỗ trợ SessionIdentity và RunGeneration để triệt tiêu lỗi Phantom Damage khi Restart.
    /// 4. Hỗ trợ cả đạn Homing (bám mục tiêu), Directional (bắn thẳng) và Ballistic (cầu vồng rơi đất).
    /// </summary>
    public sealed class DefenseProjectileSystem3D : DefenseGameplayBehaviour, IDefenseProjectileLauncher
    {
        [Header("System Configuration")]
        [SerializeField, Min(8)] private int _capacity = 64;
        [SerializeField] private DefenseProjectileDefinitionSO _defaultDefinition;

        private struct ActiveProjectile
        {
            public bool IsActive;
            public IDefenseEnemyTarget3D Target;
            public DefenseActorLifetime Lifetime;
            public DefenseEnemy3D Enemy;
            public Component Source;
            public Vector3 StartPos;
            public Vector3 EndPos;
            public float LaunchTime;
            public float FlightDuration;
            public float Damage;
            public float ArcHeight;
            public float AoERadius;
            public int RunGeneration;
            public object SessionIdentity;
            public IProjectileTrajectory Trajectory;
            public IProjectilePayload Payload;
            public Transform VisualInstance;
        }

        private ActiveProjectile[] _pool;
        private Transform _projectileRoot;

        private void Awake()
        {
            _pool = new ActiveProjectile[_capacity];
            var rootObj = new GameObject("[PROJECTILE_POOL_ROOT]");
            rootObj.transform.SetParent(transform);
            _projectileRoot = rootObj.transform;
            ClearAll();
        }

        public bool TryLaunch(DefenseProjectileLaunchRequest request)
        {
            return TryLaunchWithStrategy(
                request,
                ParabolicHomingTrajectory.Instance,
                SingleTargetDamagePayload.Instance);
        }

        public bool TryLaunchWithDefinition(DefenseProjectileLaunchRequest request, DefenseProjectileDefinitionSO definition)
        {
            var def = definition != null ? definition : _defaultDefinition;
            var trajectory = def != null ? def.GetTrajectory() : ParabolicHomingTrajectory.Instance;
            var payload = def != null ? def.GetPayload() : SingleTargetDamagePayload.Instance;
            float arcHeight = def != null ? def.ArcHeight : request.ArcHeight;
            float aoeRadius = def != null ? def.AoERadius : 0f;

            return TryLaunchInternal(request, trajectory, payload, arcHeight, aoeRadius);
        }

        public bool TryLaunchWithStrategy(
            DefenseProjectileLaunchRequest request,
            IProjectileTrajectory trajectory,
            IProjectilePayload payload)
        {
            return TryLaunchInternal(request, trajectory, payload, request.ArcHeight, 0f);
        }

        private bool TryLaunchInternal(
            DefenseProjectileLaunchRequest request,
            IProjectileTrajectory trajectory,
            IProjectilePayload payload,
            float arcHeight,
            float aoeRadius)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || _pool == null) return false;

            // Tìm slot trống trong buffer
            for (int i = 0; i < _pool.Length; i++)
            {
                if (_pool[i].IsActive) continue;

                Vector3 startPos = request.Origin;
                Vector3 endPos = request.TargetPosition;
                if (request.Target != null && request.Target.TargetTransform != null)
                {
                    endPos = request.Target.TargetTransform.position;
                }

                float distance = Vector3.Distance(startPos, endPos);
                float speed = Mathf.Max(1f, request.Speed);
                float duration = Mathf.Clamp(distance / speed, 0.05f, 5f);

                var enemy = request.Target as DefenseEnemy3D;

                _pool[i] = new ActiveProjectile
                {
                    IsActive = true,
                    Target = request.Target,
                    Enemy = enemy,
                    Lifetime = enemy != null ? enemy.Lifetime : null,
                    Source = request.Source,
                    StartPos = startPos,
                    EndPos = endPos,
                    LaunchTime = GameplayTime,
                    FlightDuration = duration,
                    Damage = request.Damage,
                    ArcHeight = arcHeight,
                    AoERadius = aoeRadius,
                    RunGeneration = request.RunGeneration,
                    SessionIdentity = request.SessionIdentity,
                    Trajectory = trajectory ?? ParabolicHomingTrajectory.Instance,
                    Payload = payload ?? SingleTargetDamagePayload.Instance,
                    VisualInstance = null
                };

                return true;
            }

            return false;
        }

        private bool IsValid(ref ActiveProjectile proj)
        {
            if (!proj.IsActive) return false;
            if (proj.RunGeneration != Gameplay.RunGeneration) return false;
            if (!ReferenceEquals(proj.SessionIdentity, Gameplay.SessionIdentity)) return false;

            // Nếu mục tiêu đã bị tiêu diệt giữa đường bay đối với homing target
            if (proj.Trajectory.Type == DefenseTrajectoryType.ParabolicHoming)
            {
                if (proj.Target == null || proj.Target.TargetTransform == null || !proj.Target.IsTargetable)
                {
                    return false;
                }

                if (proj.Lifetime != null && proj.Enemy != null)
                {
                    if (!ReferenceEquals(proj.Enemy.Lifetime, proj.Lifetime) || !proj.Lifetime.IsCurrent(Gameplay))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || _pool == null) return;

            for (int i = 0; i < _pool.Length; i++)
            {
                if (!_pool[i].IsActive) continue;

                if (!IsValid(ref _pool[i]))
                {
                    Release(i);
                    continue;
                }

                float elapsed = GameplayTime - _pool[i].LaunchTime;
                float progress = Mathf.Clamp01(elapsed / _pool[i].FlightDuration);

                // Cập nhật vị trí đích nếu mục tiêu đang di chuyển
                Vector3 currentEnd = _pool[i].EndPos;
                if (_pool[i].Target != null && _pool[i].Target.TargetTransform != null)
                {
                    currentEnd = _pool[i].Target.TargetTransform.position + Vector3.up * 0.65f;
                    _pool[i].EndPos = currentEnd;
                }

                Vector3 currentPos = _pool[i].Trajectory.EvaluatePosition(
                    _pool[i].StartPos,
                    currentEnd,
                    progress,
                    _pool[i].ArcHeight);

                Vector3 currentDir = _pool[i].Trajectory.EvaluateDirection(
                    _pool[i].StartPos,
                    currentEnd,
                    progress,
                    _pool[i].ArcHeight);

                // Cập nhật Visual Instance nếu có
                if (_pool[i].VisualInstance != null)
                {
                    _pool[i].VisualInstance.position = currentPos;
                    if (currentDir.sqrMagnitude > 0.001f)
                    {
                        _pool[i].VisualInstance.rotation = Quaternion.LookRotation(currentDir);
                    }
                }

                // Khi đạn đến đích
                if (progress >= 1f)
                {
                    var projCopy = _pool[i];
                    Release(i);

                    // Thực thi Payload sát thương
                    projCopy.Payload.Execute(
                        impactPoint: currentPos,
                        directTarget: projCopy.Target,
                        damage: projCopy.Damage,
                        source: projCopy.Source,
                        generation: projCopy.RunGeneration,
                        session: projCopy.SessionIdentity,
                        gameplay: Gameplay,
                        aoeRadius: projCopy.AoERadius);
                }
            }
        }

        private void Release(int index)
        {
            if (index < 0 || index >= _pool.Length) return;
            if (_pool[index].VisualInstance != null)
            {
                _pool[index].VisualInstance.gameObject.SetActive(false);
            }
            _pool[index] = default;
        }

        private void ClearAll()
        {
            if (_pool == null) return;
            for (int i = 0; i < _pool.Length; i++)
            {
                Release(i);
            }
        }

        protected override void OnGameplayCleanup() => ClearAll();
        protected override void OnGameplayReset() => ClearAll();
        private void OnDisable() => ClearAll();
    }
}

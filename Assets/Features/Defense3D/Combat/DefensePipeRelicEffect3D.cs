using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Kỹ năng Pháp Bảo: Điếu Cày Khói Lửa (Pipe of Dragon Smoke).
    /// Thổi ra luồng bão khói thuốc lào mù mịt bao phủ trận địa:
    /// - Phase 1: Nổ sóng khói chấn động, gây sát thương va đập ban đầu và làm quái vật sặc ho choáng váng (Stun).
    /// - Phase 2: Duy trì vùng khói độc bát quái (Smoke Hazard Zone), làm chậm tốc độ chạy (Slow 60%)
    ///   và rút máu thiêu đốt định kỳ (Damage over Time - DoT) đối với mọi yêu ma đứng trong hoặc bước vào vùng khói.
    /// - Phase 3: Tiêu biến an toàn, dọn dẹp sạch tài nguyên (0 GC Allocations).
    /// </summary>
    public sealed class DefensePipeRelicEffect3D : DefenseSkillEffect3D
    {
        private const int MAX_ACTIVE_ZONES = 4;
        private const int TARGET_BUFFER_CAPACITY = 256;

        [Header("Khống Chế & Hiệu Ứng Bất Lợi")]
        private float _slowStrength = 0.6f;
        private float _coughStunDuration = 0.6f;

        [Header("Cơ Chế Sát Thương & Vùng Khói")]
        private float _tickInterval = 0.5f;
        private float _initialBurstRatio = 0.5f;
        private float _tickDamageRatio = 0.15f;

        private IDefenseEffectService _effects;
        private DefenseEffectProfileSO _smokeProfile;
        private DefenseHeroHealth3D _health;

        private struct ActiveSmokeZone
        {
            public bool IsActive;
            public Vector3 Center;
            public float Radius;
            public float Duration;
            public float ElapsedTime;
            public float StartedAt;
            public float NextTickTime;
            public float DamagePerTick;
            public DefenseTargetMask TargetMask;
            public int RunGeneration;
            public object SessionIdentity;
            public DefenseEffectHandle Visual;
        }

        private readonly ActiveSmokeZone[] _zones = new ActiveSmokeZone[MAX_ACTIVE_ZONES];
        private readonly IDefenseTargetPosition[] _targets = new IDefenseTargetPosition[TARGET_BUFFER_CAPACITY];

        public void ConfigureTuning(float slow,float stun,float tickInterval,float burstRatio,float tickRatio)
        {_slowStrength=slow;_coughStunDuration=stun;_tickInterval=tickInterval;_initialBurstRatio=burstRatio;_tickDamageRatio=tickRatio;}

        public override DefenseSkillKind Kind => DefenseSkillKind.Relic;

        public override bool IsConfigured => true;

        /// <summary>Injects the session-owned VFX pool; missing presentation never blocks gameplay.</summary>
        public void BindPresentation(IDefenseEffectService effects, DefenseEffectProfileSO profile)
        {
            CancelAllZones();
            _effects = effects;
            _smokeProfile = profile;
            if (effects is DefenseEffectService3D service && profile != null && profile.IsValid)
                service.Prewarm(profile, 1);
        }

        /// <summary>Remaining lifetime of the newest active hazard, used by the passive skill HUD.</summary>
        public float RemainingSmokeSeconds
        {
            get
            {
                float remaining = 0f;
                for (int i = 0; i < MAX_ACTIVE_ZONES; i++)
                    if (_zones[i].IsActive) remaining = Mathf.Max(remaining, _zones[i].Duration - _zones[i].ElapsedTime);
                return remaining;
            }
        }

        public float SmokeDuration => Definition != null ? Definition.Duration : 0f;

        public float SlowStrength
        {
            get => _slowStrength;
            set => _slowStrength = Mathf.Clamp01(value);
        }

        public float CoughStunDuration
        {
            get => _coughStunDuration;
            set => _coughStunDuration = Mathf.Max(0f, value);
        }

        public float TickInterval
        {
            get => _tickInterval;
            set => _tickInterval = Mathf.Max(0.05f, value);
        }

        public float InitialBurstRatio
        {
            get => _initialBurstRatio;
            set => _initialBurstRatio = Mathf.Max(0f, value);
        }

        public float TickDamageRatio
        {
            get => _tickDamageRatio;
            set => _tickDamageRatio = Mathf.Max(0f, value);
        }

        public int ActiveZoneCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < MAX_ACTIVE_ZONES; i++)
                {
                    if (_zones[i].IsActive) count++;
                }
                return count;
            }
        }

        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            prepared = requested;
            return isActiveAndEnabled && CanAdvanceGameplay && Definition != null &&
                   Gameplay.Targets is IDefenseAreaTargetQuery;
        }

        public override void Execute(DefenseResolvedAim prepared)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || Definition == null) return;

            float radius = Definition.Radius > 0f ? Definition.Radius : 4f;
            float baseDamage = Definition.Damage;
            float duration = Definition.Duration > 0f ? Definition.Duration : 4.5f;
            DefenseTargetMask targetMask = Definition.TargetMask;

            Vector3 centerPos = new Vector3(prepared.Target.X, prepared.Target.Y, prepared.Target.Z);
            int generation = Gameplay.RunGeneration;
            object sessionIdentity = Gameplay.SessionIdentity;

            // 1. Phase 1: Sóng xung kích khói nóng ban đầu (Burst Damage & Cough Stun)
            ResolveInitialBlast(prepared.Target, centerPos, radius, baseDamage * _initialBurstRatio, targetMask, generation, sessionIdentity);

            // 2. Phase 2: Kích hoạt hiệu ứng hình ảnh khói thuốc lào rồng cuộn
            DefenseEffectHandle visual = _effects != null && _smokeProfile != null
                ? _effects.Play(_smokeProfile, centerPos + Vector3.up * .07f, radius, this) : default;

            // 3. Khởi tạo vùng khói độc duy trì (Smoke Hazard Zone Ticking)
            RegisterActiveZone(centerPos, radius, duration, baseDamage * _tickDamageRatio, targetMask, generation, sessionIdentity, visual);
        }

        private void ResolveInitialBlast(DefensePoint3 targetPoint, Vector3 centerPos, float radius, float burstDamage, DefenseTargetMask targetMask, int generation, object sessionIdentity)
        {
            if (!(Gameplay.Targets is IDefenseAreaTargetQuery area)) return;

            if (area.TryCopyWithin(targetPoint, radius, targetMask, _targets, out int count))
            {
                var damageContext = new DefenseDamageContext(
                    burstDamage,
                    this,
                    targetPoint,
                    isCritical: false,
                    runGeneration: generation,
                    sessionIdentity: sessionIdentity);

                for (int i = 0; i < count; i++)
                {
                    var target = _targets[i];
                    if (target != null && target.IsAvailable && target.Identity is IDefenseEnemyTarget3D enemy)
                    {
                        enemy.TakeDamage(damageContext);

                        if (enemy is IDefenseStatusReceiver receiver)
                        {
                            // Gây choáng sặc khói (Cough Stun) đợt đầu
                            if (_coughStunDuration > 0f)
                            {
                                receiver.TryApplyStatus(new DefenseStatusRequest(
                                    DefenseStatusKind.Stun,
                                    _coughStunDuration,
                                    1f,
                                    default,
                                    sessionIdentity,
                                    generation));
                            }

                            // Áp dụng làm chậm (Slow) ngay lập tức
                            if (_slowStrength > 0f)
                            {
                                receiver.TryApplyStatus(new DefenseStatusRequest(
                                    DefenseStatusKind.Slow,
                                    _tickInterval * 2.0f,
                                    _slowStrength,
                                    default,
                                    sessionIdentity,
                                    generation));
                            }
                        }
                    }
                }
            }
        }

        private void RegisterActiveZone(Vector3 center, float radius, float duration, float damagePerTick, DefenseTargetMask targetMask, int generation, object sessionIdentity, DefenseEffectHandle visual)
        {
            // Tìm slot trống trong mảng vùng khói
            int slot = -1;
            for (int i = 0; i < MAX_ACTIVE_ZONES; i++)
            {
                if (!_zones[i].IsActive)
                {
                    slot = i;
                    break;
                }
            }

            // Nếu đầy slot, ghi đè slot cũ nhất
            if (slot == -1)
            {
                slot = 0;
                CleanupZone(ref _zones[0]);
            }

            _zones[slot] = new ActiveSmokeZone
            {
                IsActive = true,
                Center = center,
                Radius = radius,
                Duration = duration,
                ElapsedTime = 0f,
                StartedAt = GameplayTime,
                NextTickTime = _tickInterval,
                DamagePerTick = damagePerTick,
                TargetMask = targetMask,
                RunGeneration = generation,
                SessionIdentity = sessionIdentity,
                Visual = visual
            };
        }

        private void Update()
        {
            if (!CanAdvanceGameplay)
            {
                return;
            }

            for (int i = 0; i < MAX_ACTIVE_ZONES; i++)
            {
                if (!_zones[i].IsActive) continue;

                ref var zone = ref _zones[i];

                // Kiểm tra tính hợp lệ của thế hệ trận đấu
                if (zone.RunGeneration != Gameplay.RunGeneration || !ReferenceEquals(zone.SessionIdentity, Gameplay.SessionIdentity))
                {
                    CleanupZone(ref zone);
                    continue;
                }

                zone.ElapsedTime = Mathf.Max(0f, GameplayTime - zone.StartedAt);

                // Xử lý tick sát thương DoT và duy trì Slow định kỳ
                if (zone.ElapsedTime >= zone.NextTickTime)
                {
                    zone.NextTickTime += _tickInterval;
                    TickSmokeZone(ref zone);
                }

                // Kết thúc thời gian tồn tại của vùng khói
                if (zone.ElapsedTime >= zone.Duration)
                {
                    CleanupZone(ref zone);
                }
            }
        }

        private void TickSmokeZone(ref ActiveSmokeZone zone)
        {
            if (!(Gameplay.Targets is IDefenseAreaTargetQuery area)) return;

            var point = new DefensePoint3(zone.Center.x, zone.Center.y, zone.Center.z);
            if (!area.TryCopyWithin(point, zone.Radius, zone.TargetMask, _targets, out int count)) return;

            var damageContext = new DefenseDamageContext(
                zone.DamagePerTick,
                this,
                point,
                isCritical: false,
                runGeneration: zone.RunGeneration,
                sessionIdentity: zone.SessionIdentity);

            for (int i = 0; i < count; i++)
            {
                var target = _targets[i];
                if (target != null && target.IsAvailable && target.Identity is IDefenseEnemyTarget3D enemy)
                {
                    enemy.TakeDamage(damageContext);

                    // Liên tục làm mới hiệu ứng làm chậm cho quái vật còn đứng trong hoặc mới bước vào vùng khói
                    if (_slowStrength > 0f && enemy is IDefenseStatusReceiver receiver)
                    {
                        receiver.TryApplyStatus(new DefenseStatusRequest(
                            DefenseStatusKind.Slow,
                            _tickInterval * 1.8f,
                            _slowStrength,
                            default,
                            zone.SessionIdentity,
                            zone.RunGeneration));
                    }
                }
            }
        }

        private void CleanupZone(ref ActiveSmokeZone zone)
        {
            if (zone.Visual.IsAssigned) _effects?.Stop(zone.Visual);
            zone.Visual = default;
            zone.IsActive = false;
        }

        private void CancelAllZones()
        {
            for (int i = 0; i < MAX_ACTIVE_ZONES; i++)
            {
                if (_zones[i].IsActive)
                {
                    CleanupZone(ref _zones[i]);
                }
            }
            Array.Clear(_targets, 0, TARGET_BUFFER_CAPACITY);
        }

        protected override void OnGameplayBound()
        {
            CancelAllZones();
            if (_health != null) _health.Defeated -= CancelAllZones;
            TryGetComponent(out _health);
            if (_health != null) _health.Defeated += CancelAllZones;
        }

        protected override void OnGameplayCleanup() => CancelAllZones();

        protected override void OnGameplayReset() => CancelAllZones();

        private void OnDisable() => CancelAllZones();

        protected override void OnDestroy()
        {
            CancelAllZones();
            if (_health != null) _health.Defeated -= CancelAllZones;
            base.OnDestroy();
        }
    }
}

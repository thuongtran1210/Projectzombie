using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Enemies.StatusHandlers;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Enum định nghĩa các loại hiệu ứng bất lợi (Debuff/Status Effect) lên Enemy (GDD v5.1 Slapstick Update).
    /// </summary>
    public enum StatusEffectType
    {
        Slow,           // Làm chậm (Ví dụ: Bát Quái Trận / Đạn Băng)
        Freeze,         // Đóng băng/Cố định hoàn toàn (Ví dụ: Tuyệt kỹ Vong Xuyên / Băng Bội)
        Stun,           // Choáng (Ví dụ: Choáng do đòn chém nặng / Lực bùng nổ)
        Burn,           // Thiêu đốt gây sát thương theo thời gian DoT (Ví dụ: Flamethrower / Hỏa Cầu)
        Humiliated,     // Quê Độ: Ôm mặt xấu hổ, có 40% tỉ lệ quay sang đấm quái bên cạnh (Dép Tổ Ong)
        Sleeping,       // Ngủ Say: Bất động, đòn đánh thức nhận 2.0x Damage (Chiếu Trải Hoàng Tuyền)
        Stoned,         // Say Thuốc Lào: Đi loạng choạng zíc zắc giật lùi, hết giờ nổ ho sặc sụa (Điếu Cày)
        Dancing,        // Mê Nhảy Múa: Dừng tấn công, nhảy theo nhịp, làm bia đỡ đạn (Loa Phường / Trống Đồng)
        RagdollFlight,  // Bị Bắn / Trượt Ngã: Bay parabol xoay vòng, nổ sát thương chuỗi khi chạm đất/tường
        ChickenPolymorph // Hóa Gà Con: Biến thành gà con chibi, mất khả năng tấn công, nhận thêm +50% dmg (Chổi Lông Gà)
    }

    /// <summary>
    /// Cấu trúc lưu trữ dữ liệu của 1 hiệu ứng trạng thái đang áp dụng lên Enemy.
    /// </summary>
    public class ActiveStatusEffect
    {
        public StatusEffectType Type;
        public float Duration;
        public float ElapsedTime;
        public float Value; // Tỉ lệ làm chậm / Sát thương DoT / Lực đấm đồng minh
        public float TickInterval;
        public float NextTickTime;
        public Action<float> OnTickDamage;

        public bool IsExpired => ElapsedTime >= Duration;
    }

    /// <summary>
    /// Component điều phối các Trạng thái bất lợi Slapstick/Fun trên Enemy.
    /// Triển khai Strategy Pattern & Decoupling, ủy quyền vật lý cho EnemyKinematicPhysics và hành vi cho IStatusEffectHandler.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyStatusController : MonoBehaviour, ProjectZombie.Features.Shared.IStatusReceiver
    {
        private Enemy _enemy;
        private EnemyKinematicPhysics _physics;
        private readonly List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();

        // Cache các Strategy Handlers tĩnh (Zero-GC)
        private static readonly Dictionary<StatusEffectType, IStatusEffectHandler> _handlers = new Dictionary<StatusEffectType, IStatusEffectHandler>
        {
            { StatusEffectType.Burn, new BurnStatusHandler() },
            { StatusEffectType.Humiliated, new HumiliatedStatusHandler() },
            { StatusEffectType.Stoned, new StonedStatusHandler() },
            { StatusEffectType.Sleeping, new SleepingStatusHandler() },
            { StatusEffectType.ChickenPolymorph, new ChickenPolymorphStatusHandler() }
        };

        // Cumulative Modifiers
        public float CurrentSlowMultiplier { get; private set; } = 1f;
        public bool IsStunned { get; private set; } = false;
        public bool IsFrozen { get; private set; } = false;
        public bool IsHumiliated { get; private set; } = false;
        public bool IsSleeping { get; private set; } = false;
        public bool IsStoned { get; private set; } = false;
        public bool IsDancing { get; private set; } = false;
        public bool IsChickenPolymorphed { get; private set; } = false;
        public bool IsRagdollActive => _physics != null && _physics.IsRagdollActive;

        public bool CanMove => !IsStunned && !IsFrozen && !IsSleeping && !IsDancing && (_physics == null || _physics.CanMovePhysics);
        public bool CanAttack => !IsStunned && !IsFrozen && !IsSleeping && !IsDancing && !IsHumiliated && !IsChickenPolymorphed && (_physics == null || !_physics.IsRagdollActive);

        /// <summary>
        /// Chỉ số kháng khống chế lấy trực tiếp từ Enemy (Common: 0%, Elite: 30%, Boss: 70%).
        /// </summary>
        public float Tenacity => _enemy != null ? _enemy.Tenacity : 0f;

        /// <summary>
        /// Sự kiện phát ra khi trạng thái bất lợi thay đổi (phục vụ VFX/UI icons trên đầu quái).
        /// </summary>
        public event Action<StatusEffectType, bool> OnStatusChanged;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _physics = GetComponent<EnemyKinematicPhysics>() ?? gameObject.AddComponent<EnemyKinematicPhysics>();
            _physics.OnRagdollEnded += HandleRagdollEnded;
        }

        private void OnDestroy()
        {
            if (_physics != null)
            {
                _physics.OnRagdollEnded -= HandleRagdollEnded;
            }
        }

        private void HandleRagdollEnded()
        {
            RemoveStatus(StatusEffectType.RagdollFlight);
        }

        private void OnEnable()
        {
            ClearAllStatuses();
        }

        private void OnDisable()
        {
            ClearAllStatuses();
        }

        /// <summary>
        /// Xóa sạch mọi hiệu ứng và gọi hàm phục hồi Handler khi quái chết / vào Object Pool.
        /// </summary>
        public void ClearAllStatuses()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                if (_handlers.TryGetValue(effect.Type, out var handler) && handler != null)
                {
                    handler.OnRemoved(_enemy, effect);
                }
                OnStatusChanged?.Invoke(effect.Type, false);
            }
            _activeEffects.Clear();
            CurrentSlowMultiplier = 1f;
            IsStunned = false;
            IsFrozen = false;
            IsHumiliated = false;
            IsSleeping = false;
            IsStoned = false;
            IsDancing = false;
            IsChickenPolymorphed = false;
        }

        /// <summary>
        /// Gây hiệu ứng Đẩy lùi (Knockback) lên kẻ địch.
        /// </summary>
        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            if (_physics != null)
            {
                _physics.ApplyKnockback(direction, force, duration);
            }
        }

        /// <summary>
        /// Phóng quái bay theo dạng Kinematic Parabolic Ragdoll (Slapstick Launch).
        /// </summary>
        public void ApplyRagdollLaunch(Vector2 direction, float speed, float duration, float impactDamage = 80f, float impactRadius = 2.5f)
        {
            if (IsImmuneTo(StatusEffectType.RagdollFlight)) return;

            if (_physics != null)
            {
                _physics.ApplyRagdollLaunch(direction, speed, duration, impactDamage, impactRadius);
                ApplyStatusEffect(StatusEffectType.RagdollFlight, duration);
            }
        }

        /// <summary>
        /// Kiểm tra xem quái có miễn nhiễm với trạng thái bất lợi này không.
        /// </summary>
        public bool IsImmuneTo(StatusEffectType type)
        {
            if (_enemy != null && _enemy.Config != null)
            {
                return _enemy.Config.IsImmuneTo(type);
            }
            return false;
        }

        /// <summary>
        /// Áp dụng hoặc làm mới một Status Effect (Slow, Freeze, Stun, Burn, Humiliated, Sleeping, Stoned, Dancing).
        /// Tự động kiểm tra Miễn Kháng (Immunity) và scale thời lượng theo chỉ số Tenacity (Kháng CC).
        /// </summary>
        public void ApplyStatusEffect(StatusEffectType type, float duration, float value = 0f, float tickInterval = 0.5f, Action<float> onTickDamage = null)
        {
            // 1. Kiểm tra Miễn Kháng (Immunity Check)
            if (IsImmuneTo(type)) return;

            // 2. Tính toán giảm thời gian theo chỉ số Kiên Cường (Tenacity)
            float effectiveDuration = duration * Mathf.Clamp01(1f - Tenacity);
            if (effectiveDuration <= 0.05f) return;

            var existing = _activeEffects.Find(e => e.Type == type);
            if (existing != null)
            {
                existing.Duration = Mathf.Max(existing.Duration, effectiveDuration);
                existing.ElapsedTime = 0f;
                existing.Value = Mathf.Max(existing.Value, value);
                if (onTickDamage != null) existing.OnTickDamage = onTickDamage;

                if (_handlers.TryGetValue(type, out var handler))
                {
                    handler.OnApplied(_enemy, existing);
                }
            }
            else
            {
                var newEffect = new ActiveStatusEffect
                {
                    Type = type,
                    Duration = effectiveDuration,
                    ElapsedTime = 0f,
                    Value = value,
                    TickInterval = tickInterval,
                    NextTickTime = Time.time + tickInterval,
                    OnTickDamage = onTickDamage
                };

                _activeEffects.Add(newEffect);

                if (_handlers.TryGetValue(type, out var handler))
                {
                    handler.OnApplied(_enemy, newEffect);
                }

                OnStatusChanged?.Invoke(type, true);

                if (type == StatusEffectType.Freeze)
                {
                    global::Core.Audio.AudioManager.Instance?.PlayStatusFreeze(transform.position);
                }
                else if (type == StatusEffectType.Burn)
                {
                    global::Core.Audio.AudioManager.Instance?.PlayStatusBurn(transform.position);
                }
            }

            RecalculateStatus();
        }

        public bool HasStatus(StatusEffectType type)
        {
            return _activeEffects.Exists(e => e.Type == type && !e.IsExpired);
        }

        public void RemoveStatus(StatusEffectType type)
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                if (_activeEffects[i].Type == type)
                {
                    var effect = _activeEffects[i];
                    if (_handlers.TryGetValue(type, out var handler))
                    {
                        handler.OnRemoved(_enemy, effect);
                    }
                    _activeEffects.RemoveAt(i);
                    OnStatusChanged?.Invoke(type, false);
                }
            }
            RecalculateStatus();
        }

        /// <summary>
        /// Hook xử lý sát thương nhận vào để kích hoạt Wake-up Crit khi đang ngủ.
        /// </summary>
        public float ProcessIncomingDamageMultiplier()
        {
            if (IsSleeping)
            {
                RemoveStatus(StatusEffectType.Sleeping);
                return 2.0f; // Thức giấc nhận x2 sát thương bạo kích
            }
            return 1.0f;
        }

        private void Update()
        {
            if (!ProjectZombie.Features.Shared.GameStateManager.IsPlaying) return;

            float dt = Time.deltaTime;

            if (_activeEffects.Count > 0)
            {
                bool dirty = false;
                for (int i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    var effect = _activeEffects[i];
                    effect.ElapsedTime += dt;

                    // Ủy quyền xử lý tick cho Strategy Handler
                    if (_handlers.TryGetValue(effect.Type, out var handler))
                    {
                        handler.OnTick(_enemy, effect, dt);
                    }

                    if (effect.IsExpired)
                    {
                        if (handler != null)
                        {
                            handler.OnExpired(_enemy, effect);
                        }

                        OnStatusChanged?.Invoke(effect.Type, false);
                        _activeEffects.RemoveAt(i);
                        dirty = true;
                    }
                }

                if (dirty)
                {
                    RecalculateStatus();
                }
            }
        }

        private void RecalculateStatus()
        {
            float maxSlow = 0f;
            bool isStun = false;
            bool isFreeze = false;
            bool isHumiliated = false;
            bool isSleeping = false;
            bool isStoned = false;
            bool isDancing = false;
            bool isChickenPolymorphed = false;

            for (int i = 0; i < _activeEffects.Count; i++)
            {
                var effect = _activeEffects[i];
                switch (effect.Type)
                {
                    case StatusEffectType.Slow:
                        if (effect.Value > maxSlow) maxSlow = effect.Value;
                        break;
                    case StatusEffectType.Stun:
                        isStun = true;
                        break;
                    case StatusEffectType.Freeze:
                        isFreeze = true;
                        break;
                    case StatusEffectType.Humiliated:
                        isHumiliated = true;
                        break;
                    case StatusEffectType.Sleeping:
                        isSleeping = true;
                        break;
                    case StatusEffectType.Stoned:
                        isStoned = true;
                        break;
                    case StatusEffectType.Dancing:
                        isDancing = true;
                        break;
                    case StatusEffectType.ChickenPolymorph:
                        isChickenPolymorphed = true;
                        break;
                }
            }

            CurrentSlowMultiplier = Mathf.Clamp01(1f - maxSlow);
            IsStunned = isStun;
            IsFrozen = isFreeze;
            IsHumiliated = isHumiliated;
            IsSleeping = isSleeping;
            IsStoned = isStoned;
            IsDancing = isDancing;
            IsChickenPolymorphed = isChickenPolymorphed;
        }

        /// <summary>
        /// Tính toán Tốc độ di chuyển thực tế của Enemy sau khi đã tính Slow/Stun/Freeze/Say thuốc.
        /// </summary>
        public float GetModifiedMoveSpeed(float baseSpeed)
        {
            if (!CanMove) return 0f;
            if (IsStoned) return baseSpeed * 0.55f; // Say thuốc chỉ bị làm chậm nhẹ 45% và lảo đảo bước đi
            return baseSpeed * CurrentSlowMultiplier;
        }
    }
}

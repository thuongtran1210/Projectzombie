using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Enum định nghĩa các loại hiệu ứng bất lợi (Debuff/Status Effect) lên Enemy.
    /// </summary>
    public enum StatusEffectType
    {
        Slow,       // Làm chậm (Ví dụ: Bát Quái Trận / Đạn Băng)
        Freeze,     // Đóng băng/Cố định hoàn toàn (Ví dụ: Tuyệt kỹ Vong Xuyên / Băng Bội)
        Stun,       // Choáng (Ví dụ: Choáng do đòn chém nặng / Lực bùng nổ)
        Burn        // Thiêu đốt gây sát thương theo thời gian DoT (Ví dụ: Flamethrower / Hỏa Cầu)
    }

    /// <summary>
    /// Cấu trúc lưu trữ dữ liệu của 1 hiệu ứng trạng thái đang áp dụng lên Enemy.
    /// </summary>
    public class ActiveStatusEffect
    {
        public StatusEffectType Type;
        public float Duration;
        public float ElapsedTime;
        public float Value; // Tỉ lệ làm chậm (vd: 0.3f = 30%) hoặc Sát thương mỗi tick DoT (vd: 5 dps)
        public float TickInterval;
        public float NextTickTime;
        public Action<float> OnTickDamage;

        public bool IsExpired => ElapsedTime >= Duration;
    }

    /// <summary>
    /// Component quản lý Knockback và các Trạng thái bất lợi (Slow, Freeze, Stun, Burn) trên Enemy.
    /// Hiện thực IStatusReceiver theo kiến trúc Event-Driven & Decoupling.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyStatusController : MonoBehaviour, ProjectZombie.Features.Shared.IStatusReceiver
    {
        private Enemy _enemy;
        private List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();

        // Knockback State
        private bool _isKnockbackActive = false;
        private Vector2 _knockbackVelocity;
        private float _knockbackDuration;
        private float _knockbackTimer;

        // Cumulative Modifiers
        public float CurrentSlowMultiplier { get; private set; } = 1f;
        public bool IsStunned { get; private set; } = false;
        public bool IsFrozen { get; private set; } = false;

        public bool CanMove => !IsStunned && !IsFrozen && !_isKnockbackActive;
        public bool CanAttack => !IsStunned && !IsFrozen;

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
        }

        private void OnEnable()
        {
            _activeEffects.Clear();
            _isKnockbackActive = false;
            CurrentSlowMultiplier = 1f;
            IsStunned = false;
            IsFrozen = false;
        }

        /// <summary>
        /// Gây hiệu ứng Đẩy lùi (Knockback) lên kẻ địch.
        /// Tự động kiểm tra miễn nhiễm của Boss hoặc Tenacity >= 90%.
        /// </summary>
        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            if (_enemy == null || _enemy.IsBoss || Tenacity >= 0.9f) return; // Boss miễn nhiễm Knockback

            _isKnockbackActive = true;
            _knockbackVelocity = direction.normalized * force;
            _knockbackDuration = duration;
            _knockbackTimer = 0f;
        }

        /// <summary>
        /// Áp dụng hoặc làm mới một Status Effect (Slow, Freeze, Stun, Burn).
        /// Tự động scale thời lượng theo chỉ số Tenacity (Kháng CC).
        /// </summary>
        public void ApplyStatusEffect(StatusEffectType type, float duration, float value = 0f, float tickInterval = 0.5f, Action<float> onTickDamage = null)
        {
            // Tính toán thời lượng thực tế sau khi đã giảm trừ bởi Tenacity
            float effectiveDuration = duration * Mathf.Clamp01(1f - Tenacity);
            if (effectiveDuration <= 0.05f) return; // Kháng hoàn toàn nếu thời gian hiệu lực quá nhỏ

            // Kiểm tra xem đã có effect cùng loại chưa để reset duration hoặc chồng stack
            var existing = _activeEffects.Find(e => e.Type == type);
            if (existing != null)
            {
                existing.Duration = Mathf.Max(existing.Duration, effectiveDuration);
                existing.ElapsedTime = 0f;
                existing.Value = Mathf.Max(existing.Value, value);
                if (onTickDamage != null) existing.OnTickDamage = onTickDamage;
            }
            else
            {
                _activeEffects.Add(new ActiveStatusEffect
                {
                    Type = type,
                    Duration = effectiveDuration,
                    ElapsedTime = 0f,
                    Value = value,
                    TickInterval = tickInterval,
                    NextTickTime = Time.time + tickInterval,
                    OnTickDamage = onTickDamage
                });
                OnStatusChanged?.Invoke(type, true);
            }

            RecalculateStatus();
        }

        public bool HasStatus(StatusEffectType type)
        {
            return _activeEffects.Exists(e => e.Type == type && !e.IsExpired);
        }

        public void RemoveStatus(StatusEffectType type)
        {
            int removed = _activeEffects.RemoveAll(e => e.Type == type);
            if (removed > 0)
            {
                OnStatusChanged?.Invoke(type, false);
                RecalculateStatus();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Xử lý Knockback Timer
            if (_isKnockbackActive)
            {
                _knockbackTimer += dt;
                if (_knockbackTimer >= _knockbackDuration)
                {
                    _isKnockbackActive = false;
                    _enemy.Rb.velocity = Vector2.zero;
                }
                else
                {
                    _enemy.Rb.velocity = Vector2.Lerp(_knockbackVelocity, Vector2.zero, _knockbackTimer / _knockbackDuration);
                }
            }

            // 2. Xử lý danh sách Status Effects
            if (_activeEffects.Count > 0)
            {
                bool dirty = false;
                for (int i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    var effect = _activeEffects[i];
                    effect.ElapsedTime += dt;

                    // Xử lý DoT (Burn, Poison, ...)
                    if (effect.Type == StatusEffectType.Burn && Time.time >= effect.NextTickTime)
                    {
                        effect.NextTickTime = Time.time + effect.TickInterval;
                        effect.OnTickDamage?.Invoke(effect.Value);
                    }

                    if (effect.IsExpired)
                    {
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

            foreach (var effect in _activeEffects)
            {
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
                }
            }

            CurrentSlowMultiplier = Mathf.Clamp01(1f - maxSlow);
            IsStunned = isStun;
            IsFrozen = isFreeze;
        }

        /// <summary>
        /// Tính toán Tốc độ di chuyển thực tế của Enemy sau khi đã tính Slow/Stun/Freeze.
        /// </summary>
        public float GetModifiedMoveSpeed(float baseSpeed)
        {
            if (!CanMove) return 0f;
            return baseSpeed * CurrentSlowMultiplier;
        }
    }
}

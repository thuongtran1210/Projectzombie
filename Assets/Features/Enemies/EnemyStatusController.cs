using System;
using System.Collections.Generic;
using UnityEngine;

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
        RagdollFlight   // Bị Bắn / Trượt Ngã: Bay parabol xoay vòng, nổ sát thương chuỗi khi chạm đất/tường
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
    /// Component quản lý Knockback, Kinematic Ragdoll và các Trạng thái bất lợi Slapstick/Fun trên Enemy.
    /// Hiện thực IStatusReceiver theo kiến trúc Event-Driven & Decoupling, đảm bảo Zero-Alloc.
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

        // Kinematic Ragdoll Launch State
        private bool _isRagdollActive = false;
        private Vector2 _ragdollVelocity;
        private float _ragdollDuration;
        private float _ragdollTimer;
        private float _ragdollImpactDamage;
        private float _ragdollImpactRadius;
        private float _ragdollRotationSpeed;

        // Cumulative Modifiers
        public float CurrentSlowMultiplier { get; private set; } = 1f;
        public bool IsStunned { get; private set; } = false;
        public bool IsFrozen { get; private set; } = false;
        public bool IsHumiliated { get; private set; } = false;
        public bool IsSleeping { get; private set; } = false;
        public bool IsStoned { get; private set; } = false;
        public bool IsDancing { get; private set; } = false;
        public bool IsRagdollActive => _isRagdollActive;

        public bool CanMove => !IsStunned && !IsFrozen && !IsSleeping && !IsDancing && !_isKnockbackActive && !_isRagdollActive;
        public bool CanAttack => !IsStunned && !IsFrozen && !IsSleeping && !IsDancing && !IsHumiliated && !_isRagdollActive;

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
            _isRagdollActive = false;
            CurrentSlowMultiplier = 1f;
            IsStunned = false;
            IsFrozen = false;
            IsHumiliated = false;
            IsSleeping = false;
            IsStoned = false;
            IsDancing = false;
        }

        /// <summary>
        /// Gây hiệu ứng Đẩy lùi (Knockback) lên kẻ địch.
        /// </summary>
        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            if (_enemy == null || _enemy.IsBoss || Tenacity >= 0.9f) return;

            _isKnockbackActive = true;
            _knockbackVelocity = direction.normalized * force;
            _knockbackDuration = duration;
            _knockbackTimer = 0f;
        }

        /// <summary>
        /// Phóng quái bay theo dạng Kinematic Parabolic Ragdoll (Slapstick Launch).
        /// Khi rơi xuống hoặc va vào tường sẽ gây sát thương chuỗi lên quái khác.
        /// </summary>
        public void ApplyRagdollLaunch(Vector2 direction, float speed, float duration, float impactDamage = 80f, float impactRadius = 2.5f)
        {
            if (_enemy == null || _enemy.IsBoss || Tenacity >= 0.8f) return;

            _isRagdollActive = true;
            _ragdollVelocity = direction.normalized * speed;
            _ragdollDuration = duration;
            _ragdollTimer = 0f;
            _ragdollImpactDamage = impactDamage;
            _ragdollImpactRadius = impactRadius;
            _ragdollRotationSpeed = UnityEngine.Random.Range(540f, 1080f) * (UnityEngine.Random.value > 0.5f ? 1f : -1f);

            ApplyStatusEffect(StatusEffectType.RagdollFlight, duration);
        }

        /// <summary>
        /// Áp dụng hoặc làm mới một Status Effect (Slow, Freeze, Stun, Burn, Humiliated, Sleeping, Stoned, Dancing).
        /// Tự động scale thời lượng theo chỉ số Tenacity (Kháng CC).
        /// </summary>
        public void ApplyStatusEffect(StatusEffectType type, float duration, float value = 0f, float tickInterval = 0.5f, Action<float> onTickDamage = null)
        {
            float effectiveDuration = duration * Mathf.Clamp01(1f - Tenacity);
            if (effectiveDuration <= 0.05f) return;

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

            // 2. Xử lý Kinematic Ragdoll Launch (Bay xoay tròn)
            if (_isRagdollActive)
            {
                _ragdollTimer += dt;
                transform.Rotate(0f, 0f, _ragdollRotationSpeed * dt);

                if (_ragdollTimer >= _ragdollDuration)
                {
                    EndRagdollImpact();
                }
                else
                {
                    _enemy.Rb.velocity = Vector2.Lerp(_ragdollVelocity, Vector2.zero, _ragdollTimer / _ragdollDuration);
                }
            }

            // 3. Xử lý danh sách Status Effects
            if (_activeEffects.Count > 0)
            {
                bool dirty = false;
                for (int i = _activeEffects.Count - 1; i >= 0; i--)
                {
                    var effect = _activeEffects[i];
                    effect.ElapsedTime += dt;

                    // Xử lý DoT (Burn)
                    if (effect.Type == StatusEffectType.Burn && Time.time >= effect.NextTickTime)
                    {
                        effect.NextTickTime = Time.time + effect.TickInterval;
                        effect.OnTickDamage?.Invoke(effect.Value);
                    }

                    // Xử lý Humiliated Friendly Fire (Quê độ quay sang đấm quái bên cạnh)
                    if (effect.Type == StatusEffectType.Humiliated && Time.time >= effect.NextTickTime)
                    {
                        effect.NextTickTime = Time.time + effect.TickInterval;
                        TriggerHumiliatedFriendlyPunch();
                    }

                    if (effect.IsExpired)
                    {
                        // Khi hết hiệu ứng Say Thuốc Lào -> Nổ ho sặc sụa lan Hỏa sát thương
                        if (effect.Type == StatusEffectType.Stoned)
                        {
                            TriggerStonedSneezeExplosion();
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

        private void EndRagdollImpact()
        {
            _isRagdollActive = false;
            transform.rotation = Quaternion.identity;
            _enemy.Rb.velocity = Vector2.zero;
            RemoveStatus(StatusEffectType.RagdollFlight);

            // Nổ sát thương va đập lan sang quái xung quanh
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _ragdollImpactRadius, 1 << gameObject.layer);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].gameObject != gameObject && hits[i].TryGetComponent<Enemy>(out var otherEnemy))
                {
                    otherEnemy.HealthSystem?.TakeDamage(_ragdollImpactDamage);
                    otherEnemy.ApplyKnockback((hits[i].transform.position - transform.position).normalized, 6f, 0.25f);
                }
            }
        }

        private void TriggerHumiliatedFriendlyPunch()
        {
            // Tìm 1 quái bạn gần nhất trong phạm vi 1.5m để đấm
            Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, 1.5f, 1 << gameObject.layer);
            for (int i = 0; i < allies.Length; i++)
            {
                if (allies[i].gameObject != gameObject && allies[i].TryGetComponent<Enemy>(out var allyEnemy))
                {
                    allyEnemy.HealthSystem?.TakeDamage(50f);
                    allyEnemy.ApplyKnockback((allies[i].transform.position - transform.position).normalized, 4f, 0.2f);
                    break;
                }
            }
        }

        private void TriggerStonedSneezeExplosion()
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 2.5f, 1 << gameObject.layer);
            for (int i = 0; i < nearby.Length; i++)
            {
                if (nearby[i].TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.HealthSystem?.TakeDamage(60f);
                    enemy.ApplyKnockback((nearby[i].transform.position - transform.position).normalized, 5f, 0.25f);
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
                }
            }

            CurrentSlowMultiplier = Mathf.Clamp01(1f - maxSlow);
            IsStunned = isStun;
            IsFrozen = isFreeze;
            IsHumiliated = isHumiliated;
            IsSleeping = isSleeping;
            IsStoned = isStoned;
            IsDancing = isDancing;
        }

        /// <summary>
        /// Tính toán Tốc độ di chuyển thực tế của Enemy sau khi đã tính Slow/Stun/Freeze/Say thuốc.
        /// </summary>
        public float GetModifiedMoveSpeed(float baseSpeed)
        {
            if (!CanMove) return 0f;
            if (IsStoned) return baseSpeed * 0.55f; // Say thuốc chỉ bị làm chậm nhẹ 45% và lảo đảo bước đi, không còn giật lùi xa mất kiểm soát
            return baseSpeed * CurrentSlowMultiplier;
        }
    }
}


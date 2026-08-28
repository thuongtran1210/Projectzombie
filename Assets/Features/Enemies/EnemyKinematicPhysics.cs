using System;
using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Component quản lý toàn bộ các mô phỏng chuyển động vật lý đặc thù của Enemy:
    /// 1. Knockback Velocity: Đẩy lùi giật lùi theo vector lực và nội suy giảm dần.
    /// 2. Kinematic Parabolic Ragdoll Launch: Bay xoay tròn trên không trung và phát nổ va chạm khi tiếp đất.
    /// Tách biệt hoàn toàn khỏi EnemyStatusController theo nguyên tắc Đơn Trách Nhiệm (SRP).
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyKinematicPhysics : MonoBehaviour
    {
        private Enemy _enemy;

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

        public bool IsKnockbackActive => _isKnockbackActive;
        public bool IsRagdollActive => _isRagdollActive;
        public bool CanMovePhysics => !_isKnockbackActive && !_isRagdollActive;

        public event Action OnRagdollEnded;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            ResetPhysics();
        }

        public void ResetPhysics()
        {
            _isKnockbackActive = false;
            _isRagdollActive = false;
            _knockbackTimer = 0f;
            _ragdollTimer = 0f;
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Gây hiệu ứng Đẩy lùi (Knockback) lên kẻ địch.
        /// </summary>
        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            if (_enemy == null || _enemy.IsBoss || _enemy.Tenacity >= 0.9f) return;

            _isKnockbackActive = true;
            _knockbackVelocity = direction.normalized * force;
            _knockbackDuration = duration;
            _knockbackTimer = 0f;
        }

        /// <summary>
        /// Phóng quái bay theo dạng Kinematic Parabolic Ragdoll (Slapstick Launch).
        /// </summary>
        public void ApplyRagdollLaunch(Vector2 direction, float speed, float duration, float impactDamage = 80f, float impactRadius = 2.5f)
        {
            if (_enemy == null || _enemy.IsBoss || _enemy.Tenacity >= 0.8f) return;

            _isRagdollActive = true;
            _ragdollVelocity = direction.normalized * speed;
            _ragdollDuration = duration;
            _ragdollTimer = 0f;
            _ragdollImpactDamage = impactDamage;
            _ragdollImpactRadius = impactRadius;
            _ragdollRotationSpeed = UnityEngine.Random.Range(540f, 1080f) * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
        }

        private void Update()
        {
            if (!ProjectZombie.Features.Shared.GameStateManager.IsPlaying) return;

            float dt = Time.deltaTime;

            // 1. Cập nhật Knockback
            if (_isKnockbackActive && _enemy != null && _enemy.Rb != null)
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

            // 2. Cập nhật Kinematic Ragdoll Launch (Bay xoay tròn)
            if (_isRagdollActive && _enemy != null && _enemy.Rb != null)
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
        }

        private void EndRagdollImpact()
        {
            _isRagdollActive = false;
            transform.rotation = Quaternion.identity;
            if (_enemy != null && _enemy.Rb != null)
            {
                _enemy.Rb.velocity = Vector2.zero;
            }

            OnRagdollEnded?.Invoke();

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
    }
}

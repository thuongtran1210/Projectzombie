using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Thành phần điều phối Đạn Đạo cốt lõi (Projectile Core).
    /// Quản lý vận tốc, quỹ đạo bay (Parabolic / Homing), va chạm và thực thi Attack Effects trên mục tiêu.
    /// Tương thích hoàn toàn với Object Pooling, đảm bảo 0 GC Allocations trong vòng lặp chiến đấu.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProjectileCore3D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectileVisual3D _visual;

        private Component _source;
        private IDefenseEnemyTarget3D _target;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _damage;
        private float _speed = 20f;
        private float _arcHeight = 0.8f;
        private float _flightDuration;
        private float _elapsedTime;
        private int _runGeneration = -1;
        private object _sessionIdentity;
        private IDefenseGameplayContext _gameplay;
        private IReadOnlyList<SO_AttackEffect> _effects;
        private Action<ProjectileCore3D> _returnToPoolAction;
        private bool _isActive;

        private void Awake()
        {
            if (_visual == null)
            {
                _visual = GetComponent<ProjectileVisual3D>();
                if (_visual == null) _visual = GetComponentInChildren<ProjectileVisual3D>();
            }
        }

        /// <summary>
        /// Khởi tạo đường đạn khi lấy ra từ Pool.
        /// </summary>
        public void Launch(
            Component source,
            Vector3 origin,
            IDefenseEnemyTarget3D target,
            float damage,
            float speed,
            float arcHeight,
            IReadOnlyList<SO_AttackEffect> effects,
            int runGeneration,
            object sessionIdentity,
            IDefenseGameplayContext gameplay,
            Action<ProjectileCore3D> returnToPool)
        {
            _source = source;
            _target = target;
            _damage = damage;
            _speed = Mathf.Max(1f, speed);
            _arcHeight = arcHeight;
            _effects = effects;
            _runGeneration = runGeneration;
            _sessionIdentity = sessionIdentity;
            _gameplay = gameplay;
            _returnToPoolAction = returnToPool;

            _startPosition = origin;
            transform.position = origin;
            _elapsedTime = 0f;

            if (_target != null && _target.TargetTransform != null)
            {
                _targetPosition = _target.TargetTransform.position + Vector3.up * 0.65f;
            }
            else
            {
                _targetPosition = origin + Vector3.forward * 5f;
            }

            float distance = Vector3.Distance(_startPosition, _targetPosition);
            _flightDuration = Mathf.Clamp(distance / _speed, 0.05f, 5f);

            _isActive = true;
            gameObject.SetActive(true);

            if (_visual != null)
            {
                _visual.OnSpawn();
                Vector3 initialDir = (_targetPosition - _startPosition).normalized;
                _visual.UpdateDirection(initialDir);
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            // Kiểm tra tính hợp lệ của Run Generation và Session
            if (_gameplay != null)
            {
                if (_runGeneration != _gameplay.RunGeneration || !ReferenceEquals(_sessionIdentity, _gameplay.SessionIdentity))
                {
                    Despawn();
                    return;
                }
            }

            // Bám theo mục tiêu nếu mục tiêu còn sống và di chuyển
            if (_target != null && _target.TargetTransform != null && _target.IsTargetable)
            {
                _targetPosition = _target.TargetTransform.position + Vector3.up * 0.65f;
            }

            _elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsedTime / _flightDuration);

            // Quỹ đạo Parabol bay mượt mà
            Vector3 currentPos = Vector3.Lerp(_startPosition, _targetPosition, progress) 
                               + Vector3.up * (4f * _arcHeight * progress * (1f - progress));

            Vector3 tangent = (_targetPosition - _startPosition) 
                            + Vector3.up * (4f * _arcHeight * (1f - 2f * progress));

            transform.position = currentPos;

            if (_visual != null)
            {
                _visual.UpdateDirection(tangent.normalized);
            }

            // Khi đạn đến đích
            if (progress >= 1f)
            {
                OnHit();
            }
        }

        private void OnHit()
        {
            if (!_isActive) return;

            // Gây sát thương lên mục tiêu
            if (_target != null && _target.TargetTransform != null && _target.IsTargetable)
            {
                var hitPoint = new DefensePoint3(transform.position.x, transform.position.y, transform.position.z);
                var damageContext = new DefenseDamageContext(
                    amount: _damage,
                    source: _source,
                    hitPoint: hitPoint,
                    isCritical: false,
                    runGeneration: _runGeneration,
                    sessionIdentity: _sessionIdentity);

                _target.TakeDamage(damageContext);

                // Áp dụng các hiệu ứng chiến đấu (Slow, Stun...)
                if (_effects != null && _effects.Count > 0)
                {
                    for (int i = 0; i < _effects.Count; i++)
                    {
                        var effect = _effects[i];
                        if (effect != null && UnityEngine.Random.value <= effect.TriggerChance)
                        {
                            effect.ApplyEffect(_target.TargetTransform, _damage, _gameplay);
                        }
                    }
                }
            }

            Despawn();
        }

        public void Despawn()
        {
            if (!_isActive) return;
            _isActive = false;

            if (_visual != null)
            {
                _visual.OnDespawn();
            }

            gameObject.SetActive(false);
            _returnToPoolAction?.Invoke(this);
        }
    }
}

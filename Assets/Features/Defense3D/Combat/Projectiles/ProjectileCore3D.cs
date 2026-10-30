using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Pooled projectile with session clock, target lease validation and immutable shot parameters.</summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileCore3D : MonoBehaviour
    {
        [SerializeField] private ProjectileVisual3D _visual;
        private DefenseProjectileDefinitionSO _definition;
        private Component _source;
        private IDefenseEnemyTarget3D _target;
        private DefenseEnemy3D _enemy;
        private DefenseActorLifetime _targetLifetime;
        private Vector3 _start, _end;
        private float _damage, _duration, _elapsed, _arc;
        private int _generation;
        private object _session;
        private IDefenseGameplayContext _gameplay;
        private IReadOnlyList<SO_AttackEffect> _effects;
        private Action<ProjectileCore3D> _return;
        private bool _active;
        private IProjectileTrajectory _trajectory;
        private readonly IDefenseTargetPosition[] _areaTargets = new IDefenseTargetPosition[128];
        private readonly DefenseActorLifetime[] _areaLeases = new DefenseActorLifetime[128];
        public bool IsInFlight => _active;
        public float Progress => _duration > 0f ? _elapsed / _duration : 0f;
        private void Awake()
        {
            if (_visual == null) _visual = GetComponentInChildren<ProjectileVisual3D>(true);
        }
        public void SetDefinition(DefenseProjectileDefinitionSO definition) => _definition = definition;
        public void Launch(Component source, Vector3 origin, IDefenseEnemyTarget3D target, float damage, float speed,
            float arcHeight, IReadOnlyList<SO_AttackEffect> effects, int runGeneration, object sessionIdentity,
            IDefenseGameplayContext gameplay, Action<ProjectileCore3D> returnToPool, Vector3? targetPoint = null)
        {
            _source = source; _target = target; _damage = damage; _effects = effects;
            _generation = runGeneration; _session = sessionIdentity; _gameplay = gameplay; _return = returnToPool;
            _enemy = target as DefenseEnemy3D;
            _targetLifetime = _enemy != null ? _enemy.Lifetime : null;
            _start = origin;
            _end = target != null && target.TargetTransform != null ? target.TargetTransform.position + Vector3.up * .65f
                : targetPoint ?? origin + Vector3.forward * 5f;
            _trajectory = _definition != null ? _definition.GetTrajectory() : ParabolicHomingTrajectory.Instance;
            _arc = _definition != null ? _definition.ArcHeight : arcHeight;
            float actualSpeed = _definition != null ? _definition.Speed : speed;
            float lifetime = _definition != null ? _definition.MaxLifetime : 5f;
            _duration = Mathf.Clamp(Vector3.Distance(_start, _end) / Mathf.Max(1f, actualSpeed), .05f, lifetime);
            _elapsed = 0f;
            _active = true;
            transform.position = origin;
            gameObject.SetActive(true);
            _visual?.OnSpawn();
        }
        private bool CurrentLease => _enemy == null || (ReferenceEquals(_enemy.Lifetime, _targetLifetime) &&
            (_targetLifetime == null || _targetLifetime.IsCurrent(_gameplay)));
        private bool CurrentTarget => _target != null && !(_target is UnityEngine.Object targetObject && targetObject == null)
            && _target.TargetTransform != null && _target.IsTargetable && CurrentLease;
        private void Update() => Tick(Time.deltaTime);
        public void Tick(float delta)
        {
            if (!_active) return;
            if (_gameplay != null)
            {
                if (_generation != _gameplay.RunGeneration || !ReferenceEquals(_session, _gameplay.SessionIdentity)) { Despawn(); return; }
                if (!_gameplay.AdvancesGameplay) return;
            }
            if (_trajectory.Type == DefenseTrajectoryType.ParabolicHoming)
            {
                if (!CurrentTarget) { Despawn(); return; }
                _end = _target.TargetTransform.position + Vector3.up * .65f;
            }
            _elapsed += Mathf.Max(0f, delta);
            float progress = Mathf.Clamp01(_elapsed / _duration);
            transform.position = _trajectory.EvaluatePosition(_start, _end, progress, _arc);
            _visual?.UpdateDirection(_trajectory.EvaluateDirection(_start, _end, progress, _arc));
            if (progress < 1f) return;
            try
            {
                if (_definition != null && _definition.PayloadType == DefensePayloadType.AoEExplosion)
                {
                    ResolveAreaHit();
                }
                else if (CurrentTarget)
                {
                    bool accepted = _target.TakeDamage(new DefenseDamageContext(_damage, _source, transform.position,
                        runGeneration: _generation, sessionIdentity: _session));
                    if (accepted && _effects != null)
                        for (int i = 0; i < _effects.Count; i++) _effects[i]?.ApplyEffect(_target.TargetTransform, _damage, _gameplay);
                }
            }
            finally { Despawn(); }
        }
        private void ResolveAreaHit()
        {
            if (!(_gameplay?.Targets is IDefenseAreaTargetQuery query) ||
                !query.TryCopyWithin(transform.position, _definition.AoERadius, DefenseTargetMask.All, _areaTargets, out int count)) return;
            for (int i = 0; i < count; i++) _areaLeases[i] = (_areaTargets[i].Identity as DefenseEnemy3D)?.Lifetime;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var candidate = _areaTargets[i];
                    if (!candidate.IsAvailable || !(candidate.Identity is IDefenseEnemyTarget3D target)) continue;
                    if (target is DefenseEnemy3D enemy && (!ReferenceEquals(enemy.Lifetime, _areaLeases[i]) ||
                        (_areaLeases[i] != null && !_areaLeases[i].IsCurrent(_gameplay)))) continue;
                    bool accepted = target.TakeDamage(new DefenseDamageContext(_damage, _source, transform.position,
                        runGeneration: _generation, sessionIdentity: _session));
                    if (accepted && _effects != null)
                        for (int j = 0; j < _effects.Count; j++) _effects[j]?.ApplyEffect(target.TargetTransform, _damage, _gameplay);
                }
            }
            finally
            {
                Array.Clear(_areaTargets, 0, count);
                Array.Clear(_areaLeases, 0, count);
            }
        }
        public void Despawn()
        {
            if (!_active) return;
            _active = false;
            _visual?.OnDespawn();
            var callback = _return;
            _return = null; _source = null; _target = null; _enemy = null; _targetLifetime = null;
            _effects = null; _gameplay = null; _session = null; _definition = null;
            gameObject.SetActive(false);
            callback?.Invoke(this);
        }
    }
}

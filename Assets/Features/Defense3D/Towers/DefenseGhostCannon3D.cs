using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Damageable stationary source with one reusable authored projectile; destruction stops only future shots.</summary>
    public sealed class DefenseGhostCannon3D : DefenseGameplayBehaviour, IDefenseEnemyTarget3D
    {
        [SerializeField] private string _cannonId;
        [SerializeField] private DefenseGhostCannonProfileSO _profile;
        [SerializeField] private Transform _landingAnchor;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _projectile;
        [SerializeField] private GameObject _canopy;
        [SerializeField] private Renderer _body;
        [SerializeField] private Collider _hitbox;
        [SerializeField] private float _shotOffset;
        private DefenseHealthCore _health;
        private DefenseSpawnRuntime _spawns;
        private DefenseTargetIndex _index;
        private DefenseLandingFlight _flight;
        private int _generation, _shot;
        private DefenseCannonSchedule _schedule;
        private bool _live;
        public string CannonId => _cannonId;
        public Transform TargetTransform => transform;
        public bool IsTargetable => _live && isActiveAndEnabled && CanAdvanceGameplay && !IsDead && _generation == Gameplay.RunGeneration;
        public float CurrentHP => _health != null ? _health.CurrentHp : 0f;
        public float MaxHP => _health != null ? _health.MaxHp : 0f;
        public bool IsDead => _health == null || _health.IsDead;
        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        public event Action Defeated;

        public void ValidateBindings()
        {
            if (string.IsNullOrWhiteSpace(_cannonId) || _profile == null || _landingAnchor == null || _projectile == null ||
                _muzzle == null || !_muzzle.IsChildOf(transform) || _canopy == null || _body == null || _hitbox == null || !_projectile.IsChildOf(transform) ||
                !_canopy.transform.IsChildOf(_projectile) || _landingAnchor.gameObject.scene != gameObject.scene)
                throw new InvalidOperationException("Assign cannon ID, profile, West anchor, authored projectile/canopy and body/hitbox.");
            _profile.Validate();
            if (!DefenseDamageRules.IsFinite(_shotOffset) || _shotOffset < 0f || _landingAnchor.position.y >= _profile.OpenHeight ||
                !_body.transform.IsChildOf(transform) || !_hitbox.transform.IsChildOf(transform))
                throw new InvalidOperationException("Invalid cannon offset, parachute height or body ownership.");
        }
        public void Bind(IDefenseGameplayContext gameplay, DefenseSpawnRuntime spawns)
        {
            ValidateBindings();
            _spawns = spawns ?? throw new ArgumentNullException(nameof(spawns));
            _index = gameplay.Targets as DefenseTargetIndex ?? throw new ArgumentException("Cannon requires session target registry.");
            _health = new DefenseHealthCore(_profile.Health);
            _schedule = new DefenseCannonSchedule(_profile.FirstShotTime + _shotOffset, _profile.Interval, _profile.StopLaunchingAt);
            _health.HealthChanged += (current, maximum) => HealthChanged?.Invoke(current, maximum);
            _health.Damaged += damage => Damaged?.Invoke(damage);
            _health.Defeated += OnDefeated;
            BindGameplay(gameplay);
            ResetCannon();
        }
        public bool TakeDamage(DefenseDamageContext context) => IsTargetable &&
            DefenseDamageRules.CanApply(context, Gameplay, IsDead) && _health.ApplyDamage(context);
        public void Tick()
        {
            if (!_live || !CanAdvanceGameplay || _generation != Gameplay.RunGeneration || !isActiveAndEnabled) return;
            if (_flight != null)
            {
                _flight.Advance(Gameplay);
                var point = _flight.Position;
                _projectile.position = new Vector3(point.X, point.Y, point.Z);
                _canopy.SetActive(_flight.Phase == DefenseLandingPhase.Parachuting);
                if (_flight.TryTakeLanding(Gameplay, out var landing))
                {
                    _spawns.EnqueueLanding(_cannonId, _shot, new Vector3(landing.X, landing.Y, landing.Z), _generation);
                    _flight = null;
                    _projectile.gameObject.SetActive(false);
                }
                return;
            }
            if (!_schedule.TryFire(GameplayTime, CanAdvanceGameplay, !IsDead, false)) return;
            Vector3 origin = _muzzle.position;
            Vector3 target = _landingAnchor.position;
            _flight = new DefenseLandingFlight(Gameplay, new DefensePoint3(origin.x, origin.y, origin.z),
                new DefensePoint3(target.x, target.y, target.z), _profile.BallisticSeconds, _profile.ParachuteSeconds,
                _profile.ArcHeight, _profile.OpenHeight);
            _shot++;
            _spawns.AnnounceLanding(_cannonId, _shot, target, _profile.BallisticSeconds + _profile.ParachuteSeconds, _generation);
            _projectile.position = origin;
            _canopy.SetActive(false);
            _projectile.gameObject.SetActive(true);
        }
        private void OnDefeated()
        {
            DefenseEnemyTargets3D.Unregister(_index, this);
            _body.enabled = false;
            _hitbox.enabled = false;
            Defeated?.Invoke();
        }
        private void ResetCannon()
        {
            CancelFlight();
            _generation = Gameplay.RunGeneration;
            _shot = 0;
            _schedule.Reset();
            _health.Reset(_profile.Health);
            _body.enabled = true;
            _hitbox.enabled = true;
            _live = true;
            DefenseEnemyTargets3D.Unregister(_index, this);
            DefenseEnemyTargets3D.Register(_index, this);
        }
        private void CancelFlight()
        {
            if (_flight != null) _spawns?.CancelLandingWarning(_cannonId, _shot, _generation);
            _flight?.Cancel(); _flight = null;
            if (_projectile != null) _projectile.gameObject.SetActive(false);
            if (_canopy != null) _canopy.SetActive(false);
        }
        protected override void OnGameplayCleanup()
        {
            _live = false;
            CancelFlight();
            DefenseEnemyTargets3D.Unregister(_index, this);
        }
        protected override void OnGameplayReset() => ResetCannon();
        private void OnDisable() => OnGameplayCleanup();
        protected override void OnDestroy()
        {
            DefenseEnemyTargets3D.Unregister(_index, this);
            base.OnDestroy();
        }
    }
}

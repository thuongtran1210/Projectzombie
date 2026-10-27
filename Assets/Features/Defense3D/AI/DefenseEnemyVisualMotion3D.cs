using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Source locomotion poses on visual children only; navigation and hitboxes retain ownership of the actor.</summary>
    public sealed class DefenseEnemyVisualMotion3D : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseEnemy3D _enemy;
        [SerializeField] private Transform _body;
        [SerializeField] private Transform _leftWing;
        [SerializeField] private Transform _rightWing;
        [SerializeField] private DefenseYakshaArenaCombat3D _bossCombat;
        [SerializeField] private Transform _weapon;
        [SerializeField] private float _windupLift = 1.8f;
        [SerializeField] private float _windupWeaponDegrees = 72f;
        [SerializeField] private float _bobAmplitude;
        [SerializeField] private float _bobFrequency;
        [SerializeField] private bool _absoluteBob;
        [SerializeField] private Vector3 _swayAxis = Vector3.forward;
        [SerializeField] private float _swayAmplitude;
        [SerializeField] private float _swayFrequency;
        [SerializeField] private float _wingAmplitude;
        [SerializeField] private float _wingFrequency;
        private Vector3 _bodyPosition;
        private Quaternion _bodyRotation, _leftRotation, _rightRotation;
        private Quaternion _weaponRotation;
        private bool _cached;
        private float _phase;

        private void CachePose()
        {
            if (_cached) return;
            _cached = true;
            _bodyPosition = _body.localPosition;
            _bodyRotation = _body.localRotation;
            if (_leftWing != null) _leftRotation = _leftWing.localRotation;
            if (_rightWing != null) _rightRotation = _rightWing.localRotation;
            if (_weapon != null) _weaponRotation = _weapon.localRotation;
            _phase = Mathf.Repeat(GetInstanceID() * 0.013f, Mathf.PI * 2f);
        }

        protected override void OnGameplayBound()
        {
            CachePose();
            ResetPose();
        }

        private void LateUpdate()
        {
            if (!CanAdvanceGameplay || _enemy == null || !_enemy.IsTargetable) return;
            float bob = Mathf.Sin(GameplayTime * _bobFrequency + _phase);
            if (_absoluteBob) bob = Mathf.Abs(bob);
            _body.localPosition = _bodyPosition + Vector3.up * (bob * _bobAmplitude);
            _body.localRotation = _bodyRotation * Quaternion.AngleAxis(
                Mathf.Sin(GameplayTime * _swayFrequency + _phase) * _swayAmplitude, _swayAxis);
            if (_bossCombat != null)
            {
                if (_bossCombat.PresentationPhase != DefenseYakshaPhase.AssaultBase)
                    _body.SetLocalPositionAndRotation(_bodyPosition, _bodyRotation);
                if (_weapon != null) _weapon.localRotation = _weaponRotation;
                if (_bossCombat.IsTelegraphing)
                {
                    _body.localPosition = _bodyPosition + Vector3.up * (_bossCombat.TelegraphProgress * _windupLift);
                    if (_weapon != null)
                        _weapon.localRotation = _weaponRotation * Quaternion.AngleAxis(_windupWeaponDegrees, Vector3.right);
                }
            }
            if (_leftWing != null && _rightWing != null)
            {
                float flap = Mathf.Sin(GameplayTime * _wingFrequency + _phase) * _wingAmplitude;
                _leftWing.localRotation = _leftRotation * Quaternion.AngleAxis(flap, Vector3.forward);
                _rightWing.localRotation = _rightRotation * Quaternion.AngleAxis(-flap, Vector3.forward);
            }
        }

        private void ResetPose()
        {
            if (!_cached) return;
            _body.SetLocalPositionAndRotation(_bodyPosition, _bodyRotation);
            if (_leftWing != null) _leftWing.localRotation = _leftRotation;
            if (_rightWing != null) _rightWing.localRotation = _rightRotation;
            if (_weapon != null) _weapon.localRotation = _weaponRotation;
        }

        protected override void OnGameplayCleanup() => ResetPose();
        protected override void OnGameplayReset() => ResetPose();
        private void OnDisable() => ResetPose();
    }
}

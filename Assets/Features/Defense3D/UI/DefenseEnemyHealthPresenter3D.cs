using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>One health subscription per actor lease; positions the bar at the authored head anchor.</summary>
    public sealed class DefenseEnemyHealthPresenter3D : MonoBehaviour
    {
        [SerializeField] private DefenseEnemyHealthView3D _view;
        [SerializeField] private float _headOffset = .2f;
        private DefenseEnemy3D _enemy;
        private IDefenseGameplayContext _gameplay;
        private DefenseActorLifetime _lifetime;
        private Transform _head;
        private Camera _camera;
        private Collider _body;
        public bool IsConfigured => _view != null && _view.IsConfigured;

        public void Bind(DefenseEnemy3D enemy, IDefenseGameplayContext gameplay, DefenseActorLifetime lifetime, Transform head, Camera camera, Collider body = null)
        {
            Unbind();
            if (!IsConfigured || enemy == null || gameplay == null || lifetime == null || (head == null && body == null)) return;
            _enemy = enemy; _gameplay = gameplay; _lifetime = lifetime; _head = head; _camera = camera;
            _body = body;
            _enemy.HealthChanged += OnHealthChanged;
            _enemy.Defeated += Hide;
            _gameplay.GameplayGateChanged += OnGate;
            _gameplay.RunCleanup += Hide;
            OnHealthChanged(enemy.CurrentHP, enemy.MaxHP);
            LateUpdate();
        }

        private bool Current => _enemy != null && _gameplay != null && _lifetime != null &&
            ReferenceEquals(_enemy.Lifetime, _lifetime) && _lifetime.IsCurrent(_gameplay) && !_enemy.IsDead;

        private void OnHealthChanged(float current, float maximum)
        {
            if (!Current || maximum <= 0f) { Hide(); return; }
            _view.SetHealth(current / maximum, _enemy.Stats.IsBoss);
        }

        private void LateUpdate()
        {
            if (!Current || !_gameplay.AdvancesGameplay || (_head == null && _body == null) || _camera == null || !_camera.isActiveAndEnabled)
            { Hide(); return; }
            Vector3 position = _head != null ? _head.position : new Vector3(_body.bounds.center.x, _body.bounds.max.y, _body.bounds.center.z);
            position += Vector3.up * _headOffset;
            Vector3 viewport = _camera.WorldToViewportPoint(position);
            bool visible = viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
            _view.SetPose(position, _camera.transform.rotation);
            _view.SetVisible(visible);
        }

        private void Hide() { if (_view != null) _view.SetVisible(false); }
        private void OnGate(bool active) { if (!active) Hide(); }
        public void Unbind()
        {
            if (_enemy != null) { _enemy.HealthChanged -= OnHealthChanged; _enemy.Defeated -= Hide; }
            if (_gameplay != null) { _gameplay.GameplayGateChanged -= OnGate; _gameplay.RunCleanup -= Hide; }
            _enemy = null; _gameplay = null; _lifetime = null; _head = null; _camera = null;
            _body = null;
            if (_view != null) _view.ResetView();
        }
        private void OnDisable() => Unbind();
        private void OnDestroy() => Unbind();
    }
}

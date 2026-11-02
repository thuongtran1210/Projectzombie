using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Explicit camera/health binding shared by friendly towers and hostile launchers.</summary>
    public sealed class DefenseStructureHealthPresenter3D : MonoBehaviour
    {
        [SerializeField] private DefenseEnemyHealthView3D _view;
        [SerializeField] private Transform _anchor;
        private IDefenseDamageable3D _health;
        private IDefenseGameplayContext _gameplay;
        private Camera _camera;
        public bool IsConfigured => _view != null && _view.IsConfigured && _anchor != null;
        public void Bind(IDefenseDamageable3D health, IDefenseGameplayContext gameplay, Camera camera)
        {
            Unbind();
            if (!IsConfigured || health == null || gameplay == null || camera == null) return;
            _health = health; _gameplay = gameplay; _camera = camera;
            _health.HealthChanged += OnHealthChanged;
            _health.Defeated += Hide;
            _gameplay.RunCleanup += Hide;
            _gameplay.GameplayGateChanged += OnGate;
            RefreshHealth();
        }
        private bool Alive => _health != null && !(_health is Object actor && actor == null) && !_health.IsDead && _health.MaxHP > 0f;
        private void RefreshHealth()
        {
            if (!Alive) { Hide(); return; }
            _view.SetHealth(_health.CurrentHP / _health.MaxHP, false);
        }
        private void OnHealthChanged(float current, float maximum)
        {
            if (maximum <= 0f || current <= 0f) { Hide(); return; }
            _view.SetHealth(current / maximum, false);
        }
        private void LateUpdate()
        {
            if (!isActiveAndEnabled || !IsConfigured || !Alive || _gameplay == null || !_gameplay.AdvancesGameplay ||
                _camera == null || !_camera.isActiveAndEnabled) { Hide(); return; }
            var position = _anchor.position;
            var viewport = _camera.WorldToViewportPoint(position);
            _view.SetPose(position, _camera.transform.rotation);
            _view.SetVisible(viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f);
        }
        private void OnGate(bool active) { if (!active) Hide(); }
        private void OnEnable() => RefreshHealth();
        private void Hide() { if (_view != null) _view.SetVisible(false); }
        private void OnDisable() => Hide(); // Retain the binding while the authored actor is in its inventory.
        public void Unbind()
        {
            if (_health != null && !(_health is Object actor && actor == null))
            { _health.HealthChanged -= OnHealthChanged; _health.Defeated -= Hide; }
            if (_gameplay != null) { _gameplay.RunCleanup -= Hide; _gameplay.GameplayGateChanged -= OnGate; }
            _health = null; _gameplay = null; _camera = null;
            if (_view != null) _view.ResetView();
        }
        private void OnDestroy() => Unbind();
    }
}

using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Persistent authored hero foot ring/direction, grounded independently of the capsule center.</summary>
    public sealed class DefenseHeroFootIndicator3D : DefenseGameplayBehaviour
    {
        [SerializeField] private Transform _hero;
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private Transform _visual;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private DefenseIndicatorSurface3D _surface;
        [SerializeField, Min(0f)] private float _surfaceOffset = 0.05f;
        private Collider _heroCollider;

        protected override void OnGameplayBound()
        {
            if (_hero != null) _hero.TryGetComponent(out _heroCollider);
        }

        private void LateUpdate()
        {
            bool visible = CanAdvanceGameplay && _visual != null && _hero != null && _health != null && _health.isActiveAndEnabled && !_health.IsDown;
            if (visible)
            {
                Vector3 point = _hero.position;
                point.y = _heroCollider != null ? _heroCollider.bounds.min.y : point.y;
                if (Physics.Raycast(_hero.position + Vector3.up * 2f, Vector3.down, out var hit, 8f,
                    _groundMask, QueryTriggerInteraction.Ignore)) point = hit.point;
                if (_surface != null) point = _surface.Place(_hero.position, 0f, .44f);
                _visual.SetPositionAndRotation(point + Vector3.up * _surfaceOffset,
                    Quaternion.Euler(0f, _hero.eulerAngles.y, 0f));
                SetVisible(true);
            }
            else SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (_visual != null && _visual.gameObject.activeSelf != visible) _visual.gameObject.SetActive(visible);
        }

        protected override void OnGameplayGateChanged(bool active) { if (!active) SetVisible(false); }
        protected override void OnGameplayCleanup() => SetVisible(false);
        private void OnDisable() => SetVisible(false);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Projects the hero's live status and basic attack footprint into authored views.</summary>
    public sealed class DefenseHeroStatusPresenter3D : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseHeroAutoAttack3D _attack;
        [SerializeField] private DefenseMatchPresenter _match;
        [SerializeField] private DefenseHeroKitSO _kit;
        [SerializeField] private Camera _camera;
        [SerializeField] private RectTransform _overhead;
        [SerializeField] private CanvasGroup _statusGroup;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _healthFill;
        [SerializeField] private DefenseAimIndicator3D _attackArea;
        [SerializeField, Min(0f)] private float _headOffset = 0.45f;
        private Collider _body;
        private int _lastLevel = -1;
        private bool _cleanedUp;

        protected override void OnGameplayBound()
        {
            _cleanedUp = false;
            _health.TryGetComponent(out _body);
            _health.HealthChanged -= RefreshHealth;
            _health.HealthChanged += RefreshHealth;
            _nameText.text = _kit.DisplayName;
            RefreshHealth(_health.CurrentHP, _health.MaxHP);
        }

        private void RefreshHealth(float current, float maximum)
        {
            float ratio = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
            _healthFill.rectTransform.anchorMax = new Vector2(ratio, 1f);
        }

        private void LateUpdate()
        {
            bool visible = Gameplay != null && !_cleanedUp && _health != null && _health.isActiveAndEnabled && !_health.IsDown;
            _statusGroup.alpha = visible ? 1f : 0f;
            if (!visible || _camera == null)
            {
                if (_attackArea != null) _attackArea.Hide();
                return;
            }

            Vector3 head = _health.transform.position;
            head.y = (_body != null ? _body.bounds.max.y : head.y) + _headOffset;
            if (_overhead.parent == null)
                _overhead.SetPositionAndRotation(head, _camera.transform.rotation);
            else
                _overhead.rotation = _camera.transform.rotation;

            if (Vector3.Dot(_camera.transform.forward, head - _camera.transform.position) <= 0f)
                _statusGroup.alpha = 0f;

            var experience = _match != null ? _match.ExperienceCore : null;
            if (experience != null && _lastLevel != experience.CurrentLevel)
            {
                _lastLevel = experience.CurrentLevel;
                _levelText.SetText("Lv {0}", _lastLevel);
            }

            if (!CanAdvanceGameplay || !_attack.isActiveAndEnabled)
            {
                if (_attackArea != null) _attackArea.Hide();
                return;
            }
        }

        protected override void OnGameplayGateChanged(bool active)
        {
            if (!active && _attackArea != null) _attackArea.Hide();
        }

        protected override void OnGameplayCleanup()
        {
            _cleanedUp = true;
            _statusGroup.alpha = 0f;
            if (_attackArea != null) _attackArea.Hide();
        }

        protected override void OnGameplayReset()
        {
            _cleanedUp = false;
            _lastLevel = -1;
            RefreshHealth(_health.CurrentHP, _health.MaxHP);
        }

        private void OnDisable()
        {
            _statusGroup.alpha = 0f;
            if (_attackArea != null) _attackArea.Hide();
        }

        protected override void OnDestroy()
        {
            if (_health != null) _health.HealthChanged -= RefreshHealth;
            base.OnDestroy();
        }
    }
}

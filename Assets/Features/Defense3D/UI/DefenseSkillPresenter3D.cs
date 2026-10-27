using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Connects one authored control, definition, preview and effect to one session-owned cast runtime.</summary>
    public sealed class DefenseSkillPresenter3D : MonoBehaviour, IDefenseAbilitySource
    {
        [SerializeField] private DefenseSkillDefinitionSO _definition;
        [SerializeField] private DefenseSkillAimButton3D _button;
        [SerializeField] private DefenseSkillButtonView _view;
        [SerializeField] private DefenseAimIndicator3D _indicator;
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private Transform _origin;
        [SerializeField] private DefenseSkillEffect3D _effect;
        private IDefenseGameplayContext _gameplay;
        private DefenseSkillCastRuntime _runtime;
        private int _displaySeconds = -1;
        private string _cooldownLabel = string.Empty;
        private DefenseHeroMotor3D _facing;

        public DefenseAbilityRuntime Ability => _runtime?.Ability;
        public DefenseSkillDefinitionSO DefinitionSource => _definition;
        public DefenseSkillEffect3D Effect => _effect;
        public DefenseSkillKind Kind => _definition.Capture().Kind;

        public void ValidateBindings()
        {
            if (_definition == null || _button == null || !_button.IsConfigured || _view == null || !_view.IsConfigured ||
                _indicator == null || !_indicator.IsConfigured || _health == null || _origin == null || _effect == null || !_effect.IsConfigured ||
                _definition.Capture().Kind != _effect.Kind || (_effect is DefenseHeroAttackEffect3D && !_indicator.CanShowCone))
                throw new InvalidOperationException("Skill Presenter requires authored definition/button/view/indicator/Hero/effect references.");
        }

        public void Bind(DefenseSession session)
        {
            Unbind();
            ValidateBindings();
            var definition = _definition.Capture();
            _effect.Configure(definition);
            var shared = _effect as IDefenseSharedAbilityEffect;
            if (shared != null && shared.SharedAbility == null) throw new InvalidOperationException("Bind attack owner before its presenter.");
            _runtime = new DefenseSkillCastRuntime(session, definition, shared?.SharedAbility);
            session.Own(_runtime);
            _gameplay = session;
            _origin.TryGetComponent(out _facing);
            _button.PreviewChanged += Preview;
            _button.PreviewCleared += HidePreview;
            _button.AimReleased += Cast;
            _button.Bind(session, _runtime.Ability, definition.Range, definition.AutoTargetOnTap, definition.TargetMask, definition.Kind == DefenseSkillKind.ScholarSignature);
            Refresh();
        }

        private bool ActorAvailable => isActiveAndEnabled && _health != null && _health.isActiveAndEnabled && !_health.IsDown &&
            _effect != null && _effect.isActiveAndEnabled && (_facing == null || _facing.CanAim(this));

        private void Preview(DefenseResolvedAim aim, bool cancelled)
        {
            if (_runtime == null || _gameplay == null || !_gameplay.AdvancesGameplay || !ActorAvailable)
            {
                HidePreview();
                return;
            }
            if (_facing != null && !_facing.TryClaimAim(this)) { _button.Cancel(); HidePreview(); return; }
            bool canCast = _effect.TryPrepare(aim, out var prepared);
            _facing?.SetAimFacing(this, aim.Direction);
            if (_effect is DefenseHeroAttackEffect3D attack)
                _indicator.ShowCone(canCast ? prepared : aim, _runtime.Definition.Radius, attack.ConeAngle, cancelled || !canCast);
            else
                _indicator.ShowAim(canCast ? prepared : aim, _runtime.Definition.Radius, _runtime.Definition.Range, cancelled || !canCast, Kind != DefenseSkillKind.ScholarSignature);
        }

        private void HidePreview()
        {
            _indicator?.Hide();
            _facing?.ClearAimFacing(this);
        }

        private void Cast(DefenseAimSubmission submission)
        {
            if (_runtime == null || _origin == null) return;
            Vector3 position = _origin.position;
            _runtime.TryCast(new DefenseSkillCastRequest(submission.Aim, submission.SessionIdentity, submission.RunGeneration),
                new DefensePoint3(position.x, position.y, position.z), ActorAvailable, _effect);
            Refresh();
        }

        public bool TryQuickCast()
        {
            if (_runtime == null || _gameplay == null || !ActorAvailable || !_button.TryResolveTap(out var aim)) return false;
            Vector3 p = _origin.position;
            bool cast = _runtime.TryCast(new DefenseSkillCastRequest(aim, _gameplay.SessionIdentity, _gameplay.RunGeneration),
                new DefensePoint3(p.x, p.y, p.z), ActorAvailable, _effect);
            Refresh();
            return cast;
        }

        private void Update() => Refresh();

        private void Refresh()
        {
            if (_runtime == null || _gameplay == null) return;
            float remaining = _runtime.Ability.RemainingCooldown;
            bool interactable = ActorAvailable && _gameplay.AdvancesGameplay && remaining <= 0f;
            _button.SetInteractable(interactable);
            int seconds = Mathf.CeilToInt(remaining);
            if (_displaySeconds != seconds)
            {
                _displaySeconds = seconds;
                _cooldownLabel = seconds > 0 ? seconds.ToString() : string.Empty;
            }
            _view.Render(remaining / (_effect is IDefenseSharedAbilityEffect shared ? shared.CastCooldown : _runtime.Definition.Cooldown), _cooldownLabel, !interactable && remaining <= 0f);
        }

        private void OnDisable() { _button?.SetInteractable(false); HidePreview(); }
        private void OnDestroy() => Unbind();

        public void Unbind()
        {
            if (_button != null)
            {
                _button.PreviewChanged -= Preview;
                _button.PreviewCleared -= HidePreview;
                _button.AimReleased -= Cast;
                _button.SetInteractable(false);
                _button.Unbind();
            }
            _runtime?.Dispose();
            _runtime = null;
            _gameplay = null;
            _displaySeconds = -1;
            HidePreview();
        }
    }
}

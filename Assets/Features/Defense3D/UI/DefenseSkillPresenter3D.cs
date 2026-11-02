using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Connects one authored control, definition, preview and effect to one session-owned cast runtime.</summary>
    public sealed class DefenseSkillPresenter3D : MonoBehaviour, IDefenseAbilitySource
    {
        [SerializeField] private DefenseSkillKind _slot;
        [SerializeField,HideInInspector] private DefenseSkillDefinitionSO _definition; // Cleared from assets by migration; runtime injection only.
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
        private float _metaDamageBonus;
        private float _metaCooldownReduction;
        private float _metaAreaBonus;
        private int _smokeTenths = -1;
        private string _smokeLabel = string.Empty;

        public void SetMetaSkillBonuses(float damageBonus, float cooldownReduction, float areaBonus)
        {
            _metaDamageBonus = damageBonus;
            _metaCooldownReduction = cooldownReduction;
            _metaAreaBonus = areaBonus;
        }

        public DefenseAbilityRuntime Ability => _runtime?.Ability;
        public DefenseSkillDefinitionSO DefinitionSource => _definition;
        public DefenseSkillDefinition ResolvedDefinition => _runtime?.Definition;
        public DefenseSkillEffect3D Effect => _effect;
        public DefenseSkillKind Kind => _slot;

        /// <summary>
        /// Hoán đổi định nghĩa kỹ năng, component hiệu ứng, tên hiển thị và icon động khi xuất trận (Clean/SOLID).
        /// </summary>
        public void Rebind(DefenseSkillDefinitionSO definition, DefenseSkillEffect3D effect)
        {
            if (definition != null) _definition = definition;
            if (effect != null) _effect = effect;
        }

        public void ValidateBindings()
        {
            if (_definition == null || _button == null || !_button.IsConfigured || _view == null || !_view.IsConfigured ||
                _indicator == null || !_indicator.IsConfigured || _health == null || _origin == null || _effect == null || !_effect.IsConfigured ||
                _definition.Capture().Kind != _slot || _slot != _effect.Kind || (_effect is DefenseHeroAttackEffect3D && !_indicator.CanShowCone))
                throw new InvalidOperationException("Skill Presenter requires authored definition/button/view/indicator/Hero/effect references.");
        }

        public void Bind(DefenseSession session)
        {
            Unbind();
            ValidateBindings();
            var definition = _definition.Capture();
            if (_metaDamageBonus > 0f || _metaCooldownReduction > 0f || _metaAreaBonus > 0f)
            {
                definition = definition.WithBonuses(_metaDamageBonus, _metaCooldownReduction, _metaAreaBonus);
            }
            _effect.Configure(definition);
            if(_definition.Presentation!=null)
            {
                _view.SetSkillIdentity(_definition.Presentation.DisplayName,_definition.Presentation.Icon);
                _view.SetAccent(_definition.Presentation.Accent);
            }
            bool pipe = _effect is DefensePipeRelicEffect3D;
            _view.RenderStatus(pipe ? "THỔI KHÓI" : string.Empty, 0f);
            var shared = _effect as IDefenseSharedAbilityEffect;
            if (shared != null && shared.SharedAbility == null) throw new InvalidOperationException("Bind attack owner before its presenter.");
            _runtime = new DefenseSkillCastRuntime(session, definition, shared?.SharedAbility);
            session.Own(_runtime);
            _gameplay = session;
            _origin.TryGetComponent(out _facing);
            _button.PreviewChanged += Preview;
            _button.PreviewCleared += HidePreview;
            _button.AimReleased += Cast;
            _button.BasicAttackRequested += BasicPressed;
            _button.RecastRequested += BasicPressed;
            if (_view != null) _button.AimFeedbackChanged += _view.SetAimFeedback;
            _button.SetBasicPressMode(definition.Kind == DefenseSkillKind.BasicAttack);
            bool isSelfTarget = _definition.Behavior == DefenseSkillBehavior.ScholarSignature;
            _button.Bind(session, _runtime.Ability, definition.Range, definition.AutoTargetOnTap, definition.TargetMask, isSelfTarget);
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
            else if (_effect is DefensePipeRelicEffect3D)
                _indicator.ShowArea(canCast ? prepared : aim, _runtime.Definition.Radius, _runtime.Definition.Range, cancelled || !canCast);
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
            if (_runtime == null || _gameplay == null || !ActorAvailable) return false;
            if (_effect is DefenseRelicSkillEffect3D relic && relic.Phase == DefenseRelicPhase.Anchored)
            {
                bool recast = relic.TryRecast();
                Refresh();
                return recast;
            }
            DefenseResolvedAim aim;
            if (_effect is DefenseHeroAttackEffect3D basic)
            {
                if (!basic.TryResolveBasicAim(out aim)) return false;
            }
            else if (!_button.TryResolveTap(out aim)) return false;
            Vector3 p = _origin.position;
            bool cast = _runtime.TryCast(new DefenseSkillCastRequest(aim, _gameplay.SessionIdentity, _gameplay.RunGeneration),
                new DefensePoint3(p.x, p.y, p.z), ActorAvailable, _effect);
            Refresh();
            return cast;
        }

        private void Update() => Refresh();
        private void BasicPressed() => TryQuickCast();

        private void Refresh()
        {
            if (_runtime == null || _gameplay == null) return;
            float remaining = _runtime.Ability.RemainingCooldown;
            if (_effect is DefensePipeRelicEffect3D pipe)
            {
                int tenths = Mathf.CeilToInt(pipe.RemainingSmokeSeconds * 10f);
                if (_smokeTenths != tenths)
                {
                    _smokeTenths = tenths;
                    _smokeLabel = tenths > 0 ? "KHÓI " + (tenths / 10f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "s" : string.Empty;
                }
                _view.RenderStatus(tenths > 0 ? _smokeLabel : remaining > 0f ? "ĐANG HỒI" : "THỔI KHÓI",
                    pipe.SmokeDuration > 0f ? pipe.RemainingSmokeSeconds / pipe.SmokeDuration : 0f);
            }
            var relic = _effect as DefenseRelicSkillEffect3D;
            bool recast = relic != null && relic.CanRecast;
            bool relicBusy = relic != null && relic.Phase != DefenseRelicPhase.Ready;
            bool interactable = ActorAvailable && _gameplay.AdvancesGameplay && (recast || (!relicBusy && remaining <= 0f));
            _button.SetRecastMode(recast);
            _button.SetInteractable(interactable);
            int seconds = Mathf.CeilToInt(remaining);
            if (_displaySeconds != seconds)
            {
                _displaySeconds = seconds;
                _cooldownLabel = seconds > 0 ? seconds.ToString() : string.Empty;
            }
            if (recast)
            {
                _view.Render(0f, "LAO TỚI", !interactable);
                return;
            }
            string label = Kind == DefenseSkillKind.BasicAttack ? string.Empty : _cooldownLabel;
            if (relic != null && remaining <= 0f && !relicBusy) label = "QUĂNG";
            if (relic != null && relic.Phase == DefenseRelicPhase.Flying) label = "ĐANG BAY";
            _view.Render(remaining / (_effect is IDefenseSharedAbilityEffect shared ? shared.CastCooldown : _runtime.Definition.Cooldown),
                label, !interactable && remaining <= 0f);
        }

        private void OnDisable() { _button?.SetInteractable(false); _view?.SetAimFeedback(false, false); HidePreview(); }
        private void OnDestroy() => Unbind();

        public void Unbind()
        {
            if (_button != null)
            {
                _button.PreviewChanged -= Preview;
                _button.PreviewCleared -= HidePreview;
                _button.AimReleased -= Cast;
                _button.BasicAttackRequested -= BasicPressed;
                _button.RecastRequested -= BasicPressed;
                if (_view != null) _button.AimFeedbackChanged -= _view.SetAimFeedback;
                _button.SetInteractable(false);
                _button.Unbind();
            }
            _runtime?.Dispose();
            _runtime = null;
            _gameplay = null;
            _displaySeconds = -1;
            _smokeTenths = -1;
            _view?.RenderStatus(string.Empty, 0f);
            _view?.SetAimFeedback(false, false);
            HidePreview();
        }
    }
}

using UnityEngine;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Manual cast adapter over the existing basic-attack owner and an authored hit-sector view.</summary>
    public sealed class DefenseHeroAttackEffect3D : DefenseSkillEffect3D, IDefenseSharedAbilityEffect
    {
        [SerializeField] private DefenseHeroAutoAttack3D _attack;
        [SerializeField] private DefenseAimIndicator3D _impactView;
        private float _hideAt;
        private float _showAt;
        private DefenseConeSnapshot _hit;
        public override DefenseSkillKind Kind => DefenseSkillKind.BasicAttack;
        public override bool IsConfigured => _attack != null && _impactView != null && _impactView.CanShowCone;
        public DefenseAbilityRuntime SharedAbility => _attack.Ability;
        public float CastCooldown => _attack.EffectiveAttackInterval;
        public float ConeAngle => _attack.ConeAngle;
        public bool TryResolveBasicAim(out DefenseResolvedAim aim) => _attack.TryResolveBasicAim(out aim);
        protected override void OnGameplayBound() { if (_attack != null) { _attack.AttackResolved -= OnHit; _attack.AttackResolved += OnHit; } }
        protected override void OnConfigured(DefenseSkillDefinition definition) => _attack.ConfigureAttack(definition);
        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            prepared = requested;
            return IsConfigured && Definition != null && _attack.CanPrepare && _attack.TryResolveBasicAim(out prepared);
        }
        public override void Execute(DefenseResolvedAim prepared) => _attack.QueueAttack(prepared);
        private void OnHit(DefenseConeSnapshot cone)
        {
            _hit = cone;
            _showAt = GameplayTime;
            _hideAt = GameplayTime + .22f;
            _impactView.ShowSlash(cone, 0f);
        }
        private void Update()
        {
            if (_hideAt <= 0f) return;
            if (GameplayTime >= _hideAt) Hide();
            else _impactView.ShowSlash(_hit, Mathf.Clamp01((GameplayTime - _showAt) / .22f));
        }
        private void Hide() { _hideAt = 0f; _impactView?.Hide(); }
        protected override void OnGameplayGateChanged(bool active) { if (!active) Hide(); }
        protected override void OnGameplayCleanup() => Hide();
        private void OnDisable() => Hide();
        protected override void OnDestroy() { if (_attack != null) _attack.AttackResolved -= OnHit; base.OnDestroy(); }
    }
}

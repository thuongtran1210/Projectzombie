using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Target-local elemental priming and ordered reaction dispatch. Separate from global Tương Sinh.</summary>
    [RequireComponent(typeof(Enemy))]
    public sealed class EnemyElementReactionController : MonoBehaviour
    {
        private Enemy _enemy;
        private readonly ElementMarkState _mark = new ElementMarkState();
        private float _nextReactionAt;
        private ElementReactionDispatcher _dispatcher;
        private ElementReactionSettings _reactionSettings;

        public ElementType PrimedElement => _mark.GetElement(Time.time);
        public Object PrimedSource => _mark.GetSource(Time.time);
        public float PrimeExpiresAt => _mark.ExpiresAt;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _reactionSettings = ElementReactionSettings.Runtime;
            _dispatcher = new ElementReactionDispatcher(
                FireSpreadSettings.Runtime,
                SteamSlipReactionSettings.Runtime,
                ToaiGiapReactionSettings.Runtime);
        }

        private void OnEnable() => ResetState();
        private void OnDisable() => ResetState();

        public void ApplyElement(DamageData incomingDamage)
        {
            ElementType incoming = incomingDamage.Element;
            if (incoming == ElementType.None || _enemy == null || !_enemy.isActiveAndEnabled) return;

            ElementReactionType naturalReaction = SteamSlipReactionResolver.ResolveNaturalAffinity(_enemy.CurrentElement, incoming);
            if (naturalReaction == ElementReactionType.HuoHoan && _dispatcher != null &&
                _dispatcher.TryExecute(naturalReaction, _enemy, incomingDamage))
                return;

            if (_reactionSettings == null) return;

            ElementType targetElement = _enemy.CurrentElement;
            if (naturalReaction == ElementReactionType.BocHoi)
            {
                TryTriggerReaction(naturalReaction, incomingDamage);
                return;
            }

            ElementReactionType resolved = SteamSlipReactionResolver.ResolveOrderedPair(PrimedElement, incoming);
            if (_dispatcher.CanExecute(resolved))
            {
                TryTriggerReaction(resolved, incomingDamage);
                return;
            }

            if (incoming == targetElement)
            {
                ClearPrimedElement();
                return;
            }

            float expiresAt = Time.time + _reactionSettings.markDuration;
            _mark.Apply(incoming, incomingDamage.SourceWeapon, expiresAt);
            SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                SteamSlipDiagnosticKind.ElementPrimed, _enemy.GetInstanceID(), incoming, expiresAt, 0));
        }

        private void TryTriggerReaction(ElementReactionType reaction, DamageData incomingDamage)
        {
            if (_reactionSettings == null || _dispatcher == null || Time.time < _nextReactionAt)
            {
                SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                    SteamSlipDiagnosticKind.ReactionCooldownRejected, _enemy.GetInstanceID(), incomingDamage.Element, _nextReactionAt, (int)reaction));
                return;
            }

            if (!_dispatcher.TryExecute(reaction, _enemy, incomingDamage))
                return;

            ClearPrimedElement();
            _nextReactionAt = Time.time + _reactionSettings.reactionCooldown;
            SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                SteamSlipDiagnosticKind.ReactionTriggered, _enemy.GetInstanceID(), incomingDamage.Element, Time.time, (int)reaction));
        }

        private void ClearPrimedElement()
        {
            _mark.Clear();
        }

        public void ResetState()
        {
            ClearPrimedElement();
            _nextReactionAt = 0f;
        }
    }
}

using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Target-local elemental priming and pilot reaction dispatch. Separate from global Tương Sinh.</summary>
    [RequireComponent(typeof(Enemy))]
    public sealed class EnemyElementReactionController : MonoBehaviour
    {
        private Enemy _enemy;
        private ElementType _primedElement;
        private Object _primedSource;
        private float _primeExpiresAt;
        private float _nextReactionAt;
        private HuoHoanReaction _huoHoanReaction;

        public ElementType PrimedElement => Time.time < _primeExpiresAt ? _primedElement : ElementType.None;
        public Object PrimedSource => Time.time < _primeExpiresAt ? _primedSource : null;
        public float PrimeExpiresAt => _primeExpiresAt;

        private void Awake() => _enemy = GetComponent<Enemy>();

        private void OnEnable()
        {
            ResetState();
            FireSpreadSettings fireSettings = FireSpreadSettings.Runtime;
            if (fireSettings != null)
                _huoHoanReaction = new HuoHoanReaction(fireSettings);
        }
        private void OnDisable() => ResetState();

        public void ApplyElement(DamageData incomingDamage)
        {
            ElementType incoming = incomingDamage.Element;
            if (incoming == ElementType.None || _enemy == null || !_enemy.isActiveAndEnabled) return;
            SteamSlipReactionSettings settings = SteamSlipReactionSettings.Runtime;
            if (incoming == ElementType.Moc && _huoHoanReaction != null &&
                _huoHoanReaction.TryExecute(_enemy, incomingDamage))
                return;

            if (settings == null) return;

            ElementType targetElement = _enemy.CurrentElement;
            bool matchesNaturalElement = SteamSlipReactionResolver.ShouldTriggerNaturalMatch(targetElement, incoming);
            bool matchesPrimedElement = SteamSlipReactionResolver.ShouldTrigger(PrimedElement, incoming);

            if (matchesNaturalElement || matchesPrimedElement)
            {
                TryTriggerReaction(incoming, settings);
                return;
            }

            if (incoming == targetElement)
            {
                ClearPrimedElement();
                return;
            }

            _primedElement = incoming;
            _primedSource = incomingDamage.SourceWeapon;
            _primeExpiresAt = Time.time + settings.primeDuration;
            SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                SteamSlipDiagnosticKind.ElementPrimed, _enemy.GetInstanceID(), incoming, _primeExpiresAt, 0));
        }

        private void TryTriggerReaction(ElementType incoming, SteamSlipReactionSettings settings)
        {
            if (Time.time < _nextReactionAt)
            {
                SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                    SteamSlipDiagnosticKind.ReactionCooldownRejected, _enemy.GetInstanceID(), incoming, _nextReactionAt, 0));
                return;
            }

            ClearPrimedElement();
            _nextReactionAt = Time.time + settings.reactionCooldown;
            SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                SteamSlipDiagnosticKind.ReactionTriggered, _enemy.GetInstanceID(), incoming, Time.time, 0));
            SteamSlipPatchManager.CreatePatch(_enemy);
        }

        private void ClearPrimedElement()
        {
            _primedElement = ElementType.None;
            _primedSource = null;
            _primeExpiresAt = 0f;
        }

        public void ResetState()
        {
            ClearPrimedElement();
            _nextReactionAt = 0f;
        }
    }
}

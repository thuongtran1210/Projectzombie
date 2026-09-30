using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Executes the natural Hỏa target reaction for incoming Mộc damage.</summary>
    internal sealed class HuoHoanReaction
    {
        private readonly FireSpreadSettings _settings;

        public HuoHoanReaction(FireSpreadSettings settings)
        {
            _settings = settings;
        }

        public bool TryExecute(Enemy target, DamageData triggeringDamage)
        {
            if (target == null || target.HealthSystem == null || !target.HealthSystem.IsAlive ||
                target.CurrentElement != ElementType.Hoa ||
                triggeringDamage.Element != ElementType.Moc)
                return false;

            // The element pair is recognized even when Burn cannot be applied, so it will not prime another reaction.
            if (_settings == null || target.StatusController == null ||
                target.StatusController.IsImmuneTo(StatusEffectType.Burn) || target.HasStatus(StatusEffectType.Burn))
                return true;

            float burnDamage = Mathf.Max(0f, triggeringDamage.Amount * _settings.burnDamageMultiplier);
            System.Action spreadOnFirstTick = target.IsBoss
                ? null
                : (System.Action)(() => FireSpreadManager.SpreadFrom(target, burnDamage));
            target.StatusController.ApplyDamageOverTime(
                StatusEffectType.Burn,
                _settings.burnDuration,
                burnDamage,
                _settings.burnTickInterval,
                damage => ApplyBurnDamage(target, damage),
                spreadOnFirstTick);

            global::ProjectZombie.Core.Audio.AudioService.Current?.PlayElementalReaction(target.transform.position);

            return true;
        }

        private static void ApplyBurnDamage(Enemy target, float damage)
        {
            if (target != null && target.HealthSystem != null)
                target.HealthSystem.TakeDamage(new DamageData(damage, element: ElementType.Hoa, canTriggerReaction: false));
        }
    }
}

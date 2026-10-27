using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Validates scope and prepared geometry before the sole ability owner commits cooldown.</summary>
    public sealed class DefenseSkillCastRuntime : IDisposable
    {
        private readonly IDefenseGameplayContext _gameplay;
        private bool _disposed, _casting;
        private readonly bool _ownsAbility;
        public DefenseSkillDefinition Definition { get; }
        public DefenseAbilityRuntime Ability { get; }

        public DefenseSkillCastRuntime(IDefenseGameplayContext gameplay, DefenseSkillDefinition definition, DefenseAbilityRuntime sharedAbility = null)
        {
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (sharedAbility != null && !sharedAbility.BelongsTo(gameplay)) throw new ArgumentException("Shared ability must belong to the same session.");
            Ability = sharedAbility ?? new DefenseAbilityRuntime(gameplay);
            _ownsAbility = sharedAbility == null;
        }

        public bool TryCast(DefenseSkillCastRequest request, DefensePoint3 actorPosition, bool actorAvailable,
            IDefenseSkillEffect effect)
        {
            if (_disposed || _casting || !actorAvailable || effect == null || !ValidScope(request) ||
                !ValidGeometry(request.Aim, actorPosition) || Ability.RemainingCooldown > 0f) return false;
            if (effect is IDefenseSharedAbilityEffect owner && !ReferenceEquals(owner.SharedAbility, Ability)) return false;
            _casting = true;
            try
            {
                if (!effect.TryPrepare(request.Aim, out var prepared) || !ValidScope(request) ||
                    !ValidGeometry(prepared, actorPosition) || !Ability.TryActivate(effect is IDefenseSharedAbilityEffect shared ? shared.CastCooldown : Definition.Cooldown, actorAvailable)) return false;
                // Effects are synchronous after preparation; commit before callbacks to block reentrant casts.
                effect.Execute(prepared);
                return true;
            }
            finally { _casting = false; }
        }

        private bool ValidScope(DefenseSkillCastRequest request) => _gameplay.AdvancesGameplay &&
            ReferenceEquals(request.SessionIdentity, _gameplay.SessionIdentity) && request.RunGeneration == _gameplay.RunGeneration;

        private bool ValidGeometry(DefenseResolvedAim aim, DefensePoint3 actorPosition)
        {
            if (!aim.Origin.IsFinite || !aim.Target.IsFinite || !aim.Direction.IsFinite || !actorPosition.IsFinite ||
                !DefenseDamageRules.IsFinite(aim.Distance) || aim.Distance < 0f || aim.Distance > Definition.Range + 0.001f)
                return false;
            double ox = (double)aim.Origin.X - actorPosition.X, oz = (double)aim.Origin.Z - actorPosition.Z;
            double x = (double)aim.Target.X - actorPosition.X, z = (double)aim.Target.Z - actorPosition.Z;
            return ox * ox + oz * oz <= 0.01d && x * x + z * z <= (double)(Definition.Range + 0.001f) * (Definition.Range + 0.001f);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_ownsAbility) Ability.Dispose();
        }
    }
}

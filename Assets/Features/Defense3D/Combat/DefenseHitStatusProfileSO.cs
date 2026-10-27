using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Optional post-hit control effects, authored independently of skill damage/cooldown.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Hit Status Profile", fileName = "RelicStatus_SongQue")]
    public sealed class DefenseHitStatusProfileSO : ScriptableObject
    {
        [SerializeField] private float _slowDuration = 2f;
        [SerializeField, Range(0f, 1f)] private float _slowStrength = 0.35f;
        [SerializeField] private float _stunDuration = 0.25f;
        [SerializeField] private float _knockDuration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _knockStrength = 0.5f;
        public bool IsValid => Duration(_slowDuration) && Duration(_stunDuration) && Duration(_knockDuration) &&
            Strength(_slowStrength) && Strength(_knockStrength);
        private static bool Duration(float value) => DefenseDamageRules.IsFinite(value) && value >= 0f;
        private static bool Strength(float value) => DefenseDamageRules.IsFinite(value) && value >= 0f && value <= 1f;
        public void Apply(IDefenseStatusReceiver target, IDefenseGameplayContext gameplay, DefensePoint3 direction, int generation)
        {
            if (!IsValid || target == null || gameplay == null || !gameplay.AdvancesGameplay || gameplay.RunGeneration != generation) return;
            if (_slowDuration > 0f) target.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Slow, _slowDuration, _slowStrength, default, gameplay.SessionIdentity, generation));
            if (_stunDuration > 0f) target.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Stun, _stunDuration, 1f, default, gameplay.SessionIdentity, generation));
            if (_knockDuration > 0f) target.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Knockback, _knockDuration, _knockStrength, direction, gameplay.SessionIdentity, generation));
        }
    }
}

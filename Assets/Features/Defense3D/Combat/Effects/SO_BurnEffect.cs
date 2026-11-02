using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(fileName = "SO_Effect_Burn", menuName = "TD/Effects/Burn Effect")]
    public sealed class SO_BurnEffect : SO_AttackEffect
    {
        [SerializeField, Min(.1f)] private float _duration = 3f;
        [SerializeField, Min(.01f)] private float _damagePerSecondRatio = .2f;
        public float Duration => _duration;
        public float DamagePerSecondRatio => _damagePerSecondRatio;
        public override void ApplyEffect(Transform target, float baseDamage, IDefenseGameplayContext gameplay = null)
        {
            if (target == null || gameplay == null || !ShouldTrigger()) return;
            if (target.TryGetComponent<DefenseEnemyStatus3D>(out var status))
                status.TryApplyBurn(baseDamage * _damagePerSecondRatio, _duration, gameplay);
        }
    }
}

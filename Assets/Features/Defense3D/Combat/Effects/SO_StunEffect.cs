using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Hiệu ứng làm choáng (Stun), ngăn chặn mọi hành động của mục tiêu.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Effect_Stun", menuName = "TD/Effects/Stun Effect")]
    public class SO_StunEffect : SO_AttackEffect
    {
        [Header("── THÔNG SỐ CHOÁNG ──")]
        [Tooltip("Thời gian làm choáng mục tiêu (giây)")]
        [SerializeField, Min(0.1f)] private float _duration = 1.2f;

        public float Duration => _duration;

        public override void ApplyEffect(Transform target, float baseDamage, IDefenseGameplayContext gameplay = null)
        {
            if (target == null || !ShouldTrigger()) return;

            if (target.TryGetComponent<IDefenseStatusReceiver>(out var receiver))
            {
                var request = new DefenseStatusRequest(
                    kind: DefenseStatusKind.Stun,
                    duration: _duration,
                    strength: 1f,
                    direction: default,
                    sessionIdentity: gameplay?.SessionIdentity,
                    runGeneration: gameplay?.RunGeneration ?? -1);

                receiver.TryApplyStatus(request);
            }
        }
    }
}

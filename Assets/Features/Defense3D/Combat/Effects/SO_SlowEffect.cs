using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Hiệu ứng làm chậm tốc độ di chuyển của mục tiêu.
    /// Kế thừa SO_AttackEffect và tích hợp trực tiếp vào IDefenseStatusReceiver / DefenseEnemyStatus3D.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Effect_Slow", menuName = "TD/Effects/Slow Effect")]
    public class SO_SlowEffect : SO_AttackEffect
    {
        [Header("── THÔNG SỐ LÀM CHẬM ──")]
        [Tooltip("Tỷ lệ giảm tốc độ di chuyển (0.1 = giảm 10%, 0.5 = giảm 50%)")]
        [SerializeField, Range(0.1f, 0.9f)] private float _slowPercentage = 0.3f;

        [Tooltip("Thời gian duy trì hiệu ứng (giây)")]
        [SerializeField, Min(0.1f)] private float _duration = 2f;

        public float SlowPercentage => _slowPercentage;
        public float Duration => _duration;

        public override void ApplyEffect(Transform target, float baseDamage, IDefenseGameplayContext gameplay = null)
        {
            if (target == null || !ShouldTrigger()) return;

            // 1. Tương thích với hệ thống Status Runtime của Defense3D
            if (target.TryGetComponent<IDefenseStatusReceiver>(out var receiver))
            {
                var request = new DefenseStatusRequest(
                    kind: DefenseStatusKind.Slow,
                    duration: _duration,
                    strength: _slowPercentage,
                    direction: default,
                    sessionIdentity: gameplay?.SessionIdentity,
                    runGeneration: gameplay?.RunGeneration ?? -1);

                receiver.TryApplyStatus(request);
            }
        }
    }
}

using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Abstract base class cho các hiệu ứng đòn đánh dạng lắp ghép (Modular Strategy).
    /// Cho phép Designer kéo thả các hiệu ứng (Làm chậm, Choáng, Thiêu đốt, Đẩy lùi...) vào Tháp hoặc Kỹ năng trên Inspector.
    /// </summary>
    public abstract class SO_AttackEffect : ScriptableObject
    {
        [Tooltip("Tên định danh của hiệu ứng")]
        [SerializeField] private string _effectName = "New Effect";

        [Tooltip("Tỷ lệ cơ hội kích hoạt hiệu ứng (0 = 0%, 1 = 100%)")]
        [SerializeField, Range(0f, 1f)] private float _triggerChance = 1f;

        public string EffectName => _effectName;
        public float TriggerChance => _triggerChance;

        /// <summary>
        /// Áp dụng hiệu ứng lên mục tiêu.
        /// </summary>
        /// <param name="target">Transform của thực thể bị tấn công</param>
        /// <param name="baseDamage">Sát thương cơ bản của đòn đánh</param>
        /// <param name="gameplay">Context phiên chơi (tùy chọn)</param>
        public abstract void ApplyEffect(Transform target, float baseDamage, IDefenseGameplayContext gameplay = null);

        /// <summary>
        /// Quá tải tương thích chữ ký chuẩn theo Sổ tay kiến trúc.
        /// </summary>
        public virtual void ApplyEffect(Transform target, int baseDamage)
        {
            ApplyEffect(target, (float)baseDamage, null);
        }

        protected bool ShouldTrigger()
        {
            if (_triggerChance >= 1f) return true;
            if (_triggerChance <= 0f) return false;
            return Random.value <= _triggerChance;
        }
    }
}

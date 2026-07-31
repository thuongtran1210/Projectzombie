using UnityEngine;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Lớp cơ sở ScriptableObject trừu tượng định nghĩa Factory Pattern cho Kỹ năng Chủ động (Signature Skill).
    /// Giúp mở rộng số lượng nhân vật/kỹ năng mới mà KHÔNG CẦN sửa code trong SignatureSkillManager (Tuân thủ Open/Closed Principle).
    /// </summary>
    public abstract class SignatureSkillData : ScriptableObject
    {
        [Header("Skill Information")]
        [SerializeField] private string _skillName;
        [TextArea(2, 4)]
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private float _baseCooldown = 25f;

        public string SkillName => _skillName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float BaseCooldown => _baseCooldown;

        /// <summary>
        /// Factory Method tạo ra Instance thực thi logic của ISignatureSkill.
        /// </summary>
        public abstract ISignatureSkill CreateSkill();
    }
}

using UnityEngine;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// ScriptableObject cấu hình Kỹ năng Chủ động Võ Tăng: "Phá Giới Chấn Thế".
    /// </summary>
    [CreateAssetMenu(fileName = "VoTangSignatureSkill", menuName = "ProjectZombie/Skills/Vo Tang Skill Data")]
    public class VoTangSkillData : SignatureSkillData
    {
        public override ISignatureSkill CreateSkill()
        {
            return new VoTangSignatureSkill();
        }
    }
}

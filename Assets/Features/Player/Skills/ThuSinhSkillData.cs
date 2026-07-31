using UnityEngine;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// ScriptableObject cấu hình Kỹ năng Chủ động Thư Sinh: "Phán Quyết Tiền Định".
    /// </summary>
    [CreateAssetMenu(fileName = "ThuSinhSignatureSkill", menuName = "ProjectZombie/Skills/Thu Sinh Skill Data")]
    public class ThuSinhSkillData : SignatureSkillData
    {
        public override ISignatureSkill CreateSkill()
        {
            return new ThuSinhSignatureSkill();
        }
    }
}

using UnityEngine;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// ScriptableObject cấu hình Kỹ năng Chủ động Đạo Sĩ: "Bát Quái Trận Đồ".
    /// </summary>
    [CreateAssetMenu(fileName = "DaoSiSignatureSkill", menuName = "ProjectZombie/Skills/Dao Si Skill Data")]
    public class DaoSiSkillData : SignatureSkillData
    {
        [Header("Dao Si Specific Settings")]
        [Tooltip("Prefab vùng Bát Quái Trận (BatQuaiTranZone). Để trống nếu muốn dùng Fallback tự động trong code.")]
        [SerializeField] private GameObject _zonePrefab;

        public GameObject ZonePrefab => _zonePrefab;

        public override ISignatureSkill CreateSkill()
        {
            return new DaoSiSignatureSkill(_zonePrefab);
        }
    }
}

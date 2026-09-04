using UnityEngine;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// ScriptableObject cấu hình Kỹ năng Chủ động Thanh Đồng: "Giá Đồng" (Hầu Đồng Tứ Phủ).
    /// </summary>
    [CreateAssetMenu(fileName = "ThanhDongSignatureSkill", menuName = "ProjectZombie/Skills/Thanh Dong Skill Data")]
    public class ThanhDongSkillData : SignatureSkillData
    {
        [Header("Thanh Dong (Tu Phu) Specific Settings")]
        [Tooltip("Prefab hiệu ứng Hào Quang Tứ Phủ / Dải lụa khi nhập vai vị Thánh.")]
        [SerializeField] private GameObject _tuPhuAuraPrefab;

        [Tooltip("Prefab sóng xung kích khi Thánh Giáng Ngự.")]
        [SerializeField] private GameObject _shockwavePrefab;

        public GameObject TuPhuAuraPrefab => _tuPhuAuraPrefab;
        public GameObject ShockwavePrefab => _shockwavePrefab;

        public override ISignatureSkill CreateSkill()
        {
            return new ThanhDongSignatureSkill(_tuPhuAuraPrefab, _shockwavePrefab);
        }
    }
}

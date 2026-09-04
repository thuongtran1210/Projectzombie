using UnityEngine;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// ScriptableObject cấu hình Kỹ năng Chủ động Ẩn Sĩ Sơn Lâm: "Thập Phương Chấn Thế" (GDD v5.1).
    /// </summary>
    [CreateAssetMenu(fileName = "VoTangSignatureSkill", menuName = "ProjectZombie/Skills/Vo Tang Skill Data")]
    public class VoTangSkillData : SignatureSkillData
    {
        [Header("An Si VFX Settings")]
        [Tooltip("Prefab Sóng Địa Chấn Nứt Đất Bùng Nổ")]
        [SerializeField] private GameObject _shockwavePrefab;

        [Tooltip("Prefab Vỡ Đá / Trảm Địa Khí")]
        [SerializeField] private GameObject _earthImpactPrefab;

        public GameObject ShockwavePrefab => _shockwavePrefab;
        public GameObject EarthImpactPrefab => _earthImpactPrefab;

        public override ISignatureSkill CreateSkill()
        {
            return new VoTangSignatureSkill(_shockwavePrefab, _earthImpactPrefab);
        }
    }
}

using UnityEngine;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// ScriptableObject cấu hình Kỹ năng Chủ động Thư Sinh: "Phán Quyết Tiền Định" / "Phán Quyết Âm Ty".
    /// </summary>
    [CreateAssetMenu(fileName = "ThuSinhSignatureSkill", menuName = "ProjectZombie/Skills/Thu Sinh Skill Data")]
    public class ThuSinhSkillData : SignatureSkillData
    {
        [Header("Thu Sinh VFX Settings")]
        [Tooltip("Prefab Vòng Cổ Tự Thư Pháp phát quang trên mặt đất")]
        [SerializeField] private GameObject _groundDecalPrefab;

        [Tooltip("Prefab Vệt Mực Thư Pháp Chém Xoáy")]
        [SerializeField] private GameObject _inkSlashPrefab;

        [Tooltip("Prefab Sét Phán Quan")]
        [SerializeField] private GameObject _lightningPrefab;

        public GameObject GroundDecalPrefab => _groundDecalPrefab;
        public GameObject InkSlashPrefab => _inkSlashPrefab;
        public GameObject LightningPrefab => _lightningPrefab;

        public override ISignatureSkill CreateSkill()
        {
            return new ThuSinhSignatureSkill(_groundDecalPrefab, _inkSlashPrefab, _lightningPrefab);
        }
    }
}

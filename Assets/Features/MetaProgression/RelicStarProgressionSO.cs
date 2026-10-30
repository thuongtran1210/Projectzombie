using System;
using UnityEngine;

namespace ProjectZombie.Features.MetaProgression
{
    [Serializable]
    public class RelicStarStepConfig
    {
        [Tooltip("Cấp sao mục tiêu sau khi gộp thành công (1..5)")]
        public int targetStar = 1;

        [Tooltip("Số thẻ mảnh yêu cầu")]
        public int requiredShards = 5;

        [Tooltip("Chi phí Cổ Tiền yêu cầu")]
        public int coTienCost = 0;

        [Tooltip("Hệ số tăng Sát thương Pháp Bảo (Ví dụ 0.15 = +15%)")]
        public float damageMultiplierBonus = 0f;

        [Tooltip("Hệ số giảm thời gian hồi chiêu Pháp Bảo (Ví dụ 0.15 = -15% CD)")]
        public float cooldownReductionPercent = 0f;

        [Tooltip("Hệ số tăng Phạm vi tác động Pháp Bảo (Ví dụ 0.30 = +30% Area)")]
        public float areaMultiplierBonus = 0f;

        [Tooltip("Hệ số tăng Sinh lực Quân Lính Làng tại mốc sao này (Ví dụ 0.15 = +15% HP)")]
        public float minionHpMultiplierBonus = 0f;

        [Tooltip("Hệ số tăng Tốc độ chạy Quân Lính Làng tại mốc sao này (Ví dụ 0.05 = +5% Speed)")]
        public float minionSpeedMultiplierBonus = 0f;

        [Tooltip("Hệ số tăng Sát thương Trụ Phòng Thủ tại mốc sao này (Ví dụ 0.15 = +15% Damage)")]
        public float towerDamageMultiplierBonus = 0f;

        [Tooltip("Hệ số tăng Tầm bắn Trụ Phòng Thủ tại mốc sao này (Ví dụ 0.10 = +10% Range)")]
        public float towerRangeMultiplierBonus = 0f;

        [Tooltip("Mô tả hiệu ứng đặc biệt / Thức tỉnh khi đạt mốc sao này")]
        public string perkDescription = "";
    }

    /// <summary>
    /// Cấu hình Data-Driven toàn bộ các bước gộp thẻ và tăng sao Pháp Bảo, Quân Lính và Trụ Phòng Thủ.
    /// Cho phép Game Designer chỉnh sửa độc lập trên Inspector (Single Source of Truth).
    /// </summary>
    [CreateAssetMenu(fileName = "RelicStarProgressionConfig", menuName = "ProjectZombie/Progression/Relic Star Progression Config")]
    public class RelicStarProgressionSO : ScriptableObject
    {
        [Header("Bảng Cấu Hình 5 Cấp Sao")]
        [SerializeField]
        private RelicStarStepConfig[] _starSteps = new RelicStarStepConfig[]
        {
            new RelicStarStepConfig
            {
                targetStar = 1,
                requiredShards = 5,
                coTienCost = 0,
                perkDescription = "Mở khóa sở hữu xuất trận."
            },
            new RelicStarStepConfig
            {
                targetStar = 2,
                requiredShards = 10,
                coTienCost = 500,
                damageMultiplierBonus = 0.15f,
                minionHpMultiplierBonus = 0.15f,
                minionSpeedMultiplierBonus = 0.05f,
                towerDamageMultiplierBonus = 0.15f,
                towerRangeMultiplierBonus = 0.10f,
                perkDescription = "Tăng +15% Sát thương kỹ năng."
            },
            new RelicStarStepConfig
            {
                targetStar = 3,
                requiredShards = 20,
                coTienCost = 1500,
                cooldownReductionPercent = 0.15f,
                minionHpMultiplierBonus = 0.30f,
                minionSpeedMultiplierBonus = 0.10f,
                towerDamageMultiplierBonus = 0.30f,
                towerRangeMultiplierBonus = 0.15f,
                perkDescription = "Giảm 15% Thời gian hồi chiêu."
            },
            new RelicStarStepConfig
            {
                targetStar = 4,
                requiredShards = 40,
                coTienCost = 3500,
                areaMultiplierBonus = 0.30f,
                minionHpMultiplierBonus = 0.50f,
                minionSpeedMultiplierBonus = 0.15f,
                towerDamageMultiplierBonus = 0.50f,
                towerRangeMultiplierBonus = 0.25f,
                perkDescription = "Tăng +30% Phạm vi tác động kỹ năng."
            },
            new RelicStarStepConfig
            {
                targetStar = 5,
                requiredShards = 80,
                coTienCost = 8000,
                damageMultiplierBonus = 0.30f,
                minionHpMultiplierBonus = 0.75f,
                minionSpeedMultiplierBonus = 0.20f,
                towerDamageMultiplierBonus = 0.75f,
                towerRangeMultiplierBonus = 0.35f,
                perkDescription = "Thức Tỉnh Thần Binh: Tỏa hào quang và tăng sát thương tối thượng."
            }
        };

        public RelicStarStepConfig[] StarSteps => _starSteps;

        public RelicStarStepConfig GetStepConfig(int targetStar)
        {
            if (_starSteps == null) return null;
            for (int i = 0; i < _starSteps.Length; i++)
            {
                if (_starSteps[i].targetStar == targetStar)
                    return _starSteps[i];
            }
            return null;
        }

        public int MaxStarLevel => 5;
    }
}

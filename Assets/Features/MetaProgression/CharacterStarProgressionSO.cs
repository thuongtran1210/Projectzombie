using System;
using UnityEngine;

namespace ProjectZombie.Features.MetaProgression
{
    [Serializable]
    public class CharacterStarStepConfig
    {
        [Tooltip("Cấp sao mục tiêu sau khi gộp thành công (1..5)")]
        public int targetStar = 1;

        [Tooltip("Số thẻ mảnh yêu cầu")]
        public int requiredShards = 10;

        [Tooltip("Chi phí Cổ Tiền yêu cầu")]
        public int coTienCost = 0;

        [Tooltip("Hệ số tăng Máu tối đa (Ví dụ 0.10 = +10% HP)")]
        public float healthMultiplierBonus = 0f;

        [Tooltip("Hệ số tăng Sát thương cơ bản (Ví dụ 0.10 = +10% Damage)")]
        public float damageMultiplierBonus = 0f;

        [Tooltip("Hệ số tăng Tốc độ di chuyển (Ví dụ 0.05 = +5% Move Speed)")]
        public float moveSpeedMultiplierBonus = 0f;

        [Tooltip("Hệ số giảm thời gian hồi chiêu Kỹ năng chủ động (Ví dụ 0.10 = -10% CD)")]
        public float cooldownReductionBonus = 0f;

        [Tooltip("Mô tả hiệu ứng đặc biệt / Thức tỉnh khi đạt mốc sao này")]
        public string perkDescription = "";
    }

    /// <summary>
    /// Cấu hình Data-Driven toàn bộ các bước gộp thẻ và tăng sao Tướng / Anh Hùng.
    /// Cho phép Game Designer tùy biến độc lập trên Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterStarProgressionConfig", menuName = "ProjectZombie/Progression/Character Star Progression Config")]
    public class CharacterStarProgressionSO : ScriptableObject
    {
        [Header("Bảng Cấu Hình 5 Cấp Sao Tướng")]
        [SerializeField]
        private CharacterStarStepConfig[] _starSteps = new CharacterStarStepConfig[]
        {
            new CharacterStarStepConfig { targetStar = 1, requiredShards = 10, coTienCost = 0, healthMultiplierBonus = 0f, damageMultiplierBonus = 0f, perkDescription = "Mở khóa xuất trận cùng kỹ năng bản mệnh." },
            new CharacterStarStepConfig { targetStar = 2, requiredShards = 20, coTienCost = 800, healthMultiplierBonus = 0.10f, damageMultiplierBonus = 0.05f, perkDescription = "Tăng +10% Máu tối đa & +5% Sát thương bản mệnh." },
            new CharacterStarStepConfig { targetStar = 3, requiredShards = 40, coTienCost = 2500, cooldownReductionBonus = 0.15f, perkDescription = "Giảm 15% Hồi chiêu Kỹ năng chủ động." },
            new CharacterStarStepConfig { targetStar = 4, requiredShards = 80, coTienCost = 6000, healthMultiplierBonus = 0.20f, damageMultiplierBonus = 0.15f, perkDescription = "Tăng +20% Máu tối đa & +15% Sát thương toàn diện." },
            new CharacterStarStepConfig { targetStar = 5, requiredShards = 150, coTienCost = 15000, damageMultiplierBonus = 0.25f, moveSpeedMultiplierBonus = 0.10f, perkDescription = "Thức Tỉnh Chân Thân: Tăng 25% Sát thương, +10% Tốc chạy & Kích hoạt Hào Quang Thần Thoại." }
        };

        public CharacterStarStepConfig[] StarSteps => _starSteps;

        public CharacterStarStepConfig GetStepConfig(int targetStar)
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

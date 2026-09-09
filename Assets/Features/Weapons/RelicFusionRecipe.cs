using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Định nghĩa công thức Luyện Hóa / Gộp Thẻ tạo Pháp Bảo Thần Binh theo kiến trúc Data-Driven.
    /// Cho phép ghép 1 Vũ khí/Pháp bảo Base + Nhiều Thẻ Bị Động/Phù Chú thành Pháp Bảo Tối Thượng.
    /// </summary>
    [System.Serializable]
    public struct RelicFusionRecipe
    {
        [Tooltip("Mã định danh công thức Luyện Hóa (VD: FUSION_SLIPPER_FIRE, FUSION_POT_GOLD)")]
        public string fusionId;

        [Tooltip("Tên hiển thị của Thần Binh sau khi Luyện Hóa thành công")]
        public string displayName;

        [Tooltip("ID Vũ khí / Pháp bảo cơ sở cần để Luyện Hóa (VD: W_SLIPPER, W_POT). Có thể để trống nếu chỉ ghép từ các thẻ Passive.")]
        public string requiredBaseWeaponId;

        [Tooltip("Cấp độ vũ khí cơ sở yêu cầu (Mặc định = 5/6 tương đương Max Level)")]
        public int requiredBaseWeaponLevel;

        [Tooltip("Danh sách các mã ID Thẻ Bị Động / Phù Chú / Bí Kíp cần thiết để Luyện Hóa")]
        public List<string> requiredPassiveIds;

        [Tooltip("Prefab của Pháp Bảo Thần Binh mới sẽ được khởi tạo và trang bị")]
        public GameObject resultRelicPrefab;

        [TextArea(2, 4)]
        [Tooltip("Mô tả sức mạnh đặc trưng của Pháp Bảo Thần Binh sau khi Luyện Hóa")]
        public string description;

        public RelicFusionRecipe(string fusionId, string displayName, string requiredBaseWeaponId, int requiredBaseWeaponLevel, List<string> requiredPassiveIds, GameObject resultRelicPrefab, string description)
        {
            this.fusionId = fusionId;
            this.displayName = displayName;
            this.requiredBaseWeaponId = requiredBaseWeaponId;
            this.requiredBaseWeaponLevel = requiredBaseWeaponLevel;
            this.requiredPassiveIds = requiredPassiveIds ?? new List<string>();
            this.resultRelicPrefab = resultRelicPrefab;
            this.description = description;
        }
    }
}

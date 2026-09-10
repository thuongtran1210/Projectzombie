using System;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.MetaProgression.Gacha.Data
{
    public enum GachaDropType
    {
        RelicShard,     // Mảnh Pháp Bảo / Thần Binh
        CharacterShard  // Mảnh Thẻ Tướng / Anh Hùng
    }

    /// <summary>
    /// Định nghĩa 1 phần thưởng trong Pool Gacha Rương (Pháp Bảo hoặc Tướng).
    /// </summary>
    [Serializable]
    public class GachaDropItem
    {
        [Tooltip("Loại phần thưởng: Mảnh Pháp Bảo hoặc Mảnh Tướng")]
        public GachaDropType dropType = GachaDropType.RelicShard;

        [Tooltip("Mã định danh của Pháp Bảo (khớp với weaponId trong WeaponData) hoặc characterId của Tướng.")]
        public string relicId;

        [Tooltip("Tên hiển thị của Vật Phẩm / Tướng.")]
        public string relicName;

        [Tooltip("Độ hiếm của Pháp Bảo.")]
        public ItemRarity rarity = ItemRarity.Common;

        [Tooltip("Hệ Ngũ Hành của Pháp Bảo.")]
        public ElementType element = ElementType.None;

        [Tooltip("Số lượng mảnh nhận được khi quay trúng.")]
        public int shardAmount = 5;

        [Tooltip("Trọng số xuất hiện cơ bản (càng cao càng dễ ra).")]
        public float weight = 100f;

        [Tooltip("Sprite Icon của Pháp Bảo.")]
        public Sprite icon;
    }
}

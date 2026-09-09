using System;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.MetaProgression.Gacha.Data
{
    /// <summary>
    /// Định nghĩa 1 phần thưởng trong Pool Gacha Rương Pháp Bảo.
    /// </summary>
    [Serializable]
    public class GachaDropItem
    {
        [Tooltip("Mã định danh của Pháp Bảo (khớp với weaponId trong WeaponData).")]
        public string relicId;

        [Tooltip("Tên Pháp Bảo (hiển thị / debug).")]
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

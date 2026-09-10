using System;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.MetaProgression.Gacha.Data
{
    /// <summary>
    /// Kết quả trả về sau mỗi lượt mở Gacha Rương.
    /// </summary>
    [Serializable]
    public struct GachaDropResult
    {
        public GachaDropType dropType;
        public string relicId;
        public string relicName;
        public ItemRarity rarity;
        public ElementType element;
        public int shardCount;
        public bool isNewUnlock;        // Lần đầu mở khóa (chưa từng sở hữu)
        public int currentStarLevel;   // Cấp sao hiện tại sau khi cộng mảnh
        public int totalShardsAfter;   // Tổng số mảnh hiện có sau khi cộng
        public UnityEngine.Sprite icon;
        public bool isConvertedToCurrency;      // Cờ báo hiệu đã max 5 sao -> quy đổi sang Cổ Tiền
        public int convertedCurrencyAmount;    // Số Cổ Tiền nhận được do quy đổi
    }
}

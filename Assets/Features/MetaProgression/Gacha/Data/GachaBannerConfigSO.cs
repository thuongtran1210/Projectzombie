using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.MetaProgression.Gacha.Data
{
    /// <summary>
    /// Cấu hình Banner Gacha Rương Pháp Bảo (Bảo Rương Vạn Cổ) dạng ScriptableObject.
    /// Cho phép Game Designer tinh chỉnh tỉ lệ, bảo hiểm (Pity) và pool drop không cần can thiệp code.
    /// </summary>
    [CreateAssetMenu(fileName = "GachaBannerConfig", menuName = "ProjectZombie/Gacha/Banner Config")]
    public class GachaBannerConfigSO : ScriptableObject
    {
        [Header("Thông Tin Banner")]
        [Tooltip("Mã định danh Banner (VD: banner_standard, banner_fire_element)")]
        public string bannerId = "banner_standard";

        [Tooltip("Tên hiển thị của Banner")]
        public string bannerName = "Bảo Rương Vạn Cổ";

        [TextArea(2, 4)]
        [Tooltip("Mô tả banner & quy tắc mở rương")]
        public string bannerDescription = "Mở rương thu thập Mảnh Pháp Bảo Thần Binh viễn cổ, gia tăng uy lực vĩnh viễn.";

        [Header("Chi Phí Quay (Cổ Tiền / Tiền Tệ)")]
        [Tooltip("Chi phí quay 1 lượt")]
        public int singleRollCost = 100;

        [Tooltip("Chi phí quay 10 lượt (đã chiết khấu)")]
        public int multiRollCost = 900;

        [Header("Cơ Chế Bảo Hiểm (Pity System)")]
        [Tooltip("Hard Pity: Số lượt tối đa chắc chắn nhận Thần Binh (Legendary)")]
        public int hardPityLegendary = 50;

        [Tooltip("Soft Pity: Mốc bắt đầu tăng mạnh tỉ lệ ra Thần Binh")]
        public int softPityStart = 40;

        [Tooltip("Tỉ lệ cộng thêm mỗi lượt từ sau mốc Soft Pity (VD: 0.05 = 5%)")]
        public float softPityRatePerRoll = 0.05f;

        [Tooltip("Bảo hiểm Cực Phẩm: Số lượt tối đa chắc chắn ra phẩm Cực Phẩm (Epic) trở lên")]
        public int epicGuaranteedEvery = 10;

        [Header("Quy Đổi Mảnh Dư Khi Đạt 5★ (Cổ Tiền / Mảnh)")]
        [Tooltip("Số Cổ Tiền nhận lại trên mỗi mảnh Common dư")]
        public int commonShardConvertRate = 20;

        [Tooltip("Số Cổ Tiền nhận lại trên mỗi mảnh Rare dư")]
        public int rareShardConvertRate = 40;

        [Tooltip("Số Cổ Tiền nhận lại trên mỗi mảnh Epic dư")]
        public int epicShardConvertRate = 80;

        [Tooltip("Số Cổ Tiền nhận lại trên mỗi mảnh Legendary dư")]
        public int legendaryShardConvertRate = 200;

        [Header("Danh Sách Pool Vật Phẩm")]
        [SerializeField] private List<GachaDropItem> _dropPool = new List<GachaDropItem>();

        public List<GachaDropItem> DropPool => _dropPool;

        public void SetDropPool(List<GachaDropItem> pool)
        {
            _dropPool = pool;
        }

        public int GetDuplicateShardConversionRate(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => commonShardConvertRate,
                ItemRarity.Rare => rareShardConvertRate,
                ItemRarity.Epic => epicShardConvertRate,
                ItemRarity.Legendary => legendaryShardConvertRate,
                _ => commonShardConvertRate
            };
        }
    }
}

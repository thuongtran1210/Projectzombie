using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Định nghĩa các phẩm cấp độ hiếm chuẩn mực cho toàn bộ hệ thống (Vũ khí, Gacha, Thẻ bài).
    /// </summary>
    public enum ItemRarity
    {
        Common = 0,     // Phổ Thông (Trắng)
        Rare = 1,       // Bảo Phẩm (Lam)
        Epic = 2,       // Cực Phẩm (Tím)
        Legendary = 3   // Thần Binh / Hoàng Kim (Vàng)
    }

    /// <summary>
    /// Các phương thức mở rộng hỗ trợ hiển thị UI và màu sắc Rich Text TextMeshPro cho ItemRarity.
    /// </summary>
    public static class ItemRarityExtensions
    {
        public static string GetDisplayName(this ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return "Phổ Thông";
                case ItemRarity.Rare: return "Bảo Phẩm";
                case ItemRarity.Epic: return "Cực Phẩm";
                case ItemRarity.Legendary: return "Thần Binh";
                default: return "Phổ Thông";
            }
        }

        public static string GetHexColor(this ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return "#D1D5DB";     // Xám bạc sáng
                case ItemRarity.Rare: return "#38BDF8";       // Lam ngọc
                case ItemRarity.Epic: return "#C084FC";       // Tử quang (Tím)
                case ItemRarity.Legendary: return "#FBBF24";  // Hoàng kim
                default: return "#FFFFFF";
            }
        }

        public static Color GetColor(this ItemRarity rarity)
        {
            ColorUtility.TryParseHtmlString(rarity.GetHexColor(), out Color color);
            return color;
        }

        public static string GetFormattedName(this ItemRarity rarity, string itemName)
        {
            return $"<color={rarity.GetHexColor()}><b>[{rarity.GetDisplayName()}]</b> {itemName}</color>";
        }
    }
}

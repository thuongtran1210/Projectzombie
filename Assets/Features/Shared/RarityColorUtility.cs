using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Các cấp độ độ hiếm (Rarity) trong game.
    /// </summary>
    public enum Rarity
    {
        Common,     // Trắng
        Uncommon,   // Xanh lá
        Rare,       // Xanh lam
        Epic,       // Tím
        Legendary,  // Cam
        Mythic      // Đỏ
    }

    /// <summary>
    /// Tiện ích hỗ trợ lấy màu sắc cho UI dựa trên Rarity.
    /// </summary>
    public static class RarityColorUtility
    {
        // Các mã màu HEX dùng cho Rich Text (TextMeshPro)
        public const string CommonHex = "#FFFFFF";      // Trắng
        public const string UncommonHex = "#32CD32";    // Xanh lá
        public const string RareHex = "#1E90FF";        // Xanh lam
        public const string EpicHex = "#8A2BE2";        // Tím
        public const string LegendaryHex = "#FFA500";   // Cam
        public const string MythicHex = "#FF0000";      // Đỏ

        /// <summary>
        /// Trả về object Color dựa trên Rarity, dùng cho Image hoặc Text.color.
        /// </summary>
        public static Color GetColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: 
                    return Color.white;
                case Rarity.Uncommon: 
                    ColorUtility.TryParseHtmlString(UncommonHex, out Color uncommon);
                    return uncommon;
                case Rarity.Rare: 
                    ColorUtility.TryParseHtmlString(RareHex, out Color rare);
                    return rare;
                case Rarity.Epic: 
                    ColorUtility.TryParseHtmlString(EpicHex, out Color epic);
                    return epic;
                case Rarity.Legendary: 
                    ColorUtility.TryParseHtmlString(LegendaryHex, out Color legendary);
                    return legendary;
                case Rarity.Mythic: 
                    return Color.red;
                default: 
                    return Color.white;
            }
        }

        /// <summary>
        /// Trả về mã HEX dựa trên Rarity.
        /// </summary>
        public static string GetHexColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return CommonHex;
                case Rarity.Uncommon: return UncommonHex;
                case Rarity.Rare: return RareHex;
                case Rarity.Epic: return EpicHex;
                case Rarity.Legendary: return LegendaryHex;
                case Rarity.Mythic: return MythicHex;
                default: return CommonHex;
            }
        }

        /// <summary>
        /// Bọc một đoạn text trong thẻ color của TextMeshPro dựa trên cấp độ.
        /// Ví dụ: <color=#FFFFFF>Text</color>
        /// </summary>
        public static string FormatText(string text, Rarity rarity)
        {
            return $"<color={GetHexColor(rarity)}>{text}</color>";
        }
    }
}

using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.Helpers
{
    /// <summary>
    /// Cung cấp các chuỗi Rich Text và mã màu chuẩn phong thủy Ngũ Hành theo GDD v4.0.
    /// </summary>
    public static class ElementVisualHelper
    {
        public const string COLOR_KIM = "#E8C468";
        public const string COLOR_MOC = "#4C7A3D";
        public const string COLOR_THUY = "#29B6F6";
        public const string COLOR_HOA = "#FF5722";
        public const string COLOR_THO = "#D7A87A";

        /// <summary>
        /// Trả về chuỗi Badge kèm Icon và TMP Rich Text màu sắc theo hệ.
        /// </summary>
        public static string GetElementBadgeRichText(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim:
                    return $"<color={COLOR_KIM}>[Kim]</color>";
                case ElementType.Moc:
                    return $"<color={COLOR_MOC}>[Mộc]</color>";
                case ElementType.Thuy:
                    return $"<color={COLOR_THUY}>[Thủy]</color>";
                case ElementType.Hoa:
                    return $"<color={COLOR_HOA}>[Hỏa]</color>";
                case ElementType.Tho:
                    return $"<color={COLOR_THO}>[Thổ]</color>";
                case ElementType.None:
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Lấy mã màu HEX tương ứng với thuộc tính Ngũ Hành.
        /// </summary>
        public static string GetElementHexColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim: return COLOR_KIM;
                case ElementType.Moc: return COLOR_MOC;
                case ElementType.Thuy: return COLOR_THUY;
                case ElementType.Hoa: return COLOR_HOA;
                case ElementType.Tho: return COLOR_THO;
                default: return "#FFFFFF";
            }
        }
    }
}

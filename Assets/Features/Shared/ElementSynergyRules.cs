namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Chứa các quy tắc Domain logic thuần túy về Ngũ Hành (Tương Sinh, Tương Khắc, Đồng Hệ) theo GDD.
    /// </summary>
    public static class ElementSynergyRules
    {
        /// <summary>
        /// Kiểm tra nguyên tắc Ngũ Hành Tương Sinh: 
        /// Kim sinh Thủy, Thủy sinh Mộc, Mộc sinh Hỏa, Hỏa sinh Thổ, Thổ sinh Kim.
        /// </summary>
        /// <param name="parent">Hệ nguyên tố nguồn</param>
        /// <param name="child">Hệ nguyên tố đích</param>
        /// <returns>True nếu parent sinh child</returns>
        public static bool IsElementGenerative(ElementType parent, ElementType child)
        {
            switch (parent)
            {
                case ElementType.Kim: return child == ElementType.Thuy;
                case ElementType.Thuy: return child == ElementType.Moc;
                case ElementType.Moc: return child == ElementType.Hoa;
                case ElementType.Hoa: return child == ElementType.Tho;
                case ElementType.Tho: return child == ElementType.Kim;
                default: return false;
            }
        }

        /// <summary>
        /// Kiểm tra nguyên tắc Ngũ Hành Tương Khắc:
        /// Kim khắc Mộc, Mộc khắc Thổ, Thổ khắc Thủy, Thủy khắc Hỏa, Hỏa khắc Kim.
        /// </summary>
        public static bool IsElementOvercoming(ElementType parent, ElementType child)
        {
            switch (parent)
            {
                case ElementType.Kim: return child == ElementType.Moc;
                case ElementType.Moc: return child == ElementType.Tho;
                case ElementType.Tho: return child == ElementType.Thuy;
                case ElementType.Thuy: return child == ElementType.Hoa;
                case ElementType.Hoa: return child == ElementType.Kim;
                default: return false;
            }
        }

        /// <summary>
        /// Trả về hệ Tương Sinh với hệ hiện tại (parent sinh child).
        /// </summary>
        public static ElementType GetGenerativeChild(ElementType parent)
        {
            switch (parent)
            {
                case ElementType.Kim: return ElementType.Thuy;
                case ElementType.Thuy: return ElementType.Moc;
                case ElementType.Moc: return ElementType.Hoa;
                case ElementType.Hoa: return ElementType.Tho;
                case ElementType.Tho: return ElementType.Kim;
                default: return ElementType.None;
            }
        }
    }
}

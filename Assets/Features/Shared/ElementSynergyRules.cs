namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Chứa các quy tắc Domain logic thuần túy về Ngũ Hành (Tương Sinh, Tương Khắc, Đồng Hệ) theo GDD.
    /// </summary>
    public static class ElementSynergyRules
    {
        public const float HIT_WINDOW_SECONDS = 3f;
        public const float PROC_COOLDOWN_SECONDS = 3f;
        public const float REMAINING_COOLDOWN_REDUCTION = 0.2f;
        public const float BASIC_ATTACK_SPEED_MULTIPLIER = 1.25f;
        public const float ATTACK_SPEED_BUFF_SECONDS = 3f;
        private static ulong _nextAttackId;

        /// <summary>Session-unique identity shared by every target of one attack.</summary>
        public static ulong NextAttackId() => ++_nextAttackId;

        public static ElementType GetGenerativeParent(ElementType child)
        {
            switch (child)
            {
                case ElementType.Kim: return ElementType.Tho;
                case ElementType.Moc: return ElementType.Thuy;
                case ElementType.Thuy: return ElementType.Kim;
                case ElementType.Hoa: return ElementType.Moc;
                case ElementType.Tho: return ElementType.Hoa;
                default: return ElementType.None;
            }
        }
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

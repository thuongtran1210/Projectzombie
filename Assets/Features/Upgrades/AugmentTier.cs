namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Phẩm cấp của Lõi Đột Biến (Mutation Augments) tại các mốc Lv.5, Lv.15, Lv.30.
    /// </summary>
    public enum AugmentTier
    {
        /// <summary>
        /// Lõi Bạc: Tiện ích, kinh tế, chỉ số cơ bản, chuyển hóa sơ khởi (Lv.5).
        /// </summary>
        Silver,

        /// <summary>
        /// Lõi Vàng: Cường hóa giao tranh, khống chế, đốt máu, thạch trụ nổ đất (Lv.5, Lv.15).
        /// </summary>
        Gold,

        /// <summary>
        /// Lõi Kim Cương: Thần hóa tối thượng, bẻ gãy quy tắc trò chơi (Lv.15, Lv.30).
        /// </summary>
        Prismatic
    }
}

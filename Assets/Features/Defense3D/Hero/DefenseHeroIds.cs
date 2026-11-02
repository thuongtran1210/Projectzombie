namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Định danh chuẩn (Canonical IDs) cho toàn bộ Anh Hùng trong chế độ Defense 3D.
    /// Đóng vai trò Single Source of Truth, đồng bộ với Defense3DCatalog và DefenseLoadoutIdMapper.
    /// </summary>
    public static class DefenseHeroIds
    {
        public const string THU_SINH = Defense3DCatalog.HERO_THU_SINH;      // "hero_thusinh"
        public const string DAO_SI = Defense3DCatalog.HERO_DAO_SI;          // "hero_daosi"
        public const string THANH_DONG = Defense3DCatalog.HERO_THANH_DONG;  // "hero_thanhdong"
        public const string AN_SI = Defense3DCatalog.HERO_AN_SI;            // "hero_ansi"

        /// <summary>
        /// Kiểm tra ID có phải là một trong các Canonical ID hợp lệ hay không.
        /// </summary>
        public static bool IsCanonicalHeroId(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId)) return false;
            return heroId == THU_SINH ||
                   heroId == DAO_SI ||
                   heroId == THANH_DONG ||
                   heroId == AN_SI;
        }
    }
}


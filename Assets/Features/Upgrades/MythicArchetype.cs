namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Định danh 5 Đại Trường Phái Thần Thoại Cổ Phong Việt Nam.
    /// </summary>
    public enum MythicArchetype
    {
        None = 0,
        PhuDongThienUy   = 1, // Thánh Gióng (Hệ Hỏa - Thể Tu Khổng Lồ, Ngựa Sắt Phun Lửa, Càn Quét)
        KimQuyThanCo     = 2, // Nỏ Thần An Dương Vương (Hệ Kim - Xạ Kích Vạn Tiễn, Đạn Nảy Xuyên Phá, Mai Rùa)
        TanVienSonThanh  = 3, // Sơn Tinh (Hệ Thổ - Bất Tử Giáp Đá, Thạch Trụ Đè Bẹp Quái, Phản Đòn)
        ThuyBaCuongNo    = 4, // Thủy Tinh (Hệ Thủy - Mưa Bão Toàn Map, Sóng Thần Cuốn Trôi, Băng Tê Liệt)
        LongTienHuyetMach = 5 // Lạc Long Quân & Âu Cơ (Hệ Âm Dương - Chuyển Đổi Rồng/Tiên, Miễn Tử Hồi Sinh)
    }

    public static class MythicArchetypeExtensions
    {
        public static string GetDisplayName(this MythicArchetype archetype)
        {
            switch (archetype)
            {
                case MythicArchetype.PhuDongThienUy:
                    return "Phù Đổng Thiên Uy";
                case MythicArchetype.KimQuyThanCo:
                    return "Kim Quy Thần Cơ";
                case MythicArchetype.TanVienSonThanh:
                    return "Tản Viên Sơn Thánh";
                case MythicArchetype.ThuyBaCuongNo:
                    return "Thủy Bá Cuồng Nộ";
                case MythicArchetype.LongTienHuyetMach:
                    return "Long Tiên Huyết Mạch";
                default:
                    return "Thần Thoại";
            }
        }
    }
}

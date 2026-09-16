namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Định danh 5 Đại Trường Phái Thần Thoại Cổ Phong Việt Nam.
    /// </summary>
    public enum MythicArchetype
    {
        None = 0,
        KimQuyThanCo      = 1, // An Dương Vương & Kim Quy (Hệ KIM - Xạ Kích Đạn Nảy / Trảm Sát Kiếm Khí Nảy, Mai Rùa Thủ)
        LongTienHuyetMach = 2, // Lạc Long Quân & Âu Cơ (Hệ MỘC - Âm Dương Thái Cực: Thái Âm Mộc Hút Máu / Thái Dương Dame)
        ThuyBaCuongNo     = 3, // Thủy Tinh (Hệ THỦY - Băng Hà Ngưng Đọng, Nổ Băng Diện Rộng, Sóng Thần Cuốn Trôi)
        PhuDongThienUy    = 4, // Thánh Gióng (Hệ HỎA - Càn Quét Khổng Lồ, Ngựa Sắt Vệt Lửa Thiêu Rụi)
        TanVienSonThanh   = 5  // Sơn Tinh (Hệ THỔ - Sơn Thạch Phản Sát Thương 150%, Mọc Trụ Đá Thần Sơn Chặn Quái)
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

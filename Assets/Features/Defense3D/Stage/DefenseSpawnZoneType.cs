using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Định danh chuẩn hóa các vùng xuất hiện quái trên bản đồ trận chiến.
    /// Thay thế hoàn toàn việc gõ chuỗi string tự do, loại bỏ 100% nguy cơ gõ sai chính tả.
    /// </summary>
    public enum DefenseSpawnZoneType
    {
        [InspectorName("Bờ Đông (Tiến Công Chính Mặt Đất)")]
        EastBank = 0,

        [InspectorName("Tán Rừng Phía Bắc (Đường Không Kích / Huyết Dơi)")]
        NorthCanopy = 1,

        [InspectorName("Bờ Nam (Đường Đánh Bọc Sườn)")]
        SouthBank = 2,

        [InspectorName("Lòng Kênh Sông Quê (Đường Thủy Quái Lội Nước)")]
        CanalWater = 3,

        [InspectorName("Đấu Trường Quyết Chiến (Vị Trí Boss Dạ Xoa Xuất Hiện)")]
        ArenaEast = 4
    }

    /// <summary>
    /// Các phương thức tiện ích hỗ trợ chuyển đổi 2 chiều giữa Enum vùng xuất hiện và chuỗi định danh gốc.
    /// Bảo toàn tính tương thích ngược với toàn bộ hệ thống Session/Placement cũ.
    /// </summary>
    public static class DefenseSpawnZoneExtensions
    {
        public const string ZONE_EAST_BANK = "eastBank";
        public const string ZONE_NORTH_CANOPY = "northCanopy";
        public const string ZONE_SOUTH_BANK = "southBank";
        public const string ZONE_CANAL_WATER = "canalWater";
        public const string ZONE_ARENA_EAST = "arenaEast";

        public static string ToZoneId(this DefenseSpawnZoneType zone) => zone switch
        {
            DefenseSpawnZoneType.EastBank => ZONE_EAST_BANK,
            DefenseSpawnZoneType.NorthCanopy => ZONE_NORTH_CANOPY,
            DefenseSpawnZoneType.SouthBank => ZONE_SOUTH_BANK,
            DefenseSpawnZoneType.CanalWater => ZONE_CANAL_WATER,
            DefenseSpawnZoneType.ArenaEast => ZONE_ARENA_EAST,
            _ => ZONE_EAST_BANK
        };

        public static DefenseSpawnZoneType FromZoneId(string zoneId) => zoneId switch
        {
            ZONE_EAST_BANK => DefenseSpawnZoneType.EastBank,
            ZONE_NORTH_CANOPY => DefenseSpawnZoneType.NorthCanopy,
            ZONE_SOUTH_BANK => DefenseSpawnZoneType.SouthBank,
            ZONE_CANAL_WATER => DefenseSpawnZoneType.CanalWater,
            ZONE_ARENA_EAST => DefenseSpawnZoneType.ArenaEast,
            _ => DefenseSpawnZoneType.EastBank
        };
    }
}

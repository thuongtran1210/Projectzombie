using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.Shared.Loadout
{
    /// <summary>
    /// Service hợp đồng điều phối toàn bộ nghiệp vụ Đội hình / Loadout.
    /// Giúp tầng Presentation (UI) và các hệ thống khác không phụ thuộc trực tiếp vào tầng thực thi Defense3D.
    /// Tuân thủ Clean Architecture & Dependency Inversion Principle (Rule 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public interface ILoadoutService
    {
        /// <summary>
        /// Nạp trạng thái đội hình hiện tại từ lưu trữ.
        /// </summary>
        LoadoutSelection LoadCurrentLoadout();

        /// <summary>
        /// Lưu trạng thái đội hình mới.
        /// </summary>
        void SaveLoadout(LoadoutSelection selection);

        /// <summary>
        /// Kiểm tra xem người chơi đã mở khóa thực thể theo Id hay chưa.
        /// </summary>
        bool IsOwned(string id);

        /// <summary>
        /// Tra cứu thông tin mục Catalog theo Id.
        /// </summary>
        bool TryGetCatalogEntry(string id, out LoadoutCatalogEntry entry);

        /// <summary>
        /// Lấy danh sách tất cả mục trong Catalog theo loại.
        /// </summary>
        IReadOnlyList<LoadoutCatalogEntry> GetCatalogEntriesByType(LoadoutItemType type);

        /// <summary>
        /// Bộ quy tắc ràng buộc số lượng đơn vị trong đội hình.
        /// </summary>
        LoadoutRules Rules { get; }

        /// <summary>
        /// Chuyển đổi ID từ legacy hoặc tên nhân vật/pháp bảo sang Catalog ID chuẩn.
        /// </summary>
        string ResolveSupportedId(string id, bool isHero);

        /// <summary>
        /// Tra cứu icon đại diện của thực thể.
        /// </summary>
        Sprite ResolveItemIcon(string id);

        /// <summary>
        /// Lấy tên hiển thị của thực thể theo Id.
        /// </summary>
        string GetDisplayName(string id);

        /// <summary>
        /// Tính toán phần trăm chỉ số thưởng cấp sao cho đơn vị Lính làng.
        /// </summary>
        void EvaluateMinionStarBonuses(int star, out float hpBonus, out float spdBonus);

        /// <summary>
        /// Tính toán phần trăm chỉ số thưởng cấp sao cho Trụ phòng thủ.
        /// </summary>
        void EvaluateTowerStarBonuses(int star, out float dmgBonus, out float rangeBonus);

        /// <summary>
        /// Tính toán phần trăm chỉ số thưởng cấp sao cho Pháp bảo.
        /// </summary>
        void EvaluateRelicStarBonuses(int star, RelicStarProgressionSO config, out float dmgBonus, out float cdReduction, out float areaBonus);
    }
}

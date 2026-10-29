using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Kết quả phân giải hoàn chỉnh từ DefenseLoadoutSnapshot sang thực thể runtime và danh mục catalog.
    /// Cung cấp dữ liệu đã được xác thực an toàn để áp dụng vào trận chiến 3D.
    /// </summary>
    public sealed class DefenseResolvedLoadout
    {
        public DefenseLoadoutSnapshot Snapshot { get; }
        public LoadoutCatalogEntry HeroEntry { get; }
        public LoadoutCatalogEntry RelicEntry { get; }
        public IReadOnlyList<LoadoutCatalogEntry> MinionEntries { get; }
        public IReadOnlyList<int> AllowedMinionRoles { get; }
        public IReadOnlyList<LoadoutCatalogEntry> TowerEntries { get; }

        public string HeroId => Snapshot.HeroId;
        public string RelicId => Snapshot.RelicId;
        public IReadOnlyList<string> MinionIds => Snapshot.MinionIds;
        public IReadOnlyList<string> TowerIds => Snapshot.TowerIds;

        public DefenseResolvedLoadout(
            DefenseLoadoutSnapshot snapshot,
            LoadoutCatalogEntry heroEntry,
            LoadoutCatalogEntry relicEntry,
            IReadOnlyList<LoadoutCatalogEntry> minionEntries,
            IReadOnlyList<int> allowedMinionRoles,
            IReadOnlyList<LoadoutCatalogEntry> towerEntries)
        {
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            HeroEntry = heroEntry;
            RelicEntry = relicEntry;
            MinionEntries = minionEntries ?? Array.Empty<LoadoutCatalogEntry>();
            AllowedMinionRoles = allowedMinionRoles ?? Array.Empty<int>();
            TowerEntries = towerEntries ?? Array.Empty<LoadoutCatalogEntry>();
        }

        public bool IsMinionRoleAllowed(int role)
        {
            if (AllowedMinionRoles == null || AllowedMinionRoles.Count == 0) return true;
            for (int i = 0; i < AllowedMinionRoles.Count; i++)
            {
                if (AllowedMinionRoles[i] == role) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Bộ phân giải Loadout thuần C# cho chế độ Defense 3D.
    /// Chịu trách nhiệm:
    /// 1. Kiểm tra tính hợp lệ của Snapshot, tự động fallback an toàn về cấu hình mặc định nếu dữ liệu rỗng/hỏng.
    /// 2. Ánh xạ các định danh chuỗi (ID) sang dữ liệu catalog và vai trò lính (0: Militia, 1: Gatherer, 2: Healer).
    /// </summary>
    public static class DefenseLoadoutResolver
    {
        // Vai trò lính runtime chuẩn tương ứng enum DefenseMinionRole (0: Militia, 1: Gatherer, 2: Healer)
        public const int ROLE_MILITIA = 0;
        public const int ROLE_GATHERER = 1;
        public const int ROLE_HEALER = 2;

        private static readonly Dictionary<string, int> MinionIdToRoleMap =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { Defense3DCatalog.MINION_DAN_BINH, ROLE_MILITIA },
                { Defense3DCatalog.MINION_PHU_TAP_DICH, ROLE_GATHERER },
                { Defense3DCatalog.MINION_THAY_LANG, ROLE_HEALER }
            };

        /// <summary>
        /// Chuyển đổi định danh lính trong catalog sang chỉ số vai trò lính runtime (0: Militia, 1: Gatherer, 2: Healer).
        /// </summary>
        public static bool TryGetMinionRole(string minionId, out int role)
        {
            if (!string.IsNullOrEmpty(minionId) && MinionIdToRoleMap.TryGetValue(minionId, out role))
            {
                return true;
            }
            role = -1;
            return false;
        }

        /// <summary>
        /// Phân giải một Snapshot thành DefenseResolvedLoadout.
        /// Nếu snapshot null hoặc vi phạm quy tắc ràng buộc, tự động fallback về cấu hình mặc định (CreateDefault).
        /// </summary>
        public static DefenseResolvedLoadout Resolve(
            DefenseLoadoutSnapshot snapshot,
            ILoadoutCatalog catalog = null,
            ILoadoutOwnership ownership = null)
        {
            catalog = catalog ?? Defense3DCatalog.CreateDefault();

            // Nếu snapshot không được cung cấp, sử dụng snapshot mặc định
            DefenseLoadoutSnapshot targetSnapshot = snapshot ?? DefenseLoadoutSnapshot.CreateDefault();

            // Xác thực snapshot
            var validation = LoadoutValidator.Validate(targetSnapshot, catalog, ownership);
            if (!validation.IsValid)
            {
                targetSnapshot = DefenseLoadoutSnapshot.CreateDefault();
            }

            // Lấy thông tin Tướng
            catalog.TryGetEntry(targetSnapshot.HeroId, out var heroEntry);

            // Lấy thông tin Pháp Bảo
            catalog.TryGetEntry(targetSnapshot.RelicId, out var relicEntry);

            // Lấy thông tin Lính & vai trò tương ứng
            var minionEntries = new List<LoadoutCatalogEntry>();
            var allowedRoles = new List<int>();
            foreach (var minionId in targetSnapshot.MinionIds)
            {
                if (catalog.TryGetEntry(minionId, out var entry))
                {
                    minionEntries.Add(entry);
                }
                if (TryGetMinionRole(minionId, out int role))
                {
                    if (!allowedRoles.Contains(role))
                    {
                        allowedRoles.Add(role);
                    }
                }
            }

            // Lấy thông tin Trụ
            var towerEntries = new List<LoadoutCatalogEntry>();
            foreach (var towerId in targetSnapshot.TowerIds)
            {
                if (catalog.TryGetEntry(towerId, out var entry))
                {
                    towerEntries.Add(entry);
                }
            }

            return new DefenseResolvedLoadout(
                targetSnapshot,
                heroEntry,
                relicEntry,
                minionEntries,
                allowedRoles,
                towerEntries);
        }
    }
}

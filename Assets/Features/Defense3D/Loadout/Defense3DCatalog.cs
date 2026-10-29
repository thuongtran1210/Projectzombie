using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Catalog dữ liệu chuẩn của Defense 3D hiện hành.
    /// Quản lý danh mục các thực thể hợp lệ (Tướng, Pháp Bảo, Lính làng, Trụ).
    /// </summary>
    public sealed class Defense3DCatalog : ILoadoutCatalog
    {
        // Định danh chuỗi hằng số chuẩn cho toàn dự án
        public const string HERO_THU_SINH = "hero_thusinh";
        public const string HERO_DAO_SI = "hero_daosi";
        public const string HERO_THANH_DONG = "hero_thanhdong";
        public const string HERO_AN_SI = "hero_ansi";
        public const string RELIC_DEP_TO_ONG = "relic_deptoong";
        public const string MINION_DAN_BINH = "minion_danbinh";
        public const string MINION_PHU_TAP_DICH = "minion_phutapdich";
        public const string MINION_THAY_LANG = "minion_thaylang";
        public const string TOWER_CUNG_TIEN = "tower_cungtien";

        private readonly Dictionary<string, LoadoutCatalogEntry> _entries =
            new Dictionary<string, LoadoutCatalogEntry>(StringComparer.OrdinalIgnoreCase);

        public Defense3DCatalog() { }

        public Defense3DCatalog(IEnumerable<LoadoutCatalogEntry> entries)
        {
            if (entries == null) return;
            foreach (var entry in entries)
            {
                Register(entry);
            }
        }

        public void Register(LoadoutCatalogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (_entries.ContainsKey(entry.Id))
                throw new InvalidOperationException($"Catalog đã chứa thực thể có ID '{entry.Id}'.");

            _entries[entry.Id] = entry;
        }

        public bool Contains(string id)
        {
            return !string.IsNullOrEmpty(id) && _entries.ContainsKey(id);
        }

        public bool TryGetEntry(string id, out LoadoutCatalogEntry entry)
        {
            if (string.IsNullOrEmpty(id))
            {
                entry = null;
                return false;
            }
            return _entries.TryGetValue(id, out entry);
        }

        public IReadOnlyList<LoadoutCatalogEntry> GetEntriesByType(LoadoutItemType type)
        {
            var list = new List<LoadoutCatalogEntry>();
            foreach (var entry in _entries.Values)
            {
                if (entry.Type == type) list.Add(entry);
            }
            return list;
        }

        /// <summary>
        /// Tạo Catalog mặc định chứa toàn bộ các thực thể nền tảng của Defense 3D (4 Anh Hùng, 1 Pháp Bảo, 3 Lính Làng, 1 Trụ).
        /// </summary>
        public static Defense3DCatalog CreateDefault()
        {
            var catalog = new Defense3DCatalog();
            catalog.Register(new LoadoutCatalogEntry(HERO_THU_SINH, LoadoutItemType.Hero, "Thư Sinh"));
            catalog.Register(new LoadoutCatalogEntry(HERO_DAO_SI, LoadoutItemType.Hero, "Đạo Sĩ"));
            catalog.Register(new LoadoutCatalogEntry(HERO_THANH_DONG, LoadoutItemType.Hero, "Thanh Đồng"));
            catalog.Register(new LoadoutCatalogEntry(HERO_AN_SI, LoadoutItemType.Hero, "Ẩn Sĩ"));
            catalog.Register(new LoadoutCatalogEntry(RELIC_DEP_TO_ONG, LoadoutItemType.Relic, "Dép Tổ Ong"));
            catalog.Register(new LoadoutCatalogEntry(MINION_DAN_BINH, LoadoutItemType.Minion, "Dân Binh"));
            catalog.Register(new LoadoutCatalogEntry(MINION_PHU_TAP_DICH, LoadoutItemType.Minion, "Phu Tạp Dịch"));
            catalog.Register(new LoadoutCatalogEntry(MINION_THAY_LANG, LoadoutItemType.Minion, "Thầy Lang"));
            catalog.Register(new LoadoutCatalogEntry(TOWER_CUNG_TIEN, LoadoutItemType.Tower, "Tháp Cung Tiễn"));
            return catalog;
        }
    }
}

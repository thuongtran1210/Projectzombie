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
        public const string TOWER_BANG_HA = "tower_bangha";

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
            catalog.Register(new LoadoutCatalogEntry(RELIC_DEP_TO_ONG, LoadoutItemType.Relic, "Dép Tổ Ong", true, "<b>Tuyệt Kỹ Pháp Bảo:</b> Phóng Dép Tổ Ong thần thông càn quét yêu ma trên đường bay, gây sát thương diện rộng và đẩy lùi kẻ địch."));
            catalog.Register(new LoadoutCatalogEntry(MINION_DAN_BINH, LoadoutItemType.Minion, "Dân Binh", true, "<b>Dân Binh Giáp Tre:</b> Quân lính cận chiến dũng cảm giữ cửa làng, có khả năng làm chậm và tung đòn trọng kích làm choáng yêu ma."));
            catalog.Register(new LoadoutCatalogEntry(MINION_PHU_TAP_DICH, LoadoutItemType.Minion, "Phu Tạp Dịch", true, "<b>Phu Tạp Dịch:</b> Hậu cần chăm chỉ thu gom Cổ Mộc rơi trên chiến trường về kho và tự động cầm búa tu bổ Căn Cứ / Tháp Canh bị hư hại."));
            catalog.Register(new LoadoutCatalogEntry(MINION_THAY_LANG, LoadoutItemType.Minion, "Thầy Lang", true, "<b>Thầy Lang Cứu Thương:</b> Y sĩ thảo dược đi theo hỗ trợ, liên tục niệm chú hồi phục sinh lực cho Chủ Tướng và đồng đội."));
            var towers = ProjectZombie.Core.Architecture.ServiceContext.Get<DefenseTowerLoadoutCatalogSO>();
            if (towers != null)
            {
                for (int i = 0; i < towers.Count; i++)
                {
                    var entry = towers.Get(i);
                    catalog.Register(new LoadoutCatalogEntry(entry.Id, LoadoutItemType.Tower, entry.Name));
                }
            }
            else
            {
                catalog.Register(new LoadoutCatalogEntry(TOWER_CUNG_TIEN, LoadoutItemType.Tower, "Nỏ Thần", true, "<b>Tháp Nỏ Thần Liên Châu:</b> Chòi cung tiễn tầm xa bắn tên tốc độ cao, chuyên trị các đợt yêu ma tiến công đơn lẻ và mục tiêu trên không."));
                catalog.Register(new LoadoutCatalogEntry(TOWER_BANG_HA, LoadoutItemType.Tower, "Băng Hà", true, "<b>Tháp Băng Hà Hàn Khí:</b> Chòi pháp thuật phóng đạn băng giá gây sát thương kèm hiệu ứng làm chậm tốc độ di chuyển của đàn quái."));
            }
            return catalog;
        }
    }
}

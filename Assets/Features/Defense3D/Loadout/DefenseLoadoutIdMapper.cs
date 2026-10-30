using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Bộ chuyển đổi định danh (ID Mapper) giữa hệ thống cũ (2D Roguelite / PlayerPrefs / MetaProgression)
    /// và hệ thống Defense 3D hiện hành.
    /// Đảm bảo tính tương thích ngược và fallback an toàn khi gặp ID cũ hoặc không xác định.
    /// </summary>
    public static class DefenseLoadoutIdMapper
    {
        // Danh sách bí danh ID cũ của Thư Sinh
        private static readonly HashSet<string> ThuSinhAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C001_ThuSinh",
            "C001",
            "thusinh",
            "ThuSinh",
            "Hero_ThuSinh",
            "HERO_THUSHINH",
            Defense3DCatalog.HERO_THU_SINH
        };

        // Danh sách bí danh ID cũ của Đạo Sĩ
        private static readonly HashSet<string> DaoSiAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C002_DaoSi",
            "C002",
            "daosi",
            "DaoSi",
            "Hero_DaoSi",
            "HERO_DAOSI",
            Defense3DCatalog.HERO_DAO_SI
        };

        // Danh sách bí danh ID cũ của Thanh Đồng
        private static readonly HashSet<string> ThanhDongAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C003_ThanhDong",
            "C003",
            "thanhdong",
            "ThanhDong",
            "Hero_ThanhDong",
            "HERO_THANHDONG",
            Defense3DCatalog.HERO_THANH_DONG
        };

        // Danh sách bí danh ID cũ của Ẩn Sĩ
        private static readonly HashSet<string> AnSiAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C004_AnSi",
            "C004",
            "ansi",
            "AnSi",
            "Hero_AnSi",
            "HERO_ANSI",
            Defense3DCatalog.HERO_AN_SI
        };

        // Danh sách bí danh ID cũ của Dép Tổ Ong
        private static readonly HashSet<string> DepToOngAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "R001_Deptoong",
            "R001",
            "deptoong",
            "DepToOng",
            "Skill_DepToOng",
            Defense3DCatalog.RELIC_DEP_TO_ONG
        };

        // Danh sách bí danh ID của Lính Làng
        private static readonly Dictionary<string, string> MinionAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "danbinh", Defense3DCatalog.MINION_DAN_BINH },
            { "Militia", Defense3DCatalog.MINION_DAN_BINH },
            { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_DAN_BINH },
            { "phutapdich", Defense3DCatalog.MINION_PHU_TAP_DICH },
            { "Gatherer", Defense3DCatalog.MINION_PHU_TAP_DICH },
            { Defense3DCatalog.MINION_PHU_TAP_DICH, Defense3DCatalog.MINION_PHU_TAP_DICH },
            { "thaylang", Defense3DCatalog.MINION_THAY_LANG },
            { "Healer", Defense3DCatalog.MINION_THAY_LANG },
            { Defense3DCatalog.MINION_THAY_LANG, Defense3DCatalog.MINION_THAY_LANG }
        };

        // Danh sách bí danh ID của Trụ
        private static readonly Dictionary<string, string> TowerAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "cungtien", Defense3DCatalog.TOWER_CUNG_TIEN },
            { "Archer", Defense3DCatalog.TOWER_CUNG_TIEN },
            { "tower_nothan", Defense3DCatalog.TOWER_CUNG_TIEN },
            { "tower_bangha", Defense3DCatalog.TOWER_BANG_HA },
            { Defense3DCatalog.TOWER_CUNG_TIEN, Defense3DCatalog.TOWER_CUNG_TIEN }
        };

        /// <summary>
        /// Ánh xạ ID tướng cũ sang ID chuẩn của Defense 3D.
        /// Hỗ trợ cả 4 Anh Hùng: Thư Sinh, Đạo Sĩ, Thanh Đồng, Ẩn Sĩ.
        /// Nếu ID rỗng hoặc chưa được hỗ trợ trong Defense 3D, tự động fallback về tướng mặc định (Thư Sinh).
        /// </summary>
        public static string MapHeroId(string legacyHeroId)
        {
            if (!string.IsNullOrWhiteSpace(legacyHeroId))
            {
                string clean = legacyHeroId.Trim();
                if (ThuSinhAliases.Contains(clean)) return Defense3DCatalog.HERO_THU_SINH;
                if (DaoSiAliases.Contains(clean)) return Defense3DCatalog.HERO_DAO_SI;
                if (ThanhDongAliases.Contains(clean)) return Defense3DCatalog.HERO_THANH_DONG;
                if (AnSiAliases.Contains(clean)) return Defense3DCatalog.HERO_AN_SI;
            }

            // Fallback an toàn về Tướng mặc định
            return Defense3DCatalog.HERO_THU_SINH;
        }

        /// <summary>
        /// Ánh xạ ID pháp bảo cũ sang ID chuẩn của Defense 3D.
        /// Nếu ID rỗng hoặc chưa được hỗ trợ, tự động fallback về pháp bảo mặc định (Dép Tổ Ong).
        /// </summary>
        public static string MapRelicId(string legacyRelicId)
        {
            if (!string.IsNullOrWhiteSpace(legacyRelicId) && DepToOngAliases.Contains(legacyRelicId.Trim()))
            {
                return Defense3DCatalog.RELIC_DEP_TO_ONG;
            }

            // Fallback an toàn về Pháp Bảo mặc định
            return Defense3DCatalog.RELIC_DEP_TO_ONG;
        }

        /// <summary>
        /// Chuẩn hóa danh sách ID lính. Nếu danh sách rỗng, điền 3 loại lính mặc định.
        /// </summary>
        public static List<string> MapMinionIds(IEnumerable<string> legacyMinionIds)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (legacyMinionIds != null)
            {
                foreach (var id in legacyMinionIds)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (MinionAliases.TryGetValue(id.Trim(), out var mappedId))
                    {
                        if (seen.Add(mappedId))
                        {
                            result.Add(mappedId);
                        }
                    }
                }
            }

            // Nếu không có lính nào hợp lệ, nạp mặc định cả 3 lính
            if (result.Count == 0)
            {
                result.Add(Defense3DCatalog.MINION_DAN_BINH);
                result.Add(Defense3DCatalog.MINION_PHU_TAP_DICH);
                result.Add(Defense3DCatalog.MINION_THAY_LANG);
            }

            return result;
        }

        /// <summary>
        /// Chuẩn hóa danh sách ID trụ. Nếu danh sách rỗng, điền trụ mặc định (Tháp Cung Tiễn).
        /// </summary>
        public static List<string> MapTowerIds(IEnumerable<string> legacyTowerIds)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (legacyTowerIds != null)
            {
                foreach (var id in legacyTowerIds)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (TowerAliases.TryGetValue(id.Trim(), out var mappedId))
                    {
                        if (seen.Add(mappedId))
                        {
                            result.Add(mappedId);
                        }
                    }
                    else if (Defense3DCatalog.CreateDefault().TryGetEntry(id.Trim(), out var entry) && entry.Type == LoadoutItemType.Tower && seen.Add(entry.Id))
                    {
                        result.Add(entry.Id);
                    }
                }
            }

            // Nếu không có trụ nào hợp lệ, nạp mặc định Tháp Cung Tiễn
            if (result.Count == 0)
            {
                result.Add(Defense3DCatalog.TOWER_CUNG_TIEN);
            }

            return result;
        }
    }
}

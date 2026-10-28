using System;
using System.Collections.Generic;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Adapter chuyển đổi quyền sở hữu từ MetaProgressionSaveData sang ILoadoutOwnership của Defense 3D.
    /// Giữ nguyên tiến trình sở hữu nhân vật, thẻ mảnh và mở khóa từ hệ thống meta hiện hữu.
    /// </summary>
    public sealed class DefenseMetaOwnershipAdapter : ILoadoutOwnership
    {
        private readonly MetaProgressionSaveData _saveData;
        private readonly HashSet<string> _explicitOwnedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public DefenseMetaOwnershipAdapter(MetaProgressionSaveData saveData)
        {
            _saveData = saveData;
        }

        /// <summary>
        /// Tạo Ownership Adapter mặc định sở hữu toàn bộ 6 thực thể nền tảng của Defense 3D.
        /// </summary>
        public static DefenseMetaOwnershipAdapter CreateDefault()
        {
            var adapter = new DefenseMetaOwnershipAdapter(null);
            adapter.GrantBaseline();
            return adapter;
        }

        /// <summary>
        /// Cấp quyền sở hữu thủ công cho một ID (dùng cho debug, test hoặc phần thưởng tức thì).
        /// </summary>
        public void Grant(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                _explicitOwnedIds.Add(id.Trim());
            }
        }

        private void GrantBaseline()
        {
            _explicitOwnedIds.Add(Defense3DCatalog.HERO_THU_SINH);
            _explicitOwnedIds.Add(Defense3DCatalog.RELIC_DEP_TO_ONG);
            _explicitOwnedIds.Add(Defense3DCatalog.MINION_DAN_BINH);
            _explicitOwnedIds.Add(Defense3DCatalog.MINION_PHU_TAP_DICH);
            _explicitOwnedIds.Add(Defense3DCatalog.MINION_THAY_LANG);
            _explicitOwnedIds.Add(Defense3DCatalog.TOWER_CUNG_TIEN);
        }

        public bool IsOwned(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            id = id.Trim();

            // 1. Kiểm tra danh sách được cấp quyền rõ ràng
            if (_explicitOwnedIds.Contains(id)) return true;

            // 2. Các thực thể khởi đầu miễn phí của Defense 3D luôn được sở hữu
            if (string.Equals(id, Defense3DCatalog.HERO_THU_SINH, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, Defense3DCatalog.RELIC_DEP_TO_ONG, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, Defense3DCatalog.MINION_DAN_BINH, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, Defense3DCatalog.MINION_PHU_TAP_DICH, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, Defense3DCatalog.MINION_THAY_LANG, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, Defense3DCatalog.TOWER_CUNG_TIEN, StringComparison.OrdinalIgnoreCase)) return true;

            // 3. Nếu có MetaProgressionSaveData, kiểm tra danh sách unlock cũ
            if (_saveData != null)
            {
                // Kiểm tra Tướng
                if (_saveData.unlockedCharacters != null)
                {
                    foreach (var charId in _saveData.unlockedCharacters)
                    {
                        if (string.IsNullOrWhiteSpace(charId)) continue;
                        if (string.Equals(DefenseLoadoutIdMapper.MapHeroId(charId), id, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                // Kiểm tra Pháp Bảo
                if (_saveData.relicProgressList != null)
                {
                    foreach (var relicEntry in _saveData.relicProgressList)
                    {
                        if (string.IsNullOrWhiteSpace(relicEntry.relicId)) continue;
                        if (string.Equals(DefenseLoadoutIdMapper.MapRelicId(relicEntry.relicId), id, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}

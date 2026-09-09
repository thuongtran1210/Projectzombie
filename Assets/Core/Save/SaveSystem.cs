using System;
using System.IO;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// Hệ thống Quản Lý Dữ Liệu Lưu / Nạp (Save Service Facade).
    /// Hỗ trợ cắm rút linh hoạt các Provider khác nhau (LocalJsonSaveProvider, FirebaseSaveProvider, v.v.).
    /// </summary>
    public static class SaveSystem
    {
        private static ISaveDataProvider _currentProvider;

        /// <summary>
        /// Provider hiện tại đang được sử dụng để lưu/nạp (mặc định là Local JSON).
        /// </summary>
        public static ISaveDataProvider CurrentProvider
        {
            get
            {
                if (_currentProvider == null)
                {
                    _currentProvider = new LocalJsonSaveProvider();
                }
                return _currentProvider;
            }
            set
            {
                _currentProvider = value;
                Debug.Log($"[SaveSystem] Đã chuyển đổi Save Provider sang: {_currentProvider?.ProviderName}");
            }
        }

        /// <summary>
        /// Lưu dữ liệu MetaProgressionSaveData thông qua Provider hiện tại.
        /// </summary>
        public static bool Save(MetaProgressionSaveData saveData)
        {
            return CurrentProvider.Save(saveData);
        }

        /// <summary>
        /// Nạp dữ liệu MetaProgressionSaveData thông qua Provider hiện tại.
        /// </summary>
        public static MetaProgressionSaveData Load()
        {
            return CurrentProvider.Load();
        }

        /// <summary>
        /// Xóa dữ liệu save (dùng cho Reset / Debug).
        /// </summary>
        public static void DeleteSave()
        {
            CurrentProvider.DeleteSave();
        }

        /// <summary>
        /// Kiểm tra xem có file/dữ liệu lưu hay không.
        /// </summary>
        public static bool SaveExists()
        {
            return CurrentProvider.SaveExists();
        }
    }
}

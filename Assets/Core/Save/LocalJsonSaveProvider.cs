using System;
using System.IO;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// Provider lưu trữ cục bộ dạng JSON trên thiết bị di động (Android / iOS / PC).
    /// Tự động gọi SanitizeLegacyData() khi nạp để dọn dẹp dữ liệu save 2D cũ mà không gây lỗi runtime.
    /// </summary>
    public class LocalJsonSaveProvider : ISaveDataProvider
    {
        private static readonly string DefaultSaveFileName = "player_save.json";
        private readonly string _saveFilePath;

        public string ProviderName => "Local JSON Storage";

        public LocalJsonSaveProvider(string customFileName = null)
        {
            string fileName = string.IsNullOrEmpty(customFileName) ? DefaultSaveFileName : customFileName;
            _saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public string SaveFilePath => _saveFilePath;

        public bool Save(MetaProgressionSaveData saveData)
        {
            try
            {
                if (saveData == null) saveData = new MetaProgressionSaveData();
                saveData.SanitizeLegacyData();

                saveData.lastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (string.IsNullOrEmpty(saveData.deviceId))
                {
                    saveData.deviceId = SystemInfo.deviceUniqueIdentifier;
                }

                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(_saveFilePath, json);
                Debug.Log($"[{ProviderName}] Đã lưu dữ liệu thành công tại: {_saveFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{ProviderName}] Lỗi khi lưu dữ liệu: {ex.Message}");
                return false;
            }
        }

        public MetaProgressionSaveData Load()
        {
            try
            {
                if (!File.Exists(_saveFilePath))
                {
                    Debug.Log($"[{ProviderName}] Chưa có file save cũ. Khởi tạo dữ liệu mới.");
                    var newData = new MetaProgressionSaveData();
                    Save(newData);
                    return newData;
                }

                string json = File.ReadAllText(_saveFilePath);
                var loadedData = string.IsNullOrWhiteSpace(json)
                    ? new MetaProgressionSaveData()
                    : JsonUtility.FromJson<MetaProgressionSaveData>(json);
                if (loadedData == null)
                {
                    loadedData = new MetaProgressionSaveData();
                }

                if (loadedData.SanitizeLegacyData())
                {
                    Save(loadedData);
                }

                Debug.Log($"[{ProviderName}] Nạp dữ liệu thành công từ: {_saveFilePath}");
                return loadedData;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{ProviderName}] Phát hiện dữ liệu save cũ không tương thích ({ex.Message}). Đang tự động phục hồi về mặc định an toàn.");
                var fallback = new MetaProgressionSaveData();
                Save(fallback);
                return fallback;
            }
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    File.Delete(_saveFilePath);
                    Debug.Log($"[{ProviderName}] Đã xóa file save thành công.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{ProviderName}] Lỗi khi xóa file save: {ex.Message}");
            }
        }

        public bool SaveExists()
        {
            return File.Exists(_saveFilePath);
        }
    }
}

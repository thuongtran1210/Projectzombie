using System;
using System.IO;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// Provider lưu trữ cục bộ dạng JSON trên thiết bị di động (Android / iOS / PC).
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

                // Cập nhật timestamp & metadata
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
                var loadedData = JsonUtility.FromJson<MetaProgressionSaveData>(json);
                if (loadedData == null)
                {
                    loadedData = new MetaProgressionSaveData();
                }
                Debug.Log($"[{ProviderName}] Nạp dữ liệu thành công từ: {_saveFilePath}");
                return loadedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{ProviderName}] Lỗi khi nạp dữ liệu: {ex.Message}. Khởi tạo mặc định.");
                return new MetaProgressionSaveData();
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

using System;
using System.IO;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// Hệ thống Lưu / Nạp dữ liệu cục bộ chuẩn cho Android (sử dụng JSON tại Application.persistentDataPath).
    /// </summary>
    public static class SaveSystem
    {
        private static readonly string SaveFileName = "player_save.json";

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// Lưu dữ liệu MetaProgressionSaveData xuống bộ nhớ thiết bị Android.
        /// </summary>
        public static bool Save(MetaProgressionSaveData saveData)
        {
            try
            {
                if (saveData == null) saveData = new MetaProgressionSaveData();

                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveSystem] Đã lưu dữ liệu thành công tại: {SaveFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi lưu dữ liệu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Nạp dữ liệu MetaProgressionSaveData từ bộ nhớ thiết bị Android.
        /// Trả về object mới nếu file chưa tồn tại.
        /// </summary>
        public static MetaProgressionSaveData Load()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    Debug.Log("[SaveSystem] Chưa có file save cũ. Khởi tạo dữ liệu mới.");
                    var newData = new MetaProgressionSaveData();
                    Save(newData);
                    return newData;
                }

                string json = File.ReadAllText(SaveFilePath);
                var loadedData = JsonUtility.FromJson<MetaProgressionSaveData>(json);
                if (loadedData == null)
                {
                    loadedData = new MetaProgressionSaveData();
                }
                Debug.Log($"[SaveSystem] Nạp dữ liệu thành công từ: {SaveFilePath}");
                return loadedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi nạp dữ liệu: {ex.Message}. Khởi tạo mặc định.");
                return new MetaProgressionSaveData();
            }
        }

        /// <summary>
        /// Xóa dữ liệu save (dùng cho Reset / Debug).
        /// </summary>
        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("[SaveSystem] Đã xóa file save thành công.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi xóa file save: {ex.Message}");
            }
        }
    }
}

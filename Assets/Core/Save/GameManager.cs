using System;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// GameManager quản lý vòng đời lưu / nạp tiến trình chơi (Save/Load) cho Android.
    /// Tự động nạp dữ liệu khi Start và lưu dữ liệu khi Paused, Quit hoặc kết thúc trận đấu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public MetaProgressionSaveData SaveData { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Nạp dữ liệu Save khi game khởi động
            LoadGame();
        }

        private void Start()
        {
            // Khởi tạo MetaCurrencyManager nếu tồn tại
            if (MetaCurrencyManager.Instance != null && SaveData != null)
            {
                MetaCurrencyManager.Instance.Initialize(SaveData);
            }
        }

        /// <summary>
        /// Nạp dữ liệu từ bộ nhớ thiết bị.
        /// </summary>
        public void LoadGame()
        {
            SaveData = SaveSystem.Load();
            if (MetaCurrencyManager.Instance != null && SaveData != null)
            {
                MetaCurrencyManager.Instance.Initialize(SaveData);
            }
        }

        /// <summary>
        /// Lưu tiến trình hiện tại xuống đĩa.
        /// </summary>
        public void SaveGame()
        {
            if (MetaCurrencyManager.Instance != null)
            {
                SaveData = MetaCurrencyManager.Instance.GetSaveData();
            }

            SaveSystem.Save(SaveData);
        }

        /// <summary>
        /// Cập nhật kết quả sau một lượt chơi (Run) và tự động lưu.
        /// </summary>
        public void OnRunCompleted(float runTime, int killCount, int currencyEarned)
        {
            if (SaveData == null) SaveData = new MetaProgressionSaveData();

            SaveData.UpdateBestStats(runTime, killCount);

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.AddCurrency(currencyEarned);
            }
            else
            {
                SaveData.totalCurrency += currencyEarned;
            }

            SaveGame();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}

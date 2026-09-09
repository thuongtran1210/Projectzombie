using System;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.MetaProgression.Gacha;

using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// GameManager quản lý vòng đời lưu / nạp tiến trình chơi (Save/Load) cho Android.
    /// Tự động nạp dữ liệu khi Start và lưu dữ liệu khi Paused, Quit hoặc kết thúc trận đấu.
    /// Kế thừa PersistentSingleton<GameManager> chuẩn kiến trúc.
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        public MetaProgressionSaveData SaveData { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            // Nạp dữ liệu Save khi game khởi động
            if (SaveData == null)
            {
                LoadGame();
            }
        }

        private void Start()
        {
            InitializeAllManagers();
        }

        private void InitializeAllManagers()
        {
            if (SaveData == null) return;

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.Initialize(SaveData);
            }
            if (RelicInventoryManager.Instance != null)
            {
                RelicInventoryManager.Instance.Initialize(SaveData);
            }
            if (RelicGachaManager.Instance != null)
            {
                RelicGachaManager.Instance.Initialize(SaveData);
            }
        }

        /// <summary>
        /// Nạp dữ liệu từ bộ nhớ thiết bị.
        /// </summary>
        public void LoadGame()
        {
            SaveData = SaveSystem.Load();
            InitializeAllManagers();
        }

        /// <summary>
        /// Lưu tiến trình hiện tại xuống đĩa.
        /// </summary>
        public void SaveGame()
        {
            if (SaveData == null)
            {
                SaveData = new MetaProgressionSaveData();
            }

            if (MetaCurrencyManager.Instance != null)
            {
                SaveData.totalCurrency = MetaCurrencyManager.Instance.TotalCurrency;
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

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            SaveGame();
        }
    }
}

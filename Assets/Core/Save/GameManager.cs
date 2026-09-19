using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// GameManager quản lý vòng đời lưu / nạp tiến trình chơi (Save/Load) cho Android.
    /// Tự động nạp dữ liệu khi Start và lưu dữ liệu khi Paused, Quit hoặc kết thúc trận đấu.
    /// Kế thừa PersistentSingleton<GameManager> chuẩn kiến trúc.
    /// Tuân thủ Dependency Inversion Principle (DIP): Giao tiếp với các Domain Services thông qua ISaveableModule.
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        public MetaProgressionSaveData SaveData { get; private set; }

        private readonly List<ISaveableModule> _saveableModules = new List<ISaveableModule>();

        /// <summary>
        /// Đăng ký một module nghiệp vụ tham gia vào vòng đời Save/Load.
        /// </summary>
        public void RegisterModule(ISaveableModule module)
        {
            if (module != null && !_saveableModules.Contains(module))
            {
                _saveableModules.Add(module);
                if (SaveData != null)
                {
                    module.InitializeFromSave(SaveData);
                }
            }
        }

        /// <summary>
        /// Hủy đăng ký module khỏi vòng đời Save/Load.
        /// </summary>
        public void UnregisterModule(ISaveableModule module)
        {
            if (module != null)
            {
                _saveableModules.Remove(module);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // Nạp dữ liệu Save khi game khởi động (chỉ load raw data, không vội inject vào manager con lúc Awake)
            if (SaveData == null)
            {
                SaveData = SaveSystem.Load();
            }
        }

        private void Start()
        {
            InitializeAllManagers();
        }

        public void InitializeAllManagers()
        {
            if (SaveData == null) return;

            for (int i = 0; i < _saveableModules.Count; i++)
            {
                _saveableModules[i]?.InitializeFromSave(SaveData);
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

            // Thu thập dữ liệu từ các module đăng ký
            for (int i = 0; i < _saveableModules.Count; i++)
            {
                _saveableModules[i]?.PopulateSaveData(SaveData);
            }

            SaveSystem.Save(SaveData);
        }

        /// <summary>
        /// Sự kiện phát ra khi kết thúc Run kèm lượng Cổ Tiền nhận được (dành cho MetaCurrencyService lắng nghe).
        /// </summary>
        public static event Action<int> OnRunCurrencyEarned;

        /// <summary>
        /// Cập nhật kết quả sau một lượt chơi (Run) và tự động lưu.
        /// </summary>
        public void OnRunCompleted(float runTime, int killCount, int currencyEarned, string stageId = null, bool isVictory = false)
        {
            if (SaveData == null) SaveData = new MetaProgressionSaveData();

            SaveData.UpdateBestStats(runTime, killCount);

            if (isVictory && !string.IsNullOrEmpty(stageId))
            {
                SaveData.MarkStageCompleted(stageId, runTime, 3);
            }

            if (OnRunCurrencyEarned != null)
            {
                OnRunCurrencyEarned.Invoke(currencyEarned);
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

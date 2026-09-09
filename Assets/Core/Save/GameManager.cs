using System;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.MetaProgression.Gacha;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// GameManager quản lý vòng đời lưu / nạp tiến trình chơi (Save/Load) cho Android.
    /// Tự động nạp dữ liệu khi Start và lưu dữ liệu khi Paused, Quit hoặc kết thúc trận đấu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        private static bool _isApplicationQuitting = false;

        public static GameManager Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    return _instance;
                }

                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("[Auto] GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public MetaProgressionSaveData SaveData { get; private set; }

        private void Awake()
        {
            _isApplicationQuitting = false;
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Nạp dữ liệu Save khi game khởi động
            if (SaveData == null)
            {
                LoadGame();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
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

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
            SaveGame();
        }
    }
}

using System;
using UnityEngine;
using ProjectZombie.Core.Save;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Domain Service trung tâm quản lý logic Sở Hữu, Thu Thập Thẻ Mảnh và Gộp Thẻ Nâng Sao Vũ Khí / Pháp Bảo.
    /// Thiết kế chuẩn SOLID, Data-Driven, phát sinh sự kiện để UI Presenter cập nhật phản hồi.
    /// </summary>
    public class RelicInventoryManager : MonoBehaviour
    {
        public static RelicInventoryManager Instance { get; private set; }

        [Header("Cấu Hình Progression")]
        [SerializeField] private RelicStarProgressionSO _progressionConfig;

        public event Action<string, int> OnRelicStarUpgraded; // (relicId, newStar)
        public event Action<string, int> OnRelicShardsChanged;  // (relicId, newShardCount)
        public event Action<string> OnRelicUnlocked;           // (relicId)

        private MetaProgressionSaveData _saveData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (_saveData == null)
            {
                var gm = GameManager.Instance;
                if (gm != null && gm.SaveData != null)
                {
                    Initialize(gm.SaveData);
                }
                else
                {
                    Initialize(SaveSystem.Load());
                }
            }
        }

        public void Initialize(MetaProgressionSaveData saveData)
        {
            _saveData = saveData ?? new MetaProgressionSaveData();

            // Đảm bảo vũ khí khởi đầu luôn đạt tối thiểu 1 sao nếu chưa từng tạo save
            EnsureStarterRelicUnlocked("wp_kiem_truc");

            Debug.Log($"[RelicInventoryManager] Khởi tạo thành công với {_saveData.relicProgressList?.Count ?? 0} bản ghi pháp bảo.");
        }

        public void SetProgressionConfig(RelicStarProgressionSO config)
        {
            _progressionConfig = config;
        }

        public RelicStarProgressionSO ProgressionConfig => _progressionConfig;

        public int GetRelicStarLevel(string relicId)
        {
            if (string.IsNullOrEmpty(relicId)) return 0;
            if (_saveData == null) return relicId == "wp_kiem_truc" ? 1 : 0;
            return _saveData.GetRelicStarLevel(relicId);
        }

        public int GetRelicShardCount(string relicId)
        {
            if (string.IsNullOrEmpty(relicId) || _saveData == null) return 0;
            return _saveData.GetRelicShards(relicId);
        }

        public bool IsRelicUnlocked(string relicId)
        {
            return GetRelicStarLevel(relicId) >= 1;
        }

        public RelicStarStepConfig GetNextStepConfig(string relicId)
        {
            int currentStar = GetRelicStarLevel(relicId);
            if (currentStar >= 5) return null; // Đã đạt max

            int nextStar = currentStar + 1;
            if (_progressionConfig != null)
            {
                return _progressionConfig.GetStepConfig(nextStar);
            }

            // Fallback nếu chưa gán config SO
            int reqShards = nextStar == 1 ? 5 : (nextStar == 2 ? 10 : (nextStar == 3 ? 20 : (nextStar == 4 ? 40 : 80)));
            int cost = nextStar == 1 ? 0 : (nextStar == 2 ? 500 : (nextStar == 3 ? 1500 : (nextStar == 4 ? 3500 : 8000)));
            return new RelicStarStepConfig { targetStar = nextStar, requiredShards = reqShards, coTienCost = cost };
        }

        public bool CanFuse(string relicId, out string reason)
        {
            reason = "";
            if (string.IsNullOrEmpty(relicId))
            {
                reason = "Vũ khí không hợp lệ.";
                return false;
            }

            int currentStar = GetRelicStarLevel(relicId);
            if (currentStar >= 5)
            {
                reason = "Đã đạt cảnh giới tối đa (5★).";
                return false;
            }

            var nextConfig = GetNextStepConfig(relicId);
            if (nextConfig == null)
            {
                reason = "Không tìm thấy cấu hình tăng sao.";
                return false;
            }

            int currentShards = GetRelicShardCount(relicId);
            if (currentShards < nextConfig.requiredShards)
            {
                reason = $"Chưa đủ thẻ ({currentShards}/{nextConfig.requiredShards}).";
                return false;
            }

            int currentMoney = MetaCurrencyManager.Instance != null ? MetaCurrencyManager.Instance.TotalCurrency : 0;
            if (currentMoney < nextConfig.coTienCost)
            {
                reason = $"Không đủ Cổ Tiền ({currentMoney}/{nextConfig.coTienCost}).";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Thực hiện gộp thẻ mở khóa hoặc tăng sao vũ khí. Trừ thẻ, trừ tiền và lưu đĩa vĩnh viễn.
        /// </summary>
        public bool TryFuseRelic(string relicId)
        {
            if (!CanFuse(relicId, out string reason))
            {
                Debug.LogWarning($"[RelicInventoryManager] Không thể gộp thẻ cho '{relicId}': {reason}");
                return false;
            }

            var nextConfig = GetNextStepConfig(relicId);
            int currentStar = GetRelicStarLevel(relicId);
            int currentShards = GetRelicShardCount(relicId);

            // Trừ tiền nếu có chi phí
            if (nextConfig.coTienCost > 0)
            {
                if (MetaCurrencyManager.Instance != null)
                {
                    if (!MetaCurrencyManager.Instance.SpendCurrency(nextConfig.coTienCost))
                        return false;
                }
            }

            int newShards = currentShards - nextConfig.requiredShards;
            int newStar = currentStar + 1;

            if (_saveData != null)
            {
                _saveData.SetRelicProgress(relicId, newShards, newStar);
            }

            SaveDataToDisk();

            Debug.Log($"[RelicInventoryManager] Gộp thẻ thành công '{relicId}': Cấp sao mới = {newStar}★, Thẻ còn lại = {newShards}");

            OnRelicShardsChanged?.Invoke(relicId, newShards);
            OnRelicStarUpgraded?.Invoke(relicId, newStar);

            if (newStar == 1)
            {
                OnRelicUnlocked?.Invoke(relicId);
            }

            return true;
        }

        /// <summary>
        /// Đặt trực tiếp số thẻ mảnh của pháp bảo (dùng cho Debug / Test / Tool).
        /// </summary>
        public void SetRelicShards(string relicId, int count)
        {
            if (string.IsNullOrEmpty(relicId)) return;

            int currentStar = GetRelicStarLevel(relicId);
            int newShards = Mathf.Max(0, count);

            if (_saveData != null)
            {
                _saveData.SetRelicProgress(relicId, newShards, currentStar);
            }

            SaveDataToDisk();
            Debug.Log($"[RelicInventoryManager] Set thẻ '{relicId}': {newShards}");
            OnRelicShardsChanged?.Invoke(relicId, newShards);
        }

        /// <summary>
        /// Đặt trực tiếp cấp sao của pháp bảo từ 0★ đến 5★ (dùng cho Debug / Test / Tool).
        /// </summary>
        public void SetRelicStarLevel(string relicId, int starLevel)
        {
            if (string.IsNullOrEmpty(relicId)) return;

            int currentShards = GetRelicShardCount(relicId);
            int newStar = Mathf.Clamp(starLevel, 0, 5);

            if (_saveData != null)
            {
                _saveData.SetRelicProgress(relicId, currentShards, newStar);
            }

            SaveDataToDisk();
            Debug.Log($"[RelicInventoryManager] Set cấp sao '{relicId}': {newStar}★");
            OnRelicStarUpgraded?.Invoke(relicId, newStar);
            if (newStar >= 1) OnRelicUnlocked?.Invoke(relicId);
        }

        /// <summary>
        /// Nhận thêm hoặc bớt thẻ mảnh (hỗ trợ count âm/dương).
        /// </summary>
        public void AddRelicShards(string relicId, int count)
        {
            if (string.IsNullOrEmpty(relicId)) return;

            int currentShards = GetRelicShardCount(relicId);
            int currentStar = GetRelicStarLevel(relicId);
            int newShards = Mathf.Max(0, currentShards + count);

            if (_saveData != null)
            {
                _saveData.SetRelicProgress(relicId, newShards, currentStar);
            }

            SaveDataToDisk();

            Debug.Log($"[RelicInventoryManager] Thay đổi {(count >= 0 ? "+" : "")}{count} thẻ '{relicId}'. Tổng hiện có: {newShards}");
            OnRelicShardsChanged?.Invoke(relicId, newShards);
        }

        private void EnsureStarterRelicUnlocked(string starterId)
        {
            if (_saveData != null)
            {
                int star = _saveData.GetRelicStarLevel(starterId);
                if (star == 0)
                {
                    _saveData.SetRelicProgress(starterId, 0, 1);
                    SaveDataToDisk();
                }
            }
        }

        private void SaveDataToDisk()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }
            else if (_saveData != null)
            {
                SaveSystem.Save(_saveData);
            }
        }
    }
}

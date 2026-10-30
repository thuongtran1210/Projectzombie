using System;
using UnityEngine;
using ProjectZombie.Core.Save;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Domain Service trung tâm quản lý logic Sở Hữu, Thu Thập Thẻ Mảnh và Gộp Thẻ Nâng Sao Vũ Khí / Pháp Bảo / Lính / Trụ.
    /// Thiết kế chuẩn SOLID, Data-Driven, đọc toàn bộ chi phí & chỉ số từ RelicStarProgressionSO (Single Source of Truth).
    /// Kế thừa PersistentSingleton<RelicInventoryManager> chuẩn kiến trúc.
    /// </summary>
    public class RelicInventoryManager : PersistentSingleton<RelicInventoryManager>, ISaveableModule
    {
        [Header("Cấu Hình Progression")]
        [SerializeField] private RelicStarProgressionSO _progressionConfig;

        public event Action<string, int> OnRelicStarUpgraded; // (relicId, newStar)
        public event Action<string, int> OnRelicShardsChanged;  // (relicId, newShardCount)
        public event Action<string> OnRelicUnlocked;           // (relicId)

        private MetaProgressionSaveData _saveData;

        // ====================================================================
        // ISaveableModule IMPLEMENTATION
        // ====================================================================

        public void InitializeFromSave(MetaProgressionSaveData data)
        {
            Initialize(data);
        }

        public void PopulateSaveData(MetaProgressionSaveData data)
        {
            // RelicInventory cập nhật trực tiếp vào relicProgressList trong quá trình nâng sao / nhặt shard
        }

        protected override void Awake()
        {
            base.Awake();
            EnsureInitialized();
        }

        private void Start()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            EnsureProgressionConfigLoaded();

            if (_saveData == null)
            {
                var gm = GameManager.Instance;
                if (gm != null && gm.SaveData != null)
                {
                    _saveData = gm.SaveData;
                }
                else
                {
                    _saveData = SaveSystem.Load() ?? new MetaProgressionSaveData();
                }

                EnsureDefaultDefenseUnitsUnlocked();
                Debug.Log($"[RelicInventoryManager] Khởi tạo thành công với {_saveData.relicProgressList?.Count ?? 0} bản ghi pháp bảo.");
            }
        }

        private void EnsureProgressionConfigLoaded()
        {
            if (_progressionConfig != null) return;

            _progressionConfig = Resources.Load<RelicStarProgressionSO>("RelicStarProgressionConfig");
#if UNITY_EDITOR
            if (_progressionConfig == null)
            {
                _progressionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<RelicStarProgressionSO>("Assets/_Data/RelicStarProgressionConfig.asset");
            }
#endif
            if (_progressionConfig == null)
            {
                _progressionConfig = ScriptableObject.CreateInstance<RelicStarProgressionSO>();
            }
        }

        public void Initialize(MetaProgressionSaveData saveData)
        {
            _saveData = saveData ?? new MetaProgressionSaveData();
            EnsureProgressionConfigLoaded();

            // Đảm bảo các thực thể khởi đầu từ Defense3DCatalog luôn đạt tối thiểu 1 sao
            EnsureDefaultDefenseUnitsUnlocked();

            Debug.Log($"[RelicInventoryManager] Khởi tạo thành công với {_saveData.relicProgressList?.Count ?? 0} bản ghi pháp bảo.");
        }

        public void SetProgressionConfig(RelicStarProgressionSO config)
        {
            _progressionConfig = config;
        }

        public RelicStarProgressionSO ProgressionConfig
        {
            get
            {
                EnsureProgressionConfigLoaded();
                return _progressionConfig;
            }
        }

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
            if (currentStar >= 5) return null;

            int nextStar = currentStar + 1;
            EnsureProgressionConfigLoaded();
            return _progressionConfig.GetStepConfig(nextStar);
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
            if (nextConfig == null || nextConfig.requiredShards < 0 || nextConfig.coTienCost < 0
                || nextConfig.targetStar != currentStar + 1)
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

            int currentMoney = MetaCurrencyManager.HasInstance ? MetaCurrencyManager.Instance.TotalCurrency : (_saveData?.totalCurrency ?? 0);
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

            var currency = MetaCurrencyManager.HasInstance ? MetaCurrencyManager.Instance : null;
            if (currency != null && !ReferenceEquals(currency.GetSaveData(), _saveData)) return false;
            int newShards = currentShards - nextConfig.requiredShards;
            int newStar = currentStar + 1;
            if (!CardStarUpgradeTransaction.TryCommit(_saveData, relicId, false,
                nextConfig.requiredShards, nextConfig.coTienCost, SaveSystem.Save)) return false;

            Debug.Log($"[RelicInventoryManager] Gộp thẻ thành công '{relicId}': Cấp sao mới = {newStar}★, Thẻ còn lại = {newShards}");

            currency?.RefreshCommittedBalance();
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

        private void EnsureDefaultDefenseUnitsUnlocked()
        {
            EnsureStarterRelicUnlocked("wp_kiem_truc");

            var catalog = Defense3DCatalog.CreateDefault();
            LoadoutItemType[] unitTypes = { LoadoutItemType.Relic, LoadoutItemType.Minion, LoadoutItemType.Tower };
            for (int t = 0; t < unitTypes.Length; t++)
            {
                var entries = catalog.GetEntriesByType(unitTypes[t]);
                for (int i = 0; i < entries.Count; i++)
                {
                    EnsureStarterRelicUnlocked(entries[i].Id);
                }
            }
        }

        private void EnsureStarterRelicUnlocked(string starterId)
        {
            if (_saveData != null && !string.IsNullOrEmpty(starterId))
            {
                int star = _saveData.GetRelicStarLevel(starterId);
                if (star == 0)
                {
                    _saveData.SetRelicProgress(starterId, _saveData.GetRelicShards(starterId), 1);
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

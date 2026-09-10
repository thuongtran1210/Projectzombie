using System;
using UnityEngine;
using ProjectZombie.Core.Save;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.MetaProgression
{
    public enum UpgradeCharacterResult
    {
        Success,
        NotEnoughShards,
        NotEnoughCurrency,
        MaxStarReached,
        InvalidCharacter
    }

    /// <summary>
    /// Domain Service trung tâm quản lý logic Sở Hữu, Thu Thập Thẻ Mảnh và Gộp Thẻ Nâng Sao Tướng / Anh Hùng.
    /// Thiết kế chuẩn SOLID, Data-Driven, phát sinh sự kiện để UI Presenter cập nhật phản hồi.
    /// Kế thừa PersistentSingleton<CharacterProgressionManager> chuẩn kiến trúc.
    /// </summary>
    public class CharacterProgressionManager : PersistentSingleton<CharacterProgressionManager>
    {
        [Header("Cấu Hình Progression")]
        [SerializeField] private CharacterStarProgressionSO _progressionConfig;

        public event Action<string, int> OnCharacterStarUpgraded; // (characterId, newStar)
        public event Action<string, int> OnCharacterShardsChanged;  // (characterId, newShardCount)
        public event Action<string> OnCharacterUnlocked;           // (characterId)

        private MetaProgressionSaveData _saveData;

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

            if (_progressionConfig == null)
            {
                _progressionConfig = Resources.Load<CharacterStarProgressionSO>("CharacterStarProgressionConfig");
#if UNITY_EDITOR
                if (_progressionConfig == null)
                {
                    _progressionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterStarProgressionSO>("Assets/_Data/CharacterStarProgressionConfig.asset");
                }
#endif
            }
        }

        public void Initialize(MetaProgressionSaveData saveData)
        {
            _saveData = saveData ?? new MetaProgressionSaveData();

            // Đảm bảo tướng khởi đầu (C001_ThuSinh hoặc default) luôn đạt tối thiểu 1 sao nếu chưa có bản ghi
            EnsureStarterCharacterUnlocked("C001_ThuSinh");

            // Migration từ unlockedCharacters cũ nếu có
            if (_saveData.unlockedCharacters != null)
            {
                foreach (var heroId in _saveData.unlockedCharacters)
                {
                    if (!string.IsNullOrEmpty(heroId) && _saveData.GetCharacterStarLevel(heroId) < 1)
                    {
                        _saveData.SetCharacterProgress(heroId, _saveData.GetCharacterShards(heroId), 1);
                    }
                }
            }

            Debug.Log($"[CharacterProgressionManager] Khởi tạo thành công với {_saveData.characterProgressList?.Count ?? 0} bản ghi thẻ tướng.");
        }

        public void SetProgressionConfig(CharacterStarProgressionSO config)
        {
            _progressionConfig = config;
        }

        public CharacterStarProgressionSO ProgressionConfig => _progressionConfig;

        public int GetCharacterStarLevel(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return 0;
            if (_saveData == null) return characterId == "C001_ThuSinh" || characterId == "default" ? 1 : 0;
            return _saveData.GetCharacterStarLevel(characterId);
        }

        public int GetCharacterShardCount(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || _saveData == null) return 0;
            return _saveData.GetCharacterShards(characterId);
        }

        public bool IsCharacterUnlocked(string characterId)
        {
            return GetCharacterStarLevel(characterId) >= 1;
        }

        public CharacterStarStepConfig GetNextStepConfig(string characterId)
        {
            int currentStar = GetCharacterStarLevel(characterId);
            if (currentStar >= 5) return null; // Đã đạt max

            int nextStar = currentStar + 1;
            if (_progressionConfig != null)
            {
                return _progressionConfig.GetStepConfig(nextStar);
            }

            // Fallback nếu chưa gán config SO
            int reqShards = nextStar == 1 ? 10 : (nextStar == 2 ? 20 : (nextStar == 3 ? 40 : (nextStar == 4 ? 80 : 150)));
            int cost = nextStar == 1 ? 0 : (nextStar == 2 ? 800 : (nextStar == 3 ? 2500 : (nextStar == 4 ? 6000 : 15000)));
            return new CharacterStarStepConfig { targetStar = nextStar, requiredShards = reqShards, coTienCost = cost };
        }

        /// <summary>
        /// Cộng thêm số thẻ mảnh tướng (khi quay Gacha, nhận thưởng vượt ải).
        /// </summary>
        public void AddCharacterShards(string characterId, int amount)
        {
            if (string.IsNullOrEmpty(characterId) || amount <= 0) return;
            EnsureInitialized();

            int currentShards = _saveData.GetCharacterShards(characterId);
            int currentStar = _saveData.GetCharacterStarLevel(characterId);
            int newShards = currentShards + amount;

            _saveData.SetCharacterProgress(characterId, newShards, currentStar);
            SaveCurrentData();

            OnCharacterShardsChanged?.Invoke(characterId, newShards);
            Debug.Log($"[CharacterProgressionManager] Nhận +{amount} Mảnh Tướng [{characterId}]. Tổng hiện tại: {newShards}");
        }

        /// <summary>
        /// Kiểm tra xem tướng có đủ điều kiện gộp mảnh thăng sao không.
        /// </summary>
        public bool CanUpgradeCharacter(string characterId, out string reason)
        {
            reason = "";
            if (string.IsNullOrEmpty(characterId))
            {
                reason = "Định danh tướng không hợp lệ";
                return false;
            }

            EnsureInitialized();
            int currentStar = GetCharacterStarLevel(characterId);
            if (currentStar >= 5)
            {
                reason = "Tướng đã đạt cấp sao tối đa (5★ - Thức Tỉnh Thần Thoại)";
                return false;
            }

            var nextConfig = GetNextStepConfig(characterId);
            if (nextConfig == null)
            {
                reason = "Không tìm thấy cấu hình cấp sao kế tiếp";
                return false;
            }

            int shards = GetCharacterShardCount(characterId);
            if (shards < nextConfig.requiredShards)
            {
                reason = $"Không đủ thẻ mảnh ({shards}/{nextConfig.requiredShards})";
                return false;
            }

            int curMoney = _saveData != null ? _saveData.totalCurrency : 0;
            if (MetaCurrencyManager.Instance != null)
            {
                curMoney = MetaCurrencyManager.Instance.TotalCurrency;
            }

            if (curMoney < nextConfig.coTienCost)
            {
                reason = $"Không đủ Cổ Tiền (Cần {nextConfig.coTienCost:N0})";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Thực hiện gộp thẻ thăng sao cho Tướng.
        /// </summary>
        public UpgradeCharacterResult TryUpgradeCharacterStar(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return UpgradeCharacterResult.InvalidCharacter;
            EnsureInitialized();

            int currentStar = GetCharacterStarLevel(characterId);
            if (currentStar >= 5) return UpgradeCharacterResult.MaxStarReached;

            var nextConfig = GetNextStepConfig(characterId);
            if (nextConfig == null) return UpgradeCharacterResult.InvalidCharacter;

            int shards = GetCharacterShardCount(characterId);
            if (shards < nextConfig.requiredShards) return UpgradeCharacterResult.NotEnoughShards;

            int curMoney = _saveData != null ? _saveData.totalCurrency : 0;
            if (MetaCurrencyManager.Instance != null)
            {
                curMoney = MetaCurrencyManager.Instance.TotalCurrency;
            }

            if (curMoney < nextConfig.coTienCost) return UpgradeCharacterResult.NotEnoughCurrency;

            // 1. Trừ Cổ Tiền
            if (nextConfig.coTienCost > 0)
            {
                if (MetaCurrencyManager.Instance != null)
                {
                    MetaCurrencyManager.Instance.SpendCurrency(nextConfig.coTienCost);
                }
                else if (_saveData != null)
                {
                    _saveData.totalCurrency -= nextConfig.coTienCost;
                }
            }

            // 2. Trừ mảnh thẻ & Tăng sao
            int newShards = shards - nextConfig.requiredShards;
            int newStar = currentStar + 1;
            _saveData.SetCharacterProgress(characterId, newShards, newStar);

            SaveCurrentData();

            // 3. Phát sinh sự kiện
            OnCharacterShardsChanged?.Invoke(characterId, newShards);
            OnCharacterStarUpgraded?.Invoke(characterId, newStar);

            if (currentStar == 0 && newStar == 1)
            {
                OnCharacterUnlocked?.Invoke(characterId);
            }

            Debug.Log($"<color=#00FF88>[CharacterProgressionManager]</color> Thăng sao Tướng [{characterId}] thành công lên {newStar}★!");
            return UpgradeCharacterResult.Success;
        }

        private void EnsureStarterCharacterUnlocked(string starterId)
        {
            if (_saveData == null) return;
            if (_saveData.GetCharacterStarLevel(starterId) < 1)
            {
                _saveData.SetCharacterProgress(starterId, _saveData.GetCharacterShards(starterId), 1);
                SaveCurrentData();
            }
        }

        private void SaveCurrentData()
        {
            if (_saveData != null)
            {
                SaveSystem.Save(_saveData);
            }
        }
    }
}

using System;
using UnityEngine;
using ProjectZombie.Core.Save;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.MetaProgression
{
    public enum UpgradeCharacterResult
    {
        Success,
        NotEnoughShards,
        NotEnoughCurrency,
        MaxStarReached,
        InvalidCharacter,
        SaveFailed
    }

    /// <summary>
    /// Domain Service trung tâm quản lý logic Sở Hữu, Thu Thập Thẻ Mảnh và Gộp Thẻ Nâng Sao Tướng / Anh Hùng.
    /// Thiết kế chuẩn SOLID, Data-Driven, đọc toàn bộ chi phí & chỉ số từ CharacterStarProgressionSO (Single Source of Truth).
    /// Kế thừa PersistentSingleton<CharacterProgressionManager> chuẩn kiến trúc.
    /// </summary>
    public class CharacterProgressionManager : PersistentSingleton<CharacterProgressionManager>, ISaveableModule
    {
        [Header("Cấu Hình Progression")]
        [SerializeField] private CharacterStarProgressionSO _progressionConfig;

        public event Action<string, int> OnCharacterStarUpgraded; // (characterId, newStar)
        public event Action<string, int> OnCharacterShardsChanged;  // (characterId, newShardCount)
        public event Action<string> OnCharacterUnlocked;           // (characterId)

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
            // CharacterProgressionManager cập nhật trực tiếp vào characterProgressList khi nâng sao
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
                    Initialize(gm.SaveData);
                }
                else
                {
                    Initialize(SaveSystem.Load());
                }
            }
        }

        private void EnsureProgressionConfigLoaded()
        {
            if (_progressionConfig != null) return;

            _progressionConfig = Resources.Load<CharacterStarProgressionSO>("CharacterStarProgressionConfig");
#if UNITY_EDITOR
            if (_progressionConfig == null)
            {
                _progressionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterStarProgressionSO>("Assets/_Data/CharacterStarProgressionConfig.asset");
            }
#endif
            if (_progressionConfig == null)
            {
                _progressionConfig = ScriptableObject.CreateInstance<CharacterStarProgressionSO>();
            }
        }

        public void Initialize(MetaProgressionSaveData saveData)
        {
            _saveData = saveData ?? new MetaProgressionSaveData();
            EnsureProgressionConfigLoaded();

            // 1. Mở khóa các tướng khởi đầu dựa trên Single Source of Truth (CharacterDatabaseSO & Defense3DCatalog)
            var charDb = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
            if (charDb != null && charDb.Characters != null)
            {
                for (int i = 0; i < charDb.Characters.Count; i++)
                {
                    var hero = charDb.Characters[i];
                    if (hero != null && hero.isUnlockedByDefault && !string.IsNullOrEmpty(hero.characterId))
                    {
                        EnsureStarterCharacterUnlocked(hero.characterId);
                    }
                }
            }

            var catalog = Defense3DCatalog.CreateDefault();
            foreach (var entry in catalog.GetEntriesByType(LoadoutItemType.Hero))
            {
                string metaId = DefenseLoadoutIdMapper.GetMetaCharacterId(entry.Id);
                EnsureStarterCharacterUnlocked(metaId);
            }

            // 2. Migration từ unlockedCharacters cũ nếu có (chỉ migrate các tướng hợp lệ của Defense 3D)
            if (_saveData.unlockedCharacters != null)
            {
                foreach (var heroId in _saveData.unlockedCharacters)
                {
                    if (!string.IsNullOrEmpty(heroId) && DefenseLoadoutIdMapper.TryMapHeroId(heroId, out string mappedHero))
                    {
                        string canonicalMetaId = DefenseLoadoutIdMapper.GetMetaCharacterId(mappedHero) ?? heroId;
                        if (_saveData.GetCharacterStarLevel(canonicalMetaId) < 1)
                        {
                            _saveData.SetCharacterProgress(canonicalMetaId, _saveData.GetCharacterShards(canonicalMetaId), 1);
                        }
                    }
                }
            }

            Debug.Log($"[CharacterProgressionManager] Khởi tạo thành công với {_saveData.characterProgressList?.Count ?? 0} bản ghi thẻ tướng.");
        }

        public void SetProgressionConfig(CharacterStarProgressionSO config)
        {
            _progressionConfig = config;
        }

        public CharacterStarProgressionSO ProgressionConfig
        {
            get
            {
                EnsureProgressionConfigLoaded();
                return _progressionConfig;
            }
        }

        public int GetCharacterStarLevel(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return 0;
            if (_saveData == null)
            {
                return DefenseLoadoutIdMapper.TryMapHeroId(characterId, out _) ? 1 : 0;
            }
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
            if (currentStar >= 5) return null;

            int nextStar = currentStar + 1;
            EnsureProgressionConfigLoaded();
            return _progressionConfig.GetStepConfig(nextStar);
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
            if (nextConfig == null || nextConfig.requiredShards < 0 || nextConfig.coTienCost < 0
                || nextConfig.targetStar != currentStar + 1)
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
            if (MetaCurrencyManager.HasInstance)
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
            if (nextConfig == null || nextConfig.requiredShards < 0 || nextConfig.coTienCost < 0
                || nextConfig.targetStar != currentStar + 1) return UpgradeCharacterResult.InvalidCharacter;

            int shards = GetCharacterShardCount(characterId);
            if (shards < nextConfig.requiredShards) return UpgradeCharacterResult.NotEnoughShards;

            int curMoney = _saveData != null ? _saveData.totalCurrency : 0;
            if (MetaCurrencyManager.HasInstance)
            {
                curMoney = MetaCurrencyManager.Instance.TotalCurrency;
            }

            if (curMoney < nextConfig.coTienCost) return UpgradeCharacterResult.NotEnoughCurrency;

            var currency = MetaCurrencyManager.HasInstance ? MetaCurrencyManager.Instance : null;
            if (currency != null && !ReferenceEquals(currency.GetSaveData(), _saveData))
                return UpgradeCharacterResult.SaveFailed;

            int newShards = shards - nextConfig.requiredShards;
            int newStar = currentStar + 1;
            if (!CardStarUpgradeTransaction.TryCommit(_saveData, characterId, true,
                nextConfig.requiredShards, nextConfig.coTienCost, SaveSystem.Save))
                return UpgradeCharacterResult.SaveFailed;

            // 3. Phát sinh sự kiện
            currency?.RefreshCommittedBalance();
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
            if (_saveData == null || string.IsNullOrEmpty(starterId)) return;
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

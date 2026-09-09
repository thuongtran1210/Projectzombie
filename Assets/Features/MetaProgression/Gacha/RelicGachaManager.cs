using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Core.Save;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.MetaProgression.Gacha.Data;
using ProjectZombie.Features.MetaProgression.Gacha.Core;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.MetaProgression.Gacha
{
    /// <summary>
    /// Domain Service trung tâm điều phối toàn bộ nghiệp vụ Gacha Mở Rương Pháp Bảo (Meta Shop).
    /// Áp dụng SOLID, Clean Architecture, Pity Pipeline (Hard/Soft Pity), và tích hợp mượt mà với RelicInventoryManager.
    /// Kế thừa PersistentSingleton<RelicGachaManager> chuẩn kiến trúc.
    /// </summary>
    public class RelicGachaManager : PersistentSingleton<RelicGachaManager>
    {
        [Header("Banner Cấu Hình Hiện Tại")]
        [SerializeField] private GachaBannerConfigSO _activeBanner;

        public event Action<List<GachaDropResult>> OnGachaSuccess;
        public event Action<string> OnGachaFailed;
        public event Action<int, int> OnPityCountersChanged; // (pityLegendary, pityEpic)

        private ICurrencyProcessor _currencyProcessor;
        private IGachaDataProvider _dataProvider;
        private MetaProgressionSaveData _saveData;

        public GachaBannerConfigSO ActiveBanner
        {
            get
            {
                if (_activeBanner == null)
                {
                    _activeBanner = Resources.Load<GachaBannerConfigSO>("Gacha/banner_standard");
                }
                return _activeBanner;
            }
        }
        public int PityLegendary => _saveData != null ? _saveData.gachaPityLegendary : 0;
        public int PityEpic => _saveData != null ? _saveData.gachaPityEpic : 0;

        protected override void Awake()
        {
            base.Awake();

            if (_currencyProcessor == null) _currencyProcessor = new CoTienCurrencyProcessor();
            if (_dataProvider == null) _dataProvider = new LocalSOGachaDataProvider();

            if (_activeBanner == null)
            {
                _activeBanner = Resources.Load<GachaBannerConfigSO>("Gacha/banner_standard");
            }

            EnsureInitialized();
        }

        private void Start()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_currencyProcessor == null) _currencyProcessor = new CoTienCurrencyProcessor();
            if (_dataProvider == null) _dataProvider = new LocalSOGachaDataProvider();

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
            OnPityCountersChanged?.Invoke(_saveData.gachaPityLegendary, _saveData.gachaPityEpic);
            Debug.Log($"[RelicGachaManager] Khởi tạo thành công. Pity: Leg={_saveData.gachaPityLegendary}, Epic={_saveData.gachaPityEpic}");
        }

        /// <summary>
        /// Cho phép tiêm (Inject) Processor tiền tệ khác (Linh Thạch, Vé quay, Ads...).
        /// </summary>
        public void SetCurrencyProcessor(ICurrencyProcessor processor)
        {
            _currencyProcessor = processor ?? new CoTienCurrencyProcessor();
        }

        /// <summary>
        /// Cho phép tiêm Data Provider khác (Server Remote Config).
        /// </summary>
        public void SetDataProvider(IGachaDataProvider provider)
        {
            _dataProvider = provider ?? new LocalSOGachaDataProvider();
        }

        public void SetActiveBanner(GachaBannerConfigSO banner)
        {
            _activeBanner = banner;
        }

        /// <summary>
        /// Thực thi lượt quay Gacha (count = 1 hoặc 10).
        /// </summary>
        public void Roll(int count)
        {
            if (_activeBanner == null || _activeBanner.DropPool == null || _activeBanner.DropPool.Count == 0)
            {
                OnGachaFailed?.Invoke("Không tìm thấy dữ liệu Bảo Rương.");
                return;
            }

            if (_currencyProcessor == null)
            {
                _currencyProcessor = new CoTienCurrencyProcessor();
            }

            int cost = count == 1 ? _activeBanner.singleRollCost : _activeBanner.multiRollCost;

            if (!_currencyProcessor.HasEnough(cost))
            {
                OnGachaFailed?.Invoke($"Không đủ tiền tệ! Cần <color=#FFD700>{cost:N0}</color>.");
                return;
            }

            if (!_currencyProcessor.TrySpend(cost))
            {
                OnGachaFailed?.Invoke("Giao dịch chi tiêu thất bại.");
                return;
            }

            var results = new List<GachaDropResult>();

            for (int i = 0; i < count; i++)
            {
                var drop = CalculateSingleDrop(_activeBanner);
                
                // Kiểm tra trạng thái cấp sao hiện tại
                int currentStar = RelicInventoryManager.Instance != null ? RelicInventoryManager.Instance.GetRelicStarLevel(drop.relicId) : 1;
                bool isMaxStar = currentStar >= 5;
                bool isNew = RelicInventoryManager.Instance != null && !RelicInventoryManager.Instance.IsRelicUnlocked(drop.relicId);
                
                bool isConverted = false;
                int convertedCoins = 0;

                if (isMaxStar)
                {
                    // Pháp bảo đã đạt tối đa 5 sao -> Tự động quy đổi mảnh thành Cổ Tiền
                    // Tỷ lệ: Phổ Thông = 20/mảnh, Bảo Phẩm = 40/mảnh, Cực Phẩm = 80/mảnh, Thần Binh = 200/mảnh
                    int ratePerShard = drop.rarity switch
                    {
                        ItemRarity.Common => 20,
                        ItemRarity.Rare => 40,
                        ItemRarity.Epic => 80,
                        ItemRarity.Legendary => 200,
                        _ => 20
                    };

                    convertedCoins = drop.shardAmount * ratePerShard;
                    isConverted = true;

                    if (MetaCurrencyManager.Instance != null)
                    {
                        MetaCurrencyManager.Instance.AddCurrency(convertedCoins);
                    }
                }
                else
                {
                    // Chưa đạt max sao -> Thêm mảnh vào kho
                    if (RelicInventoryManager.Instance != null)
                    {
                        RelicInventoryManager.Instance.AddRelicShards(drop.relicId, drop.shardAmount);
                    }
                }

                int totalShards = RelicInventoryManager.Instance != null ? RelicInventoryManager.Instance.GetRelicShardCount(drop.relicId) : drop.shardAmount;

                results.Add(new GachaDropResult
                {
                    relicId = drop.relicId,
                    relicName = drop.relicName,
                    rarity = drop.rarity,
                    element = drop.element,
                    shardCount = drop.shardAmount,
                    isNewUnlock = isNew,
                    currentStarLevel = currentStar,
                    totalShardsAfter = totalShards,
                    icon = drop.icon,
                    isConvertedToCurrency = isConverted,
                    convertedCurrencyAmount = convertedCoins
                });
            }

            // Tự động lưu game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }

            OnPityCountersChanged?.Invoke(_saveData != null ? _saveData.gachaPityLegendary : 0, _saveData != null ? _saveData.gachaPityEpic : 0);
            OnGachaSuccess?.Invoke(results);
        }

        /// <summary>
        /// Thuật toán quay 1 lượt kết hợp Hard/Soft Pity & Guaranteed Epic.
        /// </summary>
        private GachaDropItem CalculateSingleDrop(GachaBannerConfigSO banner)
        {
            if (_saveData == null) _saveData = new MetaProgressionSaveData();

            _saveData.gachaPityLegendary++;
            _saveData.gachaPityEpic++;
            _saveData.totalGachaRolls++;

            // 1. Kiểm tra Hard Pity Thần Binh (Legendary)
            bool forceLegendary = _saveData.gachaPityLegendary >= banner.hardPityLegendary;

            // 2. Kiểm tra Soft Pity Thần Binh
            float extraLegendaryChance = 0f;
            if (!forceLegendary && _saveData.gachaPityLegendary >= banner.softPityStart)
            {
                int steps = _saveData.gachaPityLegendary - banner.softPityStart + 1;
                extraLegendaryChance = steps * banner.softPityRatePerRoll; // VD: 5% * số bước
            }

            // 3. Kiểm tra Guaranteed Epic
            bool forceEpicOrBetter = _saveData.gachaPityEpic >= banner.epicGuaranteedEvery;

            // Phân nhóm vật phẩm theo độ hiếm
            var legendaryPool = new List<GachaDropItem>();
            var epicPool = new List<GachaDropItem>();
            var otherPool = new List<GachaDropItem>();

            for (int i = 0; i < banner.DropPool.Count; i++)
            {
                var item = banner.DropPool[i];
                if (item.rarity == ItemRarity.Legendary) legendaryPool.Add(item);
                else if (item.rarity == ItemRarity.Epic) epicPool.Add(item);
                else otherPool.Add(item);
            }

            // TH1: Kích hoạt Thần Binh (Do Hard Pity hoặc trúng Soft Pity Roll)
            if ((forceLegendary || UnityEngine.Random.value < extraLegendaryChance) && legendaryPool.Count > 0)
            {
                _saveData.gachaPityLegendary = 0;
                _saveData.gachaPityEpic = 0; // Ra Thần Binh cũng reset Pity Epic
                return PickWeightedRandom(legendaryPool);
            }

            // TH2: Kích hoạt Bảo Hiểm Epic
            if (forceEpicOrBetter)
            {
                var combinedEpicLegendary = new List<GachaDropItem>();
                combinedEpicLegendary.AddRange(epicPool);
                combinedEpicLegendary.AddRange(legendaryPool);

                if (combinedEpicLegendary.Count > 0)
                {
                    var picked = PickWeightedRandom(combinedEpicLegendary);
                    if (picked.rarity == ItemRarity.Legendary) _saveData.gachaPityLegendary = 0;
                    _saveData.gachaPityEpic = 0;
                    return picked;
                }
            }

            // TH3: Quay thông thường theo toàn bộ trọng số
            var result = PickWeightedRandom(banner.DropPool);
            if (result.rarity == ItemRarity.Legendary)
            {
                _saveData.gachaPityLegendary = 0;
                _saveData.gachaPityEpic = 0;
            }
            else if (result.rarity == ItemRarity.Epic)
            {
                _saveData.gachaPityEpic = 0;
            }

            return result;
        }

        private GachaDropItem PickWeightedRandom(List<GachaDropItem> pool)
        {
            if (pool == null || pool.Count == 0) return null;

            float totalWeight = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                totalWeight += Mathf.Max(0.1f, pool[i].weight);
            }

            float randomVal = UnityEngine.Random.Range(0f, totalWeight);
            float currentSum = 0f;

            for (int i = 0; i < pool.Count; i++)
            {
                currentSum += Mathf.Max(0.1f, pool[i].weight);
                if (randomVal <= currentSum)
                {
                    return pool[i];
                }
            }

            return pool[pool.Count - 1];
        }
    }
}

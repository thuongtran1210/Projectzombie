using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Features.Upgrades.Filters;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Quản lý danh sách tất cả các nâng cấp có thể có trong game (Pool).
    /// Cung cấp các lựa chọn ngẫu nhiên khi người chơi lên cấp thông qua hệ thống Filter Strategy Pattern
    /// và Dynamic Synergy Weight Pipeline (Ưu tiên nâng cấp đồ đang sở hữu + Tương sinh Ngũ Hành).
    /// </summary>
    public class UpgradeManager : MonoBehaviour, IUpgradeService, ProjectZombie.Core.Architecture.IResettableStatic
    {
        public static UpgradeManager Instance { get; private set; }

        public void ResetStaticState()
        {
            Instance = null;
            _cachedMasterUpgrades = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        [Header("Upgrade Pool")]
        [SerializeField] private List<UpgradeData> _allAvailableUpgrades = new List<UpgradeData>();

        [Header("Fallback Rewards (Khi cạn pool)")]
        [SerializeField] private List<UpgradeData> _fallbackRewards = new List<UpgradeData>();

        private readonly HashSet<UpgradeData> _bannedUpgrades = new HashSet<UpgradeData>();
        private readonly List<IUpgradeFilter> _filters = new List<IUpgradeFilter>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitDefaultFilters();
            InitDefaultFallbackRewards();
            AutoPopulateUpgradesIfEmpty();
        }

        private void InitDefaultFilters()
        {
            _filters.Clear();
            _filters.Add(new BannedUpgradeFilter(_bannedUpgrades));
            _filters.Add(new AvailabilityUpgradeFilter());
            _filters.Add(new YinYangUpgradeFilter());
        }

        private void InitDefaultFallbackRewards()
        {
            if (_fallbackRewards == null || _fallbackRewards.Count == 0)
            {
                var healCard = ScriptableObject.CreateInstance<FallbackRewardUpgradeData>();
                healCard.id = "FB_HEAL";
                healCard.upgradeName = "Tiên Đan Hồi Máu";
                healCard.description = "Hồi phục ngay lập tức <color=#00FF88>40% Máu tối đa</color>.";
                healCard.upgradeType = UpgradeType.CommonUpgrade;
                healCard.rewardType = FallbackRewardType.HealHealth;
                healCard.healPercentage = 0.40f;

                var goldCard = ScriptableObject.CreateInstance<FallbackRewardUpgradeData>();
                goldCard.id = "FB_GOLD";
                goldCard.upgradeName = "Túi Vàng Phong Thủy";
                goldCard.description = "Thu thập ngân lượng tăng thêm <color=#FFD700>+150 Vàng</color>.";
                goldCard.upgradeType = UpgradeType.CommonUpgrade;
                goldCard.rewardType = FallbackRewardType.GrantGold;
                goldCard.goldAmount = 150;

                _fallbackRewards = new List<UpgradeData> { healCard, goldCard };
            }
        }

        public void RegisterFilter(IUpgradeFilter filter)
        {
            if (filter != null && !_filters.Contains(filter))
            {
                _filters.Add(filter);
            }
        }

        public void RemoveFilter(IUpgradeFilter filter)
        {
            if (filter != null)
            {
                _filters.Remove(filter);
            }
        }

        private static List<UpgradeData> _cachedMasterUpgrades;
        private static UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<UpgradeData>>? _addressablesHandle;
        private Task _loadingTask;

        public void AutoPopulateUpgradesIfEmpty()
        {
            if (_allAvailableUpgrades == null || _allAvailableUpgrades.Count == 0)
            {
                if (_cachedMasterUpgrades != null && _cachedMasterUpgrades.Count > 0)
                {
                    _allAvailableUpgrades = new List<UpgradeData>(_cachedMasterUpgrades);
                    return;
                }
#if UNITY_EDITOR
                PopulateAllAvailableUpgrades();
#else
                if (_loadingTask == null || _loadingTask.IsCompleted)
                {
                    _loadingTask = PopulateAllAvailableUpgradesAsync();
                }
#endif
            }
        }

        public async Task AutoPopulateUpgradesIfEmptyAsync()
        {
            if (_allAvailableUpgrades == null || _allAvailableUpgrades.Count == 0)
            {
                if (_cachedMasterUpgrades != null && _cachedMasterUpgrades.Count > 0)
                {
                    _allAvailableUpgrades = new List<UpgradeData>(_cachedMasterUpgrades);
                    return;
                }

                if (_loadingTask != null && !_loadingTask.IsCompleted)
                {
                    await _loadingTask;
                    return;
                }

                _loadingTask = PopulateAllAvailableUpgradesAsync();
                await _loadingTask;
            }
        }

        [ContextMenu("Populate All Upgrades")]
        public void PopulateAllAvailableUpgrades()
        {
            _allAvailableUpgrades.Clear();

            if (_cachedMasterUpgrades != null && _cachedMasterUpgrades.Count > 0)
            {
                _allAvailableUpgrades.AddRange(_cachedMasterUpgrades);
                return;
            }

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpgradeData");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var upgrade = UnityEditor.AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                if (upgrade != null && !_allAvailableUpgrades.Contains(upgrade) && !(upgrade is FallbackRewardUpgradeData))
                {
                    _allAvailableUpgrades.Add(upgrade);
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[UpgradeManager] Tự động nạp {_allAvailableUpgrades.Count} thẻ UpgradeData từ dự án.");
            _cachedMasterUpgrades = new List<UpgradeData>(_allAvailableUpgrades);
#else
            // Fallback sang async task nếu gọi từ sync context trên non-editor
            _ = PopulateAllAvailableUpgradesAsync();
#endif
        }

        public async Task PopulateAllAvailableUpgradesAsync()
        {
            _allAvailableUpgrades.Clear();

            if (_cachedMasterUpgrades != null && _cachedMasterUpgrades.Count > 0)
            {
                _allAvailableUpgrades.AddRange(_cachedMasterUpgrades);
                return;
            }

#if UNITY_EDITOR
            PopulateAllAvailableUpgrades();
            await Task.Yield();
#else
            // 1. Ưu tiên nạp danh sách Thẻ Nâng Cấp từ Addressables Label "UpgradeData" (Bất đồng bộ - 0 Hitch)
            try
            {
                var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<UpgradeData>("UpgradeData", (u) =>
                {
                    if (u != null && !_allAvailableUpgrades.Contains(u) && !(u is FallbackRewardUpgradeData))
                    {
                        _allAvailableUpgrades.Add(u);
                    }
                });

                _addressablesHandle = handle;
                await handle.Task;

                if (_allAvailableUpgrades.Count > 0)
                {
                    Debug.Log($"[UpgradeManager] Load thành công {_allAvailableUpgrades.Count} thẻ UpgradeData từ Addressables (Async).");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[UpgradeManager] Không thể nạp thẻ từ Addressables: {ex.Message}. Fallback sang Resources...");
            }

            // 2. Fallback sang Resources nếu chưa có gói Addressables
            if (_allAvailableUpgrades.Count == 0)
            {
                var request = Resources.LoadAllAsync<UpgradeData>("Upgrades");
                await AwaitResourceRequest(request);

                var loadedUpgrades = request.allAssets;
                if (loadedUpgrades == null || loadedUpgrades.Length == 0)
                {
                    var fallbackReq = Resources.LoadAllAsync<UpgradeData>("");
                    await AwaitResourceRequest(fallbackReq);
                    loadedUpgrades = fallbackReq.allAssets;
                }

                if (loadedUpgrades != null)
                {
                    foreach (var asset in loadedUpgrades)
                    {
                        if (asset is UpgradeData u && !(u is FallbackRewardUpgradeData) && !_allAvailableUpgrades.Contains(u))
                        {
                            _allAvailableUpgrades.Add(u);
                        }
                    }
                }
                Debug.Log($"[UpgradeManager] Load {_allAvailableUpgrades.Count} thẻ UpgradeData từ Resources (Async).");
            }
#endif
            _cachedMasterUpgrades = new List<UpgradeData>(_allAvailableUpgrades);
        }

        private static Task AwaitResourceRequest(ResourceRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            request.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }

        public void BanUpgrade(UpgradeData upgrade)
        {
            if (upgrade != null && !_bannedUpgrades.Contains(upgrade))
            {
                _bannedUpgrades.Add(upgrade);
                Debug.Log($"[UpgradeManager] Banned upgrade: {upgrade.upgradeName}");
            }
        }

        public void ResetBannedUpgrades()
        {
            _bannedUpgrades.Clear();
        }

        public bool IsBanned(UpgradeData upgrade)
        {
            return upgrade != null && _bannedUpgrades.Contains(upgrade);
        }

        private bool IsUpgradeAllowed(UpgradeData upgrade, GameObject player)
        {
            if (upgrade == null) return false;
            for (int i = 0; i < _filters.Count; i++)
            {
                if (!_filters[i].IsAllowed(upgrade, player))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tính toán trọng số xuất hiện động (Dynamic Synergy Weight):
        /// - Đồ đang có trong ba lô: Nhân 2.5x
        /// - Thẻ Tiến Hóa: Nhân 4.0x
        /// - Cùng hệ hoặc Tương Sinh Ngũ Hành: Thêm +35%
        /// </summary>
        private float CalculateEffectiveWeight(
            UpgradeData upgrade, 
            GameObject player, 
            WeaponManager weaponManager, 
            PlayerPassives playerPassives,
            HashSet<ElementType> activeElements)
        {
            float weight = Mathf.Max(1f, upgrade.spawnWeight);

            // 1. Phân loại trọng số động theo từng loại thẻ (Polymorphic Dynamic Multiplier)
            weight *= upgrade.GetDynamicWeightMultiplier(player);

            // 2. Cộng hưởng Ngũ Hành (Element Synergy Bonus)
            if (upgrade.element != ElementType.None && activeElements != null && activeElements.Count > 0)
            {
                if (activeElements.Contains(upgrade.element))
                {
                    // Đồng Hệ (Cùng nguyên tố) -> +35%
                    weight *= 1.35f;
                }
                else
                {
                    // Tương Sinh (Thủy sinh Mộc, Mộc sinh Hỏa,...)
                    foreach (var activeElem in activeElements)
                    {
                        if (ProjectZombie.Features.UI.Helpers.ElementVisualHelper.IsElementGenerative(activeElem, upgrade.element))
                        {
                            weight *= 1.25f;
                            break;
                        }
                    }
                }
            }

            return weight;
        }

        /// <summary>
        /// Trả về danh sách nâng cấp ngẫu nhiên đã qua thuật toán cân bằng thông minh.
        /// </summary>
        public List<UpgradeData> GetRandomUpgrades(int count, GameObject player)
        {
            AutoPopulateUpgradesIfEmpty();

            var weaponManager = player != null ? player.GetComponent<WeaponManager>() : null;
            var playerPassives = player != null ? player.GetComponent<PlayerPassives>() : null;

            // Thu thập các nguyên tố Ngũ Hành mà người chơi đang sở hữu
            var activeElements = new HashSet<ElementType>();
            if (weaponManager != null)
            {
                for (int i = 0; i < weaponManager.ActiveWeapons.Count; i++)
                {
                    var w = weaponManager.ActiveWeapons[i];
                    if (w != null && w.element != ElementType.None)
                    {
                        activeElements.Add(w.element);
                    }
                }
            }

            var validUpgrades = new List<UpgradeData>();
            var weights = new List<float>();
            float totalWeight = 0f;

            // 1. Lọc thẻ hợp lệ & tính trọng số động
            for (int i = 0; i < _allAvailableUpgrades.Count; i++)
            {
                var u = _allAvailableUpgrades[i];
                if (IsUpgradeAllowed(u, player))
                {
                    float effectiveWeight = CalculateEffectiveWeight(u, player, weaponManager, playerPassives, activeElements);
                    validUpgrades.Add(u);
                    weights.Add(effectiveWeight);
                    totalWeight += effectiveWeight;
                }
            }

            var selectedUpgrades = new List<UpgradeData>();

            // 2. Thuật toán Weighted Random tiêu chuẩn
            while (selectedUpgrades.Count < count && validUpgrades.Count > 0 && totalWeight > 0f)
            {
                float randomValue = Random.Range(0f, totalWeight);
                float currentSum = 0f;

                for (int i = 0; i < validUpgrades.Count; i++)
                {
                    currentSum += weights[i];
                    if (currentSum >= randomValue || i == validUpgrades.Count - 1)
                    {
                        var chosen = validUpgrades[i];
                        selectedUpgrades.Add(chosen);
                        totalWeight -= weights[i];

                        validUpgrades.RemoveAt(i);
                        weights.RemoveAt(i);
                        break;
                    }
                }
            }

            // 3. Fallback Buffer (Bảo hiểm chống cạn pool khi Max Level toàn bộ)
            int fallbackIndex = 0;
            while (selectedUpgrades.Count < count && _fallbackRewards != null && _fallbackRewards.Count > 0)
            {
                var fallback = _fallbackRewards[fallbackIndex % _fallbackRewards.Count];
                if (!selectedUpgrades.Contains(fallback))
                {
                    selectedUpgrades.Add(fallback);
                }
                fallbackIndex++;
            }

            return selectedUpgrades;
        }

        public List<UpgradeData> GetRandomUpgrades(int count)
        {
            var player = Player.PlayerProvider.HasPlayer ? Player.PlayerProvider.PlayerGameObject : null;
            return GetRandomUpgrades(count, player);
        }
    }
}

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
            ProjectZombie.Core.Architecture.ServiceContext.Unregister<IUpgradeService>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            ProjectZombie.Core.Architecture.ServiceContext.Unregister<IUpgradeService>();
        }

        [Header("Upgrade Pool")]
        [SerializeField] private List<UpgradeData> _allAvailableUpgrades = new List<UpgradeData>();

        [Header("Fallback Rewards (Khi cạn pool)")]
        [SerializeField] private List<UpgradeData> _fallbackRewards = new List<UpgradeData>();

        private readonly HashSet<UpgradeData> _bannedUpgrades = new HashSet<UpgradeData>();
        private readonly List<IUpgradeFilter> _filters = new List<IUpgradeFilter>();
        private readonly ProjectZombie.Features.Upgrades.Weighting.UpgradeWeightPipeline _weightPipeline = new ProjectZombie.Features.Upgrades.Weighting.UpgradeWeightPipeline();

        public ProjectZombie.Features.Upgrades.Weighting.UpgradeWeightPipeline WeightPipeline => _weightPipeline;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ProjectZombie.Core.Architecture.ServiceContext.Register<IUpgradeService>(this);

            InitDefaultFilters();
            InitDefaultWeightPipeline();
            InitDefaultFallbackRewards();

            // Tự động lọc sạch trùng lặp từ Scene Serialization cũ
            DeduplicateAvailableUpgrades();
            AutoPopulateUpgradesIfEmpty();
        }

        private void DeduplicateAvailableUpgrades()
        {
            if (_allAvailableUpgrades == null || _allAvailableUpgrades.Count == 0) return;

            var uniqueList = new List<UpgradeData>(_allAvailableUpgrades.Count);
            var loadedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _allAvailableUpgrades.Count; i++)
            {
                var u = _allAvailableUpgrades[i];
                if (u == null || u is FallbackRewardUpgradeData) continue;

                string key = !string.IsNullOrEmpty(u.id) ? u.id : u.name;
                if (loadedIds.Add(key))
                {
                    uniqueList.Add(u);
                }
            }

            if (uniqueList.Count != _allAvailableUpgrades.Count)
            {
                Debug.Log($"<color=#00FF88>[UpgradeManager]</color> Đã tự động loại bỏ {_allAvailableUpgrades.Count - uniqueList.Count} thẻ trùng lặp từ Scene Serialization. Còn lại: {uniqueList.Count} thẻ duy nhất.");
                _allAvailableUpgrades = uniqueList;
            }
        }

        private void InitDefaultFilters()
        {
            _filters.Clear();
            _filters.Add(new BannedUpgradeFilter(_bannedUpgrades));
            _filters.Add(new AvailabilityUpgradeFilter());
            _filters.Add(new ArchetypeExclusionFilter());
        }

        private void InitDefaultWeightPipeline()
        {
            _weightPipeline.ClearWeighters();
            _weightPipeline.RegisterWeighter(new ProjectZombie.Features.Upgrades.Weighting.ArchetypeSynergyWeighter());
            _weightPipeline.RegisterWeighter(new ProjectZombie.Features.Upgrades.Weighting.ElementSynergyWeighter());
            _weightPipeline.RegisterWeighter(new ProjectZombie.Features.Upgrades.Weighting.OwnedWeaponPriorityWeighter());
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
            _allAvailableUpgrades?.RemoveAll(u => u == null);

            if (_allAvailableUpgrades == null || _allAvailableUpgrades.Count == 0)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    PopulateAllAvailableUpgrades();
                    return;
                }
#endif
                if (_cachedMasterUpgrades != null && _cachedMasterUpgrades.Count > 0)
                {
                    _allAvailableUpgrades = new List<UpgradeData>(_cachedMasterUpgrades);
                    _allAvailableUpgrades.RemoveAll(u => u == null);
                    return;
                }

                // Nạp tức thời đồng bộ từ Resources để không bao giờ bị rỗng pool ở frame đầu
                var resUpgrades = Resources.LoadAll<UpgradeData>("Upgrades");
                if (resUpgrades != null && resUpgrades.Length > 0)
                {
                    _allAvailableUpgrades ??= new List<UpgradeData>();
                    var loadedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                    foreach (var u in resUpgrades)
                    {
                        if (u != null && !(u is FallbackRewardUpgradeData))
                        {
                            string idKey = !string.IsNullOrEmpty(u.id) ? u.id : u.name;
                            if (loadedIds.Add(idKey))
                            {
                                _allAvailableUpgrades.Add(u);
                            }
                        }
                    }
                    _cachedMasterUpgrades = new List<UpgradeData>(_allAvailableUpgrades);
                    return;
                }

                if (_loadingTask == null || _loadingTask.IsCompleted)
                {
                    _loadingTask = PopulateAllAvailableUpgradesAsync();
                }
            }
        }

        public async Task AutoPopulateUpgradesIfEmptyAsync()
        {
            _allAvailableUpgrades?.RemoveAll(u => u == null);

            if (_allAvailableUpgrades == null || _allAvailableUpgrades.Count == 0)
            {
                if (_cachedMasterUpgrades != null && _cachedMasterUpgrades.Count > 0)
                {
                    _allAvailableUpgrades = new List<UpgradeData>(_cachedMasterUpgrades);
                    _allAvailableUpgrades.RemoveAll(u => u == null);
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

#if UNITY_EDITOR
        [ContextMenu("Populate All Upgrades (Editor Tool Only)")]
        public void PopulateAllAvailableUpgrades()
        {
            _allAvailableUpgrades.Clear();
            _cachedMasterUpgrades = null;

            string[] searchFolders = new[] { "Assets/_Data/Upgrades" };
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpgradeData", searchFolders);
            var loadedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var upgrade = UnityEditor.AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                if (upgrade != null && !(upgrade is FallbackRewardUpgradeData))
                {
                    string idKey = !string.IsNullOrEmpty(upgrade.id) ? upgrade.id : upgrade.name;
                    if (loadedIds.Add(idKey))
                    {
                        _allAvailableUpgrades.Add(upgrade);
                    }
                }
            }

            _allAvailableUpgrades.Sort((a, b) => string.Compare(a.id, b.id, System.StringComparison.OrdinalIgnoreCase));
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"<color=#00FF88>[UpgradeManager]</color> Editor Tool: Đã nạp chính xác {_allAvailableUpgrades.Count} thẻ UpgradeData Master duy nhất.");
            _cachedMasterUpgrades = new List<UpgradeData>(_allAvailableUpgrades);
        }
#else
        public void PopulateAllAvailableUpgrades()
        {
            _ = PopulateAllAvailableUpgradesAsync();
        }
#endif

        public async Task PopulateAllAvailableUpgradesAsync()
        {
            _allAvailableUpgrades.Clear();

            if (_cachedMasterUpgrades != null && _cachedMasterUpgrades.Count > 0)
            {
                _allAvailableUpgrades.AddRange(_cachedMasterUpgrades);
                _allAvailableUpgrades.RemoveAll(u => u == null);
                return;
            }

            // 1. Nạp từ Resources/Upgrades (Fallback & Direct Runtime Loading)
            var resUpgrades = Resources.LoadAll<UpgradeData>("Upgrades");
            if (resUpgrades != null && resUpgrades.Length > 0)
            {
                var loadedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var u in resUpgrades)
                {
                    if (u != null && !(u is FallbackRewardUpgradeData))
                    {
                        string idKey = !string.IsNullOrEmpty(u.id) ? u.id : u.name;
                        if (loadedIds.Add(idKey))
                        {
                            _allAvailableUpgrades.Add(u);
                        }
                    }
                }
                Debug.Log($"<color=#00FF88>[UpgradeManager]</color> Đã nạp thành công {_allAvailableUpgrades.Count} thẻ từ Resources/Upgrades.");
            }

            // 2. Nạp bổ sung qua GameDataService nếu có
            if (ProjectZombie.Core.Services.Data.GameDataService.Instance != null)
            {
                var loadedList = await ProjectZombie.Core.Services.Data.GameDataService.Instance.LoadAllAsync<UpgradeData>("UpgradeData");
                if (loadedList != null && loadedList.Count > 0)
                {
                    foreach (var item in loadedList)
                    {
                        if (item != null && !(item is FallbackRewardUpgradeData) && !_allAvailableUpgrades.Contains(item))
                        {
                            _allAvailableUpgrades.Add(item);
                        }
                    }
                }
            }

#if UNITY_EDITOR
            if (_allAvailableUpgrades.Count == 0)
            {
                Debug.LogWarning("[UpgradeManager] Không tìm thấy UpgradeData trong Resources. Fallback sang Editor AssetDatabase.");
                PopulateAllAvailableUpgrades();
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

        [Header("Progression & Reroll Token")]
        [SerializeField] private int _defaultRerollTokensPerRun = 2;
        private int _currentRerollTokens = 2;

        public int CurrentRerollTokens => _currentRerollTokens;

        public void ResetRerollTokens()
        {
            _currentRerollTokens = _defaultRerollTokensPerRun;
        }

        public bool TryConsumeRerollToken()
        {
            if (_currentRerollTokens > 0)
            {
                _currentRerollTokens--;
                Debug.Log($"<color=#00FF88>[UpgradeManager]</color> Đã dùng 1 Reroll Token! Còn lại: {_currentRerollTokens}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra xem cấp độ hiện tại có phải là Mốc Đột Biến (Lv.5, Lv.15, Lv.30) hay không.
        /// </summary>
        public bool IsMutationLevel(int level)
        {
            return level == 5 || level == 15 || level == 30;
        }

        /// <summary>
        /// Trả về danh sách nâng cấp theo đúng Tiến trình Logarithmic 4 giai đoạn:
        /// - Lv.1: Đại Lõi Khởi Nguyên.
        /// - Lv.5, 15, 30: Lõi Đột Biến Bạc/Vàng/Kim Cương.
        /// - Level Thường: Thẻ Chỉ Số Nền Tảng (Clean Pool).
        /// </summary>
        public List<UpgradeData> GetProgressionUpgrades(int count, int playerLevel, Player.PlayerContext context)
        {
            AutoPopulateUpgradesIfEmpty();
            Debug.Log($"<color=#FFFF00>[DIAG_UPGRADE_MANAGER]</color> GetProgressionUpgrades(Lv.{playerLevel}, count={count}) - TotalAvailable: {_allAvailableUpgrades.Count}");
            return UpgradeSelector.SelectUpgradesByProgression(count, playerLevel, context, _allAvailableUpgrades, _fallbackRewards);
        }

        /// <summary>
        /// Trả về danh sách nâng cấp ngẫu nhiên qua thuật toán UpgradeSelector & PlayerContext.
        /// </summary>
        public List<UpgradeData> GetRandomUpgrades(int count, Player.PlayerContext context)
        {
            int currentLvl = context?.Experience != null ? context.Experience.CurrentLevel : 2;
            return GetProgressionUpgrades(count, currentLvl, context);
        }

        public List<UpgradeData> GetRandomUpgrades(int count, GameObject player)
        {
            var context = player != null ? new Player.PlayerContext(player) : null;
            return GetRandomUpgrades(count, context);
        }

        public List<UpgradeData> GetRandomUpgrades(int count)
        {
            var player = Player.PlayerProvider.HasPlayer ? Player.PlayerProvider.PlayerGameObject : null;
            return GetRandomUpgrades(count, player);
        }

        /// <summary>
        /// Trả về danh sách các Đại Lõi Thần Thoại khả dụng (dùng cho Level 1 khởi đầu trận đấu).
        /// </summary>
        public List<UpgradeData> GetMythicCoreChoices(int count, Player.PlayerContext context)
        {
            AutoPopulateUpgradesIfEmpty();
            return UpgradeSelector.SelectArchetypeCores(count, _allAvailableUpgrades);
        }
    }
}

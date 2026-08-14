using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Upgrades.Filters;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Quản lý danh sách tất cả các nâng cấp có thể có trong game (Pool).
    /// Cung cấp các lựa chọn ngẫu nhiên khi người chơi lên cấp thông qua hệ thống Filter Strategy Pattern.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Upgrade Pool")]
        [SerializeField] private List<UpgradeData> _allAvailableUpgrades = new List<UpgradeData>();

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
            AutoPopulateUpgradesIfEmpty();
        }

        private void InitDefaultFilters()
        {
            _filters.Clear();
            _filters.Add(new BannedUpgradeFilter(_bannedUpgrades));
            _filters.Add(new AvailabilityUpgradeFilter());
            _filters.Add(new YinYangUpgradeFilter());
        }

        /// <summary>
        /// Đăng ký thêm một bộ lọc nâng cấp tùy biến (Custom Filter).
        /// </summary>
        public void RegisterFilter(IUpgradeFilter filter)
        {
            if (filter != null && !_filters.Contains(filter))
            {
                _filters.Add(filter);
            }
        }

        /// <summary>
        /// Hủy đăng ký một bộ lọc nâng cấp.
        /// </summary>
        public void RemoveFilter(IUpgradeFilter filter)
        {
            if (filter != null)
            {
                _filters.Remove(filter);
            }
        }

        /// <summary>
        /// Tự động load tất cả thẻ UpgradeData sẵn có nếu danh sách trống.
        /// </summary>
        public void AutoPopulateUpgradesIfEmpty()
        {
            if (_allAvailableUpgrades == null || _allAvailableUpgrades.Count == 0)
            {
                PopulateAllAvailableUpgrades();
            }
        }

        /// <summary>
        /// Nạp tự động toàn bộ UpgradeData trong dự án (Resources hoặc AssetDatabase khi ở Editor).
        /// </summary>
        [ContextMenu("Populate All Upgrades")]
        public void PopulateAllAvailableUpgrades()
        {
            _allAvailableUpgrades.Clear();

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpgradeData");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var upgrade = UnityEditor.AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                if (upgrade != null && !_allAvailableUpgrades.Contains(upgrade))
                {
                    _allAvailableUpgrades.Add(upgrade);
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[UpgradeManager] Tự động nạp {_allAvailableUpgrades.Count} thẻ UpgradeData từ dự án.");
#else
            var loadedUpgrades = Resources.LoadAll<UpgradeData>("");
            _allAvailableUpgrades.AddRange(loadedUpgrades);
            Debug.Log($"[UpgradeManager] Load {_allAvailableUpgrades.Count} thẻ UpgradeData từ Resources.");
#endif
        }

        /// <summary>
        /// Cấm một thẻ nâng cấp xuất hiện trong suốt Run đấu hiện tại.
        /// </summary>
        public void BanUpgrade(UpgradeData upgrade)
        {
            if (upgrade != null && !_bannedUpgrades.Contains(upgrade))
            {
                _bannedUpgrades.Add(upgrade);
                Debug.Log($"[UpgradeManager] Banned upgrade: {upgrade.upgradeName}");
            }
        }

        /// <summary>
        /// Reset danh sách các thẻ bị cấm (khi khởi tạo Run đấu mới).
        /// </summary>
        public void ResetBannedUpgrades()
        {
            _bannedUpgrades.Clear();
        }

        /// <summary>
        /// Kiểm tra xem thẻ nâng cấp có đang bị cấm hay không.
        /// </summary>
        public bool IsBanned(UpgradeData upgrade)
        {
            return upgrade != null && _bannedUpgrades.Contains(upgrade);
        }

        /// <summary>
        /// Kiểm tra xem thẻ có thỏa mãn tất cả các bộ lọc đã đăng ký hay không.
        /// </summary>
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
        /// Trả về một danh sách các lựa chọn nâng cấp ngẫu nhiên.
        /// </summary>
        public List<UpgradeData> GetRandomUpgrades(int count, GameObject player)
        {
            var validUpgrades = new List<UpgradeData>();
            float totalWeight = 0f;

            // Lọc danh sách thẻ hợp lệ qua Filter Strategy
            for (int i = 0; i < _allAvailableUpgrades.Count; i++)
            {
                var u = _allAvailableUpgrades[i];
                if (IsUpgradeAllowed(u, player))
                {
                    validUpgrades.Add(u);
                    totalWeight += u.spawnWeight;
                }
            }

            List<UpgradeData> selectedUpgrades = new List<UpgradeData>();
            
            // Thuật toán Weighted Random đã được tối ưu
            while (selectedUpgrades.Count < count && validUpgrades.Count > 0)
            {
                float randomValue = Random.Range(0f, totalWeight);
                float currentSum = 0f;

                for (int i = 0; i < validUpgrades.Count; i++)
                {
                    currentSum += validUpgrades[i].spawnWeight;
                    if (currentSum >= randomValue)
                    {
                        var chosen = validUpgrades[i];
                        selectedUpgrades.Add(chosen);
                        
                        totalWeight -= chosen.spawnWeight; // Trừ weight thay vì gọi hàm Sum() liên tục
                        
                        // Swap and Pop: Cách xóa phần tử List nhanh nhất (O(1)) không làm dịch mảng
                        validUpgrades[i] = validUpgrades[validUpgrades.Count - 1]; 
                        validUpgrades.RemoveAt(validUpgrades.Count - 1);
                        
                        break;
                    }
                }
            }

            return selectedUpgrades;
        }
    }
}

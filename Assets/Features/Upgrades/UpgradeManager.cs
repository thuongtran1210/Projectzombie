using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Quản lý danh sách tất cả các nâng cấp có thể có trong game (Pool).
    /// Cung cấp các lựa chọn ngẫu nhiên khi người chơi lên cấp.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Upgrade Pool")]
        [SerializeField] private List<UpgradeData> _allAvailableUpgrades = new List<UpgradeData>();

        private readonly HashSet<UpgradeData> _bannedUpgrades = new HashSet<UpgradeData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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
        /// Trả về một danh sách các lựa chọn nâng cấp ngẫu nhiên.
        /// </summary>
        public List<UpgradeData> GetRandomUpgrades(int count, GameObject player)
        {
            var validUpgrades = new List<UpgradeData>();
            float totalWeight = 0f;

            // Thay thế LINQ Where bằng vòng lặp để tránh tạo rác (Garbage Collection)
            foreach (var u in _allAvailableUpgrades)
            {
                if (u != null && !_bannedUpgrades.Contains(u) && u.IsAvailable(player))
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

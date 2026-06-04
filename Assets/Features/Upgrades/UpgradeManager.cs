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
        [SerializeField] private List<UpgradeData> allAvailableUpgrades = new List<UpgradeData>();

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
        /// Trả về một danh sách các lựa chọn nâng cấp ngẫu nhiên.
        /// </summary>
        public List<UpgradeData> GetRandomUpgrades(int count, GameObject player)
        {
            var validUpgrades = new List<UpgradeData>();
            float totalWeight = 0f;

            // Thay thế LINQ Where bằng vòng lặp để tránh tạo rác (Garbage Collection)
            foreach (var u in allAvailableUpgrades)
            {
                if (u.IsAvailable(player))
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

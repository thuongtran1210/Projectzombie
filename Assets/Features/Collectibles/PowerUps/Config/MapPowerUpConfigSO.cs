using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Collectibles.PowerUps.Config
{
    /// <summary>
    /// Định nghĩa thông số của một loại vật phẩm xuất hiện trong sự kiện bản đồ.
    /// </summary>
    [System.Serializable]
    public class MapPowerUpEntry
    {
        [Tooltip("Tên gợi nhớ vật phẩm (VD: Tiên Đan Hồi Phục)")]
        public string itemName = "PowerUp";

        [Tooltip("Prefab của vật phẩm (Chứa Component PowerUpItem)")]
        public GameObject itemPrefab;

        [Tooltip("Trọng số ngẫu nhiên (Weight) khi lựa chọn spawn. Càng cao càng dễ xuất hiện.")]
        [Range(1, 100)]
        public int weight = 50;

        [Tooltip("Thời gian tồn tại trên mặt đất trước khi tự động biến mất (Giây)")]
        [Range(10f, 120f)]
        public float lifeTime = 35f;
    }

    /// <summary>
    /// ScriptableObject cấu hình quy tắc sinh vật phẩm ngẫu nhiên trên bản đồ theo thời gian trận đấu.
    /// </summary>
    [CreateAssetMenu(fileName = "MapPowerUpConfig", menuName = "ProjectZombie/Collectibles/PowerUps/MapPowerUpConfig")]
    public class MapPowerUpConfigSO : ScriptableObject
    {
        [Header("Spawn Interval (Giây)")]
        [Tooltip("Thời gian tối thiểu giữa 2 lần kích hoạt sự kiện sinh vật phẩm")]
        [Range(15f, 180f)]
        public float minIntervalSeconds = 40f;

        [Tooltip("Thời gian tối đa giữa 2 lần kích hoạt sự kiện sinh vật phẩm")]
        [Range(20f, 300f)]
        public float maxIntervalSeconds = 70f;

        [Header("Spawn Placement")]
        [Tooltip("Bán kính tối thiểu so với vị trí Player")]
        [Range(4f, 20f)]
        public float minSpawnRadius = 6f;

        [Tooltip("Bán kính tối đa so với vị trí Player")]
        [Range(8f, 35f)]
        public float maxSpawnRadius = 13f;

        [Header("Limits")]
        [Tooltip("Số lượng vật phẩm bổ trợ tối đa cùng tồn tại trên sàn đấu")]
        [Range(1, 10)]
        public int maxActiveItemsOnGround = 3;

        [Header("Items Pool")]
        [Tooltip("Danh sách các vật phẩm có thể sinh ra và trọng số")]
        public List<MapPowerUpEntry> entries = new List<MapPowerUpEntry>();

        /// <summary>
        /// Thuật toán Roulette Wheel Selection để chọn ngẫu nhiên 1 entry theo trọng số (0 GC).
        /// </summary>
        public MapPowerUpEntry SelectRandomEntry()
        {
            if (entries == null || entries.Count == 0) return null;

            int totalWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].itemPrefab != null)
                {
                    totalWeight += entries[i].weight;
                }
            }

            if (totalWeight <= 0) return null;

            int randomValue = Random.Range(0, totalWeight);
            int currentSum = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.itemPrefab != null)
                {
                    currentSum += entry.weight;
                    if (randomValue < currentSum)
                    {
                        return entry;
                    }
                }
            }

            return entries[0];
        }
    }
}

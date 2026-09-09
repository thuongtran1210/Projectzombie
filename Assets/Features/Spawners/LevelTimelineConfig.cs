using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Spawners
{
    public enum TimelineEventType
    {
        Continuous, // Quái nền xuất hiện liên tục
        BurstWave,  // Bầy quái xuất hiện tức thì bao vây
        SpawnPillar,// Rơi trụ nhả quái
        BossSpawn   // Boss xuất hiện (quét sạch quái nhỏ xung quanh)
    }

    [System.Serializable]
    public class TimelineEvent
    {
        [Tooltip("Mô tả sự kiện (Ví dụ: Wave 1 - Quỷ Xương xuất hiện).")]
        public string eventName = "Wave Event";

        [Tooltip("Mốc thời gian kích hoạt tính từ đầu trận (tính bằng giây, ví dụ: 60s, 600s).")]
        public float timestampSeconds = 0f;

        [Tooltip("Loại sự kiện spawn.")]
        public TimelineEventType eventType = TimelineEventType.Continuous;

        [Tooltip("Prefab quái / Boss / Trụ sẽ spawn (Direct reference fallback).")]
        public GameObject spawnPrefab;

        [Tooltip("Địa chỉ Addressable của Prefab quái (Ví dụ: Enemies/Zombie_Walker). Dùng để load bất đồng bộ tối ưu RAM.")]
        public string enemyAddress;

        [Tooltip("AssetReferenceGameObject của Addressable (nếu cấu hình từ Inspector).")]
        public UnityEngine.AddressableAssets.AssetReferenceGameObject spawnPrefabRef;

        [Tooltip("Số lượng quái tối đa hoặc số quái spawn trong đợt burst.")]
        public int spawnCount = 10;

        [Tooltip("Khoảng thời gian giữa các lần spawn (dành cho Continuous/Pillar).")]
        public float spawnInterval = 2f;

        [Tooltip("Icon đại diện quái / sự kiện hiển thị trên thanh tiến trình Timeline (Tùy chọn, nếu để trống sẽ tự động lấy từ Prefab).")]
        public Sprite eventIcon;

        /// <summary>
        /// Lấy Key định danh duy nhất cho Object Pool (Ưu tiên Addressable enemyAddress, fallback lấy tên spawnPrefab).
        /// </summary>
        public string GetPoolKey()
        {
            if (!string.IsNullOrEmpty(enemyAddress)) return enemyAddress;
            if (spawnPrefab != null) return spawnPrefab.name;
            return string.Empty;
        }

        /// <summary>
        /// Lấy Prefab quái vật an toàn (nếu spawnPrefab bị null thì tự động fallback load theo enemyAddress / Resources / AssetDatabase).
        /// </summary>
        public GameObject GetPrefabOrLoad()
        {
            if (spawnPrefab != null) return spawnPrefab;

            if (!string.IsNullOrEmpty(enemyAddress))
            {
                spawnPrefab = Resources.Load<GameObject>($"Enemies/{enemyAddress}") ??
                              Resources.Load<GameObject>(enemyAddress);

#if UNITY_EDITOR
                if (spawnPrefab == null)
                {
                    spawnPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Prefabs/Characters/Enemies/{enemyAddress}.prefab");
                }
#endif
            }
            return spawnPrefab;
        }

        /// <summary>
        /// Lấy Sprite icon đại diện cho sự kiện (ưu tiên eventIcon, sau đó đến SpriteRenderer từ spawnPrefab).
        /// </summary>
        public Sprite GetIcon()
        {
            if (eventIcon != null) return eventIcon;
            var prefab = GetPrefabOrLoad();
            if (prefab != null)
            {
                var sr = prefab.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null) return sr.sprite;
            }
            return null;
        }
    }

    [CreateAssetMenu(fileName = "NewLevelTimeline", menuName = "ProjectZombie/Level Timeline Config")]
    public class LevelTimelineConfig : ScriptableObject
    {
        [Header("Level Information")]
        public string levelName = "Màn 1: U Minh Giới";
        public float maxLevelDuration = 900f; // 15 phút (900 giây)

        [Header("Timeline Events")]
        [Tooltip("Danh sách các sự kiện spawn xếp theo thời gian từ 0s -> 900s.")]
        public List<TimelineEvent> events = new List<TimelineEvent>();

        private void OnValidate()
        {
            if (events != null && events.Count > 1)
            {
                // Sắp xếp tự động theo mốc thời gian tăng dần trong Inspector
                events.Sort((a, b) => a.timestampSeconds.CompareTo(b.timestampSeconds));
            }
        }
    }
}

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
        [Tooltip("Tên gợi nhớ đợt sóng (VD: Phút 03:00 - Bầy Ma Da tràn lên bao vây). Hiển thị trên log và cảnh báo UI.")]
        public string eventName = "Wave Event";

        [Tooltip("Thời điểm kích hoạt tính bằng GIÂY từ lúc bắt đầu trận (VD: 180s = Phút 03:00).")]
        public float timestampSeconds = 0f;

        [Tooltip("Kiểu sinh quái: Continuous (Rải rác liên tục), BurstWave (Dồn dập dồn ép), BossSpawn (Trùm xuất hiện), hoặc SpecialPillar (Trụ kỹ năng).")]
        public TimelineEventType eventType = TimelineEventType.Continuous;

        [Tooltip("[Dự phòng/Test Editor] Kéo thả trực tiếp Prefab quái vào đây. Dùng làm phương án dự phòng khi chạy Test nhanh trên Editor.")]
        public GameObject spawnPrefab;

        [Tooltip("[Khuyên dùng Android] Mã ID / Key Addressables (VD: E_MADA). Giúp nạp ngầm bất đồng bộ trước trận đấu để tối ưu RAM và tránh giật lag.")]
        public string enemyAddress;

        [Tooltip("[Addressables Inspector] Tham chiếu kéo thả AssetReference (Tùy chọn, ưu tiên sử dụng enemyAddress bên trên).")]
        public UnityEngine.AddressableAssets.AssetReferenceGameObject spawnPrefabRef;

        [Tooltip("Tổng số lượng quái sẽ được sinh ra trong đợt sóng này.")]
        public int spawnCount = 10;

        [Tooltip("Khoảng thời gian giãn cách giữa mỗi lần sinh 1 con quái (tính bằng Giây). VD: 0.1s = sinh dồn dập 10 con trong 1 giây.")]
        public float spawnInterval = 2f;

        [Tooltip("Icon đại diện hiển thị trên thanh Timeline / Cảnh báo UI (Nếu để trống sẽ tự động lấy Icon từ Prefab quái).")]
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

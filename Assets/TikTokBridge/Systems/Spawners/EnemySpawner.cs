using UnityEngine;
using TikTokBridge.Logic;
using TikTokBridge.Models;
using TikTokBridge.Core;
using System.Collections.Generic;

namespace TikTokBridge.Systems.Spawners
{
    [System.Serializable]
    public struct GiftEnemyMapping
    {
        public string giftName;
        public GameObject enemyPrefab;
    }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Default Events Prefabs")]
        [SerializeField] private GameObject slimePrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject elitePrefab;

        [Header("Gift Mappings")]
        [SerializeField] private List<GiftEnemyMapping> giftMappings = new List<GiftEnemyMapping>();

        [Header("Spawn Settings")]
        [SerializeField] private float minSpawnRadius = 10f;
        [SerializeField] private float maxSpawnRadius = 15f;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Performance Settings")]
        [SerializeField] private int maxSpawnsPerFrame = 5;
        [SerializeField] private float spawnDelayBetweenFrames = 0.05f;

        private ICommandDispatcher _dispatcher;
        private Dictionary<string, GameObject> _giftToEnemyMap = new Dictionary<string, GameObject>();
        private Transform _playerTransform;
        private Queue<GameObject> _spawnQueue = new Queue<GameObject>();
        private Coroutine _spawnCoroutine;

        private void Awake()
        {
            foreach (var mapping in giftMappings)
            {
                if (!string.IsNullOrEmpty(mapping.giftName) && mapping.enemyPrefab != null)
                {
                    _giftToEnemyMap[mapping.giftName] = mapping.enemyPrefab;
                }
            }
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }

            // Khởi tạo sẵn quái vật từ quà tặng để không bị giật lúc livestream
            if (EnemyPoolManager.Instance != null)
            {
                if (slimePrefab != null) EnemyPoolManager.Instance.PrewarmPool(slimePrefab, 20);
                if (archerPrefab != null) EnemyPoolManager.Instance.PrewarmPool(archerPrefab, 20);
                if (elitePrefab != null) EnemyPoolManager.Instance.PrewarmPool(elitePrefab, 20);

                foreach (var mapping in giftMappings)
                {
                    if (mapping.enemyPrefab != null)
                    {
                        EnemyPoolManager.Instance.PrewarmPool(mapping.enemyPrefab, 20);
                    }
                }
            }
        }

        public void Construct(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.OnLikeReceived += HandleLike;
            _dispatcher.OnFollowReceived += HandleFollow;
            _dispatcher.OnSpawnEnemy += HandleSpawnEnemy;
        }

        private void HandleLike(GameCommandPayload cmd)
        {
            Debug.Log($"[EnemySpawner] {cmd.user} liked! Spawning Slime.");
            EnqueueSpawns(slimePrefab, 1);
        }

        private void HandleFollow(GameCommandPayload cmd)
        {
            Debug.Log($"[EnemySpawner] {cmd.user} followed! Spawning Archer.");
            EnqueueSpawns(archerPrefab, 1);
        }

        private void HandleSpawnEnemy(GameCommandPayload cmd)
        {
            string enemyType = cmd.enemy; 
            if (string.IsNullOrEmpty(enemyType) && cmd.additionalData != null)
            {
                enemyType = cmd.additionalData["enemy"]?.ToString();
            }

            int amount = cmd.amount > 0 ? cmd.amount : 1;

            if (string.IsNullOrEmpty(enemyType))
            {
                Debug.Log($"[EnemySpawner] {cmd.user} sent Gift but enemyType is null. Spawning {amount} Elite!");
                EnqueueSpawns(elitePrefab, amount);
                return;
            }

            if (_giftToEnemyMap.TryGetValue(enemyType, out GameObject prefabToSpawn))
            {
                Debug.Log($"[EnemySpawner] {cmd.user} sent Gift! Spawning {amount} {enemyType}!");
                EnqueueSpawns(prefabToSpawn, amount);
            }
            else
            {
                Debug.Log($"[EnemySpawner] Unknown gift '{enemyType}' from {cmd.user}. Spawning {amount} Elite as fallback!");
                EnqueueSpawns(elitePrefab, amount);
            }
        }

        private void EnqueueSpawns(GameObject prefab, int amount)
        {
            if (prefab == null) return;
            
            for (int i = 0; i < amount; i++)
            {
                _spawnQueue.Enqueue(prefab);
            }

            if (_spawnCoroutine == null)
            {
                _spawnCoroutine = StartCoroutine(ProcessSpawnQueue());
            }
        }

        private System.Collections.IEnumerator ProcessSpawnQueue()
        {
            while (_spawnQueue.Count > 0)
            {
                int spawnCount = Mathf.Min(maxSpawnsPerFrame, _spawnQueue.Count);
                for (int i = 0; i < spawnCount; i++)
                {
                    GameObject prefabToSpawn = _spawnQueue.Dequeue();
                    SpawnEnemy(prefabToSpawn);
                }

                if (spawnDelayBetweenFrames > 0)
                {
                    yield return new WaitForSeconds(spawnDelayBetweenFrames);
                }
                else
                {
                    yield return null;
                }
            }
            
            _spawnCoroutine = null;
        }

        private void SpawnEnemy(GameObject prefab)
        {
            if (prefab == null) return;
            
            if (EnemyPoolManager.Instance != null)
            {
                EnemyPoolManager.Instance.SpawnEnemy(prefab, GetSpawnPosition(), Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] EnemyPoolManager.Instance is null! Please add it to the scene.");
            }
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 center = _playerTransform != null ? _playerTransform.position : Vector3.zero;

            // Thử tối đa 10 lần để tìm vị trí không bị kẹt vào tường
            for (int i = 0; i < 10; i++)
            {
                // Chọn góc ngẫu nhiên từ 0 đến 360 độ
                float angle = Random.Range(0f, 360f);
                // Chọn khoảng cách ngẫu nhiên từ min đến max
                float radius = Random.Range(minSpawnRadius, maxSpawnRadius);

                // Tính toán hướng theo trục X và Y
                Vector2 randomDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPos = (Vector2)center + randomDir * radius;

                // Kiểm tra xem vị trí có trống không nếu có cấu hình obstacleLayer
                if (obstacleLayer.value != 0)
                {
                    Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.5f, obstacleLayer);
                    if (hit != null) continue; // Bị kẹt, thử vị trí khác
                }

                return spawnPos; // Trả về nếu vị trí hợp lệ
            }

            // Fallback: Nếu không tìm được chỗ trống, cứ spawn đại quanh rìa min
            float fallbackAngle = Random.Range(0f, 360f);
            Vector2 fallbackDir = new Vector2(Mathf.Cos(fallbackAngle * Mathf.Deg2Rad), Mathf.Sin(fallbackAngle * Mathf.Deg2Rad));
            return (Vector2)center + fallbackDir * minSpawnRadius;
        }

        private void OnDestroy()
        {
            if (_dispatcher != null)
            {
                _dispatcher.OnLikeReceived -= HandleLike;
                _dispatcher.OnFollowReceived -= HandleFollow;
                _dispatcher.OnSpawnEnemy -= HandleSpawnEnemy;
            }
        }
    }
}
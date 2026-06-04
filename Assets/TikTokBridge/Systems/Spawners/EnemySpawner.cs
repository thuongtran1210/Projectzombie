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

        private ICommandDispatcher _dispatcher;
        private Dictionary<string, GameObject> _giftToEnemyMap = new Dictionary<string, GameObject>();

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
            SpawnEnemy(slimePrefab);
        }

        private void HandleFollow(GameCommandPayload cmd)
        {
            Debug.Log($"[EnemySpawner] {cmd.user} followed! Spawning Archer.");
            SpawnEnemy(archerPrefab);
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
                for (int i = 0; i < amount; i++) SpawnEnemy(elitePrefab);
                return;
            }

            if (_giftToEnemyMap.TryGetValue(enemyType, out GameObject prefabToSpawn))
            {
                Debug.Log($"[EnemySpawner] {cmd.user} sent Gift! Spawning {amount} {enemyType}!");
                for (int i = 0; i < amount; i++)
                {
                    SpawnEnemy(prefabToSpawn);
                }
            }
            else
            {
                Debug.Log($"[EnemySpawner] Unknown gift '{enemyType}' from {cmd.user}. Spawning {amount} Elite as fallback!");
                for (int i = 0; i < amount; i++)
                {
                    SpawnEnemy(elitePrefab);
                }
            }
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
            return new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
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
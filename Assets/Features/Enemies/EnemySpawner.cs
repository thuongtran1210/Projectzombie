using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Core.ScriptableObjects;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Quản lý toàn bộ nhịp độ spawn kẻ địch trong một trận đấu.
    /// Theo dõi thời gian, kích hoạt các Wave theo timeline, và áp dụng
    /// Difficulty Scaling (tăng HP + spawn rate theo thời gian sống sót).
    /// 
    /// HƯỚNG DẪN SỬ DỤNG:
    /// 1. Gắn script này vào một GameObject trong scene.
    /// 2. Tạo các SpawnWaveConfig SO và kéo vào danh sách "waveConfigs".
    /// 3. Thiết lập "continuousSpawnPrefabs" cho các đợt spawn liên tục nền.
    /// 4. Gọi StartSpawning() để bắt đầu (hoặc để Spawner tự chạy khi game sẵn sàng).
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        // ====================================================================
        // INSPECTOR FIELDS
        // ====================================================================

        [Header("Wave Configuration")]
        [Tooltip("Danh sách các đợt spawn theo timeline. Sắp xếp theo triggerTimeSeconds tăng dần.")]
        [SerializeField] private List<SpawnWaveConfig> waveConfigs = new List<SpawnWaveConfig>();

        [Header("Continuous Background Spawn")]
        [Tooltip("Các prefab kẻ địch thường (Common) spawn liên tục trong nền.")]
        [SerializeField] private List<GameObject> continuousSpawnPrefabs = new List<GameObject>();

        [Tooltip("Khoảng cách giữa mỗi lần spawn nền (giây). Giảm dần theo Difficulty Scaling.")]
        [SerializeField] private float baseSpawnInterval = 2f;

        [Tooltip("Số lượng kẻ địch tối đa được phép tồn tại cùng lúc (Enemy Cap).")]
        [SerializeField] private int maxEnemyCount = 200;

        [Header("Difficulty Scaling")]
        [Tooltip("% tăng HP kẻ địch mỗi phút (+5% = 0.05).")]
        [SerializeField] private float hpScalePerMinute = 0.05f;

        [Tooltip("% giảm spawn interval mỗi phút (tăng tốc độ spawn).")]
        [SerializeField] private float spawnRateScalePerMinute = 0.08f;

        [Header("Spawn Position")]
        [Tooltip("Bán kính spawn so với Camera (enemy xuất hiện ngoài tầm nhìn).")]
        [SerializeField] private float spawnRadius = 15f;

        [Tooltip("Khoảng cách buffer tối thiểu để chắc chắn ngoài màn hình.")]
        [SerializeField] private float spawnRadiusBuffer = 2f;

        // ====================================================================
        // PRIVATE STATE
        // ====================================================================

        private float _elapsedTime = 0f;
        private int _nextWaveIndex = 0;
        private bool _isSpawning = false;
        private Camera _mainCamera;
        private int _currentEnemyCount = 0;

        // ====================================================================
        // UNITY LIFECYCLE
        // ====================================================================

        private void Awake()
        {
            _mainCamera = Camera.main;

            // Sắp xếp wave theo thời gian trigger
            waveConfigs.Sort((a, b) => a.triggerTimeSeconds.CompareTo(b.triggerTimeSeconds));
        }

        private void Start()
        {
            StartSpawning();
        }

        private void Update()
        {
            if (!_isSpawning) return;

            _elapsedTime += Time.deltaTime;

            // Kiểm tra trigger wave theo timeline
            while (_nextWaveIndex < waveConfigs.Count &&
                   _elapsedTime >= waveConfigs[_nextWaveIndex].triggerTimeSeconds)
            {
                StartCoroutine(SpawnWave(waveConfigs[_nextWaveIndex]));
                _nextWaveIndex++;
            }
        }

        // ====================================================================
        // PUBLIC API
        // ====================================================================

        public void StartSpawning()
        {
            if (_isSpawning) return;
            _isSpawning = true;
            _elapsedTime = 0f;
            _nextWaveIndex = 0;

            if (continuousSpawnPrefabs.Count > 0)
            {
                StartCoroutine(ContinuousSpawnRoutine());
            }

            Debug.Log("[EnemySpawner] Bắt đầu spawn kẻ địch.");
        }

        public void StopSpawning()
        {
            _isSpawning = false;
            StopAllCoroutines();
            Debug.Log("[EnemySpawner] Đã dừng spawn.");
        }

        /// <summary>Gọi bởi Enemy khi bị tiêu diệt để cập nhật Enemy Count.</summary>
        public void OnEnemyDied()
        {
            _currentEnemyCount = Mathf.Max(0, _currentEnemyCount - 1);
        }

        // ====================================================================
        // SPAWN LOGIC
        // ====================================================================

        /// <summary>Spawn một đợt kẻ địch theo cấu hình wave.</summary>
        private IEnumerator SpawnWave(SpawnWaveConfig wave)
        {
            if (wave.isBossWave)
            {
                Debug.Log($"[EnemySpawner] ⚠️ BOSS WAVE xuất hiện tại {FormatTime(_elapsedTime)}!");
            }
            else if (wave.isEliteWave)
            {
                Debug.Log($"[EnemySpawner] ⭐ Elite Wave xuất hiện tại {FormatTime(_elapsedTime)}!");
            }

            // Tính số lượng spawn theo Difficulty Scaling
            float minutesElapsed = _elapsedTime / 60f;
            float hpMultiplier = wave.hpMultiplierOverride > 0f
                ? wave.hpMultiplierOverride
                : 1f + (hpScalePerMinute * minutesElapsed);

            int spawnCount = wave.isBossWave ? 1 : wave.baseSpawnCount;

            for (int i = 0; i < spawnCount; i++)
            {
                if (!_isSpawning) yield break;

                // Kiểm tra Enemy Cap
                if (_currentEnemyCount >= maxEnemyCount)
                {
                    yield return new WaitForSeconds(0.5f);
                    i--; // Thử lại
                    continue;
                }

                // Chọn prefab ngẫu nhiên từ wave
                if (wave.enemyPrefabs == null || wave.enemyPrefabs.Length == 0) yield break;
                GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];

                SpawnEnemy(prefab, hpMultiplier);

                if (wave.spawnInterval > 0f)
                    yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        /// <summary>Coroutine spawn nền liên tục (kẻ địch thường).</summary>
        private IEnumerator ContinuousSpawnRoutine()
        {
            while (_isSpawning)
            {
                if (_currentEnemyCount < maxEnemyCount && continuousSpawnPrefabs.Count > 0)
                {
                    float minutesElapsed = _elapsedTime / 60f;
                    float hpMultiplier = 1f + (hpScalePerMinute * minutesElapsed);

                    GameObject prefab = continuousSpawnPrefabs[Random.Range(0, continuousSpawnPrefabs.Count)];
                    SpawnEnemy(prefab, hpMultiplier);
                }

                // Spawn interval giảm dần theo thời gian (tăng số lượng kẻ địch)
                float minutesElapsed2 = _elapsedTime / 60f;
                float currentInterval = Mathf.Max(0.3f, baseSpawnInterval * (1f - spawnRateScalePerMinute * minutesElapsed2));
                yield return new WaitForSeconds(currentInterval);
            }
        }

        /// <summary>Sinh ra một kẻ địch tại vị trí ngẫu nhiên ngoài màn hình camera (kết hợp Object Pooling 0 GC).</summary>
        private void SpawnEnemy(GameObject prefab, float hpMultiplier = 1f)
        {
            if (prefab == null) return;

            Vector3 spawnPos = GetSpawnPositionOffscreen();
            GameObject enemyObj = null;

            // Sử dụng Object Pooling 0 GC nếu EnemyPoolManager tồn tại
            if (Spawners.EnemyPoolManager.Instance != null)
            {
                var pool = Spawners.EnemyPoolManager.Instance.GetOrCreatePool(prefab);
                if (pool != null)
                {
                    enemyObj = pool.Get();
                    enemyObj.transform.position = spawnPos;
                    enemyObj.transform.rotation = Quaternion.identity;
                }
            }

            // Fallback Instantiate nếu chưa có Pool Manager trong scene
            if (enemyObj == null)
            {
                enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            }

            // Áp dụng HP scaling
            var healthSystem = enemyObj.GetComponent<ProjectZombie.Features.Shared.HealthSystem>();
            if (healthSystem != null)
            {
                if (hpMultiplier > 1f)
                {
                    healthSystem.ScaleMaxHealth(hpMultiplier);
                }

                // Đăng ký event chết để giảm enemy count (un-subscribe cũ trước để không duplicate)
                healthSystem.OnDied -= HandleEnemyDied;
                healthSystem.OnDied += HandleEnemyDied;
            }

            _currentEnemyCount++;
        }

        private void HandleEnemyDied()
        {
            OnEnemyDied();
        }

        /// <summary>Tính toán vị trí spawn ngẫu nhiên ngoài tầm nhìn camera.</summary>
        private Vector3 GetSpawnPositionOffscreen()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = spawnRadius + spawnRadiusBuffer;

            Vector3 camPos = _mainCamera != null ? _mainCamera.transform.position : Vector3.zero;
            return new Vector3(
                camPos.x + Mathf.Cos(angle) * distance,
                camPos.y + Mathf.Sin(angle) * distance,
                0f
            );
        }

        private string FormatTime(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            return $"{m:00}:{s:00}";
        }

        // ====================================================================
        // GIZMOS
        // ====================================================================

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius + spawnRadiusBuffer);
        }
#endif
    }
}

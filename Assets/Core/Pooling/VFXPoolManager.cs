using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Core.Pooling
{
    /// <summary>
    /// Component tự động thu hồi VFX về Pool sau khi hết thời gian sống.
    /// </summary>
    public class PooledVFXInstance : MonoBehaviour
    {
        private GameObject _originalPrefab;
        private Coroutine _autoReleaseRoutine;

        public void StartAutoRelease(GameObject originalPrefab, float duration)
        {
            _originalPrefab = originalPrefab;
            if (_autoReleaseRoutine != null)
            {
                StopCoroutine(_autoReleaseRoutine);
            }
            _autoReleaseRoutine = StartCoroutine(AutoReleaseTimer(duration));
        }

        private IEnumerator AutoReleaseTimer(float duration)
        {
            yield return new WaitForSeconds(duration);
            _autoReleaseRoutine = null;
            VFXPoolManager.ReleaseVFX(_originalPrefab, gameObject);
        }

        private void OnDisable()
        {
            if (_autoReleaseRoutine != null)
            {
                StopCoroutine(_autoReleaseRoutine);
                _autoReleaseRoutine = null;
            }
        }
    }

    /// <summary>
    /// Hệ thống quản lý Object Pool tập trung cho Particle Systems, Tia lửa va chạm (HitSparks) và Vệt chém VFX.
    /// Triển khai Zero-GC, tự động tái sử dụng các hiệu ứng ngắn hạn thay vì gọi Instantiate/Destroy liên tục.
    /// </summary>
    public class VFXPoolManager : MonoBehaviour
    {
        private static VFXPoolManager _instance;
        public static VFXPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[VFXPoolManager]");
                    _instance = go.AddComponent<VFXPoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private readonly Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Sinh hiệu ứng VFX từ Pool. Tự động kích hoạt ParticleSystem và tự thu hồi về Pool sau `duration` giây.
        /// </summary>
        public static GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float duration = 0.5f, int weaponLevel = 1)
        {
            if (prefab == null) return null;
            return Instance.InternalSpawnVFX(prefab, position, rotation, duration, weaponLevel);
        }

        private GameObject InternalSpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float duration, int weaponLevel = 1)
        {
            if (!_poolDictionary.TryGetValue(prefab, out var poolQueue))
            {
                poolQueue = new Queue<GameObject>();
                _poolDictionary[prefab] = poolQueue;
            }

            GameObject instance = null;
            while (poolQueue.Count > 0 && instance == null)
            {
                instance = poolQueue.Dequeue();
            }

            if (instance == null)
            {
                instance = Instantiate(prefab, transform);
                if (instance.GetComponent<PooledVFXInstance>() == null)
                {
                    instance.AddComponent<PooledVFXInstance>();
                }
            }

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            // Tự động phân cấp hiệu ứng VFX theo cấp độ vũ khí (Lv1-Lv5)
            if (instance.TryGetComponent<ProjectZombie.Features.Shared.VFX.VFXLevelScaler>(out var levelScaler))
            {
                levelScaler.ApplyLevelScaling(weaponLevel);
            }

            // Khởi động lại các ParticleSystem nếu có
            var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Clear();
                particleSystems[i].Play();
            }

            // Đặt lịch tự động trả về Pool
            var poolHelper = instance.GetComponent<PooledVFXInstance>();
            poolHelper.StartAutoRelease(prefab, duration);

            return instance;
        }

        /// <summary>
        /// Thu hồi VFX về lại Pool tương ứng.
        /// </summary>
        public static void ReleaseVFX(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null) return;
            if (_instance != null)
            {
                _instance.InternalReleaseVFX(prefab, instance);
            }
            else
            {
                Destroy(instance);
            }
        }

        private void InternalReleaseVFX(GameObject prefab, GameObject instance)
        {
            if (!_poolDictionary.TryGetValue(prefab, out var poolQueue))
            {
                poolQueue = new Queue<GameObject>();
                _poolDictionary[prefab] = poolQueue;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            poolQueue.Enqueue(instance);
        }

        /// <summary>
        /// Xóa sạch mọi pool khi chuyển đổi màn chơi lớn nếu cần.
        /// </summary>
        public static void ClearPools()
        {
            if (_instance != null)
            {
                foreach (var kvp in _instance._poolDictionary)
                {
                    while (kvp.Value.Count > 0)
                    {
                        var obj = kvp.Value.Dequeue();
                        if (obj != null) Destroy(obj);
                    }
                }
                _instance._poolDictionary.Clear();
            }
        }
    }
}

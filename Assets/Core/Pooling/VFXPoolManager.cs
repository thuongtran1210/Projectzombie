using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared.VFX;

namespace ProjectZombie.Core.Pooling
{
    /// <summary>
    /// Component tự động thu hồi VFX về Pool sau khi hết thời gian sống.
    /// Duy trì để tương thích ngược với các script hoặc asset cũ.
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
    /// Đã được hợp nhất về GlobalVFXPoolManager để đảm bảo vòng đời thống nhất và ngăn chặn rò rỉ bộ nhớ.
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
                    Transform parent = PoolHierarchyManager.Instance != null 
                        ? PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.VFX) 
                        : null;

                    GameObject go = new GameObject("[VFXPoolManager]");
                    if (parent != null)
                    {
                        go.transform.SetParent(parent);
                    }
                    _instance = go.AddComponent<VFXPoolManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null && PoolHierarchyManager.Instance != null)
                {
                    transform.SetParent(PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.VFX));
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Sinh hiệu ứng VFX từ Pool. Chuyển tiếp (delegate) tới GlobalVFXPoolManager.
        /// </summary>
        public static GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float duration = 0.5f, int weaponLevel = 1)
        {
            if (prefab == null) return null;

            if (GlobalVFXPoolManager.Instance != null)
            {
                return GlobalVFXPoolManager.Instance.PlayEffect(prefab, position, rotation, duration, null, weaponLevel);
            }

            // Fallback nếu GlobalVFXPoolManager chưa khởi tạo
            var instance = Instantiate(prefab, position, rotation);
            if (instance.TryGetComponent<VFXLevelScaler>(out var levelScaler))
            {
                levelScaler.ApplyLevelScaling(weaponLevel);
            }
            Destroy(instance, duration);
            return instance;
        }

        /// <summary>
        /// Thu hồi VFX về lại Pool tương ứng.
        /// </summary>
        public static void ReleaseVFX(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null) return;
            if (GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.ReleaseEffect(instance);
            }
            else
            {
                Destroy(instance);
            }
        }

        /// <summary>
        /// Xóa sạch mọi VFX đang hoạt động.
        /// </summary>
        public static void ClearPools()
        {
            if (GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.ClearAllActiveEffects();
            }
        }
    }
}

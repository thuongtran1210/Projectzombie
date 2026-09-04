using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Core.Pooling
{
    /// <summary>
    /// Quản lý phân cấp Hierarchy tập trung cho các Object Pool trong Runtime.
    /// Giữ màn hình Scene Hierarchy luôn gọn gàng mà KHÔNG gây overhead đổi Parent trong Game loop.
    /// </summary>
    public class PoolHierarchyManager : MonoBehaviour
    {
        private static PoolHierarchyManager _instance;
        public static PoolHierarchyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PoolHierarchyManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("--- [POOL_HIERARCHY_ROOT] ---");
                        _instance = go.AddComponent<PoolHierarchyManager>();
                    }
                }
                return _instance;
            }
        }

        public enum PoolCategory
        {
            Projectiles,
            Enemies,
            Collectibles,
            VFX,
            Misc
        }

        private readonly Dictionary<PoolCategory, Transform> _categoryRoots = new();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitializeRoots();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _categoryRoots.Clear();
            }
        }

        private void InitializeRoots()
        {
            foreach (PoolCategory category in System.Enum.GetValues(typeof(PoolCategory)))
            {
                var categoryGo = new GameObject($"[{category}]");
                categoryGo.transform.SetParent(transform);
                _categoryRoots[category] = categoryGo.transform;
            }
        }

        /// <summary>
        /// Lấy Transform Root tương ứng cho một Category để set parent ngay lúc khởi tạo (Instantiate).
        /// </summary>
        public Transform GetCategoryRoot(PoolCategory category)
        {
            if (_categoryRoots.TryGetValue(category, out var root) && root != null)
            {
                return root;
            }

            var categoryGo = new GameObject($"[{category}]");
            categoryGo.transform.SetParent(transform);
            _categoryRoots[category] = categoryGo.transform;
            return categoryGo.transform;
        }
    }
}

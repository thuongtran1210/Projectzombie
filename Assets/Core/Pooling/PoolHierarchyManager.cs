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
        public static PoolHierarchyManager Instance { get; private set; }

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
            if (Instance == null)
            {
                Instance = this;
                InitializeRoots();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
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

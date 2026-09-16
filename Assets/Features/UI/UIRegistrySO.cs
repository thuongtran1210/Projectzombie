using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AddressableAssets;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Bảng đăng ký cấu hình tập trung cho toàn bộ các Màn hình UI Ngoài Game (Meta Screens).
    /// Tuân thủ nguyên tắc Data-Driven và Open/Closed Principle.
    /// Giúp thêm màn hình mới chỉ bằng việc kéo Prefab vào Inspector mà không cần sửa code C#.
    /// </summary>
    [CreateAssetMenu(fileName = "UIRegistry", menuName = "ProjectZombie/UI/UI Registry")]
    public class UIRegistrySO : ScriptableObject
    {
        [Serializable]
        public struct ScreenEntry
        {
            public MetaScreenType screenType;
            [Tooltip("Tham chiếu trực tiếp Prefab (Fallback truyền thống)")]
            public BaseMetaScreenView screenPrefab;
            [Tooltip("Tham chiếu Addressables (Khuyên dùng để tối ưu RAM)")]
            public AssetReferenceGameObject screenPrefabRef;
            [Tooltip("Tên định danh hiển thị trên Hierarchy khi instantiate")]
            public string screenHierarchyName;
        }

        [Header("Danh Sách Màn Hình Đăng Ký")]
        [SerializeField] private List<ScreenEntry> _screens = new List<ScreenEntry>();

        private Dictionary<MetaScreenType, ScreenEntry> _cacheLookup;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _cacheLookup = new Dictionary<MetaScreenType, ScreenEntry>();
            if (_screens == null) return;

            foreach (var entry in _screens)
            {
                if (!_cacheLookup.ContainsKey(entry.screenType))
                {
                    _cacheLookup.Add(entry.screenType, entry);
                }
                else
                {
                    Debug.LogWarning($"[UIRegistrySO] Trùng lặp cấu hình cho ScreenType: {entry.screenType}.");
                }
            }
        }

        /// <summary>
        /// Lấy thông tin cấu hình Prefab của một màn hình theo ScreenType.
        /// </summary>
        public bool TryGetScreenEntry(MetaScreenType screenType, out ScreenEntry entry)
        {
            if (_cacheLookup == null || _cacheLookup.Count == 0)
            {
                BuildLookup();
            }

            return _cacheLookup.TryGetValue(screenType, out entry);
        }

        /// <summary>
        /// Lấy Prefab của màn hình theo ScreenType.
        /// </summary>
        public BaseMetaScreenView GetPrefab(MetaScreenType screenType)
        {
            if (TryGetScreenEntry(screenType, out var entry))
            {
                return entry.screenPrefab;
            }
            return null;
        }

        public IReadOnlyList<ScreenEntry> AllScreens => _screens;
    }
}

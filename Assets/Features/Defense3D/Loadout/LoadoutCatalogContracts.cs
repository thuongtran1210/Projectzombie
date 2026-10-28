using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Bản ghi thông tin thực thể trong Catalog của Defense 3D.
    /// Hoàn toàn độc lập với UnityEngine / MonoBehaviour.
    /// </summary>
    public sealed class LoadoutCatalogEntry
    {
        public string Id { get; }
        public LoadoutItemType Type { get; }
        public string DisplayName { get; }
        public bool IsAvailable { get; }

        public LoadoutCatalogEntry(string id, LoadoutItemType type, string displayName, bool isAvailable = true)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id không được để trống.", nameof(id));

            Id = id;
            Type = type;
            DisplayName = displayName ?? id;
            IsAvailable = isAvailable;
        }
    }

    /// <summary>
    /// Hợp đồng tra cứu danh tính, loại và tính khả dụng của nội dung trong Catalog.
    /// </summary>
    public interface ILoadoutCatalog
    {
        bool Contains(string id);
        bool TryGetEntry(string id, out LoadoutCatalogEntry entry);
        IReadOnlyList<LoadoutCatalogEntry> GetEntriesByType(LoadoutItemType type);
    }

    /// <summary>
    /// Hợp đồng kiểm tra quyền sở hữu nội dung (tướng, pháp bảo) từ tiến trình người chơi.
    /// </summary>
    public interface ILoadoutOwnership
    {
        bool IsOwned(string id);
    }
}

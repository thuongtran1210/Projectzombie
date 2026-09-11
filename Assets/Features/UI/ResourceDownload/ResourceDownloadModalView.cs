using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI.ResourceDownload
{
    /// <summary>
    /// View hiển thị Modal Quản Lý Dữ Liệu Tải Xuống (Resource Download Modal View).
    /// </summary>
    public class ResourceDownloadModalView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.ResourceDownload;

        [Header("Header & Summary")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _summaryStorageText;

        [Header("Global Action Buttons")]
        [SerializeField] private Button _downloadAllButton;
        [SerializeField] private TextMeshProUGUI _downloadAllButtonText;
        [SerializeField] private Button _clearAllCacheButton;
        [SerializeField] private Button _closeButton;

        [Header("Item List Container")]
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private ResourcePackageItemView _itemPrefab;

        public event Action OnDownloadAllClicked;
        public event Action OnClearAllCacheClicked;
        public event Action OnCloseClicked;

        private readonly List<ResourcePackageItemView> _spawnedItemViews = new List<ResourcePackageItemView>();

        protected override void Awake()
        {
            base.Awake();

            if (_downloadAllButton != null) _downloadAllButton.onClick.AddListener(() => OnDownloadAllClicked?.Invoke());
            if (_clearAllCacheButton != null) _clearAllCacheButton.onClick.AddListener(() => OnClearAllCacheClicked?.Invoke());
            if (_closeButton != null) _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OnCloseClicked?.Invoke();
        }

        public void SetSummary(float localCachedMb, float remoteMissingMb)
        {
            if (_summaryStorageText != null)
            {
                _summaryStorageText.text = $"Dung lượng đã lưu trong máy: <color=#00FF88>{localCachedMb:0.0} MB</color>  |  Chưa tải: <color=#FFAA00>{remoteMissingMb:0.0} MB</color>";
            }

            if (_downloadAllButton != null)
            {
                _downloadAllButton.gameObject.SetActive(remoteMissingMb > 0.05f);
                if (_downloadAllButtonText != null)
                {
                    _downloadAllButtonText.text = $"TẢI TẤT CẢ ({remoteMissingMb:0.0} MB)";
                }
            }

            if (_clearAllCacheButton != null)
            {
                _clearAllCacheButton.gameObject.SetActive(localCachedMb > 0.05f);
            }
        }

        public List<ResourcePackageItemView> PopulateItems(
            List<ResourcePackageItemData> dataList,
            Action<ResourcePackageItemData> onDownloadItem,
            Action<ResourcePackageItemData> onDeleteItem)
        {
            // Clear old items
            foreach (var view in _spawnedItemViews)
            {
                if (view != null && view.gameObject != null)
                {
                    Destroy(view.gameObject);
                }
            }
            _spawnedItemViews.Clear();

            if (_itemsContainer == null || _itemPrefab == null || dataList == null) return _spawnedItemViews;

            foreach (var itemData in dataList)
            {
                var itemView = Instantiate(_itemPrefab, _itemsContainer);
                itemView.gameObject.SetActive(true);
                itemView.Bind(itemData);
                itemView.OnDownloadClicked += onDownloadItem;
                itemView.OnDeleteClicked += onDeleteItem;
                _spawnedItemViews.Add(itemView);
            }

            return _spawnedItemViews;
        }

        public ResourcePackageItemView FindItemViewByKey(string addressKey)
        {
            return _spawnedItemViews.Find(v => v != null && v.name.Contains(addressKey));
        }
    }
}

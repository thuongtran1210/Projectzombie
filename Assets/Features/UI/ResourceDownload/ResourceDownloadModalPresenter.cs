using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Core.Services.Addressables;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Features.UI.ResourceDownload
{
    /// <summary>
    /// Presenter điều phối toàn bộ danh mục tài nguyên tải về (MVP Pattern).
    /// </summary>
    public class ResourceDownloadModalPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private ResourceDownloadModalView _view;

        private AddressablePatchManager _patchManager => AddressablePatchManager.Instance;
        private readonly List<ResourcePackageItemData> _packages = new List<ResourcePackageItemData>();
        private readonly Dictionary<string, ResourcePackageItemView> _viewMap = new Dictionary<string, ResourcePackageItemView>();

        private void Awake()
        {
            if (_view == null) _view = GetComponent<ResourceDownloadModalView>();
        }

        private void OnEnable()
        {
            InitializePackageDefinitions();
            RefreshAllStatuses();
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnDownloadAllClicked += HandleDownloadAll;
                _view.OnClearAllCacheClicked += HandleClearAllCache;
                _view.OnCloseClicked += HandleClose;
            }

            _patchManager.OnPatchProgressChanged += HandlePatchProgress;
            _patchManager.OnPatchCompleted += HandlePatchCompleted;
            _patchManager.OnPatchFailed += HandlePatchFailed;
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnDownloadAllClicked -= HandleDownloadAll;
                _view.OnClearAllCacheClicked -= HandleClearAllCache;
                _view.OnCloseClicked -= HandleClose;
            }

            if (_patchManager != null)
            {
                _patchManager.OnPatchProgressChanged -= HandlePatchProgress;
                _patchManager.OnPatchCompleted -= HandlePatchCompleted;
                _patchManager.OnPatchFailed -= HandlePatchFailed;
            }
        }

        private void InitializePackageDefinitions()
        {
            _packages.Clear();

            // 1. Ải 2: Cổ Thành Đông Sơn
            _packages.Add(new ResourcePackageItemData
            {
                packageId = "STAGE_02",
                title = "Ải 2: Cổ Thành Đông Sơn (Bản đồ & Quái Vật)",
                description = "Bản đồ thành cổ Đông Sơn, hiệu ứng sương mù và các chủng quái vật cổ đại.",
                addressableKey = "Map_AncientCitadel",
                estimatedSizeMb = 12.4f
            });

            // 2. Ải 3: Đầm Lầy Thần Sa
            _packages.Add(new ResourcePackageItemData
            {
                packageId = "STAGE_03",
                title = "Ải 3: Đầm Lầy Thần Sa (Bản đồ & Bùn Độc)",
                description = "Bản đồ đầm lầy chu sa, cơ chế vũng bùn làm chậm và quái vật thủy quái.",
                addressableKey = "Map_CinnabarSwamp",
                estimatedSizeMb = 14.2f
            });

            // 3. Gói Âm Thanh & BGM Chất Lượng Cao
            _packages.Add(new ResourcePackageItemData
            {
                packageId = "AUDIO_DLC",
                title = "Nhạc Nền & Âm Thanh Không Gian (BGM DLC)",
                description = "Nhạc nền cổ phong Ải 2, Ải 3 và hiệu ứng âm thanh kỹ năng mở rộng.",
                addressableKey = "BGM_AncientCitadel",
                estimatedSizeMb = 8.6f
            });

            // 4. Gói Dữ Liệu Cập Nhật Nóng (Live-Ops Configs & Upgrades)
            _packages.Add(new ResourcePackageItemData
            {
                packageId = "META_CONFIGS",
                title = "Dữ Liệu Thẻ Kỹ Năng & Cấu Hình Cập Nhật Nóng",
                description = "Các thẻ nâng cấp vũ khí, cây kỹ năng Tứ Bất Tử và cấu hình banner Gacha mới nhất.",
                addressableKey = "WorldStageDatabase",
                estimatedSizeMb = 0.5f
            });
        }

        public async void RefreshAllStatuses()
        {
            if (_view == null) return;

            float totalCachedMb = 0f;
            float totalMissingMb = 0f;

            // Render danh sách lên View
            var spawnedViews = _view.PopulateItems(_packages, HandleDownloadSingleItem, HandleDeleteSingleItem);
            _viewMap.Clear();

            for (int i = 0; i < _packages.Count; i++)
            {
                var pkg = _packages[i];
                if (i < spawnedViews.Count)
                {
                    _viewMap[pkg.addressableKey] = spawnedViews[i];
                }

                // Kiểm tra trạng thái thực tế từ Addressables
                var status = await _patchManager.CheckAssetStatusAsync(pkg.addressableKey);
                pkg.isDownloaded = !status.NeedsDownload;
                pkg.actualDownloadSizeBytes = status.DownloadSizeBytes;

                if (pkg.isDownloaded)
                {
                    totalCachedMb += pkg.estimatedSizeMb;
                }
                else
                {
                    float missing = status.DownloadSizeBytes > 0 ? status.DownloadSizeBytes / 1048576f : pkg.estimatedSizeMb;
                    totalMissingMb += missing;
                }

                if (_viewMap.TryGetValue(pkg.addressableKey, out var itemView) && itemView != null)
                {
                    itemView.UpdateState(pkg.isDownloaded, pkg.actualDownloadSizeBytes);
                }
            }

            _view.SetSummary(totalCachedMb, totalMissingMb);
        }

        private async void HandleDownloadSingleItem(ResourcePackageItemData data)
        {
            if (data == null || string.IsNullOrEmpty(data.addressableKey)) return;

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();

            if (_viewMap.TryGetValue(data.addressableKey, out var itemView) && itemView != null)
            {
                itemView.UpdateProgress(0f, "Đang kết nối CDN...");
            }

            await _patchManager.DownloadPatchAsync(new object[] { data.addressableKey });
        }

        private void HandleDeleteSingleItem(ResourcePackageItemData data)
        {
            if (data == null || string.IsNullOrEmpty(data.addressableKey)) return;

            global::Core.Audio.AudioManager.Instance?.PlayUIClick();

            // Xóa cache của key này
            _patchManager.ClearAssetCache(data.addressableKey);
            Debug.Log($"<color=#FFD700>[ResourceDownloadModal]</color> Đã xóa cache của gói: {data.title}");

            RefreshAllStatuses();
        }

        private async void HandleDownloadAll()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();

            var keysToDownload = new List<object>();
            foreach (var pkg in _packages)
            {
                if (!pkg.isDownloaded)
                {
                    keysToDownload.Add(pkg.addressableKey);
                }
            }

            if (keysToDownload.Count > 0)
            {
                await _patchManager.DownloadPatchAsync(keysToDownload);
            }
        }

        private void HandleClearAllCache()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();

            foreach (var pkg in _packages)
            {
                _patchManager.ClearAssetCache(pkg.addressableKey);
            }

            Debug.Log("<color=#FFD700>[ResourceDownloadModal]</color> Đã xóa toàn bộ cache DLC!");
            RefreshAllStatuses();
        }

        private void HandlePatchProgress(PatchProgress progress)
        {
            if (_patchManager == null || !_patchManager.IsDownloading) return;

            string downloadingKey = _patchManager.CurrentDownloadingKey?.ToString();
            if (string.IsNullOrEmpty(downloadingKey)) return;

            if (_viewMap.TryGetValue(downloadingKey, out var itemView) && itemView != null)
            {
                itemView.UpdateProgress(progress.Percent, progress.FormattedProgress);
            }
        }

        private void HandlePatchCompleted()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm(1.2f);
            RefreshAllStatuses();
        }

        private void HandlePatchFailed(string error)
        {
            RefreshAllStatuses();
        }

        private void HandleClose()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}

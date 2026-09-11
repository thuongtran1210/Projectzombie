using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectZombie.Core.Services.Addressables
{
    /// <summary>
    /// Trạng thái của quá trình tải bản vá DLC / Addressables Patch.
    /// </summary>
    public enum PatchState
    {
        CheckingForUpdates,
        UpdateAvailable,
        UpToDate,
        Downloading,
        Completed,
        Failed
    }

    /// <summary>
    /// DTO chứa thông tin tiến độ tải tài nguyên từ CDN.
    /// </summary>
    public struct PatchProgress
    {
        public PatchState State;
        public float Percent; // 0.0f -> 1.0f
        public long DownloadedBytes;
        public long TotalBytes;
        public string StatusMessage;

        public string FormattedProgress => $"{DownloadedBytes / 1048576f:0.0}MB / {TotalBytes / 1048576f:0.0}MB ({(Percent * 100f):0}%)";
    }

    /// <summary>
    /// Service điều phối kiểm tra phiên bản trên Firebase CDN, tính dung lượng patch và tải gói DLC bất đồng bộ.
    /// Tuân thủ quy chuẩn Event-Driven và Non-blocking cho nền tảng Android.
    /// </summary>
    public class AddressablePatchManager
    {
        private static AddressablePatchManager _instance;
        public static AddressablePatchManager Instance => _instance ??= new AddressablePatchManager();

        public event Action<PatchProgress> OnPatchProgressChanged;
        public event Action OnPatchCompleted;
        public event Action<string> OnPatchFailed;

        private List<string> _catalogsToUpdate = new();
        private long _totalDownloadSize = 0;

        public long TotalDownloadSize => _totalDownloadSize;
        public bool HasUpdate => _totalDownloadSize > 0 || _catalogsToUpdate.Count > 0;

        // Trạng thái theo dõi tải nền (Background Download Persistence)
        public bool IsDownloading { get; private set; } = false;
        public object CurrentDownloadingKey { get; private set; } = null;
        public PatchProgress CurrentProgress { get; private set; } = default;

        /// <summary>
        /// 1. Khởi tạo Addressables và kiểm tra xem trên CDN có Catalog bản vá mới không.
        /// </summary>
        public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                NotifyProgress(PatchState.CheckingForUpdates, 0f, 0, 0, "Đang kiểm tra dữ liệu máy chủ...");

                // Khởi tạo Addressables Runtime
                var initHandle = UnityEngine.AddressableAssets.Addressables.InitializeAsync();
                await initHandle.Task;

                if (cancellationToken.IsCancellationRequested) return false;

                // Kiểm tra danh sách Catalogs có bản update
                var checkHandle = UnityEngine.AddressableAssets.Addressables.CheckForCatalogUpdates(false);
                var catalogs = await checkHandle.Task;

                if (cancellationToken.IsCancellationRequested) return false;

                _catalogsToUpdate.Clear();
                if (catalogs != null && catalogs.Count > 0)
                {
                    _catalogsToUpdate.AddRange(catalogs);
                }

                // Cập nhật Catalogs nếu có
                if (_catalogsToUpdate.Count > 0)
                {
                    NotifyProgress(PatchState.CheckingForUpdates, 0.5f, 0, 0, "Đang đồng bộ danh mục tài nguyên mới...");
                    var updateHandle = UnityEngine.AddressableAssets.Addressables.UpdateCatalogs(_catalogsToUpdate, false);
                    await updateHandle.Task;
                }

                // Kiểm tra dung lượng các dependencies cần tải về máy
                // Lấy tất cả ResourceLocations thuộc Addressables Catalog thay vì hardcode key 'default'
                long totalSize = 0;
                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync((IEnumerable<object>)new object[] { "default", "preload", "Map", "UpgradeData" }, UnityEngine.AddressableAssets.Addressables.MergeMode.Union);
                await locHandle.Task;

                if (locHandle.Status == AsyncOperationStatus.Succeeded && locHandle.Result != null && locHandle.Result.Count > 0)
                {
                    var sizeHandle = UnityEngine.AddressableAssets.Addressables.GetDownloadSizeAsync(locHandle.Result);
                    totalSize = await sizeHandle.Task;
                }
                _totalDownloadSize = totalSize;

                if (_totalDownloadSize > 0)
                {
                    NotifyProgress(PatchState.UpdateAvailable, 0f, 0, _totalDownloadSize, $"Có bản vá mới: {_totalDownloadSize / 1048576f:0.0} MB");
                    return true;
                }
                else
                {
                    NotifyProgress(PatchState.UpToDate, 1f, 0, 0, "Dữ liệu trò chơi đã là mới nhất!");
                    OnPatchCompleted?.Invoke();
                    return false;
                }
            }
            catch (Exception ex)
            {
                string error = $"Lỗi kiểm tra bản vá CDN: {ex.Message}";
                Debug.LogWarning($"[{nameof(AddressablePatchManager)}] {error}. Tự động bỏ qua và vào Sảnh.");
                NotifyProgress(PatchState.UpToDate, 1f, 0, 0, "Dữ liệu trò chơi đã sẵn sàng!");
                OnPatchCompleted?.Invoke();
                return false;
            }
        }

        /// <summary>
        /// 2. Bắt đầu tải các gói AssetBundle từ Firebase CDN về bộ nhớ Cache điện thoại.
        /// </summary>
        public async Task<bool> DownloadPatchAsync(IEnumerable<object> keys = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IsDownloading = true;
                if (keys != null)
                {
                    using var enumerator = keys.GetEnumerator();
                    if (enumerator.MoveNext()) CurrentDownloadingKey = enumerator.Current;
                }
                else
                {
                    CurrentDownloadingKey = "ALL_PATCH";
                }

                NotifyProgress(PatchState.Downloading, 0f, 0, _totalDownloadSize, "Đang kết nối CDN...");

                AsyncOperationHandle downloadHandle;
                long targetExpectedSize = _totalDownloadSize;

                if (keys != null)
                {
                    // Lấy dung lượng dự kiến của keys này để tính % mượt mà
                    try
                    {
                        var sizeCheck = UnityEngine.AddressableAssets.Addressables.GetDownloadSizeAsync(keys);
                        long kSize = await sizeCheck.Task;
                        if (kSize > 0) targetExpectedSize = kSize;
                    }
                    catch { }

                    // 1. Kiểm tra xem các keys này có tồn tại trong Addressables Catalog không trước khi tải
                    var checkLocHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(keys, UnityEngine.AddressableAssets.Addressables.MergeMode.Union);
                    await checkLocHandle.Task;

                    if (checkLocHandle.Status == AsyncOperationStatus.Succeeded && checkLocHandle.Result != null && checkLocHandle.Result.Count > 0)
                    {
                        downloadHandle = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync(checkLocHandle.Result, false);
                    }
                    else
                    {
                        // Fallback: Thử truyền trực tiếp keys vào DownloadDependenciesAsync
                        downloadHandle = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync(keys, UnityEngine.AddressableAssets.Addressables.MergeMode.Union, false);
                    }
                }
                else
                {
                    var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync((IEnumerable<object>)new object[] { "default", "preload", "Map", "UpgradeData" }, UnityEngine.AddressableAssets.Addressables.MergeMode.Union);
                    await locHandle.Task;
                    if (locHandle.Status == AsyncOperationStatus.Succeeded && locHandle.Result != null && locHandle.Result.Count > 0)
                    {
                        downloadHandle = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync(locHandle.Result, false);
                    }
                    else
                    {
                        IsDownloading = false;
                        CurrentDownloadingKey = null;
                        NotifyProgress(PatchState.Completed, 1f, 0, 0, "Không có nội dung cần tải.");
                        OnPatchCompleted?.Invoke();
                        return true;
                    }
                }

                // Theo dõi tiến trình tải % liên tục (Non-blocking loop)
                float lastPercent = 0f;
                while (!downloadHandle.IsDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        UnityEngine.AddressableAssets.Addressables.Release(downloadHandle);
                        IsDownloading = false;
                        CurrentDownloadingKey = null;
                        NotifyProgress(PatchState.Failed, 0f, 0, targetExpectedSize, "Đã hủy tải bản vá.");
                        return false;
                    }

                    var status = downloadHandle.GetDownloadStatus();
                    float percent = status.Percent;
                    long downloaded = status.DownloadedBytes;
                    long total = status.TotalBytes > 0 ? status.TotalBytes : targetExpectedSize;

                    // Nếu status.TotalBytes chưa kịp trả về từ CDN, tính % dựa trên downloaded/targetExpectedSize
                    if (percent <= 0f && total > 0 && downloaded > 0)
                    {
                        percent = Mathf.Clamp01((float)downloaded / total);
                    }

                    if (percent > lastPercent) lastPercent = percent;

                    string progressText = total > 0 
                        ? $"Đang tải ({downloaded / 1048576f:0.1} / {total / 1048576f:0.1} MB - {lastPercent * 100f:0}%)"
                        : $"Đang tải tài nguyên... ({lastPercent * 100f:0}%)";

                    NotifyProgress(PatchState.Downloading, lastPercent, downloaded, total, progressText);

                    await Task.Yield();
                }

                if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    UnityEngine.AddressableAssets.Addressables.Release(downloadHandle);
                    _totalDownloadSize = 0;
                    _catalogsToUpdate.Clear();
                    IsDownloading = false;
                    CurrentDownloadingKey = null;

                    NotifyProgress(PatchState.Completed, 1f, _totalDownloadSize, _totalDownloadSize, "Cập nhật thành công!");
                    OnPatchCompleted?.Invoke();
                    return true;
                }
                else
                {
                    string error = $"Tải bản vá thất bại. Status: {downloadHandle.Status}";
                    Debug.LogError($"[{nameof(AddressablePatchManager)}] {error}");
                    UnityEngine.AddressableAssets.Addressables.Release(downloadHandle);
                    IsDownloading = false;
                    CurrentDownloadingKey = null;
                    NotifyProgress(PatchState.Failed, 0f, 0, 0, error);
                    OnPatchFailed?.Invoke(error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                string error = $"Ngoại lệ khi tải CDN: {ex.Message}";
                Debug.LogError($"[{nameof(AddressablePatchManager)}] {error}");
                IsDownloading = false;
                CurrentDownloadingKey = null;
                NotifyProgress(PatchState.Failed, 0f, 0, 0, error);
                OnPatchFailed?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// 3. Kiểm tra trạng thái đồng bộ của một Asset cụ thể (Key / Label).
        /// Trả về (isNeedsUpdate: true nếu chưa tải hoặc cần update, downloadSizeBytes: dung lượng cần tải).
        /// </summary>
        public async Task<(bool NeedsDownload, long DownloadSizeBytes)> CheckAssetStatusAsync(object key)
        {
            try
            {
                // Kiểm tra xem Key có tồn tại trong ResourceLocations của Addressables Catalog không
                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(key);
                var locations = await locHandle.Task;
                if (locations == null || locations.Count == 0)
                {
                    return (false, 0);
                }

                var sizeHandle = UnityEngine.AddressableAssets.Addressables.GetDownloadSizeAsync(key);
                long bytes = await sizeHandle.Task;
                return (bytes > 0, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(AddressablePatchManager)}] Không thể kiểm tra dung lượng cho key '{key}': {ex.Message}");
                return (false, 0);
            }
        }

        /// <summary>
        /// Xóa cache của một key hoặc toàn bộ cache để tải lại từ CDN.
        /// </summary>
        public void ClearAssetCache(object key)
        {
            UnityEngine.AddressableAssets.Addressables.ClearDependencyCacheAsync(key, true);
        }

        private void NotifyProgress(PatchState state, float percent, long downloadedBytes, long totalBytes, string statusMessage)
        {
            CurrentProgress = new PatchProgress
            {
                State = state,
                Percent = percent,
                DownloadedBytes = downloadedBytes,
                TotalBytes = totalBytes,
                StatusMessage = statusMessage
            };
            OnPatchProgressChanged?.Invoke(CurrentProgress);
        }
    }
}

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

        public string FormattedProgress
        {
            get
            {
                if (TotalBytes > 100000)
                {
                    return $"{DownloadedBytes / 1048576f:0.0}MB / {TotalBytes / 1048576f:0.0}MB ({(Percent * 100f):0}%)";
                }
                else if (TotalBytes > 0)
                {
                    return $"{DownloadedBytes / 1024f:0.0}KB / {TotalBytes / 1024f:0.0}KB ({(Percent * 100f):0}%)";
                }
                else
                {
                    return $"{(Percent * 100f):0}%";
                }
            }
        }
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

        public AddressablePatchManager()
        {
            SetupInternalIdTransform();
        }

        /// <summary>
        /// Chuẩn hóa URL khi tải từ Firebase Storage CDN.
        /// Tránh lỗi Addressables tự động chèn /tên_file vào sau query param '?alt=media'.
        /// </summary>
        private void SetupInternalIdTransform()
        {
            UnityEngine.AddressableAssets.Addressables.InternalIdTransformFunc = location =>
            {
                if (string.IsNullOrEmpty(location.InternalId)) return location.InternalId;

                // Kiểm tra nếu là URL Firebase Storage
                if (location.InternalId.Contains("firebasestorage.googleapis.com") || location.InternalId.Contains("vongxuyen.firebasestorage.app"))
                {
                    string rawUrl = location.InternalId;
                    
                    // Tìm file .bundle hoặc .json hoặc .hash trong chuỗi URL
                    var match = System.Text.RegularExpressions.Regex.Match(rawUrl, @"(?<filename>[\w\-\._]+\.(bundle|hash|json))", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string fileName = match.Groups["filename"].Value;
#if UNITY_ANDROID
                        string platformFolder = "Android";
#elif UNITY_IOS
                        string platformFolder = "iOS";
#else
                        string platformFolder = "StandaloneWindows64";
#endif
                        string correctedUrl = $"https://firebasestorage.googleapis.com/v0/b/vongxuyen.firebasestorage.app/o/{platformFolder}%2F{fileName}?alt=media";
                        return correctedUrl;
                    }
                }

                return location.InternalId;
            };
        }

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

                // Kiểm tra kết nối mạng Offline
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Debug.Log($"[{nameof(AddressablePatchManager)}] Thiết bị đang Offline, sử dụng Asset nội bộ máy.");
                    NotifyProgress(PatchState.UpToDate, 1f, 0, 0, "Đang ở chế độ Ngoại Tuyến (Offline)");
                    OnPatchCompleted?.Invoke();
                    return false;
                }

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
                // Danh sách các Label DLC cần kiểm tra dung lượng từ CDN
                var dlcLabels = new object[] { "RemoteDLC", "MetaConfigs", "UpgradeData", "Map", "default", "preload" };
                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync((IEnumerable<object>)dlcLabels, UnityEngine.AddressableAssets.Addressables.MergeMode.Union);
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

                // Đảm bảo Addressables Runtime đã được Initialize
                await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

                // Tự động kiểm tra và cập nhật Catalog mới nhất từ Firebase CDN nếu có
                try
                {
                    var checkHandle = UnityEngine.AddressableAssets.Addressables.CheckForCatalogUpdates(false);
                    var catalogs = await checkHandle.Task;
                    if (catalogs != null && catalogs.Count > 0)
                    {
                        var updateHandle = UnityEngine.AddressableAssets.Addressables.UpdateCatalogs(catalogs, false);
                        await updateHandle.Task;
                        Debug.Log("<color=#00FF88>[AddressablePatchManager]</color> Đã cập nhật Catalog mới nhất từ Firebase CDN!");
                    }
                }
                catch (Exception catEx)
                {
                    Debug.LogWarning($"[{nameof(AddressablePatchManager)}] Không thể kiểm tra Catalog Update: {catEx.Message}");
                }

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

                    // 1. Tải Dependencies trực tiếp cho keys
                    downloadHandle = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync(keys, UnityEngine.AddressableAssets.Addressables.MergeMode.Union, false);
                }
                else
                {
                    var dlcLabels = new object[] { "RemoteDLC", "MetaConfigs", "UpgradeData", "Map", "default", "preload" };
                    var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync((IEnumerable<object>)dlcLabels, UnityEngine.AddressableAssets.Addressables.MergeMode.Union);
                    await locHandle.Task;
                    if (locHandle.Status == AsyncOperationStatus.Succeeded && locHandle.Result != null && locHandle.Result.Count > 0)
                    {
                        downloadHandle = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync(locHandle.Result, false);
                    }
                    else
                    {
                        downloadHandle = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync((IEnumerable<object>)dlcLabels, UnityEngine.AddressableAssets.Addressables.MergeMode.Union, false);
                    }
                }

                // Theo dõi tiến trình tải % liên tục (Non-blocking loop) có Timeout bảo vệ
                float lastPercent = 0f;
                float downloadStartTime = Time.realtimeSinceStartup;
                const float DOWNLOAD_TIMEOUT_SECONDS = 45f; // Timeout 45 giây nếu kẹt kết nối

                while (!downloadHandle.IsDone)
                {
                    if (cancellationToken.IsCancellationRequested || (Time.realtimeSinceStartup - downloadStartTime > DOWNLOAD_TIMEOUT_SECONDS && lastPercent <= 0f))
                    {
                        UnityEngine.AddressableAssets.Addressables.Release(downloadHandle);
                        IsDownloading = false;
                        CurrentDownloadingKey = null;
                        string timeoutMsg = cancellationToken.IsCancellationRequested ? "Đã hủy tải bản vá." : "Quá thời gian kết nối CDN (Timeout).";
                        NotifyProgress(PatchState.Failed, 0f, 0, targetExpectedSize, timeoutMsg);
                        OnPatchFailed?.Invoke(timeoutMsg);
                        return false;
                    }

                    var status = downloadHandle.GetDownloadStatus();
                    float percent = status.Percent;
                    long downloaded = status.DownloadedBytes;
                    long total = status.TotalBytes;

                    if (total <= 0 && targetExpectedSize > 0)
                    {
                        total = targetExpectedSize;
                    }

                    // Nếu status.TotalBytes chưa kịp trả về từ CDN, tính % dựa trên downloaded/total
                    if (percent <= 0f && total > 0 && downloaded > 0)
                    {
                        percent = Mathf.Clamp01((float)downloaded / total);
                    }

                    if (percent > lastPercent)
                    {
                        lastPercent = percent;
                        downloadStartTime = Time.realtimeSinceStartup; // Reset timer nếu có byte mới tải về
                    }

                    NotifyProgress(PatchState.Downloading, lastPercent, downloaded, total, CurrentProgress.FormattedProgress);

                    await Task.Yield();
                }

                // Đợi hoàn tất tác vụ ngầm
                await downloadHandle.Task;

                if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"<color=#00FF88>[AddressablePatchManager]</color> TẢI THÀNH CÔNG GÓI: {CurrentDownloadingKey}");
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
                await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(key);
                var locations = await locHandle.Task;
                if (locations == null || locations.Count == 0)
                {
                    // Thử cập nhật Catalog từ Firebase CDN nếu key chưa có
                    try
                    {
                        var checkHandle = UnityEngine.AddressableAssets.Addressables.CheckForCatalogUpdates(false);
                        var catalogs = await checkHandle.Task;
                        if (catalogs != null && catalogs.Count > 0)
                        {
                            var updateHandle = UnityEngine.AddressableAssets.Addressables.UpdateCatalogs(catalogs, false);
                            await updateHandle.Task;

                            var retryLoc = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(key);
                            locations = await retryLoc.Task;
                        }
                    }
                    catch { }

                    if (locations == null || locations.Count == 0)
                    {
                        // Vẫn chưa tìm thấy sau khi update catalog -> Báo cần tải
                        return (true, 0);
                    }
                }

                var sizeHandle = UnityEngine.AddressableAssets.Addressables.GetDownloadSizeAsync(key);
                long bytes = await sizeHandle.Task;
                return (bytes > 0, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(AddressablePatchManager)}] Không thể kiểm tra dung lượng cho key '{key}': {ex.Message}");
                return (true, 0);
            }
        }

        /// <summary>
        /// Xóa cache của một key hoặc toàn bộ cache để tải lại từ CDN.
        /// </summary>
        public async Task ClearAssetCacheAsync(object key)
        {
            try
            {
                // Khi autoRelease = true (mặc định), Unity Addressables sẽ tự động Release handle khi hoàn tất, không được gọi Addressables.Release thủ công.
                var handle = UnityEngine.AddressableAssets.Addressables.ClearDependencyCacheAsync(key, true);
                if (handle.IsValid())
                {
                    await handle.Task;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(AddressablePatchManager)}] Lỗi xóa cache cho key '{key}': {ex.Message}");
            }
        }

        public async Task ClearAllCacheAsync()
        {
            try
            {
                Caching.ClearCache();
                UnityEngine.AddressableAssets.Addressables.CleanBundleCache();
                await Task.Yield();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(AddressablePatchManager)}] Lỗi xóa toàn bộ cache: {ex.Message}");
            }
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

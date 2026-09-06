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
    /// Triển khai dịch vụ quản lý nạp/giải phóng bộ nhớ Asset qua Unity Addressables System.
    /// Tích hợp In-Flight Task Cache, Reference Counting chuẩn xác và hỗ trợ CancellationToken để tối ưu bộ nhớ.
    /// </summary>
    public class AddressableAssetManager : IAssetProvider
    {
        private static AddressableAssetManager _instance;
        public static AddressableAssetManager Instance => _instance ??= new AddressableAssetManager();

        private readonly Dictionary<string, AsyncOperationHandle> _completedHandles = new();
        private readonly Dictionary<string, int> _refCounts = new();
        private readonly Dictionary<string, Task<object>> _inFlightTasks = new();

        public bool IsAssetLoaded(string address)
        {
            if (string.IsNullOrEmpty(address)) return false;
            return _completedHandles.TryGetValue(address, out var handle) && handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;
        }

        public async Task<T> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"[{nameof(AddressableAssetManager)}] Địa chỉ truyền vào bị rỗng/null.");
                return null;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // 1. Trả về ngay nếu Asset đã có sẵn trong Cache
            if (_completedHandles.TryGetValue(address, out AsyncOperationHandle handle))
            {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _refCounts[address] = _refCounts.GetValueOrDefault(address, 0) + 1;
                    return (T)handle.Result;
                }
            }

            // 2. Tái sử dụng In-Flight Task nếu đang có request khác cùng tải address này
            if (_inFlightTasks.TryGetValue(address, out Task<object> inFlightTask))
            {
                try
                {
                    using (cancellationToken.Register(() => { }))
                    {
                        var resultObj = await inFlightTask;
                        if (cancellationToken.IsCancellationRequested) return null;

                        if (resultObj is T typedResult)
                        {
                            _refCounts[address] = _refCounts.GetValueOrDefault(address, 0) + 1;
                            return typedResult;
                        }
                    }
                    return null;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{nameof(AddressableAssetManager)}] Lỗi khi đợi in-flight task '{address}': {ex.Message}");
                    return null;
                }
            }

            // 3. Khởi tạo In-Flight Task mới
            var taskCompletionSource = new TaskCompletionSource<object>();
            _inFlightTasks[address] = taskCompletionSource.Task;

            try
            {
                var asyncHandle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(address);

                // Lắng nghe cancellation nếu có
                if (cancellationToken.CanBeCanceled)
                {
                    cancellationToken.Register(() =>
                    {
                        if (!asyncHandle.IsDone && asyncHandle.IsValid())
                        {
                            UnityEngine.AddressableAssets.Addressables.Release(asyncHandle);
                        }
                    });
                }

                await asyncHandle.Task;

                if (cancellationToken.IsCancellationRequested)
                {
                    if (asyncHandle.IsValid())
                    {
                        UnityEngine.AddressableAssets.Addressables.Release(asyncHandle);
                    }
                    taskCompletionSource.TrySetCanceled(cancellationToken);
                    return null;
                }

                if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    _completedHandles[address] = asyncHandle;
                    _refCounts[address] = _refCounts.GetValueOrDefault(address, 0) + 1;
                    taskCompletionSource.TrySetResult(asyncHandle.Result);
                    return asyncHandle.Result;
                }

                Debug.LogError($"[{nameof(AddressableAssetManager)}] Không thể tải Asset tại địa chỉ: '{address}'. Status: {asyncHandle.Status}");
                taskCompletionSource.TrySetResult(null);
                return null;
            }
            catch (OperationCanceledException)
            {
                taskCompletionSource.TrySetCanceled(cancellationToken);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AddressableAssetManager)}] Ngoại lệ khi tải Addressable '{address}': {ex.Message}");
                taskCompletionSource.TrySetException(ex);
                return null;
            }
            finally
            {
                _inFlightTasks.Remove(address);
            }
        }

        public async Task<GameObject> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null, CancellationToken cancellationToken = default)
        {
            var prefab = await LoadAssetAsync<GameObject>(address, cancellationToken);
            if (prefab == null || cancellationToken.IsCancellationRequested) return null;

            return UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
        }

        public void ReleaseAsset(string address)
        {
            if (string.IsNullOrEmpty(address)) return;

            if (_completedHandles.TryGetValue(address, out AsyncOperationHandle handle))
            {
                if (_refCounts.TryGetValue(address, out int count))
                {
                    count--;
                    if (count > 0)
                    {
                        _refCounts[address] = count;
                        return;
                    }
                }

                // Reference Count <= 0: Thực sự giải phóng tài nguyên khỏi RAM
                _refCounts.Remove(address);
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
                _completedHandles.Remove(address);
            }
        }

        public void Cleanup()
        {
            _inFlightTasks.Clear();
            _refCounts.Clear();
            foreach (var handle in _completedHandles.Values)
            {
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
            }
            _completedHandles.Clear();
        }
    }
}

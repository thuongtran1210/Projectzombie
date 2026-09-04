using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectZombie.Core.Services.Addressables
{
    /// <summary>
    /// Triển khai dịch vụ quản lý nạp/giải phóng bộ nhớ Asset qua Unity Addressables System.
    /// Tích hợp In-Flight Task Cache và Reference Counting chuẩn xác để tối ưu bộ nhớ.
    /// </summary>
    public class AddressableAssetManager : IAssetProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle> _completedHandles = new();
        private readonly Dictionary<string, int> _refCounts = new();
        private readonly Dictionary<string, Task<object>> _inFlightTasks = new();

        public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"[{nameof(AddressableAssetManager)}] Địa chỉ truyền vào bị rỗng/null.");
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
                    var resultObj = await inFlightTask;
                    if (resultObj is T typedResult)
                    {
                        _refCounts[address] = _refCounts.GetValueOrDefault(address, 0) + 1;
                        return typedResult;
                    }
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
                await asyncHandle.Task;

                if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    _completedHandles[address] = asyncHandle;
                    _refCounts[address] = _refCounts.GetValueOrDefault(address, 0) + 1;
                    taskCompletionSource.SetResult(asyncHandle.Result);
                    return asyncHandle.Result;
                }

                Debug.LogError($"[{nameof(AddressableAssetManager)}] Không thể tải Asset tại địa chỉ: '{address}'. Status: {asyncHandle.Status}");
                taskCompletionSource.SetResult(null);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AddressableAssetManager)}] Ngoại lệ khi tải Addressable '{address}': {ex.Message}");
                taskCompletionSource.SetException(ex);
                return null;
            }
            finally
            {
                _inFlightTasks.Remove(address);
            }
        }

        public async Task<GameObject> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var prefab = await LoadAssetAsync<GameObject>(address);
            if (prefab == null) return null;

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

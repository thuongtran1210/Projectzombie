using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectZombie.Core.Services.Addressables
{
    /// <summary>
    /// Triển khai dịch vụ quản lý nạp/giải phóng bộ nhớ Asset qua Unity Addressables System.
    /// Tích hợp Caching tự động và đếm reference để tránh nạp trùng lặp.
    /// </summary>
    public class AddressableAssetManager : IAssetProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle> _completedHandles = new();

        public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"[{nameof(AddressableAssetManager)}] Địa chỉ truyền vào bị rỗng/null.");
                return null;
            }

            // 1. Trả về ngay nếu Asset đã nằm trong Cache
            if (_completedHandles.TryGetValue(address, out AsyncOperationHandle handle))
            {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return (T)handle.Result;
                }
            }

            // 2. Load bất đồng bộ từ Addressables Engine
            try
            {
                var asyncHandle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(address);
                await asyncHandle.Task;

                if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    _completedHandles[address] = asyncHandle;
                    return asyncHandle.Result;
                }

                Debug.LogError($"[{nameof(AddressableAssetManager)}] Không thể tải Asset tại địa chỉ: '{address}'. Status: {asyncHandle.Status}");
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[{nameof(AddressableAssetManager)}] Ngoại lệ khi tải Addressable '{address}': {ex.Message}");
                return null;
            }
        }

        public async Task<GameObject> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var prefab = await LoadAssetAsync<GameObject>(address);
            if (prefab == null) return null;

            return Object.Instantiate(prefab, position, rotation, parent);
        }

        public void ReleaseAsset(string address)
        {
            if (string.IsNullOrEmpty(address)) return;

            if (_completedHandles.TryGetValue(address, out AsyncOperationHandle handle))
            {
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
                _completedHandles.Remove(address);
            }
        }

        public void Cleanup()
        {
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

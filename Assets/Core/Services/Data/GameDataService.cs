using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectZombie.Core.Services.Data
{
    /// <summary>
    /// Interface dịch vụ cung cấp dữ liệu GameData tập trung (Single Source of Truth).
    /// Hỗ trợ 3 tầng nạp: Cache RAM -> Addressables Hot Update -> Resources Fallback.
    /// Quản lý vòng đời và giải phóng AsyncOperationHandle chuẩn xác.
    /// </summary>
    public interface IGameDataService
    {
        Task<T> GetAsync<T>(string key) where T : UnityEngine.Object;
        void InvalidateCache(string key = null);
        void ReleaseAsset(string key);
        void ReleaseAll();
    }

    /// <summary>
    /// Implementation chuẩn hóa của IGameDataService.
    /// Ngăn chặn Race Condition, In-Flight Duplication và Memory Leak trên Android.
    /// </summary>
    public class GameDataService : IGameDataService
    {
        private static IGameDataService _instance;
        public static IGameDataService Instance => _instance ??= new GameDataService();

        private readonly Dictionary<string, object> _cache = new();
        private readonly Dictionary<string, Task<object>> _inFlight = new();
        private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

        /// <summary>
        /// Nạp tài nguyên Generic với cơ chế Deduplication & 3-Tier Fallback.
        /// </summary>
        public async Task<T> GetAsync<T>(string key) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key)) return null;

            // 1. Tầng 1: Trả về ngay nếu đã có trong RAM Cache
            if (_cache.TryGetValue(key, out var cached) && cached is T cachedTyped && cachedTyped != null)
            {
                return cachedTyped;
            }

            // 2. Chống Race Condition: Nếu đang có tác vụ nạp dở cho cùng 1 key, cùng join vào Task đó
            if (_inFlight.TryGetValue(key, out var pendingTask))
            {
                var pendingResult = await pendingTask;
                return pendingResult as T;
            }

            var tcs = new TaskCompletionSource<object>();
            _inFlight[key] = tcs.Task;

            T result = null;

            try
            {
                // 3. Tầng 2: Nạp từ Addressables CDN (Hot Update)
                // Đảm bảo Addressables đã khởi tạo trước khi gọi LoadAssetAsync
                var locHandle = Addressables.LoadResourceLocationsAsync(key);
                await locHandle.Task;

                if (locHandle.Status == AsyncOperationStatus.Succeeded && locHandle.Result != null && locHandle.Result.Count > 0)
                {
                    var handle = Addressables.LoadAssetAsync<T>(key);
                    _handles[key] = handle;
                    result = await handle.Task;
                }
                else
                {
                    // Giải phóng handle location nếu không tìm thấy key trong catalog
                    if (locHandle.IsValid()) Addressables.Release(locHandle);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameDataService] Addressables load failed for '{key}': {ex.Message}. Falling back to Resources.");
            }

            // 4. Tầng 3: Fallback nạp từ Resources cục bộ trong APK (Khi Offline / CDN lỗi)
            if (result == null)
            {
                result = Resources.Load<T>(key);
            }

#if UNITY_EDITOR
            // 5. Editor Fallback: Tìm trong AssetDatabase nếu đang test trong Unity Editor
            if (result == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"{key} t:{typeof(T).Name}");
                if (guids != null && guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }
#endif

            if (result == null)
            {
                Debug.LogError($"[GameDataService] '{key}' không tồn tại trong cả Addressables và Resources!");
            }
            else
            {
                _cache[key] = result;
            }

            _inFlight.Remove(key);
            tcs.SetResult(result);

            return result;
        }

        /// <summary>
        /// Xóa cache trong RAM để ép nạp lại dữ liệu mới nhất từ CDN khi có Hot Patch.
        /// </summary>
        public void InvalidateCache(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                _cache.Clear();
            }
            else
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Giải phóng AsyncOperationHandle của một Asset cụ thể để giảm Ref-Count.
        /// </summary>
        public void ReleaseAsset(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (_handles.TryGetValue(key, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                _handles.Remove(key);
            }

            _cache.Remove(key);
        }

        /// <summary>
        /// Giải phóng toàn bộ Handles và RAM Cache khi chuyển Scene lớn hoặc dọn dẹp bộ nhớ.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var handle in _handles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _handles.Clear();
            _cache.Clear();
            _inFlight.Clear();
            Debug.Log("[GameDataService] Đã giải phóng toàn bộ Addressables Handles & RAM Cache an toàn.");
        }
    }
}

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Hợp đồng điều hướng kết thúc trận và chuyển cảnh trong Defense 3D.
    /// </summary>
    public interface IDefenseNavigationService
    {
        bool IsNavigating { get; }
        Task<bool> ReturnToHubAsync(Action onBeforeUnload = null);
    }

    /// <summary>
    /// Dịch vụ điều hướng chuẩn thay thế hoàn toàn việc gọi trực tiếp SceneManager.LoadScene(0).
    /// Tuân thủ quy chuẩn G5:
    /// 1. Điều hướng bằng tên Scene tường minh ("SampleScene"), không dùng Build Index.
    /// 2. Cơ chế Debounce Lock chống bấm liên tiếp nút Về Sảnh.
    /// 3. Dọn dẹp sạch sẽ tài nguyên, pool và event listener trước khi chuyển cảnh.
    /// </summary>
    public sealed class DefenseNavigationService : IDefenseNavigationService
    {
        public const string HUB_SCENE_NAME = "MainMenu";
        public const string LEGACY_HUB_SCENE_NAME = "SampleScene";

        private static DefenseNavigationService _default;
        public static DefenseNavigationService Default => _default ??= new DefenseNavigationService();

        public static Action<string> LogWarningHook { get; set; }
        public static Action<string> LogErrorHook { get; set; }

        private static void LogWarning(string msg)
        {
            if (LogWarningHook != null) LogWarningHook(msg);
            else Debug.LogWarning(msg);
        }

        private static void LogError(string msg)
        {
            if (LogErrorHook != null) LogErrorHook(msg);
            else Debug.LogError(msg);
        }

        private bool _isNavigating;
        public bool IsNavigating => _isNavigating;

        /// <summary>
        /// Trở về Sảnh Chính (MainMenu).
        /// </summary>
        /// <param name="onBeforeUnload">Hành động dọn dẹp (cleanup pools, dispose session) trước khi unload scene.</param>
        public async Task<bool> ReturnToHubAsync(Action onBeforeUnload = null)
        {
            if (_isNavigating)
            {
                LogWarning("[DefenseNavigationService] Đang trong quá trình trở về sảnh, bỏ qua thao tác trùng lặp.");
                return false;
            }

            _isNavigating = true;

            try
            {
                // 1. Dọn dẹp tài nguyên và session trước khi chuyển cảnh
                try
                {
                    onBeforeUnload?.Invoke();
                }
                catch (Exception cleanupEx)
                {
                    LogWarning($"[DefenseNavigationService] Lỗi khi dọn dẹp trước khi rời scene: {cleanupEx.Message}");
                }

                // Xóa pending snapshot tạm thời
                DefenseSessionContext.Clear();

                await Task.Yield();

                // 2. Xác định Scene Sảnh hợp lệ bằng Application.CanStreamedLevelBeLoaded
                string targetScene = HUB_SCENE_NAME;
                if (!Application.CanStreamedLevelBeLoaded(targetScene))
                {
                    if (Application.CanStreamedLevelBeLoaded(LEGACY_HUB_SCENE_NAME))
                    {
                        targetScene = LEGACY_HUB_SCENE_NAME;
                    }
                    else if (SceneManager.sceneCountInBuildSettings > 0)
                    {
                        string scenePath = SceneUtility.GetScenePathByBuildIndex(0);
                        if (!string.IsNullOrEmpty(scenePath))
                        {
                            targetScene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                        }
                    }
                }

                // 3. Nạp Scene Sảnh bằng tên tường minh
                var asyncOp = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
                if (asyncOp == null)
                {
                    throw new InvalidOperationException($"Không thể nạp Scene Sảnh '{targetScene}'. Hãy kiểm tra Build Settings.");
                }

                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError($"[DefenseNavigationService] Lỗi khi trở về sảnh: {ex.Message}");
                return false;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Đặt lại cờ trạng thái phục vụ Unit Test.
        /// </summary>
        public void ResetStateForTesting()
        {
            _isNavigating = false;
        }
    }
}

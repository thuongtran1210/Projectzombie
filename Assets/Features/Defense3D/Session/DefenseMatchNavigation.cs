using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Dịch vụ điều hướng nạp và chuyển cảnh vào trận đấu Defense 3D.
    /// Tuân thủ quy chuẩn G4:
    /// 1. Điều hướng bằng tên Scene tường minh ("MapSongQue3D_Prototype"), tuyệt đối không dùng Build Index.
    /// 2. Cơ chế chống bấm liên tiếp (Debounce Guard / Re-entrancy Lock).
    /// 3. Truyền DefenseLoadoutSnapshot an toàn sang Scene mới qua DefenseSessionContext.
    /// 4. Tự động giải phóng trạng thái và cho phép thử lại nếu quá trình nạp thất bại hoặc bị hủy.
    /// </summary>
    public static class DefenseMatchNavigation
    {
        public const string TARGET_SCENE_NAME = "MapSongQue3D_Prototype";

        private static bool _isNavigating;

        /// <summary>
        /// Cho biết hệ thống có đang trong tiến trình nạp chuyển cảnh hay không.
        /// </summary>
        public static bool IsNavigating => _isNavigating;

        /// <summary>
        /// Kích hoạt nạp Scene trận đấu Defense 3D với snapshot đội hình đã chuẩn bị.
        /// </summary>
        /// <param name="snapshot">Snapshot đội hình xuất trận (nếu null, tự động dùng snapshot mặc định).</param>
        /// <param name="reportProgress">Callback báo cáo tiến trình (0.0 đến 1.0) và thông điệp hiển thị.</param>
        /// <returns>True nếu nạp thành công, False nếu bị từ chối hoặc lỗi.</returns>
        public static async Task<bool> LaunchDefenseMatchAsync(
            DefenseLoadoutSnapshot snapshot,
            Action<float, string> reportProgress = null)
        {
            // Chống bấm liên tiếp (Debounce)
            if (_isNavigating)
            {
                Debug.LogWarning("[DefenseMatchNavigation] Đang có tiến trình chuyển cảnh đang chạy, bỏ qua yêu cầu trùng lặp.");
                return false;
            }

            _isNavigating = true;

            try
            {
                reportProgress?.Invoke(0.1f, "Đang kiểm định đội hình xuất trận...");
                await Task.Yield();

                // Chuẩn hóa snapshot
                var targetSnapshot = snapshot ?? DefenseLoadoutSnapshot.CreateDefault();
                DefenseSessionContext.PendingSnapshot = targetSnapshot;

                reportProgress?.Invoke(0.3f, "Đang mở cửa vào Sông Quê 3D...");
                await Task.Yield();

                // Nạp scene bằng tên tường minh
                var asyncOp = SceneManager.LoadSceneAsync(TARGET_SCENE_NAME);
                if (asyncOp == null)
                {
                    throw new InvalidOperationException($"Không thể bắt đầu nạp Scene '{TARGET_SCENE_NAME}'. Hãy kiểm tra Build Settings.");
                }

                while (!asyncOp.isDone)
                {
                    float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                    reportProgress?.Invoke(0.3f + progress * 0.6f, "Đang tải dữ liệu chiến trường Sông Quê...");
                    await Task.Yield();
                }

                reportProgress?.Invoke(1.0f, "Chiến trường 3D đã sẵn sàng!");
                await Task.Yield();

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DefenseMatchNavigation] Lỗi khi nạp Scene Defense 3D: {ex.Message}");
                // Giải phóng bộ nhớ tạm để cho phép thử lại
                DefenseSessionContext.Clear();
                return false;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Đặt lại trạng thái điều hướng thủ công (dùng cho unit test).
        /// </summary>
        public static void ResetStateForTesting()
        {
            _isNavigating = false;
            DefenseSessionContext.Clear();
        }
    }
}

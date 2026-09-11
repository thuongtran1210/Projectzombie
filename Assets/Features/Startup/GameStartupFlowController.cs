using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Core.Services.Addressables;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Features.Startup
{
    /// <summary>
    /// Component quản lý luồng khởi động game ban đầu (Cold Start Patch Flow).
    /// Tự động kiểm tra bản vá / DLC từ Firebase CDN thông qua AddressablePatchManager.
    /// Nếu có bản vá, hiển thị tiến trình tải qua LoadingScreenPresenter, sau đó chuyển thẳng vào Sảnh (Meta Hub).
    /// </summary>
    public class GameStartupFlowController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Có tự động kiểm tra bản vá CDN khi khởi động hay không")]
        [SerializeField] private bool _autoCheckCdnOnStart = true;
        [Tooltip("Thời gian chờ tối đa (giây) khi kiểm tra kết nối CDN tránh bị treo nếu offline")]
        [SerializeField] private float _networkTimeoutSeconds = 4f;

        private void Start()
        {
            StartCoroutine(StartupRoutine());
        }

        private IEnumerator StartupRoutine()
        {
            // Đảm bảo TimeScale hoạt động
            Time.timeScale = 1f;

            if (_autoCheckCdnOnStart)
            {
                bool isDone = false;
                string currentStatus = "Đang kiểm tra dữ liệu máy chủ...";
                float currentPercent = 0f;

                // Subscribe sự kiện tiến trình từ AddressablePatchManager
                Action<PatchProgress> onProgress = (p) =>
                {
                    currentStatus = p.StatusMessage;
                    currentPercent = p.Percent;
                };
                AddressablePatchManager.Instance.OnPatchProgressChanged += onProgress;

                // Sử dụng LoadingScreenPresenter để hiển thị tiến trình mượt mà
                if (LoadingScreenPresenter.Instance != null)
                {
                    LoadingScreenPresenter.Instance.ShowTaskLoading(async (reportProgress) =>
                    {
                        reportProgress?.Invoke(0.1f, "Đang kết nối cổng Hoàng Tuyền...");
                        
                        // Kiểm tra catalog & tính dung lượng
                        var checkTask = AddressablePatchManager.Instance.CheckForUpdatesAsync();
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_networkTimeoutSeconds));

                        var completedTask = await Task.WhenAny(checkTask, timeoutTask);
                        if (completedTask == checkTask && checkTask.Result)
                        {
                            reportProgress?.Invoke(0.3f, $"Tìm thấy bản vá ({AddressablePatchManager.Instance.TotalDownloadSize / 1048576f:0.0} MB). Đang tải...");
                            await AddressablePatchManager.Instance.DownloadPatchAsync();
                        }
                        else
                        {
                            reportProgress?.Invoke(0.5f, "Dữ liệu trò chơi đã sẵn sàng!");
                        }

                        reportProgress?.Invoke(1.0f, "Đang mở sảnh chính...");
                        await Task.Delay(150);
                    }, () =>
                    {
                        isDone = true;
                    }, "Đang đồng bộ dữ liệu cõi âm...");

                    while (!isDone) yield return null;
                }
                else
                {
                    // Fallback chạy ngầm nếu chưa có LoadingScreenPresenter
                    var task = AddressablePatchManager.Instance.CheckForUpdatesAsync();
                    while (!task.IsCompleted) yield return null;
                }

                AddressablePatchManager.Instance.OnPatchProgressChanged -= onProgress;
            }

            // Chuyển sang Sảnh chính (Main Hub)
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.OpenScreen(MetaScreenType.MainHub);
            }

            Debug.Log("<color=#55FF55>[GameStartupFlowController] Đã hoàn tất luồng khởi động game thành công!</color>");
        }
    }
}

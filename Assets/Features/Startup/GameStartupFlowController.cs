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
#pragma warning disable CS0414
        [Header("Settings")]
        [Tooltip("Có tự động kiểm tra bản vá CDN khi khởi động hay không")]
        [SerializeField] private bool _autoCheckCdnOnStart = true;
        [Tooltip("Thời gian chờ tối đa (giây) khi kiểm tra kết nối CDN tránh bị treo nếu offline")]
        [SerializeField] private float _networkTimeoutSeconds = 4f;
#pragma warning restore CS0414

        private void Awake()
        {
            // Thiết lập chuẩn 60 FPS mượt mà và giữ màn hình luôn sáng trên Android
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Start()
        {
            StartCoroutine(StartupRoutine());
        }

        private IEnumerator StartupRoutine()
        {
            // Đảm bảo TimeScale hoạt động
            Time.timeScale = 1f;

#if !UNITY_EDITOR
            // Trên thiết bị thực tế (Android / Mobile) nếu có bật kiểm tra CDN
            if (_autoCheckCdnOnStart)
            {
                bool isDone = false;

                if (LoadingScreenPresenter.Instance != null)
                {
                    LoadingScreenPresenter.Instance.ShowTaskLoading(async (reportProgress) =>
                    {
                        try
                        {
                            reportProgress?.Invoke(0.1f, "Đang kết nối cổng Hoàng Tuyền...");

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
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[{nameof(GameStartupFlowController)}] Bỏ qua kiểm tra CDN do lỗi: {ex.Message}");
                        }

                        reportProgress?.Invoke(1.0f, "Đang mở sảnh chính...");
                        await Task.Delay(100);
                    }, () =>
                    {
                        isDone = true;
                    }, "Đang đồng bộ dữ liệu cõi âm...");

                    float maxWait = _networkTimeoutSeconds + 2f;
                    float elapsed = 0f;
                    while (!isDone && elapsed < maxWait)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }
            }
#else
            yield return null;
#endif

            // Chuyển sang Sảnh chính (Main Hub)
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.OpenScreen(MetaScreenType.MainHub);
            }

            Debug.Log("<color=#55FF55>[GameStartupFlowController] Đã hoàn tất luồng khởi động game thành công!</color>");
        }
    }
}

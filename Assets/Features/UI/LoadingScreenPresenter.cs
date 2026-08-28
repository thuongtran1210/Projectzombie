using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ logic của Màn hình Loading / Chuyển cảnh.
    /// Tự động lựa chọn ngẫu nhiên các Bí Kíp Dân Gian & Quy Luật Ngũ Hành, 
    /// xử lý hoạt ảnh tiến trình (Progress Interpolation) mượt mà không giật lag.
    /// </summary>
    public class LoadingScreenPresenter : MonoBehaviour
    {
        public static LoadingScreenPresenter Instance { get; private set; }

        [SerializeField] private LoadingScreenView _view;

        private struct LoreTip
        {
            public string title;
            public string body;

            public LoreTip(string title, string body)
            {
                this.title = title;
                this.body = body;
            }
        }

        private static readonly LoreTip[] _tipsDatabase = new LoreTip[]
        {
            new LoreTip("<color=#FFD700>[ QUY LUẬT TƯƠNG KHẮC ]</color>", "Đánh trúng hệ khắc chế gây thêm <color=#FFD700>+30% Sát thương</color> và kích hoạt hiệu ứng suy yếu!"),
            new LoreTip("<color=#2ECC71>[ NĂNG LƯỢNG TƯƠNG SINH ]</color>", "Trang bị Pháp bảo Tương Sinh với Tướng giúp giảm <color=#2ECC71>-20% Thời gian hồi chiêu</color>!"),
            new LoreTip("<color=#FF8A50>[ DÉP TỔ ONG THẦN SA ]</color>", "Tung đòn kết liễu Combo Hit 3 sẽ triệu hồi Lốc Dép 360 độ khiến quái bị <color=#FF8A50>Quê Độ</color> quay sang đánh nhau!"),
            new LoreTip("<color=#F1C40F>[ NỒI CƠM THẠCH SANH ]</color>", "Nồi cơm tự động gom quái nguy hiểm và bắn pháo, rơi cơm nắm thần kỳ hồi phục <color=#2ECC71>5% Máu</color>!"),
            new LoreTip("<color=#E74C3C>[ ĐIẾU CÀY CỬU U ]</color>", "Luồng khói thuốc lào khiến quái vật <color=#E74C3C>Say Thuốc</color>, ho sặc sụa và đi giật lùi vô hại!"),
            new LoreTip("<color=#FFD700>[ BÚT PHÁN QUAN ]</color>", "Nét bút thư pháp của Thư Sinh có thể phán quyết chí mạng kẻ địch ở cả 2 hướng trước và sau!"),
            new LoreTip("<color=#F1C40F>[ TRỐNG ĐỒNG ĐÔNG SƠN ]</color>", "Sóng âm trảm linh phát ra từ trống đồng gây <color=#F1C40F>Choáng diện rộng</color> trong 0.5s!"),
            new LoreTip("<color=#9B59B6>[ MIẾU TỨ BẤT TỬ ]</color>", "Dùng Cổ Tiền tích lũy sau mỗi trận để mở khóa vĩnh viễn các nhánh Thần Linh tăng cường sức mạnh tối thượng!"),
            new LoreTip("<color=#E67E22>[ TIẾN HÓA THẦN KHÍ ]</color>", "Nâng cấp Pháp Bảo lên Cấp 6 kết hợp thẻ bổ trợ tương thích để đột phá thành hình thái Tối Thượng!")
        };

        private Coroutine _loadingCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (_view == null) _view = GetComponent<LoadingScreenView>() ?? GetComponentInChildren<LoadingScreenView>(true);
        }

        /// <summary>
        /// Kích hoạt màn hình Loading với thời gian giả lập (Simulation) mượt mà cho chuyển ải / bắt đầu trận.
        /// </summary>
        public void ShowLoading(float duration, Action onComplete = null, string statusMessage = "Đang khai mở cửa Hoàng Tuyền...")
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (_view != null && !_view.gameObject.activeSelf) _view.gameObject.SetActive(true);

            if (_loadingCoroutine != null) StopCoroutine(_loadingCoroutine);
            _loadingCoroutine = StartCoroutine(RoutineSimulatedLoading(duration, onComplete, statusMessage));
        }

        /// <summary>
        /// Kích hoạt màn hình Loading bám sát tiến trình Async Operation thực tế (Async Scene / Asset Loading).
        /// </summary>
        public void ShowAsyncLoading(AsyncOperation asyncOp, Action onComplete = null, string statusMessage = "Đang tải dữ liệu cõi âm...")
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (_view != null && !_view.gameObject.activeSelf) _view.gameObject.SetActive(true);

            if (_loadingCoroutine != null) StopCoroutine(_loadingCoroutine);
            _loadingCoroutine = StartCoroutine(RoutineAsyncLoading(asyncOp, onComplete, statusMessage));
        }

        private IEnumerator RoutineSimulatedLoading(float duration, Action onComplete, string statusMessage)
        {
            // 1. Cập nhật ngẫu nhiên bí kíp
            RefreshRandomTip();
            _view.SetStatusMessage(statusMessage);
            _view.SetProgress(0f);

            // 2. Fade In
            bool fadeInDone = false;
            _view.FadeIn(0.2f, () => fadeInDone = true);
            while (!fadeInDone) yield return null;

            // 3. Tăng thanh tiến trình theo Easing
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                
                // Đường cong Easing mượt mà
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                _view.SetProgress(smoothProgress);

                if (smoothProgress > 0.4f && smoothProgress < 0.7f)
                {
                    _view.SetStatusMessage("Đang ngưng tụ linh khí ngũ hành...");
                }
                else if (smoothProgress >= 0.7f)
                {
                    _view.SetStatusMessage("Chiến trường đã sẵn sàng!");
                }

                yield return null;
            }

            _view.SetProgress(1f);
            yield return new WaitForSecondsRealtime(0.15f);

            // 4. Gọi Callback hoàn tất
            onComplete?.Invoke();

            // 5. Fade Out biến mất
            _view.FadeOut(0.25f);
        }

        private IEnumerator RoutineAsyncLoading(AsyncOperation asyncOp, Action onComplete, string statusMessage)
        {
            RefreshRandomTip();
            _view.SetStatusMessage(statusMessage);
            _view.SetProgress(0f);

            bool fadeInDone = false;
            _view.FadeIn(0.2f, () => fadeInDone = true);
            while (!fadeInDone) yield return null;

            while (!asyncOp.isDone)
            {
                // AsyncOperation dừng ở 0.9 khi nạp xong trước khi active
                float targetProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                _view.SetProgress(targetProgress);
                yield return null;
            }

            _view.SetProgress(1f);
            yield return new WaitForSecondsRealtime(0.1f);

            onComplete?.Invoke();
            _view.FadeOut(0.25f);
        }

        private void RefreshRandomTip()
        {
            if (_view == null || _tipsDatabase.Length == 0) return;
            var randomTip = _tipsDatabase[UnityEngine.Random.Range(0, _tipsDatabase.Length)];
            _view.SetTip(randomTip.title, randomTip.body);
        }
    }
}

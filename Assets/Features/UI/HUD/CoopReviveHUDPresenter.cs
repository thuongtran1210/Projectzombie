// ============================================================================
// FILE: CoopReviveHUDPresenter.cs — TẦNG PRESENTER (MVP)
// Trách nhiệm DUY NHẤT: Lắng nghe Model (IDownedStateProvider),
// format chuỗi hiển thị theo quy chuẩn TextMeshPro Rich Text,
// và cập nhật xuống CoopReviveWorldView.
// Đảm bảo 0-GC allocations trong gameplay loop.
// ============================================================================

using UnityEngine;
using ProjectZombie.Features.Combat.Coop;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// Presenter kết nối giữa Model (IDownedStateProvider) và View (CoopReviveWorldView).
    /// </summary>
    public class CoopReviveHUDPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private CoopReviveWorldView _view;

        private IDownedStateProvider _downedProvider;
        private int _lastReportedPercent = -1;
        private bool _isConstructed = false;

        private const string SOS_TITLE = "<color=#FF3333><b>[CẦN CỨU VIỆN!]</b></color>";
        private const string SOS_PROMPT = "<color=#FFD700>Đứng gần để Cứu</color>";
        private const string REVIVING_SUBTEXT = "<color=#AACCFF>Giữ vị trí gần...</color>";

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<CoopReviveWorldView>();
                if (_view == null)
                {
                    _view = GetComponentInChildren<CoopReviveWorldView>(true);
                }
            }
        }

        private void Start()
        {
            if (!_isConstructed)
            {
                var provider = GetComponentInParent<IDownedStateProvider>();
                if (provider != null)
                {
                    Construct(provider);
                }
            }
        }

        /// <summary>
        /// Inject IDownedStateProvider theo nguyên tắc Dependency Inversion (SOLID).
        /// </summary>
        public void Construct(IDownedStateProvider downedProvider)
        {
            if (_isConstructed)
            {
                Unsubscribe();
            }

            _downedProvider = downedProvider;
            Subscribe();

            _isConstructed = true;
            Refresh();
        }

        private void OnEnable()
        {
            if (_isConstructed)
            {
                Subscribe();
                Refresh();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_downedProvider != null)
            {
                _downedProvider.OnDownedStateChanged += HandleDownedStateChanged;
                _downedProvider.OnReviveProgressChanged += HandleReviveProgressChanged;
            }
        }

        private void Unsubscribe()
        {
            if (_downedProvider != null)
            {
                _downedProvider.OnDownedStateChanged -= HandleDownedStateChanged;
                _downedProvider.OnReviveProgressChanged -= HandleReviveProgressChanged;
            }
        }

        private void HandleDownedStateChanged(bool isDowned)
        {
            if (_view == null) return;

            _view.SetVisible(isDowned);
            _lastReportedPercent = -1;

            if (isDowned)
            {
                _view.SetStatusText(SOS_TITLE, SOS_PROMPT);
                _view.SetProgress(0f, isReviving: false);
            }
        }

        private void HandleReviveProgressChanged(float progressNormalized)
        {
            if (_view == null || _downedProvider == null || !_downedProvider.IsDowned) return;

            int currentPercent = Mathf.Clamp(Mathf.RoundToInt(progressNormalized * 100f), 0, 100);

            // Tối ưu 0 GC: Chỉ format chuỗi khi tỷ lệ % nguyên thực sự thay đổi
            if (currentPercent != _lastReportedPercent)
            {
                _lastReportedPercent = currentPercent;

                if (currentPercent > 0)
                {
                    string progressTitle = $"<color=#00FF88><b>ĐANG CỨU: {currentPercent}%</b></color>";
                    _view.SetStatusText(progressTitle, REVIVING_SUBTEXT);
                    _view.SetProgress(progressNormalized, isReviving: true);
                }
                else
                {
                    _view.SetStatusText(SOS_TITLE, SOS_PROMPT);
                    _view.SetProgress(0f, isReviving: false);
                }
            }
        }

        private void Refresh()
        {
            if (_downedProvider != null)
            {
                HandleDownedStateChanged(_downedProvider.IsDowned);
                if (_downedProvider.IsDowned)
                {
                    HandleReviveProgressChanged(_downedProvider.ReviveProgressNormalized);
                }
            }
        }
    }
}

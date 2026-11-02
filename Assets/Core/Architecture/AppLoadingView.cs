using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>Persistent cover: displayed progress catches up to actual work, never runs ahead.</summary>
    public sealed class AppLoadingView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _status;
        [SerializeField] private Button _dismiss;
        [SerializeField] private TextMeshProUGUI _title, _percent, _eyebrow, _tip;
        [SerializeField] private Image _fill;
        [SerializeField] private RectTransform _head, _seal;
        [SerializeField] private TextMeshProUGUI[] _steps;
        private float _target, _displayed;
        private bool _visible, _error;
        private int _lastPercent = -1, _version;
        public float Progress => _target;
        public float DisplayedProgress => _displayed;
        public bool IsVisible => _visible;
        public event Action<float, string> ProgressChanged;
        private static readonly Color Jade = new Color(.22f,.87f,.69f), Gold = new Color(.91f,.73f,.4f);
        private void Awake() { if (_dismiss != null) _dismiss.onClick.AddListener(Hide); }
        private void OnDestroy() { if (_dismiss != null) _dismiss.onClick.RemoveListener(Hide); }
        public void Begin(string stage, bool returning)
        {
            Show(returning ? "Đang trở về sảnh..." : "Đang rời sảnh...");
            if (_title != null) _title.text = string.IsNullOrWhiteSpace(stage) ? "VONG XUYÊN" : stage.Replace(" (3D)", "").Trim();
            if (_eyebrow != null) _eyebrow.text = returning ? "HÀNH TRÌNH TIẾP NỐI" : "CHUẨN BỊ XUẤT TRẬN";
            if (_tip != null) _tip.text = returning ? "Điều chỉnh đội hình và trang bị trước hành trình tiếp theo." :
                "Kết hợp Băng Hà làm chậm với Hỏa Long nổ lan để chặn đàn quái đông.";
        }
        public void Show(string message)
        {
            _version++; _target = _displayed = 0f; _lastPercent = -1; _error = false; _visible = true;
            if (_dismiss != null) _dismiss.gameObject.SetActive(false);
            _group.alpha = 1f; _group.blocksRaycasts = true; _group.interactable = true;
            if (_fill != null) _fill.color = Jade;
            if (_status != null) _status.text = message;
            DrawProgress(); ProgressChanged?.Invoke(0f, message);
        }
        public void ReportProgress(float progress, string message)
        {
            if (!_visible || _error || float.IsNaN(progress) || float.IsInfinity(progress)) return;
            _target = Mathf.Max(_target, Mathf.Clamp01(progress));
            if (_status != null && _status.text != message) _status.text = message;
            ProgressChanged?.Invoke(_target, message);
        }
        private void Update()
        {
            if (!_visible || _error) return;
            _displayed = Mathf.MoveTowards(_displayed, _target, Time.unscaledDeltaTime * 1.1f);
            if (_seal != null) _seal.Rotate(0,0,-Time.unscaledDeltaTime * 9f);
            DrawProgress();
        }
        private void DrawProgress()
        {
            if (_fill != null) _fill.fillAmount = _displayed;
            if (_head != null) { _head.anchorMin = _head.anchorMax = new Vector2(_displayed,.5f); _head.anchoredPosition = Vector2.zero; }
            int percent = Mathf.FloorToInt(_displayed * 100f + .001f);
            if (_percent != null && percent != _lastPercent) { _lastPercent = percent; _percent.text = percent + "%"; }
            if (_steps != null) for (int i=0;i<_steps.Length;i++)
                if (_steps[i] != null) _steps[i].color = _displayed >= (i==0?.2f:i==1?.62f:.98f) ? Gold : new Color(.45f,.57f,.54f);
        }
        public async Task CompleteAsync()
        {
            if (!_visible || _error) return;
            ReportProgress(1f, "Sẵn sàng"); int version = _version;
            if (!Application.isPlaying) { _displayed = 1f; DrawProgress(); Hide(); return; }
            float until = Time.realtimeSinceStartup + .18f;
            while (_displayed < 1f && Time.realtimeSinceStartup < until && version == _version)
            { _displayed = Mathf.MoveTowards(_displayed, 1f, Time.unscaledDeltaTime * 8f); DrawProgress(); await Task.Yield(); }
            if (version != _version) return;
            _displayed = 1f; DrawProgress(); await Task.Yield();
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < .16f && version == _version)
            { _group.alpha = 1f - Mathf.Clamp01((Time.realtimeSinceStartup-start)/.16f); await Task.Yield(); }
            if (version == _version) Hide();
        }
        public void ShowError(string message, bool canDismiss = true)
        {
            Show(message); _error = true;
            if (_title != null) _title.text = "HÀNH TRÌNH GIÁN ĐOẠN";
            if (_eyebrow != null) _eyebrow.text = "CHƯA THỂ VÀO TRẬN";
            if (_percent != null) _percent.text = "!";
            if (_fill != null) _fill.color = new Color(.9f,.35f,.25f);
            if (_tip != null) _tip.text = canDismiss ? "Đã trở về sảnh. Bạn có thể đóng thông báo và thử lại." : "Hãy kiểm tra kết nối trước khi khởi động lại trò chơi.";
            if (_dismiss != null) _dismiss.gameObject.SetActive(canDismiss);
        }
        public void Hide()
        {
            _version++; _visible = false;
            _group.alpha = 0f; _group.blocksRaycasts = false; _group.interactable = false;
        }
    }
}

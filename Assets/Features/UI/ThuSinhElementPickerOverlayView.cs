using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý Overlay chọn 1 trong 5 icon hệ Ngũ Hành cho skill Phán Quyết Tiền Định của Thư Sinh (Mục 3.1.1 GDD v4.0).
    /// Hiển thị trong 1.5s (không pause game). Tự động fallback nếu hết thời gian đếm ngược.
    /// </summary>
    public class ThuSinhElementPickerOverlayView : MonoBehaviour
    {
        [Header("Element Buttons")]
        [SerializeField] private Button _btnKim;
        [SerializeField] private Button _btnMoc;
        [SerializeField] private Button _btnThuy;
        [SerializeField] private Button _btnHoa;
        [SerializeField] private Button _btnTho;

        [Header("Timer Display")]
        [SerializeField] private Image _timerProgressBar;

        public event System.Action<ElementType> OnElementPicked;

        private float _timeRemaining;
        private const float SELECTION_WINDOW_SECONDS = 1.5f;
        private bool _isShowing;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_btnKim != null) _btnKim.onClick.AddListener(() => SelectElement(ElementType.Kim));
            if (_btnMoc != null) _btnMoc.onClick.AddListener(() => SelectElement(ElementType.Moc));
            if (_btnThuy != null) _btnThuy.onClick.AddListener(() => SelectElement(ElementType.Thuy));
            if (_btnHoa != null) _btnHoa.onClick.AddListener(() => SelectElement(ElementType.Hoa));
            if (_btnTho != null) _btnTho.onClick.AddListener(() => SelectElement(ElementType.Tho));

            gameObject.SetActive(false);
        }

        public void ShowOverlay()
        {
            _timeRemaining = SELECTION_WINDOW_SECONDS;
            _isShowing = true;
            gameObject.SetActive(true);
        }

        public void HideOverlay()
        {
            _isShowing = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isShowing) return;

            _timeRemaining -= Time.deltaTime;

            if (_timerProgressBar != null)
            {
                _timerProgressBar.fillAmount = Mathf.Clamp01(_timeRemaining / SELECTION_WINDOW_SECONDS);
            }

            if (_timeRemaining <= 0f)
            {
                // Hết 1.5s không bấm: Tự động fallback (ElementType.None để Manager tự pick)
                SelectElement(ElementType.None);
            }
        }

        private void SelectElement(ElementType element)
        {
            if (!_isShowing) return;
            HideOverlay();
            OnElementPicked?.Invoke(element);
        }
    }
}

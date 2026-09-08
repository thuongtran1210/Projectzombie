using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI.Controls.Customization
{
    /// <summary>
    /// Component gắn trực tiếp lên bất kỳ nút điều khiển nào cần cho phép người chơi tùy biến vị trí, kích thước và độ mờ.
    /// Hoạt động độc lập, không can thiệp logic gameplay của nút khi ở Runtime thường.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CustomizableControlButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Control Identification")]
        [Tooltip("ID duy nhất để nhận diện nút (ví dụ: Btn_Attack, Btn_Dash, Btn_SignatureSkill, Btn_RelicSkill, Joystick)")]
        [SerializeField] private string _controlId;

        [Tooltip("Tên hiển thị thân thiện với người dùng trong chế độ Chỉnh Sửa")]
        [SerializeField] private string _displayName = "Nút Điều Khiển";

        [Header("Scale Boundaries")]
        [SerializeField] private float _minScale = 0.7f;
        [SerializeField] private float _maxScale = 1.5f;

        [Header("Visual References")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private static int s_ActiveEditModeCount = 0;
        public static bool IsAnyInEditMode => s_ActiveEditModeCount > 0;

        private RectTransform _rectTransform;
        private Vector2 _defaultAnchoredPosition;
        private Vector2 _defaultScale = Vector3.one;
        private float _defaultOpacity = 1.0f;

        private Vector2 _currentScale = Vector2.one;
        private float _currentOpacity = 1.0f;

        private bool _isEditMode = false;
        private Image _selectionHighlight;

        public string ControlId => string.IsNullOrEmpty(_controlId) ? gameObject.name : _controlId;
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? gameObject.name : _displayName;
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());
        public Vector2 CurrentScale => _currentScale;
        public float CurrentOpacity => _currentOpacity;

        public event Action<CustomizableControlButton> OnSelectedInEditMode;
        public event Action<CustomizableControlButton, Vector2> OnPositionChangedInEditMode;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _defaultAnchoredPosition = _rectTransform.anchoredPosition;
            _defaultScale = _rectTransform.localScale;
            _defaultOpacity = _canvasGroup.alpha;

            _currentScale = _defaultScale;
            _currentOpacity = _defaultOpacity;

            if (string.IsNullOrEmpty(_controlId))
            {
                _controlId = gameObject.name;
            }

            CreateSelectionHighlight();
        }

        private void CreateSelectionHighlight()
        {
            if (_selectionHighlight != null) return;

            GameObject hlObj = new GameObject("EditMode_SelectionHighlight", typeof(RectTransform), typeof(Image));
            hlObj.transform.SetParent(transform, false);
            hlObj.transform.SetAsLastSibling();

            RectTransform hlRT = hlObj.GetComponent<RectTransform>();
            hlRT.anchorMin = Vector2.zero;
            hlRT.anchorMax = Vector2.one;
            hlRT.offsetMin = new Vector2(-6, -6);
            hlRT.offsetMax = new Vector2(6, 6);

            _selectionHighlight = hlObj.GetComponent<Image>();
            _selectionHighlight.color = new Color(0.15f, 0.95f, 0.45f, 0.65f); // Xanh neon phát sáng viền
            _selectionHighlight.raycastTarget = false;
            hlObj.SetActive(false);
        }

        public void ApplyLayout(ControlButtonLayoutData data)
        {
            if (data == null) return;

            if (RectTransform != null)
            {
                RectTransform.anchoredPosition = data.anchoredPosition;
                float s = Mathf.Clamp(data.scale, _minScale, _maxScale);
                RectTransform.localScale = new Vector3(s, s, 1f);
                _currentScale = new Vector2(s, s);
            }

            if (_canvasGroup != null)
            {
                float op = Mathf.Clamp(data.opacity, 0.3f, 1.0f);
                _canvasGroup.alpha = op;
                _currentOpacity = op;
            }
        }

        public ControlButtonLayoutData ExportCurrentLayout()
        {
            return new ControlButtonLayoutData(
                ControlId,
                RectTransform.anchoredPosition,
                _currentScale.x,
                _currentOpacity
            );
        }

        public void SetScale(float scaleVal)
        {
            float s = Mathf.Clamp(scaleVal, _minScale, _maxScale);
            _currentScale = new Vector2(s, s);
            if (RectTransform != null)
            {
                RectTransform.localScale = new Vector3(s, s, 1f);
            }
        }

        public void SetOpacity(float opacityVal)
        {
            _currentOpacity = Mathf.Clamp(opacityVal, 0.3f, 1.0f);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _currentOpacity;
            }
        }

        public void ResetToDefault()
        {
            if (RectTransform != null)
            {
                RectTransform.anchoredPosition = _defaultAnchoredPosition;
                RectTransform.localScale = _defaultScale;
                _currentScale = _defaultScale;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _defaultOpacity;
                _currentOpacity = _defaultOpacity;
            }
        }

        private void OnDestroy()
        {
            if (_isEditMode)
            {
                _isEditMode = false;
                s_ActiveEditModeCount = Mathf.Max(0, s_ActiveEditModeCount - 1);
            }
        }

        public void SetEditMode(bool enable)
        {
            if (_isEditMode != enable)
            {
                _isEditMode = enable;
                if (enable)
                {
                    s_ActiveEditModeCount++;
                }
                else
                {
                    s_ActiveEditModeCount = Mathf.Max(0, s_ActiveEditModeCount - 1);
                }
            }

            if (_selectionHighlight != null)
            {
                _selectionHighlight.gameObject.SetActive(false);
            }

            // Tắt hoàn toàn việc kích hoạt đòn đánh / skill / joystick khi đang ở chế độ tùy chỉnh
            var dragHandlers = GetComponentsInChildren<SmartSkillDragHandler>(true);
            foreach (var dh in dragHandlers)
            {
                dh.SetInteractable(!enable);
                dh.enabled = !enable;
            }

            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                btn.interactable = !enable;
            }

            var sigViews = GetComponentsInChildren<SignatureSkillButtonView>(true);
            foreach (var sig in sigViews)
            {
                sig.SetInteractable(!enable);
            }

            var relicViews = GetComponentsInChildren<RelicSkillButtonView>(true);
            foreach (var relic in relicViews)
            {
                relic.SetInteractable(!enable);
            }

            var dashViews = GetComponentsInChildren<DashButtonView>(true);
            foreach (var dash in dashViews)
            {
                dash.SetInteractable(!enable);
            }

            var virtualJoysticks = GetComponentsInChildren<DynamicVirtualJoystick>(true);
            foreach (var vj in virtualJoysticks)
            {
                vj.enabled = !enable;
            }

            var onScreenControls = GetComponentsInChildren<UnityEngine.InputSystem.OnScreen.OnScreenControl>(true);
            foreach (var osc in onScreenControls)
            {
                osc.enabled = !enable;
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (_selectionHighlight != null)
            {
                _selectionHighlight.gameObject.SetActive(_isEditMode && isSelected);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isEditMode) return;
            eventData.Use();
            OnSelectedInEditMode?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isEditMode || RectTransform == null) return;
            eventData.Use();

            RectTransform parentRect = RectTransform.parent as RectTransform;
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                // Clamp trong phạm vi Safe Area màn hình
                Vector2 clampedPos = ClampToParentBounds(parentRect, localPoint);
                RectTransform.localPosition = clampedPos;
                OnPositionChangedInEditMode?.Invoke(this, RectTransform.anchoredPosition);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isEditMode) return;
        }

        private Vector2 ClampToParentBounds(RectTransform parent, Vector2 pos)
        {
            if (parent == null) return pos;

            Rect parentBounds = parent.rect;
            float halfWidth = (RectTransform.rect.width * _currentScale.x) * 0.5f;
            float halfHeight = (RectTransform.rect.height * _currentScale.y) * 0.5f;

            float minX = parentBounds.xMin + halfWidth + 10f;
            float maxX = parentBounds.xMax - halfWidth - 10f;
            float minY = parentBounds.yMin + halfHeight + 10f;
            float maxY = parentBounds.yMax - halfHeight - 10f;

            return new Vector2(
                Mathf.Clamp(pos.x, minX, maxX),
                Mathf.Clamp(pos.y, minY, maxY)
            );
        }
    }
}

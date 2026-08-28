using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectZombie.Features.UI.Controls
{
    /// <summary>
    /// Component xử lý cảm ứng Kéo Thả Định Hướng Kép (Smart Quick-Tap & Drag-Aim) cho mọi nút Skill UI.
    /// Chuẩn hóa theo phong cách điều khiển MOBA Liên Quân Mobile / Wild Rift.
    /// </summary>
    public class SmartSkillDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Drag Sensitivity")]
        [Tooltip("Bán kính tối thiểu (pixel) để chuyển từ Quick Tap sang Drag Aim")]
        [SerializeField] private float _dragThreshold = 25f;
        [Tooltip("Khoảng cách kéo tối đa (pixel) đạt 100% tầm chiêu")]
        [SerializeField] private float _maxDragDistance = 140f;
        [Tooltip("Nếu true: Chỉ hiện chỉ dấu khi ĐÈ (Hold > 0.12s) hoặc KÉO (Drag). Nhấp nhanh (Tap) sẽ đánh ngay mà không chớp chỉ dấu.")]
        [SerializeField] private bool _requireHoldOrDrag = true;
        [SerializeField] private float _holdDurationThreshold = 0.12f;

        private Vector2 _pointerDownPos;
        private Vector2 _currentPointerPos;
        private bool _isDragging;
        private bool _isAimActive;
        private bool _isInteractable = true;
        private Coroutine _holdCoroutine;

        public bool IsDragging => _isDragging;
        public bool RequireHoldOrDrag
        {
            get => _requireHoldOrDrag;
            set => _requireHoldOrDrag = value;
        }

        public event Action OnAimStarted;
        public event Action<Vector2, float, bool> OnAimUpdated; // (direction, pullPercent, isCancelHovered)
        public event Action<Vector2, bool> OnAimReleased;      // (direction, isQuickTap)
        public event Action OnAimCancelled;

        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
            if (!interactable && _isAimActive)
            {
                CancelAim();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            _pointerDownPos = eventData.position;
            _currentPointerPos = eventData.position;
            _isDragging = false;
            _isAimActive = false;

            if (_holdCoroutine != null) StopCoroutine(_holdCoroutine);

            if (_requireHoldOrDrag)
            {
                _holdCoroutine = StartCoroutine(RoutineCheckHold());
            }
            else
            {
                TriggerAimStarted();
            }
        }

        private IEnumerator RoutineCheckHold()
        {
            yield return new WaitForSecondsRealtime(_holdDurationThreshold);
            if (!_isAimActive)
            {
                TriggerAimStarted();
            }
            _holdCoroutine = null;
        }

        private void TriggerAimStarted()
        {
            _isAimActive = true;
            OnAimStarted?.Invoke();
            UICancelSkillZone.Instance?.SetVisible(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            _currentPointerPos = eventData.position;
            Vector2 delta = _currentPointerPos - _pointerDownPos;
            float dist = delta.magnitude;

            if (dist >= _dragThreshold)
            {
                _isDragging = true;
                if (!_isAimActive)
                {
                    if (_holdCoroutine != null)
                    {
                        StopCoroutine(_holdCoroutine);
                        _holdCoroutine = null;
                    }
                    TriggerAimStarted();
                }
            }

            if (_isAimActive)
            {
                Vector2 aimDirection = dist > 0.01f ? delta.normalized : Vector2.zero;
                float pullPercent = Mathf.Clamp01(dist / _maxDragDistance);

                // Kiểm tra ngón tay kéo vào Vùng Hủy Chiêu
                bool isCancel = UICancelSkillZone.Instance != null && 
                                UICancelSkillZone.Instance.IsPointerInsideCancelZone(eventData.position, eventData.pressEventCamera);

                UICancelSkillZone.Instance?.SetHovered(isCancel);
                OnAimUpdated?.Invoke(aimDirection, pullPercent, isCancel);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }

            _currentPointerPos = eventData.position;
            Vector2 delta = _currentPointerPos - _pointerDownPos;
            float dist = delta.magnitude;

            bool isCancel = _isAimActive && UICancelSkillZone.Instance != null && 
                            UICancelSkillZone.Instance.IsPointerInsideCancelZone(eventData.position, eventData.pressEventCamera);

            if (_isAimActive)
            {
                UICancelSkillZone.Instance?.SetVisible(false);
            }

            if (isCancel)
            {
                OnAimCancelled?.Invoke();
            }
            else
            {
                bool isQuickTap = dist < _dragThreshold && !_isAimActive;
                Vector2 finalDirection = dist > 0.01f ? delta.normalized : Vector2.zero;
                OnAimReleased?.Invoke(finalDirection, isQuickTap);
            }

            _isDragging = false;
            _isAimActive = false;
        }

        private void CancelAim()
        {
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }
            _isDragging = false;
            _isAimActive = false;
            UICancelSkillZone.Instance?.SetVisible(false);
            OnAimCancelled?.Invoke();
        }
    }
}

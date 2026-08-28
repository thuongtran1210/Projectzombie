using System;
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

        private Vector2 _pointerDownPos;
        private Vector2 _currentPointerPos;
        private bool _isDragging;
        private bool _isInteractable = true;

        public bool IsDragging => _isDragging;

        public event Action OnAimStarted;
        public event Action<Vector2, float, bool> OnAimUpdated; // (direction, pullPercent, isCancelHovered)
        public event Action<Vector2, bool> OnAimReleased;      // (direction, isQuickTap)
        public event Action OnAimCancelled;

        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            _pointerDownPos = eventData.position;
            _currentPointerPos = eventData.position;
            _isDragging = false;

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
            }

            Vector2 aimDirection = dist > 0.01f ? delta.normalized : Vector2.zero;
            float pullPercent = Mathf.Clamp01(dist / _maxDragDistance);

            // Kiểm tra ngón tay kéo vào Vùng Hủy Chiêu
            bool isCancel = UICancelSkillZone.Instance != null && 
                            UICancelSkillZone.Instance.IsPointerInsideCancelZone(eventData.position, eventData.pressEventCamera);

            UICancelSkillZone.Instance?.SetHovered(isCancel);
            OnAimUpdated?.Invoke(aimDirection, pullPercent, isCancel);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            _currentPointerPos = eventData.position;
            Vector2 delta = _currentPointerPos - _pointerDownPos;
            float dist = delta.magnitude;

            bool isCancel = UICancelSkillZone.Instance != null && 
                            UICancelSkillZone.Instance.IsPointerInsideCancelZone(eventData.position, eventData.pressEventCamera);

            UICancelSkillZone.Instance?.SetVisible(false);

            if (isCancel)
            {
                OnAimCancelled?.Invoke();
            }
            else
            {
                bool isQuickTap = dist < _dragThreshold;
                Vector2 finalDirection = dist > 0.01f ? delta.normalized : Vector2.zero;
                OnAimReleased?.Invoke(finalDirection, isQuickTap);
            }

            _isDragging = false;
        }
    }
}

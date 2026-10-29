using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectZombie.Features.UI.Controls
{
    /// <summary>Legacy 2D event adapter over the shared engine-free pointer policy.</summary>
    public class SmartSkillDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler,
        IInitializePotentialDragHandler
    {
        [SerializeField] private float _dragThreshold = 25f;
        [SerializeField] private float _maxDragDistance = 140f;
        [SerializeField] private bool _requireHoldOrDrag = true;
        [SerializeField] private float _holdDurationThreshold = 0.12f;
        private SkillPointerGesture _gesture;
        private bool _isInteractable = true;

        public bool IsDragging => _gesture != null && _gesture.IsDragging;
        public bool RequireHoldOrDrag
        {
            get => _requireHoldOrDrag;
            set
            {
                if (_requireHoldOrDrag == value) return;
                CancelAim();
                _requireHoldOrDrag = value;
                _gesture = null;
            }
        }

        public event Action OnAimStarted;
        public event Action<Vector2, float, bool> OnAimUpdated;
        public event Action<Vector2, bool> OnAimReleased;
        public event Action<Vector2, float, bool> OnAimDetailedReleased;
        public event Action OnAimCancelled;

        private bool CanInteract => isActiveAndEnabled && _isInteractable &&
            !Customization.CustomizableControlButton.IsAnyInEditMode;
        private SkillPointerGesture Gesture => _gesture ?? (_gesture = new SkillPointerGesture(
            _dragThreshold, _maxDragDistance, _holdDurationThreshold, _requireHoldOrDrag));

        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
            if (!interactable) CancelAim();
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanInteract || eventData.button != PointerEventData.InputButton.Left) return;
            Dispatch(Gesture.Press(eventData.pointerId, eventData.position.x, eventData.position.y, Time.unscaledTimeAsDouble));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!CanInteract) { CancelAim(); return; }
            Dispatch(Gesture.Move(eventData.pointerId, eventData.position.x, eventData.position.y,
                Time.unscaledTimeAsDouble, IsCancel(eventData)));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!CanInteract) { CancelAim(); return; }
            if (_gesture == null) return;
            Dispatch(_gesture.Release(eventData.pointerId, eventData.position.x, eventData.position.y,
                Time.unscaledTimeAsDouble, IsCancel(eventData)));
        }

        private void Update()
        {
            if (_gesture == null || !_gesture.IsPressed) return;
            if (!CanInteract) { CancelAim(); return; }
            Dispatch(_gesture.Tick(Time.unscaledTimeAsDouble));
        }

        private static bool IsCancel(PointerEventData eventData) => UICancelSkillZone.Instance != null &&
            UICancelSkillZone.Instance.IsPointerInsideCancelZone(eventData.position, eventData.pressEventCamera);

        private void Dispatch(SkillPointerSample sample)
        {
            if (sample.Signal == SkillPointerSignal.None || sample.Signal == SkillPointerSignal.Pressed) return;
            if (sample.AimStarted)
            {
                UICancelSkillZone.Instance?.SetVisible(true);
                OnAimStarted?.Invoke();
                if (_gesture == null || !_gesture.IsPressed) return;
            }
            if (sample.Signal == SkillPointerSignal.Aiming)
            {
                UICancelSkillZone.Instance?.SetHovered(sample.CancelHovered);
                OnAimUpdated?.Invoke(new Vector2(sample.DirectionX, sample.DirectionY), sample.Pull, sample.CancelHovered);
                return;
            }
            UICancelSkillZone.Instance?.SetVisible(false);
            if (sample.Signal == SkillPointerSignal.Cancelled)
            {
                OnAimCancelled?.Invoke();
                return;
            }
            bool quickTap = sample.Signal == SkillPointerSignal.TapReleased;
            var direction = new Vector2(sample.DirectionX, sample.DirectionY);
            OnAimReleased?.Invoke(direction, quickTap);
            OnAimDetailedReleased?.Invoke(direction, sample.Pull, quickTap);
        }

        public void CancelAim()
        {
            if (_gesture != null) Dispatch(_gesture.Cancel());
        }

        private void OnDisable() => CancelAim();
        private void OnApplicationFocus(bool focused) { if (!focused) CancelAim(); }
        private void OnApplicationPause(bool paused) { if (paused) CancelAim(); }
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectZombie.Features.UI.Controls;

namespace ProjectZombie.Features.Defense3D.UI
{
    public readonly struct DefenseAimSubmission
    {
        public readonly DefenseResolvedAim Aim;
        public readonly int RunGeneration;
        public readonly object SessionIdentity;
        public readonly bool IsQuickTap;

        internal DefenseAimSubmission(DefenseResolvedAim aim, DefenseSkillGestureSample sample)
        {
            Aim = aim;
            RunGeneration = sample.RunGeneration;
            SessionIdentity = sample.SessionIdentity;
            IsQuickTap = sample.Pointer.Signal == SkillPointerSignal.TapReleased;
        }
    }

    /// <summary>Authored pointer/aim adapter. Emits scoped requests; never executes effects or spends cooldown.</summary>
    public sealed class DefenseSkillAimButton3D : MonoBehaviour, IPointerDownHandler, IDragHandler,
        IPointerUpHandler, IInitializePotentialDragHandler
    {
        [SerializeField] private DefenseAimProjector3D _projector;
        [SerializeField] private DefenseHeroHealth3D _heroHealth;
        [SerializeField] private RectTransform _cancelZone;
        [SerializeField] private DefenseSkillStickView _stick;
        [SerializeField, Min(1f)] private float _dragThreshold = 6f;
        [SerializeField, Min(1f)] private float _maxDragDistance = 52f;
        [SerializeField, Min(0f)] private float _initialAimDistance = 3f;
        [SerializeField, Min(0f)] private float _minimumDragDistance = 1.5f;
        [SerializeField, Min(0f)] private float _cancelPaddingPixels = 12f;
        private int _pointerId;
        private Vector2 _pressPosition, _pointerPosition;
        private Camera _pressCamera;
        private DefenseSkillGestureSession _gesture;
        private IDefenseGameplayContext _gameplay;
        private float _range;
        private bool _autoTargetOnTap, _showingCancel, _paused, _selfTarget;
        private int _bindingVersion;
        private bool _interactable;
        private DefenseTargetMask _targetMask = DefenseTargetMask.All;
        public bool IsConfigured => _projector != null && _projector.IsConfigured && _heroHealth != null && _cancelZone != null &&
            _stick != null && _stick.IsConfigured;

        public event Action<DefenseResolvedAim, bool> PreviewChanged;
        public event Action PreviewCleared;
        public event Action<DefenseAimSubmission> AimReleased;

        public void Bind(IDefenseGameplayContext gameplay, DefenseAbilityRuntime ability, float range, bool autoTargetOnTap,
            DefenseTargetMask targetMask = DefenseTargetMask.All, bool selfTarget = false)
        {
            Unbind();
            if (!IsConfigured ||
                !DefenseDamageRules.IsFinite(range) || range < 0f)
                throw new InvalidOperationException("Assign projector/camera/ground/origin, Hero Health, thumb pad and cancel-zone refs before binding skill controls.");
            var pointer = new SkillPointerGesture(_dragThreshold, _maxDragDistance, 0d, false, true);
            _gesture = new DefenseSkillGestureSession(gameplay, ability, pointer);
            _gameplay = gameplay;
            _range = range;
            _autoTargetOnTap = autoTargetOnTap;
            _targetMask = targetMask;
            _selfTarget = selfTarget;
            _gesture.Changed += OnSample;
            _heroHealth.Defeated += Cancel;
            RefreshAvailability();
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_gesture == null || _gesture.IsPressed) return;
            _pointerId = eventData.pointerId;
            _pressPosition = _pointerPosition = eventData.position;
            _pressCamera = eventData.pressEventCamera;
            RefreshAvailability();
            _gesture?.Press(eventData.pointerId, eventData.position.x, eventData.position.y, Time.unscaledTimeAsDouble);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId) return;
            _pointerPosition = eventData.position;
            RefreshAvailability();
            _gesture?.Move(eventData.pointerId, eventData.position.x, eventData.position.y, Time.unscaledTimeAsDouble, InCancelZone(eventData));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            RefreshAvailability();
            _gesture?.Release(eventData.pointerId, eventData.position.x, eventData.position.y, Time.unscaledTimeAsDouble, InCancelZone(eventData));
        }

        private bool InCancelZone(PointerEventData eventData)
        {
            if (_cancelZone == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_cancelZone,
                eventData.position, eventData.pressEventCamera, out var local)) return false;
            float radius = Mathf.Min(_cancelZone.rect.width, _cancelZone.rect.height) * 0.5f +
                _cancelPaddingPixels / Mathf.Max(0.001f, Mathf.Abs(_cancelZone.lossyScale.x));
            return (local - _cancelZone.rect.center).sqrMagnitude <= radius * radius;
        }

        private void Update()
        {
            RefreshAvailability();
            _gesture?.Tick(Time.unscaledTimeAsDouble);
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            RefreshAvailability();
        }

        private void RefreshAvailability() => _gesture?.SetAvailable(_interactable && isActiveAndEnabled && Application.isFocused && !_paused &&
            _projector != null && _projector.IsConfigured && _heroHealth != null && _heroHealth.isActiveAndEnabled && !_heroHealth.IsDown);

        private void OnSample(DefenseSkillGestureSample sample)
        {
            int bindingVersion = _bindingVersion;
            if (sample.Pointer.Signal == SkillPointerSignal.Pressed) return;
            if (sample.Pointer.Signal == SkillPointerSignal.Aiming)
            {
                ShowCancelZone(true);
                if (sample.Pointer.AimStarted) _stick.Begin(_pressPosition, _pressCamera);
                _stick.Move(_pointerPosition, _maxDragDistance, sample.Pointer.CancelHovered);
                DefenseResolvedAim preview;
                bool validSurface;
                bool resolved;
                if (_selfTarget) { resolved = _projector.TrySelf(out preview); validSurface = resolved; }
                else resolved = _gesture.IsDragging ?
                    _projector.TryPreviewDrag(sample.Pointer.DirectionX, sample.Pointer.DirectionY, sample.Pointer.Pull,
                        _range, out preview, out validSurface, _minimumDragDistance) :
                    _projector.TryPreviewDefault(_range, _initialAimDistance, out preview, out validSurface);
                if (resolved)
                    PreviewChanged?.Invoke(preview, sample.Pointer.CancelHovered || !validSurface);
                else PreviewCleared?.Invoke();
                return;
            }
            ShowCancelZone(false);
            _stick?.Hide();
            PreviewCleared?.Invoke();
            // Presentation callbacks may pause, disable, rebind or restart before a release is dispatched.
            if (bindingVersion != _bindingVersion || !isActiveAndEnabled || _gameplay == null || !_gameplay.AdvancesGameplay ||
                sample.RunGeneration != _gameplay.RunGeneration || !ReferenceEquals(sample.SessionIdentity, _gameplay.SessionIdentity) ||
                _heroHealth == null || _heroHealth.IsDown || sample.Pointer.Signal == SkillPointerSignal.Cancelled) return;
            DefenseResolvedAim aim;
            bool valid = _selfTarget ? _projector.TrySelf(out aim) : sample.Pointer.Signal == SkillPointerSignal.TapReleased ?
                _projector.TryTap(_gameplay.Targets, _range, _autoTargetOnTap, out aim, _targetMask) :
                _projector.TryDrag(sample.Pointer.DirectionX, sample.Pointer.DirectionY, sample.Pointer.Pull,
                    _range, out aim, _minimumDragDistance);
            if (valid) AimReleased?.Invoke(new DefenseAimSubmission(aim, sample));
        }

        private void ShowCancelZone(bool visible)
        {
            if (_cancelZone == null || (!visible && !_showingCancel)) return;
            _showingCancel = visible;
            _cancelZone.gameObject.SetActive(visible);
        }

        public bool TryResolveTap(out DefenseResolvedAim aim)
        {
            aim = default;
            return _interactable && _gesture != null && !_gesture.IsPressed && _gameplay != null && _gameplay.AdvancesGameplay &&
                Application.isFocused && !_paused && (_selfTarget ? _projector.TrySelf(out aim) : _projector.TryTap(_gameplay.Targets, _range, _autoTargetOnTap, out aim, _targetMask));
        }

        public void Cancel() => _gesture?.Cancel();

        public void Unbind()
        {
            _bindingVersion++;
            if (_heroHealth != null) _heroHealth.Defeated -= Cancel;
            if (_gesture != null)
            {
                _gesture.Changed -= OnSample;
                _gesture.Dispose();
                _gesture = null;
            }
            _gameplay = null;
            _stick?.Hide();
            ShowCancelZone(false);
            PreviewCleared?.Invoke();
        }

        private void OnDisable() => _gesture?.SetAvailable(false);
        private void OnDestroy() => Unbind();
        private void OnApplicationFocus(bool focused) { if (!focused) Cancel(); }
        private void OnApplicationPause(bool paused) { _paused = paused; if (paused) Cancel(); }
    }
}

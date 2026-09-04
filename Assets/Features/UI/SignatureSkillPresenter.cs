using UnityEngine;
using ProjectZombie.Features.Player.Skills;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter kết nối giữa SignatureSkillManager (Model) và các View UI (SignatureSkillButtonView, ThuSinhElementPickerOverlayView).
    /// Tuân thủ Mô hình MVP (Section 12 Rules): Đảm bảo quản lý vòng đời subscribe/unsubscribe chuẩn mực.
    /// </summary>
    public class SignatureSkillPresenter : MonoBehaviour
    {
        [Header("View References")]
        [SerializeField] private SignatureSkillButtonView _buttonView;
        [SerializeField] private ThuSinhElementPickerOverlayView _elementPickerOverlayView;

        [Header("Model References")]
        [SerializeField] private SignatureSkillManager _skillManager;

        public void Bind(SignatureSkillManager manager)
        {
            if (_skillManager != null)
            {
                _skillManager.OnCooldownUpdated -= OnCooldownUpdated;
                _skillManager.OnSkillReady -= OnSkillReady;
                _skillManager.OnSkillExecuted -= OnSkillExecuted;
            }

            _skillManager = manager;

            if (_skillManager != null)
            {
                _skillManager.OnCooldownUpdated += OnCooldownUpdated;
                _skillManager.OnSkillReady += OnSkillReady;
                _skillManager.OnSkillExecuted += OnSkillExecuted;
            }

            RefreshUIState();
        }

        private void Start()
        {
            if (_skillManager == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Bind(player.GetComponent<SignatureSkillManager>());
                }
            }

            if (_buttonView != null)
            {
                _buttonView.OnButtonClicked += OnButtonClicked;
                _buttonView.OnAimStarted += HandleAimStarted;
                _buttonView.OnAimUpdated += HandleAimUpdated;
                _buttonView.OnAimReleased += HandleAimReleased;
                _buttonView.OnAimCancelled += HandleAimCancelled;
            }

            if (_elementPickerOverlayView != null)
            {
                _elementPickerOverlayView.OnElementPicked += OnElementPickedFromOverlay;
            }

            RefreshUIState();
        }

        private void OnDestroy()
        {
            if (_skillManager != null)
            {
                _skillManager.OnCooldownUpdated -= OnCooldownUpdated;
                _skillManager.OnSkillReady -= OnSkillReady;
                _skillManager.OnSkillExecuted -= OnSkillExecuted;
            }

            if (_buttonView != null)
            {
                _buttonView.OnButtonClicked -= OnButtonClicked;
                _buttonView.OnAimStarted -= HandleAimStarted;
                _buttonView.OnAimUpdated -= HandleAimUpdated;
                _buttonView.OnAimReleased -= HandleAimReleased;
                _buttonView.OnAimCancelled -= HandleAimCancelled;
            }

            if (_elementPickerOverlayView != null)
            {
                _elementPickerOverlayView.OnElementPicked -= OnElementPickedFromOverlay;
            }
        }

        private void HandleAimStarted()
        {
            if (_skillManager == null || !_skillManager.IsReady) return;

            var aimConfig = (_skillManager.ActiveSkill is Combat.Aiming.IAimableSkill aimable)
                ? aimable.AimConfig
                : new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.ConeSector, 5.5f, 2.5f, 120f, true);

            Combat.Aiming.SkillAimIndicatorController.Instance?.StartAim(aimConfig);
        }

        private void HandleAimUpdated(Vector2 direction, float pullPercent, bool isCancel)
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.UpdateAim(direction, pullPercent, isCancel);
        }

        private void HandleAimReleased(Vector2 direction, bool isQuickTap)
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.StopAim();
            OnButtonClicked();
        }

        private void HandleAimCancelled()
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.StopAim();
        }

        private void RefreshUIState()
        {
            if (_skillManager == null || _buttonView == null) return;

            bool canExecute = _skillManager.CanExecuteCurrentSkill() && _skillManager.RemainingCooldown <= 0f;
            _buttonView.SetInteractable(canExecute);

            float rem = _skillManager.RemainingCooldown;
            float max = _skillManager.MaxCooldown;
            string text = rem > 0f ? $"{Mathf.CeilToInt(rem)}s" : string.Empty;
            _buttonView.SetCooldown(rem, max, text);
        }

        private void OnCooldownUpdated(float remaining, float max)
        {
            if (_buttonView == null) return;
            string text = remaining > 0f ? $"{Mathf.CeilToInt(remaining)}s" : string.Empty;
            _buttonView.SetCooldown(remaining, max, text);

            bool canExecute = _skillManager != null && _skillManager.CanExecuteCurrentSkill() && remaining <= 0f;
            _buttonView.SetInteractable(canExecute);
        }

        private void OnSkillReady()
        {
            RefreshUIState();
        }

        private void OnSkillExecuted()
        {
            RefreshUIState();
        }

        private void OnButtonClicked()
        {
            if (_skillManager == null) return;

            if (!_skillManager.IsReady || _skillManager.RemainingCooldown > 0f)
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIError();
                return;
            }

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();

            // Thi triển trực tiếp kỹ năng (đồng bộ cho cả 4 nhân vật)
            _skillManager.TryExecuteSkill();
        }

        private void OnElementPickedFromOverlay(ElementType selectedElement)
        {
            if (_skillManager == null) return;

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();

            // Nếu người chơi tự chọn hoặc hết 1.5s timeout, thi triển skill với hệ tương ứng
            if (selectedElement == ElementType.None)
            {
                var thuSinhSkill = _skillManager.ActiveSkill as ThuSinhSignatureSkill;
                if (thuSinhSkill != null && _skillManager.gameObject != null)
                {
                    selectedElement = thuSinhSkill.GetAutoSelectFallbackElement(_skillManager.gameObject);
                }
            }

            var activeThuSinhSkill = _skillManager.ActiveSkill as ThuSinhSignatureSkill;
            if (activeThuSinhSkill != null)
            {
                activeThuSinhSkill.ApplyVirtualElementHit(selectedElement);
            }

            _skillManager.TryExecuteSkill();
        }
    }
}

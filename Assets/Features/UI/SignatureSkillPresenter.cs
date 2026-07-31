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

        private void Start()
        {
            if (_skillManager == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _skillManager = player.GetComponent<SignatureSkillManager>();
                }
            }

            if (_skillManager != null)
            {
                _skillManager.OnCooldownUpdated += OnCooldownUpdated;
                _skillManager.OnSkillReady += OnSkillReady;
                _skillManager.OnSkillExecuted += OnSkillExecuted;
            }

            if (_buttonView != null)
            {
                _buttonView.OnButtonClicked += OnButtonClicked;
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
            }

            if (_elementPickerOverlayView != null)
            {
                _elementPickerOverlayView.OnElementPicked -= OnElementPickedFromOverlay;
            }
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
            if (_skillManager == null || !_skillManager.IsReady) return;

            // Nếu là skill của Thư Sinh: Hiển thị Overlay chọn hệ 1.5s
            if (_skillManager.ActiveSkill is ThuSinhSignatureSkill && _elementPickerOverlayView != null)
            {
                _elementPickerOverlayView.ShowOverlay();
            }
            else
            {
                // Đạo Sĩ hoặc Võ Tăng thi triển trực tiếp
                _skillManager.TryExecuteSkill();
            }
        }

        private void OnElementPickedFromOverlay(ElementType selectedElement)
        {
            if (_skillManager == null) return;

            // Nếu người chơi tự chọn hoặc hết 1.5s timeout, thi triển skill với hệ tương ứng
            if (selectedElement == ElementType.None)
            {
                var thuSinhSkill = _skillManager.ActiveSkill as ThuSinhSignatureSkill;
                if (thuSinhSkill != null && _skillManager.gameObject != null)
                {
                    selectedElement = thuSinhSkill.GetAutoSelectFallbackElement(_skillManager.gameObject);
                }
            }

            _skillManager.TryExecuteSkill((picked) =>
            {
                var thuSinhSkill = _skillManager.ActiveSkill as ThuSinhSignatureSkill;
                if (thuSinhSkill != null)
                {
                    thuSinhSkill.ApplyVirtualElementHit(selectedElement);
                }
            });
        }
    }
}

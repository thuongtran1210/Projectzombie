using UnityEngine;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View quản lý hiển thị chung của Bảng lựa chọn Nâng cấp (Upgrade Panel).
    /// </summary>
    public class UpgradeUIView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private UpgradeCardView[] _upgradeCards; // Thường là 3 card

        [Header("Optional Roguelite Controls")]
        [SerializeField] private UnityEngine.UI.Button _rerollButton;
        [SerializeField] private UnityEngine.UI.Button _skipButton;
        [SerializeField] private TMPro.TextMeshProUGUI _rerollCountText;

        private System.Action _onRerollClicked;
        private System.Action _onSkipClicked;

        private void Awake()
        {
            if (_upgradePanel != null)
            {
                _upgradePanel.SetActive(false);
            }

            if (_rerollButton != null)
            {
                _rerollButton.onClick.AddListener(() => _onRerollClicked?.Invoke());
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(() => _onSkipClicked?.Invoke());
            }
        }

        public void SetRerollButtonCallback(System.Action onReroll)
        {
            _onRerollClicked = onReroll;
            if (_rerollButton != null)
            {
                _rerollButton.gameObject.SetActive(onReroll != null);
            }
        }

        public void SetSkipButtonCallback(System.Action onSkip)
        {
            _onSkipClicked = onSkip;
            if (_skipButton != null)
            {
                _skipButton.gameObject.SetActive(onSkip != null);
            }
        }

        public void SetRerollCountText(string text)
        {
            if (_rerollCountText != null)
            {
                _rerollCountText.text = text;
            }
        }

        public void SetRerollInteractable(bool interactable)
        {
            if (_rerollButton != null)
            {
                _rerollButton.interactable = interactable;
            }
        }

        public void SetActive(bool isActive)
        {
            if (_upgradePanel == null)
            {
                Debug.LogWarning($"[{nameof(UpgradeUIView)}] _upgradePanel chưa được gán trong Inspector.");
                return;
            }
            _upgradePanel.SetActive(isActive);
        }

        public int GetCardsLength()
        {
            return _upgradeCards != null ? _upgradeCards.Length : 0;
        }

        /// <summary>
        /// Lấy tham chiếu đến một Card View cụ thể theo index.
        /// </summary>
        public UpgradeCardView GetCardView(int index)
        {
            if (_upgradeCards == null)
            {
                Debug.LogWarning($"[{nameof(UpgradeUIView)}] _upgradeCards chưa được gán trong Inspector.");
                return null;
            }

            if (index < 0 || index >= _upgradeCards.Length)
            {
                return null;
            }
            return _upgradeCards[index];
        }
    }
}

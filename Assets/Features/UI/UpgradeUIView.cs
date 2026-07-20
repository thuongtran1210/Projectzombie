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

        private void Awake()
        {
            if (_upgradePanel != null)
            {
                _upgradePanel.SetActive(false);
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

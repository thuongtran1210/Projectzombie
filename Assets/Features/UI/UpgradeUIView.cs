using UnityEngine;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View quản lý hiển thị chung của Bảng lựa chọn Nâng cấp (Upgrade Panel).
    /// </summary>
    public class UpgradeUIView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private UpgradeCardView[] upgradeCards; // Thường là 3 card

        private void Awake()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
        }

        public void SetActive(bool isActive)
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(isActive);
            }
        }

        public int GetCardsLength()
        {
            return upgradeCards != null ? upgradeCards.Length : 0;
        }

        /// <summary>
        /// Lấy tham chiếu đến một Card View cụ thể theo index.
        /// </summary>
        public UpgradeCardView GetCardView(int index)
        {
            if (upgradeCards == null || index < 0 || index >= upgradeCards.Length)
            {
                return null;
            }
            return upgradeCards[index];
        }
    }
}

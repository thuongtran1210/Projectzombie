using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View giao diện Cây Nâng Cấp Vĩnh Viễn tiêu Cổ Tiền (Meta Upgrade Shop).
    /// </summary>
    public class MetaUpgradeShopView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.SanctuaryTree;

        [SerializeField] private TextMeshProUGUI _coTienBalanceText;
        [SerializeField] private TextMeshProUGUI _upgradeTitleText;
        [SerializeField] private TextMeshProUGUI _upgradeCostText;
        [SerializeField] private TextMeshProUGUI _upgradeLevelText;
        [SerializeField] private Button _buyUpgradeButton;
        [SerializeField] private Button _closeButton;

        public event Action OnBuyUpgradeClicked;
        public event Action OnCloseClicked;

        protected override void Awake()
        {
            base.Awake();
            if (_buyUpgradeButton != null) _buyUpgradeButton.onClick.AddListener(() => OnBuyUpgradeClicked?.Invoke());
            if (_closeButton != null) _closeButton.onClick.AddListener(() => {
                OnCloseClicked?.Invoke();
                OnBackPressed();
            });
        }

        public void SetCoTienBalance(string formattedBalance)
        {
            if (_coTienBalanceText != null) _coTienBalanceText.text = formattedBalance;
        }

        public void DisplayUpgradeDetails(string title, string formattedCost, string formattedLevel, bool canAfford)
        {
            if (_upgradeTitleText != null) _upgradeTitleText.text = title;
            if (_upgradeCostText != null) _upgradeCostText.text = formattedCost;
            if (_upgradeLevelText != null) _upgradeLevelText.text = formattedLevel;
            if (_buyUpgradeButton != null) _buyUpgradeButton.interactable = canAfford;
        }
    }
}

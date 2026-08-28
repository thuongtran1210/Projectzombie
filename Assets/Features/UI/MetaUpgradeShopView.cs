using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    [System.Serializable]
    public class UpgradeNodeCardItem
    {
        public GameObject rootObject;
        public Button selectButton;
        public Image iconImage;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI levelText;
        public Image selectionHighlight;
        public Image iconFrame;
    }

    /// <summary>
    /// Passive View giao diện Miếu Tứ Bất Tử (Meta Upgrade Shop) 3 Nhánh Thần Linh Cổ Phong.
    /// </summary>
    public class MetaUpgradeShopView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.SanctuaryTree;

        [Header("Header & Currency")]
        [SerializeField] private TextMeshProUGUI _coTienBalanceText;
        [SerializeField] private Button _closeButton;

        [Header("3 Branch Tabs")]
        [SerializeField] private Button _tabTanVienButton;
        [SerializeField] private Button _tabPhuDongButton;
        [SerializeField] private Button _tabLieuHanhButton;
        [SerializeField] private Image _tabTanVienBg;
        [SerializeField] private Image _tabPhuDongBg;
        [SerializeField] private Image _tabLieuHanhBg;
        [SerializeField] private TextMeshProUGUI _tabTanVienText;
        [SerializeField] private TextMeshProUGUI _tabPhuDongText;
        [SerializeField] private TextMeshProUGUI _tabLieuHanhText;

        [Header("Left Column: Node Card Items (3 Slots)")]
        [SerializeField] private UpgradeNodeCardItem[] _nodeCards = new UpgradeNodeCardItem[3];

        [Header("Right Column: Node Details Panel")]
        [SerializeField] private Image _detailIcon;
        [SerializeField] private TextMeshProUGUI _detailTitleText;
        [SerializeField] private TextMeshProUGUI _detailBranchText;
        [SerializeField] private TextMeshProUGUI _detailDescText;
        [SerializeField] private TextMeshProUGUI _detailLevelText;
        [SerializeField] private Image _detailLevelProgressBar;
        [SerializeField] private TextMeshProUGUI _detailBonusPreviewText;
        [SerializeField] private TextMeshProUGUI _upgradeCostText;
        [SerializeField] private Button _buyUpgradeButton;
        [SerializeField] private TextMeshProUGUI _buyButtonText;

        public event Action<SanctuaryBranch> OnTabSelected;
        public event Action<int> OnNodeCardSelected;
        public event Action OnBuyUpgradeClicked;
        public event Action OnCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_tabTanVienButton != null) _tabTanVienButton.onClick.AddListener(() => OnTabSelected?.Invoke(SanctuaryBranch.TanVienSonThanh));
            if (_tabPhuDongButton != null) _tabPhuDongButton.onClick.AddListener(() => OnTabSelected?.Invoke(SanctuaryBranch.PhuDongThienVuong));
            if (_tabLieuHanhButton != null) _tabLieuHanhButton.onClick.AddListener(() => OnTabSelected?.Invoke(SanctuaryBranch.LieuHanhChuDongTu));

            if (_nodeCards != null)
            {
                for (int i = 0; i < _nodeCards.Length; i++)
                {
                    int index = i;
                    if (_nodeCards[i]?.selectButton != null)
                    {
                        _nodeCards[i].selectButton.onClick.AddListener(() => OnNodeCardSelected?.Invoke(index));
                    }
                }
            }

            if (_buyUpgradeButton != null) _buyUpgradeButton.onClick.AddListener(() => OnBuyUpgradeClicked?.Invoke());

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(() =>
                {
                    OnCloseClicked?.Invoke();
                    OnBackPressed();
                });
            }
        }

        public void SetCoTienBalance(string formattedBalance)
        {
            if (_coTienBalanceText != null) _coTienBalanceText.text = formattedBalance;
        }

        public void UpdateTabVisuals(SanctuaryBranch activeBranch, Sprite tabActiveSprite, Sprite tabInactiveSprite)
        {
            Color activeTextColor = new Color(0.98f, 0.92f, 0.72f, 1f);
            Color inactiveTextColor = new Color(0.65f, 0.60f, 0.52f, 1f);

            SetTabState(_tabTanVienBg, _tabTanVienText, activeBranch == SanctuaryBranch.TanVienSonThanh, tabActiveSprite, tabInactiveSprite, activeTextColor, inactiveTextColor);
            SetTabState(_tabPhuDongBg, _tabPhuDongText, activeBranch == SanctuaryBranch.PhuDongThienVuong, tabActiveSprite, tabInactiveSprite, activeTextColor, inactiveTextColor);
            SetTabState(_tabLieuHanhBg, _tabLieuHanhText, activeBranch == SanctuaryBranch.LieuHanhChuDongTu, tabActiveSprite, tabInactiveSprite, activeTextColor, inactiveTextColor);
        }

        private void SetTabState(Image bg, TextMeshProUGUI text, bool isActive, Sprite activeSprite, Sprite inactiveSprite, Color activeColor, Color inactiveColor)
        {
            if (bg != null && (activeSprite != null || inactiveSprite != null))
            {
                bg.sprite = isActive ? activeSprite : inactiveSprite;
            }
            if (text != null)
            {
                text.color = isActive ? activeColor : inactiveColor;
                text.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        public void RenderNodeCard(int cardIndex, string title, string levelStr, Sprite icon, bool isSelected, bool isMaxLevel)
        {
            if (_nodeCards == null || cardIndex < 0 || cardIndex >= _nodeCards.Length) return;
            var card = _nodeCards[cardIndex];
            if (card == null) return;

            if (card.rootObject != null) card.rootObject.SetActive(true);
            if (card.titleText != null) card.titleText.text = title;
            if (card.levelText != null)
            {
                card.levelText.text = isMaxLevel ? "<color=#FFD700>ĐẠT TỐI ĐA</color>" : levelStr;
            }
            if (card.iconImage != null && icon != null)
            {
                card.iconImage.sprite = icon;
                card.iconImage.gameObject.SetActive(true);
            }
            if (card.selectionHighlight != null)
            {
                card.selectionHighlight.gameObject.SetActive(isSelected);
            }
        }

        public void DisplayUpgradeDetails(string title, string branchName, string desc, string levelStr, float progressRatio, string bonusPreview, string costStr, bool canAfford, bool isMaxLevel, Sprite icon)
        {
            if (_detailTitleText != null) _detailTitleText.text = title;
            if (_detailBranchText != null) _detailBranchText.text = branchName;
            if (_detailDescText != null) _detailDescText.text = desc;
            if (_detailLevelText != null) _detailLevelText.text = levelStr;
            if (_detailLevelProgressBar != null) _detailLevelProgressBar.fillAmount = Mathf.Clamp01(progressRatio);
            if (_detailBonusPreviewText != null) _detailBonusPreviewText.text = bonusPreview;
            if (_upgradeCostText != null) _upgradeCostText.text = costStr;

            if (_detailIcon != null && icon != null)
            {
                _detailIcon.sprite = icon;
                _detailIcon.gameObject.SetActive(true);
            }

            if (_buyUpgradeButton != null)
            {
                _buyUpgradeButton.interactable = canAfford && !isMaxLevel;
            }

            if (_buyButtonText != null)
            {
                _buyButtonText.text = isMaxLevel ? "ĐÃ ĐẠT TỐI ĐA" : (canAfford ? "CẦU PHÚC (NÂNG CẤP)" : "THIẾU CỔ TIỀN");
            }
        }
    }
}

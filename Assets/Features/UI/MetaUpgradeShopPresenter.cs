using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối số dư Cổ Tiền và mua nâng cấp chỉ số vĩnh viễn với SaveSystem.
    /// </summary>
    public class MetaUpgradeShopPresenter : MonoBehaviour
    {
        [SerializeField] private MetaUpgradeShopView _view;
        [SerializeField] private int _upgradeBaseCost = 100;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<MetaUpgradeShopView>();
        }

        private void OnEnable()
        {
            RenderShop();
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnBuyUpgradeClicked += OnBuyUpgrade;
                _view.OnCloseClicked += OnCloseShop;
            }

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
            }

            RenderShop();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnBuyUpgradeClicked -= OnBuyUpgrade;
                _view.OnCloseClicked -= OnCloseShop;
            }

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
            }
        }

        private void OnCurrencyChanged(int newBalance)
        {
            RenderShop();
        }

        private void OnBuyUpgrade()
        {
            if (MetaCurrencyManager.Instance == null) return;

            int currentBalance = MetaCurrencyManager.Instance.TotalCurrency;
            if (currentBalance >= _upgradeBaseCost)
            {
                if (MetaCurrencyManager.Instance.SpendCurrency(_upgradeBaseCost))
                {
                    Debug.Log($"[{nameof(MetaUpgradeShopPresenter)}] Nâng cấp thành công! Đã trừ {_upgradeBaseCost} Cổ Tiền.");
                    RenderShop();
                }
            }
        }

        private void OnCloseShop()
        {
            gameObject.SetActive(false);
        }

        private void RenderShop()
        {
            if (_view == null) return;

            int currentBalance = MetaCurrencyManager.Instance != null ? MetaCurrencyManager.Instance.TotalCurrency : 0;
            string balanceFormatted = $"<color=#FFD700>{currentBalance:N0}</color> Cổ Tiền";
            string costFormatted = $"Giá: <color=#FFD700>{_upgradeBaseCost}</color> Cổ Tiền";
            bool canAfford = currentBalance >= _upgradeBaseCost;

            _view.SetCoTienBalance(balanceFormatted);
            _view.DisplayUpgradeDetails("Tăng Máu Tối Đa Vĩnh Viễn (+10 HP)", costFormatted, "Cấp: 1 / 10", canAfford);
        }
    }
}

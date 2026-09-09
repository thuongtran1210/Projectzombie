using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.MetaProgression.Gacha;
using ProjectZombie.Features.MetaProgression.Gacha.Data;

namespace ProjectZombie.Features.UI.Gacha
{
    /// <summary>
    /// Presenter điều phối toàn bộ logic kết nối giữa Domain Model (RelicGachaManager, MetaCurrencyManager) và View (GachaChestView).
    /// Chịu trách nhiệm định dạng toàn bộ số liệu sang Rich Text TextMeshPro trước khi đẩy cho View.
    /// </summary>
    public class GachaChestPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private GachaChestView _view;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<GachaChestView>();
            if (_view == null) _view = GetComponentInChildren<GachaChestView>(true);

            if (_view != null)
            {
                _view.OnSingleRollClicked += HandleSingleRoll;
                _view.OnMultiRollClicked += HandleMultiRoll;
                _view.OnCloseResultClicked += HandleCloseResult;
            }
        }

        private void OnEnable()
        {
            // Subscribe Model Events
            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }

            if (RelicGachaManager.Instance != null)
            {
                RelicGachaManager.Instance.OnGachaSuccess += HandleGachaSuccess;
                RelicGachaManager.Instance.OnGachaFailed += HandleGachaFailed;
                RelicGachaManager.Instance.OnPityCountersChanged += HandlePityChanged;
            }

            RefreshAllUI();
        }

        private void OnDisable()
        {
            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }

            if (RelicGachaManager.Instance != null)
            {
                RelicGachaManager.Instance.OnGachaSuccess -= HandleGachaSuccess;
                RelicGachaManager.Instance.OnGachaFailed -= HandleGachaFailed;
                RelicGachaManager.Instance.OnPityCountersChanged -= HandlePityChanged;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnSingleRollClicked -= HandleSingleRoll;
                _view.OnMultiRollClicked -= HandleMultiRoll;
                _view.OnCloseResultClicked -= HandleCloseResult;
            }
        }

        private void RefreshAllUI()
        {
            if (_view == null) return;

            // 1. Cổ Tiền
            int totalCoins = MetaCurrencyManager.Instance != null ? MetaCurrencyManager.Instance.TotalCurrency : 0;
            _view.SetCurrencyBalance($"Cổ Tiền: <color=#FFD700><b>{totalCoins:N0}</b></color>");

            // 2. Banner Info & Costs
            var banner = RelicGachaManager.Instance != null ? RelicGachaManager.Instance.ActiveBanner : null;
            if (banner != null)
            {
                _view.SetBannerInfo($"<color=#FFD700>[ {banner.bannerName} ]</color>", banner.bannerDescription);
                _view.SetCosts($"Quay 1x\n<color=#FFD700>{banner.singleRollCost:N0} Cổ Tiền</color>", $"Quay 10x\n<color=#00FF88>{banner.multiRollCost:N0} Cổ Tiền</color>");

                // 3. Pity counters
                int curLegendary = RelicGachaManager.Instance.PityLegendary;
                int curEpic = RelicGachaManager.Instance.PityEpic;
                UpdatePityDisplay(banner, curLegendary, curEpic);
            }
            else
            {
                _view.SetCosts("100 Cổ Tiền", "900 Cổ Tiền");
            }
        }

        private void UpdatePityDisplay(GachaBannerConfigSO banner, int pityLeg, int pityEpic)
        {
            int remainLeg = Mathf.Max(0, banner.hardPityLegendary - pityLeg);
            int remainEpic = Mathf.Max(0, banner.epicGuaranteedEvery - pityEpic);

            string legText = remainLeg == 0 
                ? "<color=#FBBF24><b>[LƯỢT TIẾP THEO CHẮC CHẮN THẦN BINH!]</b></color>"
                : $"Còn <color=#FBBF24><b>{remainLeg}</b></color> lượt chắc chắn ra <color=#FBBF24>Thần Binh</color>";

            string epicText = remainEpic == 0 
                ? "<color=#C084FC><b>[LƯỢT TIẾP THEO CHẮC CHẮN CỰC PHẨM!]</b></color>"
                : $"Còn <color=#C084FC><b>{remainEpic}</b></color> lượt chắc chắn ra <color=#C084FC>Cực Phẩm</color>";

            _view.SetPityInfo(legText, epicText);
        }

        private void HandleCurrencyChanged(int newBalance)
        {
            if (_view != null)
            {
                _view.SetCurrencyBalance($"Cổ Tiền: <color=#FFD700><b>{newBalance:N0}</b></color>");
            }
        }

        private void HandlePityChanged(int pityLeg, int pityEpic)
        {
            var banner = RelicGachaManager.Instance != null ? RelicGachaManager.Instance.ActiveBanner : null;
            if (banner != null && _view != null)
            {
                UpdatePityDisplay(banner, pityLeg, pityEpic);
            }
        }

        private void HandleSingleRoll()
        {
            if (RelicGachaManager.Instance != null)
            {
                RelicGachaManager.Instance.Roll(1);
            }
        }

        private void HandleMultiRoll()
        {
            if (RelicGachaManager.Instance != null)
            {
                RelicGachaManager.Instance.Roll(10);
            }
        }

        private void HandleGachaSuccess(List<GachaDropResult> results)
        {
            if (_view != null)
            {
                _view.ShowStatusMessage("");
                _view.PlayChestOpenAnimation(() =>
                {
                    _view.DisplayResults(results);
                });
            }
        }

        private void HandleGachaFailed(string error)
        {
            if (_view != null)
            {
                _view.ShowStatusMessage($"<color=#FF4444>⚠️ {error}</color>");
            }
        }

        private void HandleCloseResult()
        {
            if (_view != null)
            {
                _view.CloseResultPopup();
            }
        }
    }
}

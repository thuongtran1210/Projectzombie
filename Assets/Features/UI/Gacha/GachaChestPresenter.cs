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
    public class GachaChestPresenter : MonoBehaviour, ProjectZombie.Core.Audio.IAudioServiceConsumer
    {
        private ProjectZombie.Core.Audio.IAudioService _audioService;
        public void InjectAudioService(ProjectZombie.Core.Audio.IAudioService audioService) => _audioService = audioService;
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
                _view.OnChestSelected += HandleChestSelected;
            }
        }

        private void OnEnable()
        {
            ProjectZombie.Core.Architecture.AppBootGate.OnCoreReady -= HandleCoreReady;
            ProjectZombie.Core.Architecture.AppBootGate.OnCoreReady += HandleCoreReady;
            SubscribeManagers();
            RefreshAllUI();
        }

        private void HandleCoreReady()
        {
            if (this == null || !isActiveAndEnabled) return;
            SubscribeManagers();
            RefreshAllUI();
        }

        private void SubscribeManagers()
        {
            if (MetaCurrencyManager.HasInstance)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
                MetaCurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }

            if (RelicGachaManager.HasInstance)
            {
                RelicGachaManager.Instance.OnGachaSuccess -= HandleGachaSuccess;
                RelicGachaManager.Instance.OnGachaSuccess += HandleGachaSuccess;
                RelicGachaManager.Instance.OnGachaFailed -= HandleGachaFailed;
                RelicGachaManager.Instance.OnGachaFailed += HandleGachaFailed;
                RelicGachaManager.Instance.OnPityCountersChanged -= HandlePityChanged;
                RelicGachaManager.Instance.OnPityCountersChanged += HandlePityChanged;
            }
        }

        private void OnDisable()
        {
            ProjectZombie.Core.Architecture.AppBootGate.OnCoreReady -= HandleCoreReady;
            if (MetaCurrencyManager.HasInstance)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }

            if (RelicGachaManager.HasInstance)
            {
                RelicGachaManager.Instance.OnGachaSuccess -= HandleGachaSuccess;
                RelicGachaManager.Instance.OnGachaFailed -= HandleGachaFailed;
                RelicGachaManager.Instance.OnPityCountersChanged -= HandlePityChanged;
            }
        }

        private void OnDestroy()
        {
            ProjectZombie.Core.Architecture.AppBootGate.OnCoreReady -= HandleCoreReady;
            if (_view != null)
            {
                _view.OnSingleRollClicked -= HandleSingleRoll;
                _view.OnMultiRollClicked -= HandleMultiRoll;
                _view.OnCloseResultClicked -= HandleCloseResult;
                _view.OnChestSelected -= HandleChestSelected;
            }
        }

        private void RefreshAllUI()
        {
            if (_view == null) return;

            int totalCoins = MetaCurrencyManager.HasInstance ? MetaCurrencyManager.Instance.TotalCurrency : 0;
            _view.SetCurrencyBalance($"Cổ Tiền: <color=#FFD700><b>{totalCoins:N0}</b></color>");

            var banner = RelicGachaManager.HasInstance ? RelicGachaManager.Instance.ActiveBanner : null;
            if (banner != null)
            {
                _view.SetBannerInfo($"<color=#FFD700>[ {banner.bannerName} ]</color>", banner.bannerDescription);
                _view.SetCosts($"Quay 1x\n<color=#FFD700>{banner.singleRollCost:N0} Cổ Tiền</color>", $"Quay 10x\n<color=#00FF88>{banner.multiRollCost:N0} Cổ Tiền</color>");
                _view.SetSelectedChestVisuals(banner.bannerId);

                int curLegendary = RelicGachaManager.Instance.PityLegendary;
                int curEpic = RelicGachaManager.Instance.PityEpic;
                UpdatePityDisplay(banner, curLegendary, curEpic);
            }
            else
            {
                _view.SetCosts("100 Cổ Tiền", "900 Cổ Tiền");
            }
        }

        private async void HandleChestSelected(string bannerId)
        {
            if (!RelicGachaManager.HasInstance) return;

            // 1. Nạp từ Data Provider (Single Source of Truth: Resources / _Data/Gacha/*.asset)
            GachaBannerConfigSO targetBanner = RelicGachaManager.Instance.GetBannerById(bannerId);

            // 2. Fallback nạp qua GameDataService (Addressables) nếu chưa có trong Provider
            if (targetBanner == null && ProjectZombie.Core.Services.Data.GameDataService.Instance != null)
            {
                targetBanner = await ProjectZombie.Core.Services.Data.GameDataService.Instance.GetAsync<GachaBannerConfigSO>($"Gacha/{bannerId}") ??
                               await ProjectZombie.Core.Services.Data.GameDataService.Instance.GetAsync<GachaBannerConfigSO>(bannerId);
            }

            if (targetBanner != null)
            {
                RelicGachaManager.Instance.SetActiveBanner(targetBanner);
                RefreshAllUI();
                _audioService?.PlayUIClick();
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
            var banner = RelicGachaManager.HasInstance ? RelicGachaManager.Instance.ActiveBanner : null;
            if (banner != null && _view != null)
            {
                UpdatePityDisplay(banner, pityLeg, pityEpic);
            }
        }

        private void HandleSingleRoll()
        {
            if (RelicGachaManager.HasInstance)
            {
                RelicGachaManager.Instance.Roll(1);
            }
        }

        private void HandleMultiRoll()
        {
            if (RelicGachaManager.HasInstance)
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
                _view.ShowStatusMessage($"<color=#FF4444>[Lỗi] {error}</color>");
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

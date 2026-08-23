using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối giữa MetaCurrencyManager / SaveData (Model) và MainHubView (View).
    /// Tuân thủ MVP: Cập nhật Cổ Tiền, điều hướng mở các phân khu và phát tín hiệu bắt đầu trận đấu.
    /// </summary>
    public class MainHubPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private MainHubView _view;

        [Header("Manager Reference")]
        [SerializeField] private MetaCurrencyManager _currencyManager;

        public event System.Action OnStartRunRequested;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<MainHubView>();
            if (_currencyManager == null) _currencyManager = FindObjectOfType<MetaCurrencyManager>();
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnStartRunClicked += HandleStartRunClicked;
                _view.OnHeroSelectClicked += HandleHeroSelectClicked;
                _view.OnSanctuaryTreeClicked += HandleSanctuaryTreeClicked;
                _view.OnCodexClicked += HandleCodexClicked;
                _view.OnSettingsClicked += HandleSettingsClicked;
            }

            if (_currencyManager != null)
            {
                _currencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
            }

            UpdateCurrencyDisplay();
            InitSelectedHeroPreview();
        }

        private void InitSelectedHeroPreview()
        {
            if (_view != null)
            {
                var selectionData = Resources.Load<ProjectZombie.Features.Player.CharacterSelectionData>("CharacterSelectionData");
                if (selectionData != null && selectionData.Characters != null && 
                    selectionData.SelectedCharacterIndex >= 0 && selectionData.SelectedCharacterIndex < selectionData.Characters.Count)
                {
                    var selected = selectionData.Characters[selectionData.SelectedCharacterIndex];
                    _view.SetSelectedHeroPreview(selected.characterName, selected.avatar);
                }
                else
                {
                    _view.SetSelectedHeroPreview("THƯ SINH (HỆ KIM)", null);
                }
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnStartRunClicked -= HandleStartRunClicked;
                _view.OnHeroSelectClicked -= HandleHeroSelectClicked;
                _view.OnSanctuaryTreeClicked -= HandleSanctuaryTreeClicked;
                _view.OnCodexClicked -= HandleCodexClicked;
                _view.OnSettingsClicked -= HandleSettingsClicked;
            }

            if (_currencyManager != null)
            {
                _currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
            }
        }

        public void UpdateCurrencyDisplay(int amount = -1)
        {
            if (_view != null)
            {
                int balance = amount >= 0 ? amount : (_currencyManager != null ? _currencyManager.TotalCurrency : 0);
                _view.SetCoTienBalance($"<color=#FFD700>{balance:N0}</color> Cổ Tiền");
            }
        }

        private void HandleStartRunClicked()
        {
            OnStartRunRequested?.Invoke();
        }

        private void HandleHeroSelectClicked()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.OpenScreen(MetaScreenType.CharacterSelect);
            }
        }

        private void HandleSanctuaryTreeClicked()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.OpenScreen(MetaScreenType.SanctuaryTree);
            }
        }

        private void HandleCodexClicked()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.OpenScreen(MetaScreenType.Codex);
            }
        }

        private void HandleSettingsClicked()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.OpenScreen(MetaScreenType.Settings);
            }
        }
    }
}

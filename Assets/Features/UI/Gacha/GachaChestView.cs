using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.MetaProgression.Gacha.Data;

namespace ProjectZombie.Features.UI.Gacha
{
    /// <summary>
    /// View thụ động (Passive View) cho màn hình Gacha Bảo Rương Vạn Cổ.
    /// Tuân thủ quy chuẩn MVP: Chỉ nhận dữ liệu đã định dạng từ Presenter, không chứa logic nghiệp vụ.
    /// </summary>
    public class GachaChestView : MonoBehaviour
    {
        [Header("Texts (TextMeshProUGUI)")]
        [SerializeField] private TextMeshProUGUI _currencyBalanceText;
        [SerializeField] private TextMeshProUGUI _bannerTitleText;
        [SerializeField] private TextMeshProUGUI _bannerDescriptionText;
        [SerializeField] private TextMeshProUGUI _singleCostText;
        [SerializeField] private TextMeshProUGUI _multiCostText;
        [SerializeField] private TextMeshProUGUI _legendaryPityText;
        [SerializeField] private TextMeshProUGUI _epicPityText;
        [SerializeField] private TextMeshProUGUI _statusMessageText;

        [Header("Buttons")]
        [SerializeField] private Button _singleRollButton;
        [SerializeField] private Button _multiRollButton;
        [SerializeField] private Button _closeResultButton;

        [Header("Animation & Result Modal")]
        [SerializeField] private Animator _chestAnimator;
        [SerializeField] private GameObject _resultPopupPanel;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private GachaCardRewardView _cardPrefab;

        public event Action OnSingleRollClicked;
        public event Action OnMultiRollClicked;
        public event Action OnCloseResultClicked;

        private readonly List<GachaCardRewardView> _spawnedCards = new List<GachaCardRewardView>();

        private void Awake()
        {
            if (_chestAnimator != null)
            {
                // Bắt buộc theo quy chuẩn 12.6 của AGENTS.md
                _chestAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_singleRollButton != null) _singleRollButton.onClick.AddListener(() => OnSingleRollClicked?.Invoke());
            if (_multiRollButton != null) _multiRollButton.onClick.AddListener(() => OnMultiRollClicked?.Invoke());
            if (_closeResultButton != null) _closeResultButton.onClick.AddListener(() => OnCloseResultClicked?.Invoke());

            if (_resultPopupPanel != null)
            {
                _resultPopupPanel.SetActive(false);
            }
        }

        public void SetCurrencyBalance(string formattedBalance)
        {
            if (_currencyBalanceText != null) _currencyBalanceText.text = formattedBalance;
        }

        public void SetBannerInfo(string title, string description)
        {
            if (_bannerTitleText != null) _bannerTitleText.text = title;
            if (_bannerDescriptionText != null) _bannerDescriptionText.text = description;
        }

        public void SetCosts(string singleCost, string multiCost)
        {
            if (_singleCostText != null) _singleCostText.text = singleCost;
            if (_multiCostText != null) _multiCostText.text = multiCost;
        }

        public void SetPityInfo(string legendaryPity, string epicPity)
        {
            if (_legendaryPityText != null) _legendaryPityText.text = legendaryPity;
            if (_epicPityText != null) _epicPityText.text = epicPity;
        }

        public void ShowStatusMessage(string message)
        {
            if (_statusMessageText != null)
            {
                _statusMessageText.text = message;
            }
        }

        public void PlayChestOpenAnimation(Action onAnimationComplete)
        {
            if (_chestAnimator != null)
            {
                _chestAnimator.SetTrigger("OpenChest");
            }

            // Gọi callback hiển thị kết quả
            onAnimationComplete?.Invoke();
        }

        public void DisplayResults(List<GachaDropResult> results)
        {
            if (_resultPopupPanel == null || _cardsContainer == null) return;

            // Xóa/tái sử dụng thẻ cũ
            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                if (_spawnedCards[i] != null) Destroy(_spawnedCards[i].gameObject);
            }
            _spawnedCards.Clear();

            if (_cardPrefab != null)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    var card = Instantiate(_cardPrefab, _cardsContainer);
                    card.BindData(results[i]);
                    _spawnedCards.Add(card);
                }
            }

            _resultPopupPanel.SetActive(true);
        }

        public void CloseResultPopup()
        {
            if (_resultPopupPanel != null)
            {
                _resultPopupPanel.SetActive(false);
            }
        }
    }
}

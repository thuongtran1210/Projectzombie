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
    public class GachaChestView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.GachaShop;
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
        [SerializeField] private RectTransform _sunburstTransform;
        [SerializeField] private Image _sunburstImage;
        [SerializeField] private GameObject _resultPopupPanel;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private GachaCardRewardView _cardPrefab;

        [Header("Chest Selection Buttons")]
        [SerializeField] private Button _chestBronzeButton;
        [SerializeField] private Button _chestHeroButton;
        [SerializeField] private GameObject _glowBronze;
        [SerializeField] private GameObject _glowHero;

        public event Action OnSingleRollClicked;
        public event Action OnMultiRollClicked;
        public event Action OnCloseResultClicked;
        public event Action<string> OnChestSelected; // "banner_standard" hoặc "banner_hero"

        private readonly List<GachaCardRewardView> _spawnedCards = new List<GachaCardRewardView>();
        private bool _isRolling = false;

        [Header("Back Navigation Button")]
        [SerializeField] private Button _backButton;

        public event Action OnBackClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_backButton != null) _backButton.onClick.AddListener(() => {
                if (_isRolling) return;
                OnBackClicked?.Invoke();
                if (MetaUIManager.Instance != null) MetaUIManager.Instance.PopScreen();
            });
            if (_chestAnimator != null)
            {
                // Bắt buộc theo quy chuẩn 12.6 của AGENTS.md
                _chestAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_singleRollButton != null) _singleRollButton.onClick.AddListener(() => {
                if (_isRolling) return;
                OnSingleRollClicked?.Invoke();
            });
            if (_multiRollButton != null) _multiRollButton.onClick.AddListener(() => {
                if (_isRolling) return;
                OnMultiRollClicked?.Invoke();
            });
            if (_closeResultButton != null) _closeResultButton.onClick.AddListener(() => OnCloseResultClicked?.Invoke());

            if (_chestBronzeButton != null) _chestBronzeButton.onClick.AddListener(() => {
                if (_isRolling) return;
                OnChestSelected?.Invoke("banner_standard");
            });

            if (_chestHeroButton != null) _chestHeroButton.onClick.AddListener(() => {
                if (_isRolling) return;
                OnChestSelected?.Invoke("banner_hero");
            });

            if (_resultPopupPanel != null)
            {
                _resultPopupPanel.SetActive(false);
            }
        }

        public void SetSelectedChestVisuals(string activeBannerId)
        {
            bool isStandard = activeBannerId == "banner_standard";
            if (_glowBronze != null) _glowBronze.SetActive(isStandard);
            if (_glowHero != null) _glowHero.SetActive(!isStandard);
        }

        private void Update()
        {
            // Hào quang Sunburst tự động xoay nhẹ nhàng êm ái khi ở trạng thái Idle
            if (_sunburstTransform != null && gameObject.activeInHierarchy && !_isRolling)
            {
                _sunburstTransform.Rotate(0f, 0f, -20f * Time.unscaledDeltaTime);
            }
        }

        public void SetCurrencyBalance(string formattedBalance)
        {
            if (_currencyBalanceText == null)
            {
                foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (tmp.transform.parent != null && (tmp.transform.parent.name == "Box_CoTien" || tmp.transform.parent.name.Contains("CoTien") || tmp.name.Contains("Currency")))
                    {
                        _currencyBalanceText = tmp;
                        break;
                    }
                }
            }

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
            StopAllCoroutines();
            StartCoroutine(AnimateChestOpenRoutine(onAnimationComplete));
        }

        private System.Collections.IEnumerator AnimateChestOpenRoutine(Action onAnimationComplete)
        {
            _isRolling = true;
            SetRollButtonsInteractable(false);

            if (_chestAnimator != null)
            {
                _chestAnimator.SetTrigger("OpenChest");
            }

            // Hiệu ứng Hào Quang Sunburst bừng sáng và xoay tốc độ cao trong lúc mở rương (0.85 giây)
            float duration = 0.85f;
            float elapsed = 0f;
            Vector3 originalScale = _sunburstTransform != null ? _sunburstTransform.localScale : Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                if (_sunburstTransform != null)
                {
                    // Xoay nhanh dần
                    _sunburstTransform.Rotate(0f, 0f, -240f * Time.unscaledDeltaTime);

                    // Phóng to bừng sáng hình sin
                    float scaleFactor = 1f + 0.22f * Mathf.Sin(progress * Mathf.PI);
                    _sunburstTransform.localScale = originalScale * scaleFactor;
                }

                yield return null;
            }

            if (_sunburstTransform != null)
            {
                _sunburstTransform.localScale = originalScale;
            }

            _isRolling = false;
            SetRollButtonsInteractable(true);

            // Mở bảng kết quả thẻ bài sau khi hoạt ảnh mở rương kết thúc
            onAnimationComplete?.Invoke();
        }

        private void SetRollButtonsInteractable(bool interactable)
        {
            if (_singleRollButton != null) _singleRollButton.interactable = interactable;
            if (_multiRollButton != null) _multiRollButton.interactable = interactable;
            if (_backButton != null) _backButton.interactable = interactable;
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

            // Trả trạng thái rương về Idle sau khi đóng popup
            if (_chestAnimator != null)
            {
                _chestAnimator.Play("Idle", 0, 0f);
            }
        }
    }
}

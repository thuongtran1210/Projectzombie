using UnityEngine;
using System.Collections.Generic;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View quản lý hiển thị chung của Bảng lựa chọn Nâng cấp (Upgrade Panel) theo mô hình MVP.
    /// Hỗ trợ Container động và Object Pooling nội bộ cho các Thẻ Nâng Cấp (Upgrade Cards).
    /// </summary>
    public class UpgradeUIView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private UpgradeCardView _cardPrefab;

        [Header("Legacy Compatibility (Optional Fallback)")]
        [SerializeField] private UpgradeCardView[] _upgradeCards;

        [Header("Optional Roguelite Controls")]
        [SerializeField] private UnityEngine.UI.Button _rerollButton;
        [SerializeField] private UnityEngine.UI.Button _skipButton;
        [SerializeField] private TMPro.TextMeshProUGUI _rerollCountText;

        private readonly List<UpgradeCardView> _cardPool = new List<UpgradeCardView>();
        private System.Action _onRerollClicked;
        private System.Action _onSkipClicked;

        private void Awake()
        {
            if (_upgradePanel != null)
            {
                _upgradePanel.SetActive(false);
            }

            if (_rerollButton != null)
            {
                _rerollButton.onClick.AddListener(() => _onRerollClicked?.Invoke());
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(() => _onSkipClicked?.Invoke());
            }

            InitializeCardPool();
        }

        /// <summary>
        /// Thu thập các Card View có sẵn trong scene để đưa vào pool tái sử dụng.
        /// </summary>
        private void InitializeCardPool()
        {
            _cardPool.Clear();

            // 1. Quét các thẻ con đã có sẵn trong _cardsContainer
            if (_cardsContainer != null)
            {
                UpgradeCardView[] existingInContainer = _cardsContainer.GetComponentsInChildren<UpgradeCardView>(true);
                if (existingInContainer != null && existingInContainer.Length > 0)
                {
                    _cardPool.AddRange(existingInContainer);
                }
            }

            // 2. Tương thích ngược: Nếu chưa có thẻ nào trong pool nhưng có kéo mảng _upgradeCards
            if (_cardPool.Count == 0 && _upgradeCards != null && _upgradeCards.Length > 0)
            {
                foreach (var card in _upgradeCards)
                {
                    if (card != null && !_cardPool.Contains(card))
                    {
                        _cardPool.Add(card);
                    }
                }
            }

            // 3. Nếu chưa gán _cardPrefab nhưng đã có thẻ mẫu trong scene, lấy thẻ đầu tiên làm template
            if (_cardPrefab == null && _cardPool.Count > 0)
            {
                _cardPrefab = _cardPool[0];
            }

            // 4. Nếu chưa gán _cardsContainer nhưng có card trong pool, lấy parent của nó làm container
            if (_cardsContainer == null && _cardPool.Count > 0 && _cardPool[0] != null)
            {
                _cardsContainer = _cardPool[0].transform.parent;
            }
        }

        /// <summary>
        /// Lấy hoặc khởi tạo danh sách Card View với số lượng tùy ý (Object Pooling, zero-GC runtime).
        /// Tự động bật đúng số lượng cần thiết và ẩn các card thừa.
        /// </summary>
        public IReadOnlyList<UpgradeCardView> GetOrCreateCardViews(int requiredCount)
        {
            if (requiredCount <= 0)
            {
                // Tắt tất cả các card hiện có
                for (int i = 0; i < _cardPool.Count; i++)
                {
                    if (_cardPool[i] != null) _cardPool[i].gameObject.SetActive(false);
                }
                return System.Array.Empty<UpgradeCardView>();
            }

            // Sinh thêm thẻ nếu pool chưa đủ số lượng yêu cầu
            while (_cardPool.Count < requiredCount)
            {
                if (_cardPrefab == null || _cardsContainer == null)
                {
                    Debug.LogWarning($"[{nameof(UpgradeUIView)}] Không thể sinh thêm thẻ do thiếu _cardPrefab hoặc _cardsContainer.");
                    break;
                }

                UpgradeCardView newCard = Instantiate(_cardPrefab, _cardsContainer);
                newCard.name = $"UpgradeCard_{_cardPool.Count}";
                _cardPool.Add(newCard);
            }

            int countToReturn = Mathf.Min(requiredCount, _cardPool.Count);
            List<UpgradeCardView> result = new List<UpgradeCardView>(countToReturn);

            for (int i = 0; i < _cardPool.Count; i++)
            {
                UpgradeCardView card = _cardPool[i];
                if (card == null) continue;

                bool shouldBeActive = i < countToReturn;
                card.gameObject.SetActive(shouldBeActive);

                if (shouldBeActive)
                {
                    result.Add(card);
                }
            }

            return result;
        }

        public void SetRerollButtonCallback(System.Action onReroll)
        {
            _onRerollClicked = onReroll;
            if (_rerollButton != null)
            {
                _rerollButton.gameObject.SetActive(onReroll != null);
            }
        }

        public void SetSkipButtonCallback(System.Action onSkip)
        {
            _onSkipClicked = onSkip;
            if (_skipButton != null)
            {
                _skipButton.gameObject.SetActive(onSkip != null);
            }
        }

        public void SetRerollCountText(string text)
        {
            if (_rerollCountText != null)
            {
                _rerollCountText.text = text;
            }
        }

        public void SetRerollInteractable(bool interactable)
        {
            if (_rerollButton != null)
            {
                _rerollButton.interactable = interactable;
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

        /// <summary>
        /// API Tương thích ngược: Trả về số lượng thẻ hiện có trong Pool/Inspector.
        /// </summary>
        public int GetCardsLength()
        {
            return _cardPool.Count > 0 ? _cardPool.Count : (_upgradeCards != null ? _upgradeCards.Length : 0);
        }

        /// <summary>
        /// API Tương thích ngược: Lấy tham chiếu đến một Card View cụ thể theo index.
        /// </summary>
        public UpgradeCardView GetCardView(int index)
        {
            if (index >= 0 && index < _cardPool.Count)
            {
                return _cardPool[index];
            }

            if (_upgradeCards != null && index >= 0 && index < _upgradeCards.Length)
            {
                return _upgradeCards[index];
            }

            return null;
        }
    }
}

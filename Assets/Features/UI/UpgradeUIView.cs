using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View quản lý hiển thị chung của Bảng lựa chọn Nâng cấp (Upgrade Panel) theo mô hình MVP.
    /// Hỗ trợ Container động, Object Pooling nội bộ và các nút điều khiển Reroll/Skip.
    /// </summary>
    public class UpgradeUIView : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject _upgradePanel;

        [Header("Cards Dynamic Container")]
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private UpgradeCardView _cardPrefab;

        [Header("Legacy Compatibility (Optional Fallback)")]
        [SerializeField] private UpgradeCardView[] _upgradeCards;

        [Header("Roguelite Controls")]
        [SerializeField] private Button _rerollButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TextMeshProUGUI _rerollCountText;

        private readonly List<UpgradeCardView> _cardPool = new List<UpgradeCardView>();
        private System.Action _onRerollClicked;
        private System.Action _onSkipClicked;

        private void Awake()
        {
            if (_upgradePanel != null)
            {
                _upgradePanel.SetActive(false);
            }

            EnsureControlsFound();
            SetupButtonListeners();

            // Tự động đảm bảo Animator không bị đóng băng khi pause game (Time.timeScale = 0)
            if (_upgradePanel != null)
            {
                var animator = _upgradePanel.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                }
            }

            InitializeCardPool();
        }

        private void EnsureControlsFound()
        {
            if (_rerollButton == null || _skipButton == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    if (btn == null) continue;
                    string bName = btn.name.ToLower();
                    if (_rerollButton == null && (bName.Contains("reroll") || bName.Contains("doithe") || bName.Contains("doi_the")))
                    {
                        _rerollButton = btn;
                    }
                    else if (_skipButton == null && (bName.Contains("skip") || bName.Contains("boqua") || bName.Contains("bo_qua")))
                    {
                        _skipButton = btn;
                    }
                }
            }

            if (_rerollCountText == null && _rerollButton != null)
            {
                _rerollCountText = _rerollButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        private void SetupButtonListeners()
        {
            EnsureControlsFound();

            if (_rerollButton != null)
            {
                _rerollButton.gameObject.layer = LayerMask.NameToLayer("UI");
                _rerollButton.onClick.RemoveAllListeners();
                _rerollButton.onClick.AddListener(() => {
                    Debug.Log("<color=#00FF88>[UpgradeUIView]</color> Reroll Button Clicked!");
                    _onRerollClicked?.Invoke();
                });

                // Đảm bảo Footer_Controls luôn hiển thị trên cùng trong Hierarchy để không bị Cards_Container che raycast
                if (_rerollButton.transform.parent != null && _rerollButton.transform.parent.name.Contains("Footer"))
                {
                    _rerollButton.transform.parent.SetAsLastSibling();
                }

                // Vô hiệu hóa raycastTarget cho tất cả con bên trong nút (chữ, icon trang trí)
                var graphics = _rerollButton.GetComponentsInChildren<Graphic>(true);
                foreach (var g in graphics)
                {
                    if (g.gameObject != _rerollButton.gameObject)
                    {
                        g.raycastTarget = false;
                    }
                }
            }

            if (_skipButton != null)
            {
                _skipButton.gameObject.layer = LayerMask.NameToLayer("UI");
                _skipButton.onClick.RemoveAllListeners();
                _skipButton.onClick.AddListener(() => {
                    Debug.Log("<color=#00FF88>[UpgradeUIView]</color> Skip Button Clicked!");
                    _onSkipClicked?.Invoke();
                });

                // Vô hiệu hóa raycastTarget cho tất cả con bên trong nút
                var graphics = _skipButton.GetComponentsInChildren<Graphic>(true);
                foreach (var g in graphics)
                {
                    if (g.gameObject != _skipButton.gameObject)
                    {
                        g.raycastTarget = false;
                    }
                }
            }

            if (_rerollCountText != null)
            {
                _rerollCountText.gameObject.layer = LayerMask.NameToLayer("UI");
                _rerollCountText.raycastTarget = false;
            }
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
                for (int i = 0; i < _cardPool.Count; i++)
                {
                    if (_cardPool[i] != null) _cardPool[i].gameObject.SetActive(false);
                }
                return System.Array.Empty<UpgradeCardView>();
            }

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
            SetupButtonListeners();
        }

        public void SetSkipButtonCallback(System.Action onSkip)
        {
            _onSkipClicked = onSkip;
            SetupButtonListeners();
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

        public int GetCardsLength()
        {
            return _cardPool.Count > 0 ? _cardPool.Count : (_upgradeCards != null ? _upgradeCards.Length : 0);
        }

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

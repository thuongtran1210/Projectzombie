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
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Controls Cache")]
        [SerializeField] private GameObject _cachedMobileControlsPanel;

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
            if (_upgradePanel == null)
            {
                Transform panelTrans = transform.Find("Panel_Upgrade") ?? transform.Find("Upgrade_Panel") ?? transform.Find("Modal_Upgrade") ?? transform;
                _upgradePanel = panelTrans.gameObject;
            }

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

            // 1. Tự động tìm _cardsContainer nếu chưa gán
            if (_cardsContainer == null && _upgradePanel != null)
            {
                Transform found = _upgradePanel.transform.Find("Cards_Container") ??
                                  _upgradePanel.transform.Find("CardsContainer") ??
                                  _upgradePanel.transform.Find("Content_Cards") ??
                                  _upgradePanel.transform.Find("Panel_Cards") ??
                                  _upgradePanel.transform.Find("Layout_Cards");
                if (found != null)
                {
                    _cardsContainer = found;
                }
            }

            // 2. Quét các thẻ con đã có sẵn trong _cardsContainer
            if (_cardsContainer != null)
            {
                UpgradeCardView[] existingInContainer = _cardsContainer.GetComponentsInChildren<UpgradeCardView>(true);
                if (existingInContainer != null && existingInContainer.Length > 0)
                {
                    _cardPool.AddRange(existingInContainer);
                }
            }

            // 3. Quét toàn bộ thẻ trong _upgradePanel hoặc component con nếu pool vẫn rỗng
            if (_cardPool.Count == 0)
            {
                var searchRoot = _upgradePanel != null ? _upgradePanel.transform : transform;
                UpgradeCardView[] allNested = searchRoot.GetComponentsInChildren<UpgradeCardView>(true);
                if (allNested != null && allNested.Length > 0)
                {
                    _cardPool.AddRange(allNested);
                }
            }

            // 4. Tương thích ngược: Nếu chưa có thẻ nào trong pool nhưng có kéo mảng _upgradeCards
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

            // 5. Nếu chưa gán _cardPrefab nhưng đã có thẻ mẫu trong scene, lấy thẻ đầu tiên làm template
            if (_cardPrefab == null && _cardPool.Count > 0 && _cardPool[0] != null)
            {
                _cardPrefab = _cardPool[0];
            }

            // 6. Nếu chưa gán _cardsContainer nhưng có card trong pool, lấy parent của nó làm container
            if (_cardsContainer == null && _cardPool.Count > 0 && _cardPool[0] != null)
            {
                _cardsContainer = _cardPool[0].transform.parent;
            }

            Debug.Log($"<color=#FF00FF>[DIAG_UPGRADE_VIEW]</color> InitializeCardPool: _cardsContainer = {(_cardsContainer != null ? _cardsContainer.name : "NULL")}, _cardPrefab = {(_cardPrefab != null ? _cardPrefab.name : "NULL")}, PoolCount = {_cardPool.Count}");
        }

        /// <summary>
        /// Lấy hoặc khởi tạo danh sách Card View với số lượng tùy ý (Object Pooling, zero-GC runtime).
        /// Tự động bật đúng số lượng cần thiết và ẩn các card thừa.
        /// </summary>
        public IReadOnlyList<UpgradeCardView> GetOrCreateCardViews(int requiredCount)
        {
            Debug.Log($"<color=#FF00FF>[DIAG_UPGRADE_VIEW]</color> GetOrCreateCardViews(requiredCount: {requiredCount}) - Hiện có {_cardPool.Count} cards trong pool");

            if (requiredCount <= 0)
            {
                for (int i = 0; i < _cardPool.Count; i++)
                {
                    if (_cardPool[i] != null) _cardPool[i].gameObject.SetActive(false);
                }
                return System.Array.Empty<UpgradeCardView>();
            }

            if (_cardPool.Count == 0)
            {
                InitializeCardPool();
            }

            while (_cardPool.Count < requiredCount)
            {
                if (_cardPrefab == null || _cardsContainer == null)
                {
                    Debug.LogWarning($"<color=#FF00FF>[DIAG_UPGRADE_VIEW]</color> CẢNH BÁO: Không thể sinh thêm thẻ do thiếu _cardPrefab ({_cardPrefab != null}) hoặc _cardsContainer ({_cardsContainer != null}).");
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

            Debug.Log($"<color=#FF00FF>[DIAG_UPGRADE_VIEW]</color> GetOrCreateCardViews hoàn tất: trả về {result.Count} cards");
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

        /// <summary>
        /// Chuyển đổi trực quan chế độ giao diện và biến đổi Upgrade_Panel theo mốc Level (Cấp 1 Khởi Nguyên, Level Thường, Mốc Đột Biến).
        /// </summary>
        public void SetVisualMode(UpgradeUIVisualMode mode, int currentLevel)
        {
            EnsureControlsFound();

            if (_titleText == null && _upgradePanel != null)
            {
                _titleText = _upgradePanel.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            RectTransform panelRect = _upgradePanel != null ? _upgradePanel.GetComponent<RectTransform>() : null;
            Image panelImg = _upgradePanel != null ? _upgradePanel.GetComponent<Image>() : null;
            HorizontalLayoutGroup containerLayout = _cardsContainer != null ? _cardsContainer.GetComponent<HorizontalLayoutGroup>() : null;

            switch (mode)
            {
                case UpgradeUIVisualMode.ArchetypeCore:
                    if (_titleText != null)
                    {
                        _titleText.text = "KHỞI NGUYÊN ĐẠI LÕI\n<size=14><color=#FFD700>[ Kích hoạt Kho Thẻ Sạch (Clean Pool) ]</color></size>";
                    }
                    if (_rerollButton != null) _rerollButton.gameObject.SetActive(false);
                    if (_skipButton != null) _skipButton.gameObject.SetActive(false);
                    break;

                case UpgradeUIVisualMode.MicroStats:
                    if (_titleText != null)
                    {
                        _titleText.text = $"TĂNG CƯỜNG CHỈ SỐ (CẤP {currentLevel})";
                    }
                    if (_rerollButton != null) _rerollButton.gameObject.SetActive(false);
                    if (_skipButton != null) _skipButton.gameObject.SetActive(true);
                    break;

                case UpgradeUIVisualMode.MutationAugment:
                    if (_titleText != null)
                    {
                        _titleText.text = $"ĐỘT BIẾN THẦN THẠCH (CẤP {currentLevel})";
                    }
                    if (_rerollButton != null) _rerollButton.gameObject.SetActive(true);
                    if (_skipButton != null) _skipButton.gameObject.SetActive(false);
                    break;
            }
        }

        public void SetActive(bool isActive)
        {
            EnsureControlsFound();
            if (_cardPool.Count == 0)
            {
                InitializeCardPool();
            }

            Debug.Log($"<color=#FF00FF>[DIAG_UPGRADE_VIEW]</color> SetActive({isActive}) - _upgradePanel: {(_upgradePanel != null ? _upgradePanel.name : "NULL")}, Root: {gameObject.name}");

            if (_upgradePanel != null)
            {
                _upgradePanel.SetActive(isActive);
                transform.SetAsLastSibling();
                _upgradePanel.transform.SetAsLastSibling();

                var cg = _upgradePanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = isActive ? 1f : 0f;
                    cg.blocksRaycasts = isActive;
                    cg.interactable = isActive;
                }
                Debug.Log($"<color=#FF00FF>[DIAG_UPGRADE_VIEW]</color> _upgradePanel.activeSelf = {_upgradePanel.activeSelf}, CanvasGroup Alpha = {(cg != null ? cg.alpha.ToString() : "NoCG")}");
            }
            else
            {
                Debug.LogWarning($"<color=#FF00FF>[DIAG_UPGRADE_VIEW]</color> _upgradePanel chưa được gán trong Inspector.");
            }

            // Đồng bộ an toàn: Khi mở bảng nâng cấp, bắt buộc ẩn cụm phím điều khiển và thu hồi toàn bộ chỉ dấu
            if (isActive)
            {
                if (GameplayUIManager.Instance != null)
                {
                    GameplayUIManager.Instance.SetMobileControlsActive(false);
                }
                else if (_cachedMobileControlsPanel != null)
                {
                    _cachedMobileControlsPanel.SetActive(false);
                }

                // Thu hồi mọi chỉ dấu đang vẽ trên màn hình
                Combat.Aiming.SkillAimIndicatorController.Instance?.StopAim();
                DynamicVirtualJoystick.Instance?.ResetJoystick();
            }
            else
            {
                // Khi đóng bảng nâng cấp và game đang chạy, khôi phục lại Panel_MobileControls
                if (ProjectZombie.Features.Shared.GameStateManager.Instance == null || 
                    ProjectZombie.Features.Shared.GameStateManager.Instance.CurrentState == ProjectZombie.Features.Shared.GameState.Playing)
                {
                    if (GameplayUIManager.Instance != null)
                    {
                        GameplayUIManager.Instance.SetMobileControlsActive(true);
                    }
                    else if (_cachedMobileControlsPanel != null)
                    {
                        _cachedMobileControlsPanel.SetActive(true);
                    }
                }
            }
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

    /// <summary>
    /// Các chế độ hiển thị trực quan của bảng UI nâng cấp.
    /// </summary>
    public enum UpgradeUIVisualMode
    {
        ArchetypeCore,     // Lv.1: Đại Lõi Khởi Nguyên
        MicroStats,        // Level Thường (Lv.2-4, 6-14, 16-29)
        MutationAugment    // Mốc Đột Biến (Lv.5, Lv.15, Lv.30)
    }
}

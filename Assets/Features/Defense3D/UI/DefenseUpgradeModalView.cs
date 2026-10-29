using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    [Serializable]
    public sealed class DefenseUpgradeCardSlotUI
    {
        public Button CardButton;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI DescriptionText;
        public TextMeshProUGUI TagText;
    }

    /// <summary>
    /// Modal hiển thị 3 thẻ nâng cấp Roguelite khi thăng cấp (U3D-020).
    /// Tuân thủ quy tắc MVP: View thụ động, phát sự kiện OnCardSelected(index) cho Presenter.
    /// </summary>
    public class DefenseUpgradeModalView : MonoBehaviour
    {
        [Header("Root Modal")]
        [SerializeField] private GameObject _modalRoot;

        [Header("3 Thẻ Lựa Chọn")]
        [SerializeField] private DefenseUpgradeCardSlotUI[] _cardSlots = new DefenseUpgradeCardSlotUI[3];

        [Header("Tùy Chọn Phụ (Reroll & Skip)")]
        [SerializeField] private Button _rerollButton;
        [SerializeField] private TextMeshProUGUI _rerollCountText;
        [SerializeField] private Button _skipButton;

        public event Action<int> OnCardSelected;
        public event Action OnRerollRequested;
        public event Action OnSkipRequested;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            for (int i = 0; i < _cardSlots.Length; i++)
            {
                int index = i;
                if (_cardSlots[i]?.CardButton != null)
                {
                    _cardSlots[i].CardButton.onClick.AddListener(() => OnCardSelected?.Invoke(index));
                }
            }

            if (_rerollButton != null)
            {
                _rerollButton.onClick.AddListener(() => OnRerollRequested?.Invoke());
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(() => OnSkipRequested?.Invoke());
            }
        }

        public void Show(DefenseUpgradeCard[] cards, int rerollCount = -1)
        {
            if (_modalRoot != null) _modalRoot.SetActive(true);
            if (cards == null) return;

            for (int i = 0; i < _cardSlots.Length; i++)
            {
                if (_cardSlots[i] == null) continue;
                bool hasCard = i < cards.Length;
                if (_cardSlots[i].CardButton != null)
                    _cardSlots[i].CardButton.gameObject.SetActive(hasCard);

                if (hasCard)
                {
                    if (_cardSlots[i].TitleText != null)
                        _cardSlots[i].TitleText.text = cards[i].Title;
                    if (_cardSlots[i].DescriptionText != null)
                        _cardSlots[i].DescriptionText.text = cards[i].Description;
                    if (_cardSlots[i].TagText != null)
                    {
                        string tag = cards[i].UpgradeType switch
                        {
                            DefenseUpgradeType.HeroAttackDamage or DefenseUpgradeType.HeroAttackSpeed or DefenseUpgradeType.HeroMoveSpeed => "<color=#FFD700>[TƯỚNG]</color>",
                            DefenseUpgradeType.TowerDamage or DefenseUpgradeType.TowerAttackRange => "<color=#4CAF50>[THÁP]</color>",
                            _ => "<color=#2196F3>[CĂN CỨ]</color>"
                        };
                        _cardSlots[i].TagText.text = tag;
                    }
                }
            }

            if (_rerollButton != null)
            {
                _rerollButton.gameObject.SetActive(rerollCount >= 0);
            }
            if (_rerollCountText != null && rerollCount >= 0)
            {
                _rerollCountText.text = $"Đổi Thẻ ({rerollCount})";
            }
        }

        public void Hide()
        {
            if (_modalRoot != null) _modalRoot.SetActive(false);
        }
    }
}

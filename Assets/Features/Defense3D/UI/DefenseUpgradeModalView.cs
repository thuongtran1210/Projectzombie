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

        public event Action<int> OnCardSelected;

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
        }

        public void Show(DefenseUpgradeCard[] cards)
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
                }
            }
        }

        public void Hide()
        {
            if (_modalRoot != null) _modalRoot.SetActive(false);
        }
    }
}

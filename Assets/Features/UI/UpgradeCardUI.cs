using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Upgrades;
using System;

namespace ProjectZombie.Features.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI categoryText; // Hiển thị phân loại thẻ (Weapon, Rare, Evolution...)
        [SerializeField] private TextMeshProUGUI levelText;    // Hiển thị cấp độ của vũ khí (New, Lv.2, Lv.6...)
        [SerializeField] private Button selectButton;

        private UpgradeData _currentData;
        private Action<UpgradeData> _onSelected;

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnButtonClicked);
            }

            // Đảm bảo Animator chạy bình thường ngay cả khi Time.timeScale = 0 (Game Pause)
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        public void Setup(UpgradeData data, Action<UpgradeData> onSelected)
        {
            _currentData = data;
            _onSelected = onSelected;

            if (iconImage != null) iconImage.sprite = data.icon;
            if (nameText != null) nameText.text = data.upgradeName;
            if (descriptionText != null) descriptionText.text = data.description;
            
            if (categoryText != null)
            {
                categoryText.text = FormatCategoryName(data.upgradeType);
            }

            if (levelText != null)
            {
                if (data is WeaponUpgradeData weaponData)
                {
                    if (weaponData.requiredCurrentLevel == 0)
                        levelText.text = "NEW!";
                    else
                        levelText.text = $"Lv.{weaponData.requiredCurrentLevel + 1}";
                }
                else if (data is EvolutionUpgradeData)
                {
                    levelText.text = "EVOLUTION";
                }
                else
                {
                    levelText.text = ""; // Các thẻ kỹ năng bị động hoặc thẻ khác có thể không cần hiện level
                }
            }
        }

        private string FormatCategoryName(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.WeaponUpgrade: return "Weapon Upgrade";
                case UpgradeType.SignatureSkillUpgrade: return "Signature Skill";
                case UpgradeType.CommonUpgrade: return "Common Upgrade";
                case UpgradeType.FactionCounterUpgrade: return "Faction Counter";
                case UpgradeType.RareUpgrade: return "Rare Upgrade";
                case UpgradeType.EvolutionUpgrade: return "Evolution Upgrade";
                default: return type.ToString();
            }
        }

        private void OnButtonClicked()
        {
            if (_currentData != null)
            {
                _onSelected?.Invoke(_currentData);
            }
        }
    }
}

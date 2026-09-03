using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View hiển thị thụ động cho một thẻ nâng cấp (Upgrade Card) theo mô hình MVP.
    /// </summary>
    public class UpgradeCardView : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cardFrameImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _statDiffText;
        [SerializeField] private TextMeshProUGUI _elementBadgeText;

        [Header("Card Tier Frames (9-Slice Skins)")]
        [SerializeField] private Sprite _frameCommonWood;
        [SerializeField] private Sprite _frameRareJade;
        [SerializeField] private Sprite _frameEvolutionGold;
        [SerializeField] private Sprite _frameSynergyAmber;

        [Header("Evolution Synergy & Styling")]
        [SerializeField] private GameObject _synergyContainer;
        [SerializeField] private Image _synergyIconImage;
        [SerializeField] private TextMeshProUGUI _synergyLabelText;
        [SerializeField] private GameObject _evolutionBanner;
        [SerializeField] private Color _normalFrameColor = Color.white;
        [SerializeField] private Color _evolutionFrameColor = new Color(1f, 0.95f, 0.7f, 1f); // Vàng Kim Thần Khí

        [Header("Buttons")]
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _banButton;

        private Action _onClicked;
        private Action _onBanClicked;

        private void Awake()
        {
            EnsureTierSpritesLoaded();

            if (_selectButton != null)
            {
                _selectButton.onClick.AddListener(OnButtonClicked);
            }
            if (_banButton != null)
            {
                _banButton.onClick.AddListener(OnBanButtonClicked);
            }

            // Đảm bảo Animator chạy bình thường ngay cả khi Time.timeScale = 0 (khi pause chọn nâng cấp)
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        private void EnsureTierSpritesLoaded()
        {
#if UNITY_EDITOR
            if (_frameCommonWood == null) _frameCommonWood = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Wood_9Slice.png");
            if (_frameRareJade == null) _frameRareJade = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Jade_9Slice.png");
            if (_frameEvolutionGold == null) _frameEvolutionGold = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Evolution_Gold_9Slice.png");
            if (_frameSynergyAmber == null) _frameSynergyAmber = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Synergy_9Slice.png");
#endif
        }

        /// <summary>
        /// Thay đổi Khung Thẻ theo Phẩm Cấp (Gỗ Mun Thường / Ngọc Bích Hiếm / Hoàng Kim Tiến Hóa / Hổ Phách Duyên Phận).
        /// </summary>
        public void SetCardTier(UpgradeType upgradeType, bool isEvolution, bool hasSynergy)
        {
            EnsureTierSpritesLoaded();

            if (_cardFrameImage == null) return;

            if (isEvolution || upgradeType == UpgradeType.BreakthroughUltimate || upgradeType == UpgradeType.EvolutionUpgrade)
            {
                if (_frameEvolutionGold != null) _cardFrameImage.sprite = _frameEvolutionGold;
                SetEvolutionMode(true);
            }
            else if (hasSynergy)
            {
                if (_frameSynergyAmber != null) _cardFrameImage.sprite = _frameSynergyAmber;
                SetEvolutionMode(false);
            }
            else if (upgradeType == UpgradeType.RareUpgrade ||
                     upgradeType == UpgradeType.ComboAugment ||
                     upgradeType == UpgradeType.DashTrait ||
                     upgradeType == UpgradeType.ConditionalPassive ||
                     upgradeType == UpgradeType.SignatureSkillUpgrade)
            {
                if (_frameRareJade != null) _cardFrameImage.sprite = _frameRareJade;
                SetEvolutionMode(false);
            }
            else
            {
                if (_frameCommonWood != null) _cardFrameImage.sprite = _frameCommonWood;
                SetEvolutionMode(false);
            }
        }

        [Header("Fallback Placeholder Icons")]
        [SerializeField] private Sprite _fallbackWeaponIcon;
        [SerializeField] private Sprite _fallbackPassiveIcon;
        [SerializeField] private Sprite _fallbackDashIcon;
        [SerializeField] private Sprite _fallbackComboIcon;

        /// <summary>
        /// Thiết lập hiển thị toàn diện của thẻ nâng cấp kèm chuỗi Stat Diff thay đổi chỉ số.
        /// </summary>
        public void Setup(
            Sprite icon,
            string cardName,
            string description,
            string category,
            string level,
            string statDiff,
            Action onClicked,
            Action onBanClicked = null)
        {
            _onClicked = onClicked;
            _onBanClicked = onBanClicked;

            if (_banButton != null)
            {
                _banButton.gameObject.SetActive(onBanClicked != null);
            }

            // Tự động giải quyết fallback icon nếu thẻ chưa được gán icon riêng trong Inspector
            if (icon == null)
            {
                if (category != null && category.Contains("LƯỚT")) icon = _fallbackDashIcon;
                else if (category != null && category.Contains("BÍ KÍP")) icon = _fallbackComboIcon;
                else if (category != null && category.Contains("PHÁP BẢO")) icon = _fallbackWeaponIcon;
                else icon = _fallbackPassiveIcon;
            }

            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
                _iconImage.preserveAspect = true;

                // Nếu có object cha là Icon_Slot_Frame thì bật/tắt đồng bộ
                if (_iconImage.transform.parent != null && _iconImage.transform.parent != transform)
                {
                    _iconImage.transform.parent.gameObject.SetActive(icon != null);
                }
            }

            if (_nameText != null) _nameText.text = cardName;
            if (_descriptionText != null) _descriptionText.text = description;
            if (_categoryText != null) _categoryText.text = category;
            if (_levelText != null) _levelText.text = level;

            SetStatDiff(statDiff);
            SetEvolutionMode(false);
            SetSynergyInfo(null, null);
        }

        /// <summary>
        /// Cấu hình hiển thị thông tin Duyên Phận / Mảnh ghép Tiến Hóa.
        /// </summary>
        public void SetSynergyInfo(Sprite synergyIcon, string formattedSynergyText)
        {
            if (_synergyContainer == null && _synergyLabelText == null) return;

            bool hasSynergy = !string.IsNullOrEmpty(formattedSynergyText);
            if (_synergyContainer != null)
            {
                _synergyContainer.SetActive(hasSynergy);
            }

            if (_synergyIconImage != null)
            {
                _synergyIconImage.gameObject.SetActive(synergyIcon != null);
                if (synergyIcon != null)
                {
                    _synergyIconImage.sprite = synergyIcon;
                }
            }

            if (_synergyLabelText != null)
            {
                _synergyLabelText.gameObject.SetActive(hasSynergy);
                if (hasSynergy)
                {
                    _synergyLabelText.text = formattedSynergyText;
                }
            }
        }

        /// <summary>
        /// Bật/Tắt giao diện Thần Khí Tiến Hóa Vàng Kim đặc biệt.
        /// </summary>
        public void SetEvolutionMode(bool isEvolution)
        {
            if (_evolutionBanner != null)
            {
                _evolutionBanner.SetActive(isEvolution);
            }

            if (_cardFrameImage != null)
            {
                _cardFrameImage.color = isEvolution ? _evolutionFrameColor : _normalFrameColor;
            }
        }

        /// <summary>
        /// Thiết lập hiển thị chuỗi so sánh chỉ số (Stat Diff) với TMP Rich Text.
        /// </summary>
        public void SetStatDiff(string statDiffFormattedText)
        {
            if (_statDiffText == null) return;

            if (string.IsNullOrEmpty(statDiffFormattedText))
            {
                _statDiffText.gameObject.SetActive(false);
            }
            else
            {
                _statDiffText.gameObject.SetActive(true);
                _statDiffText.text = statDiffFormattedText;
            }
        }

        /// <summary>
        /// Hiển thị thuộc tính Ngũ Hành trên thẻ nâng cấp với TMP Rich Text màu sắc.
        /// </summary>
        public void SetElementBadge(string badgeFormattedText)
        {
            if (_elementBadgeText == null) return;

            if (string.IsNullOrEmpty(badgeFormattedText))
            {
                _elementBadgeText.gameObject.SetActive(false);
            }
            else
            {
                _elementBadgeText.gameObject.SetActive(true);
                _elementBadgeText.text = badgeFormattedText;
            }
        }

        private void OnButtonClicked()
        {
            _onClicked?.Invoke();
        }

        private void OnBanButtonClicked()
        {
            _onBanClicked?.Invoke();
        }
    }
}

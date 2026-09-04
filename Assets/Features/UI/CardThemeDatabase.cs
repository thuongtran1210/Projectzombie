using UnityEngine;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// ScriptableObject quản lý tập trung toàn bộ tài nguyên giao diện, khung viền 9-slice,
    /// icon dự phòng và màu sắc nguyên tố cho hệ thống Thẻ Nâng Cấp (Upgrade Cards).
    /// </summary>
    [CreateAssetMenu(fileName = "CardThemeDatabase", menuName = "ProjectZombie/UI/Card Theme Database")]
    public class CardThemeDatabase : ScriptableObject
    {
        [Header("Card Tier Frames (9-Slice Sprites)")]
        [Tooltip("Khung Gỗ Mộc - Dành cho Nâng cấp Thường (Common)")]
        [SerializeField] private Sprite _frameCommonWood;

        [Tooltip("Khung Ngọc Bích - Dành cho Nâng cấp Hiếm, Lướt, Bí Kíp, Nội Tại (Rare)")]
        [SerializeField] private Sprite _frameRareJade;

        [Tooltip("Khung Hoàng Kim - Dành cho Đột Phá, Tuyệt Kỹ, Tiến Hóa (Breakthrough / Evolution)")]
        [SerializeField] private Sprite _frameEvolutionGold;

        [Tooltip("Khung Hổ Phách - Dành cho Kỹ năng có Duyên Phận Liên Kết (Synergy)")]
        [SerializeField] private Sprite _frameSynergyAmber;

        [Header("Fallback Placeholder Icons")]
        [SerializeField] private Sprite _fallbackWeaponIcon;
        [SerializeField] private Sprite _fallbackPassiveIcon;
        [SerializeField] private Sprite _fallbackDashIcon;
        [SerializeField] private Sprite _fallbackComboIcon;

        [Header("Element & Rarity Accent Colors")]
        [SerializeField] private Color _commonColor = Color.white;
        [SerializeField] private Color _rareColor = new Color(0.2f, 0.85f, 0.6f); // Xanh Ngọc
        [SerializeField] private Color _evolutionColor = new Color(1f, 0.88f, 0.4f); // Vàng Kim Thần Khí
        [SerializeField] private Color _synergyColor = new Color(1f, 0.65f, 0.2f); // Cam Hổ Phách

        [Header("Elemental Glow Colors")]
        [SerializeField] private Color _fireGlow = new Color(1f, 0.35f, 0.15f, 0.6f);
        [SerializeField] private Color _thunderGlow = new Color(0.75f, 0.35f, 1f, 0.6f);
        [SerializeField] private Color _windGlow = new Color(0.25f, 0.95f, 0.65f, 0.6f);
        [SerializeField] private Color _waterGlow = new Color(0.2f, 0.65f, 1f, 0.6f);
        [SerializeField] private Color _defaultGlow = new Color(1f, 1f, 1f, 0.3f);

        // Getters
        public Sprite FrameCommonWood => _frameCommonWood;
        public Sprite FrameRareJade => _frameRareJade;
        public Sprite FrameEvolutionGold => _frameEvolutionGold;
        public Sprite FrameSynergyAmber => _frameSynergyAmber;

        public Sprite FallbackWeaponIcon => _fallbackWeaponIcon;
        public Sprite FallbackPassiveIcon => _fallbackPassiveIcon;
        public Sprite FallbackDashIcon => _fallbackDashIcon;
        public Sprite FallbackComboIcon => _fallbackComboIcon;

        public Color CommonColor => _commonColor;
        public Color RareColor => _rareColor;
        public Color EvolutionColor => _evolutionColor;
        public Color SynergyColor => _synergyColor;

        /// <summary>
        /// Lấy Khung 9-Slice phù hợp theo Loại Nâng cấp và Trạng thái Tiến Hóa/Duyên Phận.
        /// </summary>
        public Sprite GetFrameSprite(UpgradeType upgradeType, bool isEvolution, bool hasSynergy)
        {
            if (isEvolution || upgradeType == UpgradeType.BreakthroughUltimate || upgradeType == UpgradeType.EvolutionUpgrade)
            {
                return _frameEvolutionGold != null ? _frameEvolutionGold : _frameCommonWood;
            }
            if (hasSynergy)
            {
                return _frameSynergyAmber != null ? _frameSynergyAmber : _frameRareJade;
            }
            if (upgradeType == UpgradeType.RareUpgrade ||
                upgradeType == UpgradeType.ComboAugment ||
                upgradeType == UpgradeType.DashTrait ||
                upgradeType == UpgradeType.ConditionalPassive ||
                upgradeType == UpgradeType.SignatureSkillUpgrade)
            {
                return _frameRareJade != null ? _frameRareJade : _frameCommonWood;
            }

            return _frameCommonWood;
        }

        /// <summary>
        /// Lấy Icon dự phòng phù hợp theo phân loại (Category string).
        /// </summary>
        public Sprite GetFallbackIcon(string category)
        {
            if (string.IsNullOrEmpty(category)) return _fallbackPassiveIcon;

            if (category.Contains("LƯỚT") || category.Contains("THÂN PHÁP")) return _fallbackDashIcon;
            if (category.Contains("BÍ KÍP") || category.Contains("COMBO")) return _fallbackComboIcon;
            if (category.Contains("PHÁP BẢO") || category.Contains("VŨ KHÍ")) return _fallbackWeaponIcon;

            return _fallbackPassiveIcon;
        }

        /// <summary>
        /// Lấy màu Glow tương ứng theo hệ nguyên tố.
        /// </summary>
        public Color GetElementGlowColor(string elementName)
        {
            if (string.IsNullOrEmpty(elementName)) return _defaultGlow;

            string lower = elementName.ToLower();
            if (lower.Contains("hỏa") || lower.Contains("fire")) return _fireGlow;
            if (lower.Contains("lôi") || lower.Contains("thunder") || lower.Contains("lightning")) return _thunderGlow;
            if (lower.Contains("phong") || lower.Contains("wind")) return _windGlow;
            if (lower.Contains("thủy") || lower.Contains("băng") || lower.Contains("water") || lower.Contains("ice")) return _waterGlow;

            return _defaultGlow;
        }
    }
}

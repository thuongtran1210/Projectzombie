using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý giao diện Tàng Bảo Các (Kho Pháp Bảo) chuẩn mỹ thuật Cổ Phong Đông Sơn 2 Cột.
    /// </summary>
    public class WeaponLoadoutView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.WeaponLoadout;

        [Header("Hero Info Header")]
        [SerializeField] private Image _heroAvatarImage;
        [SerializeField] private TextMeshProUGUI _heroNameText;
        [SerializeField] private TextMeshProUGUI _heroElementText;

        [Header("Inventory Tabs")]
        [SerializeField] private Button _tabPrimaryButton;
        [SerializeField] private Button _tabRelicsButton;
        [SerializeField] private Image _tabPrimaryBg;
        [SerializeField] private Image _tabRelicsBg;
        [SerializeField] private TextMeshProUGUI _tabPrimaryText;
        [SerializeField] private TextMeshProUGUI _tabRelicsText;

        [Header("Inventory 12-Slot Grid Container")]
        [SerializeField] private Transform _inventoryGridContainer;

        [Header("Equipped Loadout Slots")]
        [SerializeField] private Image _primarySlotIcon;
        [SerializeField] private TextMeshProUGUI _primarySlotName;
        [SerializeField] private Image[] _relicSlotIcons;
        [SerializeField] private TextMeshProUGUI[] _relicSlotNames;

        [Header("Detail Inspection Panel")]
        [SerializeField] private Image _detailIcon;
        [SerializeField] private TextMeshProUGUI _detailNameText;
        [SerializeField] private TextMeshProUGUI _detailTypeText;
        [SerializeField] private TextMeshProUGUI _detailDamageText;
        [SerializeField] private TextMeshProUGUI _detailCooldownText;
        [SerializeField] private Image _damageFillBar;
        [SerializeField] private Image _cooldownFillBar;
        [SerializeField] private TextMeshProUGUI _detailDescText;

        [Header("Action Buttons")]
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Button _backButton;

        public event Action OnStartBattleClicked;
        public event Action OnBackClicked;
        public event Action OnTabPrimaryClicked;
        public event Action OnTabRelicsClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_tabPrimaryButton != null)
                _tabPrimaryButton.onClick.AddListener(() => OnTabPrimaryClicked?.Invoke());

            if (_tabRelicsButton != null)
                _tabRelicsButton.onClick.AddListener(() => OnTabRelicsClicked?.Invoke());

            if (_startBattleButton != null)
                _startBattleButton.onClick.AddListener(() => OnStartBattleClicked?.Invoke());

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(() =>
                {
                    OnBackClicked?.Invoke();
                    OnBackPressed();
                });
            }
        }

        public void DisplayHeroHeader(string heroName, string elementText, Sprite avatar)
        {
            if (_heroNameText != null) _heroNameText.text = heroName;
            if (_heroElementText != null) _heroElementText.text = elementText;
            if (_heroAvatarImage != null)
            {
                _heroAvatarImage.sprite = avatar;
                _heroAvatarImage.enabled = avatar != null;
            }
        }

        public void SetTabState(bool isPrimaryTab)
        {
            Color activeBg = new Color(0.24f, 0.18f, 0.12f, 1f); // Nâu đồng nổi
            Color inactiveBg = new Color(0.12f, 0.10f, 0.16f, 0.9f); // Gỗ trầm chìm
            Color activeTxt = new Color(1f, 0.88f, 0.45f, 1f); // Vàng kim
            Color inactiveTxt = new Color(0.65f, 0.65f, 0.72f, 1f); // Xám bạc

            if (_tabPrimaryBg != null) _tabPrimaryBg.color = isPrimaryTab ? activeBg : inactiveBg;
            if (_tabRelicsBg != null) _tabRelicsBg.color = !isPrimaryTab ? activeBg : inactiveBg;

            if (_tabPrimaryText != null) _tabPrimaryText.color = isPrimaryTab ? activeTxt : inactiveTxt;
            if (_tabRelicsText != null) _tabRelicsText.color = !isPrimaryTab ? activeTxt : inactiveTxt;
        }

        public void DisplayEquippedLoadout(CharacterEntry hero, List<WeaponData> relics)
        {
            // 1. Đòn Đánh Cơ Bản Của Tướng (Ô Lục Giác Trái)
            if (hero != null)
            {
                if (_primarySlotName != null)
                {
                    _primarySlotName.text = !string.IsNullOrEmpty(hero.basicAttackConfig?.attackName) 
                        ? hero.basicAttackConfig.attackName 
                        : $"{hero.characterName} (Đòn Đánh)";
                }

                if (_primarySlotIcon != null)
                {
                    Sprite atkIcon = hero.basicAttackConfig?.attackIcon != null ? hero.basicAttackConfig.attackIcon : hero.avatar;
                    _primarySlotIcon.sprite = atkIcon;
                    _primarySlotIcon.enabled = atkIcon != null;
                    _primarySlotIcon.color = atkIcon != null ? Color.white : new Color(1, 1, 1, 0);
                }
            }

            // 2. Pháp Bảo Hộ Thân (1 Slot Phải)
            if (_relicSlotIcons != null && _relicSlotIcons.Length > 0 && _relicSlotIcons[0] != null)
            {
                bool hasRelic = relics != null && relics.Count > 0 && relics[0] != null;
                if (hasRelic && relics[0].icon != null)
                {
                    _relicSlotIcons[0].sprite = relics[0].icon;
                    _relicSlotIcons[0].enabled = true;
                    _relicSlotIcons[0].color = Color.white;
                }
                else
                {
                    _relicSlotIcons[0].enabled = false;
                    _relicSlotIcons[0].color = new Color(1, 1, 1, 0);
                }

                if (_relicSlotNames != null && _relicSlotNames.Length > 0 && _relicSlotNames[0] != null)
                {
                    _relicSlotNames[0].text = hasRelic ? relics[0].weaponName : "Trống (Chọn 1)";
                }
            }
        }

        public void DisplayEquippedLoadout(WeaponData primary, List<WeaponData> relics)
        {
            // 1. Vũ Khí Chính (Ô Lục Giác)
            if (_primarySlotName != null)
                _primarySlotName.text = primary != null ? primary.weaponName : "Đòn Đánh Tướng";

            if (_primarySlotIcon != null)
            {
                _primarySlotIcon.sprite = primary != null ? primary.icon : null;
                _primarySlotIcon.enabled = primary != null && primary.icon != null;
                _primarySlotIcon.color = (primary != null && primary.icon != null) ? Color.white : new Color(1, 1, 1, 0);
            }

            // 2. Pháp Bảo Hộ Thân (1 Slot)
            if (_relicSlotIcons != null && _relicSlotIcons.Length > 0 && _relicSlotIcons[0] != null)
            {
                bool hasRelic = relics != null && relics.Count > 0 && relics[0] != null;
                if (hasRelic && relics[0].icon != null)
                {
                    _relicSlotIcons[0].sprite = relics[0].icon;
                    _relicSlotIcons[0].enabled = true;
                    _relicSlotIcons[0].color = Color.white;
                }
                else
                {
                    _relicSlotIcons[0].enabled = false;
                    _relicSlotIcons[0].color = new Color(1, 1, 1, 0);
                }

                if (_relicSlotNames != null && _relicSlotNames.Length > 0 && _relicSlotNames[0] != null)
                {
                    _relicSlotNames[0].text = hasRelic ? relics[0].weaponName : "Trống (Chọn 1)";
                }
            }
        }

        public void DisplayWeaponDetail(WeaponData weapon, float damageFill, float cooldownFill)
        {
            if (weapon == null) return;

            if (_detailNameText != null) 
            {
                _detailNameText.text = weapon.weaponName;
                _detailNameText.color = new Color(0.18f, 0.10f, 0.05f, 1f); // Nâu đen gốm tương phản cao
            }

            if (_detailDescText != null) 
            {
                // Làm sạch chuỗi kỹ thuật như (CircleReticle 8.5m) nếu có
                string cleanDesc = weapon.description ?? "";
                cleanDesc = System.Text.RegularExpressions.Regex.Replace(cleanDesc, @"\(CircleReticle[^)]*\)", "");
                cleanDesc = System.Text.RegularExpressions.Regex.Replace(cleanDesc, @"\(Aim[^)]*\)", "");
                _detailDescText.text = cleanDesc.Trim();
                _detailDescText.color = new Color(0.24f, 0.16f, 0.10f, 1f);
            }

            if (_detailDamageText != null) 
                _detailDamageText.text = $"Sát thương: <color=#A33418><b>{weapon.baseDamage}</b></color>";
            if (_detailCooldownText != null) 
                _detailCooldownText.text = $"Hồi chiêu: <color=#0E6073><b>{weapon.baseAttackSpeed:F1}s</b></color>";

            if (_damageFillBar != null) _damageFillBar.fillAmount = Mathf.Clamp01(damageFill);
            if (_cooldownFillBar != null) _cooldownFillBar.fillAmount = Mathf.Clamp01(cooldownFill);

            if (_detailTypeText != null)
            {
                string roleTag = "";
                switch (weapon.weaponRole)
                {
                    case WeaponRole.RelicOrbitalShield:
                        roleTag = "<color=#007A4D><b>[QUỸ ĐẠO HỘ VỆ]</b></color>";
                        break;
                    case WeaponRole.RelicOnHitTrigger:
                        roleTag = "<color=#A86008><b>[KÍCH ỨNG BỒI ĐÒN]</b></color>";
                        break;
                    case WeaponRole.RelicSupportAura:
                        roleTag = "<color=#0E6073><b>[HỖ TRỢ KHỐNG CHẾ]</b></color>";
                        break;
                    default:
                        roleTag = "<color=#A33418><b>[PHÁP BẢO CHỦ ĐỘNG]</b></color>";
                        break;
                }
                
                string elemColor = "#A86008";
                switch (weapon.elementType)
                {
                    case ElementType.Kim: elemColor = "#B8860B"; break;
                    case ElementType.Moc: elemColor = "#2E7D32"; break;
                    case ElementType.Thuy: elemColor = "#1565C0"; break;
                    case ElementType.Hoa: elemColor = "#C62828"; break;
                    case ElementType.Tho: elemColor = "#5D4037"; break;
                }

                _detailTypeText.text = $"{roleTag}  •  <color={elemColor}><b>Hệ {weapon.elementType}</b></color>";
            }

            if (_detailIcon != null)
            {
                _detailIcon.sprite = weapon.icon;
                _detailIcon.enabled = weapon.icon != null;
                _detailIcon.color = weapon.icon != null ? Color.white : new Color(1, 1, 1, 0);
            }
        }

        public Transform InventoryGridContainer => _inventoryGridContainer;
    }
}

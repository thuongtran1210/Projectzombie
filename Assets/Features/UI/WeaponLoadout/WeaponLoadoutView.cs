using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Weapons;

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

        public void DisplayEquippedLoadout(WeaponData primary, List<WeaponData> relics)
        {
            // 1. Vũ Khí Chính (Ô Lục Giác)
            if (_primarySlotName != null)
                _primarySlotName.text = primary != null ? primary.weaponName : "Chưa Chọn";

            if (_primarySlotIcon != null)
            {
                _primarySlotIcon.sprite = primary != null ? primary.icon : null;
                _primarySlotIcon.enabled = primary != null && primary.icon != null;
                _primarySlotIcon.color = (primary != null && primary.icon != null) ? Color.white : new Color(1, 1, 1, 0);
            }

            // 2. Pháp Bảo Hộ Thân (3 Slot)
            if (_relicSlotIcons != null)
            {
                for (int i = 0; i < _relicSlotIcons.Length; i++)
                {
                    if (_relicSlotIcons[i] == null) continue;

                    bool hasRelic = relics != null && i < relics.Count && relics[i] != null;
                    if (hasRelic && relics[i].icon != null)
                    {
                        _relicSlotIcons[i].sprite = relics[i].icon;
                        _relicSlotIcons[i].enabled = true;
                        _relicSlotIcons[i].color = Color.white;
                    }
                    else
                    {
                        _relicSlotIcons[i].enabled = false;
                        _relicSlotIcons[i].color = new Color(1, 1, 1, 0);
                    }

                    if (_relicSlotNames != null && i < _relicSlotNames.Length && _relicSlotNames[i] != null)
                    {
                        _relicSlotNames[i].text = hasRelic ? relics[i].weaponName : "Trống";
                    }
                }
            }
        }

        public void DisplayWeaponDetail(WeaponData weapon, float damageFill, float cooldownFill)
        {
            if (weapon == null) return;

            if (_detailNameText != null) _detailNameText.text = weapon.weaponName;
            if (_detailDescText != null) _detailDescText.text = weapon.description;
            if (_detailDamageText != null) _detailDamageText.text = $"Sát thương: <color=#FFD700>{weapon.baseDamage}</color>";
            if (_detailCooldownText != null) _detailCooldownText.text = $"Hồi chiêu: <color=#4DEEEA>{weapon.baseAttackSpeed:F1}s</color>";

            if (_damageFillBar != null) _damageFillBar.fillAmount = Mathf.Clamp01(damageFill);
            if (_cooldownFillBar != null) _cooldownFillBar.fillAmount = Mathf.Clamp01(cooldownFill);

            if (_detailTypeText != null)
            {
                string roleName = weapon.weaponRole == WeaponRole.PrimaryWeapon 
                    ? "<color=#FF8800>[VŨ KHÍ CHÍNH] (ĐÁNH TAY COMBO)</color>" 
                    : "<color=#00FF88>[PHÁP BẢO] (HỘ THÂN TỰ ĐỘNG)</color>";
                _detailTypeText.text = $"{roleName}\nHệ <color=#FFD700>{weapon.elementType}</color>";
            }

            if (_detailIcon != null)
            {
                _detailIcon.sprite = weapon.icon;
                _detailIcon.enabled = weapon.icon != null;
            }
        }

        public Transform InventoryGridContainer => _inventoryGridContainer;
    }
}

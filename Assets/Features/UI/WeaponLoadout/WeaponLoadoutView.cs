using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý giao diện Tàng Bảo Các (Chọn Vũ Khí Chính & Pháp Bảo Hộ Thân) tuân thủ mô hình MVP.
    /// </summary>
    public class WeaponLoadoutView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.WeaponLoadout;

        [Header("Hero Info Header")]
        [SerializeField] private Image _heroAvatarImage;
        [SerializeField] private TextMeshProUGUI _heroNameText;
        [SerializeField] private TextMeshProUGUI _heroElementText;

        [Header("Equipped Slots (Loadout)")]
        [SerializeField] private Image _primarySlotIcon;
        [SerializeField] private TextMeshProUGUI _primarySlotName;
        [SerializeField] private Image[] _relicSlotIcons;
        [SerializeField] private TextMeshProUGUI[] _relicSlotNames;

        [Header("Selection Containers")]
        [SerializeField] private Transform _primaryWeaponsContainer;
        [SerializeField] private Transform _relicWeaponsContainer;

        [Header("Detail Inspection Panel")]
        [SerializeField] private Image _detailIcon;
        [SerializeField] private TextMeshProUGUI _detailNameText;
        [SerializeField] private TextMeshProUGUI _detailTypeText;
        [SerializeField] private TextMeshProUGUI _detailDamageText;
        [SerializeField] private TextMeshProUGUI _detailCooldownText;
        [SerializeField] private TextMeshProUGUI _detailDescText;

        [Header("Action Buttons")]
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Button _backButton;

        public event Action OnStartBattleClicked;
        public event Action OnBackClicked;
        public event Action<WeaponData> OnWeaponClicked;

        protected override void Awake()
        {
            base.Awake();

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

        public void DisplayEquippedLoadout(WeaponData primary, List<WeaponData> relics)
        {
            // 1. Vũ Khí Chính
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

        public void DisplayWeaponDetail(WeaponData weapon)
        {
            if (weapon == null) return;

            if (_detailNameText != null) _detailNameText.text = weapon.weaponName;
            if (_detailDescText != null) _detailDescText.text = weapon.description;
            if (_detailDamageText != null) _detailDamageText.text = $"Sát thương: <color=#FFD700>{weapon.baseDamage}</color>";
            if (_detailCooldownText != null) _detailCooldownText.text = $"Hồi chiêu: <color=#4DEEEA>{weapon.baseAttackSpeed:F1}s</color>";

            if (_detailTypeText != null)
            {
                string roleName = weapon.weaponRole == WeaponRole.PrimaryWeapon 
                    ? "<color=#FF8800>⚔️ VŨ KHÍ CHÍNH (ĐÁNH TAY COMBO)</color>" 
                    : "<color=#00FF88>🛡️ PHÁP BẢO HỘ THÂN (TỰ ĐỘNG)</color>";
                _detailTypeText.text = $"{roleName} • Hệ {weapon.elementType}";
            }

            if (_detailIcon != null)
            {
                _detailIcon.sprite = weapon.icon;
                _detailIcon.enabled = weapon.icon != null;
            }
        }

        public Transform PrimaryWeaponsContainer => _primaryWeaponsContainer;
        public Transform RelicWeaponsContainer => _relicWeaponsContainer;
    }
}

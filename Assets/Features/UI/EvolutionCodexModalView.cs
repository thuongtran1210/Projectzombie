using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Modal Popup "Bát Quái Đồ Phổ / Cẩm Nang Pháp Bảo" (Evolution Codex)
    /// Hiển thị ma trận 17 công thức tiến hóa kèm trạng thái real-time trong trận.
    /// </summary>
    public class EvolutionCodexModalView : MonoBehaviour
    {
        [System.Serializable]
        public struct CodexItemEntry
        {
            public GameObject rootObject;
            public Image weaponIcon;
            public Image passiveIcon;
            public Image evolutionIcon;
            public TextMeshProUGUI weaponNameText;
            public TextMeshProUGUI passiveNameText;
            public TextMeshProUGUI evolutionNameText;
            public TextMeshProUGUI statusBadgeText;
            public Image backgroundImage;
        }

        [Header("UI Containers")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private Button _closeButton;

        [Header("Colors & Styling")]
        [SerializeField] private Color _unownedColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        [SerializeField] private Color _inProgressColor = new Color(0.15f, 0.35f, 0.55f, 0.8f);
        [SerializeField] private Color _readyToEvolveColor = new Color(0.85f, 0.7f, 0.1f, 0.9f); // Vàng Kim rực rỡ

        private readonly List<CodexItemEntry> _spawnedEntries = new List<CodexItemEntry>();

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        public void Show(WeaponManager weaponManager, PlayerPassives playerPassives)
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            RefreshCodex(weaponManager, playerPassives);
        }

        public void Hide()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        public void Toggle(WeaponManager weaponManager, PlayerPassives playerPassives)
        {
            if (_panelRoot != null && _panelRoot.activeSelf)
            {
                Hide();
            }
            else
            {
                Show(weaponManager, playerPassives);
            }
        }

        public void RefreshCodex(WeaponManager weaponManager, PlayerPassives playerPassives)
        {
            if (WeaponEvolutionManager.Instance == null) return;

            var recipes = WeaponEvolutionManager.Instance.Recipes;
            if (recipes == null) return;

            // Logic cập nhật trạng thái của từng công thức
            for (int i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                bool hasWeapon = weaponManager != null && weaponManager.GetWeaponById(recipe.baseWeaponId) != null;
                WeaponBase currentWeapon = hasWeapon ? weaponManager.GetWeaponById(recipe.baseWeaponId) : null;
                bool isWeaponMaxLevel = currentWeapon != null && currentWeapon.WeaponLevel >= currentWeapon.MaxLevel;

                bool hasPassive = playerPassives != null && playerPassives.HasPassive(recipe.requiredPassiveId);

                // Trạng thái tiến hóa:
                // 1. Ready: Đạt Max Level + Có Passive
                // 2. In Progress: Có 1 trong 2 món
                // 3. Unowned: Chưa có món nào
                if (isWeaponMaxLevel && hasPassive)
                {
                    // SẴN SÀNG TIẾN HÓA
                }
                else if (hasWeapon || hasPassive)
                {
                    // ĐANG THU THẬP
                }
                else
                {
                    // CHƯA SỞ HỮU
                }
            }
        }
    }
}

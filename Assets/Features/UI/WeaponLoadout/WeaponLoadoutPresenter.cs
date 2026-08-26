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
    /// Presenter điều phối toàn bộ luồng logic chọn Vũ Khí Chính & Pháp Bảo Hộ Thân (Tàng Bảo Các) theo mô hình MVP.
    /// </summary>
    public class WeaponLoadoutPresenter : MonoBehaviour
    {
        [SerializeField] private WeaponLoadoutView _view;

        [Header("Weapon Database")]
        [SerializeField] private List<WeaponData> _allWeapons = new List<WeaponData>();

        private CharacterEntry _currentHero;
        private WeaponData _selectedPrimary;
        private readonly List<WeaponData> _selectedRelics = new List<WeaponData>();

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<WeaponLoadoutView>();
                if (_view == null) _view = GetComponentInChildren<WeaponLoadoutView>(true);
            }

            LoadAllWeaponsIfEmpty();

            if (_view != null)
            {
                _view.OnStartBattleClicked += HandleStartBattle;
                _view.OnBackClicked += HandleBack;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnStartBattleClicked -= HandleStartBattle;
                _view.OnBackClicked -= HandleBack;
            }
        }

        private void OnEnable()
        {
            // Tự động đồng bộ Anh Hùng đã chọn từ RunLoadoutState
            if (RunLoadoutState.SelectedCharacter != null)
            {
                SetupForHero(RunLoadoutState.SelectedCharacter);
            }
            else
            {
                RefreshUI();
            }
        }

        public void LoadAllWeaponsIfEmpty()
        {
            if (_allWeapons == null || _allWeapons.Count == 0)
            {
                #if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/_Data/Weapons" });
                _allWeapons = new List<WeaponData>();
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var wd = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                    if (wd != null && !_allWeapons.Contains(wd))
                    {
                        _allWeapons.Add(wd);
                    }
                }
                #else
                var loaded = Resources.LoadAll<WeaponData>("ScriptableObjects/Weapons");
                _allWeapons = new List<WeaponData>(loaded);
                #endif
            }
        }

        /// <summary>
        /// Khởi tạo và đồng bộ Loadout cho Anh Hùng cụ thể (Bước 2 của Flow).
        /// </summary>
        public void SetupForHero(CharacterEntry hero)
        {
            _currentHero = hero;
            LoadAllWeaponsIfEmpty();

            // 1. Gán Vũ Khí Chính mặc định
            if (hero != null && hero.defaultPrimaryWeapon != null)
            {
                _selectedPrimary = hero.defaultPrimaryWeapon;
            }
            else if (_selectedPrimary == null)
            {
                _selectedPrimary = _allWeapons.Find(w => w.weaponRole == WeaponRole.PrimaryWeapon);
            }

            // 2. Gán Pháp Bảo Hộ Thân mặc định
            _selectedRelics.Clear();
            if (hero != null && hero.defaultRelics != null && hero.defaultRelics.Count > 0)
            {
                foreach (var r in hero.defaultRelics)
                {
                    if (r != null && !_selectedRelics.Contains(r) && _selectedRelics.Count < 3)
                    {
                        _selectedRelics.Add(r);
                    }
                }
            }

            // Fallback nếu danh sách relics trống: lấy 3 relic đầu tiên
            if (_selectedRelics.Count == 0)
            {
                foreach (var w in _allWeapons)
                {
                    if (w.weaponRole != WeaponRole.PrimaryWeapon && !_selectedRelics.Contains(w) && _selectedRelics.Count < 3)
                    {
                        _selectedRelics.Add(w);
                    }
                }
            }

            RefreshUI();
        }

        public void SelectPrimaryWeapon(WeaponData weapon)
        {
            if (weapon == null) return;
            _selectedPrimary = weapon;
            RefreshUI();
            if (_view != null) _view.DisplayWeaponDetail(weapon);
        }

        public void ToggleRelic(WeaponData relic)
        {
            if (relic == null) return;

            if (_selectedRelics.Contains(relic))
            {
                _selectedRelics.Remove(relic);
            }
            else
            {
                if (_selectedRelics.Count >= 3)
                {
                    _selectedRelics.RemoveAt(0); // Thay thế vị trí đầu tiên nếu đã đầy 3 slot
                }
                _selectedRelics.Add(relic);
            }

            RefreshUI();
            if (_view != null) _view.DisplayWeaponDetail(relic);
        }

        private void RefreshUI()
        {
            if (_view == null) return;

            // 1. Cập nhật Header
            if (_currentHero != null)
            {
                string elemStr = $"<color={_currentHero.elementHexColor}>Hệ {_currentHero.element}</color>";
                _view.DisplayHeroHeader(_currentHero.characterName, elemStr, _currentHero.avatar);
            }

            // 2. Cập nhật 4 Ô Trang Bị
            _view.DisplayEquippedLoadout(_selectedPrimary, _selectedRelics);

            // 3. Hiển thị chi tiết vũ khí đang chọn
            if (_selectedPrimary != null)
            {
                _view.DisplayWeaponDetail(_selectedPrimary);
            }
            else if (_selectedRelics.Count > 0)
            {
                _view.DisplayWeaponDetail(_selectedRelics[0]);
            }

            // 4. Sinh Grid Buttons danh sách vũ khí
            PopulateWeaponsGrid();
        }

        private void PopulateWeaponsGrid()
        {
            if (_view == null) return;

            // Grid Vũ Khí Chính
            if (_view.PrimaryWeaponsContainer != null)
            {
                ClearChildren(_view.PrimaryWeaponsContainer);
                foreach (var w in _allWeapons)
                {
                    if (w.weaponRole == WeaponRole.PrimaryWeapon)
                    {
                        CreateWeaponItemButton(w, _view.PrimaryWeaponsContainer, isPrimary: true);
                    }
                }
            }

            // Grid Pháp Bảo Hộ Thân
            if (_view.RelicWeaponsContainer != null)
            {
                ClearChildren(_view.RelicWeaponsContainer);
                foreach (var w in _allWeapons)
                {
                    if (w.weaponRole != WeaponRole.PrimaryWeapon)
                    {
                        CreateWeaponItemButton(w, _view.RelicWeaponsContainer, isPrimary: false);
                    }
                }
            }
        }

        private void CreateWeaponItemButton(WeaponData weapon, Transform parent, bool isPrimary)
        {
            if (weapon == null || parent == null) return;

            GameObject itemObj = new GameObject($"Item_{weapon.weaponId}", typeof(RectTransform), typeof(Image), typeof(Button));
            itemObj.transform.SetParent(parent, false);

            var rt = itemObj.GetComponent<RectTransform>();
            rt.sizeDelta = isPrimary ? new Vector2(72, 72) : new Vector2(74, 74);

            bool isEquipped = isPrimary ? (weapon == _selectedPrimary) : _selectedRelics.Contains(weapon);
            var bgImg = itemObj.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.color = isEquipped ? new Color(0.95f, 0.78f, 0.25f, 1f) : new Color(0.24f, 0.20f, 0.32f, 0.95f);
            }

            // Khung đệm bên trong
            GameObject innerObj = new GameObject("InnerBg", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(itemObj.transform, false);
            var inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = isEquipped ? new Vector2(3, 3) : new Vector2(2, 2);
            inRT.offsetMax = isEquipped ? new Vector2(-3, -3) : new Vector2(-2, -2);
            var inImg = innerObj.GetComponent<Image>();
            if (inImg != null)
            {
                inImg.color = isEquipped ? new Color(0.18f, 0.14f, 0.26f, 1f) : new Color(0.12f, 0.10f, 0.16f, 1f);
            }

            // Icon chính
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(innerObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(4, 4);
            iconRT.offsetMax = new Vector2(-4, -4);

            var iconImg = iconObj.GetComponent<Image>();
            if (iconImg != null)
            {
                iconImg.sprite = weapon.icon;
                iconImg.enabled = weapon.icon != null;
                iconImg.color = Color.white;
                iconImg.preserveAspect = true;
            }

            // Badge viền vàng khi trang bị
            if (isEquipped)
            {
                GameObject checkObj = new GameObject("Badge_Equipped", typeof(RectTransform), typeof(Image));
                checkObj.transform.SetParent(itemObj.transform, false);
                var cRT = checkObj.GetComponent<RectTransform>();
                cRT.anchorMin = new Vector2(1, 1);
                cRT.anchorMax = new Vector2(1, 1);
                cRT.pivot = new Vector2(1, 1);
                cRT.anchoredPosition = new Vector2(2, 2);
                cRT.sizeDelta = new Vector2(16, 16);
                var cImg = checkObj.GetComponent<Image>();
                if (cImg != null)
                {
                    cImg.color = new Color(0.95f, 0.78f, 0.25f, 1f);
                }
            }

            var btn = itemObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (isPrimary) SelectPrimaryWeapon(weapon);
                    else ToggleRelic(weapon);
                });
            }
        }

        private void ClearChildren(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        private void HandleStartBattle()
        {
            Debug.Log($"<color=#00FF88>[WeaponLoadoutPresenter]</color> XÁC NHẬN XUẤT TRẬN: Hero={_currentHero?.characterName}, Primary={_selectedPrimary?.weaponName}, Relics Count={_selectedRelics.Count}");

            // Lưu toàn bộ cấu hình vào RunLoadoutState
            RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics);

            // Kích hoạt Transition Overlay chuyển vào Gameplay
            if (MetaSceneTransitionController.Instance != null)
            {
                MetaSceneTransitionController.Instance.StartRun();
            }
            else
            {
                var transitionCtrl = FindObjectOfType<MetaSceneTransitionController>();
                if (transitionCtrl != null)
                {
                    transitionCtrl.StartRun();
                }
                else if (MetaUIManager.Instance != null)
                {
                    // Fallback: Ẩn Canvas Meta Menu nếu không tìm thấy Transition Controller
                    MetaUIManager.Instance.SetMetaCanvasActive(false);
                }
            }
        }

        private void HandleBack()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
        }
    }
}

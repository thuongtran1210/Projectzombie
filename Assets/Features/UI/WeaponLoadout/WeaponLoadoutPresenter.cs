using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    public enum LoadoutInventoryTab
    {
        PrimaryWeapons,
        Relics
    }

    /// <summary>
    /// Presenter điều phối toàn bộ luồng logic Tàng Bảo Các (Kho Pháp Bảo) chuẩn 2 Cột Đối Xứng (MVP).
    /// </summary>
    public class WeaponLoadoutPresenter : MonoBehaviour
    {
        [SerializeField] private WeaponLoadoutView _view;

        [Header("Weapon Database")]
        [SerializeField] private List<WeaponData> _allWeapons = new List<WeaponData>();

        private CharacterEntry _currentHero;
        private WeaponData _selectedPrimary;
        private readonly List<WeaponData> _selectedRelics = new List<WeaponData>();
        private WeaponData _inspectedWeapon;
        private LoadoutInventoryTab _currentTab = LoadoutInventoryTab.PrimaryWeapons;

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
                _view.OnTabPrimaryClicked += () => SetTab(LoadoutInventoryTab.PrimaryWeapons);
                _view.OnTabRelicsClicked += () => SetTab(LoadoutInventoryTab.Relics);
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

        public void SetupForHero(CharacterEntry hero)
        {
            _currentHero = hero;
            LoadAllWeaponsIfEmpty();

            // 1. Vũ Khí Chính Mặc Định
            if (hero != null && hero.defaultPrimaryWeapon != null)
            {
                _selectedPrimary = hero.defaultPrimaryWeapon;
            }
            else if (_selectedPrimary == null)
            {
                _selectedPrimary = _allWeapons.Find(w => w.weaponRole == WeaponRole.PrimaryWeapon);
            }

            // 2. Pháp Bảo Hộ Thân Mặc Định (Chỉ 1 Slot)
            _selectedRelics.Clear();
            if (hero != null && hero.defaultRelic != null)
            {
                _selectedRelics.Add(hero.defaultRelic);
            }
            else if (hero != null && hero.defaultRelics != null && hero.defaultRelics.Count > 0)
            {
                _selectedRelics.Add(hero.defaultRelics[0]);
            }

            if (_selectedRelics.Count == 0)
            {
                var defaultR = _allWeapons.Find(w => w.weaponRole != WeaponRole.PrimaryWeapon);
                if (defaultR != null) _selectedRelics.Add(defaultR);
            }

            _inspectedWeapon = _selectedRelics.Count > 0 ? _selectedRelics[0] : _selectedPrimary;
            _currentTab = LoadoutInventoryTab.Relics;
            RefreshUI();
        }

        public void SetTab(LoadoutInventoryTab tab)
        {
            _currentTab = tab;
            RefreshUI();
        }

        public void SelectPrimaryWeapon(WeaponData weapon)
        {
            if (weapon == null) return;
            _selectedPrimary = weapon;
            _inspectedWeapon = weapon;
            RefreshUI();
        }

        public void ToggleRelic(WeaponData relic)
        {
            if (relic == null) return;

            // Cơ chế 1 Pháp Bảo Duy Nhất: Chọn cái mới sẽ thay thế cái cũ
            _selectedRelics.Clear();
            _selectedRelics.Add(relic);

            _inspectedWeapon = relic;
            RefreshUI();
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

            // 2. Cập nhật Tab State (Tab Pháp Bảo Duy Nhất)
            _view.SetTabState(false);

            // 3. Cập nhật Trang Bị: Đòn Đánh Tướng (Trái) & 1 Pháp Bảo Hộ Thân (Phải)
            if (_currentHero != null)
            {
                _view.DisplayEquippedLoadout(_currentHero, _selectedRelics);
            }
            else
            {
                _view.DisplayEquippedLoadout(_selectedPrimary, _selectedRelics);
            }

            // 4. Cập nhật Chi Tiết Soi Chỉ Số
            if (_inspectedWeapon != null)
            {
                float dmgFill = Mathf.Clamp01(_inspectedWeapon.baseDamage / 35f);
                float cdFill = 1f - Mathf.Clamp01(_inspectedWeapon.baseAttackSpeed / 2.5f);
                _view.DisplayWeaponDetail(_inspectedWeapon, dmgFill, cdFill);
            }

            // 5. Sinh Grid 12 Ô Vật Phẩm (Chỉ hiển thị Pháp Bảo)
            Populate12SlotInventoryGrid();
        }

        private void Populate12SlotInventoryGrid()
        {
            if (_view == null || _view.InventoryGridContainer == null) return;

            ClearChildren(_view.InventoryGridContainer);

            // Lọc danh sách Pháp Bảo
            var targetList = new List<WeaponData>();
            foreach (var w in _allWeapons)
            {
                if (w != null && w.weaponRole != WeaponRole.PrimaryWeapon)
                {
                    targetList.Add(w);
                }
            }

            // Nếu danh sách rỗng, nạp tất cả
            if (targetList.Count == 0) targetList.AddRange(_allWeapons);

            // Tạo đúng 12 Ô (4 Cột x 3 Hàng)
            int totalSlots = 12;
            for (int i = 0; i < totalSlots; i++)
            {
                if (i < targetList.Count)
                {
                    CreateItemSlot(targetList[i], _view.InventoryGridContainer, isLocked: false);
                }
                else
                {
                    CreateItemSlot(null, _view.InventoryGridContainer, isLocked: true);
                }
            }
        }

        private void CreateItemSlot(WeaponData weapon, Transform parent, bool isLocked)
        {
            GameObject slotObj = new GameObject(isLocked ? "Slot_Locked" : $"Slot_{weapon.weaponId}", typeof(RectTransform));
            slotObj.transform.SetParent(parent, false);

            var slotRT = slotObj.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(80, 96); // 80px ô + 16px nhãn bên dưới

            // Khung Ô Vật Phẩm (80 x 80)
            GameObject boxObj = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(Button));
            boxObj.transform.SetParent(slotObj.transform, false);
            var boxRT = boxObj.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 1f);
            boxRT.anchorMax = new Vector2(0.5f, 1f);
            boxRT.pivot = new Vector2(0.5f, 1f);
            boxRT.anchoredPosition = Vector2.zero;
            boxRT.sizeDelta = new Vector2(80, 80);

            var boxImg = boxObj.GetComponent<Image>();
            Color borderColor = isLocked ? new Color(0.25f, 0.22f, 0.30f, 0.8f) : GetElementColor(weapon.elementType);
            boxImg.color = borderColor;

            // Nền bên trong (Inner Background)
            GameObject innerObj = new GameObject("InnerBg", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(boxObj.transform, false);
            var inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(3, 3);
            inRT.offsetMax = new Vector2(-3, -3);

            var inImg = innerObj.GetComponent<Image>();
            inImg.color = isLocked ? new Color(0.08f, 0.07f, 0.10f, 0.95f) : new Color(0.14f, 0.11f, 0.18f, 0.95f);

            // Icon bên trong
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(innerObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(6, 6);
            iconRT.offsetMax = new Vector2(-6, -6);

            var iconImg = iconObj.GetComponent<Image>();
            if (isLocked)
            {
                iconImg.color = new Color(0.35f, 0.30f, 0.40f, 0.6f);
                // Có thể load sprite ổ khóa nếu có
            }
            else
            {
                iconImg.sprite = weapon.icon;
                iconImg.enabled = weapon.icon != null;
                iconImg.color = Color.white;
                iconImg.preserveAspect = true;
            }

            // Nhãn text bên dưới ô (Weapon Name / Element / Khóa)
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(slotObj.transform, false);
            var lblRT = lblObj.GetComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0, 0);
            lblRT.anchorMax = new Vector2(1, 0);
            lblRT.pivot = new Vector2(0.5f, 0);
            lblRT.anchoredPosition = Vector2.zero;
            lblRT.sizeDelta = new Vector2(0, 16);

            var lblTMP = lblObj.GetComponent<TextMeshProUGUI>();
            lblTMP.fontSize = 11;
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.fontStyle = FontStyles.Bold;

            if (isLocked)
            {
                lblTMP.text = "<color=#666677>Khóa</color>";
            }
            else
            {
                lblTMP.text = $"<color=#{ColorUtility.ToHtmlStringRGB(borderColor)}>{weapon.weaponName}</color>";
            }

            // Xử lý Click
            var btn = boxObj.GetComponent<Button>();
            if (!isLocked && btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (weapon.weaponRole == WeaponRole.PrimaryWeapon)
                    {
                        SelectPrimaryWeapon(weapon);
                    }
                    else
                    {
                        ToggleRelic(weapon);
                    }
                });
            }
        }

        private Color GetElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim: return new Color(1.0f, 0.84f, 0.0f, 1f); // Vàng Kim
                case ElementType.Moc: return new Color(0.30f, 0.75f, 0.35f, 1f); // Xanh Lục
                case ElementType.Thuy: return new Color(0.20f, 0.65f, 0.95f, 1f); // Xanh Lam
                case ElementType.Hoa: return new Color(0.95f, 0.28f, 0.22f, 1f); // Đỏ Chu Sa
                case ElementType.Tho: return new Color(0.65f, 0.48f, 0.32f, 1f); // Nâu Đất Đồng
                default: return new Color(0.85f, 0.85f, 0.90f, 1f);
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

            RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics);

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
                    MetaUIManager.Instance.SetMetaCanvasActive(false);
                }
            }
        }

        private void HandleBack()
        {
            RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics);

            var mainHubPresenter = FindObjectOfType<MainHubPresenter>(true);
            if (mainHubPresenter != null)
            {
                mainHubPresenter.RefreshHubState();
            }

            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
        }
    }
}

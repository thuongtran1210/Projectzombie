using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI
{
    public partial class CardCodexView
    {
        [Serializable] public struct CatalogIcon { public string Id; public Sprite Icon; }
        [Header("Loadout and catalog (authored in prefab)")]
        [SerializeField] private Button _towerTabButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private TextMeshProUGUI _equipLabel;
        [SerializeField] private TextMeshProUGUI _equipStatus;
        [SerializeField] private TextMeshProUGUI _loadoutSummary;
        [SerializeField] private CodexLoadoutSlotView[] _heroSlots;
        [SerializeField] private CodexLoadoutSlotView[] _relicSlots;
        [SerializeField] private CodexLoadoutSlotView[] _minionSlots;
        [SerializeField] private CodexLoadoutSlotView[] _towerSlots;
        [SerializeField] private CatalogIcon[] _catalogIcons;
        [SerializeField] private CodexSlotItemView _cardSlotPrefab;
        private Action<CodexTabType, string> _slotCallback;
        public event Action OnEquipClicked;
        public event Action<CodexTabType, string> OnLoadoutSlotClicked;

        private void EnsureLoadoutControls()
        {
            _slotCallback = HandleSlotClick;
            if (_towerTabButton != null) _towerTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(CodexTabType.Towers));
            if (_equipButton != null) _equipButton.onClick.AddListener(() => OnEquipClicked?.Invoke());
        }
        private void HandleSlotClick(CodexTabType tab, string id) => OnLoadoutSlotClicked?.Invoke(tab, id);
        private void UpdateTowerTab(CodexTabType tab, Sprite active, Sprite inactive)
        {
            if (_towerTabButton == null) return;
            var img = _towerTabButton.GetComponent<Image>();
            var sprite = tab == CodexTabType.Towers ? active : inactive;
            if (img != null)
            {
                if (sprite != null) img.sprite = sprite;
                img.color = tab == CodexTabType.Towers ? new Color(1,.84f,.56f) : new Color(.67f,.67f,.67f);
            }
            var label = _towerTabButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.color = tab == CodexTabType.Towers ? new Color(.98f,.88f,.5f) : new Color(.85f,.78f,.65f);
        }
        public Sprite GetCatalogIcon(string id)
        {
            if (_catalogIcons != null) foreach (var item in _catalogIcons) if (item.Id == id) return item.Icon;
            return null;
        }
        public void ShowLoadoutSummary(string summary)
        {
            if (_loadoutSummary != null) _loadoutSummary.text = summary;
        }
        public void ShowLoadoutSlots(string hero, string relic, IReadOnlyList<string> minions,
            IReadOnlyList<string> towers, int maxMinions, int maxTowers, Func<string,string> name, Func<string,Sprite> icon)
        {
            BindSlot(_heroSlots, 0, CodexTabType.HeroCards, hero, name, icon);
            BindSlot(_relicSlots, 0, CodexTabType.RelicFusion, relic, name, icon);
            BindGroup(_minionSlots, CodexTabType.Minions, minions, maxMinions, name, icon);
            BindGroup(_towerSlots, CodexTabType.Towers, towers, maxTowers, name, icon);
        }
        private void BindGroup(CodexLoadoutSlotView[] slots, CodexTabType tab, IReadOnlyList<string> ids, int max,
            Func<string,string> name, Func<string,Sprite> icon)
        {
            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].gameObject.SetActive(i < max);
                if (i < max) BindSlot(slots, i, tab, i < ids.Count ? ids[i] : null, name, icon);
            }
        }
        private void BindSlot(CodexLoadoutSlotView[] slots, int index, CodexTabType tab, string id,
            Func<string,string> name, Func<string,Sprite> icon)
        {
            if (slots == null || index >= slots.Length || slots[index] == null) return;
            slots[index].Bind(tab, id, id == null ? "Thêm" : name(id), id == null ? null : icon(id), _slotCallback);
        }
        public void ShowEquipAction(string label, bool enabled)
        {
            if (_equipButton == null) return;
            bool unsupported = label == "CHƯA HỖ TRỢ TRONG DEFENSE";
            _equipButton.gameObject.SetActive(!unsupported);
            _equipButton.interactable = enabled;
            if (_equipLabel != null) _equipLabel.text = label;
            if (_equipStatus != null)
            {
                _equipStatus.gameObject.SetActive(unsupported);
                _equipStatus.text = "Thẻ sưu tập • Chưa thể mang vào trận";
            }
        }
        private CodexSlotItemView AllocateCardSlot() => _cardSlotPrefab != null
            ? Instantiate(_cardSlotPrefab, _cardsGridContainer) : CodexSlotItemView.CreateDynamicSlot(_cardsGridContainer);
    }
}

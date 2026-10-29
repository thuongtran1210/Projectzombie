using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    public struct RelicSlotViewModel
    {
        public string WeaponId;
        public string WeaponName;
        public Sprite Icon;
        public ProjectZombie.Features.Shared.ItemRarity Rarity;
        public Sprite ElementBadge;
        public Color NameColor;
        public int StarLevel;
        public int ShardCount;
        public int ReqShards;
        public bool IsUnlocked;
        public bool IsSelected;
    }

    public struct HeroSlotViewModel
    {
        public string CharacterId;
        public string CharacterName;
        public Sprite Avatar;
        public ProjectZombie.Features.Shared.ItemRarity Rarity;
        public Sprite ElementBadge;
        public Color NameColor;
        public int StarLevel;
        public int ShardCount;
        public int ReqShards;
        public bool IsUnlocked;
        public bool IsSelected;
    }

    public struct UpgradeSlotViewModel
    {
        public string UpgradeId;
        public string UpgradeName;
        public Sprite Icon;
        public bool IsSelected;
    }

    /// <summary>
    /// Passive View quản lý hiển thị một ô Thần Thẻ / Pháp Bảo / Thẻ Tướng trong Grid Bách Bảo Các.
    /// Chuẩn MVP: Không chứa logic nghiệp vụ hay truy xuất Model.
    /// </summary>
    public class CodexSlotItemView : MonoBehaviour
    {
        [SerializeField] private Image _boxImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _elementBadgeImage;
        [SerializeField] private TextMeshProUGUI _starText;
        [SerializeField] private TextMeshProUGUI _shardProgressText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _equippedBadgeText;
        [SerializeField] private TextMeshProUGUI _lockedBadgeText;
        [SerializeField] private Button _clickButton;
        [SerializeField] private Sprite _normalSlotSprite;
        [SerializeField] private Sprite _selectedSlotSprite;
        private Action _onClickedCallback;
        private bool _equipped;
        private void Awake() => EnsureButtonListener();
        public void EnsureButtonListener()
        {
            if (_clickButton == null) _clickButton = GetComponentInChildren<Button>(true);
            if (_clickButton == null) return;
            _clickButton.onClick.RemoveListener(HandleClick);
            _clickButton.onClick.AddListener(HandleClick);
        }
        private void HandleClick() => _onClickedCallback?.Invoke();
        public void BindRelic(RelicSlotViewModel vm, Sprite normal, Sprite selected, Action click)
            => Bind(vm.WeaponName, vm.Icon, vm.ElementBadge, vm.StarLevel, vm.IsUnlocked, vm.IsSelected, normal, selected, click);
        public void BindHero(HeroSlotViewModel vm, Sprite normal, Sprite selected, Action click)
            => Bind(vm.CharacterName, vm.Avatar, vm.ElementBadge, vm.StarLevel, vm.IsUnlocked, vm.IsSelected, normal, selected, click);
        public void BindUpgrade(UpgradeSlotViewModel vm, Sprite normal, Sprite selected, Action click)
            => Bind(vm.UpgradeName, vm.Icon, null, 0, true, vm.IsSelected, normal, selected, click);
        private void Bind(string name, Sprite icon, Sprite element, int stars, bool owned, bool inspected,
            Sprite normal, Sprite selected, Action click)
        {
            _onClickedCallback = click; _normalSlotSprite = normal; _selectedSlotSprite = selected;
            EnsureButtonListener();
            if (_iconImage != null) { _iconImage.sprite = icon; _iconImage.enabled = icon != null; _iconImage.color = owned ? Color.white : new Color(.65f,.65f,.65f,.8f); }
            if (_elementBadgeImage != null) { _elementBadgeImage.sprite = element; _elementBadgeImage.gameObject.SetActive(element != null); }
            if (_starText != null) { _starText.text = stars + " Sao"; _starText.gameObject.SetActive(stars > 0); }
            if (_shardProgressText != null) _shardProgressText.gameObject.SetActive(false);
            SetEquipped(false);
            SetSelected(inspected, owned, null, name);
        }
        public void SetEquipped(bool equipped)
        {
            _equipped = equipped;
            if (_equippedBadgeText != null) _equippedBadgeText.gameObject.SetActive(equipped);
            if (equipped && _lockedBadgeText != null) _lockedBadgeText.gameObject.SetActive(false);
        }
        public void SetSelected(bool isSelected, bool isUnlocked = true, Color? customNameColor = null, string baseName = null)
        {
            if (_boxImage != null)
            {
                var sprite = isSelected ? _selectedSlotSprite : _normalSlotSprite;
                if (sprite != null) _boxImage.sprite = sprite;
                _boxImage.color = isSelected || isUnlocked ? Color.white : new Color(.72f,.72f,.72f,.85f);
            }
            if (_nameText != null && baseName != null)
            {
                _nameText.text = baseName;
                _nameText.color = isSelected ? Color.white : new Color(.89f,.85f,.75f);
            }
            if (_lockedBadgeText != null) _lockedBadgeText.gameObject.SetActive(!isUnlocked && !_equipped);
        }

        public static CodexSlotItemView CreateDynamicSlot(Transform parent)
        {
            GameObject slotObj = new GameObject("CodexSlotItem", typeof(RectTransform));
            slotObj.transform.SetParent(parent, false);

            var slotRT = slotObj.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(120, 140);

            // Box
            GameObject boxObj = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(Button));
            boxObj.transform.SetParent(slotObj.transform, false);
            var boxRT = boxObj.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 1f);
            boxRT.anchorMax = new Vector2(0.5f, 1f);
            boxRT.pivot = new Vector2(0.5f, 1f);
            boxRT.anchoredPosition = Vector2.zero;
            boxRT.sizeDelta = new Vector2(110, 110);

            var boxImg = boxObj.GetComponent<Image>();
            boxImg.type = Image.Type.Sliced;
            boxImg.raycastTarget = true;
            var boxBtn = boxObj.GetComponent<Button>();

            // InnerBg
            GameObject innerObj = new GameObject("InnerBg", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(boxObj.transform, false);
            var inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(5, 5);
            inRT.offsetMax = new Vector2(-5, -5);

            var inImg = innerObj.GetComponent<Image>();
            inImg.color = new Color(0.12f, 0.09f, 0.16f, 0.85f);
            inImg.raycastTarget = false;

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(innerObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(6, 6);
            iconRT.offsetMax = new Vector2(-6, -6);
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Element Badge
            GameObject elemBadgeObj = new GameObject("Badge_Element", typeof(RectTransform), typeof(Image));
            elemBadgeObj.transform.SetParent(boxObj.transform, false);
            var ebRT = elemBadgeObj.GetComponent<RectTransform>();
            ebRT.anchorMin = new Vector2(0, 1);
            ebRT.anchorMax = new Vector2(0, 1);
            ebRT.pivot = new Vector2(0, 1);
            ebRT.anchoredPosition = new Vector2(2, -2);
            ebRT.sizeDelta = new Vector2(24, 24);
            var ebImg = elemBadgeObj.GetComponent<Image>();
            ebImg.preserveAspect = true;
            ebImg.raycastTarget = false;
            elemBadgeObj.SetActive(false);

            // Star Badge
            GameObject starObj = new GameObject("Badge_Star", typeof(RectTransform), typeof(TextMeshProUGUI));
            starObj.transform.SetParent(boxObj.transform, false);
            var starRT = starObj.GetComponent<RectTransform>();
            starRT.anchorMin = new Vector2(1, 1);
            starRT.anchorMax = new Vector2(1, 1);
            starRT.pivot = new Vector2(1, 1);
            starRT.anchoredPosition = new Vector2(-4, -2);
            starRT.sizeDelta = new Vector2(48, 20);
            var starTMP = starObj.GetComponent<TextMeshProUGUI>();
            starTMP.fontSize = 12;
            starTMP.alignment = TextAlignmentOptions.Right;
            starTMP.raycastTarget = false;
            starObj.SetActive(false);

            // Shard Progress
            GameObject shardObj = new GameObject("Txt_Shards", typeof(RectTransform), typeof(TextMeshProUGUI));
            shardObj.transform.SetParent(boxObj.transform, false);
            var shardRT = shardObj.GetComponent<RectTransform>();
            shardRT.anchorMin = new Vector2(0, 0);
            shardRT.anchorMax = new Vector2(1, 0);
            shardRT.pivot = new Vector2(0.5f, 0);
            shardRT.anchoredPosition = new Vector2(0, 3);
            shardRT.sizeDelta = new Vector2(-8, 16);
            var shardTMP = shardObj.GetComponent<TextMeshProUGUI>();
            shardTMP.fontSize = 11f;
            shardTMP.alignment = TextAlignmentOptions.Center;
            shardTMP.raycastTarget = false;

            // Name Label
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(slotObj.transform, false);
            var lblRT = lblObj.GetComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0, 0);
            lblRT.anchorMax = new Vector2(1, 0);
            lblRT.pivot = new Vector2(0.5f, 0);
            lblRT.anchoredPosition = Vector2.zero;
            lblRT.sizeDelta = new Vector2(0, 24);
            var lblTMP = lblObj.GetComponent<TextMeshProUGUI>();
            lblTMP.fontSize = 13f;
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.overflowMode = TextOverflowModes.Ellipsis;
            lblTMP.raycastTarget = false;

            boxBtn.targetGraphic = boxImg;

            // Attach & Bind Fields
            var slotView = slotObj.AddComponent<CodexSlotItemView>();
            slotView._boxImage = boxImg;
            slotView._iconImage = iconImg;
            slotView._elementBadgeImage = ebImg;
            slotView._starText = starTMP;
            slotView._shardProgressText = shardTMP;
            slotView._nameText = lblTMP;
            slotView._clickButton = boxBtn;
            slotView.EnsureButtonListener();

            return slotView;
        }
    }
}

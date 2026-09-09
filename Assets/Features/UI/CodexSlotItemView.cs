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
    /// Passive View quản lý hiển thị một ô Thần Thẻ / Pháp Bảo trong Grid Bách Bảo Các.
    /// Chuẩn MVP: Không chứa logic nghiệp vụ hay truy xuất Model.
    /// </summary>
    public class CodexSlotItemView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image _boxImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _elementBadgeImage;
        [SerializeField] private TextMeshProUGUI _starText;
        [SerializeField] private TextMeshProUGUI _shardProgressText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Button _clickButton;

        [Header("Sprites Config")]
        [SerializeField] private Sprite _normalSlotSprite;
        [SerializeField] private Sprite _selectedSlotSprite;

        private Action _onClickedCallback;

        private void Awake()
        {
            if (_clickButton != null)
            {
                _clickButton.onClick.AddListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            _onClickedCallback?.Invoke();
        }

        public void BindRelic(RelicSlotViewModel vm, Sprite normalSlot, Sprite selectedSlot, Action onClicked)
        {
            _onClickedCallback = onClicked;
            _normalSlotSprite = normalSlot;
            _selectedSlotSprite = selectedSlot;

            // 1. Box State
            UpdateBoxVisual(vm.IsSelected, vm.IsUnlocked);

            // 2. Icon
            if (_iconImage != null)
            {
                _iconImage.sprite = vm.Icon;
                _iconImage.enabled = vm.Icon != null;
                _iconImage.color = vm.IsUnlocked ? Color.white : new Color(0.35f, 0.30f, 0.35f, 0.45f);
            }

            // 3. Element Badge
            if (_elementBadgeImage != null)
            {
                if (vm.IsUnlocked && vm.ElementBadge != null)
                {
                    _elementBadgeImage.sprite = vm.ElementBadge;
                    _elementBadgeImage.gameObject.SetActive(true);
                }
                else
                {
                    _elementBadgeImage.gameObject.SetActive(false);
                }
            }

            // 4. Star Badge
            if (_starText != null)
            {
                if (vm.StarLevel > 0)
                {
                    _starText.text = $"<color=#FFD700><b>{vm.StarLevel}★</b></color>";
                    _starText.gameObject.SetActive(true);
                }
                else
                {
                    _starText.gameObject.SetActive(false);
                }
            }

            // 5. Shards Progress
            if (_shardProgressText != null)
            {
                if (vm.StarLevel >= 5)
                {
                    _shardProgressText.text = "<color=#00FF88><b>TỐI ĐA</b></color>";
                }
                else
                {
                    string colorHex = vm.ShardCount >= vm.ReqShards ? "00FF88" : "FFAA00";
                    _shardProgressText.text = $"<color=#{colorHex}><b>{vm.ShardCount}/{vm.ReqShards}</b></color>";
                }
            }

            // 6. Label Name
            if (_nameText != null)
            {
                if (!vm.IsUnlocked)
                {
                    _nameText.text = vm.IsSelected ? "<color=#FFCC88>Chưa Mở Khóa</color>" : "<color=#665544>Chưa Mở Khóa</color>";
                }
                else
                {
                    string nameColorHex = vm.IsSelected ? "FFFFFF" : ColorUtility.ToHtmlStringRGB(vm.NameColor);
                    _nameText.text = $"<color=#{nameColorHex}>{vm.WeaponName}</color>";
                }
            }
        }

        public void BindUpgrade(UpgradeSlotViewModel vm, Sprite normalSlot, Sprite selectedSlot, Action onClicked)
        {
            _onClickedCallback = onClicked;
            _normalSlotSprite = normalSlot;
            _selectedSlotSprite = selectedSlot;

            // 1. Box State
            UpdateBoxVisual(vm.IsSelected, true);

            // 2. Icon
            if (_iconImage != null)
            {
                _iconImage.sprite = vm.Icon;
                _iconImage.enabled = vm.Icon != null;
                _iconImage.color = Color.white;
            }

            // 3. Hide Element & Star for basic upgrades
            if (_elementBadgeImage != null) _elementBadgeImage.gameObject.SetActive(false);
            if (_starText != null) _starText.gameObject.SetActive(false);
            if (_shardProgressText != null) _shardProgressText.text = "";

            // 4. Label Name
            if (_nameText != null)
            {
                _nameText.text = vm.IsSelected ? $"<color=#FFFFFF>{vm.UpgradeName}</color>" : $"<color=#F1E6C8>{vm.UpgradeName}</color>";
            }
        }

        public void SetSelected(bool isSelected, bool isUnlocked = true, Color? customNameColor = null, string baseName = null)
        {
            UpdateBoxVisual(isSelected, isUnlocked);

            if (_nameText != null)
            {
                if (!isUnlocked)
                {
                    _nameText.text = isSelected ? "<color=#FFCC88>Chưa Mở Khóa</color>" : "<color=#665544>Chưa Mở Khóa</color>";
                }
                else if (!string.IsNullOrEmpty(baseName))
                {
                    string nameColorHex = isSelected ? "FFFFFF" : (customNameColor.HasValue ? ColorUtility.ToHtmlStringRGB(customNameColor.Value) : "F1E6C8");
                    _nameText.text = $"<color=#{nameColorHex}>{baseName}</color>";
                }
            }
        }

        private void UpdateBoxVisual(bool isSelected, bool isUnlocked)
        {
            if (_boxImage == null) return;

            if (isSelected)
            {
                if (_selectedSlotSprite != null) _boxImage.sprite = _selectedSlotSprite;
                _boxImage.color = Color.white;
            }
            else
            {
                if (_normalSlotSprite != null) _boxImage.sprite = _normalSlotSprite;
                _boxImage.color = isUnlocked ? Color.white : new Color(0.40f, 0.35f, 0.30f, 0.6f);
            }
        }

        /// <summary>
        /// Tạo cấu trúc GameObject đầy đủ chuẩn Cổ Phong runtime nếu prefab chưa được build trong Asset.
        /// </summary>
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

            // Attach & Bind Fields
            var slotView = slotObj.AddComponent<CodexSlotItemView>();
            slotView._boxImage = boxImg;
            slotView._iconImage = iconImg;
            slotView._elementBadgeImage = ebImg;
            slotView._starText = starTMP;
            slotView._shardProgressText = shardTMP;
            slotView._nameText = lblTMP;
            slotView._clickButton = boxBtn;

            return slotView;
        }
    }
}

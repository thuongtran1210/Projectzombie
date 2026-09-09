using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.Common
{
    /// <summary>
    /// Passive View chuẩn mực duy nhất (Single Source of Truth) quản lý hiển thị một ô Item / Pháp Bảo / Thần Binh.
    /// Tuân thủ nghiêm ngặt mô hình MVP: Không chứa logic nghiệp vụ, chỉ nhận ViewModel và render thị giác.
    /// </summary>
    public class UniversalItemSlotView : MonoBehaviour
    {
        [Header("Khung & Nền")]
        [SerializeField] private Image _boxBorderImage;
        [SerializeField] private Image _innerBgImage;
        [SerializeField] private Image _rarityGlowImage;
        [SerializeField] private Image _iconImage;

        [Header("Huy Hiệu Góc")]
        [SerializeField] private Image _elementBadgeImage;      // Góc trái trên (Ngũ Hành)
        [SerializeField] private TextMeshProUGUI _starBadgeText;  // Góc phải trên (1★ - 5★)
        [SerializeField] private GameObject _equippedBadge;      // Góc phải dưới (Đã trang bị)

        [Header("Nhãn & Tiến Trình")]
        [SerializeField] private TextMeshProUGUI _bottomProgressText; // Dưới icon (Tiến trình mảnh X/5)
        [SerializeField] private TextMeshProUGUI _nameText;           // Tên vật phẩm

        [Header("Tương Tác")]
        [SerializeField] private Button _clickButton;

        [Header("Sprite Resources Cổ Phong")]
        [SerializeField] private Sprite _normalSlotWoodSprite;
        [SerializeField] private Sprite _selectedSlotGlowSprite;

        private Action _onClickedCallback;
        private UniversalItemSlotViewModel _currentVM;

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

        /// <summary>
        /// Nạp dữ liệu và render toàn bộ trạng thái của Item Slot.
        /// </summary>
        public void BindData(UniversalItemSlotViewModel vm, Sprite normalSlot, Sprite selectedSlot, Action onClicked)
        {
            _currentVM = vm;
            _onClickedCallback = onClicked;
            if (normalSlot != null) _normalSlotWoodSprite = normalSlot;
            if (selectedSlot != null) _selectedSlotGlowSprite = selectedSlot;

            // 1. Icon Pháp Bảo
            if (_iconImage != null)
            {
                _iconImage.sprite = vm.Icon;
                _iconImage.enabled = vm.Icon != null;
                _iconImage.color = vm.IsLocked ? new Color(0.35f, 0.30f, 0.35f, 0.45f) : Color.white;
            }

            // 2. Viền Độ Hiếm & Hào Quang (Rarity Frame)
            UpdateRarityAndSelectionVisuals(vm);

            // 3. Huy hiệu Ngũ Hành (Top-Left)
            if (_elementBadgeImage != null)
            {
                if (!vm.IsLocked && vm.ElementBadge != null)
                {
                    _elementBadgeImage.sprite = vm.ElementBadge;
                    _elementBadgeImage.gameObject.SetActive(true);
                }
                else
                {
                    _elementBadgeImage.gameObject.SetActive(false);
                }
            }

            // 4. Cấp Sao (Top-Right)
            if (_starBadgeText != null)
            {
                if (vm.StarLevel > 0)
                {
                    _starBadgeText.text = $"<color=#FFD700><b>{vm.StarLevel}★</b></color>";
                    _starBadgeText.gameObject.SetActive(true);
                }
                else
                {
                    _starBadgeText.gameObject.SetActive(false);
                }
            }

            // 5. Huy hiệu Đã Trang Bị (Equipped Badge)
            if (_equippedBadge != null)
            {
                _equippedBadge.SetActive(vm.IsEquipped);
            }

            // 6. Thanh Tiến Trình / Nhãn Dưới (Bottom Progress Text)
            if (_bottomProgressText != null)
            {
                if (!string.IsNullOrEmpty(vm.CustomBottomText))
                {
                    _bottomProgressText.text = vm.CustomBottomText;
                }
                else if (vm.StarLevel >= 5)
                {
                    _bottomProgressText.text = "<color=#00FF88><b>TỐI ĐA</b></color>";
                }
                else if (vm.ReqShards > 0)
                {
                    string colorHex = vm.ShardCount >= vm.ReqShards ? "00FF88" : "FFAA00";
                    _bottomProgressText.text = $"<color=#{colorHex}><b>{vm.ShardCount}/{vm.ReqShards}</b></color>";
                }
                else
                {
                    _bottomProgressText.text = "";
                }
            }

            // 7. Nhãn Tên Vật Phẩm (Name Label)
            if (_nameText != null)
            {
                if (vm.IsLocked)
                {
                    _nameText.text = vm.IsSelected ? "<color=#FFCC88>Chưa Mở Khóa</color>" : "<color=#665544>Chưa Mở Khóa</color>";
                }
                else
                {
                    Color rarityColor = vm.Rarity.GetColor();
                    string colorHex = vm.IsSelected ? "FFFFFF" : (vm.CustomNameColor.HasValue ? ColorUtility.ToHtmlStringRGB(vm.CustomNameColor.Value) : ColorUtility.ToHtmlStringRGB(rarityColor));
                    _nameText.text = $"<color=#{colorHex}>{vm.ItemName}</color>";
                }
            }
        }

        private void UpdateRarityAndSelectionVisuals(UniversalItemSlotViewModel vm)
        {
            if (_boxBorderImage == null) return;

            Color rarityColor = vm.Rarity.GetColor();

            if (vm.IsSelected || vm.IsEquipped)
            {
                if (_selectedSlotGlowSprite != null) _boxBorderImage.sprite = _selectedSlotGlowSprite;
                _boxBorderImage.color = vm.IsEquipped ? new Color(1f, 0.85f, 0.2f, 1f) : Color.white;
            }
            else
            {
                if (_normalSlotWoodSprite != null) _boxBorderImage.sprite = _normalSlotWoodSprite;
                _boxBorderImage.color = vm.IsLocked ? new Color(0.40f, 0.35f, 0.30f, 0.6f) : Color.white;
            }

            if (_rarityGlowImage != null)
            {
                if (!vm.IsLocked)
                {
                    _rarityGlowImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.25f);
                    _rarityGlowImage.gameObject.SetActive(true);
                }
                else
                {
                    _rarityGlowImage.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Tạo cấu trúc GameObject đầy đủ chuẩn Cổ Phong runtime nếu prefab chưa được build trong Asset.
        /// </summary>
        public static UniversalItemSlotView CreateDynamicSlot(Transform parent)
        {
            GameObject slotObj = new GameObject("UniversalItemSlot", typeof(RectTransform));
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

            // Rarity Glow (Nền ánh quang phẩm cấp)
            GameObject glowObj = new GameObject("RarityGlow", typeof(RectTransform), typeof(Image));
            glowObj.transform.SetParent(innerObj.transform, false);
            var glowRT = glowObj.GetComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.offsetMin = Vector2.zero;
            glowRT.offsetMax = Vector2.zero;
            var glowImg = glowObj.GetComponent<Image>();
            glowImg.raycastTarget = false;
            glowObj.SetActive(false);

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

            // Element Badge (Top-Left)
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

            // Star Badge (Top-Right)
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

            // Equipped Badge (Bottom-Right / Top Center)
            GameObject eqObj = new GameObject("Badge_Equipped", typeof(RectTransform), typeof(Image));
            eqObj.transform.SetParent(boxObj.transform, false);
            var eqRT = eqObj.GetComponent<RectTransform>();
            eqRT.anchorMin = new Vector2(1, 0);
            eqRT.anchorMax = new Vector2(1, 0);
            eqRT.pivot = new Vector2(1, 0);
            eqRT.anchoredPosition = new Vector2(-2, 2);
            eqRT.sizeDelta = new Vector2(22, 22);
            var eqImg = eqObj.GetComponent<Image>();
            eqImg.color = new Color(1f, 0.85f, 0.2f, 1f);
            eqImg.raycastTarget = false;
            eqObj.SetActive(false);

            // Shard Progress / Bottom text
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

            // Attach Component
            var slotView = slotObj.AddComponent<UniversalItemSlotView>();
            slotView._boxBorderImage = boxImg;
            slotView._innerBgImage = inImg;
            slotView._rarityGlowImage = glowImg;
            slotView._iconImage = iconImg;
            slotView._elementBadgeImage = ebImg;
            slotView._starBadgeText = starTMP;
            slotView._equippedBadge = eqObj;
            slotView._bottomProgressText = shardTMP;
            slotView._nameText = lblTMP;
            slotView._clickButton = boxBtn;

            return slotView;
        }
    }
}

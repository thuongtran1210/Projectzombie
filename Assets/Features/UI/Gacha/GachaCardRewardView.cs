using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.MetaProgression.Gacha.Data;

namespace ProjectZombie.Features.UI.Gacha
{
    /// <summary>
    /// UI Card hiển thị 1 vật phẩm/thẻ pháp bảo nhận được từ Gacha.
    /// Tuân thủ quy tắc MVP & TextMeshPro Rich Text.
    /// </summary>
    public class GachaCardRewardView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityBorderImage;
        [SerializeField] private Image _elementBadgeImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _shardCountText;
        [SerializeField] private GameObject _newBadgeObject;
        [SerializeField] private TextMeshProUGUI _starLevelText;

        public void BindData(GachaDropResult data)
        {
            if (_nameText != null)
            {
                _nameText.text = $"<b>{data.relicName}</b>";
            }

            if (_rarityText != null)
            {
                _rarityText.text = $"<color={data.rarity.GetHexColor()}>[{data.rarity.GetDisplayName()}]</color>";
            }

            if (_shardCountText != null)
            {
                if (data.isConvertedToCurrency)
                {
                    _shardCountText.text = $"<color=#FFD700>+{data.convertedCurrencyAmount:N0} Cổ Tiền</color> (Đã đạt 5 Sao)";
                }
                else
                {
                    _shardCountText.text = $"<color=#00FF88>+{data.shardCount}</color> Mảnh";
                }
            }

            if (_starLevelText != null)
            {
                if (data.isConvertedToCurrency || data.currentStarLevel >= 5)
                {
                    _starLevelText.text = "<color=#FFD700><b>5 Sao (Tối Đa)</b></color>";
                }
                else
                {
                    _starLevelText.text = data.currentStarLevel > 0 ? $"<color=#FFD700>{data.currentStarLevel} Sao</color>" : "<color=#888888>Chưa mở khóa</color>";
                }
            }

            if (_iconImage != null)
            {
                _iconImage.sprite = data.icon;
                _iconImage.gameObject.SetActive(data.icon != null);
            }

            if (_rarityBorderImage != null)
            {
                _rarityBorderImage.color = data.rarity.GetColor();
            }

            if (_newBadgeObject != null)
            {
                _newBadgeObject.SetActive(data.isNewUnlock);
            }
        }
    }
}

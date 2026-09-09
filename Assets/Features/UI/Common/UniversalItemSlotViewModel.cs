using System;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.Common
{
    /// <summary>
    /// Pure ViewModel đại diện cho dữ liệu của một ô Item / Vũ Khí / Pháp Bảo (MVP Pattern).
    /// Chứa toàn bộ thông tin đã format sẵn, không phụ thuộc vào Model hay MonoBehaviour.
    /// </summary>
    [Serializable]
    public struct UniversalItemSlotViewModel
    {
        public string ItemId;
        public string ItemName;
        public Sprite Icon;
        
        // Độ Hiếm & Ngũ Hành (Visual Contract)
        public ItemRarity Rarity;
        public ElementType Element;
        public Sprite ElementBadge;
        
        // Tiến trình & Cấp Sao
        public int StarLevel;       // 0: Chưa sở hữu, 1-5: Cấp sao
        public int ShardCount;     // Số mảnh hiện có
        public int ReqShards;       // Số mảnh yêu cầu
        
        // Trạng thái tương tác
        public bool IsLocked;       // true nếu chưa mở khóa / chưa sở hữu
        public bool IsEquipped;     // true nếu đang trang bị vào loadout
        public bool IsSelected;     // true nếu đang được focus/chọn xem chi tiết
        
        // Nhãn tùy biến dưới chân ô
        public string CustomBottomText; // Nếu có giá trị, sẽ ưu tiên hiển thị thay vì ShardCount
        public Color? CustomNameColor;
    }
}

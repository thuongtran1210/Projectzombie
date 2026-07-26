using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Base class trừu tượng cho tất cả các Kỹ năng Nội tại riêng của từng Nhân vật (Character Passive Traits).
    /// Tuân thủ mô hình Đa Hình (Polymorphism) và Strategy Pattern.
    /// </summary>
    public abstract class CharacterPassiveData : ScriptableObject
    {
        [Header("Display Info")]
        [Tooltip("Tên kỹ năng nội tại")]
        public string traitName;

        [TextArea]
        [Tooltip("Mô tả hiệu ứng kỹ năng nội tại")]
        public string description;

        [Tooltip("Icon hiển thị trên UI")]
        public Sprite icon;

        /// <summary>
        /// Kích hoạt hiệu ứng nội tại lên nhân vật khi bắt đầu trận đấu.
        /// </summary>
        public abstract void ApplyPassive(GameObject player);
    }
}

using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// ScriptableObject lưu trữ các chỉ số gốc của nhân vật (Base Stats).
    /// Tránh hard-code chỉ số vào script, Game Designer có thể tạo/chỉnh sửa dễ dàng.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHeroStats", menuName = "ProjectZombie/Hero Stats")]
    public class HeroStatsConfig : ScriptableObject
    {
        [Header("Survival Stats")]
        [Tooltip("Máu tối đa")]
        public float maxHealth = 100f;
        
        [Tooltip("Tốc độ di chuyển")]
        public float moveSpeed = 5f;

        [Tooltip("Thời gian hồi chiêu lướt (giây)")]
        public float dashCooldown = 8f;

        [Header("Combat Stats")]
        [Tooltip("Sát thương cơ bản")]
        public float baseDamage = 10f;

        [Tooltip("Tốc độ đánh (số lần tấn công mỗi giây)")]
        public float attackSpeed = 1f;

        [Tooltip("Tỉ lệ chí mạng (0.05 = 5%)")]
        [Range(0f, 1f)]
        public float critChance = 0.05f;

        [Header("Utility Stats")]
        [Tooltip("Tầm nhặt vật phẩm / EXP")]
        public float pickupRange = 100f;
    }
}

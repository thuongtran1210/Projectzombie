using UnityEngine;

namespace ProjectZombie.Features.Weapons
{
    public enum WeaponRole
    {
        PrimaryWeapon,      // Vũ Khí Chính (Gắn nút Tấn Công Chủ Động, Combo 3 đòn)
        RelicOrbitalShield, // Pháp Bảo Quỹ Đạo Hộ Thân (Xoay quanh người cản đạn/quái)
        RelicOnHitTrigger,  // Pháp Bảo Kích Ứng Bồi Đòn (Tự động ra đòn khi chém trúng quái)
        RelicSupportAura    // Pháp Bảo Hỗ Trợ & Trị Liệu (Hồi máu, làm chậm, tạo vùng an toàn)
    }

    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ProjectZombie/Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity & Role (GDD v5.0)")]
        [Tooltip("Vai trò chiến đấu của vũ khí/pháp bảo")]
        public WeaponRole weaponRole = WeaponRole.PrimaryWeapon;

        [Tooltip("ID duy nhất của vũ khí (VD: W001)")]
        public string weaponId;
        
        [Tooltip("Tên hiển thị trong game")]
        public string weaponName;

        [Tooltip("Icon đại diện hiển thị trên UI HUD")]
        public Sprite icon;

        [TextArea(2, 4)]
        [Tooltip("Mô tả chi tiết & hiệu ứng đặc trưng của Pháp Bảo")]
        public string description;

        [Tooltip("Độ hiếm của Pháp Bảo")]
        public string rarity = "Common";

        [Tooltip("ID Evolution / Vũ khí tối thượng liên kết")]
        public string evolutionWeaponId;

        [Header("Vong Xuyen Attributes (v4.0)")]
        [Tooltip("Thuộc tính Ngũ Hành của Pháp Bảo")]
        public ProjectZombie.Features.Shared.ElementType elementType = ProjectZombie.Features.Shared.ElementType.None;

        [Header("Prefabs")]
        [Tooltip("Prefab của vũ khí sẽ được gắn vào Player")]
        public WeaponBase weaponPrefab;
        
        [Header("Starting Stats (Optional)")]
        [Tooltip("Sát thương cơ bản khi mới nhận được")]
        public float baseDamage;
        
        [Tooltip("Tốc độ đánh cơ bản / Cooldown (nếu muốn override)")]
        public float baseAttackSpeed;
    }
}

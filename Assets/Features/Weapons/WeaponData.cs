using UnityEngine;

namespace ProjectZombie.Features.Weapons
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ProjectZombie/Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("ID duy nhất của vũ khí (VD: weapon_sword_01)")]
        public string weaponId;
        
        [Tooltip("Tên hiển thị trong game")]
        public string weaponName;

        [Header("Prefabs")]
        [Tooltip("Prefab của vũ khí sẽ được gắn vào Player")]
        public WeaponBase weaponPrefab;
        
        [Header("Starting Stats (Optional)")]
        [Tooltip("Sát thương cơ bản khi mới nhận được")]
        public float baseDamage;
        
        [Tooltip("Tốc độ đánh cơ bản (nếu muốn override)")]
        public float baseAttackSpeed;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// ScriptableObject định nghĩa dữ liệu toàn diện cho một Anh Hùng / Tướng (Single Source of Truth).
    /// Chuẩn hóa theo Architecture v5.0 (Cổ Phong Đông Sơn - Action Roguelite).
    /// </summary>
    [CreateAssetMenu(fileName = "Hero_NewCharacter", menuName = "ProjectZombie/Characters/Character Data")]
    public class CharacterDataSO : ScriptableObject
    {
        [Header("1. Định Danh & Trình Diễn (Identity & Visuals)")]
        [Tooltip("Mã định danh duy nhất (VD: C001_ThuSinh)")]
        public string characterId = "C001_Hero";

        [Tooltip("Tên hiển thị của Anh Hùng")]
        public string characterName = "Tên Anh Hùng";

        [Tooltip("Hệ Ngũ Hành (Kim / Mộc / Thủy / Hỏa / Thổ)")]
        public ElementType element = ElementType.Kim;

        [Tooltip("Màu Hex hiển thị của Hệ (VD: #FFD700)")]
        public string elementHexColor = "#FFD700";

        [Tooltip("Ảnh đại diện (Avatar 2D)")]
        public Sprite avatar;

        [Tooltip("Prefab nhân vật trong trận đấu")]
        public GameObject playerPrefab;

        [Tooltip("Mô tả truyền thuyết nhân vật")]
        [TextArea(2, 4)]
        public string description;

        [Header("2. Chỉ Số Chiến Đấu Cơ Bản (Base Combat Stats)")]
        [Tooltip("Máu tối đa khởi điểm")]
        public float baseMaxHealth = 100f;

        [Tooltip("Tốc độ di chuyển cơ bản (2.0 - 9.0)")]
        public float baseMoveSpeed = 5.0f;

        [Tooltip("Sát thương cơ bản")]
        public float baseDamage = 10f;

        [Tooltip("Tỉ lệ bạo kích khởi điểm (0.05 = 5%)")]
        public float baseCritChance = 0.05f;

        [Tooltip("Thời gian hồi chiêu Lướt (Dash Cooldown)")]
        public float baseDashCooldown = 2.0f;

        [Header("3. Tỉ Lệ Hiển Thị Biểu Đồ UI (0.0 -> 1.0)")]
        [Range(0f, 1f)] public float uiAtkRatio = 0.8f;
        [Range(0f, 1f)] public float uiSpdRatio = 0.7f;
        [Range(0f, 1f)] public float uiDefRatio = 0.6f;

        [Header("4. Đòn Đánh Thường Bản Mệnh (Basic Attack Config)")]
        public CharacterAttackConfig basicAttackConfig = new CharacterAttackConfig();

        [Header("5. Kỹ Năng Chủ Động (Active Signature Skill)")]
        public string signatureSkillName;
        [TextArea(2, 3)] public string signatureSkillDesc;

        [Header("6. Nội Tại Độc Quyền (Passive Trait)")]
        public string passiveTraitName;
        [TextArea(2, 3)] public string passiveTraitDesc;

        [Header("7. Trang Bị Khởi Điểm")]
        [Tooltip("1 Pháp Bảo Hộ Thân duy nhất mang theo")]
        public WeaponData defaultRelic;

        [Tooltip("Tương thích ngược: Vũ khí chính")]
        public WeaponData defaultPrimaryWeapon;

        [Tooltip("Tương thích ngược: Danh sách pháp bảo cũ")]
        public List<WeaponData> defaultRelics = new List<WeaponData>();

        [Header("8. Trạng Thái Mở Khóa")]
        public bool isUnlockedByDefault = true;
    }
}

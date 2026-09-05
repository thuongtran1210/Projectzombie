using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player
{
    public enum CharacterAttackType
    {
        MeleeSlash,
        RangedProjectile
    }

    [Serializable]
    public class CharacterAttackConfig
    {
        [Tooltip("Loại đòn đánh: Cận chiến chém vệt (MeleeSlash) hoặc Tầm xa bắn đạn (RangedProjectile)")]
        public CharacterAttackType attackType = CharacterAttackType.MeleeSlash;

        [Tooltip("Tên đòn đánh cơ bản")]
        public string attackName = "Đòn Đánh Thường";

        [Tooltip("Icon đại diện đòn đánh hiển thị trên Attack Button")]
        public Sprite attackIcon;

        [Tooltip("Hệ nguyên tố của đòn đánh")]
        public ElementType element = ElementType.None;

        [Tooltip("Hệ số sát thương cơ bản (Ví dụ 1.0 = 100% Base Attack)")]
        public float baseDamageMultiplier = 1.0f;

        [Tooltip("Tốc độ ra đòn cơ bản (Số đòn / giây)")]
        public float baseAttackSpeed = 1.8f;

        [Header("Melee Slash Settings")]
        [Tooltip("Kích thước vùng chém (Chiều rộng x Chiều dài)")]
        public Vector2 meleeAreaSize = new Vector2(2.5f, 2.0f);
        [Tooltip("Khoảng cách tâm vùng chém so với nhân vật")]
        public float meleeOffset = 1.2f;
        [Tooltip("VFX Vệt Chém (Slash Particle System Prefab)")]
        public GameObject slashVfxPrefab;

        [Header("Ranged Projectile Settings")]
        [Tooltip("Prefab đạn tầm xa")]
        public GameObject projectilePrefab;
        [Tooltip("Tốc độ bay của đạn")]
        public float projectileSpeed = 12f;
        [Tooltip("Tầm bắn tối đa / Thời gian tồn tại")]
        public float projectileLifetime = 1.5f;
        [Tooltip("Số lượng đạn bắn ra mỗi lần")]
        public int projectileCount = 1;
        [Tooltip("Góc tỏa đạn (độ)")]
        public float spreadAngle = 0f;

        [Header("Timing & Action Window (Đồng Bộ Nhịp Đánh)")]
        [Tooltip("Tỉ lệ thời gian vung tay chuẩn bị trước khi bung VFX chém (0.15 = 15% chu kỳ đòn đánh)")]
        [Range(0.05f, 0.4f)] public float windupRatio = 0.15f;
        [Tooltip("Thời gian tồn tại của VFX đòn đánh (giây)")]
        public float vfxDuration = 0.3f;

        [Header("Combo & Multipliers")]
        public int maxComboSteps = 3;
        public float comboResetWindow = 1.0f;
        public float comboStep2Multiplier = 1.2f;
        public float comboStep3Multiplier = 1.8f;
        public float knockbackForce = 4.0f;
    }

    [Serializable]
    public class CharacterEntry
    {
        public string characterId;
        public string characterName;
        public ElementType element;
        public string elementHexColor = "#FFD700";
        [TextArea(2, 4)] public string description;

        [Header("Đòn Đánh Cơ Bản (Character Signature Basic Attack)")]
        public CharacterAttackConfig basicAttackConfig = new CharacterAttackConfig();
        
        [Header("Kỹ Năng Chủ Động (Active Signature Skill)")]
        public string signatureSkillName;
        [TextArea(2, 4)] public string signatureSkillDesc;

        [Header("Nội Tại Độc Quyền (Passive Trait)")]
        public string passiveTraitName;
        [TextArea(2, 4)] public string passiveTraitDesc;

        [Header("Chỉ Số Chiến Đấu Cơ Bản (Base Stats)")]
        public float baseMaxHealth = 100f;
        public float baseMoveSpeed = 5.0f;
        public float baseDamage = 10f;
        public float baseCritChance = 0.05f;
        public float baseDashCooldown = 2.0f;

        [Header("Tỉ Lệ Hiển Thị Biểu Đồ UI (0.0 -> 1.0)")]
        [Range(0f, 1f)] public float uiAtkRatio = 0.8f;
        [Range(0f, 1f)] public float uiSpdRatio = 0.7f;
        [Range(0f, 1f)] public float uiDefRatio = 0.6f;

        public Sprite avatar;
        public GameObject playerPrefab;
        public bool isUnlocked = true;

        [Header("Trang Bị Khởi Điểm (1 Pháp Bảo Hộ Thân Duy Nhất)")]
        [Tooltip("1 Pháp bảo hộ thân duy nhất mang theo vào trận")]
        public Weapons.WeaponData defaultRelic;

        [Tooltip("Tương thích ngược: Vũ khí chính cũ")]
        public Weapons.WeaponData defaultPrimaryWeapon;

        [Tooltip("Tương thích ngược: danh sách relics cũ")]
        public List<Weapons.WeaponData> defaultRelics = new List<Weapons.WeaponData>();
    }

    /// <summary>
    /// ScriptableObject lưu trữ danh sách toàn bộ nhân vật có thể chơi trong game (Database Nhân Vật)
    /// và ghi nhớ nhân vật hiện đang được người chơi lựa chọn.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSelectionData", menuName = "ProjectZombie/Character Selection Data")]
    public class CharacterSelectionData : ScriptableObject
    {
        [Header("Character Database")]
        [Tooltip("Danh sách toàn bộ nhân vật có thể chọn trong game")]
        [SerializeField] private List<CharacterEntry> characters = new List<CharacterEntry>();

        [Header("Runtime Selection")]
        [Tooltip("Prefab của nhân vật đã chọn để spawn vào game")]
        [SerializeField] private GameObject selectedPlayerPrefab;

        [Tooltip("Index nhân vật được chọn gần nhất")]
        [SerializeField] private int selectedCharacterIndex = 0;

        public IReadOnlyList<CharacterEntry> Characters => characters;

        public GameObject SelectedPlayerPrefab
        {
            get
            {
                if (selectedPlayerPrefab != null) return selectedPlayerPrefab;
                if (characters != null && characters.Count > 0 && selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count)
                {
                    return characters[selectedCharacterIndex].playerPrefab;
                }
                return null;
            }
            set => selectedPlayerPrefab = value;
        }

        public int SelectedCharacterIndex
        {
            get => selectedCharacterIndex;
            set
            {
                selectedCharacterIndex = value;
                if (characters != null && selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count)
                {
                    selectedPlayerPrefab = characters[selectedCharacterIndex].playerPrefab;
                }
            }
        }

        public void SelectCharacter(int index)
        {
            SelectedCharacterIndex = index;
        }

        public void SetCharacters(List<CharacterEntry> list)
        {
            characters = list;
        }
    }
}

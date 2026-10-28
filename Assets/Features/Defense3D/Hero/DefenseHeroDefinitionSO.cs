using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Định nghĩa hồ sơ và cấu hình dữ liệu của một Anh Hùng / Tướng trong chế độ Defense 3D.
    /// Tuân thủ quy chuẩn Single Source of Truth (SSOT) và Data-Driven Architecture:
    /// - Quản lý định danh (HeroId, DisplayName, Avatar)
    /// - Quản lý Prefab xuất trận 3D
    /// - Quản lý Bộ Kỹ Năng (HeroKitSO)
    /// - Quản lý Chỉ số cơ bản (Máu, Tốc độ chạy, Sát thương cơ bản)
    /// </summary>
    [CreateAssetMenu(fileName = "Hero3D_NewDefinition", menuName = "Defense3D/Hero Definition")]
    public sealed class DefenseHeroDefinitionSO : ScriptableObject
    {
        [Header("1. Định Danh & Trình Diễn (Identity & Visuals)")]
        [Tooltip("Mã định danh khớp với Catalog Loadout (VD: HERO_THUSHINH, HERO_CUOI, HERO_CHIENTHAN, HERO_COTAM)")]
        [SerializeField] private string _heroId = "HERO_THUSHINH";

        [Tooltip("Tên hiển thị thuần Việt Cổ Phong")]
        [SerializeField] private string _displayName = "Thư Sinh";

        [Tooltip("Ảnh chân dung Avatar hiển thị trên HUD / Danh sách chọn tướng")]
        [SerializeField] private Sprite _avatar;

        [Tooltip("Prefab nhân vật 3D hoàn chỉnh dùng để xuất trận trong Map Sông Quê 3D")]
        [SerializeField] private GameObject _heroPrefab;

        [Header("2. Bộ Kỹ Năng Xuất Trận (Hero Kit)")]
        [Tooltip("Cấu hình Skill đánh thường, lướt và tuyệt kỹ đặc trưng")]
        [SerializeField] private DefenseHeroKitSO _heroKit;

        [Header("3. Chỉ Số Chiến Đấu Cơ Bản (Base Combat Stats)")]
        [Tooltip("Lượng máu cơ bản ban đầu")]
        [SerializeField, Min(1f)] private float _baseMaxHp = 100f;

        [Tooltip("Tốc độ di chuyển cơ bản (m/s)")]
        [SerializeField, Min(0.1f)] private float _baseMoveSpeed = 5.0f;

        [Tooltip("Tốc độ xoay hướng (rad/s)")]
        [SerializeField, Min(1f)] private float _turnRate = 14f;

        [Tooltip("Sát thương đánh thường cơ bản")]
        [SerializeField, Min(1f)] private float _baseDamage = 10f;

        [Tooltip("Tầm đánh thường cơ bản (m)")]
        [SerializeField, Min(0.5f)] private float _baseAttackRange = 6.0f;

        // Public Accessors
        public string HeroId => _heroId;
        public string DisplayName => _displayName;
        public Sprite Avatar => _avatar;
        public GameObject HeroPrefab => _heroPrefab;
        public DefenseHeroKitSO HeroKit => _heroKit;
        public float BaseMaxHp => _baseMaxHp;
        public float BaseMoveSpeed => _baseMoveSpeed;
        public float TurnRate => _turnRate;
        public float BaseDamage => _baseDamage;
        public float BaseAttackRange => _baseAttackRange;

        /// <summary>
        /// Áp dụng các chỉ số cơ bản của Tướng vào các component runtime trong trận.
        /// </summary>
        public void ApplyStatsToHero(DefenseHeroHealth3D health, DefenseHeroMotor3D motor, DefenseHeroAutoAttack3D attack)
        {
            if (health != null)
            {
                // Đồng bộ chỉ số máu cơ bản
                health.SetMaxHealth(_baseMaxHp);
            }

            if (motor != null)
            {
                // Đồng bộ tốc độ di chuyển
                motor.SetMovementParameters(_baseMoveSpeed, _turnRate);
            }
        }
    }
}

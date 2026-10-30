using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Cấu hình dữ liệu Tháp dạng module hóa (Data-Driven Tower Data).
    /// Hỗ trợ đa tầng cấp độ nâng cấp (Progression Tiers) và danh sách hiệu ứng đòn đánh lắp ghép (Attack Effects).
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Tower_", menuName = "TD/Tower/Tower Data")]
    public class SO_TowerData : ScriptableObject
    {
        [Header("── 1. ĐỊNH DANH THÁP (Identity) ──")]
        [Tooltip("Mã định danh duy nhất (ví dụ: tower_archer, tower_ice, tower_cannon)")]
        [SerializeField] private string _id = "tower_archer";

        [Tooltip("Tên hiển thị trong game")]
        [SerializeField] private string _towerName = "Tháp Cung Thủ";
        [SerializeField, TextArea] private string _description;
        public string Description => _description;

        [Tooltip("Icon đại diện hiển thị trên UI Build / Shop")]
        [SerializeField] private Sprite _icon;

        [Header("── 2. CHIẾN LƯỢC MÔ-ĐUN (Modular Strategies) ──")]
        [Tooltip("Chiến lược chọn mục tiêu (Gần nhất, Máu thấp nhất, v.v.)")]
        [SerializeField] private DefenseAttackTargetingProfileSO _targetingStrategy;

        [Tooltip("Danh sách các hiệu ứng đòn đánh bổ trợ (Làm chậm, Choáng, Thiêu đốt, Đẩy lùi...)")]
        [SerializeField] private List<SO_AttackEffect> _attackEffects = new List<SO_AttackEffect>();

        [Header("── 3. CẤU HÌNH ĐẠN ĐẠO (Projectile Configuration) ──")]
        [Tooltip("Prefab đạn của tháp (P_Proj_Arrow_Archer, P_Proj_IceShard_BangHa...)")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private DefenseTowerAttackProfileSO _attack;
        [SerializeField] private DefenseTowerAttackProfileSO _specialAttack;
        public DefenseTowerAttackProfileSO Attack => _attack;
        public DefenseTowerAttackProfileSO SpecialAttack => _specialAttack;

        [Tooltip("Vận tốc bay của đạn (mét/giây)")]
        [SerializeField, Min(1f)] private float _projectileSpeed = 22f;

        [Tooltip("Độ võng/vồng của đường đạn parabol (0 = bắn thẳng, > 0 = cầu vồng)")]
        [SerializeField, Min(0f)] private float _projectileArcHeight = 0.6f;

        [Header("── 4. TĂNG TIẾN CẤP ĐỘ (Progression Tiers) ──")]
        [Tooltip("Danh sách thông số từng cấp độ (Cấp 1 -> Cấp N)")]
        [SerializeField] private TowerLevelStats[] _levels;

        public string Id => _id;
        public string TowerName => _towerName;
        public Sprite Icon => _icon;
        public DefenseAttackTargetingProfileSO TargetingStrategy => _targetingStrategy;
        public List<SO_AttackEffect> AttackEffects => _attackEffects;
        public GameObject ProjectilePrefab => _projectilePrefab;
        public float ProjectileSpeed => _projectileSpeed;
        public float ProjectileArcHeight => _projectileArcHeight;
        public TowerLevelStats[] Levels => _levels;

        public int MaxLevel => _levels != null ? _levels.Length : 0;

        public TowerLevelStats GetLevelStats(int level)
        {
            if (_levels == null || _levels.Length == 0) return default;
            int index = Mathf.Clamp(level - 1, 0, _levels.Length - 1);
            return _levels[index];
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                Debug.LogWarning($"[{name}] ID của Tháp không được để trống!", this);
            }

            if (_levels == null || _levels.Length == 0)
            {
                Debug.LogError($"[{name}] Tháp phải có ít nhất 1 cấp độ thiết lập!", this);
            }
        }
    }

    /// <summary>
    /// Thông số chi tiết của Tháp tại một cấp độ nhất định.
    /// </summary>
    [Serializable]
    public struct TowerLevelStats
    {
        [Min(1f)] public float maxHealth;
        [Tooltip("Chi phí Cổ Mộc (Gỗ) để xây dựng hoặc nâng cấp lên cấp độ này")]
        public int buildCost;

        [Tooltip("Sát thương mỗi phát bắn")]
        public float damage;

        [Tooltip("Tầm bắn hiệu dụng tối đa (mét)")]
        public float attackRange;

        [Tooltip("Tốc độ bắn (số phát bắn mỗi giây). Ví dụ: 1.5 = 1.5 phát/giây")]
        public float attackRate;

        [Tooltip("Prefab mô hình 3D hiển thị tương ứng với cấp độ này (tùy chọn hoán đổi visual khi nâng cấp)")]
        public GameObject visualModelPrefab;
    }
}

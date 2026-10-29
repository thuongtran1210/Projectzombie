using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseEnemyLocomotionType
    {
        GroundNavMesh,
        AmphibianWaterToShore,
        AirFlight,
        ParatrooperLanding
    }

    /// <summary>
    /// U3D-006: Data-driven definition for Enemies in TREBRAVE Defense 3D.
    /// Preserves exact source IDs and benchmark attributes from Stage01 & Vongxuyen contracts.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TREBRAVE/Defense3D/EnemyData")]
    public sealed class DefenseEnemyDataSO : ScriptableObject
    {
        [Header("── 1. ĐỊNH DANH YÊU MA (Identity) ──")]
        [Tooltip("Mã định danh duy nhất (khớp với hệ thống Vọng Xuyên: magiap, thuyquai, huyetdoi, quaidu, phicuong, magiap_boss, yaksha_boss)")]
        [SerializeField] private string enemyId = "magiap";

        [Tooltip("Tên hiển thị trong game (Ví dụ: Ma Giáp, Thủy Quái, Huyết Dơi, Quái Dù, Phi Cương, Dạ Xoa Vương)")]
        [SerializeField] private string displayName = "Ma Giáp";

        [Tooltip("Phương thức di chuyển / Địa hình hoạt động:\n• GroundNavMesh: Đi bộ theo NavMesh trên mặt đất\n• AmphibianWaterToShore: Thủy quái lội từ sông lên bờ\n• AirFlight: Phi hành bay trên không (bỏ qua địa hình sông)\n• ParatrooperLanding: Quái dù nhảy dù từ trên không xuống")]
        [SerializeField] private DefenseEnemyLocomotionType locomotionType = DefenseEnemyLocomotionType.GroundNavMesh;

        [Tooltip("Đánh dấu nếu đây là Trùm / Thủ Lĩnh (Boss hoặc Miniboss). Sẽ hiển thị thanh máu Boss đặc biệt và kích hoạt âm thanh cảnh báo.")]
        [SerializeField] private bool isBossOrMiniboss;

        [Tooltip("Prefab 3D của thực thể quái vật (phải gắn các component Animator, Collider, DefenseEnemy3D)")]
        [SerializeField] private GameObject prefab;

        [Header("── 2. CHỈ SỐ CHIẾN ĐẤU CƠ BẢN (Base Stats) ──")]
        [Tooltip("Lượng máu tối đa ban đầu khi chưa nhân hệ số đợt sóng (HP)")]
        [SerializeField, Min(1f)] private float maxHP = 130f;

        [Tooltip("Tốc độ di chuyển hành quân trên bản đồ (mét / giây)")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 1.45f;

        [Tooltip("Lượng sát thương gây ra cho Anh Hùng, Lính hoặc Thành lũy trên mỗi đòn đánh thường")]
        [SerializeField, Min(0f)] private float attackDamage = 16f;

        [Tooltip("Thời gian giãn cách giữa hai đòn đánh liên tiếp (giây). Chỉ số càng nhỏ đánh càng nhanh.")]
        [SerializeField, Min(0.1f)] private float attackInterval = 2f;

        [Tooltip("Khoảng cách tối đa để quái bắt đầu tung đòn đánh vào mục tiêu (mét)")]
        [SerializeField, Min(0.1f)] private float attackRange = 1.2f;

        [Header("── 3. PHẦN THƯỞNG KINH TẾ (Economy Rewards) ──")]
        [Tooltip("Số lượng Cổ Mộc (Gỗ) người chơi nhận được ngay khi hạ gục quái này")]
        [SerializeField, Min(0)] private int woodDrop = 10;

        [Tooltip("Lượng điểm kinh nghiệm (EXP) nhận được giúp Anh Hùng tăng cấp độ")]
        [SerializeField, Min(0)] private int expDrop = 5;

        public string EnemyId => enemyId;
        public string DisplayName => displayName;
        public DefenseEnemyLocomotionType LocomotionType => locomotionType;
        public bool IsBossOrMiniboss => isBossOrMiniboss;
        public GameObject Prefab => prefab;
        public float MaxHP => maxHP;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public float AttackRange => attackRange;
        public int WoodDrop => woodDrop;
        public int ExpDrop => expDrop;

        // Copy values per spawn; damage and scaling never mutate the shared asset.
        public DefenseEnemyStats CreateStats()
        {
            return new DefenseEnemyStats(enemyId, displayName, locomotionType, maxHP,
                moveSpeed, attackDamage, woodDrop, expDrop, isBossOrMiniboss)
            {
                AttackInterval = attackInterval,
                AttackRange = attackRange
            };
        }

        /// <summary>
        /// Calculate scaled stats according to wave/phase multiplier.
        /// Benchmark formula from Web runtime: 1 + (phase - 1) * 0.18f
        /// </summary>
        public float GetScaledHP(int phase)
        {
            float mult = 1f + Mathf.Max(0, phase - 1) * 0.18f;
            return maxHP * mult;
        }
    }
}

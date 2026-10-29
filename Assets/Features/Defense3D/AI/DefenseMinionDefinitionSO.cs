using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseMinionRole { Militia, Gatherer, Healer }
    public enum DefenseMinionOrder { Defend, Follow, Advance }

    /// <summary>Captured recruitment and role statistics from Vongxuyen's MinionFactory/MinionManager.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Minion Definition")]
    public sealed class DefenseMinionDefinitionSO : ScriptableObject
    {
        [Header("── 1. ĐỊNH DANH & CHI PHÍ TUYỂN MỘ (Recruitment) ──")]
        [Tooltip("Mã định danh duy nhất (Ví dụ: minion_danbinh, minion_phutapdich, minion_thaylang, minion_xathu)")]
        [SerializeField] private string _id;
        public string Id => _id;

        [Tooltip("Tên hiển thị thuần Việt Cổ Phong (Ví dụ: Dân Binh Giáp Tre, Phu Tạp Dịch, Thầy Lang Cứu Thương)")]
        [SerializeField] private string _displayName;
        public string DisplayName => _displayName;

        [Tooltip("Vai trò quân chủng:\n• Militia: Dân Binh cận chiến / phòng thủ\n• Gatherer: Phu Tạp Dịch khai thác gỗ & sửa chữa\n• Healer: Thầy Lang hồi phục sinh lực")]
        [SerializeField] private DefenseMinionRole _role;
        public DefenseMinionRole Role => _role;

        [Tooltip("Lượng Cổ Mộc (Gỗ) tiêu hao tại Trạm Tuyển Mộ để chiêu mộ 1 đơn vị lính")]
        [Min(1)] [SerializeField] private int _woodCost;
        public int WoodCost => _woodCost;

        [Header("── 2. SINH MỆNH & ĐIỀU HƯỚNG (Vitals & Navigation) ──")]
        [Tooltip("Lượng máu tối đa của đơn vị")]
        [Min(1)] [SerializeField] private float _maxHp;
        public float MaxHp => _maxHp;

        [Tooltip("Tốc độ di chuyển hành quân trên NavMesh (mét / giây)")]
        [Min(0.1f)] [SerializeField] private float _speed;
        public float Speed => _speed;

        [Tooltip("Thời gian giãn cách giữa các chu kỳ tư duy AI tìm mục tiêu (giây)")]
        [Min(0.1f)] [SerializeField] private float _thinkInterval = 0.2f;
        public float ThinkInterval => _thinkInterval;

        [Tooltip("Bán kính tối đa lính chủ động phát hiện kẻ địch hoặc công trình cần sửa (mét)")]
        [Min(1f)] [SerializeField] private float _searchRange = 8f;
        public float SearchRange => _searchRange;

        [Header("── 3. CHIẾN ĐẤU & KHỐNG CHẾ (Dân Binh - Combat & CC) ──")]
        [Tooltip("Sát thương mỗi đòn đánh cận chiến thông thường")]
        [Min(0)] [SerializeField] private float _damage;
        public float Damage => _damage;

        [Tooltip("Tầm đánh hiệu dụng tính từ tâm nhân vật đến mục tiêu (mét)")]
        [Min(0.1f)] [SerializeField] private float _attackRange;
        public float AttackRange => _attackRange;

        [Tooltip("Thời gian giãn cách giữa 2 đòn đánh liên tiếp (giây). Càng nhỏ đánh càng nhanh.")]
        [Min(0.1f)] [SerializeField] private float _attackInterval = 1f;
        public float AttackInterval => _attackInterval;

        [Tooltip("Sát thương đòn đánh nặng / kỹ năng giáng mạnh")]
        [SerializeField, Min(0)] private float _heavyDamage = 14f;
        public float HeavyDamage => _heavyDamage;

        [Tooltip("Thời gian làm choáng mục tiêu từ đòn đánh nặng (giây)")]
        [Min(0)] [SerializeField] private float _heavyStunDuration = 1.25f;
        public float HeavyStunDuration => _heavyStunDuration;

        [Tooltip("Lực xô ngã / đẩy lùi mục tiêu (mét)")]
        [SerializeField, Min(0)] private float _knockStrength = .325f;
        public float KnockStrength => _knockStrength;

        [Tooltip("Thời gian hiệu lực của đòn đẩy lùi (giây)")]
        [SerializeField, Min(.01f)] private float _knockDuration = .2f;
        public float KnockDuration => _knockDuration;

        [Tooltip("Tỷ lệ làm chậm tốc độ chạy của kẻ địch (0.0 = không làm chậm, 0.5 = giảm 50% tốc chạy)")]
        [Min(0)] [SerializeField] private float _slowStrength = 0.5f;
        public float SlowStrength => _slowStrength;

        [Tooltip("Thời gian hiệu lực làm chậm (giây)")]
        [Min(0)] [SerializeField] private float _slowDuration = 2.2f;
        public float SlowDuration => _slowDuration;

        [Header("── 4. CỨU THƯƠNG & HỒI PHỤC (Thầy Lang - Healing Support) ──")]
        [Tooltip("Lượng máu hồi phục cho quân lính đồng minh mỗi chu kỳ")]
        [Min(1f)] [SerializeField] private float _healAmount = 45f;
        public float HealAmount => _healAmount;

        [Tooltip("Lượng máu hồi phục cho Tướng Quân (Anh Hùng) mỗi chu kỳ")]
        [Min(1f)] [SerializeField] private float _heroHealAmount = 28f;
        public float HeroHealAmount => _heroHealAmount;

        [Tooltip("Thời gian giãn cách giữa 2 lần niệm chú cứu thương (giây)")]
        [Min(0.1f)] [SerializeField] private float _healInterval = 2.6f;
        public float HealInterval => _healInterval;

        [Header("── 5. KHAI THÁC & TU BỔ (Phu Tạp Dịch - Logistics & Repair) ──")]
        [Tooltip("Khối lượng Cổ Mộc (Gỗ) tối đa mà 1 Phu Tạp Dịch có thể gánh trên lưng mang về Trạm")]
        [Min(0)] [SerializeField] private int _carryCapacity = 60;
        public int CarryCapacity => _carryCapacity;

        [Tooltip("Lượng máu phục hồi cho Tháp Cung / Trụ phòng thủ mỗi chu kỳ sửa chữa")]
        [SerializeField, Min(1f)] private float _repairAmount = 25f;
        public float RepairAmount => _repairAmount;

        [Tooltip("Lượng máu phục hồi cho Cửa Thành / Nhà Dân mỗi chu kỳ sửa chữa")]
        [SerializeField, Min(1f)] private float _baseRepairAmount = 15f;
        public float BaseRepairAmount => _baseRepairAmount;

        [Tooltip("Thời gian giãn cách giữa 2 lần búa gõ sửa chữa công trình (giây)")]
        [SerializeField, Min(.1f)] private float _repairInterval = .95f;
        public float RepairInterval => _repairInterval;
    }
}

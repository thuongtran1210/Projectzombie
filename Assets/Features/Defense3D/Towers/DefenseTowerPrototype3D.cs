using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Bộ điều khiển Tháp tự động (Tower Controller) hướng dữ liệu (Data-Driven).
    /// Hỗ trợ cấu hình qua SO_TowerData hoặc fallback thông số SerializeField cục bộ để bảo toàn tương thích.
    /// Tích hợp modular SO_AttackEffect và tối ưu nhịp quét mục tiêu 60 FPS Mobile.
    /// </summary>
    public sealed class DefenseTowerPrototype3D : DefenseGameplayBehaviour
    {
        [Header("── 1. LIÊN KẾT DỮ LIỆU CHÒI (DATA-DRIVEN BINDING) ──")]
        [Tooltip("Asset dữ liệu định nghĩa tháp (Sổ tay Kiến trúc: SO_TowerData). Nếu gán, các chỉ số sẽ đọc từ đây.")]
        [SerializeField] private SO_TowerData towerData;

        [Tooltip("Điểm neo bắn đạn (Socket FirePoint). Nếu để trống sẽ mặc định dùng tâm tháp + 1.65m")]
        [SerializeField] private Transform firePoint;

        [Header("── 2. CHỈ SỐ DỰ PHÒNG (Fallback khi không gán SO_TowerData) ──")]
        [Tooltip("Tầm bắn hiệu dụng tối đa tính từ tâm tháp (mét)")]
        [SerializeField, Min(0.1f)] private float attackRange = 8f;

        [Tooltip("Sát thương mỗi mũi tên / viên đạn gây ra")]
        [SerializeField, Min(1f)] private float damage = 22f;

        [Tooltip("Thời gian giãn cách giữa 2 phát bắn (giây). Càng nhỏ bắn càng nhanh.")]
        [SerializeField, Min(0.1f)] private float fireInterval = 0.9f;

        [Tooltip("Cấu hình ưu tiên chọn mục tiêu (Quái gần nhất, Quái máu thấp nhất, v.v.)")]
        [SerializeField] private DefenseAttackTargetingProfileSO _targeting;

        [Tooltip("Hiệu ứng visual đường bay của đạn")]
        [SerializeField] private DefenseTowerProjectileVfx3D _projectileVfx;

        [Tooltip("Bộ phóng đạn tùy chỉnh (tùy chọn)")]
        [SerializeField] private MonoBehaviour _customLauncher;

        [Header("── 3. TĂNG TIẾN NÂNG CẤP DỰ PHÒNG (Fallback Scaling) ──")]
        [Tooltip("Lượng sát thương cộng thêm mỗi khi tháp lên cấp")]
        [SerializeField] private float _damagePerLevel = 10f;

        [Tooltip("Tầm bắn cộng thêm mỗi khi tháp lên cấp (mét)")]
        [SerializeField] private float _rangePerLevel = 1f;

        [Tooltip("Tốc độ bắn gia tăng mỗi khi tháp lên cấp (tỷ lệ phần trăm)")]
        [SerializeField] private float _fireRatePerLevel = .3f;

        [Header("── 4. HIỆU ỨNG ĐÒN ĐÁNH CỤC BỘ (Tùy chọn) ──")]
        [Tooltip("Danh sách hiệu ứng đòn đánh (nếu không dùng qua SO_TowerData)")]
        [SerializeField] private List<SO_AttackEffect> _attackEffects = new List<SO_AttackEffect>();

        private IDefenseProjectileLauncher _launcher;
        private DefenseAttackTargetSelector3D _targetSelector;
        private float nextShot;
        private float nextTargetScanTime;
        private IDefenseEnemyTarget3D _currentTarget;
        private DefenseSupportTarget3D _health;

        private const float SCAN_INTERVAL = 0.2f;

        public SO_TowerData TowerData => towerData;
        public int Level { get; private set; } = 1;

        public int MaxLevel => towerData != null ? towerData.MaxLevel : 3;

        public int BuildCost => towerData != null && towerData.Levels != null && towerData.Levels.Length > 0 ? towerData.Levels[0].buildCost : 25;

        public int UpgradeCost
        {
            get
            {
                if (Level >= MaxLevel) return -1;
                if (towerData != null)
                {
                    var nextTier = towerData.GetLevelStats(Level + 1);
                    return nextTier.buildCost > 0 ? nextTier.buildCost : (Level * 40);
                }
                return Level * 40;
            }
        }

        public float BaseDamage
        {
            get
            {
                if (towerData != null)
                {
                    var stats = towerData.GetLevelStats(Level);
                    return stats.damage > 0f ? stats.damage : damage;
                }
                return damage + (Level - 1) * _damagePerLevel;
            }
        }

        public float BaseRange
        {
            get
            {
                if (towerData != null)
                {
                    var stats = towerData.GetLevelStats(Level);
                    return stats.attackRange > 0f ? stats.attackRange : attackRange;
                }
                return attackRange + (Level - 1) * _rangePerLevel;
            }
        }

        public float BaseFireInterval
        {
            get
            {
                if (towerData != null)
                {
                    var stats = towerData.GetLevelStats(Level);
                    if (stats.attackRate > 0f) return 1f / stats.attackRate;
                }
                return fireInterval / (1f + (Level - 1) * _fireRatePerLevel);
            }
        }

        public float EffectiveDamage => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerDamage, BaseDamage) : BaseDamage;
        public float EffectiveRange => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerAttackRange, BaseRange) : BaseRange;

        public Vector3 FireOrigin => firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.65f;

        protected override void OnGameplayBound()
        {
            var profile = (towerData != null && towerData.TargetingStrategy != null) ? towerData.TargetingStrategy : _targeting;
            _targetSelector = new DefenseAttackTargetSelector3D(transform, profile);

            if (_customLauncher == null)
            {
                _customLauncher = FindObjectOfType<DefenseProjectilePool3D>();
            }

            _launcher = _customLauncher as IDefenseProjectileLauncher ?? _projectileVfx;
            if (_health != null) _health.Defeated -= OnDefeated;
            _health = GetComponent<DefenseSupportTarget3D>();
            if (_health != null) _health.Defeated += OnDefeated;
        }

        public void PrepareForBuild()
        {
            Level = 1;
            nextShot = 0f;
            nextTargetScanTime = 0f;
            _currentTarget = null;
            _health?.ResetForRecruit();
        }

        public bool TryUpgrade(DefenseWoodWallet3D wallet)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || (_health != null && _health.IsDead) || UpgradeCost < 0 || wallet == null)
                return false;

            if (!wallet.TrySpend(UpgradeCost))
                return false;

            Level++;
            _health?.IncreaseCapacity(100f);
            return true;
        }

        private void OnDefeated() => gameObject.SetActive(false);

        protected override void OnGameplayReset()
        {
            nextShot = 0f;
            nextTargetScanTime = 0f;
            Level = 1;
            _currentTarget = null;
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || (_health != null && _health.IsDead)) return;

            // Tối ưu hóa 60 FPS Mobile: Quét mục tiêu theo chu kỳ SCAN_INTERVAL (0.2s) thay vì quét mỗi frame
            if (GameplayTime >= nextTargetScanTime)
            {
                UpdateTarget();
                nextTargetScanTime = GameplayTime + SCAN_INTERVAL;
            }

            if (_currentTarget != null && GameplayTime >= nextShot)
            {
                // Kiểm tra mục tiêu còn hợp lệ và trong tầm bắn không
                if (!_currentTarget.IsTargetable || Vector3.Distance(transform.position, _currentTarget.TargetTransform.position) > EffectiveRange)
                {
                    _currentTarget = null;
                    return;
                }

                Attack(_currentTarget);
                nextShot = GameplayTime + BaseFireInterval;
            }
        }

        private void UpdateTarget()
        {
            _currentTarget = _targetSelector?.Find(Gameplay?.Targets, EffectiveRange);
        }

        private void Attack(IDefenseEnemyTarget3D target)
        {
            if (target == null) return;

            float speed = towerData != null ? towerData.ProjectileSpeed : 25f;
            float arcHeight = towerData != null ? towerData.ProjectileArcHeight : 0.5f;

            var request = new DefenseProjectileLaunchRequest(
                source: this,
                origin: FireOrigin,
                target: target,
                damage: EffectiveDamage,
                runGeneration: Gameplay?.RunGeneration ?? -1,
                sessionIdentity: Gameplay?.SessionIdentity,
                speed: speed,
                arcHeight: arcHeight);

            bool launched = false;
            if (_customLauncher is DefenseProjectilePool3D poolLauncher && towerData != null && towerData.ProjectilePrefab != null)
            {
                launched = poolLauncher.LaunchProjectile(towerData.ProjectilePrefab, request, towerData.AttackEffects);
            }
            else if (_launcher != null)
            {
                launched = _launcher.TryLaunch(request);
            }

            if (!launched)
            {
                target.TakeDamage(new DefenseDamageContext(EffectiveDamage, this, target.TargetTransform.position,
                    runGeneration: Gameplay?.RunGeneration ?? -1, sessionIdentity: Gameplay?.SessionIdentity));

                // Kích hoạt các hiệu ứng đòn đánh bổ trợ tức thì khi không dùng đạn bay
                ApplyEffects(target.TargetTransform);
            }

#if UNITY_EDITOR && DEFENSE_TOWER_DEBUG
            Debug.Log($"[TREBRAVE Defense3D] Tower shot fired at {target.TargetTransform.name} for {EffectiveDamage}");
#endif
        }

        private void ApplyEffects(Transform targetTransform)
        {
            if (targetTransform == null) return;

            // 1. Áp dụng hiệu ứng từ SO_TowerData
            if (towerData != null && towerData.AttackEffects != null)
            {
                for (int i = 0; i < towerData.AttackEffects.Count; i++)
                {
                    var effect = towerData.AttackEffects[i];
                    if (effect != null)
                    {
                        effect.ApplyEffect(targetTransform, EffectiveDamage, Gameplay);
                    }
                }
            }

            // 2. Áp dụng hiệu ứng cục bộ (nếu có gán thêm)
            if (_attackEffects != null)
            {
                for (int i = 0; i < _attackEffects.Count; i++)
                {
                    var effect = _attackEffects[i];
                    if (effect != null)
                    {
                        effect.ApplyEffect(targetTransform, EffectiveDamage, Gameplay);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, EffectiveRange);
        }

        protected override void OnDestroy()
        {
            if (_health != null) _health.Defeated -= OnDefeated;
            base.OnDestroy();
        }
    }
}

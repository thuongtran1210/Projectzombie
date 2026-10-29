using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Simple stationary auto-attack tower for the Defense MVP.</summary>
    public sealed class DefenseTowerPrototype3D : DefenseGameplayBehaviour
    {
        [Header("── 1. CHỈ SỐ BẮN CƠ BẢN (Cấp 1) ──")]
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

        [Header("── 2. TĂNG TIẾN KHI NÂNG CẤP (Level Up Scaling) ──")]
        [Tooltip("Lượng sát thương cộng thêm mỗi khi tháp lên cấp")]
        [SerializeField] private float _damagePerLevel = 10f;

        [Tooltip("Tầm bắn cộng thêm mỗi khi tháp lên cấp (mét)")]
        [SerializeField] private float _rangePerLevel = 1f;

        [Tooltip("Tốc độ bắn gia tăng mỗi khi tháp lên cấp (tỷ lệ phần trăm)")]
        [SerializeField] private float _fireRatePerLevel = .3f;

        private IDefenseProjectileLauncher _launcher;
        private DefenseAttackTargetSelector3D _targetSelector;
        private float nextShot;
        private DefenseSupportTarget3D _health;
        public int Level { get; private set; } = 1;
        public int UpgradeCost => Level < 3 ? Level * 40 : -1;
        public float EffectiveDamage => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerDamage, damage + (Level - 1) * _damagePerLevel) : damage;
        public float EffectiveRange => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.TowerAttackRange, attackRange + (Level - 1) * _rangePerLevel) : attackRange;

        protected override void OnGameplayBound()
        {
            _targetSelector = new DefenseAttackTargetSelector3D(transform, _targeting);
            _launcher = _customLauncher as IDefenseProjectileLauncher ?? _projectileVfx;
            if (_health != null) _health.Defeated -= OnDefeated;
            _health = GetComponent<DefenseSupportTarget3D>();
            if (_health != null) _health.Defeated += OnDefeated;
        }

        public void PrepareForBuild() { Level = 1; nextShot = 0f; _health?.ResetForRecruit(); }
        public bool TryUpgrade(DefenseWoodWallet3D wallet)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || (_health != null && _health.IsDead) || UpgradeCost < 0 || wallet == null) return false;
            if (!wallet.TrySpend(UpgradeCost)) return false;
            Level++;
            _health?.IncreaseCapacity(100f);
            return true;
        }
        private void OnDefeated() => gameObject.SetActive(false);

        protected override void OnGameplayReset() { nextShot = 0f; Level = 1; }

        private void Update()
        {
            if (!CanAdvanceGameplay || (_health != null && _health.IsDead) || GameplayTime < nextShot) return;
            var target = _targetSelector?.Find(Gameplay.Targets, EffectiveRange);
            if (target == null) return;
            nextShot = GameplayTime + fireInterval / (1f + (Level - 1) * _fireRatePerLevel);

            var request = new DefenseProjectileLaunchRequest(
                source: this,
                origin: transform.position + Vector3.up * 1.65f,
                target: target,
                damage: EffectiveDamage,
                runGeneration: Gameplay.RunGeneration,
                sessionIdentity: Gameplay.SessionIdentity);

            bool launched = _launcher != null && _launcher.TryLaunch(request);
            if (!launched)
            {
                target.TakeDamage(new DefenseDamageContext(EffectiveDamage, this, target.TargetTransform.position,
                    runGeneration: Gameplay.RunGeneration, sessionIdentity: Gameplay.SessionIdentity));
            }
#if UNITY_EDITOR && DEFENSE_TOWER_DEBUG
            Debug.Log("[TREBRAVE Defense3D] Tower hit " + target.TargetTransform.name + " for " + EffectiveDamage);
#endif
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, EffectiveRange);
        }

        protected override void OnDestroy() { if (_health != null) _health.Defeated -= OnDefeated; base.OnDestroy(); }
    }
}

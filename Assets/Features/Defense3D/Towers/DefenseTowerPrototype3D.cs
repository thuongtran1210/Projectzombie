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
        private float _nextSpecialShot;
        private bool _upgrading;
        private IDefenseEffectService _presentation;
        private bool _firingLaser;
        private readonly IDefenseEnemyTarget3D[] _beamTargets = new IDefenseEnemyTarget3D[3];
        private readonly DefenseActorLifetime[] _beamLeases = new DefenseActorLifetime[3];
        private readonly DefenseEffectHandle[] _beamHandles = new DefenseEffectHandle[3];
        public void BindAttackPresentation(IDefenseEffectService effects) { ClearBeams(); _presentation = effects; }
        private void ClearBeams()
        {
            for (int i = 0; i < _beamHandles.Length; i++)
            { if (_beamHandles[i].IsAssigned) _presentation?.Stop(_beamHandles[i]); _beamHandles[i] = default; }
        }
        private GameObject[] _levelVisuals;
        private Transform[] _levelTurrets, _levelMuzzles;
        private Renderer[] _originalRenderers;
        private bool[] _originalEnabled;
        public event System.Action<DefenseTowerPrototype3D> Released;
        public void Configure(SO_TowerData data, IDefenseProjectileService launcher)
        {
            towerData = data;
            _customLauncher = launcher as MonoBehaviour;
            _launcher = launcher;
        }
        public async System.Threading.Tasks.Task PrepareVisualsAsync()
        {
            _originalRenderers = GetComponentsInChildren<Renderer>(true);
            _originalEnabled = new bool[_originalRenderers.Length];
            for (int i = 0; i < _originalRenderers.Length; i++) _originalEnabled[i] = _originalRenderers[i].enabled;
            _levelVisuals = new GameObject[MaxLevel];
            _levelTurrets = new Transform[MaxLevel]; _levelMuzzles = new Transform[MaxLevel];
            for (int i = 0; i < MaxLevel; i++)
            {
                var prefab = towerData.GetLevelStats(i + 1).visualModelPrefab;
                if (prefab == null) continue;
                _levelVisuals[i] = Instantiate(prefab, transform);
                _levelTurrets[i] = _levelVisuals[i].transform.Find("Turret");
                _levelMuzzles[i] = _levelTurrets[i] != null ? _levelTurrets[i].Find("Muzzle") : null;
                _levelVisuals[i].SetActive(false);
                await System.Threading.Tasks.Task.Yield();
            }
        }
        private void ApplyLevelVisual()
        {
            if (_levelVisuals == null) return;
            bool custom = _levelVisuals[Level - 1] != null;
            for (int i = 0; i < _originalRenderers.Length; i++)
                if (_originalRenderers[i].GetComponentInParent<UI.DefenseEnemyHealthView3D>() == null)
                    _originalRenderers[i].enabled = !custom && _originalEnabled[i];
            for (int i = 0; i < _levelVisuals.Length; i++) if (_levelVisuals[i] != null) _levelVisuals[i].SetActive(custom && i == Level - 1);
        }
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
                    return nextTier.buildCost;
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

        public Vector3 FireOrigin => _levelMuzzles != null && Level > 0 && Level <= _levelMuzzles.Length && _levelMuzzles[Level - 1] != null
            ? _levelMuzzles[Level - 1].position : firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.65f;
        public void BindHealthPresentation(Camera camera)
        {
            if (TryGetComponent<UI.DefenseStructureHealthPresenter3D>(out var presenter))
                presenter.Bind(GetComponent<DefenseSupportTarget3D>(), Gameplay, camera);
        }

        protected override void OnGameplayBound()
        {
            var profile = (towerData != null && towerData.TargetingStrategy != null) ? towerData.TargetingStrategy : _targeting;
            _targetSelector = new DefenseAttackTargetSelector3D(transform, profile);

            _launcher = _customLauncher as IDefenseProjectileLauncher ?? _launcher ?? _projectileVfx;
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
            _nextSpecialShot = GameplayTime + (towerData != null && towerData.SpecialAttack != null ? towerData.SpecialAttack.Cooldown : 0f);
            _health?.ConfigureCapacity(towerData != null && towerData.GetLevelStats(1).maxHealth > 0f ? towerData.GetLevelStats(1).maxHealth : 150f);
            ApplyLevelVisual();
        }

        public bool TryUpgrade(DefenseWoodWallet3D wallet)
        {
            if (_upgrading || !CanAdvanceGameplay || !isActiveAndEnabled || (_health != null && _health.IsDead) || UpgradeCost < 0 || wallet == null)
                return false;

            _upgrading = true;
            try
            {
                if (!wallet.TrySpend(UpgradeCost)) return false;
                Level++;
                _health?.ConfigureCapacity(towerData != null && towerData.GetLevelStats(Level).maxHealth > 0f ? towerData.GetLevelStats(Level).maxHealth : 150f + (Level - 1) * 100f);
                ApplyLevelVisual();
                return true;
            }
            finally { _upgrading = false; }
        }

        private void OnDefeated() => gameObject.SetActive(false);
        private void OnDisable()
        {
            ClearBeams();
            _currentTarget = null;
            Released?.Invoke(this);
        }

        protected override void OnGameplayReset()
        {
            ClearBeams();
            nextShot = 0f;
            nextTargetScanTime = 0f;
            _nextSpecialShot = GameplayTime + (towerData != null && towerData.SpecialAttack != null ? towerData.SpecialAttack.Cooldown : 0f);
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
                if (!IsValidTarget(_currentTarget))
                {
                    _currentTarget = null;
                    return;
                }

                if (TryFire(_currentTarget, towerData != null ? towerData.Attack : null)) nextShot = GameplayTime + BaseFireInterval;
            }
        }

        private void LateUpdate()
        {
            if (!CanAdvanceGameplay || towerData == null || towerData.SpecialAttack == null || _currentTarget == null) return;
            var special = towerData.SpecialAttack;
            if (Level < special.UnlockLevel || GameplayTime < _nextSpecialShot) return;
            if (TryFire(_currentTarget, special)) _nextSpecialShot = GameplayTime + Mathf.Max(.1f, special.Cooldown);
        }

        private void UpdateTarget()
        {
            _currentTarget = _targetSelector?.Find(Gameplay?.Targets, EffectiveRange);
        }

        private bool IsValidTarget(IDefenseEnemyTarget3D target)
        {
            // Interface references do not use Unity's destroyed-object null comparison.
            if (target == null || (target is Object actor && actor == null)) return false;
            return target.IsTargetable && !target.IsDead && target.TargetTransform != null &&
                (target.TargetTransform.position - transform.position).sqrMagnitude <= EffectiveRange * EffectiveRange;
        }

        /// <summary>Snapshot one attack. A rejected launch consumes no cooldown and causes no fallback damage.</summary>
        public bool TryFire(IDefenseEnemyTarget3D target, DefenseTowerAttackProfileSO profile)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || (_health != null && _health.IsDead) || !IsValidTarget(target) ||
                towerData == null || profile == null || Level < profile.UnlockLevel) return false;
            if (profile.Mode == DefenseTowerAttackMode.Laser) return TryFireLaser(target, profile);
            if (!(_launcher is IDefenseProjectileService pool)) return false;
            var definition = profile.Projectile;
            var prefab = definition != null && definition.VisualPrefab != null ? definition.VisualPrefab : towerData.ProjectilePrefab;
            if (!pool.CanLaunch(prefab, profile.Shots)) return false;
            if (_levelTurrets != null && Level <= _levelTurrets.Length && _levelTurrets[Level - 1] != null)
            {
                var turret = _levelTurrets[Level - 1];
                Vector3 direction = target.TargetTransform.position - turret.position; direction.y = 0f;
                if (direction.sqrMagnitude > .001f) turret.rotation = Quaternion.LookRotation(direction);
            }
            var request = new DefenseProjectileLaunchRequest(this, FireOrigin, target,
                EffectiveDamage * profile.DamageMultiplier, Gameplay.RunGeneration, Gameplay.SessionIdentity,
                towerData.ProjectileSpeed, towerData.ProjectileArcHeight);
            for (int i = 0; i < profile.Shots; i++)
                if (!pool.LaunchProjectile(prefab, request, profile.Effects, definition)) return false;
            return true;
        }
        private bool TryFireLaser(IDefenseEnemyTarget3D primary, DefenseTowerAttackProfileSO profile)
        {
            if (_firingLaser) return false;
            _firingLaser = true;
            try { return FireLaser(primary, profile); }
            finally { _firingLaser = false; }
        }
        private bool FireLaser(IDefenseEnemyTarget3D primary, DefenseTowerAttackProfileSO profile)
        {
            ClearBeams();
            int count = Mathf.Clamp(Level, 1, 3), generation = Gameplay.RunGeneration;
            float damagePerBeam = EffectiveDamage * profile.DamageMultiplier;
            Vector3 origin = FireOrigin;
            _beamTargets[0] = primary;
            for (int i = 1; i < count; i++)
            {
                var secondary = _targetSelector?.FindExcluding(Gameplay.Targets, EffectiveRange, _beamTargets, i);
                _beamTargets[i] = IsValidTarget(secondary) ? secondary : primary;
            }
            for (int i = 0; i < count; i++) _beamLeases[i] = (_beamTargets[i] as DefenseEnemy3D)?.Lifetime;
            bool hit = false;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var candidate = _beamTargets[i]; bool duplicate = false;
                    for (int j = 0; j < i; j++) if (ReferenceEquals(candidate, _beamTargets[j])) duplicate = true;
                    if (duplicate || !CanAdvanceGameplay || generation != Gameplay.RunGeneration ||
                        (_health != null && _health.IsDead) || !IsValidTarget(candidate)) continue;
                    if (candidate is DefenseEnemy3D enemy && (!ReferenceEquals(enemy.Lifetime, _beamLeases[i]) ||
                        (_beamLeases[i] != null && !_beamLeases[i].IsCurrent(Gameplay)))) continue;
                    int beams = 0;
                    for (int j = 0; j < count; j++) if (ReferenceEquals(candidate, _beamTargets[j])) beams++;
                    Vector3 endpoint = candidate.TargetTransform.position + Vector3.up * .65f;
                    bool accepted = candidate.TakeDamage(new DefenseDamageContext(damagePerBeam * beams, this, endpoint,
                        runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity));
                    if (!accepted) continue;
                    hit = true;
                    if (this == null || !isActiveAndEnabled || !CanAdvanceGameplay || generation != Gameplay.RunGeneration ||
                        (_health != null && _health.IsDead)) continue;
                    for (int j = 0; j < count; j++) if (ReferenceEquals(candidate, _beamTargets[j]))
                    {
                        Vector3 direction = endpoint - origin;
                        Vector3 side = Vector3.Cross(direction, Vector3.up).normalized;
                        float offset = beams > 1 ? (j - (count - 1) * .5f) * .28f : 0f;
                        Vector3 start = origin + side * offset;
                        Vector3 end = endpoint + side * (offset * .5f);
                        _beamHandles[j] = PresentBeam(profile.BeamEffect, start, end);
                    }
                    PresentLaserHit(profile.Projectile?.ImpactEffect, endpoint);
                    if (!IsValidTarget(candidate) || (candidate is DefenseEnemy3D current && !ReferenceEquals(current.Lifetime, _beamLeases[i]))) continue;
                    foreach (var effect in profile.Effects) effect?.ApplyEffect(candidate.TargetTransform, damagePerBeam, Gameplay);
                }
            }
            finally { System.Array.Clear(_beamTargets, 0, _beamTargets.Length); System.Array.Clear(_beamLeases, 0, _beamLeases.Length); }
            return hit;
        }
        private DefenseEffectHandle PresentBeam(DefenseEffectProfileSO profile, Vector3 start, Vector3 end)
        {
            if (_presentation == null || profile == null) return default;
            Vector3 direction = end - start;
            if (direction.sqrMagnitude < .001f) return default;
            try { return _presentation.Play(new DefenseEffectRequest(profile, start, direction.magnitude * .5f, this,
                rotation: Quaternion.LookRotation(direction))); }
            catch (System.Exception exception) { Debug.LogException(exception); return default; }
        }
        private void PresentLaserHit(DefenseEffectProfileSO profile, Vector3 position)
        {
            if (_presentation == null || profile == null) return;
            try { _presentation.Play(profile, position, .6f); }
            catch (System.Exception exception) { Debug.LogException(exception); }
        }
        protected override void OnGameplayCleanup() => ClearBeams();

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, EffectiveRange);
        }

        protected override void OnDestroy()
        {
            ClearBeams();
            if (_health != null) _health.Defeated -= OnDefeated;
            base.OnDestroy();
        }
    }
}

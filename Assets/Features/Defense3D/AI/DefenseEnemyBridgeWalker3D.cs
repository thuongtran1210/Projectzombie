using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Compatibility adapter for existing bridge scenes and Yaksha callers.
    /// New health, movement, combat and drop behavior live in independent components.
    /// </summary>
    [RequireComponent(typeof(DefenseEnemy3D))]
    [RequireComponent(typeof(DefenseEnemyDrop3D))]
    public sealed class DefenseEnemyBridgeWalker3D : MonoBehaviour, IDefenseDamageable3D, IDefenseBossCombatAdapter3D
    {
        // Retain serialized fields so existing scene/prefab overrides survive migration.
        [SerializeField, Min(0.1f)] private float speed = 2.5f;
        [SerializeField, Min(0f)] private float baseDamage = 25f;
        [SerializeField, Min(0.1f)] private float arrivalRadius = 1.2f;
        [SerializeField, Min(0.1f)] private float attackInterval = 2f;
        [SerializeField, Min(1f)] private float maxHP = 100f;
        [SerializeField, Min(1)] private int woodDropAmount = 10;
        [SerializeField] private DefenseBasePrototype3D targetBase;
        private DefenseEnemy3D enemy;
        private DefenseGroundMovement3D movement;
        private DefenseEnemyAttack3D attack;
        private DefenseGroundAggro3D _aggro;
        private DefenseHeroHealth3D _hero;
        private bool configured;
        private DefenseEnemy3D Enemy
        {
            get { EnsureComponents(); return enemy; }
        }
        public float CurrentHP => Enemy.CurrentHP;
        public float MaxHP => Enemy.MaxHP;
        public bool IsDead => Enemy.IsDead;
        public event Action<DefenseEnemyBridgeWalker3D> Defeated;
        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        private event Action damageableDefeated;
        event Action IDefenseDamageable3D.Defeated
        {
            add => damageableDefeated += value;
            remove => damageableDefeated -= value;
        }
        private void Awake() => EnsureComponents();

        /// <summary>Inject session dependencies before initializing a spawned actor.</summary>
        public void BindGameplay(IDefenseGameplayContext gameplay, DefenseWoodWallet3D collectorWallet, Transform pickupRoot = null, DefenseFriendlyTargets3D friends = null, IDefenseWoodDrops woodDrops = null)
        {
            EnsureComponents();
            enemy.BindGameplay(gameplay);
            if (TryGetComponent<DefenseEnemyVisualMotion3D>(out var visual)) visual.BindGameplay(gameplay);
            if (collectorWallet != null) collectorWallet.TryGetComponent(out _hero);
            if (movement != null)
            {
                if (_hero != null) movement.BindHero(_hero);
                movement.BindGameplay(gameplay);
            }
            if (attack != null) attack.BindGameplay(gameplay);
            var drops = GetComponent<DefenseEnemyDrop3D>();
            if (drops != null)
            {
                drops.BindGameplay(gameplay);
                drops.BindCollector(collectorWallet, pickupRoot, woodDrops);
            }
            if (_aggro != null)
            {
                _aggro.BindFriendlyTargets(friends);
                _aggro.BindGameplay(gameplay);
            }
        }
        private void EnsureComponents()
        {
            if (enemy != null) return;
            // Composition is authored on the prefab, visible before entering Play.
            enemy = GetComponent<DefenseEnemy3D>();
            movement = GetComponent<DefenseGroundMovement3D>();
            attack = GetComponent<DefenseEnemyAttack3D>();
            _aggro = GetComponent<DefenseGroundAggro3D>();
            if (movement != null) movement.Arrived += BeginAttack;
            enemy.Defeated += OnDefeated;
            enemy.HealthChanged += ForwardHealth;
            enemy.Damaged += ForwardDamage;
        }
        public void Initialize(DefenseEnemyStats stats)
        {
            EnsureComponents();
            attack?.StopAttack();
            movement?.Stop();
            enemy.Initialize(stats);
            _aggro?.Stop();
            configured = true;
        }
        public void Initialize(DefenseEnemyDataSO definition)
        {
            EnsureComponents();
            attack?.StopAttack();
            movement?.Stop();
            enemy.Initialize(definition);
            _aggro?.Stop();
            configured = true;
        }
        public void Initialize(DefenseEnemySpawnDefinition definition)
        {
            EnsureComponents();
            attack?.StopAttack();
            movement?.Stop();
            _aggro?.Stop();
            enemy.Initialize(definition);
            configured = true;
        }
        private void EnsureConfiguration()
        {
            EnsureComponents();
            if (configured) return;
            if (enemy.Definition != null) enemy.Initialize(enemy.Definition);
            else enemy.Initialize(new DefenseEnemyStats("legacy", name,
                DefenseEnemyLocomotionType.GroundNavMesh, maxHP, speed, baseDamage, woodDropAmount, 0)
                { AttackInterval = attackInterval, AttackRange = arrivalRadius });
            configured = true;
        }
        public void Configure(int lane, DefenseBasePrototype3D target)
        {
            EnsureConfiguration();
            targetBase = target;
            attack?.StopAttack();
            if (movement != null && _hero != null) movement.BindHero(_hero);

            // Đa hình OCP: Ủy quyền trực tiếp cho Locomotion Strategy đã gán
            if (enemy.Locomotion != null)
            {
                enemy.Locomotion.Configure(lane, target);
                return;
            }

            // Fallback an toàn
            if (movement != null)
            {
                movement.Configure(lane, target);
            }
        }
        public void SetMaxHealth(float hp)
        {
            EnsureConfiguration();
            enemy.SetMaxHealth(hp);
        }
        public void PrepareArenaBoss(float hp)
        {
            SetMaxHealth(hp);
            _aggro?.Stop();
            movement?.Stop();
            attack?.StopAttack();
        }
        public void BeginBossAssault(DefenseBasePrototype3D target)
        {
            if (IsDead) return;
            Configure(1, target);
            Debug.Log("[TREBRAVE Defense3D] Boss entered ASSAULT_BASE.");
        }
        public void StopBossCombat()
        {
            _aggro?.Stop();
            movement?.Stop();
            attack?.StopAttack();
        }
        private void BeginAttack()
        {
            if (_aggro != null && _aggro.OwnsCombat) return;
            if (!IsDead && targetBase != null && !targetBase.IsDestroyed && attack != null) attack.Begin(targetBase);
        }
        public bool TakeDamage(DefenseDamageContext context) => Enemy.TakeDamage(context);
        public void ReceiveDamage(float damage) => TakeDamage(new DefenseDamageContext(damage));
        private void OnDefeated()
        {
            movement?.Stop();
            attack?.StopAttack();
            Debug.Log("[TREBRAVE Defense3D] Enemy defeated: " + name);
            Defeated?.Invoke(this);
            damageableDefeated?.Invoke();
        }
        private void ForwardHealth(float current, float max) => HealthChanged?.Invoke(current, max);
        private void ForwardDamage(DefenseDamageContext context) => Damaged?.Invoke(context);
        private void OnDisable()
        {
            if (movement != null) movement.Stop();
            if (attack != null) attack.StopAttack();
        }
        private void OnDestroy()
        {
            if (movement != null) movement.Arrived -= BeginAttack;
            if (enemy == null) return;
            enemy.Defeated -= OnDefeated;
            enemy.HealthChanged -= ForwardHealth;
            enemy.Damaged -= ForwardDamage;
        }
    }
}

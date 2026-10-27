using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Compatibility adapter for existing bridge scenes and Yaksha callers.
    /// New health, movement, combat and drop behavior live in independent components.
    /// </summary>
    [RequireComponent(typeof(DefenseEnemy3D), typeof(DefenseGroundMovement3D), typeof(DefenseEnemyAttack3D))]
    [RequireComponent(typeof(DefenseEnemyDrop3D))]
    public sealed class DefenseEnemyBridgeWalker3D : MonoBehaviour, IDefenseDamageable3D
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
        public void BindGameplay(IDefenseGameplayContext gameplay, DefenseWoodWallet3D collectorWallet, Transform pickupRoot = null, DefenseFriendlyTargets3D friends = null)
        {
            EnsureComponents();
            enemy.BindGameplay(gameplay);
            if (TryGetComponent<DefenseEnemyVisualMotion3D>(out var visual)) visual.BindGameplay(gameplay);
            movement.BindGameplay(gameplay);
            attack.BindGameplay(gameplay);
            var drops = GetComponent<DefenseEnemyDrop3D>();
            drops.BindGameplay(gameplay);
            drops.BindCollector(collectorWallet,pickupRoot);
            if (_aggro!=null)
            {
                _aggro.BindFriendlyTargets(friends);
                _aggro.BindGameplay(gameplay);
            }
            if (collectorWallet!=null) collectorWallet.TryGetComponent(out _hero);
        }
        private void EnsureComponents()
        {
            if (enemy != null) return;
            // Composition is authored on the prefab, visible before entering Play.
            enemy = GetComponent<DefenseEnemy3D>();
            movement = GetComponent<DefenseGroundMovement3D>();
            attack = GetComponent<DefenseEnemyAttack3D>();
            _aggro=GetComponent<DefenseGroundAggro3D>();
            movement.Arrived += BeginAttack;
            enemy.Defeated += OnDefeated;
            enemy.HealthChanged += ForwardHealth;
            enemy.Damaged += ForwardDamage;
        }
        public void Initialize(DefenseEnemyStats stats)
        {
            EnsureComponents();
            attack.StopAttack();
            movement.Stop();
            enemy.Initialize(stats);
            _aggro?.Stop();
            configured = true;
        }
        public void Initialize(DefenseEnemyDataSO definition)
        {
            EnsureComponents();
            attack.StopAttack();
            movement.Stop();
            enemy.Initialize(definition);
            _aggro?.Stop();
            configured = true;
        }
        public void Initialize(DefenseEnemySpawnDefinition definition)
        {
            EnsureComponents();
            attack.StopAttack();
            movement.Stop();
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
            attack.StopAttack();
            if (enemy.Stats.Locomotion == DefenseEnemyLocomotionType.AmphibianWaterToShore)
            {
                var water = GetComponent<DefenseWaterMovement3D>();
                if (water == null) throw new InvalidOperationException("Missing authored amphibian movement.");
                water.Begin(target);
                return;
            }
            if (enemy.Stats.Locomotion == DefenseEnemyLocomotionType.AirFlight)
            {
                var air = GetComponent<DefenseAirMovement3D>();
                if (air == null) throw new InvalidOperationException("Missing authored Air movement.");
                air.Begin(target);
                return;
            }
            // Configure route without resetting HP; arena -> assault preserves damage.
            if (_aggro!=null && !enemy.Stats.IsBoss)
                _aggro.Configure(lane,target,_hero,enemy.Stats.Locomotion==DefenseEnemyLocomotionType.ParatrooperLanding);
            else
            {
                _aggro?.Stop();
                var collider=target!=null ? target.GetComponent<Collider>() : null;
                float extent=collider!=null ? Mathf.Max(collider.bounds.extents.x,collider.bounds.extents.z) : 0f;
                movement.Follow(DefenseBridgeRoute3D.Create(lane,transform.position,target.transform.position),
                    enemy.Stats.Speed,enemy.Stats.AttackRange+extent);
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
            movement.Stop();
            attack.StopAttack();
        }
        public void BeginBossAssault(DefenseBasePrototype3D target)
        {
            if (IsDead) return;
            Configure(1, target);
            Debug.Log("[TREBRAVE Defense3D] Yaksha entered ASSAULT_BASE.");
        }
        public void StopBossCombat()
        {
            _aggro?.Stop();
            movement?.Stop();
            attack?.StopAttack();
        }
        private void BeginAttack()
        {
            if (_aggro!=null && _aggro.OwnsCombat) return;
            if (!IsDead && targetBase != null && !targetBase.IsDestroyed) attack.Begin(targetBase);
        }
        public bool TakeDamage(DefenseDamageContext context) => Enemy.TakeDamage(context);
        public void ReceiveDamage(float damage) => TakeDamage(new DefenseDamageContext(damage));
        private void OnDefeated()
        {
            movement.Stop();
            attack.StopAttack();
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

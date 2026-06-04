using ProjectZombie.Core.ScriptableObjects;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HealthSystem))]
    public class Enemy : MonoBehaviour
    {
        [Header("References")]
        public EnemyConfig Config;
        [SerializeField] private GameObject expGemPrefab;
        
        public Rigidbody2D Rb { get; private set; }
        public HealthSystem HealthSystem { get; private set; }
        public EnemyAnimator EnemyAnimator { get; private set; }
        public Transform PlayerTransform { get; private set; }
        public HealthSystem PlayerHealthSystem { get; private set; }
        
        public AttackStrategy Attacker { get; private set; }
        public CombatMovementStrategy Movement { get; private set; }
        public EnemyStateMachine StateMachine { get; private set; }

        // Các trạng thái
        public EnemyIdleState IdleState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyRepositionState RepositionState { get; private set; }
        public EnemyDeadState DeadState { get; private set; }

        // Faction Passive Multipliers
        public float MoveSpeedMultiplier { get; set; } = 1f;
        public float DamageMultiplier { get; set; } = 1f;

        public float GetTotalDamage()
        {
            if (Config == null) return 0f;
            return Config.damageToPlayer * DamageMultiplier;
        }

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            HealthSystem = GetComponent<HealthSystem>();
            EnemyAnimator = GetComponent<EnemyAnimator>();
            Rb.freezeRotation = true;

            if (Config != null)
            {
                HealthSystem.SetMaxHealth(Config.maxHealth);
            }

            Attacker = GetComponent<AttackStrategy>();
            Movement = GetComponent<CombatMovementStrategy>();

            // Khởi tạo State Machine và các trạng thái
            StateMachine = new EnemyStateMachine();
            IdleState = new EnemyIdleState(this, StateMachine);
            ChaseState = new EnemyChaseState(this, StateMachine);
            AttackState = new EnemyAttackState(this, StateMachine);
            RepositionState = new EnemyRepositionState(this, StateMachine);
            DeadState = new EnemyDeadState(this, StateMachine);
        }

        private void OnEnable()
        {
            FindPlayer();

            // Đặt lại State thành Chase mỗi khi được lấy ra từ Pool
            if (StateMachine != null && ChaseState != null)
            {
                StateMachine.Initialize(ChaseState);
            }

            if (HealthSystem != null)
            {
                HealthSystem.OnDied += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (HealthSystem != null)
            {
                HealthSystem.OnDied -= HandleDeath;
            }
        }

        public void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerTransform = player.transform;
                PlayerHealthSystem = player.GetComponent<HealthSystem>();
            }
        }

        private void Update()
        {
            StateMachine.CurrentState?.Update();
        }

        private void FixedUpdate()
        {
            StateMachine.CurrentState?.FixedUpdate();
        }

        private void HandleDeath()
        {
            if (expGemPrefab != null)
            {
                Instantiate(expGemPrefab, transform.position, Quaternion.identity);
            }

            StateMachine.ChangeState(DeadState);
        }
    }
}

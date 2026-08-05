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

        [Header("Boss Settings")]
        [SerializeField] private bool isBoss = false;
        public bool IsBoss => isBoss || CompareTag("Boss");

        // Temporary Trap Circling State
        private bool _isTrapCircling;
        private Vector3 _trapCenter;
        private float _trapRadius;
        private float _trapEndTime;

        public bool IsTrapCircling => _isTrapCircling && Time.time < _trapEndTime;
        public Vector3 TrapCenter => _trapCenter;
        public float TrapRadius => _trapRadius;

        public float GetTotalDamage()
        {
            if (Config == null) return 0f;
            return Config.damageToPlayer * DamageMultiplier;
        }

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            HealthSystem = GetComponent<HealthSystem>();
            EnemyAnimator = GetComponentInChildren<EnemyAnimator>();
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

        /// <summary>
        /// Ép trạng thái di chuyển bám quanh 8 điểm neo của Bát Quái Trận Đồ (Mục 3.1.2 GDD v4.0).
        /// </summary>
        public void ApplyTrapCirclingState(Vector3 center, float radius, float duration)
        {
            if (IsBoss) return; // Boss miễn nhiễm
            _isTrapCircling = true;
            _trapCenter = center;
            _trapRadius = radius;
            _trapEndTime = Time.time + duration;
        }

        private void HandleDeath()
        {
            if (expGemPrefab != null)
            {
                Instantiate(expGemPrefab, transform.position, Quaternion.identity);
            }

            StateMachine.ChangeState(DeadState);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Config == null) return;

            bool isRanged = Attacker is RangedAttackStrategy || Movement is RangedMovementStrategy;

            if (isRanged)
            {
                // Tầm đánh Tầm xa (Ranged)
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, Config.attackRange);

                if (Config.preferredDistance > 0f)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(transform.position, Config.preferredDistance);
                }
                if (Config.minDistance > 0f)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f);
                    Gizmos.DrawWireSphere(transform.position, Config.minDistance);
                }

                UnityEditor.Handles.Label(transform.position + Vector3.up * (Config.attackRange + 0.2f), $"🏹 Ranged Attack: {Config.attackRange}m");
            }
            else
            {
                // Tầm đánh Cận chiến (Melee) — Chỉ vẽ duy nhất 1 vòng màu đỏ rõ ràng
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
                Gizmos.DrawWireSphere(transform.position, Config.attackRange);
                
                // Vẽ đĩa mờ thể hiện vùng sát thương cận chiến
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
                Gizmos.DrawSphere(transform.position, Config.attackRange);

                UnityEditor.Handles.Label(transform.position + Vector3.up * (Config.attackRange + 0.2f), $"⚔️ Melee Slash Range: {Config.attackRange}m");
            }
        }
#endif
    }
}

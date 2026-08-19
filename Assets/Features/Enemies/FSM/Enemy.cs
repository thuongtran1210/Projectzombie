using ProjectZombie.Core.ScriptableObjects;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HealthSystem))]
    public class Enemy : MonoBehaviour, IStatusReceiver
    {
        [Header("References")]
        public EnemyConfig Config;
        // FIX TODO: Sử dụng Object Pooling cho ExpGem để tránh Instantiate/Destroy nhiều lần gây lag
        [SerializeField] private GameObject expGemPrefab;
        
        public Rigidbody2D Rb { get; private set; }
        public HealthSystem HealthSystem { get; private set; }
        public EnemyAnimator EnemyAnimator { get; private set; }
        public Transform PlayerTransform { get; private set; }
        public HealthSystem PlayerHealthSystem { get; private set; }
        
        public AttackStrategy Attacker { get; private set; }
        public CombatMovementStrategy Movement { get; private set; }
        public EnemyStateMachine StateMachine { get; private set; }
        public EnemyStatusController StatusController { get; private set; }

        // IStatusReceiver Properties
        public float Tenacity => IsBoss ? 0.7f : (Config != null && Config.tier == EnemyTier.Elite ? 0.3f : 0f);
        public bool CanMove => StatusController != null ? StatusController.CanMove : true;
        public bool CanAttack => StatusController != null ? StatusController.CanAttack : true;

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
        public bool IsHeavyArmor => Config != null && Config.isHeavyArmor;

        private ProjectZombie.Features.Boss.BossElementController _bossElementController;

        public ElementType CurrentElement
        {
            get
            {
                if (_bossElementController != null)
                {
                    return _bossElementController.CurrentElement;
                }
                return Config != null ? Config.elementType : ElementType.None;
            }
        }

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
            _bossElementController = GetComponent<ProjectZombie.Features.Boss.BossElementController>();
            Rb.freezeRotation = true;

            if (Config != null)
            {
                HealthSystem.SetMaxHealth(Config.maxHealth);
            }

            Attacker = GetComponent<AttackStrategy>();
            Movement = GetComponent<CombatMovementStrategy>();
            StatusController = GetComponent<EnemyStatusController>();
            if (StatusController == null) StatusController = gameObject.AddComponent<EnemyStatusController>();

            // Tự động đảm bảo có HitFlashFeedback
            if (GetComponent<Visuals.HitFlashFeedback>() == null)
            {
                gameObject.AddComponent<Visuals.HitFlashFeedback>();
            }

            // Khởi tạo State Machine và các trạng thái
            StateMachine = new EnemyStateMachine();
            IdleState = new EnemyIdleState(this, StateMachine);
            ChaseState = new EnemyChaseState(this, StateMachine);
            AttackState = new EnemyAttackState(this, StateMachine);
            RepositionState = new EnemyRepositionState(this, StateMachine);
            DeadState = new EnemyDeadState(this, StateMachine);
        }

        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            if (StatusController != null)
            {
                StatusController.ApplyKnockback(direction, force, duration);
            }
        }

        public void ApplyStatusEffect(StatusEffectType type, float duration, float value = 0f, float tickInterval = 0.5f, System.Action<float> onTickDamage = null)
        {
            if (StatusController != null)
            {
                StatusController.ApplyStatusEffect(type, duration, value, tickInterval, onTickDamage ?? ((damage) =>
                {
                    if (HealthSystem != null) HealthSystem.TakeDamage(damage);
                }));
            }
        }

        public bool HasStatus(StatusEffectType type)
        {
            return StatusController != null && StatusController.HasStatus(type);
        }

        public void RemoveStatus(StatusEffectType type)
        {
            StatusController?.RemoveStatus(type);
        }

        private void OnEnable()
        {
            PlayerProvider.OnPlayerSpawned += HandlePlayerSpawned;
            PlayerProvider.OnPlayerDespawned += HandlePlayerDespawned;

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
            PlayerProvider.OnPlayerSpawned -= HandlePlayerSpawned;
            PlayerProvider.OnPlayerDespawned -= HandlePlayerDespawned;

            if (HealthSystem != null)
            {
                HealthSystem.OnDied -= HandleDeath;
            }
        }

        private void HandlePlayerSpawned(Transform playerTf, HealthSystem playerHp)
        {
            PlayerTransform = playerTf;
            PlayerHealthSystem = playerHp;
            if (StateMachine != null && StateMachine.CurrentState == IdleState && ChaseState != null)
            {
                StateMachine.ChangeState(ChaseState);
            }
        }

        private void HandlePlayerDespawned()
        {
            PlayerTransform = null;
            PlayerHealthSystem = null;
        }

        public void SetPlayer(Transform playerTransform, HealthSystem playerHealthSystem = null)
        {
            PlayerTransform = playerTransform;
            if (playerHealthSystem != null)
            {
                PlayerHealthSystem = playerHealthSystem;
            }
            else if (playerTransform != null)
            {
                PlayerHealthSystem = playerTransform.GetComponent<HealthSystem>();
            }
        }

        public void FindPlayer()
        {
            if (PlayerTransform != null) return;

            if (PlayerProvider.HasPlayer)
            {
                PlayerTransform = PlayerProvider.PlayerTransform;
                PlayerHealthSystem = PlayerProvider.PlayerHealth;
                return;
            }

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
                float expAmount = Config != null ? Config.expReward : 10f;
                if (Collectibles.ExpGemPoolManager.Instance != null)
                {
                    Collectibles.ExpGemPoolManager.Instance.SpawnGem(expGemPrefab, transform.position, expAmount);
                }
                else
                {
                    var gemObj = Instantiate(expGemPrefab, transform.position, Quaternion.identity);
                    var gem = gemObj.GetComponent<Collectibles.ExpGem>();
                    if (gem != null) gem.SetExpAmount(expAmount);
                }
            }

            StateMachine.ChangeState(DeadState);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Config == null) return;

            var attacker = Attacker != null ? Attacker : GetComponent<AttackStrategy>();
            var movement = Movement != null ? Movement : GetComponent<CombatMovementStrategy>();
            bool isRanged = attacker is RangedAttackStrategy || movement is RangedMovementStrategy;

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

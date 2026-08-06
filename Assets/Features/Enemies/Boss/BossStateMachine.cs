using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Boss;

namespace ProjectZombie.Features.Enemies.Boss
{
    public enum BossPhase
    {
        Phase1_100_50,
        Phase2_Sub50
    }

    /// <summary>
    /// FSM Quản lý Boss 1: Ngưu Đầu Mã Diện theo GDD 5.2.
    /// Phase 1 (100%-50% HP): Ngưu Xung Thiên (Dash x3 speed) + Địa Chấn Âm Ty (Ground Slam AoE Slow 40%).
    /// Phase 2 (<50% HP): Luân phiên đổi hệ Thổ/Hỏa + Triệu Hồn Âm Binh (Gọi 10 Ma Giáp).
    /// </summary>
    public class BossStateMachine : MonoBehaviour
    {
        [Header("Boss Identity")]
        public string bossName = "Ngưu Đầu Mã Diện";
        public ElementType currentElement = ElementType.Tho;

        [Header("Skills")]
        [SerializeField] private Skills.BullDashSkill bullDashSkill;
        [SerializeField] private Skills.GroundSlamSkill groundSlamSkill;

        [Header("Phase 2 Minion Spawn")]
        [SerializeField] private GameObject maGiapPrefab;
        [SerializeField] private int minionCount = 10;

        private HealthSystem _healthSystem;
        private BossAnimator _bossAnimator;
        private BossAnimationEventHandler _eventHandler;
        private BossPhase _currentPhase = BossPhase.Phase1_100_50;
        private float _skillTimer = 0f;
        private float _elementSwapTimer = 0f;
        private Transform _playerTransform;

        public BossPhase CurrentPhase => _currentPhase;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _bossAnimator = GetComponentInChildren<BossAnimator>();
            _eventHandler = GetComponentInChildren<BossAnimationEventHandler>();
            if (bullDashSkill == null) bullDashSkill = GetComponent<Skills.BullDashSkill>();
            if (groundSlamSkill == null) groundSlamSkill = GetComponent<Skills.GroundSlamSkill>();
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }

        private bool _isDead = false;

        private void Update()
        {
            if (_healthSystem == null) return;

            if (_healthSystem.CurrentHealth <= 0)
            {
                if (!_isDead)
                {
                    _isDead = true;
                    if (_bossAnimator != null) _bossAnimator.PlayAnimation("Dead");
                }
                return;
            }

            if (_playerTransform != null && _bossAnimator != null)
            {
                float dirX = _playerTransform.position.x - transform.position.x;
                _bossAnimator.FlipToDirection(dirX);
            }

            float hpPercent = _healthSystem.CurrentHealth / Mathf.Max(1f, _healthSystem.MaxHealth);

            // Transition Phase
            if (_currentPhase == BossPhase.Phase1_100_50 && hpPercent <= 0.5f)
            {
                EnterPhase2();
            }

            // Skill Cooldown Loop
            _skillTimer += Time.deltaTime;
            if (_skillTimer >= skillCooldown)
            {
                _skillTimer = 0f;
                ExecuteRandomSkill();
            }

            // Phase 2 Element Swap (10s/lần)
            if (_currentPhase == BossPhase.Phase2_Sub50)
            {
                _elementSwapTimer += Time.deltaTime;
                if (_elementSwapTimer >= 10f)
                {
                    _elementSwapTimer = 0f;
                    currentElement = currentElement == ElementType.Tho ? ElementType.Hoa : ElementType.Tho;
                    Debug.Log($"[BossStateMachine] {bossName} đổi thuộc tính Ngũ Hành sang: {currentElement}");
                }
            }
        }

        private void EnterPhase2()
        {
            _currentPhase = BossPhase.Phase2_Sub50;
            Debug.Log($"[BossStateMachine] ⚠️ {bossName} KÍCH HOẠT PHASE 2! HP <= 50%");

            // Triệu Hồn Âm Binh (Gọi 10 Ma Giáp)
            if (maGiapPrefab != null)
            {
                for (int i = 0; i < minionCount; i++)
                {
                    Vector3 spawnOffset = (Vector3)Random.insideUnitCircle * 3f;
                    Instantiate(maGiapPrefab, transform.position + spawnOffset, Quaternion.identity);
                }
            }
        }

        [Header("Skill Cooldown Settings")]
        [SerializeField] private float skillCooldown = 5f;

        private void ExecuteRandomSkill()
        {
            if (_playerTransform == null) return;

            if (Random.value > 0.5f && bullDashSkill != null)
            {
                TriggerBullDash();
            }
            else if (groundSlamSkill != null)
            {
                TriggerGroundSlam();
            }
        }

        [ContextMenu("Debug/Trigger Bull Dash Skill")]
        public void TriggerBullDash()
        {
            if (_playerTransform == null) Start();
            if (bullDashSkill != null && _playerTransform != null)
            {
                Debug.Log($"[DEBUG BOSS] Ép kích hoạt chiêu 'Ngưu Xung Thiên' (Bull Dash)!");
                bullDashSkill.PerformDash(_playerTransform.position);
            }
        }

        [ContextMenu("Debug/Trigger Ground Slam Skill")]
        public void TriggerGroundSlam()
        {
            if (groundSlamSkill != null)
            {
                Debug.Log($"[DEBUG BOSS] Ép kích hoạt chiêu 'Địa Chấn Âm Ty' (Ground Slam)!");
                groundSlamSkill.PerformGroundSlam();
            }
        }

        [ContextMenu("Debug/Force Phase 2")]
        public void ForcePhase2()
        {
            EnterPhase2();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 textPos = transform.position + Vector3.up * 2.2f;
            float remainingCd = Mathf.Max(0f, skillCooldown - _skillTimer);
            string debugText = $"👹 {bossName} [{_currentPhase}]\n" +
                               $"🔥 Hệ: {currentElement}\n" +
                               $"⏱️ Skill Cooldown: {remainingCd:F1}s";

            UnityEditor.Handles.Label(textPos, debugText);
        }
#endif
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player
{
    [RequireComponent(typeof(HealthSystem))]
    [RequireComponent(typeof(PlayerStats))]
    public class ViralBurstUltimate : MonoBehaviour
    {
        [Header("Input Action")]
        [SerializeField] private InputActionReference ultimateAction;
        
        [Header("Ultimate Settings")]
        [SerializeField] private float cooldownTime = 120f;
        [SerializeField] private float damageAmount = 500f; // Sát thương diện rộng

        private HealthSystem _healthSystem;
        private PlayerStats _playerStats;
        private float _lastUsedTime = -999f; // Sẵn sàng ngay từ đầu

        private static readonly Collider2D[] _hitBuffer = new Collider2D[100];
        private static int _enemyLayerMask = -1;

        // ====================================================================
        // PUBLIC API — Cho phép Upgrade System buff Skill tại runtime
        // ====================================================================

        /// <summary>Giảm cooldown (giây). Truyền vào số dương để giảm.</summary>
        public void ReduceCooldown(float amount)
        {
            cooldownTime = Mathf.Max(5f, cooldownTime - amount);
            Debug.Log($"[ViralBurst] Cooldown còn lại: {cooldownTime}s");
        }

        /// <summary>Tăng sát thương Ultimate.</summary>
        public void AddDamage(float amount)
        {
            damageAmount += amount;
            Debug.Log($"[ViralBurst] Sát thương mới: {damageAmount}");
        }

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _playerStats = GetComponent<PlayerStats>();
            if (_enemyLayerMask == -1)
            {
                _enemyLayerMask = LayerMask.GetMask("Enemy", "Enemies");
                if (_enemyLayerMask == 0) _enemyLayerMask = ~0; // Fallback toàn bộ layer nếu chưa set layer Enemy
            }
        }

        private void OnEnable()
        {
            if (ultimateAction != null)
            {
                ultimateAction.action.Enable();
                ultimateAction.action.performed += OnUltimatePerformed;
            }
        }

        private void OnDisable()
        {
            if (ultimateAction != null)
            {
                ultimateAction.action.Disable();
                ultimateAction.action.performed -= OnUltimatePerformed;
            }
        }

        private void OnUltimatePerformed(InputAction.CallbackContext context)
        {
            if (Time.time >= _lastUsedTime + cooldownTime)
            {
                ActivateUltimate();
                _lastUsedTime = Time.time;
            }
        }

        private void ActivateUltimate()
        {
            Debug.Log("[ViralBurst] Kích hoạt Ultimate: VIRAL BURST!");

            // 1. Gây sát thương diện rộng toàn màn hình (NonAlloc 0-GC)
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, 30f, _hitBuffer, _enemyLayerMask);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitBuffer[i];
                if (hit != null && hit.TryGetComponent<HealthSystem>(out var enemyHealth))
                {
                    enemyHealth.TakeDamage(damageAmount);
                }
            }

            // 2. Hồi 20% máu tối đa
            float healAmount = _playerStats.MaxHealth * 0.2f;
            _healthSystem.Heal(healAmount);
            Debug.Log($"[ViralBurst] Hồi {healAmount} máu.");

            // 3. Hút toàn bộ EXP (TODO)
            // TODO: Triển khai hệ thống ExperienceSystem và tìm các hạt EXP (EXP Orbs)
            // để kéo về phía Player. Sẽ làm ở một Epic sau.
        }
    }
}

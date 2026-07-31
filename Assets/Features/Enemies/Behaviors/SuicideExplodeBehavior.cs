using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies.Behaviors
{
    /// <summary>
    /// Behavior tự sát nổ AoE cho Hồ Ly Tinh Nhỏ (E_HOALYTINH) theo GDD 5.1.
    /// Khi áp sát người chơi trong khoảng cách 1.5m, kích hoạt phát nổ gây 50 Damage diện rộng.
    /// </summary>
    public class SuicideExplodeBehavior : MonoBehaviour
    {
        [Header("Explosion Settings")]
        [SerializeField] private float explodeRadius = 2.5f;
        [SerializeField] private float triggerDistance = 1.5f;
        [SerializeField] private float explodeDelay = 0.5f;
        [SerializeField] private float explosionDamage = 50f;
        [SerializeField] private LayerMask targetLayer;

        private Transform _playerTransform;
        private bool _isExploding = false;
        private HealthSystem _healthSystem;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            _isExploding = false;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (_isExploding || _playerTransform == null) return;

            float dist = Vector2.Distance(transform.position, _playerTransform.position);
            if (dist <= triggerDistance)
            {
                StartCoroutine(ExplodeRoutine());
            }
        }

        private System.Collections.IEnumerator ExplodeRoutine()
        {
            _isExploding = true;
            yield return new WaitForSeconds(explodeDelay);

            // Gây sát thương nổ AoE
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius, targetLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var playerHealth = hit.GetComponent<HealthSystem>();
                    if (playerHealth != null)
                    {
                        DamageData damageData = new DamageData(explosionDamage, false, ElementType.Hoa);
                        playerHealth.TakeDamage(damageData);
                    }
                }
            }

            Debug.Log($"[SuicideExplodeBehavior] Hồ Ly Tinh phát nổ AoE {explosionDamage} DMG tại {transform.position}");

            // Tự sát
            if (_healthSystem != null)
            {
                _healthSystem.TakeDamage(new DamageData(99999f));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explodeRadius);
        }
#endif
    }
}

using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Component hành vi va chạm: Gây sát thương cho 1 mục tiêu (hỗ trợ xuyên mục tiêu).
    /// </summary>
    [RequireComponent(typeof(ProjectileCore))]
    [RequireComponent(typeof(Collider2D))]
    public class Hit_SingleTarget : MonoBehaviour
    {
        [Header("Piercing Settings")]
        [SerializeField] private bool canPierce = false;

        private ProjectileCore _core;
        private HashSet<Collider2D> _hitEnemies = new HashSet<Collider2D>();

        private void Awake()
        {
            _core = GetComponent<ProjectileCore>();
        }

        private void OnEnable()
        {
            _hitEnemies.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                if (_hitEnemies.Contains(collision)) return; // Không gây sát thương 2 lần cho cùng 1 mục tiêu

                if (collision.TryGetComponent(out HealthSystem enemyHealth))
                {
                    enemyHealth.TakeDamage(_core.DamageData);
                    _hitEnemies.Add(collision);
                    
                    if (!canPierce)
                    {
                        _core.ReturnToPool();
                    }
                }
            }
        }
    }
}

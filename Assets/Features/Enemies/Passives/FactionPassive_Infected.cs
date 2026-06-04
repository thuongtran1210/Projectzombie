using UnityEngine;
using ProjectZombie.Core;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Enemies.Passives
{
    [RequireComponent(typeof(Enemy))]
    public class FactionPassive_Infected : MonoBehaviour
    {
        [Header("Infected Settings")]
        public float searchRadius = 3f;
        public int requiredAllies = 3;
        public float speedMultiplier = 1.5f;
        public LayerMask enemyLayer;

        private Enemy _enemy;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[20];

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            // Fallback if layer is not set in inspector
            if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("Enemy");
        }

        private void OnEnable()
        {
            TickManager.OnTick += ScanHorde;
        }

        private void OnDisable()
        {
            TickManager.OnTick -= ScanHorde;
        }

        private void ScanHorde()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, searchRadius, _hitBuffer, enemyLayer);
            int infectedCount = 0;

            for (int i = 0; i < count; i++)
            {
                var hit = _hitBuffer[i];
                if (hit.GetComponent<FactionPassive_Infected>() != null)
                {
                    infectedCount++;
                }
            }

            // Lưu ý: infectedCount đã bao gồm cả chính quái vật này
            if (infectedCount > requiredAllies)
            {
                _enemy.MoveSpeedMultiplier = speedMultiplier;
            }
            else
            {
                _enemy.MoveSpeedMultiplier = 1f;
            }
        }
    }
}

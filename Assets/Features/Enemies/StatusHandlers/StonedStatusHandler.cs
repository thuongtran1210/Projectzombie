using UnityEngine;

namespace ProjectZombie.Features.Enemies.StatusHandlers
{
    /// <summary>
    /// Strategy xử lý hiệu ứng Say Thuốc Lào (Stoned): Giảm tốc, khi hết thời gian nổ ho sặc sụa lan sát thương và đẩy lùi.
    /// </summary>
    public class StonedStatusHandler : IStatusEffectHandler
    {
        public StatusEffectType Type => StatusEffectType.Stoned;

        public void OnApplied(Enemy enemy, ActiveStatusEffect effectData) { }

        public void OnTick(Enemy enemy, ActiveStatusEffect effectData, float deltaTime) { }

        private static readonly Collider2D[] _nearbyBuffer = new Collider2D[24];

        public void OnExpired(Enemy enemy, ActiveStatusEffect effectData)
        {
            if (enemy == null) return;

            int count = Physics2D.OverlapCircleNonAlloc(enemy.transform.position, 2.5f, _nearbyBuffer, 1 << enemy.gameObject.layer);
            for (int i = 0; i < count; i++)
            {
                var col = _nearbyBuffer[i];
                if (col != null && col.gameObject != enemy.gameObject && col.TryGetComponent<Enemy>(out var otherEnemy))
                {
                    otherEnemy.HealthSystem?.TakeDamage(60f);
                    otherEnemy.ApplyKnockback((col.transform.position - enemy.transform.position).normalized, 5f, 0.25f);
                }
            }
        }

        public void OnRemoved(Enemy enemy, ActiveStatusEffect effectData) { }
    }
}

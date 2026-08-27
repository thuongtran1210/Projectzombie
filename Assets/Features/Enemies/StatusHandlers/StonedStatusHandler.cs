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

        public void OnExpired(Enemy enemy, ActiveStatusEffect effectData)
        {
            if (enemy == null) return;

            Collider2D[] nearby = Physics2D.OverlapCircleAll(enemy.transform.position, 2.5f, 1 << enemy.gameObject.layer);
            for (int i = 0; i < nearby.Length; i++)
            {
                if (nearby[i].TryGetComponent<Enemy>(out var otherEnemy))
                {
                    otherEnemy.HealthSystem?.TakeDamage(60f);
                    otherEnemy.ApplyKnockback((nearby[i].transform.position - enemy.transform.position).normalized, 5f, 0.25f);
                }
            }
        }

        public void OnRemoved(Enemy enemy, ActiveStatusEffect effectData) { }
    }
}

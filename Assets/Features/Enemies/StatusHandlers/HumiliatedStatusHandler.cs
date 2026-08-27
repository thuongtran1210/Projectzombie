using UnityEngine;

namespace ProjectZombie.Features.Enemies.StatusHandlers
{
    /// <summary>
    /// Strategy xử lý hiệu ứng Quê Độ (Humiliated): Có xác suất quay sang đấm quái đồng minh bên cạnh (Friendly Fire).
    /// </summary>
    public class HumiliatedStatusHandler : IStatusEffectHandler
    {
        public StatusEffectType Type => StatusEffectType.Humiliated;

        public void OnApplied(Enemy enemy, ActiveStatusEffect effectData) { }

        public void OnTick(Enemy enemy, ActiveStatusEffect effectData, float deltaTime)
        {
            if (Time.time >= effectData.NextTickTime)
            {
                effectData.NextTickTime = Time.time + effectData.TickInterval;
                TriggerFriendlyPunch(enemy);
            }
        }

        private void TriggerFriendlyPunch(Enemy enemy)
        {
            if (enemy == null) return;

            // Tìm 1 quái bạn gần nhất trong phạm vi 1.5m để đấm
            Collider2D[] allies = Physics2D.OverlapCircleAll(enemy.transform.position, 1.5f, 1 << enemy.gameObject.layer);
            for (int i = 0; i < allies.Length; i++)
            {
                if (allies[i].gameObject != enemy.gameObject && allies[i].TryGetComponent<Enemy>(out var allyEnemy))
                {
                    allyEnemy.HealthSystem?.TakeDamage(50f);
                    allyEnemy.ApplyKnockback((allies[i].transform.position - enemy.transform.position).normalized, 4f, 0.2f);
                    break;
                }
            }
        }

        public void OnExpired(Enemy enemy, ActiveStatusEffect effectData) { }

        public void OnRemoved(Enemy enemy, ActiveStatusEffect effectData) { }
    }
}

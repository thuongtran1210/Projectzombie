using UnityEngine;

namespace ProjectZombie.Features.Enemies.StatusHandlers
{
    /// <summary>
    /// Strategy xử lý hiệu ứng Thiêu Đốt (Burn): Gây sát thương theo chu kỳ (DoT).
    /// </summary>
    public class BurnStatusHandler : IStatusEffectHandler
    {
        public StatusEffectType Type => StatusEffectType.Burn;

        public void OnApplied(Enemy enemy, ActiveStatusEffect effectData) { }

        public void OnTick(Enemy enemy, ActiveStatusEffect effectData, float deltaTime)
        {
            if (Time.time >= effectData.NextTickTime)
            {
                effectData.NextTickTime = Time.time + effectData.TickInterval;
                effectData.OnTickDamage?.Invoke(effectData.Value);
            }
        }

        public void OnExpired(Enemy enemy, ActiveStatusEffect effectData) { }

        public void OnRemoved(Enemy enemy, ActiveStatusEffect effectData) { }
    }
}

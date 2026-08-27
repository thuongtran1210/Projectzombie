using UnityEngine;

namespace ProjectZombie.Features.Enemies.StatusHandlers
{
    /// <summary>
    /// Strategy xử lý hiệu ứng Ngủ Say (Sleeping): Đứng yên bất động, thức giấc khi nhận sát thương đầu tiên với hệ số 2.0x Damage.
    /// </summary>
    public class SleepingStatusHandler : IStatusEffectHandler
    {
        public StatusEffectType Type => StatusEffectType.Sleeping;

        public void OnApplied(Enemy enemy, ActiveStatusEffect effectData) { }

        public void OnTick(Enemy enemy, ActiveStatusEffect effectData, float deltaTime) { }

        public void OnExpired(Enemy enemy, ActiveStatusEffect effectData) { }

        public void OnRemoved(Enemy enemy, ActiveStatusEffect effectData) { }
    }
}

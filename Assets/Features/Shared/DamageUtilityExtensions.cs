using UnityEngine;
using ProjectZombie.Core.Events;

namespace ProjectZombie.Features.Shared
{
    public static partial class DamageUtilityExtensions
    {
        /// <summary>
        /// Phát tán sự kiện khi đòn đánh trúng mục tiêu, tự động kích hoạt VFX và Damage Text.
        /// </summary>
        public static void NotifyDamageDealt(Vector3 hitPosition, DamageData damageData, GameObject target)
        {
            GameEventBus.Publish(new DamageDealtEvent(hitPosition, damageData, target));
        }
    }
}

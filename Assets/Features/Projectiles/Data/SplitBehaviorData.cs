using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "SplitBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Split")]
    public class SplitBehaviorData : ProjectileBehaviorData
    {
        public ProjectileData ChildProjectileData;
        public int SplitCount = 3;
        public float SpreadAngle = 45f;
        
        [Tooltip("Có chia lượng damage gốc ra làm nhiều phần không?")]
        public bool DivideDamage = true;

        [Tooltip("Khi nào thì tách? (Despawn = Hết tầm bắn hoặc Va chạm nổ)")]
        public bool TriggerOnHit = true;
        public bool TriggerOnDespawn = false;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new SplitBehavior(controller, this);
        }
    }
}

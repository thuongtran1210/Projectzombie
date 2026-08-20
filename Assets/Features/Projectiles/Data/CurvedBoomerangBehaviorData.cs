using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "CurvedBoomerangBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/CurvedBoomerang")]
    public class CurvedBoomerangBehaviorData : ProjectileBehaviorData
    {
        [Tooltip("Tốc độ bẻ lái uốn cong hình lưỡi liềm (độ/giây)")]
        public float curveTurnRate = 240f;

        [Tooltip("Thời lượng bay ra trước khi bắt đầu quay về (giây)")]
        public float forwardDuration = 0.5f;

        [Tooltip("Tốc độ tự xoay quanh tâm Z của phi tiêu (độ/giây)")]
        public float spinSpeed = 1080f;

        [Tooltip("Tốc độ bẻ lái đuổi theo người chơi khi quay về (độ/giây)")]
        public float returnTurnRate = 420f;

        [Tooltip("Hệ số tăng tốc khi quay về người chơi")]
        public float returnSpeedMultiplier = 1.3f;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new CurvedBoomerangBehavior(controller, this);
        }
    }
}

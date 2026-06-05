using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "VampiricBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Vampiric")]
    public class VampiricBehaviorData : ProjectileBehaviorData
    {
        [Header("Vampiric Settings")]
        [Tooltip("Lượng máu hồi phục cho người chơi mỗi khi dơi cắn trúng kẻ địch")]
        public float HealAmount = 1f;
        
        [Tooltip("Tỉ lệ hút máu thành công (0.0 đến 1.0)")]
        [Range(0f, 1f)]
        public float VampiricChance = 1f;

        [Header("Homing Settings (Bầy dơi tự tìm mồi)")]
        [Tooltip("Tốc độ xoay (homing) để bay về phía mục tiêu")]
        public float RotationSpeed = 180f;
        
        [Tooltip("Phạm vi quét tìm mục tiêu")]
        public float SearchRadius = 10f;
        
        [Tooltip("Layer chứa quái vật")]
        public LayerMask EnemyLayer;

        public override Behaviors.IProjectileBehavior CreateBehavior(Components.ProjectileController controller)
        {
            return new Behaviors.VampiricBehavior(controller, this);
        }
    }
}

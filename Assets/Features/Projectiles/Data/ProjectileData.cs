using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "NewProjectileData", menuName = "ProjectZombie/Projectiles/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Identity")]
        public string ProjectileID;
        public GameObject LogicPrefab; // Prefab chứa ProjectileController và các logic, có thể chứa Visual bên trong.

        [Header("Movement")]
        public float Speed = 10f;
        public float MaxRange = 20f;
        public float Lifetime = 3f;

        [Header("Collision")]
        public float CollisionRadius = 0.5f;
        public LayerMask HitLayer;

        [Header("Pool Config")]
        public int PrewarmCount = 10;
        public int MaxPoolSize = 50;

        [Header("Behaviors")]
        public System.Collections.Generic.List<ProjectileBehaviorData> Behaviors = new System.Collections.Generic.List<ProjectileBehaviorData>();

        [Header("Damage")]
        public float BaseDamage;
        // Có thể mở rộng DamageData sau
    }
}

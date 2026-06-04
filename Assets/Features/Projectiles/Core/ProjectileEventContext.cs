using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Projectiles.Core
{
    public struct ProjectileEventContext
    {
        public ProjectileController Projectile;
        public ProjectileRuntimeState State;
        public DamageContext Damage;

        public Collider2D TargetCollider;
        public Vector2 HitPoint;
        public Vector2 HitNormal;

        public ProjectileEventContext(ProjectileController projectile, Collider2D targetCollider, Vector2 hitPoint, Vector2 hitNormal)
        {
            Projectile = projectile;
            State = projectile.State;
            Damage = projectile.Damage;
            TargetCollider = targetCollider;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }
}

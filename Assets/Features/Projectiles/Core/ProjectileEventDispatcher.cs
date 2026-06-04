using System;
using ProjectZombie.Features.Projectiles.Components;
using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Core
{
    public class ProjectileEventDispatcher
    {
        public event Action<ProjectileController> OnProjectileSpawned;
        public event Action<ProjectileEventContext> OnProjectileHit;
        public event Action<ProjectileController> OnProjectileExpired;
        public event Action<ProjectileController> OnProjectileDespawned;

        public void RaiseSpawned(ProjectileController projectile) => OnProjectileSpawned?.Invoke(projectile);
        public void RaiseHit(ProjectileEventContext context) => OnProjectileHit?.Invoke(context);
        public void RaiseExpired(ProjectileController projectile) => OnProjectileExpired?.Invoke(projectile);
        public void RaiseDespawned(ProjectileController projectile) => OnProjectileDespawned?.Invoke(projectile);
    }
}

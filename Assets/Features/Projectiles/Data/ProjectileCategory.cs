namespace ProjectZombie.Features.Projectiles.Data
{
    public enum ProjectileCategory
    {
        Transient = 0,      // Normal fired projectiles (Straight, Homing, etc.)
        Orbit = 1,          // Orbiting projectiles around player
        PersistentAura = 2  // Persistent aura / stationary area projectiles
    }
}

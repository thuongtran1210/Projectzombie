using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Interface đại diện cho bất kỳ đối tượng nào có thể nhận sát thương (Player, Enemy, Breakable Obstacles, Boss).
    /// Tuân thủ Dependency Inversion Principle (DIP) và Rule 3.4 trong AGENTS.md.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
        void TakeDamage(DamageData damageData);
        void TakeDamage(DamageContext context);
    }
}

using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Interface cho các đối tượng có thể hồi máu (Player, Pet, Ally).
    /// </summary>
    public interface IHealable
    {
        void Heal(float amount, bool allowRevive = false);
    }
}

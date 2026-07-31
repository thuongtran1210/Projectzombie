using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho Kỹ năng Chủ động (Signature Skill).
    /// Quản lý cooldown nền và điều kiện thi triển cơ bản.
    /// </summary>
    public abstract class SignatureSkillBase : ISignatureSkill
    {
        public abstract float Cooldown { get; }

        public virtual bool CanExecute(PlayerStats stats, HealthSystem health)
        {
            return stats != null && health != null && health.IsAlive;
        }

        public abstract void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null);

        public virtual void Tick(float deltaTime)
        {
        }
    }
}

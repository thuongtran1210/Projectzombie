using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Skills;

namespace ProjectZombie.Features.Multiplayer.Core.Commands
{
    /// <summary>
    /// Lệnh thực hiện Kỹ năng Bản mệnh (Signature Skill Action).
    /// </summary>
    public class SignatureSkillActionCommand : INetworkActionCommand
    {
        public void Execute(GameObject playerRoot, Vector2 aimDirection)
        {
            if (aimDirection.sqrMagnitude > 0.001f)
            {
                var anim = playerRoot.GetComponentInChildren<PlayerAnimator>();
                if (anim != null) anim.FlipToDirection(aimDirection.x);
            }

            if (playerRoot.TryGetComponent<SignatureSkillManager>(out var sig))
            {
                sig.TryExecuteSkill();
            }
        }
    }
}

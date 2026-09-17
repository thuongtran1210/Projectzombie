using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Multiplayer.Core.Commands
{
    /// <summary>
    /// Lệnh thực hiện Đòn đánh thường (Primary Attack Action).
    /// </summary>
    public class AttackActionCommand : INetworkActionCommand
    {
        public void Execute(GameObject playerRoot, Vector2 aimDirection)
        {
            if (playerRoot.TryGetComponent<CharacterCombat>(out var combat))
            {
                if (aimDirection.sqrMagnitude > 0.001f)
                {
                    combat.TriggerAttack(aimDirection);
                }
                else
                {
                    combat.TriggerAttack();
                }
            }
            else if (playerRoot.TryGetComponent<WeaponManager>(out var wm))
            {
                wm.TriggerPrimaryAttack();
            }
        }
    }
}

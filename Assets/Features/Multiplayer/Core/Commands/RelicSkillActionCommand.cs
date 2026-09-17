using UnityEngine;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Multiplayer.Core.Commands
{
    /// <summary>
    /// Lệnh kích hoạt Kỹ năng Cổ vật / Pháp bảo (Relic Skill Action).
    /// </summary>
    public class RelicSkillActionCommand : INetworkActionCommand
    {
        public void Execute(GameObject playerRoot, Vector2 aimDirection)
        {
            if (playerRoot.TryGetComponent<WeaponManager>(out var wm))
            {
                if (aimDirection.sqrMagnitude > 0.001f)
                {
                    wm.TriggerEquippedRelicSkill(aimDirection);
                }
                else
                {
                    wm.TriggerEquippedRelicSkill();
                }
            }
        }
    }
}

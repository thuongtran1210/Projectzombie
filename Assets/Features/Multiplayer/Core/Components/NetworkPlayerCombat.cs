using UnityEngine;
using ProjectZombie.Features.Multiplayer.Core.Commands;

namespace ProjectZombie.Features.Multiplayer.Core.Components
{
    /// <summary>
    /// Component chuyên trách thực thi các hành động chiến đấu mạng (Dash, Attack, Skills) qua Command Pattern.
    /// Đảm bảo 0 GC Allocations trong Gameplay Loop (Mục 6.1 AGENTS.md).
    /// </summary>
    public class NetworkPlayerCombat : MonoBehaviour
    {
        private NetworkInputButtons _previousButtons = NetworkInputButtons.None;

        private static readonly System.Collections.Generic.KeyValuePair<NetworkInputButtons, INetworkActionCommand>[] _actionCommands =
        {
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, INetworkActionCommand>(NetworkInputButtons.Dash, new DashActionCommand()),
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, INetworkActionCommand>(NetworkInputButtons.Attack, new AttackActionCommand()),
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, INetworkActionCommand>(NetworkInputButtons.SignatureSkill, new SignatureSkillActionCommand()),
            new System.Collections.Generic.KeyValuePair<NetworkInputButtons, INetworkActionCommand>(NetworkInputButtons.RelicSkill, new RelicSkillActionCommand()),
        };

        public void ProcessNetworkActions(NetworkInputButtons currentButtons, Vector2 aimDirection)
        {
            for (int i = 0; i < _actionCommands.Length; i++)
            {
                var kvp = _actionCommands[i];
                if ((currentButtons & kvp.Key) != 0 && (_previousButtons & kvp.Key) == 0)
                {
                    kvp.Value.Execute(gameObject, aimDirection);
                }
            }
            _previousButtons = currentButtons;
        }

        public void ResetButtons()
        {
            _previousButtons = NetworkInputButtons.None;
        }
    }
}

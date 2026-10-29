using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Multiplayer.Core.Components
{
    /// <summary>
    /// Component chuyên trách đồng bộ vũ khí và pháp bảo của nhân vật qua mạng.
    /// Tuân thủ Single Responsibility Principle (Mục 3.1 AGENTS.md).
    /// </summary>
    public class NetworkPlayerLoadout : MonoBehaviour
    {
        public string GetLocalEquippedRelicId()
        {
            if (RunLoadoutState.SelectedRelic != null && !string.IsNullOrEmpty(RunLoadoutState.SelectedRelic.weaponId))
            {
                return RunLoadoutState.SelectedRelic.weaponId;
            }
            return string.Empty;
        }

        public void SyncRemoteWeapon(string weaponId)
        {
            // Trong chế độ Defense3D: Đồng bộ hóa qua DefenseLoadoutIdMapper
        }
    }
}

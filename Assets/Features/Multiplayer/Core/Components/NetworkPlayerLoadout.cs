using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Multiplayer.Core.Components
{
    /// <summary>
    /// Component chuyên trách đồng bộ vũ khí và pháp bảo của nhân vật qua mạng.
    /// Tuân thủ Single Responsibility Principle (Mục 3.1 AGENTS.md).
    /// </summary>
    public class NetworkPlayerLoadout : MonoBehaviour
    {
        private WeaponManager _weaponManager;

        private void Awake()
        {
            _weaponManager = GetComponent<WeaponManager>();
        }

        public string GetLocalEquippedRelicId()
        {
            if (RunLoadoutState.SelectedRelic != null && !string.IsNullOrEmpty(RunLoadoutState.SelectedRelic.weaponId))
            {
                return RunLoadoutState.SelectedRelic.weaponId;
            }
            if (_weaponManager != null && _weaponManager.EquippedRelic != null)
            {
                return _weaponManager.EquippedRelic.weaponId;
            }
            return string.Empty;
        }

        public void SyncRemoteWeapon(string weaponId)
        {
            if (_weaponManager == null || string.IsNullOrEmpty(weaponId)) return;

            var equipped = _weaponManager.EquippedRelic;
            if (equipped == null || equipped.weaponId != weaponId)
            {
                _weaponManager.EquipWeaponById(weaponId, isPrimary: false);
            }
        }
    }
}

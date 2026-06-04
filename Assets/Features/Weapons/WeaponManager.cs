using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    using ProjectZombie.Features.Upgrades;

    /// <summary>
    /// Ba lô chứa vũ khí của Player. Quản lý việc gọi Tick() cho tất cả vũ khí đang sở hữu.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class WeaponManager : MonoBehaviour
    {
        private PlayerStats _playerStats;
        private PlayerPassives _playerPassives;
        private List<WeaponBase> _activeWeapons = new List<WeaponBase>();

        public IReadOnlyList<WeaponBase> ActiveWeapons => _activeWeapons;

        public event System.Action OnWeaponsChanged;

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            _playerPassives = GetComponent<PlayerPassives>();
        }

        private void Start()
        {
            // Tự động tìm tất cả các vũ khí được gắn vào nhân vật (ở các object con)
            WeaponBase[] weapons = GetComponentsInChildren<WeaponBase>();
            foreach (var w in weapons)
            {
                AddWeapon(w);
            }
        }

        public void AddWeapon(WeaponBase weapon)
        {
            if (!_activeWeapons.Contains(weapon))
            {
                weapon.Initialize(_playerStats);
                _activeWeapons.Add(weapon);
                OnWeaponsChanged?.Invoke();
            }
        }

        public void NotifyWeaponsChanged()
        {
            OnWeaponsChanged?.Invoke();
        }

        public WeaponBase GetWeaponById(string id)
        {
            return _activeWeapons.Find(w => w.weaponId == id);
        }

        public void RemoveWeapon(WeaponBase weapon)
        {
            if (_activeWeapons.Contains(weapon))
            {
                _activeWeapons.Remove(weapon);
                Destroy(weapon.gameObject);
                NotifyWeaponsChanged();
            }
        }

        private void Update()
        {
            // Cho phép tất cả vũ khí hoạt động
            foreach (var weapon in _activeWeapons)
            {
                weapon.Tick();
            }
        }
    }
}

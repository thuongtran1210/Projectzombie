using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player.Mechanics;
using ProjectZombie.Features.Player.Skills;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Data Transfer Object (DTO) chứa tất cả tham chiếu cốt lõi của Player sau khi spawn.
    /// Dùng cho việc Dependency Injection sạch sẽ vào các hệ thống UI, Camera, Buffs,...
    /// </summary>
    public class PlayerContext
    {
        public GameObject GameObject { get; }
        public Transform Transform { get; }
        public PlayerStats Stats { get; }
        public HealthSystem Health { get; }
        public PlayerExperience Experience { get; }
        public WeaponManager WeaponManager { get; }
        public PlayerPassives Passives { get; }
        public ICharacterGaugeProvider GaugeProvider { get; }
        public SignatureSkillManager SignatureSkillManager { get; }

        public PlayerContext(GameObject playerObject)
        {
            GameObject = playerObject;
            Transform = playerObject != null ? playerObject.transform : null;

            if (playerObject != null)
            {
                Stats = playerObject.GetComponent<PlayerStats>();
                Health = playerObject.GetComponent<HealthSystem>();
                Experience = playerObject.GetComponent<PlayerExperience>();
                WeaponManager = playerObject.GetComponent<WeaponManager>();
                Passives = playerObject.GetComponent<PlayerPassives>();
                GaugeProvider = playerObject.GetComponent<ICharacterGaugeProvider>();
                SignatureSkillManager = playerObject.GetComponent<SignatureSkillManager>();

                ValidateComponents();
            }
        }

        public static PlayerContext Create(GameObject playerObject)
        {
            return new PlayerContext(playerObject);
        }

        private void ValidateComponents()
        {
            if (Stats == null) Debug.LogWarning($"[PlayerContext] {GameObject.name} thiếu PlayerStats!");
            if (Health == null) Debug.LogWarning($"[PlayerContext] {GameObject.name} thiếu HealthSystem!");
            if (Experience == null) Debug.LogWarning($"[PlayerContext] {GameObject.name} thiếu PlayerExperience!");
            if (WeaponManager == null) Debug.LogWarning($"[PlayerContext] {GameObject.name} thiếu WeaponManager!");
        }
    }
}

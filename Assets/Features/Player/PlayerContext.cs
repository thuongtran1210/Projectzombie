using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player.Input;

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
        public PlayerController Controller { get; }
        public PlayerInputReader InputReader { get; }
        public ProjectZombie.Features.Combat.PlayerCombatEvents CombatEvents { get; }

        public bool IsLocal { get; set; } = true;
        public int PlayerId { get; set; } = 0;
        public bool IsAlive => Health != null && Health.CurrentHealth > 0;

        public PlayerContext(GameObject playerObject, bool isLocal = true, int playerId = 0)
        {
            GameObject = playerObject;
            Transform = playerObject != null ? playerObject.transform : null;
            IsLocal = isLocal;
            PlayerId = playerId;

            if (playerObject != null)
            {
                Stats = playerObject.GetComponent<PlayerStats>();
                Health = playerObject.GetComponent<HealthSystem>();
                Controller = playerObject.GetComponent<PlayerController>();
                InputReader = playerObject.GetComponent<PlayerInputReader>();
                CombatEvents = playerObject.GetComponent<ProjectZombie.Features.Combat.PlayerCombatEvents>();
                if (CombatEvents == null)
                {
                    CombatEvents = playerObject.AddComponent<ProjectZombie.Features.Combat.PlayerCombatEvents>();
                }

                ValidateComponents();
            }
        }

        public static PlayerContext Create(GameObject playerObject, bool isLocal = true, int playerId = 0)
        {
            return new PlayerContext(playerObject, isLocal, playerId);
        }

        private void ValidateComponents()
        {
            if (Stats == null) Debug.LogWarning($"[PlayerContext] {GameObject.name} thiếu PlayerStats!");
            if (Health == null) Debug.LogWarning($"[PlayerContext] {GameObject.name} thiếu HealthSystem!");
        }
    }
}

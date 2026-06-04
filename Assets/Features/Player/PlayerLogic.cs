using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player
{
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerLogic : MonoBehaviour
    {
        private HealthSystem _healthSystem;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _healthSystem.OnDied += HandlePlayerDeath;
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("[PlayerLogic] Player has died! Game Over.");
            // We can trigger Game Over UI or restart scene here
            // For now we just disable the player controller so they can't move
            var controller = GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDied -= HandlePlayerDeath;
            }
        }
    }
}

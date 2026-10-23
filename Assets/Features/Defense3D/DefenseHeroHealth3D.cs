using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Prototype hero down/respawn. Hero death never ends the Defense run.</summary>
    public sealed class DefenseHeroHealth3D : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHP = 200f;
        [SerializeField, Min(0.1f)] private float respawnDelay = 5f;
        private Vector3 originalSpawn;
        private float respawnAt;
        private bool down;
        public float CurrentHP { get; private set; }
        public bool IsDown => down;

        private void Awake()
        {
            originalSpawn = transform.position;
            CurrentHP = maxHP;
        }

        public void ReceiveDamage(float amount)
        {
            if (down || amount <= 0f) return;
            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            if (CurrentHP > 0f) return;
            down = true;
            respawnAt = Time.time + respawnDelay;
            var motor = GetComponent<DefenseHeroMotor3D>();
            if (motor != null) motor.enabled = false;
            Debug.Log("[TREBRAVE Defense3D] Hero downed; respawning. Base remains the only loss condition.");
        }

        private void Update()
        {
            if (!down || Time.time < respawnAt) return;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.position = originalSpawn;
            }
            else transform.position = originalSpawn;
            CurrentHP = maxHP;
            down = false;
            var motor = GetComponent<DefenseHeroMotor3D>();
            if (motor != null) motor.enabled = true;
            Debug.Log("[TREBRAVE Defense3D] Hero respawned.");
        }
    }
}

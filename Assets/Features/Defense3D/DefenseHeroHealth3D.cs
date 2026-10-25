using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Hero down/respawn according to D-04/A.
    /// Hero death never ends the Defense run (only BaseHP=0 causes LOSS).
    /// Respawn delay is 5 gameplay seconds and freezes during Pause/LevelUp.
    /// </summary>
    public sealed class DefenseHeroHealth3D : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHP = 200f;
        [SerializeField, Min(0.1f)] private float respawnDelay = 5f;

        private Vector3 originalSpawn;
        private DefenseHeroRespawnTracker tracker;
        private DefenseHeroMotor3D motor;
        private Rigidbody rb;

        public float CurrentHP => tracker != null ? tracker.CurrentHp : maxHP;
        public bool IsDown => tracker != null && tracker.IsDown;
        public float RemainingRespawnTime => tracker != null ? tracker.RemainingRespawnTime : 0f;

        private void Awake()
        {
            originalSpawn = transform.position;
            motor = GetComponent<DefenseHeroMotor3D>();
            rb = GetComponent<Rigidbody>();
            InitTracker();
        }

        private void InitTracker()
        {
            tracker = new DefenseHeroRespawnTracker(maxHP, respawnDelay);
            tracker.Downed += OnHeroDowned;
            tracker.Respawned += OnHeroRespawned;
        }

        public void ReceiveDamage(float amount)
        {
            if (tracker == null) InitTracker();
            tracker.TakeDamage(amount);
        }

        /// <summary>
        /// Advance respawn timer using authoritative gameplay delta seconds.
        /// </summary>
        public void AdvanceGameplayTime(float gameplayDeltaSeconds, bool isTerminal)
        {
            if (tracker == null) return;
            tracker.Tick(gameplayDeltaSeconds, isTerminal);
        }

        public void ResetHero()
        {
            if (tracker != null) tracker.Reset();
            RespawnAtOrigin();
        }

        private void OnHeroDowned()
        {
            if (motor != null) motor.enabled = false;
            Debug.Log("[TREBRAVE Defense3D] Hero downed; 5s gameplay respawn countdown started. Base remains the only loss condition.");
        }

        private void OnHeroRespawned()
        {
            RespawnAtOrigin();
            Debug.Log("[TREBRAVE Defense3D] Hero respawned at origin.");
        }

        private void RespawnAtOrigin()
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.position = originalSpawn;
            }
            else
            {
                transform.position = originalSpawn;
            }
            if (motor != null) motor.enabled = true;
        }
    }
}


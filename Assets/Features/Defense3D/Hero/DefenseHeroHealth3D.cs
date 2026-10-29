using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Hero damage capability; Downed starts a gameplay-time respawn and never ends the match.</summary>
    public sealed class DefenseHeroHealth3D : DefenseGameplayBehaviour, IDefenseDamageable3D
    {
        [SerializeField, Min(1f)] private float maxHP = 200f;
        [SerializeField, Min(0.1f)] private float respawnDelay = 5f;
        private Vector3 originalSpawn;
        private DefenseHeroRespawnTracker tracker;
        private DefenseHeroMotor3D motor;
        private Rigidbody rb;
        public float CurrentHP => Tracker.CurrentHp;
        public float MaxHP => Tracker.MaxHp;
        public bool IsDown => Tracker.IsDown;
        public bool IsDead => IsDown;
        public float RemainingRespawnTime => Tracker.RemainingRespawnTime;
        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        public event Action Defeated;

        private DefenseHeroRespawnTracker Tracker
        {
            get
            {
                if (tracker != null) return tracker;
                tracker = new DefenseHeroRespawnTracker(maxHP, respawnDelay);
                tracker.Downed += OnHeroDowned;
                tracker.Respawned += RespawnAtOrigin;
                tracker.HealthChanged += (current, maximum) => HealthChanged?.Invoke(current, maximum);
                tracker.Damaged += context => Damaged?.Invoke(context);
                return tracker;
            }
        }

        private void Awake()
        {
            originalSpawn = transform.position;
            motor = GetComponent<DefenseHeroMotor3D>();
            rb = GetComponent<Rigidbody>();
            _ = Tracker;
        }

        public bool TakeDamage(DefenseDamageContext context)
        {
            return isActiveAndEnabled && DefenseDamageRules.CanApply(context, Gameplay, IsDead) && Tracker.ApplyDamage(context);
        }

        public void ReceiveDamage(float amount) => TakeDamage(new DefenseDamageContext(amount));
        public bool Heal(float amount) => isActiveAndEnabled && CanAdvanceGameplay && !IsDown && Tracker.Heal(amount);
        public void AdvanceGameplayTime(float gameplayDeltaSeconds, bool isTerminal) => Tracker.Tick(gameplayDeltaSeconds, isTerminal);

        public void ResetHero()
        {
            Tracker.Reset();
            RespawnAtOrigin();
        }

        public void SetMaxHealth(float newMaxHp)
        {
            if (newMaxHp <= 0f) return;
            maxHP = newMaxHp;
            Tracker.SetMaxHp(newMaxHp);
        }

        private void OnHeroDowned()
        {
            if (motor != null) motor.enabled = false;
            Defeated?.Invoke();
        }

        private void RespawnAtOrigin()
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.position = originalSpawn;
            }
            else transform.position = originalSpawn;
            if (motor != null) motor.enabled = true;
        }
    }
}


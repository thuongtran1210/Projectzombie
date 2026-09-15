using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Combat.Events
{
    /// <summary>
    /// Hợp đồng sự kiện gây sát thương (Player gây dame lên quái, hoặc quái đánh Player).
    /// Đóng gói dưới dạng readonly struct truyền qua 'in' để đạt 0 GC Allocations.
    /// </summary>
    public readonly struct DamageDealtEvent
    {
        public readonly GameObject Attacker;
        public readonly GameObject Target;
        public readonly float Damage;
        public readonly Vector2 HitPosition;
        public readonly Vector2 HitDirection;
        public readonly bool IsCrit;
        public readonly ElementType Element;
        public readonly string SourceWeaponId;

        public DamageDealtEvent(
            GameObject attacker,
            GameObject target,
            float damage,
            Vector2 hitPosition,
            Vector2 hitDirection = default,
            bool isCrit = false,
            ElementType element = ElementType.None,
            string sourceWeaponId = null)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
            HitPosition = hitPosition;
            HitDirection = hitDirection;
            IsCrit = isCrit;
            Element = element;
            SourceWeaponId = sourceWeaponId;
        }
    }

    /// <summary>
    /// Hợp đồng sự kiện tiêu diệt kẻ địch.
    /// </summary>
    public readonly struct KillEvent
    {
        public readonly GameObject Killer;
        public readonly GameObject Enemy;
        public readonly Vector2 Position;
        public readonly bool IsCrit;
        public readonly ElementType Element;
        public readonly float MaxHealthOfEnemy;

        public KillEvent(
            GameObject killer,
            GameObject enemy,
            Vector2 position,
            bool isCrit = false,
            ElementType element = ElementType.None,
            float maxHealthOfEnemy = 0f)
        {
            Killer = killer;
            Enemy = enemy;
            Position = position;
            IsCrit = isCrit;
            Element = element;
            MaxHealthOfEnemy = maxHealthOfEnemy;
        }
    }

    /// <summary>
    /// Hợp đồng sự kiện Lướt (Dash) & Thần Pháp Di Chuyển.
    /// </summary>
    public readonly struct DashEvent
    {
        public readonly GameObject Instigator;
        public readonly Vector2 StartPosition;
        public readonly Vector2 EndPosition;
        public readonly Vector2 DashDirection;
        public readonly float DashDistance;
        public readonly float Duration;

        public DashEvent(
            GameObject instigator,
            Vector2 startPos,
            Vector2 endPos,
            Vector2 direction,
            float distance,
            float duration)
        {
            Instigator = instigator;
            StartPosition = startPos;
            EndPosition = endPos;
            DashDirection = direction;
            DashDistance = distance;
            Duration = duration;
        }
    }

    /// <summary>
    /// Hợp đồng sự kiện Hồi Máu / Hút Máu.
    /// </summary>
    public readonly struct HealEvent
    {
        public readonly GameObject Target;
        public readonly float HealAmount;
        public readonly float CurrentHealth;
        public readonly float MaxHealth;
        public readonly bool IsOverheal;

        public HealEvent(
            GameObject target,
            float healAmount,
            float currentHealth,
            float maxHealth,
            bool isOverheal = false)
        {
            Target = target;
            HealAmount = healAmount;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            IsOverheal = isOverheal;
        }
    }

    /// <summary>
    /// Hợp đồng sự kiện Hồi Sinh (Revive / Tái Sinh / Miễn Tử).
    /// </summary>
    public readonly struct ReviveEvent
    {
        public readonly GameObject Player;
        public readonly float HealthPercent;
        public readonly float InvulnerableDuration;
        public readonly string Source;

        public ReviveEvent(
            GameObject player,
            float healthPercent,
            float invulnerableDuration,
            string source = "Default")
        {
            Player = player;
            HealthPercent = healthPercent;
            InvulnerableDuration = invulnerableDuration;
            Source = source;
        }
    }
}

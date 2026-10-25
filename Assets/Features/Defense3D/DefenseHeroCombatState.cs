using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseHeroArchetype
    {
        ThuSinh,    // MVP Hero
        KiemKhach,
        DaoSi,
        XaThu
    }

    /// <summary>
    /// U3D-017: Hero combat attributes and state controller.
    /// Invariants:
    /// - Manages Dash cooldown and basic attack cooldown via gameplay delta time.
    /// - Controls locked during Modal (Paused/LevelUp) and when Down.
    /// </summary>
    public sealed class DefenseHeroCombatState
    {
        public DefenseHeroArchetype Archetype { get; }
        public float AttackRange { get; }
        public float BaseDamage { get; }
        public float AttackCooldown { get; }
        public float DashCooldown { get; }
        public float DashDuration { get; }

        public float AttackCooldownRemaining { get; private set; }
        public float DashCooldownRemaining { get; private set; }
        public bool IsDashing => dashTimeRemaining > 0f;
        private float dashTimeRemaining;

        public DefenseHeroCombatState(DefenseHeroArchetype archetype = DefenseHeroArchetype.ThuSinh)
        {
            Archetype = archetype;
            // Thu Sinh MVP benchmark stats
            AttackRange = 5.5f;
            BaseDamage = 40f;
            AttackCooldown = 0.60f;
            DashCooldown = 3.0f;
            DashDuration = 0.25f;

            AttackCooldownRemaining = 0f;
            DashCooldownRemaining = 0f;
            dashTimeRemaining = 0f;
        }

        public void Tick(float gameplayDt)
        {
            if (gameplayDt <= 0f) return;

            if (AttackCooldownRemaining > 0f)
                AttackCooldownRemaining = Math.Max(0f, AttackCooldownRemaining - gameplayDt);

            if (DashCooldownRemaining > 0f)
                DashCooldownRemaining = Math.Max(0f, DashCooldownRemaining - gameplayDt);

            if (dashTimeRemaining > 0f)
                dashTimeRemaining = Math.Max(0f, dashTimeRemaining - gameplayDt);
        }

        public bool CanAttack(bool isDown, bool isModal)
        {
            return !isDown && !isModal && AttackCooldownRemaining <= 0f;
        }

        public bool TryTriggerAttack(bool isDown, bool isModal)
        {
            if (!CanAttack(isDown, isModal)) return false;
            AttackCooldownRemaining = AttackCooldown;
            return true;
        }

        public bool CanDash(bool isDown, bool isModal)
        {
            return !isDown && !isModal && DashCooldownRemaining <= 0f && !IsDashing;
        }

        public bool TryTriggerDash(bool isDown, bool isModal)
        {
            if (!CanDash(isDown, isModal)) return false;
            DashCooldownRemaining = DashCooldown;
            dashTimeRemaining = DashDuration;
            return true;
        }

        public void Reset()
        {
            AttackCooldownRemaining = 0f;
            DashCooldownRemaining = 0f;
            dashTimeRemaining = 0f;
        }
    }
}

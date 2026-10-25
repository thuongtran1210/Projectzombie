using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseRelicState
    {
        Ready,
        Aiming,
        Active,
        OnCooldown
    }

    /// <summary>
    /// U3D-018: "Dep To Ong" (Honeycomb Slipper) MVP Pháp Bảo.
    /// Invariants:
    /// - 1 slot per Hero.
    /// - Aim -> Cast -> Damage -> Cooldown.
    /// - Cancelling aim does NOT consume cooldown.
    /// - Modal / Down freezes or cancels aim.
    /// </summary>
    public sealed class DefenseDepToOngRelic
    {
        public const string RelicId = "dep_to_ong";
        public const string DisplayName = "Dép Tổ Ong";
        public const float CooldownDuration = 12f;
        public const float CastDamage = 180f;
        public const float ImpactRadius = 3.5f;
        public const float MaxRange = 10f;

        public DefenseRelicState State { get; private set; }
        public float CooldownRemaining { get; private set; }
        public Vector3 CurrentAimTarget { get; private set; }

        public event Action<Vector3> CastTriggered;
        public event Action CooldownFinished;

        public DefenseDepToOngRelic()
        {
            State = DefenseRelicState.Ready;
            CooldownRemaining = 0f;
        }

        public bool TryStartAim(bool isDown, bool isModal)
        {
            if (isDown || isModal || State != DefenseRelicState.Ready) return false;
            State = DefenseRelicState.Aiming;
            return true;
        }

        public void UpdateAimPosition(Vector3 targetPoint)
        {
            if (State != DefenseRelicState.Aiming) return;
            CurrentAimTarget = targetPoint;
        }

        public void CancelAim()
        {
            if (State == DefenseRelicState.Aiming)
            {
                State = DefenseRelicState.Ready;
            }
        }

        public bool TryCast(bool isDown, bool isModal, Vector3 targetPoint)
        {
            if (isDown || isModal || State != DefenseRelicState.Aiming) return false;

            State = DefenseRelicState.OnCooldown;
            CooldownRemaining = CooldownDuration;
            CastTriggered?.Invoke(targetPoint);
            return true;
        }

        public void Tick(float gameplayDt, bool isDown, bool isModal)
        {
            if (isDown || isModal)
            {
                if (State == DefenseRelicState.Aiming) CancelAim();
                return;
            }

            if (State == DefenseRelicState.OnCooldown && gameplayDt > 0f)
            {
                CooldownRemaining = Math.Max(0f, CooldownRemaining - gameplayDt);
                if (CooldownRemaining <= 0f)
                {
                    State = DefenseRelicState.Ready;
                    CooldownFinished?.Invoke();
                }
            }
        }

        public void Reset()
        {
            State = DefenseRelicState.Ready;
            CooldownRemaining = 0f;
        }
    }
}

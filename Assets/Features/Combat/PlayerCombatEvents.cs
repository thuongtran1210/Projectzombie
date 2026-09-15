using System;
using UnityEngine;
using ProjectZombie.Features.Combat.Events;

namespace ProjectZombie.Features.Combat
{
    /// <summary>
    /// Trung tâm phát tín hiệu chiến đấu của Player (Decoupled Combat Event Hub).
    /// Tuân thủ nguyên tắc 0 GC Allocations thông qua việc truyền readonly struct qua tham chiếu 'in'.
    /// </summary>
    public class PlayerCombatEvents : MonoBehaviour
    {
        public delegate void DamageDealtHandler(in DamageDealtEvent e);
        public delegate void KillEventHandler(in KillEvent e);
        public delegate void DashEventHandler(in DashEvent e);
        public delegate void HealEventHandler(in HealEvent e);
        public delegate void ReviveEventHandler(in ReviveEvent e);

        public event DamageDealtHandler OnDamageDealt;
        public event KillEventHandler OnEnemyKilled;
        public event DashEventHandler OnDashPerformed;
        public event HealEventHandler OnHealReceived;
        public event ReviveEventHandler OnPlayerRevived;

        public void PublishDamageDealt(in DamageDealtEvent e) => OnDamageDealt?.Invoke(e);
        public void PublishEnemyKilled(in KillEvent e) => OnEnemyKilled?.Invoke(e);
        public void PublishDashPerformed(in DashEvent e) => OnDashPerformed?.Invoke(e);
        public void PublishHealReceived(in HealEvent e) => OnHealReceived?.Invoke(e);
        public void PublishPlayerRevived(in ReviveEvent e) => OnPlayerRevived?.Invoke(e);
    }
}

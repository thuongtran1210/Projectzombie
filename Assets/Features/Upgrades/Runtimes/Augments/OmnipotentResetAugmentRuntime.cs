using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi Kim Cương [Thần Hành Tối Thượng] (AUG_PRIS_OMNIPOTENT_RESET):
    /// Mỗi khi tích lũy đủ 10 mạng hạ gục, lập tức hồi toàn bộ hồi chiêu Dash và tăng 40% Tốc chạy trong 2.5s.
    /// </summary>
    public class OmnipotentResetAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private int _killsRequired = 10;
        [SerializeField] private float _speedBonus = 0.40f;
        [SerializeField] private float _speedDuration = 2.5f;

        private int _killCounter = 0;
        private float _speedBuffEndTime = 0f;
        private bool _isSpeedBuffActive = false;

        protected override void SubscribeCombatEvents(PlayerCombatEvents events)
        {
            events.OnEnemyKilled += HandleEnemyKilled;
        }

        protected override void UnsubscribeCombatEvents(PlayerCombatEvents events)
        {
            events.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(in KillEvent e)
        {
            _killCounter++;
            if (_killCounter >= _killsRequired)
            {
                _killCounter = 0;
                TriggerOmnipotentReset();
            }
        }

        private void TriggerOmnipotentReset()
        {
            _speedBuffEndTime = Time.time + _speedDuration;

            if (!_isSpeedBuffActive && Context?.Stats != null)
            {
                _isSpeedBuffActive = true;
                Context.Stats.AddMoveSpeed(_speedBonus);
            }

            Debug.Log("<color=#00E5FF>[Thần Hành Tối Thượng] Reset Thần Pháp Di Chuyển & Tăng tốc bứt phá!</color>");
        }

        private void Update()
        {
            if (_isSpeedBuffActive && Time.time >= _speedBuffEndTime)
            {
                _isSpeedBuffActive = false;
                if (Context?.Stats != null)
                {
                    Context.Stats.AddMoveSpeed(-_speedBonus);
                }
            }
        }

        protected override void OnAugmentTeardown()
        {
            if (_isSpeedBuffActive && Context?.Stats != null)
            {
                _isSpeedBuffActive = false;
                Context.Stats.AddMoveSpeed(-_speedBonus);
            }
        }
    }
}

using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi [Huyết Thần Cuồng Bạo] (AUG_GOLD_VAMPIRIC_FRENZY):
    /// Khi hạ gục quái vật, hồi 1% Máu và kích hoạt buff cuồng nộ tăng 25% tốc độ tấn công trong 3s.
    /// </summary>
    public class VampiricFrenzyAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _healPercentPerKill = 0.01f;
        [SerializeField] private float _frenzyAttackSpeedBonus = 0.25f;
        [SerializeField] private float _frenzyDuration = 3.0f;

        private float _frenzyEndTime = 0f;
        private bool _isFrenzyActive = false;

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
            if (Context?.Health != null)
            {
                float healAmount = Context.Health.MaxHealth * _healPercentPerKill;
                Context.Health.Heal(healAmount);
            }

            _frenzyEndTime = Time.time + _frenzyDuration;

            if (!_isFrenzyActive && Context?.Stats != null)
            {
                _isFrenzyActive = true;
                Context.Stats.AddAttackSpeed(_frenzyAttackSpeedBonus);
            }
        }

        private void Update()
        {
            if (_isFrenzyActive && Time.time >= _frenzyEndTime)
            {
                _isFrenzyActive = false;
                if (Context?.Stats != null)
                {
                    Context.Stats.AddAttackSpeed(-_frenzyAttackSpeedBonus);
                }
            }
        }

        protected override void OnAugmentTeardown()
        {
            if (_isFrenzyActive && Context?.Stats != null)
            {
                _isFrenzyActive = false;
                Context.Stats.AddAttackSpeed(-_frenzyAttackSpeedBonus);
            }
        }
    }
}

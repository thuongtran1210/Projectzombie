using UnityEngine;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies.Passives
{
    [RequireComponent(typeof(HealthSystem), typeof(Enemy))]
    public class FactionPassive_Undead : MonoBehaviour
    {
        [Header("Undead Settings")]
        [Range(0f, 1f)]
        public float reviveChance = 0.5f;

        private HealthSystem _healthSystem;
        private Enemy _enemy;
        private bool _hasRevived = false;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            _hasRevived = false;
            _healthSystem.OnTryDie += HandleTryDie;
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnTryDie -= HandleTryDie;
            }
        }

        private bool HandleTryDie()
        {
            if (!_hasRevived && Random.value <= reviveChance)
            {
                _hasRevived = true;
                
                // Hồi 50% máu và cho phép hồi sinh
                _healthSystem.Heal(_enemy.Config.maxHealth * 0.5f, true);

                _enemy.Animator?.TriggerRevive();

                // Nếu quái đang ở trạng thái chết, trả về trạng thái truy đuổi
                if (_enemy.ChaseState != null)
                {
                    _enemy.StateMachine.ChangeState(_enemy.ChaseState);
                }

                return true; // Chặn cái chết
            }

            return false; // Cho phép chết bình thường
        }
    }
}

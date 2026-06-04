using UnityEngine;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Enemies.Passives
{
    [RequireComponent(typeof(Enemy))]
    public class FactionPassive_Void : MonoBehaviour
    {
        [Header("Void Corruption Settings")]
        public float corruptionRadius = 4f;
        [Tooltip("Giá trị dương: giảm % lượng EXP nhận được (ví dụ 0.5 = giảm 50%)")]
        public float expPenalty = 0.5f; 
        
        private Enemy _enemy;
        private PlayerStats _affectedPlayer;
        private bool _isDebuffing;
        private float _nextScanTime;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void Update()
        {
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + 0.5f;
                CheckCorruption();
            }
        }

        private void CheckCorruption()
        {
            if (_enemy.PlayerTransform == null) return;

            float distance = Vector2.Distance(transform.position, _enemy.PlayerTransform.position);

            if (distance <= corruptionRadius)
            {
                if (!_isDebuffing)
                {
                    ApplyDebuff();
                }
            }
            else
            {
                if (_isDebuffing)
                {
                    RemoveDebuff();
                }
            }
        }

        private void ApplyDebuff()
        {
            // Cần lấy Component trong Parent giống như EnemyAttackState (nếu Player GameObject lồng nhau)
            PlayerStats playerStats = _enemy.PlayerTransform.GetComponentInParent<PlayerStats>();
            
            // Fallback
            if (playerStats == null)
            {
                playerStats = _enemy.PlayerTransform.GetComponent<PlayerStats>();
            }

            if (playerStats != null)
            {
                _isDebuffing = true;
                _affectedPlayer = playerStats;
                // Trừ đi expPenalty (ví dụ giảm 50% => ExpMultiplier = 1.0 - 0.5 = 0.5)
                _affectedPlayer.AddExpMultiplier(-expPenalty);
            }
        }

        private void RemoveDebuff()
        {
            if (_isDebuffing && _affectedPlayer != null)
            {
                // Hoàn trả lại giá trị
                _affectedPlayer.AddExpMultiplier(expPenalty);
                _isDebuffing = false;
                _affectedPlayer = null;
            }
        }

        private void OnDisable()
        {
            RemoveDebuff();
        }
    }
}

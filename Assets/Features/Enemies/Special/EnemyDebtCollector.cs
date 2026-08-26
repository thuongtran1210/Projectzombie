using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Collectibles;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Enemies.Special
{
    /// <summary>
    /// E_MADOINO — Ma Đòi Nợ (Debt Collector Ghost).
    /// - Không tấn công làm giảm HP người chơi.
    /// - Áp sát từ phía sau, chạm vào người chơi sẽ "thó" mất 50 Cổ Tiền hoặc 20 Exp.
    /// - Sau khi thó tiền: Cắm đầu bỏ chạy thật nhanh (+80% Speed) trong 5.0s.
    /// - Nếu bị tiêu diệt trong 5s: Trả lại toàn bộ + Nhân đôi phần thưởng (+100 Cổ Tiền / +40 Exp).
    /// - Nếu chạy thoát: Tự hủy và biến mất vĩnh viễn.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyDebtCollector : MonoBehaviour
    {
        [Header("Steal Settings")]
        [SerializeField] private int stolenCoins = 50;
        [SerializeField] private float stolenExp = 20f;
        [SerializeField] private float fleeDuration = 5.0f;
        [SerializeField] private float fleeSpeedMultiplier = 1.8f;

        private Enemy _enemy;
        private bool _hasStolen = false;
        private float _fleeTimer = 0f;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            _hasStolen = false;
            _fleeTimer = 0f;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hasStolen) return;

            if (collision.CompareTag("Player") || collision.GetComponent<Player.PlayerController>() != null)
            {
                ExecuteSteal();
            }
        }

        private void ExecuteSteal()
        {
            _hasStolen = true;
            _fleeTimer = fleeDuration;

            // Trừ tiền / Exp từ RunStats hoặc CoinManager
            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.AddCoins(-stolenCoins);
            }

            // Tăng tốc bỏ chạy
            _enemy.MoveSpeedMultiplier = fleeSpeedMultiplier;

            StartCoroutine(RoutineFleeAndEscape());
        }

        private IEnumerator RoutineFleeAndEscape()
        {
            while (_fleeTimer > 0f)
            {
                _fleeTimer -= Time.deltaTime;

                // Di chuyển ngược hướng người chơi (bỏ chạy)
                if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
                {
                    float baseSpeed = _enemy.Config != null ? _enemy.Config.moveSpeed : 3f;
                    float finalSpeed = baseSpeed * _enemy.MoveSpeedMultiplier;
                    Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)PlayerProvider.PlayerTransform.position).normalized;
                    _enemy.Rb.velocity = awayFromPlayer * finalSpeed;
                }

                yield return null;
            }

            // Hết 5s mà không bị giết -> Chạy thoát thành công
            if (_enemy.HealthSystem != null && _enemy.HealthSystem.IsAlive)
            {
                gameObject.SetActive(false);
            }
        }

        public void HandleDeathReward()
        {
            if (_hasStolen)
            {
                // Thưởng gấp đôi khi bắt và tiêu diệt được Ma Đòi Nợ
                int rewardCoins = stolenCoins * 2;
                float rewardExp = stolenExp * 2;

                if (CoinPoolManager.Instance != null)
                {
                    CoinPoolManager.Instance.SpawnCoin(transform.position, rewardCoins);
                }
                if (ExpGemPoolManager.Instance != null && _enemy.ExpGemPrefab != null)
                {
                    ExpGemPoolManager.Instance.SpawnGem(_enemy.ExpGemPrefab, transform.position, rewardExp);
                }
            }
        }
    }
}

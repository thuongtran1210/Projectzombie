using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Collectibles;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.UI.DamageText;

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

        public bool HasStolen => _hasStolen;
        public bool IsFleeing => _hasStolen && _fleeTimer > 0f;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            _hasStolen = false;
            _fleeTimer = 0f;
            if (_enemy != null && _enemy.HealthSystem != null)
            {
                _enemy.HealthSystem.OnDied += HandleDeathReward;
            }
        }

        private void OnDisable()
        {
            if (_enemy != null && _enemy.HealthSystem != null)
            {
                _enemy.HealthSystem.OnDied -= HandleDeathReward;
            }
        }

        private void Update()
        {
            if (_hasStolen)
            {
                _fleeTimer -= Time.deltaTime;
                if (_fleeTimer <= 0f)
                {
                    // Hết 5s mà không bị tiêu diệt -> Chạy thoát thành công và biến mất
                    if (_enemy != null && _enemy.HealthSystem != null && _enemy.HealthSystem.IsAlive)
                    {
                        gameObject.SetActive(false);
                    }
                }
                return;
            }

            // Chủ động quét cự ly áp sát người chơi để thó tiền ngay cả khi va chạm vật lý bị IgnoreCollision
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                float stealRadius = _enemy != null && _enemy.Config != null ? Mathf.Max(_enemy.Config.AttackRange, 0.9f) : 1.0f;
                float dist = Vector2.Distance(transform.position, PlayerProvider.PlayerTransform.position);
                if (dist <= stealRadius)
                {
                    ExecuteSteal();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hasStolen) return;

            if (collision.CompareTag("Player") || collision.GetComponent<Player.PlayerController>() != null)
            {
                ExecuteSteal();
            }
        }

        public void ExecuteSteal()
        {
            if (_hasStolen) return;

            _hasStolen = true;
            _fleeTimer = fleeDuration;

            // Trừ tiền / Exp từ RunStats hoặc CoinManager
            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.AddCoins(-stolenCoins);
            }

            // Tái sử dụng chung ObjectPool DamageText (0-GC Alloc): Hiện -50 màu Đỏ Cam thuần số (không icon, không text)
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                Vector3 spawnPos = PlayerProvider.PlayerTransform.position + Vector3.up * 0.8f;
                if (DamageTextManager.Instance != null)
                {
                    DamageTextManager.Instance.ShowCustomText($"-{stolenCoins}", spawnPos, new Color(1f, 0.35f, 0.2f, 1f));
                }
            }

            // Tăng tốc bỏ chạy
            if (_enemy != null)
            {
                _enemy.MoveSpeedMultiplier = fleeSpeedMultiplier;
                if (_enemy.StateMachine != null && _enemy.ChaseState != null)
                {
                    _enemy.StateMachine.ChangeState(_enemy.ChaseState);
                }
            }
        }

        public void HandleDeathReward()
        {
            if (_hasStolen)
            {
                // Thưởng gấp đôi khi bắt và tiêu diệt được Ma Đòi Nợ
                int rewardCoins = stolenCoins * 2;
                float rewardExp = stolenExp * 2;

                // Tái sử dụng chung ObjectPool DamageText (0-GC Alloc): Hiện +100 màu Vàng Hoàng Kim thuần số
                Vector3 spawnPos = transform.position + Vector3.up * 0.8f;
                if (DamageTextManager.Instance != null)
                {
                    DamageTextManager.Instance.ShowCustomText($"+{rewardCoins}", spawnPos, new Color(1f, 0.88f, 0.2f, 1f));
                }

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

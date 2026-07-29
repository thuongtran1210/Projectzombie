using UnityEngine;
using ProjectZombie.Features.Shared;
using System.Collections;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Controller cho Boss 2: Skeleton King (Vua Xương) - Final Boss MVP (Phút 20, GDD 3.0).
    /// Hỗ trợ 2 Phase chiến đấu, Sword Wave, Bone Cage, Death Zone AoE và Summon Skeleton Archer Guard.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class SkeletonKingBossController : MonoBehaviour
    {
        [Header("Boss Config")]
        [SerializeField] private float phase2HealthThresholdRatio = 0.4f;
        [SerializeField] private GameObject victoryChestPrefab;
        [SerializeField] private GameObject skeletonArcherPrefab;

        [Header("Skill 1: Sword Wave")]
        [SerializeField] private float swordWaveCooldown = 4f;
        [SerializeField] private int waveCount = 3;
        [SerializeField] private float waveDamage = 25f;

        [Header("Skill 2: Bone Cage")]
        [SerializeField] private float boneCageCooldown = 12f;
        [SerializeField] private float cageDuration = 3f;

        [Header("Skill 3: Phase 2 Death Zone")]
        [SerializeField] private float deathZoneCooldown = 20f;
        [SerializeField] private float deathZoneRadius = 6f;
        [SerializeField] private float deathZoneDamagePerSec = 15f;

        private Enemy _enemy;
        private HealthSystem _healthSystem;
        private bool _isPhase2 = false;
        private bool _isPerformingSkill = false;

        private float _nextSwordWaveTime;
        private float _nextBoneCageTime;
        private float _nextDeathZoneTime;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _healthSystem = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            _isPhase2 = false;
            _isPerformingSkill = false;
            _nextSwordWaveTime = Time.time + 2f;
            _nextBoneCageTime = Time.time + 8f;
            _nextDeathZoneTime = Time.time + 12f;

            if (_healthSystem != null)
            {
                _healthSystem.OnHealthChanged += CheckPhaseTransition;
                _healthSystem.OnDied += HandleBossDeath;
            }
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnHealthChanged -= CheckPhaseTransition;
                _healthSystem.OnDied -= HandleBossDeath;
            }
        }

        private void Update()
        {
            if (_enemy.PlayerTransform == null || _healthSystem == null || _healthSystem.CurrentHealth <= 0) return;
            if (_isPerformingSkill) return;

            float distanceToPlayer = Vector2.Distance(transform.position, _enemy.PlayerTransform.position);

            // Skill 1: Sword Wave
            if (Time.time >= _nextSwordWaveTime && distanceToPlayer <= 10f)
            {
                StartCoroutine(PerformSwordWave());
                return;
            }

            // Skill 2: Bone Cage
            if (Time.time >= _nextBoneCageTime)
            {
                StartCoroutine(PerformBoneCage());
                return;
            }

            // Phase 2 Skill 3: Death Zone
            if (_isPhase2 && Time.time >= _nextDeathZoneTime && distanceToPlayer <= deathZoneRadius)
            {
                StartCoroutine(PerformDeathZone());
                return;
            }
        }

        private void CheckPhaseTransition(float currentHp, float maxHp)
        {
            if (!_isPhase2 && (currentHp / maxHp) <= phase2HealthThresholdRatio)
            {
                _isPhase2 = true;
                _enemy.DamageMultiplier *= 1.25f;
                Debug.Log("[SkeletonKing] BOSS CHIẾN ĐẤU PHASE 2 - LINH HỒN TỐI TĂM!");

                // Call 4 Skeleton Archers ở 4 góc
                SummonSkeletonGuard();
            }
        }

        private IEnumerator PerformSwordWave()
        {
            _isPerformingSkill = true;
            _nextSwordWaveTime = Time.time + swordWaveCooldown;
            _enemy.Rb.velocity = Vector2.zero;

            yield return new WaitForSeconds(0.4f);

            if (_enemy.PlayerTransform != null)
            {
                Vector2 baseDir = (_enemy.PlayerTransform.position - transform.position).normalized;
                float startAngle = -30f;
                float angleStep = 30f;

                for (int i = 0; i < waveCount; i++)
                {
                    float angle = startAngle + i * angleStep;
                    Vector2 waveDir = Quaternion.Euler(0, 0, angle) * baseDir;
                    
                    // Gửi sát thương sóng kiếm nếu quạt trúng Player
                    float dist = Vector2.Distance(transform.position, _enemy.PlayerTransform.position);
                    if (dist <= 8f && _enemy.PlayerHealthSystem != null)
                    {
                        _enemy.PlayerHealthSystem.TakeDamage(waveDamage * _enemy.DamageMultiplier);
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);
            _isPerformingSkill = false;
        }

        private IEnumerator PerformBoneCage()
        {
            _isPerformingSkill = true;
            _nextBoneCageTime = Time.time + boneCageCooldown;
            _enemy.Rb.velocity = Vector2.zero;

            Debug.Log("[SkeletonKing] BẤY LỒNG XƯƠNG (BONE CAGE) KHÓA DI CHUYỂN PLAYER!");
            // Khóa/làm chậm di chuyển Player trong 3 giây
            if (_enemy.PlayerTransform != null)
            {
                var playerController = _enemy.PlayerTransform.GetComponent<Player.PlayerController>();
                if (playerController != null)
                {
                    playerController.enabled = false;
                    yield return new WaitForSeconds(cageDuration);
                    playerController.enabled = true;
                }
                else
                {
                    yield return new WaitForSeconds(cageDuration);
                }
            }
            else
            {
                yield return new WaitForSeconds(cageDuration);
            }

            _isPerformingSkill = false;
        }

        private IEnumerator PerformDeathZone()
        {
            _isPerformingSkill = true;
            _nextDeathZoneTime = Time.time + deathZoneCooldown;

            float elapsedTime = 0f;
            while (elapsedTime < 4f)
            {
                if (_enemy.PlayerTransform != null)
                {
                    float dist = Vector2.Distance(transform.position, _enemy.PlayerTransform.position);
                    if (dist <= deathZoneRadius && _enemy.PlayerHealthSystem != null)
                    {
                        _enemy.PlayerHealthSystem.TakeDamage(deathZoneDamagePerSec * Time.deltaTime * _enemy.DamageMultiplier);
                    }
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _isPerformingSkill = false;
        }

        private void SummonSkeletonGuard()
        {
            if (skeletonArcherPrefab == null) return;

            Vector3[] offsets = new Vector3[]
            {
                new Vector3(4, 4, 0),
                new Vector3(-4, 4, 0),
                new Vector3(4, -4, 0),
                new Vector3(-4, -4, 0)
            };

            foreach (var offset in offsets)
            {
                Instantiate(skeletonArcherPrefab, transform.position + offset, Quaternion.identity);
            }
        }

        private void HandleBossDeath()
        {
            if (victoryChestPrefab != null)
            {
                Instantiate(victoryChestPrefab, transform.position, Quaternion.identity);
            }
            Debug.Log("[SkeletonKing] FINAL BOSS ĐÃ BỊ TIÊU DIỆT! VICTORY CHEST RỚT OUT!");
        }
    }
}

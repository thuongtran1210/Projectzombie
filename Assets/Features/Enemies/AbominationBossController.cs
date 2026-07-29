using UnityEngine;
using ProjectZombie.Features.Shared;
using System.Collections;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Controller cho Boss 1: Abomination (Kẻ Biến Dạng) - Xuất hiện phút 10 (GDD 3.0).
    /// Hỗ trợ 2 Phase chiến đấu, Bull Dash, Ground Slam, Toxic Cloud và Summon Swarm.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class AbominationBossController : MonoBehaviour
    {
        [Header("Boss Config")]
        [SerializeField] private float phase2HealthThresholdRatio = 0.5f;
        [SerializeField] private GameObject evolutionChestPrefab;
        [SerializeField] private GameObject walkerZombiePrefab;

        [Header("Skill 1: Bull Dash")]
        [SerializeField] private float dashCooldown = 8f;
        [SerializeField] private float dashWarningTime = 1.5f;
        [SerializeField] private float dashSpeedMultiplier = 3f;
        [SerializeField] private float dashDuration = 1.2f;

        [Header("Skill 2: Ground Slam")]
        [SerializeField] private float slamCooldown = 5f;
        [SerializeField] private float slamRadius = 3.5f;
        [SerializeField] private float slamDamage = 35f;

        [Header("Skill 3: Phase 2 Toxic Cloud")]
        [SerializeField] private float toxicCloudRadius = 4f;
        [SerializeField] private float toxicDamagePerSec = 5f;

        private Enemy _enemy;
        private HealthSystem _healthSystem;
        private bool _isPhase2 = false;
        private bool _isPerformingSkill = false;

        private float _nextDashTime;
        private float _nextSlamTime;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _healthSystem = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            _isPhase2 = false;
            _isPerformingSkill = false;
            _nextDashTime = Time.time + 3f;
            _nextSlamTime = Time.time + 5f;

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

            // Skill 1: Bull Dash
            if (Time.time >= _nextDashTime && distanceToPlayer > 3f)
            {
                StartCoroutine(PerformBullDash());
                return;
            }

            // Skill 2: Ground Slam
            if (Time.time >= _nextSlamTime && distanceToPlayer <= slamRadius)
            {
                StartCoroutine(PerformGroundSlam());
                return;
            }

            // Phase 2 Toxic Cloud AoE Damage
            if (_isPhase2 && distanceToPlayer <= toxicCloudRadius)
            {
                if (_enemy.PlayerHealthSystem != null)
                {
                    _enemy.PlayerHealthSystem.TakeDamage(toxicDamagePerSec * Time.deltaTime);
                }
            }
        }

        private void CheckPhaseTransition(float currentHp, float maxHp)
        {
            if (!_isPhase2 && (currentHp / maxHp) <= phase2HealthThresholdRatio)
            {
                _isPhase2 = true;
                _enemy.MoveSpeedMultiplier *= 1.2f;
                _enemy.DamageMultiplier *= 1.15f;
                Debug.Log("[Abomination] BOSS CHIẾN ĐẤU PHASE 2 - CUỒNG HĂNG!");

                // Summon Swarm ngay khi chuyển Phase 2
                SummonZombieSwarm(10);
            }
        }

        private IEnumerator PerformBullDash()
        {
            _isPerformingSkill = true;
            _nextDashTime = Time.time + dashCooldown;

            // Báo hiệu vệt lùi nhẹ chuẩn bị lao
            Vector2 dashDirection = (_enemy.PlayerTransform.position - transform.position).normalized;
            _enemy.Rb.velocity = Vector2.zero;

            yield return new WaitForSeconds(dashWarningTime);

            float elapsedTime = 0f;
            float baseSpeed = _enemy.Config != null ? _enemy.Config.moveSpeed : 2.2f;
            float dashSpeed = baseSpeed * dashSpeedMultiplier * _enemy.MoveSpeedMultiplier;

            while (elapsedTime < dashDuration)
            {
                _enemy.Rb.velocity = dashDirection * dashSpeed;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _enemy.Rb.velocity = Vector2.zero;
            _isPerformingSkill = false;
        }

        private IEnumerator PerformGroundSlam()
        {
            _isPerformingSkill = true;
            _nextSlamTime = Time.time + slamCooldown;
            _enemy.Rb.velocity = Vector2.zero;

            yield return new WaitForSeconds(0.5f); // Anim đập búa

            if (_enemy.PlayerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _enemy.PlayerTransform.position);
                if (dist <= slamRadius && _enemy.PlayerHealthSystem != null)
                {
                    _enemy.PlayerHealthSystem.TakeDamage(slamDamage * _enemy.DamageMultiplier);
                }
            }

            yield return new WaitForSeconds(0.5f);
            _isPerformingSkill = false;
        }

        private void SummonZombieSwarm(int count)
        {
            if (walkerZombiePrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * 3f;
                Vector3 spawnPos = transform.position + (Vector3)randomCircle;
                Instantiate(walkerZombiePrefab, spawnPos, Quaternion.identity);
            }
        }

        private void HandleBossDeath()
        {
            if (evolutionChestPrefab != null)
            {
                Instantiate(evolutionChestPrefab, transform.position, Quaternion.identity);
            }
            Debug.Log("[Abomination] BOSS ĐÃ BỊ TIÊU DIỆT! RỚT EVOLUTION CHEST!");
        }
    }
}

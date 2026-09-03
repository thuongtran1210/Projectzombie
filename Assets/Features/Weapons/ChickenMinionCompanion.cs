using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Gà Con Hộ Vệ (Chicken Minion Companion):
    /// - Được triệu hồi từ Cơn Lốc Chổi Lông Gà.
    /// - Đi theo sau Hero, tự động lướt đến mổ quái gần nhất (Rapid Peck).
    /// - Hỗ trợ hoạt ảnh Idle, Run, Attack thông qua Animator.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ChickenMinionCompanion : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private float attackRange = 2.5f;
        [SerializeField] private float peckDamage = 35f;
        [SerializeField] private float peckInterval = 0.35f;
        [SerializeField] private float lifetime = 25f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private SpriteRenderer _sr;
        private Transform _targetEnemy;
        private float _lastPeckTime;
        private float _spawnTime;
        private Vector2 _followOffset;
        private bool _isAttacking;
        private static readonly Collider2D[] _scanBuffer = new Collider2D[16];

        private static readonly int AnimStateIdle = Animator.StringToHash("Idle");
        private static readonly int AnimStateRun = Animator.StringToHash("Run");
        private static readonly int AnimStateAttack = Animator.StringToHash("Attack");

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
            {
                _sr.sortingLayerName = "Entities";
                _sr.sortingOrder = 10;
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            _followOffset = Random.insideUnitCircle.normalized * Random.Range(1.0f, 1.8f);
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
            transform.localScale = Vector3.one;
            PlayAnim(AnimStateIdle);
        }

        private void Update()
        {
            // Tự biến mất khi hết thời gian sống
            if (Time.time >= _spawnTime + lifetime)
            {
                Despawn();
                return;
            }

            Transform playerTf = PlayerProvider.HasPlayer ? PlayerProvider.PlayerTransform : null;
            if (playerTf == null) return;

            // 1. Quét tìm quái gần nhất
            FindNearestEnemy();

            // 2. Di chuyển: Nếu có quái gần -> lao vào mổ; Không có quái -> chạy theo Hero
            if (_targetEnemy != null)
            {
                Vector2 dirToEnemy = (_targetEnemy.position - transform.position);
                float dist = dirToEnemy.magnitude;

                if (dist > 0.6f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, _targetEnemy.position, moveSpeed * 1.3f * Time.deltaTime);
                    if (!_isAttacking) PlayAnim(AnimStateRun);
                }

                // Quay mặt theo hướng di chuyển
                if (dirToEnemy.x != 0)
                {
                    transform.localScale = new Vector3(Mathf.Sign(dirToEnemy.x), 1f, 1f);
                }

                // Mổ quái vật
                if (dist <= 1.0f && Time.time >= _lastPeckTime + peckInterval)
                {
                    PerformPeckAttack(_targetEnemy);
                }
            }
            else
            {
                // Đi theo Hero với hiệu ứng nhún nhảy lon ton
                Vector3 targetPos = playerTf.position + (Vector3)_followOffset;
                float distToTarget = Vector2.Distance(transform.position, targetPos);

                if (distToTarget > 0.35f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    if (!_isAttacking) PlayAnim(AnimStateRun);

                    Vector2 dirToPlayer = (targetPos - transform.position);
                    if (dirToPlayer.x != 0)
                    {
                        transform.localScale = new Vector3(Mathf.Sign(dirToPlayer.x), 1f, 1f);
                    }
                }
                else
                {
                    if (!_isAttacking) PlayAnim(AnimStateIdle);
                }
            }
        }

        private void PlayAnim(int stateHash)
        {
            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                animator.Play(stateHash);
            }
        }

        private void PerformPeckAttack(Transform enemyTf)
        {
            _lastPeckTime = Time.time;
            _isAttacking = true;
            PlayAnim(AnimStateAttack);

            if (enemyTf != null && enemyTf.TryGetComponent<HealthSystem>(out var hp))
            {
                DamageData dmg = new DamageData(peckDamage, false, ElementType.Kim, false, null);
                hp.TakeDamage(dmg);

                // Hiệu ứng mổ nảy người
                transform.localScale = new Vector3(transform.localScale.x * 1.15f, 0.85f, 1f);
                StartCoroutine(RoutineResetScale());

                // Âm thanh mổ vui nhộn
                global::Core.Audio.AudioManager.Instance?.PlaySlash(false, transform.position);
            }

            StartCoroutine(RoutineEndAttackAnim());
        }

        private IEnumerator RoutineEndAttackAnim()
        {
            yield return new WaitForSeconds(0.25f);
            _isAttacking = false;
        }

        private IEnumerator RoutineResetScale()
        {
            yield return new WaitForSeconds(0.08f);
            float sign = Mathf.Sign(transform.localScale.x);
            transform.localScale = new Vector3(sign, 1f, 1f);
        }

        private void FindNearestEnemy()
        {
            if (_targetEnemy != null && _targetEnemy.gameObject.activeInHierarchy)
            {
                float d = Vector2.Distance(transform.position, _targetEnemy.position);
                if (d <= attackRange * 1.5f) return; // Vẫn bám theo mục tiêu cũ
            }

            _targetEnemy = null;
            int mask = TargetingUtility.EnemyLayerMask;
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, _scanBuffer, mask);
            float nearestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _scanBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    _targetEnemy = col.transform;
                }
            }
        }

        private void Despawn()
        {
            Destroy(gameObject);
        }
    }
}

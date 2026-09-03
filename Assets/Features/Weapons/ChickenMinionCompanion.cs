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
    /// - Hành vi Pet sống động: Tự động chạy theo sau Hero, dạo quanh mổ đất khi rảnh (Roam & Forage),
    ///   nhảy lò cò (Hop bobbing), né nhau khi đông (Flock separation), và lướt mổ quái (Leap & Peck).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ChickenMinionCompanion : MonoBehaviour
    {
        private enum PetState
        {
            IdleForaging, // Đứng yên ngắm nghía hoặc mổ đất tìm thóc
            Wandering,    // Đi lon ton khám phá gần Hero
            CatchingUp,   // Chạy theo kịp Hero khi Hero chạy xa
            HuntingTarget // Lao đến tấn công quái vật
        }

        [Header("Stats")]
        [SerializeField] private float moveSpeed = 5.4f;
        [SerializeField] private float attackDetectRange = 5.5f;
        [SerializeField] private float peckRange = 1.1f;
        [SerializeField] private float peckDamage = 35f;
        [SerializeField] private float peckInterval = 0.32f;
        [SerializeField] private float lifetime = 25f;

        [Header("Pet Natural Behavior")]
        [SerializeField] private float followRadiusMin = 1.2f;
        [SerializeField] private float followRadiusMax = 2.4f;
        [SerializeField] private float separationRadius = 0.6f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private SpriteRenderer _sr;
        private Transform _targetEnemy;
        private float _lastPeckTime;
        private float _spawnTime;
        private bool _isAttacking;

        private PetState _state = PetState.CatchingUp;
        private Vector2 _currentWanderOffset;
        private float _nextWanderDecisionTime;
        private float _idleActionEndTime;
        private Vector2 _currentVelocity;
        private float _hopTimer;

        private static readonly List<ChickenMinionCompanion> _allActiveMinions = new List<ChickenMinionCompanion>();
        private static readonly Collider2D[] _scanBuffer = new Collider2D[24];

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

            PickNewWanderPoint();
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
            transform.localScale = Vector3.one;
            _allActiveMinions.Add(this);
            PlayAnim(AnimStateIdle);
        }

        private void OnDisable()
        {
            _allActiveMinions.Remove(this);
        }

        private void PlayAnim(int stateHash)
        {
            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                animator.Play(stateHash);
            }
        }

        private void Update()
        {
            if (Time.time >= _spawnTime + lifetime)
            {
                Despawn();
                return;
            }

            Transform playerTf = PlayerProvider.HasPlayer ? PlayerProvider.PlayerTransform : null;
            if (playerTf == null) return;

            // 1. Quét tìm quái vật ưu tiên
            FindNearestEnemy();

            // 2. Cập nhật FSM hành vi
            if (_targetEnemy != null)
            {
                _state = PetState.HuntingTarget;
                UpdateHuntingState();
            }
            else
            {
                UpdateCompanionPetState(playerTf);
            }

            // 3. Hiệu ứng nhún nhảy lò cò (Hopping) khi đang di chuyển
            ApplyMovementBobbing();
        }

        #region COMPANION PET AI (ROAM & FORAGE & CATCH UP)
        private void UpdateCompanionPetState(Transform playerTf)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTf.position);

            // Nếu Player chạy ra xa -> Chuyển ngay sang CatchingUp
            if (distToPlayer > followRadiusMax + 0.6f)
            {
                _state = PetState.CatchingUp;
            }

            switch (_state)
            {
                case PetState.CatchingUp:
                    Vector2 catchUpTarget = (Vector2)playerTf.position + _currentWanderOffset * 0.7f;
                    MoveTowardsPoint(catchUpTarget, moveSpeed * 1.35f);

                    if (distToPlayer <= followRadiusMin + 0.5f)
                    {
                        PickNewWanderPoint();
                        _state = PetState.Wandering;
                    }
                    break;

                case PetState.Wandering:
                    Vector2 roamTarget = (Vector2)playerTf.position + _currentWanderOffset;
                    float distToRoam = Vector2.Distance(transform.position, roamTarget);

                    if (distToRoam > 0.3f)
                    {
                        MoveTowardsPoint(roamTarget, moveSpeed * 0.75f);
                    }
                    else
                    {
                        // Đã tới điểm dạo -> Dừng lại mổ đất / ngắm nghía
                        _state = PetState.IdleForaging;
                        _idleActionEndTime = Time.time + Random.Range(1.2f, 2.5f);
                        PlayAnim(AnimStateIdle);

                        // 40% tỉ lệ làm động tác mổ đất giả định vui nhộn
                        if (Random.value < 0.4f && !_isAttacking)
                        {
                            StartCoroutine(RoutineFakeGroundPeck());
                        }
                    }

                    if (Time.time >= _nextWanderDecisionTime)
                    {
                        PickNewWanderPoint();
                    }
                    break;

                case PetState.IdleForaging:
                    if (distToPlayer > followRadiusMax + 0.3f)
                    {
                        _state = PetState.CatchingUp;
                        break;
                    }

                    if (Time.time >= _idleActionEndTime)
                    {
                        PickNewWanderPoint();
                        _state = PetState.Wandering;
                    }
                    else
                    {
                        if (!_isAttacking) PlayAnim(AnimStateIdle);
                    }
                    break;
            }
        }

        private void PickNewWanderPoint()
        {
            // Chọn một góc ngẫu nhiên quanh Player trong phạm vi min..max
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(followRadiusMin, followRadiusMax);
            _currentWanderOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
            _nextWanderDecisionTime = Time.time + Random.Range(2.8f, 5.0f);
        }

        private void MoveTowardsPoint(Vector2 targetPos, float speed)
        {
            // Thêm lực né nhau giữa các con gà (Flock Separation)
            Vector2 sepForce = CalculateSeparationForce();
            Vector2 finalTarget = targetPos + sepForce;

            Vector2 newPos = Vector2.SmoothDamp(transform.position, finalTarget, ref _currentVelocity, 0.12f, speed);
            transform.position = newPos;

            Vector2 moveDir = finalTarget - (Vector2)transform.position;
            if (Mathf.Abs(moveDir.x) > 0.05f)
            {
                transform.localScale = new Vector3(Mathf.Sign(moveDir.x), 1f, 1f);
            }

            if (!_isAttacking)
            {
                PlayAnim(AnimStateRun);
            }
        }

        private Vector2 CalculateSeparationForce()
        {
            Vector2 push = Vector2.zero;
            for (int i = 0; i < _allActiveMinions.Count; i++)
            {
                var other = _allActiveMinions[i];
                if (other == null || other == this) continue;

                Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
                float d = diff.magnitude;
                if (d > 0.001f && d < separationRadius)
                {
                    push += diff.normalized * ((separationRadius - d) * 1.5f);
                }
            }
            return push;
        }

        private void ApplyMovementBobbing()
        {
            // Khi đang chạy: nhún nhảy lò cò như gà con lon ton
            if (_currentVelocity.sqrMagnitude > 0.4f && !_isAttacking)
            {
                _hopTimer += Time.deltaTime * 18f;
                float hopScaleY = 1.0f + Mathf.Sin(_hopTimer) * 0.12f;
                float signX = Mathf.Sign(transform.localScale.x);
                transform.localScale = new Vector3(signX, hopScaleY, 1f);
            }
            else
            {
                _hopTimer = 0f;
            }
        }

        private IEnumerator RoutineFakeGroundPeck()
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            if (_state == PetState.IdleForaging && !_isAttacking)
            {
                PlayAnim(AnimStateAttack);
                yield return new WaitForSeconds(0.22f);
                PlayAnim(AnimStateIdle);
            }
        }
        #endregion

        #region HUNTING & ATTACK COMBAT
        private void UpdateHuntingState()
        {
            if (_targetEnemy == null) return;

            Vector2 dirToEnemy = (_targetEnemy.position - transform.position);
            float dist = dirToEnemy.magnitude;

            if (dist > peckRange)
            {
                MoveTowardsPoint(_targetEnemy.position, moveSpeed * 1.4f);
            }
            else
            {
                // Hướng mặt về quái vật
                if (Mathf.Abs(dirToEnemy.x) > 0.05f)
                {
                    transform.localScale = new Vector3(Mathf.Sign(dirToEnemy.x), 1f, 1f);
                }

                if (Time.time >= _lastPeckTime + peckInterval)
                {
                    PerformPeckAttack(_targetEnemy);
                }
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

                // Cú nhào mổ nảy người
                float sign = Mathf.Sign(transform.localScale.x);
                transform.localScale = new Vector3(sign * 1.25f, 0.8f, 1f);
                StartCoroutine(RoutineResetScale());

                // Âm thanh mổ vui tai
                global::Core.Audio.AudioManager.Instance?.PlaySlash(false, transform.position);
            }

            StartCoroutine(RoutineEndAttackAnim());
        }

        private IEnumerator RoutineEndAttackAnim()
        {
            yield return new WaitForSeconds(0.22f);
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
                if (d <= attackDetectRange * 1.3f) return; // Tiếp tục săn mục tiêu hiện tại
            }

            _targetEnemy = null;
            int mask = TargetingUtility.EnemyLayerMask;
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackDetectRange, _scanBuffer, mask);
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
        #endregion

        private void Despawn()
        {
            Destroy(gameObject);
        }
    }
}

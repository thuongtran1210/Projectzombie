using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons.Pets
{
    public class PetController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Tốc độ bay theo người chơi")]
        public float followSpeed = 5f;

        [Tooltip("Khoảng cách tối đa pet có thể bay xa khỏi người chơi trước khi bắt buộc quay về (MaxChasingRadius)")]
        public float maxDistanceFromPlayer = 10f;

        [Tooltip("Tốc độ lao vào kẻ địch khi tấn công")]
        public float attackMoveSpeed = 15f;

        [Tooltip("Khoảng cách bắt đầu đuổi theo người chơi")]
        public float followRadius = 3f;

        [Tooltip("Khoảng cách tối thiểu để dừng lại (vùng tròn xung quanh người chơi)")]
        public float stopRadius = 1.5f;

        [Header("Targeting")]
        [Tooltip("Bán kính tự động tìm mục tiêu (tính từ Pet)")]
        public float searchRadius = 8f;
        public LayerMask enemyLayer;

        [Header("Vampiric Settings")]
        [Tooltip("Lượng máu hồi phục cho người chơi mỗi khi pet cắn trúng kẻ địch")]
        public float healAmount = 1f;

        [Header("Idle & Flight Animation")]
        [Tooltip("Tốc độ thở/nhảy nhẹ tại chỗ")]
        public float bobSpeed = 3f;
        [Tooltip("Biên độ thở/nhảy nhẹ tại chỗ")]
        public float bobAmplitude = 0.2f;
        [Tooltip("Độ mượt mà khi đổi hướng (Quán tính càng cao số càng nhỏ, khuyên dùng: 0.1 - 0.3)")]
        public float smoothTime = 0.25f;

        [Header("References")]
        [Tooltip("Animator điều khiển hoạt ảnh của Pet")]
        [SerializeField] private Animator animator;

        private WeaponBase _weapon;
        private Transform _player;
        private Transform _target;

        // Trạng thái của Pet
        public enum PetState { Idle, Follow, Combat, Return }
        private PetState _currentState = PetState.Idle;

        private string _currentAnimState;
        private Vector3 _moveVelocity; // Dùng cho SmoothDamp
        private float _playerDirectionX = -1f; // Lưu hướng để neo vị trí Pet

        public PetState CurrentState => _currentState;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void PlayAnimation(string stateName)
        {
            if (animator == null) return;
            if (_currentAnimState == stateName) return;
            animator.Play(stateName);
            _currentAnimState = stateName;
        }

        public void Initialize(WeaponBase weapon, Transform player)
        {
            _weapon = weapon;
            _player = player;
        }

        private void Update()
        {
            if (_player == null) return;

            float distToPlayer = Vector2.Distance(transform.position, _player.position);

            // Cập nhật hướng di chuyển của Player để Pet né chọn vị trí đứng (Ví dụ: Luôn bay phía sau lưng Player)
            // Bạn có thể thay đổi logic này dựa vào scale hoặc vận tốc của Player
            if (_player.position.x - transform.position.x > 0.1f) _playerDirectionX = -1f; // Player đi bên phải, Pet đứng bên trái
            else if (_player.position.x - transform.position.x < -0.1f) _playerDirectionX = 1f;

            // Return (Quay về gấp)
            if (_currentState != PetState.Return && distToPlayer > maxDistanceFromPlayer)
            {
                _currentState = PetState.Return;
                _target = null;
            }

            if (_currentState != PetState.Return)
            {
                if (_target == null || !_target.gameObject.activeInHierarchy || Vector2.Distance(transform.position, _target.position) > searchRadius)
                {
                    FindTarget();
                }

                if (_target != null)
                {
                    _currentState = PetState.Combat;
                }
            }

            switch (_currentState)
            {
                case PetState.Idle:
                    PlayIdleBehavior(distToPlayer);
                    break;
                case PetState.Follow:
                    FollowPlayer(distToPlayer);
                    break;
                case PetState.Combat:
                    MoveToTargetAndAttack();
                    break;
                case PetState.Return:
                    ReturnToPlayer(distToPlayer);
                    break;
            }
        }

        private void PlayIdleBehavior(float distToPlayer)
        {
            if (distToPlayer > followRadius)
            {
                _currentState = PetState.Follow;
                return;
            }

            PlayAnimation("Idle");

            // Hiệu ứng nhấp nhô sinh động tại chỗ
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            Vector3 targetPos = _player.position + new Vector3(_playerDirectionX * stopRadius, 1.2f + bob, 0f);

            FlipTowards(targetPos.x);

            // Sử dụng SmoothDamp thay vì Lerp để triệt tiêu hoàn toàn sự giật khựng
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _moveVelocity, smoothTime);
        }

        private void FollowPlayer(float distToPlayer)
        {
            if (distToPlayer <= stopRadius)
            {
                _currentState = PetState.Idle;
                return;
            }

            PlayAnimation("Idle"); // Hoặc hoạt ảnh "Fly/Run" nếu có

            // Vẫn giữ hiệu ứng nhấp nhô nhẹ khi bay để tạo cảm giác thực tế
            float bob = Mathf.Sin(Time.time * bobSpeed * 1.2f) * (bobAmplitude * 0.5f);
            Vector3 targetPos = _player.position + new Vector3(_playerDirectionX * stopRadius, 1.5f + bob, 0f);

            FlipTowards(targetPos.x);

            // Dùng SmoothDamp giúp Pet bám đuổi có độ trễ (Quán tính bay) cực kỳ tự nhiên
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _moveVelocity, smoothTime, followSpeed);
        }

        private void MoveToTargetAndAttack()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                _currentState = PetState.Idle;
                return;
            }

            // Khi chiến đấu tấn công quyết liệt, dùng MoveTowards để đạt độ chính xác cao thẳng vào quái
            FlipTowards(_target.position.x);
            transform.position = Vector3.MoveTowards(transform.position, _target.position, attackMoveSpeed * Time.deltaTime);
        }

        private void ReturnToPlayer(float distToPlayer)
        {
            FlipTowards(_player.position.x);
            // Bay về với vận tốc cao
            transform.position = Vector3.MoveTowards(transform.position, _player.position, attackMoveSpeed * Time.deltaTime);

            if (distToPlayer <= stopRadius)
            {
                _moveVelocity = Vector3.zero; // Reset vận tốc tránh bị quán tính kéo đi tiếp
                _currentState = PetState.Idle;
            }
        }

        private void FlipTowards(float targetX)
        {
            if (animator == null) return;
            Transform visualTransform = animator.transform;
            if (targetX > transform.position.x + 0.05f)
            {
                visualTransform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (targetX < transform.position.x - 0.05f)
            {
                visualTransform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        public void BiteTarget()
        {
            if (_target == null) return;

            var health = _target.GetComponent<HealthSystem>();
            if (health != null)
            {
                DamageData damageData = _weapon != null 
                    ? _weapon.CreateDamageData() 
                    : DamageUtility.CalculateDamage(10f, 0.05f);
                health.TakeDamage(damageData);

                var playerHealth = _player.GetComponent<HealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(healAmount);
                }
            }
        }

        private void FindTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);
            float minDistance = float.MaxValue;
            _target = null;

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<HealthSystem>();
                if (health != null && health.CurrentHealth <= 0) continue;

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    _target = hit.transform;
                }
            }
        }

        public bool HasTarget() => _target != null;

        public bool IsCloseToTarget() => _target != null && Vector2.Distance(transform.position, _target.position) < 0.6f;

        public void TriggerAttack()
        {
            if (_currentState == PetState.Idle || _currentState == PetState.Follow)
            {
                FindTarget();
                if (HasTarget())
                {
                    _currentState = PetState.Combat;
                }
            }
        }
    }
}
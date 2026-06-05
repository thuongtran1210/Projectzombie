using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons.Pets
{
    public class PetController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Tốc độ bay theo người chơi")]
        public float followSpeed = 5f;
        
        [Tooltip("Khoảng cách tối đa pet có thể bay xa khỏi người chơi")]
        public float maxDistanceFromPlayer = 10f;
        
        [Tooltip("Tốc độ lao vào kẻ địch khi tấn công")]
        public float attackMoveSpeed = 15f;

        [Header("Targeting")]
        [Tooltip("Bán kính tự động tìm mục tiêu (tính từ Pet)")]
        public float searchRadius = 8f;
        public LayerMask enemyLayer;

        [Header("Vampiric Settings")]
        [Tooltip("Lượng máu hồi phục cho người chơi mỗi khi pet cắn trúng kẻ địch")]
        public float healAmount = 1f;

        private WeaponBase _weapon;
        private Transform _player;
        private Transform _target;
        
        // Trạng thái của Pet
        private enum PetState { Following, Attacking, Returning }
        private PetState _currentState = PetState.Following;

        public void Initialize(WeaponBase weapon, Transform player)
        {
            _weapon = weapon;
            _player = player;
        }

        private void Update()
        {
            if (_player == null) return;

            // Luôn tìm kiếm mục tiêu nếu chưa có
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                FindTarget();
            }

            switch (_currentState)
            {
                case PetState.Following:
                    FollowPlayer();
                    break;
                case PetState.Attacking:
                    MoveToTargetAndBite();
                    break;
                case PetState.Returning:
                    ReturnToPlayer();
                    break;
            }
        }

        private void FollowPlayer()
        {
            // Bay lượn quanh người chơi (có thể thêm logic bay hình sin hoặc hình số 8 nếu muốn mượt hơn)
            // Tạm thời cho bay theo vị trí ngay trên đầu/bên cạnh player
            Vector3 targetPos = _player.position + new Vector3(-1f, 1.5f, 0f);
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }

        private void MoveToTargetAndBite()
        {
            if (_target == null)
            {
                _currentState = PetState.Returning;
                return;
            }

            // Bay tốc độ cao về phía kẻ địch
            transform.position = Vector3.MoveTowards(transform.position, _target.position, attackMoveSpeed * Time.deltaTime);

            // Kiểm tra nếu chạm vào kẻ địch thì cắn
            if (Vector2.Distance(transform.position, _target.position) < 0.5f)
            {
                BiteTarget();
                _currentState = PetState.Returning; // Cắn xong bay về
                _target = null; // Reset mục tiêu để tìm mục tiêu mới cho lần đánh sau
            }
        }

        private void ReturnToPlayer()
        {
            // Bay về người chơi
            transform.position = Vector3.MoveTowards(transform.position, _player.position, attackMoveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, _player.position) < 2f)
            {
                _currentState = PetState.Following;
            }
        }

        private void BiteTarget()
        {
            if (_target == null) return;

            var health = _target.GetComponent<HealthSystem>();
            if (health != null)
            {
                // Gây sát thương dựa trên chỉ số của súng
                DamageData damageData = DamageUtility.CalculateDamage(_weapon.GetFinalDamage(), _weapon.GetFinalCritChance(), _weapon.GetFinalCritDamage());
                health.TakeDamage(damageData);

                // Hút máu cho người chơi
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

        /// <summary>
        /// Được gọi từ Weapon_PetSummon mỗi khi hết thời gian Cooldown
        /// </summary>
        public void TriggerAttack()
        {
            if (_currentState == PetState.Following && HasTarget())
            {
                _currentState = PetState.Attacking;
            }
        }
    }
}

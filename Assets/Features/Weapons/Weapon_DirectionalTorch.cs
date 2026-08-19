using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Độc Quyền Thanh Đồng: "Mồi Lửa Soi Đường" (Ngọn Đuốc Định Hướng).
    /// Phun luồng bão lửa hình nón liên tục theo hướng điều khiển joystick / di chuyển của người chơi.
    /// Thiêu đốt diện rộng và càn quét đội hình quái vật.
    /// </summary>
    public class Weapon_DirectionalTorch : Weapon_RangedBase
    {
        [Header("Torch Stream Settings")]
        [Tooltip("Góc tỏa của luồng lửa hình nón (độ)")]
        [SerializeField] private float _coneAngle = 55f;

        [Tooltip("Khoảng cách phun lửa cơ bản")]
        [SerializeField] private float _torchRange = 5.0f;

        [Tooltip("Nếu không di chuyển, có tự động ngắm mục tiêu gần nhất hay giữ hướng nhìn")]
        [SerializeField] private bool _autoAimNearestWhenIdle = true;

        private PlayerController _playerController;
        private Vector2 _currentAimDirection = Vector2.right;

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();
        }

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            if (_playerController == null)
            {
                _playerController = GetComponentInParent<PlayerController>();
            }
        }

        protected override bool CanAttack()
        {
            // Xác định hướng phun lửa
            UpdateAimDirection();
            return true;
        }

        private void UpdateAimDirection()
        {
            if (_playerController != null && _playerController.MovementInput.sqrMagnitude > 0.01f)
            {
                // Ưu tiên hướng joystick/di chuyển của người chơi
                _currentAimDirection = _playerController.MovementInput.normalized;
            }
            else if (_autoAimNearestWhenIdle)
            {
                // Khi đứng yên: tìm quái gần nhất trong tầm
                float range = CharacterStats != null ? CharacterStats.AttackRange : _torchRange;
                Transform nearestEnemy = TargetingUtility.FindNearestEnemy(transform.position, range);
                if (nearestEnemy != null)
                {
                    _currentAimDirection = ((Vector2)(nearestEnemy.position - transform.position)).normalized;
                }
                else if (transform.root != null)
                {
                    // Giữ theo hướng quay mặt của nhân vật
                    _currentAimDirection = transform.root.localScale.x >= 0 ? Vector2.right : Vector2.left;
                }
            }
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            Vector2 baseDir = _currentAimDirection.sqrMagnitude > 0.01f ? _currentAimDirection : Vector2.right;
            Vector3 originPos = firePoint != null ? firePoint.position : transform.position;

            DamageData damageData = CreateDamageData();
            int projectileCount = Mathf.Max(1, GetFinalProjectileCount());

            // Phun chùm tia lửa phân tán trong góc nón
            for (int i = 0; i < projectileCount; i++)
            {
                float randomSpread = Random.Range(-_coneAngle * 0.5f, _coneAngle * 0.5f);
                Vector2 flameDir = Quaternion.Euler(0, 0, randomSpread) * baseDir;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(
                    projectileData, 
                    originPos, 
                    flameDir, 
                    gameObject, 
                    damageData
                );

                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}

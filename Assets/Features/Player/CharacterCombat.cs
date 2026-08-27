using System;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.Juice;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Quản lý đòn đánh cơ bản đặc trưng cho từng nhân vật (Character Signature Basic Attack).
    /// Hỗ trợ cả 2 trường phái: Cận chiến (Melee Slash) và Tầm xa (Ranged Projectile),
    /// điều phối Animation, hiệu ứng VFX Slash/Đạn và tính toán sát thương kết hợp Combo 1-2-3.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class CharacterCombat : MonoBehaviour
    {
        [Header("Character Basic Attack Configuration")]
        [SerializeField] private CharacterAttackConfig attackConfig;

        [Header("References")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private PlayerAnimator playerAnimator;
        [SerializeField] private PlayerController playerController;

        private PlayerStats _playerStats;
        private float _lastAttackTime;
        private int _currentComboStep = 1;
        private float _lastComboHitTime;

        // Bộ nhớ đệm không cấp phát GC cho đòn quét cận chiến
        private static readonly Collider2D[] _meleeHitBuffer = new Collider2D[50];

        public CharacterAttackConfig Config => attackConfig;
        public int CurrentComboStep => _currentComboStep;
        public Sprite AttackIcon => attackConfig != null ? attackConfig.attackIcon : null;

        /// <summary>
        /// Sự kiện phát ra khi nhân vật tung một đòn đánh thường.
        /// </summary>
        public event Action<int> OnAttackExecuted; // comboStep

        /// <summary>
        /// Sự kiện phát ra khi đòn đánh trúng kẻ địch. Dành cho các Pháp bảo On-Hit lắng nghe.
        /// </summary>
        public event Action<DamageData, Collider2D> OnHitEnemy;

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (firePoint == null) firePoint = transform;

            // Khởi tạo config từ RunLoadoutState nếu có
            if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.basicAttackConfig != null)
            {
                attackConfig = RunLoadoutState.SelectedCharacter.basicAttackConfig;
            }
        }

        public void SetAttackConfig(CharacterAttackConfig config)
        {
            if (config != null)
            {
                attackConfig = config;
            }
        }

        public float RemainingCooldown
        {
            get
            {
                float totalAttackSpeed = GetTotalAttackSpeed();
                float attackCooldown = 1f / Mathf.Max(0.01f, totalAttackSpeed);
                float remaining = (_lastAttackTime + attackCooldown) - Time.time;
                return Mathf.Max(0f, remaining);
            }
        }

        public float GetTotalAttackSpeed()
        {
            float baseSpeed = attackConfig != null ? attackConfig.baseAttackSpeed : 1.8f;
            float statBonus = _playerStats != null ? _playerStats.AttackSpeed : 1.0f;
            return baseSpeed * statBonus;
        }

        private void Update()
        {
            // Tự động reset combo về nhát 1 nếu quá thời gian chờ (Combo Window)
            float resetWindow = attackConfig != null ? attackConfig.comboResetWindow : 1.0f;
            if (_currentComboStep > 1 && Time.time >= _lastComboHitTime + resetWindow)
            {
                _currentComboStep = 1;
            }
        }

        /// <summary>
        /// Kích hoạt đòn đánh từ nút Tấn Công (Attack Button).
        /// </summary>
        public bool TriggerAttack()
        {
            if (RemainingCooldown > 0f) return false;

            ExecuteAttack(_currentComboStep);

            // Cập nhật Combo Step tiếp theo
            int maxCombo = attackConfig != null ? attackConfig.maxComboSteps : 3;
            _currentComboStep = (_currentComboStep % maxCombo) + 1;
            _lastComboHitTime = Time.time;
            _lastAttackTime = Time.time;

            return true;
        }

        private void ExecuteAttack(int comboStep)
        {
            if (attackConfig == null) return;

            // 1. Kích hoạt hoạt ảnh nhân vật & Slowdown tạo lực đầm
            if (playerAnimator != null)
            {
                playerAnimator.ChangeAnimationState(PlayerAnimationState.Attack);
            }

            if (playerController != null)
            {
                playerController.NotifyAttackStarted(comboStep);
            }

            // 2. Định hướng đánh: Ưu tiên hướng nhìn hiện tại hoặc quái gần nhất
            Vector2 attackDirection = GetAttackDirection();

            // 3. Thực thi đòn đánh theo Type
            if (attackConfig.attackType == CharacterAttackType.MeleeSlash)
            {
                ExecuteMeleeSlash(comboStep, attackDirection);
            }
            else
            {
                ExecuteRangedProjectile(comboStep, attackDirection);
            }

            OnAttackExecuted?.Invoke(comboStep);
        }

        private Vector2 GetAttackDirection()
        {
            // Kiểm tra hướng lật mặt của visual hoặc input di chuyển
            float facing = transform.localScale.x >= 0 ? 1f : -1f;
            if (playerController != null && playerController.MovementInput != Vector2.zero)
            {
                return playerController.MovementInput.normalized;
            }

            // Thử soft-aim quái gần nhất trong tầm 5m
            Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 5.0f);
            if (nearest != null)
            {
                return ((Vector2)(nearest.position - transform.position)).normalized;
            }

            return new Vector2(facing, 0f);
        }

        private void ExecuteMeleeSlash(int comboStep, Vector2 direction)
        {
            float offset = attackConfig.meleeOffset;
            Vector2 boxSize = attackConfig.meleeAreaSize;
            Vector2 center = (Vector2)transform.position + direction * offset;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 1. Sinh hiệu ứng VFX Vệt Chém nếu có
            if (attackConfig.slashVfxPrefab != null)
            {
                GameObject vfxObj = Instantiate(attackConfig.slashVfxPrefab, center, Quaternion.Euler(0, 0, angle));
                Destroy(vfxObj, 1.0f);
            }

            // 2. Tính toán sát thương
            float comboMultiplier = GetComboMultiplier(comboStep);
            float baseAtk = _playerStats != null ? _playerStats.GetTotalDamage() : 20f;
            float totalDamage = baseAtk * attackConfig.baseDamageMultiplier * comboMultiplier;
            bool isCrit = _playerStats != null && UnityEngine.Random.value < _playerStats.CritChance;
            if (isCrit) totalDamage *= 1.5f;

            DamageData damageData = new DamageData(
                totalDamage,
                isCrit,
                attackConfig.element,
                false,
                null
            );

            // 3. Quét va chạm gây damage
            int mask = TargetingUtility.EnemyLayerMask;
            int numHits = Physics2D.OverlapBoxNonAlloc(center, boxSize, angle, _meleeHitBuffer, mask);
            bool hitAnyEnemy = false;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _meleeHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth > 0)
                {
                    ElementType defenderElement = ElementType.None;
                    if (hit.TryGetComponent<Enemies.Enemy>(out var enemy))
                    {
                        defenderElement = enemy.CurrentElement;
                    }

                    DamageData hitDamage = DamageUtility.CalculateHitDamage(
                        damageData.Amount,
                        damageData.IsCritical,
                        damageData.Element,
                        defenderElement,
                        null
                    );

                    health.TakeDamage(hitDamage);
                    hitAnyEnemy = true;
                    OnHitEnemy?.Invoke(hitDamage, hit);

                    if (enemy != null && !enemy.IsHeavyArmor)
                    {
                        Vector2 pushDir = ((Vector2)(hit.transform.position - transform.position)).normalized;
                        enemy.ApplyKnockback(pushDir, attackConfig.knockbackForce, 0.15f);
                    }
                }
            }

            if (hitAnyEnemy)
            {
                if (isCrit)
                {
                    GameJuiceEvents.RequestCameraShake(0.15f, 0.15f);
                    GameJuiceEvents.RequestHitStop(0.05f);
                }
                else
                {
                    GameJuiceEvents.RequestCameraShake(0.08f, 0.04f);
                }
            }
        }

        private void ExecuteRangedProjectile(int comboStep, Vector2 direction)
        {
            if (attackConfig.projectilePrefab == null) return;

            float comboMultiplier = GetComboMultiplier(comboStep);
            float baseAtk = _playerStats != null ? _playerStats.GetTotalDamage() : 20f;
            float totalDamage = baseAtk * attackConfig.baseDamageMultiplier * comboMultiplier;
            bool isCrit = _playerStats != null && UnityEngine.Random.value < _playerStats.CritChance;
            if (isCrit) totalDamage *= 1.5f;

            DamageData damageData = new DamageData(
                totalDamage,
                isCrit,
                attackConfig.element,
                false,
                null
            );

            int count = Mathf.Max(1, attackConfig.projectileCount);
            float spread = attackConfig.spreadAngle;
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

            for (int i = 0; i < count; i++)
            {
                float offsetAngle = count > 1 ? (i - (count - 1) / 2f) * (spread / (count - 1)) : 0f;
                Vector2 boltDir = Quaternion.Euler(0, 0, offsetAngle) * direction;

                GameObject projObj = Instantiate(attackConfig.projectilePrefab, spawnPos, Quaternion.Euler(0, 0, Mathf.Atan2(boltDir.y, boltDir.x) * Mathf.Rad2Deg));
                
                // Gán vận tốc và thông số nếu có Rigidbody2D
                if (projObj.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.velocity = boltDir * attackConfig.projectileSpeed;
                }

                Destroy(projObj, attackConfig.projectileLifetime);
            }
        }

        private float GetComboMultiplier(int step)
        {
            if (attackConfig == null) return 1.0f;
            switch (step)
            {
                case 2: return attackConfig.comboStep2Multiplier;
                case 3: return attackConfig.comboStep3Multiplier;
                default: return 1.0f;
            }
        }
    }
}

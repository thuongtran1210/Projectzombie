using System;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.Juice;
using ProjectZombie.Features.Projectiles;

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
        private Rigidbody2D _rb;
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
            _rb = GetComponent<Rigidbody2D>();
            if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (firePoint == null) firePoint = transform;

            // Khởi tạo config từ RunLoadoutState nếu có
            if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.basicAttackConfig != null)
            {
                attackConfig = RunLoadoutState.SelectedCharacter.basicAttackConfig;
            }

            // Fallback an toàn: Nếu chưa có attackConfig hoặc thiếu slashVfxPrefab thì tự nạp từ CharacterSelectionData
            EnsureAttackConfigFallback();
        }

        private void EnsureAttackConfigFallback()
        {
            if (attackConfig == null || (attackConfig.attackType == CharacterAttackType.MeleeSlash && attackConfig.slashVfxPrefab == null))
            {
#if UNITY_EDITOR
                var data = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterSelectionData>("Assets/_Data/CharacterSelectionData.asset");
                if (data != null && data.Characters != null && data.Characters.Count > 0)
                {
                    var charEntry = data.Characters[0];
                    if (attackConfig == null) attackConfig = charEntry.basicAttackConfig;
                    else if (attackConfig.slashVfxPrefab == null) attackConfig.slashVfxPrefab = charEntry.basicAttackConfig.slashVfxPrefab;
                }

                if (attackConfig != null && attackConfig.slashVfxPrefab == null)
                {
                    attackConfig.slashVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_ThuSinh_InkSlash.prefab");
                }
#endif
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

        private SpriteRenderer _aimIndicator;

        private void InitAimIndicator()
        {
            if (_aimIndicator != null) return;

            GameObject arrowObj = new GameObject("VFX_Attack_Aim_Indicator");
            arrowObj.transform.SetParent(transform, false);
            arrowObj.transform.localPosition = Vector3.zero;

            _aimIndicator = arrowObj.AddComponent<SpriteRenderer>();
            _aimIndicator.sprite = Resources.Load<Sprite>("Art/UI/HUD/Tex_Attack_Aim_Arc_Reticle");
            if (_aimIndicator.sprite == null)
            {
#if UNITY_EDITOR
                _aimIndicator.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Tex_Attack_Aim_Arc_Reticle.png");
#endif
            }
            _aimIndicator.sortingLayerName = "Skill";
            _aimIndicator.sortingOrder = 3;

            // Màu sắc theo bản sắc nguyên tố tướng (Thư Sinh: Vàng Kim, Đạo Sĩ: Xanh Ngọc, Thanh Đồng: Đỏ Cam, Ẩn Sĩ: Hổ Phách)
            Color themeColor = new Color(1.0f, 0.85f, 0.2f, 0.65f);
            if (attackConfig != null)
            {
                if (attackConfig.attackName.Contains("Tiên Đạo") || attackConfig.attackName.Contains("Linh Phù"))
                    themeColor = new Color(0.25f, 0.95f, 0.85f, 0.65f); // Xanh ngọc
                else if (attackConfig.attackName.Contains("Đuốc") || attackConfig.attackName.Contains("Lửa"))
                    themeColor = new Color(1.0f, 0.4f, 0.1f, 0.7f); // Đỏ cam
                else if (attackConfig.attackName.Contains("Thạch") || attackConfig.attackName.Contains("Địa"))
                    themeColor = new Color(0.9f, 0.65f, 0.25f, 0.7f); // Hổ phách
            }
            _aimIndicator.color = themeColor;
            arrowObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }

        private void UpdateAimIndicator()
        {
            if (_aimIndicator == null)
            {
                InitAimIndicator();
            }

            if (_aimIndicator != null)
            {
                Vector2 dir = GetAttackDirection();
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                _aimIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
                _aimIndicator.transform.localPosition = (Vector3)(dir * 0.4f);
            }
        }

        private void Update()
        {
            UpdateAimIndicator();

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

            // 1. Đồng bộ tốc độ Animation của Animator theo Tốc Đánh thực tế
            float currentAtkSpeed = attackConfig.baseAttackSpeed;
            if (_playerStats != null && _playerStats.AttackSpeed > 0.01f)
            {
                currentAtkSpeed *= _playerStats.AttackSpeed;
            }
            if (playerAnimator != null)
            {
                playerAnimator.SetAttackAnimationSpeed(currentAtkSpeed / Mathf.Max(0.1f, attackConfig.baseAttackSpeed));
                playerAnimator.ChangeAnimationState(PlayerAnimationState.Attack);
            }

            if (playerController != null)
            {
                playerController.NotifyAttackStarted(comboStep);
            }

            // 2. Định hướng đánh: Ưu tiên hướng nhìn hiện tại hoặc quái gần nhất
            Vector2 attackDirection = GetAttackDirection();

            // 3. Thực thi đòn đánh theo Action Window Timing (Zero-GC Coroutine Flow)
            if (attackConfig.attackType == CharacterAttackType.MeleeSlash)
            {
                StartCoroutine(ExecuteMeleeSlashRoutine(comboStep, attackDirection, currentAtkSpeed));
            }
            else
            {
                StartCoroutine(ExecuteRangedProjectileRoutine(comboStep, attackDirection, currentAtkSpeed));
            }

            OnAttackExecuted?.Invoke(comboStep);
        }

        private Vector2 GetAttackDirection()
        {
            // 1. Kiểm tra hướng di chuyển đang bấm nếu có
            if (playerController != null && playerController.MovementInput != Vector2.zero)
            {
                return playerController.MovementInput.normalized;
            }

            // 2. Thử soft-aim quái gần nhất trong tầm 5m
            Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 5.0f);
            if (nearest != null)
            {
                return ((Vector2)(nearest.position - transform.position)).normalized;
            }

            // 3. Nếu đứng yên và không có quái: Giữ nguyên hướng mặt hiện tại (FacingDirection: Trái hoặc Phải)
            float facing = playerController != null ? playerController.FacingDirection : (playerAnimator != null ? playerAnimator.FacingDirection : 1f);
            return new Vector2(facing, 0f);
        }

        private System.Collections.IEnumerator ExecuteMeleeSlashRoutine(int comboStep, Vector2 direction, float currentAtkSpeed)
        {
            // Tự động xoay mặt nhân vật theo hướng chém
            if (playerAnimator != null && Mathf.Abs(direction.x) > 0.05f)
            {
                playerAnimator.FlipToDirection(direction.x);
            }

            // Pha 1: Wind-up Delay (Chờ tay nhân vật giơ lên trước khi bung vệt kiếm)
            float totalCycle = 1f / Mathf.Max(0.1f, currentAtkSpeed);
            float windupDelay = totalCycle * Mathf.Clamp(attackConfig.windupRatio, 0.05f, 0.35f);
            if (windupDelay > 0.01f)
            {
                yield return new WaitForSeconds(windupDelay);
            }

            // Pha 2: Active Impact (Bung vệt chém + Quét va chạm đúng khoảnh khắc chém)
            float offset = attackConfig.meleeOffset;
            Vector2 boxSize = attackConfig.meleeAreaSize;
            Vector2 center = (Vector2)transform.position + direction * offset;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 1. Sinh hiệu ứng VFX Vệt Chém
            if (attackConfig.slashVfxPrefab != null)
            {
                GameObject vfxObj = Instantiate(attackConfig.slashVfxPrefab, center, Quaternion.Euler(0, 0, angle));
                var psList = vfxObj.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var p in psList)
                {
                    p.Clear();
                    p.Play();
                }
                float life = attackConfig.vfxDuration > 0 ? attackConfig.vfxDuration : 0.45f;
                Destroy(vfxObj, life);
            }

            // 2. Lực dấn người tới trước (Attack Lunge Impulse)
            ApplyAttackLunge(comboStep, direction);

            // 3. Tính toán sát thương
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

            // 4. Quét va chạm gây damage (Zero-GC OverlapBox)
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

                    // Spawn Tia lửa va chạm (HitSparks) tại điểm trúng
                    SpawnHitImpactSparks(hit.transform.position);

                    if (enemy != null && !enemy.IsHeavyArmor)
                    {
                        Vector2 pushDir = ((Vector2)(hit.transform.position - transform.position)).normalized;
                        float knockbackForce = attackConfig.knockbackForce;
                        if (comboStep == 3) knockbackForce *= 1.6f;
                        enemy.ApplyKnockback(pushDir, knockbackForce, comboStep == 3 ? 0.22f : 0.15f);
                    }
                }
            }

            // 5. Game Feel theo bậc thang Combo (HitStop + CameraShake)
            if (hitAnyEnemy)
            {
                TriggerDynamicGameJuice(comboStep, isCrit);
            }
        }

        private System.Collections.IEnumerator ExecuteRangedProjectileRoutine(int comboStep, Vector2 direction, float currentAtkSpeed)
        {
            if (attackConfig.projectilePrefab == null) yield break;

            float totalCycle = 1f / Mathf.Max(0.1f, currentAtkSpeed);
            float windupDelay = totalCycle * Mathf.Clamp(attackConfig.windupRatio, 0.05f, 0.35f);
            if (windupDelay > 0.01f)
            {
                yield return new WaitForSeconds(windupDelay);
            }

            ExecuteRangedProjectile(comboStep, direction);
        }

        /// <summary>
        /// Tạo lực dấn người nhẹ (Attack Lunge) khi vung đòn.
        /// </summary>
        private void ApplyAttackLunge(int comboStep, Vector2 direction)
        {
            if (_rb == null) return;

            float lungeSpeed = 1.8f;
            if (comboStep == 2) lungeSpeed = 2.5f;
            else if (comboStep == 3) lungeSpeed = 4.0f; // Nhát 3 vút mạnh

            _rb.velocity = direction * lungeSpeed;
        }

        /// <summary>
        /// Kích hoạt mức độ Rung màn hình và Dừng hình (HitStop) tăng dần theo nhịp Combo.
        /// </summary>
        private void TriggerDynamicGameJuice(int comboStep, bool isCrit)
        {
            float shakeDuration = 0.06f;
            float shakeStrength = 0.05f;
            float hitStopDuration = 0.03f;

            if (comboStep == 1)
            {
                shakeDuration = 0.06f;
                shakeStrength = 0.05f;
                hitStopDuration = 0.025f;
            }
            else if (comboStep == 2)
            {
                shakeDuration = 0.09f;
                shakeStrength = 0.09f;
                hitStopDuration = 0.045f;
            }
            else if (comboStep == 3) // Finisher: Đầm lực ngàn cân
            {
                shakeDuration = 0.18f;
                shakeStrength = 0.18f;
                hitStopDuration = 0.08f;
            }

            if (isCrit)
            {
                shakeStrength *= 1.4f;
                hitStopDuration += 0.02f;
            }

            GameJuiceEvents.RequestCameraShake(shakeDuration, shakeStrength);
            GameJuiceEvents.RequestHitStop(hitStopDuration);
        }

        private static GameObject _cachedHitSparksPrefab;
        private void SpawnHitImpactSparks(Vector3 hitPos)
        {
            if (_cachedHitSparksPrefab == null)
            {
                _cachedHitSparksPrefab = Resources.Load<GameObject>("Prefabs/VFX/PS_ImpactSparks");
                if (_cachedHitSparksPrefab == null)
                {
                    _cachedHitSparksPrefab = Resources.Load<GameObject>("PS_ImpactSparks");
                }
            }

            if (_cachedHitSparksPrefab != null)
            {
                GameObject sparks = Instantiate(_cachedHitSparksPrefab, hitPos, Quaternion.identity);
                Destroy(sparks, 0.6f);
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

                if (projObj.TryGetComponent<SimpleProjectile>(out var simpleProj))
                {
                    simpleProj.Initialize(damageData, gameObject, attackConfig.knockbackForce);
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

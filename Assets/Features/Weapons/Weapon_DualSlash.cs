using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared.VFX;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí đánh ra nhiều phía cùng lúc sử dụng OverlapBox kết hợp các hiệu ứng hình ảnh (VFX) nâng cao.
    /// Đã được Refactor để sử dụng GlobalVFXPoolManager và Event-Driven Game Feel.
    /// </summary>
    public class Weapon_DualSlash : Weapon_MeleeBase
    {
        [Header("Omni Slash Hitbox")]
        [SerializeField] private Vector2 hitboxSize = new Vector2(3f, 2f);
        [SerializeField] private float forwardOffset = 2f;

        [Range(1, 12)]
        [Tooltip("Số lượng hướng chém. Tăng cấp vũ khí sẽ tăng số này lên.")]
        [SerializeField] public int slashCount = 1; // Public để hệ thống nâng cấp dễ dàng can thiệp

        [Header("VFX Prefabs")]
        [SerializeField] private ParticleSystem directionalSlashPrefab;
        [SerializeField] private ParticleSystem groundDecalPrefab;
        [SerializeField] private ParticleSystem shockwavePrefab;

        private PlayerController _playerController;
        private float _overchargeTickTimer = 0f;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            _playerController = GetComponentInParent<PlayerController>();
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 12.0f;
            if (activeDuration <= 0f) activeDuration = 5.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Hỏa Long Bộc Phát";
        }

        protected override bool CanAttack()
        {
            return true; 
        }

        protected override void PerformAttack()
        {
            PerformComboAttack(CurrentComboStep);
        }

        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.ConeSector, 4.5f, 2.5f, 120f, true);

        /// <summary>
        /// Kỹ năng chủ động: Hỏa Long Bộc Phát — Kích hoạt trạng thái thần uy trong 5s: Tăng 35% tốc độ đánh, liên tục phóng ra Hỏa Long trảm quét 8 hướng và tạo vệt thiêu đốt bầy quái.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            _overchargeTickTimer = 0f;
            slashCount = Mathf.Max(6, slashCount * 2);
            PerformComboAttack(3);
            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(transform.position);

            // Bồi thêm vệt chém Hỏa Long định hướng
            if (customAimDirection != Vector2.zero)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + (Vector3)(customAimDirection * 2.5f), 3.5f, TargetingUtility.EnemyLayerMask);
                DamageData dragonDmg = new DamageData(GetFinalDamage() * 2.5f, true, ElementType.Hoa, true, this);
                foreach (var h in hits)
                {
                    if (h != null && h.TryGetComponent<IDamageable>(out var dmg))
                    {
                        dmg.TakeDamage(dragonDmg);
                    }
                }
            }
        }

        protected override void TickRelicSkillDuration()
        {
            _overchargeTickTimer += Time.deltaTime;
            if (_overchargeTickTimer >= 0.28f)
            {
                _overchargeTickTimer = 0f;
                int oldSlash = slashCount;
                slashCount = Mathf.Max(8, slashCount * 2);
                PerformComboAttack(2);
                slashCount = oldSlash;
            }
        }

        protected override void PerformComboAttack(int step)
        {
            Vector2 center = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            
            // Tính toán sát thương dựa theo bước combo (Nhát 1: 100%, Nhát 2: 120%, Nhát 3: 180%)
            float comboMultiplier = GetComboDamageMultiplier(step);
            DamageData baseDamage = CreateDamageData();
            DamageData damageData = new DamageData(
                baseDamage.Amount * comboMultiplier,
                baseDamage.IsCritical,
                baseDamage.Element,
                this
            );

            // Thông số Hitbox và Lực đẩy lùi theo bước combo
            Vector2 currentHitbox = hitboxSize;
            float knockbackForce = 4.0f;
            if (step == 2)
            {
                currentHitbox = new Vector2(hitboxSize.x * 1.25f, hitboxSize.y * 1.2f);
                knockbackForce = 5.5f;
            }
            else if (step == 3)
            {
                currentHitbox = new Vector2(hitboxSize.x * 1.5f, hitboxSize.y * 1.4f);
                knockbackForce = 8.0f;
            }

            // Thông báo cho PlayerController phát animation / hiệu ứng tương ứng
            if (_playerController != null)
            {
                _playerController.NotifyAttackStarted(step);
            }

            // Phát âm thanh vung kiếm chém gió
            global::Core.Audio.AudioManager.Instance?.PlaySlash(false, center);

            // Xác định góc gốc dựa trên hướng mặt của Player
            float baseAngle = 0f;
            if (_playerController != null && _playerController.FacingDirection < 0)
            {
                baseAngle = 180f;
            }

            float angleStep = 360f / Mathf.Max(1, slashCount);

            // Đòn Combo 3 (hoặc Level 5-6): Sinh ra vòng sóng xung kích (Shockwave) từ tâm nhân vật
            if ((step == 3 || WeaponLevel >= 5) && shockwavePrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.PlayEffect(shockwavePrefab, center, Quaternion.identity, 0.5f);
            }

            for (int i = 0; i < slashCount; i++)
            {
                float angle = baseAngle + (i * angleStep);
                float rad = angle * Mathf.Deg2Rad;
                
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 hitCenter = center + (direction * forwardOffset);

                // Gây sát thương (kế thừa từ Weapon_MeleeBase tự phát Hit Sparks và Game Feel)
                DealDamageInArea(hitCenter, currentHitbox, angle, damageData, knockbackForce);

                // Sinh ra vệt chém theo hướng từ Global Pool
                if (directionalSlashPrefab != null && GlobalVFXPoolManager.Instance != null)
                {
                    var slash = GlobalVFXPoolManager.Instance.PlayEffect(
                        directionalSlashPrefab, 
                        hitCenter, 
                        Quaternion.Euler(0, 0, angle), 
                        0.25f,
                        Vector3.one * (GetFinalScale() * (step == 3 ? 1.3f : 1.0f))
                    );
                    if (slash != null)
                    {
                        ConfigureSlashVFX(slash, WeaponLevel);
                    }
                }

                // Để lại vệt xém đất mờ dần từ Global Pool
                if (groundDecalPrefab != null && GlobalVFXPoolManager.Instance != null)
                {
                    GlobalVFXPoolManager.Instance.PlayEffect(
                        groundDecalPrefab, 
                        hitCenter, 
                        Quaternion.Euler(0, 0, angle), 
                        0.6f, 
                        Vector3.one * GetFinalScale()
                    );
                }
            }

            // Phát hiệu ứng tĩnh phụ nếu không có prefab vệt chém đa hướng
            if (directionalSlashPrefab == null)
            {
                PlaySlashVFX();
            }
        }

        private void ConfigureSlashVFX(ParticleSystem ps, int level)
        {
            var main = ps.main;
            Color targetColor;
            float sizeMultiplier = GetFinalScale();

            if (level <= 2)
            {
                // Level 1-2: Xanh Neon thanh mảnh
                ColorUtility.TryParseHtmlString("#00FF66", out targetColor);
            }
            else if (level <= 4)
            {
                // Level 3-4: Cam đỏ lửa hoành tráng
                ColorUtility.TryParseHtmlString("#FF4500", out targetColor);
            }
            else
            {
                // Level 5-6: Doom Purple quyền lực
                ColorUtility.TryParseHtmlString("#8A2BE2", out targetColor);
                sizeMultiplier *= 1.25f;
            }

            main.startColor = targetColor;
            ps.transform.localScale = Vector3.one * sizeMultiplier;
        }

        public override void ApplyStatModifier(Upgrades.WeaponStatModifier modifier)
        {
            base.ApplyStatModifier(modifier);

            // Level 1-2: 1 nhát chém (hướng trước mặt)
            // Level 3-4: 2 nhát chém (trước & sau)
            // Level 5-6: 4 nhát chém (tỏa 4 hướng chữ thập)
            if (WeaponLevel >= 5)
            {
                slashCount = 4;
            }
            else if (WeaponLevel >= 3)
            {
                slashCount = 2;
            }
            else
            {
                slashCount = 1;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Vector2 center = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            
            float angleStep = 360f / Mathf.Max(1, slashCount);

            for (int i = 0; i < slashCount; i++)
            {
                float angle = i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 hitCenter = center + (direction * forwardOffset);

                Gizmos.DrawSphere(hitCenter, 0.3f);
                Gizmos.DrawWireCube(hitCenter, hitboxSize);
            }
        }
    }
}


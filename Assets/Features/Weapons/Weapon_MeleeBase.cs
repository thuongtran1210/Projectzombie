using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Core.Juice;
using ProjectZombie.Features.Shared.VFX;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Lớp cơ sở cho các vũ khí cận chiến (Melee).
    /// Tối ưu hóa bằng cách dùng Physics2D.OverlapBox thay vì Spawn Projectile.
    /// </summary>
    public abstract class Weapon_MeleeBase : WeaponBase
    {
        [Header("Melee Settings")]
        [Tooltip("Hiệu ứng vệt chém (Gắn Particle System vào đây)")]
        [SerializeField] protected ParticleSystem slashParticles;
        
        [Tooltip("Số lượng quái tối đa chém trúng trong 1 nhát. (Để 0 = vô hạn)")]
        [SerializeField] protected int maxTargetsHit = 0;

        [Header("Hit Impact & Game Feel")]
        [SerializeField] protected ParticleSystem hitSparkPrefab;

        [Header("Debug")]
        [SerializeField] protected bool showGizmos = true;

        // Khởi tạo một mảng tĩnh tái sử dụng cho tất cả các đòn Melee để giảm thiểu GC Allocation
        private static readonly Collider2D[] _hitBuffer = new Collider2D[50];

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            SetupParticles();
        }

        protected virtual void SetupParticles()
        {
            if (slashParticles != null)
            {
                var main = slashParticles.main;
                main.playOnAwake = false;
                slashParticles.Stop();
            }
        }

        public virtual float GetComboDamageMultiplier(int step)
        {
            switch (step)
            {
                case 2: return 1.2f;
                case 3: return 1.8f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// Quét vùng hình chữ nhật để gây sát thương. Không cần Spawn đạn.
        /// Tối ưu 0 GC Allocation và lọc bằng LayerMask ở tầng C++ Physics.
        /// </summary>
        protected void DealDamageInArea(Vector2 center, Vector2 boxSize, float angle, DamageData damageData, float knockbackForce = 4.0f)
        {
            int mask = TargetingUtility.EnemyLayerMask;
            int numHits = Physics2D.OverlapBoxNonAlloc(center, boxSize, angle, _hitBuffer, mask);
            int hitCount = 0;
            bool hitAnyEnemy = false;
            bool hitCrit = false;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _hitBuffer[i];
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
                        this
                    );

                    health.TakeDamage(hitDamage);
                    hitCount++;
                    hitAnyEnemy = true;

                    // Phát sự kiện trúng đích cho các Pháp bảo (Relics) On-Hit
                    NotifyHitEnemy(hitDamage, hit);

                    // Áp dụng lực đẩy lùi ra xa người chơi (trừ quái Heavy Armor)
                    if (enemy != null && !enemy.IsHeavyArmor)
                    {
                        Vector2 pushDir = ((Vector2)(hit.transform.position - transform.position)).normalized;
                        if (pushDir.sqrMagnitude < 0.001f) pushDir = Vector2.right;
                        enemy.ApplyKnockback(pushDir, knockbackForce, 0.15f);
                    }

                    if (hitDamage.IsCritical)
                    {
                        hitCrit = true;
                    }

                    // Kích hoạt Vòng Tương Sinh (Element Generation)
                    if (damageData.Element != ElementType.None && YinYang.ElementCycleManager.Instance != null)
                    {
                        YinYang.ElementCycleManager.Instance.RegisterHit(damageData.Element, this);
                    }

                    // Sinh tóe lửa (Hit Sparks) tại vị trí quái vật
                    if (hitSparkPrefab != null && GlobalVFXPoolManager.Instance != null)
                    {
                        GlobalVFXPoolManager.Instance.PlayEffect(hitSparkPrefab, hit.transform.position, Quaternion.identity, 0.4f);
                    }
                    
                    // Nếu có giới hạn số lượng mục tiêu thì dừng lại
                    if (maxTargetsHit > 0 && hitCount >= maxTargetsHit)
                    {
                        break;
                    }
                }
            }

            if (hitAnyEnemy)
            {
                TriggerHitImpact(hitCrit);
            }
        }

        /// <summary>
        /// Phát tín hiệu Game Feel (Rung màn hình, Hit Stop & Âm thanh chém/bạo kích) thông qua Event Hub.
        /// </summary>
        protected void TriggerHitImpact(bool isCritical)
        {
            global::Core.Audio.AudioManager.Instance?.PlaySlash(isCritical, transform.position);

            if (isCritical)
            {
                GameJuiceEvents.RequestCameraShake(0.15f, 0.15f);
                GameJuiceEvents.RequestHitStop(0.05f);
            }
            else
            {
                GameJuiceEvents.RequestCameraShake(0.08f, 0.04f);
            }
        }

        protected void PlaySlashVFX()
        {
            global::Core.Audio.AudioManager.Instance?.PlaySlash(false, transform.position);

            if (slashParticles != null)
            {
                slashParticles.Stop(); // Reset lại nếu đang play dở
                slashParticles.Play();
            }
        }
    }
}


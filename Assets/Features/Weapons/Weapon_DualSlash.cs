using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí đánh ra nhiều phía cùng lúc sử dụng OverlapBox kết hợp các hiệu ứng hình ảnh (VFX) nâng cao.
    /// </summary>
    public class Weapon_DualSlash : Weapon_MeleeBase
    {
        [Header("Omni Slash Hitbox")]
        [SerializeField] private Vector2 hitboxSize = new Vector2(3f, 2f);
        [SerializeField] private float forwardOffset = 2f;

        [Range(1, 12)]
        [Tooltip("Số lượng hướng chém. Tăng cấp vũ khí sẽ tăng số này lên.")]
        [SerializeField] public int slashCount = 2; // Public để hệ thống nâng cấp dễ dàng can thiệp

        [Header("VFX Suggestions Settings")]
        [SerializeField] private ParticleSystem directionalSlashPrefab;
        [SerializeField] private ParticleSystem hitSparkPrefab;
        [SerializeField] private ParticleSystem groundDecalPrefab;
        [SerializeField] private ParticleSystem shockwavePrefab;

        private PlayerController _playerController;

        // Object Pools để tối ưu hiệu năng và triệt tiêu GC Allocation
        private ObjectPool<ParticleSystem> _slashVfxPool;
        private ObjectPool<ParticleSystem> _hitSparkPool;
        private ObjectPool<ParticleSystem> _decalPool;
        private ObjectPool<ParticleSystem> _shockwavePool;

        private static readonly Collider2D[] _localHitBuffer = new Collider2D[50];

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            _playerController = GetComponentInParent<PlayerController>();
            InitializePools();
        }

        private void InitializePools()
        {
            if (directionalSlashPrefab != null)
            {
                _slashVfxPool = new ObjectPool<ParticleSystem>(
                    createFunc: () => Instantiate(directionalSlashPrefab, transform),
                    actionOnGet: ps => { ps.gameObject.SetActive(true); ps.Play(); },
                    actionOnRelease: ps => { ps.Stop(); ps.gameObject.SetActive(false); },
                    actionOnDestroy: ps => Destroy(ps.gameObject),
                    defaultCapacity: 10,
                    maxSize: 30
                );
            }
            if (hitSparkPrefab != null)
            {
                _hitSparkPool = new ObjectPool<ParticleSystem>(
                    createFunc: () => Instantiate(hitSparkPrefab, transform),
                    actionOnGet: ps => { ps.gameObject.SetActive(true); ps.Play(); },
                    actionOnRelease: ps => { ps.Stop(); ps.gameObject.SetActive(false); },
                    actionOnDestroy: ps => Destroy(ps.gameObject),
                    defaultCapacity: 15,
                    maxSize: 40
                );
            }
            if (groundDecalPrefab != null)
            {
                _decalPool = new ObjectPool<ParticleSystem>(
                    createFunc: () => Instantiate(groundDecalPrefab, transform),
                    actionOnGet: ps => { ps.gameObject.SetActive(true); ps.Play(); },
                    actionOnRelease: ps => { ps.Stop(); ps.gameObject.SetActive(false); },
                    actionOnDestroy: ps => Destroy(ps.gameObject),
                    defaultCapacity: 10,
                    maxSize: 30
                );
            }
            if (shockwavePrefab != null)
            {
                _shockwavePool = new ObjectPool<ParticleSystem>(
                    createFunc: () => Instantiate(shockwavePrefab, transform),
                    actionOnGet: ps => { ps.gameObject.SetActive(true); ps.Play(); },
                    actionOnRelease: ps => { ps.Stop(); ps.gameObject.SetActive(false); },
                    actionOnDestroy: ps => Destroy(ps.gameObject),
                    defaultCapacity: 5,
                    maxSize: 10
                );
            }
        }

        protected override bool CanAttack()
        {
            return true; 
        }

        protected override void PerformAttack()
        {
            Vector2 center = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            DamageData damageData = DamageUtility.CalculateDamage(CharacterStats.GetTotalDamage(), CharacterStats.CritChance);

            // Xác định góc gốc dựa trên hướng mặt của Player
            float baseAngle = 0f;
            if (_playerController != null && _playerController.transform.localScale.x < 0)
            {
                baseAngle = 180f;
            }

            float angleStep = 360f / Mathf.Max(1, slashCount);

            // Level 5 - 6: Sinh ra vòng sóng xung kích (Shockwave) từ tâm nhân vật
            if (WeaponLevel >= 5 && _shockwavePool != null && shockwavePrefab != null)
            {
                var sw = _shockwavePool.Get();
                sw.transform.position = center;
                ReturnParticleToPool(_shockwavePool, sw, 0.5f);
            }

            for (int i = 0; i < slashCount; i++)
            {
                float angle = baseAngle + (i * angleStep);
                float rad = angle * Mathf.Deg2Rad;
                
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 hitCenter = center + (direction * forwardOffset);

                // Gây sát thương với các hiệu ứng va chạm (Sparks, Hit Stop, Screen Shake)
                DealDamageInAreaWithEffects(hitCenter, hitboxSize, angle, damageData);

                // Sinh ra vệt chém theo hướng từ pool
                if (_slashVfxPool != null && directionalSlashPrefab != null)
                {
                    var slash = _slashVfxPool.Get();
                    slash.transform.position = hitCenter;
                    slash.transform.rotation = Quaternion.Euler(0, 0, angle);
                    ConfigureSlashVFX(slash, WeaponLevel);
                    ReturnParticleToPool(_slashVfxPool, slash, 0.25f);
                }

                // Để lại vệt xém đất mờ dần
                if (_decalPool != null && groundDecalPrefab != null)
                {
                    var decal = _decalPool.Get();
                    decal.transform.position = hitCenter;
                    decal.transform.rotation = Quaternion.Euler(0, 0, angle);
                    decal.transform.localScale = Vector3.one * GetFinalScale();
                    ReturnParticleToPool(_decalPool, decal, 0.6f);
                }
            }

            // Phát hiệu ứng tĩnh phụ (nếu có)
            PlaySlashVFX();
        }

        private void DealDamageInAreaWithEffects(Vector2 center, Vector2 boxSize, float angle, DamageData damageData)
        {
            int numHits = Physics2D.OverlapBoxNonAlloc(center, boxSize, angle, _localHitBuffer);
            int hitCount = 0;
            bool hitAnyEnemy = false;
            bool hitCrit = false;

            for (int i = 0; i < numHits; i++)
            {
                var hit = _localHitBuffer[i];
                if (hit.CompareTag("Enemy"))
                {
                    var health = hit.GetComponent<HealthSystem>();
                    if (health != null && health.CurrentHealth > 0)
                    {
                        health.TakeDamage(damageData);
                        hitCount++;
                        hitAnyEnemy = true;
                        if (damageData.IsCritical)
                        {
                            hitCrit = true;
                        }

                        // Sinh tóe lửa (Hit Sparks) tại vị trí quái vật
                        if (_hitSparkPool != null && hitSparkPrefab != null)
                        {
                            var spark = _hitSparkPool.Get();
                            spark.transform.position = hit.transform.position;
                            ReturnParticleToPool(_hitSparkPool, spark, 0.4f);
                        }

                        if (maxTargetsHit > 0 && hitCount >= maxTargetsHit)
                        {
                            break;
                        }
                    }
                }
            }

            // Xử lý game feel (Rung Camera và Khựng hình)
            if (hitAnyEnemy)
            {
                if (hitCrit)
                {
                    TriggerCameraShake(0.15f, 0.15f);
                    StartCoroutine(HitStopCoroutine(0.05f));
                }
                else
                {
                    TriggerCameraShake(0.08f, 0.04f);
                }
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

        private void TriggerCameraShake(float duration, float magnitude)
        {
            var camFollow = Camera.main != null ? Camera.main.GetComponent<ProjectZombie.Features.Arena.CameraFollow>() : null;
            if (camFollow != null)
            {
                camFollow.TriggerShake(duration, magnitude);
            }
        }

        private System.Collections.IEnumerator HitStopCoroutine(float duration)
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1.0f;
        }

        private void ReturnParticleToPool(ObjectPool<ParticleSystem> pool, ParticleSystem ps, float delay)
        {
            StartCoroutine(ReturnParticleToPoolCoroutine(pool, ps, delay));
        }

        private System.Collections.IEnumerator ReturnParticleToPoolCoroutine(ObjectPool<ParticleSystem> pool, ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (ps != null)
            {
                pool.Release(ps);
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

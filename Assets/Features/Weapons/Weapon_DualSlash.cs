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
        [SerializeField] public int slashCount = 2; // Public để hệ thống nâng cấp dễ dàng can thiệp

        [Header("VFX Prefabs")]
        [SerializeField] private ParticleSystem directionalSlashPrefab;
        [SerializeField] private ParticleSystem groundDecalPrefab;
        [SerializeField] private ParticleSystem shockwavePrefab;

        private PlayerController _playerController;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            _playerController = GetComponentInParent<PlayerController>();
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
            if (WeaponLevel >= 5 && shockwavePrefab != null && GlobalVFXPoolManager.Instance != null)
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
                DealDamageInArea(hitCenter, hitboxSize, angle, damageData);

                // Sinh ra vệt chém theo hướng từ Global Pool
                if (directionalSlashPrefab != null && GlobalVFXPoolManager.Instance != null)
                {
                    var slash = GlobalVFXPoolManager.Instance.PlayEffect(
                        directionalSlashPrefab, 
                        hitCenter, 
                        Quaternion.Euler(0, 0, angle), 
                        0.25f
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

            // Phát hiệu ứng tĩnh phụ (nếu có)
            PlaySlashVFX();
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


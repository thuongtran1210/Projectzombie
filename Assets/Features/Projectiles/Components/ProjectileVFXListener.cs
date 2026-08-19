using UnityEngine;
using ProjectZombie.Features.Projectiles.Core;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Shared.VFX;

namespace ProjectZombie.Features.Projectiles.Components
{
    /// <summary>
    /// Component lắng nghe các sự kiện của hệ thống Đạn (ProjectileSystem EventDispatcher) 
    /// để tự động phát hiệu ứng hình ảnh (VFX) và âm thanh (SFX).
    /// Tách biệt hoàn toàn 100% logic hiển thị khỏi logic tính toán sát thương.
    /// </summary>
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileVFXListener : MonoBehaviour
    {
        private ProjectileController _controller;

        private void Awake()
        {
            _controller = GetComponent<ProjectileController>();
        }

        private void OnEnable()
        {
            if (ProjectileSystem.Instance != null && ProjectileSystem.Instance.EventDispatcher != null)
            {
                ProjectileSystem.Instance.EventDispatcher.OnProjectileSpawned += HandleSpawnVFX;
                ProjectileSystem.Instance.EventDispatcher.OnProjectileHit += HandleHitVFX;
                ProjectileSystem.Instance.EventDispatcher.OnProjectileDespawned += HandleDespawnVFX;
            }
        }

        private void OnDisable()
        {
            if (ProjectileSystem.Instance != null && ProjectileSystem.Instance.EventDispatcher != null)
            {
                ProjectileSystem.Instance.EventDispatcher.OnProjectileSpawned -= HandleSpawnVFX;
                ProjectileSystem.Instance.EventDispatcher.OnProjectileHit -= HandleHitVFX;
                ProjectileSystem.Instance.EventDispatcher.OnProjectileDespawned -= HandleDespawnVFX;
            }
        }

        private void HandleSpawnVFX(ProjectileController projectile)
        {
            if (_controller == null) _controller = GetComponent<ProjectileController>();
            if (projectile != _controller || _controller == null || _controller.Data == null) return;

            ref readonly var vfx = ref _controller.Data.VFXConfig;
            if (vfx.SpawnVFXPrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.PlayEffect(vfx.SpawnVFXPrefab, transform.position, transform.rotation, 0.4f);
            }
        }

        private void HandleHitVFX(ProjectileEventContext context)
        {
            if (_controller == null) _controller = GetComponent<ProjectileController>();
            if (_controller == null || context.Projectile != _controller || _controller.Data == null) return;

            ref readonly var vfx = ref _controller.Data.VFXConfig;
            if (vfx.HitImpactVFXPrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                Quaternion rotation = context.HitNormal != Vector2.zero 
                    ? Quaternion.LookRotation(Vector3.forward, context.HitNormal) 
                    : transform.rotation;

                GlobalVFXPoolManager.Instance.PlayEffect(
                    vfx.HitImpactVFXPrefab, 
                    context.HitPoint != Vector2.zero ? (Vector3)context.HitPoint : transform.position, 
                    rotation, 
                    0.85f
                );
            }
        }

        private void HandleDespawnVFX(ProjectileController projectile)
        {
            if (_controller == null) _controller = GetComponent<ProjectileController>();
            if (projectile != _controller || _controller == null || _controller.Data == null) return;

            ref readonly var vfx = ref _controller.Data.VFXConfig;
            if (vfx.DespawnVFXPrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.PlayEffect(vfx.DespawnVFXPrefab, transform.position, transform.rotation, 0.85f);
            }
        }
    }
}

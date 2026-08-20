using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Projectiles.Core;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    /// <summary>
    /// Hành vi gây sát thương định kỳ theo vùng (Periodic Zone / AoE DoT):
    /// - Quét vùng định kỳ mỗi hitCooldown giây (0 GC Allocation qua static buffer).
    /// - Gây sát thương chuẩn Ngũ Hành lên toàn bộ kẻ địch trong bán kính.
    /// - Áp dụng hiệu ứng Làm Chậm (Slow Debuff) lên quái vật đi vào vùng.
    /// </summary>
    public class PeriodicHitBehavior : IProjectileBehavior
    {
        private readonly ProjectileController _controller;
        private readonly PeriodicHitBehaviorData _data;

        private float _tickTimer;
        private static readonly Collider2D[] _scanBuffer = new Collider2D[64];

        public PeriodicHitBehavior(ProjectileController controller, PeriodicHitBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            _tickTimer = 0f;
        }

        public void OnUpdate()
        {
            if (_controller == null || _controller.Data == null) return;

            float interval = _data != null ? _data.hitCooldown : 0.6f;
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= interval)
            {
                _tickTimer = 0f;
                PerformZoneTick();
            }
        }

        private void PerformZoneTick()
        {
            float radius = _controller.Data.CollisionRadius * _controller.State.ScaleMultiplier;
            int hitLayerMask = _controller.Data.HitLayer.value != 0 ? _controller.Data.HitLayer.value : LayerMask.GetMask("Enemy");

            int hitCount = Physics2D.OverlapCircleNonAlloc(_controller.transform.position, radius, _scanBuffer, hitLayerMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _scanBuffer[i];
                if (col == null) continue;

                // Bỏ qua Player và Owner
                if (col.CompareTag("Player") || col.transform.root.CompareTag("Player")) continue;
                if (_controller.Owner != null && (col.gameObject == _controller.Owner || col.transform.root == _controller.Owner.transform.root)) continue;

                // 1. Gây sát thương DoT
                IDamageable damageable = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    ElementType defenderElement = ElementType.None;
                    Enemy enemy = col.GetComponent<Enemy>() ?? col.GetComponentInParent<Enemy>();
                    if (enemy != null)
                    {
                        defenderElement = enemy.CurrentElement;
                    }

                    DamageData tickDamage = DamageUtility.CalculateHitDamage(
                        _controller.Damage.BaseDamage,
                        _controller.Damage.IsCritical,
                        _controller.Damage.Element,
                        defenderElement,
                        _controller.Damage.SourceWeapon
                    );

                    damageable.TakeDamage(tickDamage);

                    // 2. Kích hoạt Làm Chậm (Slow Debuff)
                    if (enemy != null && _data != null && _data.slowPercentage > 0f)
                    {
                        var statusController = enemy.GetComponent<EnemyStatusController>();
                        if (statusController != null)
                        {
                            statusController.ApplyStatusEffect(StatusEffectType.Slow, _data.slowDuration, _data.slowPercentage);
                        }
                    }

                    // 3. Tương Sinh Ngũ Hành
                    if (_controller.Damage.Element != ElementType.None && YinYang.ElementCycleManager.Instance != null)
                    {
                        var weapon = _controller.Damage.SourceWeapon as Weapons.WeaponBase;
                        YinYang.ElementCycleManager.Instance.RegisterHit(_controller.Damage.Element, weapon);
                    }
                }
            }
        }

        public BehaviorHitResult OnHit(ProjectileEventContext context)
        {
            // Vùng nước thánh giữ sống liên tục trong suốt Lifetime, không bị despawn khi quái chạm
            return BehaviorHitResult.KeepAlive;
        }

        public void OnDespawn()
        {
            _tickTimer = 0f;
        }
    }
}

using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Projectiles.Components
{
    public class ProjectileCollision : MonoBehaviour
    {
        private ProjectileController _controller;
        private bool _isInitialized;

        private Vector2 _lastPosition;

        public void Initialize(ProjectileController controller)
        {
            _controller = controller;
            _isInitialized = true;
            _lastPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (_isInitialized)
            {
                _lastPosition = transform.position;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isInitialized || _controller == null) return;

            // Verify we are not hitting the owner (Người chơi bắn ra không tự bắn trúng mình)
            if (collision.gameObject == _controller.Owner) return;

            int hitLayerMask = _controller.Data != null ? _controller.Data.HitLayer.value : 0;
            bool isLayerMatch = hitLayerMask != 0 && (((1 << collision.gameObject.layer) & hitLayerMask) != 0);
            bool isTargetDamageable = collision.TryGetComponent(out IDamageable target) || collision.CompareTag("Enemy");

            // Tự động nhận diện va chạm hợp lệ: hoặc khớp LayerMask, hoặc đối tượng có IDamageable / Tag Enemy
            if (isLayerMatch || isTargetDamageable)
            {
                // Calculate HitPoint and HitNormal using a short Raycast
                Vector2 currentPos = transform.position;
                Vector2 dir = (currentPos - _lastPosition).normalized;
                float dist = Vector2.Distance(_lastPosition, currentPos) + 0.1f;

                // Fallback values
                Vector2 hitPoint = currentPos;
                Vector2 hitNormal = -dir;

                if (hitLayerMask != 0)
                {
                    RaycastHit2D hit = Physics2D.Raycast(_lastPosition, dir, dist, hitLayerMask);
                    if (hit.collider != null && hit.collider == collision)
                    {
                        hitPoint = hit.point;
                        hitNormal = hit.normal;
                    }
                }

                Core.ProjectileEventContext context = new Core.ProjectileEventContext(_controller, collision, hitPoint, hitNormal);

                if (collision.TryGetComponent(out IDamageable damageableTarget))
                {
                    ElementType defenderElement = ElementType.None;
                    if (collision.TryGetComponent(out Enemies.Enemy enemy))
                    {
                        defenderElement = enemy.CurrentElement;
                    }

                    // Tính toán sát thương tương khắc 1 chiều
                    DamageData hitDamage = DamageUtility.CalculateHitDamage(
                        _controller.Damage.BaseDamage,
                        _controller.Damage.IsCritical,
                        _controller.Damage.Element,
                        defenderElement,
                        _controller.Damage.SourceWeapon
                    );

                    damageableTarget.TakeDamage(hitDamage);

                    // Áp dụng lực đẩy lùi theo hướng bay của đạn (trừ quái Heavy Armor)
                    if (enemy != null && !enemy.IsHeavyArmor)
                    {
                        Vector2 pushDir = _controller.CurrentDirection.sqrMagnitude > 0.01f 
                            ? _controller.CurrentDirection 
                            : ((Vector2)(collision.transform.position - transform.position)).normalized;
                        enemy.ApplyKnockback(pushDir, 2.5f, 0.12f);
                    }

                    // Kích hoạt Vòng Tương Sinh (Element Generation)
                    if (_controller.Damage.Element != ElementType.None && YinYang.ElementCycleManager.Instance != null)
                    {
                        var weapon = _controller.Damage.SourceWeapon as Weapons.WeaponBase;
                        YinYang.ElementCycleManager.Instance.RegisterHit(_controller.Damage.Element, weapon);
                    }
                }

                _controller.HandleHit(context);
            }
        }
    }
}

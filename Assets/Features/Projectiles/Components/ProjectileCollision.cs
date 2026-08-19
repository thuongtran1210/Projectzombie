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
            if (!_isInitialized || _controller == null || collision == null) return;

            // 1. Tuyệt đối không tự va chạm với chính Owner hoặc các bộ phận con/cha của Owner
            if (_controller.Owner != null)
            {
                if (collision.gameObject == _controller.Owner || 
                    collision.transform.root == _controller.Owner.transform.root)
                {
                    return;
                }
            }

            // 2. Xác định phe phát đạn (Player hay Enemy)
            bool isPlayerSource = true;
            if (_controller.Owner != null)
            {
                if (_controller.Owner.CompareTag("Enemy") || _controller.Owner.GetComponent<Enemies.Enemy>() != null)
                {
                    isPlayerSource = false;
                }
            }

            // 3. Phân luồng lọc va chạm theo phe để triệt tiêu việc tự gây sát thương
            if (isPlayerSource)
            {
                // Đạn của Pháp Bảo (Player) -> TUYỆT ĐỐI KHÔNG GÂY SÁT THƯƠNG CHO PLAYER
                if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
                {
                    return;
                }

                // Chỉ nhận va chạm nếu là Enemy hoặc có IDamageable (khác Player)
                bool isEnemy = collision.CompareTag("Enemy") || 
                               collision.GetComponent<Enemies.Enemy>() != null ||
                               collision.gameObject.layer == LayerMask.NameToLayer("Enemy");

                if (!isEnemy && !collision.TryGetComponent(out IDamageable _))
                {
                    return;
                }
            }
            else
            {
                // Đạn của Quái vật -> TUYỆT ĐỐI KHÔNG GÂY SÁT THƯƠNG CHO QUÁI VẬT KHÁC
                if (collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy"))
                {
                    return;
                }

                // Chỉ nhận va chạm nếu là Player
                if (!collision.CompareTag("Player") && !collision.transform.root.CompareTag("Player"))
                {
                    return;
                }
            }

            // 4. Tính toán HitPoint và HitNormal
            Vector2 currentPos = transform.position;
            Vector2 dir = (currentPos - _lastPosition).normalized;
            float dist = Vector2.Distance(_lastPosition, currentPos) + 0.1f;

            Vector2 hitPoint = currentPos;
            Vector2 hitNormal = -dir;

            int hitLayerMask = _controller.Data != null ? _controller.Data.HitLayer.value : 0;
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

            // 5. Gây sát thương lên mục tiêu
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

            // 6. Xử lý logic va chạm đặc thù (Xuyên thấu, Nảy, Homing,...)
            _controller.HandleHit(context);
        }
    }
}

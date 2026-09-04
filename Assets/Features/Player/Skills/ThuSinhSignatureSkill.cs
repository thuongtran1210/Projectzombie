using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.YinYang;
using ProjectZombie.Core.Juice;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Chủ động Thư Sinh: "Phán Quyết Tiền Định" / "Phán Quyết Âm Ty" (GDD v5.1).
    /// Thi triển:
    /// 1. Triệu hồi Trận Địa Cổ Tự Thư Pháp (Ground Decal) & 4 Vệt Mực Xoáy (Ink Slash).
    /// 2. Gây 250% Sát thương Phép hệ Kim/Thủy diện rộng (6.0m) + Hất lùi nhẹ.
    /// 3. Giảm ngay 20% thời gian hồi chiêu của toàn bộ Vũ Khí / Pháp Bảo.
    /// 4. Đẩy 1 hit ảo Ngũ Hành vào buffer Tương Sinh.
    /// 5. Nhận buff +30% Tốc đánh & +20% Tốc chạy trong 4.0s (kèm tàn ảnh hoàng kim).
    /// </summary>
    public class ThuSinhSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 25.0f;

        private readonly GameObject _groundDecalPrefab;
        private readonly GameObject _inkSlashPrefab;
        private readonly GameObject _lightningPrefab;

        private const float AOE_RADIUS = 6.0f;
        private const float DAMAGE_RATIO = 2.5f; // 250% Base Damage
        private const float BUFF_DURATION = 4.0f;
        private const float ATTACK_SPEED_BONUS = 0.30f;
        private const float MOVE_SPEED_BONUS = 0.20f;
        private const float WEAPON_COOLDOWN_REDUCTION_RATIO = 0.20f; // -20% Weapon CD

        private static readonly Collider2D[] _hitBuffer = new Collider2D[80];

        public ThuSinhSignatureSkill(GameObject groundDecalPrefab = null, GameObject inkSlashPrefab = null, GameObject lightningPrefab = null)
        {
            _groundDecalPrefab = groundDecalPrefab;
            _inkSlashPrefab = inkSlashPrefab;
            _lightningPrefab = lightningPrefab;
        }

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            Vector3 center = playerObj.transform.position;

            // 1. Sinh Hiệu Ứng VFX Trận Địa Cổ Tự & Mực Thư Pháp
            SpawnVisualEffects(center, playerObj.transform);

            // 2. Quét Sát Thương Diện Rộng (AOE Sát Thương Kim/Thủy)
            ExecuteInkExplosionDamage(playerObj, center);

            // 3. Giảm 20% Cooldown tất cả vũ khí/pháp bảo đang mang
            ReduceAllWeaponsCooldown(playerObj);

            // 4. Đẩy phần tử ảo vào Vòng Tương Sinh Ngũ Hành
            ElementType fallbackElement = GetAutoSelectFallbackElement(playerObj);
            ApplyVirtualElementHit(fallbackElement);
            onElementSelectedCallback?.Invoke(fallbackElement);

            // 5. Rung Camera & Âm thanh Phán Quyết
            GameJuiceEvents.RequestCameraShake(0.18f, 0.35f);

            // 6. Kích hoạt Coroutine Buff Tốc Đánh & Tốc Chạy (4s)
            var playerMono = playerObj.GetComponent<MonoBehaviour>();
            if (playerMono != null)
            {
                playerMono.StartCoroutine(ScholarBuffRoutine(playerObj));
            }
        }

        private void SpawnVisualEffects(Vector3 center, Transform parent)
        {
            // Trận Địa Thư Pháp Dưới Chân
            if (_groundDecalPrefab != null)
            {
                Object.Instantiate(_groundDecalPrefab, center, Quaternion.identity);
            }

            // Vệt Mực Xoáy 4 Hướng
            if (_inkSlashPrefab != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 90f;
                    Quaternion rot = Quaternion.Euler(0f, 0f, angle);
                    Object.Instantiate(_inkSlashPrefab, center, rot);
                }
            }

            // Sét Phán Quan nếu có Prefab
            if (_lightningPrefab != null)
            {
                Object.Instantiate(_lightningPrefab, center, Quaternion.identity);
            }
        }

        private void ExecuteInkExplosionDamage(GameObject playerObj, Vector3 center)
        {
            var playerStats = playerObj.GetComponent<PlayerStats>();
            float baseDmg = playerStats != null ? playerStats.GetTotalDamage() : 20f;
            float totalDamage = baseDmg * DAMAGE_RATIO;

            int mask = TargetingUtility.EnemyLayerMask;
            int count = Physics2D.OverlapCircleNonAlloc(center, AOE_RADIUS, _hitBuffer, mask);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || col.gameObject == playerObj) continue;

                if (col.TryGetComponent<HealthSystem>(out var enemyHealth))
                {
                    DamageData dmg = new DamageData(
                        totalDamage, 
                        isCritical: false, 
                        element: ElementType.Kim, 
                        isCounter: false
                    );
                    enemyHealth.TakeDamage(dmg);
                }

                if (col.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 pushDir = ((Vector2)col.transform.position - (Vector2)center).normalized;
                    if (pushDir == Vector2.zero) pushDir = Vector2.up;
                    rb.AddForce(pushDir * 3.5f, ForceMode2D.Impulse);
                }
            }
        }

        private void ReduceAllWeaponsCooldown(GameObject playerObj)
        {
            var weaponManager = playerObj.GetComponent<WeaponManager>();
            if (weaponManager == null || weaponManager.ActiveWeapons == null) return;

            foreach (var weapon in weaponManager.ActiveWeapons)
            {
                if (weapon != null)
                {
                    weapon.ReduceCurrentCooldown(WEAPON_COOLDOWN_REDUCTION_RATIO);
                }
            }
        }

        private IEnumerator ScholarBuffRoutine(GameObject playerObj)
        {
            if (playerObj == null) yield break;

            var stats = playerObj.GetComponent<PlayerStats>();
            float atkSpeedAdd = 0f;
            float moveSpeedAdd = 0f;

            if (stats != null)
            {
                atkSpeedAdd = stats.AttackSpeed * ATTACK_SPEED_BONUS;
                moveSpeedAdd = stats.MoveSpeed * MOVE_SPEED_BONUS;
                stats.AddAttackSpeed(atkSpeedAdd);
                stats.AddMoveSpeed(moveSpeedAdd);
            }

            // Kích hoạt tàn ảnh hoàng kim thư pháp
            var dashVisuals = playerObj.GetComponent<Visuals.PlayerDashVisuals>();
            if (dashVisuals != null)
            {
                dashVisuals.StartSpeedBuffVisual(BUFF_DURATION, new Color(1f, 0.85f, 0.2f, 0.6f));
            }

            yield return new WaitForSeconds(BUFF_DURATION);

            if (stats != null)
            {
                if (atkSpeedAdd > 0f) stats.AddAttackSpeed(-atkSpeedAdd);
                if (moveSpeedAdd > 0f) stats.AddMoveSpeed(-moveSpeedAdd);
            }
        }

        /// <summary>
        /// Tự động chọn thuộc tính Ngũ Hành khớp với vũ khí đang có Cooldown hồi chiêu lâu nhất.
        /// </summary>
        public ElementType GetAutoSelectFallbackElement(GameObject playerObj)
        {
            var weaponManager = playerObj.GetComponent<WeaponManager>();
            if (weaponManager == null || weaponManager.ActiveWeapons == null || weaponManager.ActiveWeapons.Count == 0)
            {
                return ElementType.Kim;
            }

            WeaponBase longestCdWeapon = null;
            float maxRemainingCd = -1f;

            foreach (var weapon in weaponManager.ActiveWeapons)
            {
                if (weapon == null) continue;
                float rem = weapon.RemainingCooldown;
                if (rem > maxRemainingCd)
                {
                    maxRemainingCd = rem;
                    longestCdWeapon = weapon;
                }
            }

            if (longestCdWeapon != null && longestCdWeapon.element != ElementType.None)
            {
                return longestCdWeapon.element;
            }

            return ElementType.Kim;
        }

        /// <summary>
        /// Đẩy phần tử ảo vào ElementCycleManager.
        /// </summary>
        public void ApplyVirtualElementHit(ElementType selectedElement)
        {
            if (selectedElement == ElementType.None)
            {
                selectedElement = ElementType.Kim;
            }

            if (ElementCycleManager.Instance != null)
            {
                ElementCycleManager.Instance.PushVirtualElementHit(selectedElement);
            }
        }
    }
}

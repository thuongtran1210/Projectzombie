using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Khung vũ khí sinh ra các vật thể xoay tròn xung quanh người chơi.
    /// Tự động cập nhật số lượng và tốc độ xoay khi lên cấp.
    /// </summary>
    public class Weapon_Orbit : WeaponBase
    {
        [Header("Orbit Settings")]
        [SerializeField] private GameObject orbPrefab; 
        [SerializeField] private float baseRadius = 2f;
        [SerializeField] private float baseOrbitSpeed = 180f; // Độ / giây
        [SerializeField] private int baseOrbCount = 1;

        private List<GameObject> _activeOrbs = new List<GameObject>();
        private int _lastProjectileCount = -1;
        private float _lastScaleBonus = -1f;

        protected override void PerformAttack()
        {
            // Vũ khí Aura/Orbit không tấn công theo nhịp (Cooldown).
            // Do đó hàm này bị bỏ trống. Việc gây sát thương do Orb tự quản lý.
        }

        private void Update()
        {
            if (CharacterStats == null) return;

            // Tính toán tổng số lượng đạn (vệ tinh) và kích thước
            int currentExpectedOrbs = baseOrbCount + localProjectileCountBonus;
            float currentScale = 1f + localScaleBonus;

            // Nếu số lượng hoặc kích thước thay đổi (do vừa Level Up), ta khởi tạo lại toàn bộ vệ tinh
            if (currentExpectedOrbs != _lastProjectileCount || !Mathf.Approximately(currentScale, _lastScaleBonus))
            {
                RefreshOrbs(currentExpectedOrbs, currentScale);
                _lastProjectileCount = currentExpectedOrbs;
                _lastScaleBonus = currentScale;
            }

            // Tốc độ xoay được cộng hưởng bởi AttackSpeed của nhân vật
            float currentOrbitSpeed = baseOrbitSpeed * (CharacterStats.AttackSpeed + localAttackSpeedBonus);

            // Cập nhật tốc độ xoay và bán kính liên tục (Hữu ích khi bạn kéo thanh trượt trong Inspector)
            foreach (var orb in _activeOrbs)
            {
                if (orb != null)
                {
                    var moveOrbit = orb.GetComponent<Move_Orbit>();
                    if (moveOrbit != null)
                    {
                        moveOrbit.UpdateOrbitSpeed(currentOrbitSpeed);
                        moveOrbit.UpdateRadius(baseRadius);
                    }
                }
            }
        }

        /// <summary>
        /// Xóa các vệ tinh cũ và sinh ra lứa vệ tinh mới chia đều nhau.
        /// </summary>
        private void RefreshOrbs(int orbCount, float scale)
        {
            foreach (var orb in _activeOrbs)
            {
                if (orb != null) Destroy(orb);
            }
            _activeOrbs.Clear();

            if (orbPrefab == null) return;

            float angleStep = 360f / Mathf.Max(1, orbCount);
            DamageData damageData = DamageUtility.CalculateDamage(GetDamage(), CharacterStats.CritChance);

            for (int i = 0; i < orbCount; i++)
            {
                float startAngle = i * angleStep;
                
                // Sinh ra trực tiếp làm con của Weapon
                GameObject newOrb = Instantiate(orbPrefab, transform.position, Quaternion.identity, transform);
                newOrb.transform.localScale = Vector3.one * scale;

                var core = newOrb.GetComponent<ProjectileCore>();
                if (core != null)
                {
                    core.Initialize(Vector2.zero, damageData);
                }

                var moveOrbit = newOrb.GetComponent<Move_Orbit>();
                if (moveOrbit != null)
                {
                    float initialSpeed = baseOrbitSpeed * (CharacterStats.AttackSpeed + localAttackSpeedBonus);
                    // Dùng firePoint làm tâm xoay
                    moveOrbit.Initialize(firePoint != null ? firePoint : transform, baseRadius, initialSpeed, startAngle);
                }
                else
                {
                    Debug.LogError($"[Weapon_Orbit] LỖI: Prefab '{orbPrefab.name}' không được gắn script Move_Orbit!");
                }

                _activeOrbs.Add(newOrb);
            }
            
            Debug.Log($"[Weapon_Orbit] Đã tạo {orbCount} vệ tinh. AttackSpeed hiện tại = {CharacterStats.AttackSpeed}");
        }

        private void OnDisable()
        {
            foreach (var orb in _activeOrbs)
            {
                if (orb != null) Destroy(orb);
            }
            _activeOrbs.Clear();
            _lastProjectileCount = -1;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform center = firePoint != null ? firePoint : transform;
            Gizmos.color = Color.cyan;
            
            // Vẽ quỹ đạo vòng xoay (Gizmos.DrawWireSphere)
            Gizmos.DrawWireSphere(center.position, baseRadius);

            // Có thể vẽ thêm một số điểm tượng trưng cho vị trí khởi tạo của lưỡi cưa
            Gizmos.color = Color.yellow;
            int previewCount = Mathf.Max(1, baseOrbCount + localProjectileCountBonus);
            float angleStep = 360f / previewCount;
            for (int i = 0; i < previewCount; i++)
            {
                float rad = i * angleStep * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * baseRadius;
                Gizmos.DrawWireSphere(center.position + offset, 0.2f); // Lưỡi cưa minh họa
            }
        }
#endif
    }
}

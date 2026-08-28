using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Holy Water (W011): Thả bãi nước thánh ngẫu nhiên trong bán kính xung quanh Player (Ground Zone).
    /// Đòi hỏi người chơi di chuyển chiến thuật (Kite) để lùa quái vật bước vào các vũng nước thánh.
    /// </summary>
    public class Weapon_HolyWater : Weapon_RangedBase
    {
        [Header("Holy Water Settings")]
        [Tooltip("Bán kính tối thiểu rơi bãi nước (tránh rơi đè lên tâm người chơi)")]
        [SerializeField] private float minDropDistance = 1.2f;

        [Tooltip("Bán kính tối đa rơi bãi nước xung quanh người chơi")]
        [SerializeField] private float baseDropRadius = 5.5f;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 15.0f;
            if (activeDuration <= 0f) activeDuration = 6.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Trận Pháp Giếng Thiêng";
        }

        protected override bool CanAttack()
        {
            return true; // Luôn luôn thả nước thánh định kỳ xung quanh người chơi
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            DamageData damageData = CreateDamageData();
            int count = GetFinalProjectileCount();
            float maxRadius = CharacterStats != null ? Mathf.Max(baseDropRadius, CharacterStats.AttackRange * 0.65f) : baseDropRadius;

            for (int i = 0; i < count; i++)
            {
                // Sinh ngẫu nhiên góc và khoảng cách xung quanh người chơi (Random Annular Distribution)
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float randomDist = Random.Range(minDropDistance, maxRadius);

                Vector2 offset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDist;
                Vector3 spawnPos = transform.position + (Vector3)offset;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, spawnPos, Vector2.zero, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }

        /// <summary>
        /// Kỹ năng chủ động: Trận Pháp Giếng Thiêng — Tạo 3 giếng thiêng phong tỏa xung quanh chân và hồi phục máu.
        /// </summary>
        protected override void PerformActiveRelicSkill()
        {
            if (projectileData != null)
            {
                DamageData damageData = CreateDamageData();
                damageData = new DamageData(damageData.Amount * 1.5f, true, ElementType.Tho, damageData.IsCounter, this);

                for (int i = 0; i < 3; i++)
                {
                    float angle = i * 120f * Mathf.Deg2Rad;
                    Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 1.8f;
                    var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, spawnPos, Vector2.zero, gameObject, damageData);
                    if (proj != null)
                    {
                        proj.transform.localScale = Vector3.one * Mathf.Max(1.4f, GetFinalScale() * 1.4f);
                    }
                }
            }

            // Hồi phục 10% HP cho Hero khi kích hoạt
            if (transform.root.TryGetComponent<HealthSystem>(out var health))
            {
                health.Heal(health.MaxHealth * 0.10f);
            }
            else if (transform.root.TryGetComponent<IHealable>(out var healable))
            {
                healable.Heal(25f);
            }

            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(transform.position);
        }
    }
}

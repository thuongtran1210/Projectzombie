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

        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.VectorWall, 6.5f, 4.5f, 0f, true);

        /// <summary>
        /// Kỹ năng chủ động: Trận Pháp Giếng Thiêng — Dựng Bức Tường Nước Thánh theo vector vạch sẵn, phong tỏa làm chậm 50% quái và hồi ngay 10% Max HP.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            if (projectileData == null) return;

            DamageData damageData = CreateDamageData();
            Vector2 forwardDir = customAimDirection != Vector2.zero ? customAimDirection : (Vector2)transform.right;
            Vector3 centerPos = transform.position + (Vector3)(forwardDir * 3.5f);
            Vector2 perpendicular = new Vector2(-forwardDir.y, forwardDir.x);

            // Dựng 4 giếng nước thánh xếp thành một đường thẳng tường chắn
            int wallSegments = 4;
            float segmentSpacing = 1.3f;
            float startOffset = -((wallSegments - 1) * segmentSpacing * 0.5f);

            for (int i = 0; i < wallSegments; i++)
            {
                Vector3 nodePos = centerPos + (Vector3)(perpendicular * (startOffset + i * segmentSpacing));
                var proj = Projectiles.Core.ProjectileSystem.Instance?.Spawn(projectileData, nodePos, Vector2.zero, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * (GetFinalScale() * 1.3f);
                }
            }

            // Hồi 10% Max HP cho người chơi
            if (CharacterStats is Player.PlayerStats ps)
            {
                var hp = ps.GetComponent<HealthSystem>();
                if (hp != null) hp.Heal(hp.MaxHealth * 0.10f);
            }
            else if (transform.root.TryGetComponent<IHealable>(out var healable))
            {
                healable.Heal(25f);
            }

            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(transform.position);
        }
    }
}

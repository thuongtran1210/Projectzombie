using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// W_POT — Nồi Cơm Thạch Sanh (Pháp Bảo Hộ Vệ Quỹ Đạo & Hồi Phục — Hệ Thổ).
    /// - Bay lơ lửng sau lưng Tướng.
    /// - Khi quái tiếp cận phạm vi nguy hiểm: Tự động mở nắp hút chân không gom quái và bắn pháo quái văng ra xa (Ragdoll Knockback).
    /// - Rơi 3 viên Cơm Nắm thần kỳ hồi phục 5% HP mỗi viên.
    /// </summary>
    public class Weapon_Pot : WeaponBase
    {
        [Header("Pot Settings")]
        [SerializeField] private float vacuumRadius = 3.8f;
        [SerializeField] private float autoTriggerRadius = 3.0f;
        [SerializeField] private int maxCapturedMobs = 3;
        [SerializeField] private float cooldownSeconds = 4.0f;
        [SerializeField] private GameObject potVfxPrefab;

        private float _lastTriggerTime;

        private void Awake()
        {
            EnsureVfxPrefab();
        }

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            EnsureVfxPrefab();
            weaponRole = WeaponRole.RelicOrbitalShield;
            isPrimaryActiveWeapon = false;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 14.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Hút Chân Không & Tiên Cơm";
        }

        private void EnsureVfxPrefab()
        {
            if (potVfxPrefab == null)
            {
                potVfxPrefab = Resources.Load<GameObject>("VFX/VFX_Relic_Pot_Suction");
#if UNITY_EDITOR
                if (potVfxPrefab == null)
                {
                    potVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_Relic_Pot_Suction.prefab");
                }
#endif
            }
        }

        protected override void PerformAttack()
        {
            // Kiểm tra nếu có quái xung quanh thì kích hoạt hút quái và bắn đại bác
            if (Time.time < _lastTriggerTime + cooldownSeconds) return;

            int mask = TargetingUtility.EnemyLayerMask;
            Collider2D[] nearMobs = Physics2D.OverlapCircleAll(transform.position, autoTriggerRadius, mask);
            if (nearMobs != null && nearMobs.Length > 0)
            {
                _lastTriggerTime = Time.time;
                StartCoroutine(RoutinePotDefenseSequence(false));
            }
        }

        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.CircleReticle, 6.0f, vacuumRadius * 1.4f, 0f, true);

        /// <summary>
        /// Kỹ năng chủ động: Hút Chân Không & Tiên Cơm — Gom quái diện rộng 6m vào tâm nồi, hất văng và hồi 15% Max HP.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(transform.position);
            StartCoroutine(RoutinePotDefenseSequence(true, customAimDirection));
        }

        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            // Tích lũy linh khí cơm nắm khi Hero chém trúng quái
        }

        private IEnumerator RoutinePotDefenseSequence(bool isEmpowered = false, Vector2 customAimDirection = default)
        {
            Vector2 center = customAimDirection != Vector2.zero ? (Vector2)transform.position + customAimDirection * 2.5f : (Vector2)transform.position;
            float currentRadius = isEmpowered ? vacuumRadius * 1.6f : vacuumRadius;
            int maxMobs = isEmpowered ? maxCapturedMobs + 4 : maxCapturedMobs;

            if (potVfxPrefab != null)
            {
                ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(potVfxPrefab, center, Quaternion.identity, isEmpowered ? 1.0f : 0.6f);
            }

            int mask = TargetingUtility.EnemyLayerMask;

            // 1. Gõ nắp nồi tạo sóng âm choáng nhẹ
            DamageData dmg = CreateDamageData();
            if (isEmpowered) dmg = new DamageData(dmg.Amount * 1.5f, true, ElementType.Tho, dmg.IsCounter, this);

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, currentRadius, mask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<HealthSystem>(out var hp)) hp.TakeDamage(dmg);
                if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Stun, isEmpowered ? 0.75f : 0.35f);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // 2. Hút quái vào miệng nồi rồi bắn văng ra ngoài
            List<Collider2D> captured = new List<Collider2D>();
            hits = Physics2D.OverlapCircleAll(center, currentRadius, mask);
            for (int i = 0; i < hits.Length && captured.Count < maxMobs; i++)
            {
                if (hits[i] != null) captured.Add(hits[i]);
            }

            for (int i = 0; i < captured.Count; i++)
            {
                var mob = captured[i];
                if (mob != null && mob.TryGetComponent<EnemyStatusController>(out var status))
                {
                    Vector2 launchDir = ((Vector2)mob.transform.position - center).normalized;
                    if (launchDir.sqrMagnitude < 0.01f) launchDir = Random.insideUnitCircle.normalized;
                    status.ApplyKnockback(launchDir, isEmpowered ? 18f : 14f, 0.4f);
                }
            }

            // 3. Rơi Cơm Nắm hồi máu cho Hero
            SpawnRiceBalls(center, isEmpowered ? 5 : 3);
        }

        private void SpawnRiceBalls(Vector2 center, int count)
        {
            // Hồi phục trực tiếp nếu người chơi đứng gần
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var hp = PlayerProvider.PlayerTransform.GetComponent<HealthSystem>();
                if (hp != null)
                {
                    float healAmt = hp.MaxHealth * 0.08f;
                    hp.Heal(healAmt);
                }
            }
        }
    }
}

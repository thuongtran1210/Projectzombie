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

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicOrbitalShield;
            isPrimaryActiveWeapon = false;

            if (potVfxPrefab == null)
            {
                potVfxPrefab = Resources.Load<GameObject>("VFX_Relic_Pot_Suction");
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
                StartCoroutine(RoutinePotDefenseSequence());
            }
        }

        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            // Tích lũy linh khí cơm nắm khi Hero chém trúng quái
        }

        private IEnumerator RoutinePotDefenseSequence()
        {
            Vector2 center = transform.position;

            if (potVfxPrefab != null)
            {
                if (GlobalVFXPoolManager.Instance != null)
                    GlobalVFXPoolManager.Instance.PlayEffect(potVfxPrefab, center, Quaternion.identity, 0.6f);
                else
                    Instantiate(potVfxPrefab, center, Quaternion.identity);
            }

            int mask = TargetingUtility.EnemyLayerMask;

            // 1. Gõ nắp nồi tạo sóng âm choáng nhẹ 0.35s
            DamageData dmg = CreateDamageData();
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, vacuumRadius, mask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<HealthSystem>(out var hp)) hp.TakeDamage(dmg);
                if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Stun, 0.35f);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // 2. Hút quái vào miệng nồi rồi bắn văng ra ngoài
            List<Collider2D> captured = new List<Collider2D>();
            hits = Physics2D.OverlapCircleAll(center, vacuumRadius, mask);
            for (int i = 0; i < hits.Length && captured.Count < maxCapturedMobs; i++)
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
                    status.ApplyKnockback(launchDir, 14f, 0.35f);
                }
            }

            // 3. Rơi 3 viên Cơm Nắm hồi máu cho Hero
            SpawnRiceBalls(center, 3);
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

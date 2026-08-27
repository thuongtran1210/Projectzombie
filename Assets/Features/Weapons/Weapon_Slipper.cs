using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// W_SLIPPER — Dép Tổ Ong Thần Sa (Pháp Bảo Hộ Thân Kích Ứng Bồi Đòn — Hệ Kim).
    /// - Khi Hero chém trúng quái: Tự động phóng Boomerang Dép Tổ Ong bay xuyên mục tiêu và quay về.
    /// - Khi Hero kết thúc Combo Hit 3: Kích hoạt "Lốc Dép Vạn Năng" 360 độ gom quái, gây 4 hit vả liên hoàn và khiến quái bị "Quê Độ" (Humiliated).
    /// </summary>
    public class Weapon_Slipper : WeaponBase
    {
        [Header("Slipper Settings")]
        [SerializeField] private float throwRange = 4.5f;
        [SerializeField] private float returnSpeed = 12f;
        [SerializeField] private float humiliatedChance = 0.5f;
        [SerializeField] private float autoWhirlwindCooldown = 3.5f;
        [SerializeField] private GameObject whirlwindVfxPrefab;

        private float _lastWhirlwindTime;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicOnHitTrigger;
            isPrimaryActiveWeapon = false;

            if (whirlwindVfxPrefab == null)
            {
                whirlwindVfxPrefab = Resources.Load<GameObject>("VFX_Relic_Slipper_Whirlwind");
#if UNITY_EDITOR
                if (whirlwindVfxPrefab == null)
                {
                    whirlwindVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_Relic_Slipper_Whirlwind.prefab");
                }
#endif
            }
        }

        protected override void PerformAttack()
        {
            // Tự động tìm kẻ địch gần nhất và ném dép Boomerang
            Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 6.0f);
            Vector2 dir = nearest != null ? ((Vector2)nearest.position - (Vector2)transform.position).normalized : (Vector2)transform.right;
            StartCoroutine(RoutineThrowSlipper(dir, throwRange, 1.2f));
        }

        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            // Khi Hero chém trúng quái: Bồi thêm 1 chiếc dép Boomerang phóng thẳng vào mục tiêu
            if (enemyHit != null && Random.value <= 0.6f)
            {
                Vector2 dir = ((Vector2)enemyHit.transform.position - (Vector2)transform.position).normalized;
                StartCoroutine(RoutineThrowSlipper(dir, throwRange, 1.1f));
            }
        }

        public override void OnHeroComboFinished(int finalStep, Vector2 attackDirection)
        {
            // Khi Hero tung đòn kết liễu Combo Hit 3: Lập tức kích hoạt Lốc Dép Vạn Năng 360 độ
            if (finalStep == 3 && Time.time >= _lastWhirlwindTime + autoWhirlwindCooldown)
            {
                _lastWhirlwindTime = Time.time;
                StartCoroutine(RoutineWhirlwindSlippers());
            }
        }

        private void DealDamageAtPosition(Vector2 pos, DamageData dmg, float knockback)
        {
            int mask = TargetingUtility.EnemyLayerMask;
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 1.2f, mask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<HealthSystem>(out var hp))
                {
                    hp.TakeDamage(dmg);
                }
                if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                {
                    Vector2 kbDir = ((Vector2)hits[i].transform.position - pos).normalized;
                    if (kbDir.sqrMagnitude < 0.01f) kbDir = Vector2.up;
                    status.ApplyKnockback(kbDir, knockback, 0.2f);
                }
            }
        }

        private IEnumerator RoutineThrowSlipper(Vector2 dir, float range, float dmgMult)
        {
            Vector2 startPos = transform.position;
            Vector2 targetPos = startPos + dir.normalized * range;
            float duration = range / returnSpeed;
            float elapsed = 0f;

            DamageData dmg = CreateDamageData();
            dmg = new DamageData(dmg.Amount * dmgMult, dmg.IsCritical, ElementType.Kim, dmg.IsCounter, this);

            // Bay tới đích
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
                DealDamageAtPosition(currentPos, dmg, 4f);
                yield return null;
            }

            // Bay ngược về người chơi
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Vector2 playerPos = transform.position;
                Vector2 currentPos = Vector2.Lerp(targetPos, playerPos, t);
                DealDamageAtPosition(currentPos, dmg, 3f);
                yield return null;
            }
        }

        private IEnumerator RoutineWhirlwindSlippers()
        {
            Vector2 center = transform.position;

            if (whirlwindVfxPrefab != null)
            {
                if (GlobalVFXPoolManager.Instance != null)
                    GlobalVFXPoolManager.Instance.PlayEffect(whirlwindVfxPrefab, center, Quaternion.identity, 0.5f);
                else
                    Instantiate(whirlwindVfxPrefab, center, Quaternion.identity);
            }

            DamageData baseDmg = CreateDamageData();
            DamageData hitDmg = new DamageData(baseDmg.Amount * 0.5f, baseDmg.IsCritical, ElementType.Kim, baseDmg.IsCounter, this);

            // 4 đợt vả xoay tròn 360 độ
            for (int wave = 0; wave < 4; wave++)
            {
                center = transform.position;
                int mask = TargetingUtility.EnemyLayerMask;
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, 3.2f, mask);

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth > 0)
                    {
                        hp.TakeDamage(hitDmg);
                        if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                        {
                            // Kéo nhẹ quái lại gần tâm lốc
                            Vector2 pullDir = (center - (Vector2)hits[i].transform.position).normalized;
                            status.ApplyKnockback(-pullDir, 2.5f, 0.15f);

                            // Áp dụng Quê Độ ở đợt vả cuối
                            if (wave == 3 && Random.value <= humiliatedChance)
                            {
                                status.ApplyStatusEffect(StatusEffectType.Humiliated, 2.0f);
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}

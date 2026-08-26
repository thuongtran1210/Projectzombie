using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// W_SLIPPER — Dép Tổ Ong Thần Sa (Vũ Khí Ném Boomerang Slapstick — Hệ Kim).
    /// Chuỗi Combo 3 Đòn:
    /// - Hit 1: Ném dép trái thẳng 4m rồi quay về tay, gây 110% DMG.
    /// - Hit 2: Ném dép phải bay chéo 30 độ, gây 130% DMG.
    /// - Hit 3 (Lốc Dép Vạn Năng): Xoay 360 độ quăng 2 chiếc dép tạo lốc xoáy gom quái và vả 4 hit, kích hoạt "Quê Độ" (Humiliated).
    /// </summary>
    public class Weapon_Slipper : Weapon_MeleeBase
    {
        [Header("Slipper Settings")]
        [SerializeField] private float throwRange = 4.5f;
        [SerializeField] private float returnSpeed = 12f;
        [SerializeField] private float humiliatedChance = 0.45f;

        protected override void PerformAttack()
        {
            PerformComboAttack(CurrentComboStep);
        }

        protected override void PerformComboAttack(int step)
        {
            Vector2 attackDir = transform.right;
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null)
                {
                    if (player.MovementInput.sqrMagnitude > 0.01f)
                        attackDir = player.MovementInput.normalized;
                    else
                        attackDir = player.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
                }
            }

            switch (step)
            {
                case 1:
                    // Hit 1: Ném Dép Trái thẳng
                    StartCoroutine(RoutineThrowSlipper(attackDir, throwRange, 1.1f, 0f));
                    break;

                case 2:
                    // Hit 2: Ném Dép Phải chéo
                    Vector2 angledDir = Quaternion.Euler(0, 0, 25f) * attackDir;
                    StartCoroutine(RoutineThrowSlipper(angledDir, throwRange, 1.3f, 0f));
                    break;

                case 3:
                    // Hit 3: Lốc Dép Vạn Năng 360 độ (4 vả liên hoàn)
                    StartCoroutine(RoutineWhirlwindSlippers());
                    break;
            }
        }

        private IEnumerator RoutineThrowSlipper(Vector2 dir, float range, float dmgMult, float angleOffset)
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
                DealDamageInArea(currentPos, new Vector2(1.2f, 1.2f), 0f, dmg, 4f);
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
                DealDamageInArea(currentPos, new Vector2(1.2f, 1.2f), 0f, dmg, 3f);
                yield return null;
            }
        }

        private IEnumerator RoutineWhirlwindSlippers()
        {
            DamageData baseDmg = CreateDamageData();
            DamageData hitDmg = new DamageData(baseDmg.Amount * 0.5f, baseDmg.IsCritical, ElementType.Kim, baseDmg.IsCounter, this);

            // 4 đợt vả xoay tròn 360 độ
            for (int wave = 0; wave < 4; wave++)
            {
                Vector2 center = transform.position;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// W_POT — Nồi Cơm Thạch Sanh (Vũ Khí Gom Quái & Bắn Quái Làm Đạn Pháo — Hệ Thổ).
    /// Chuỗi Combo 3 Đòn:
    /// - Hit 1 (Gõ Nắp): Gõ nắp nồi đập mạnh xuống đất gây 100% DMG, choáng nhẹ 0.3s góc quạt 90 độ.
    /// - Hit 2 (Hút Quái): Mở nắp nồi tạo lực hút chân không gom tối đa 3 quái thường lại gần miệng nồi.
    /// - Hit 3 (Phóng Quái Đại Bác): Phóng quái bay ra như đạn pháo (Ragdoll Launch) 240% DMG; chạm đất rơi 3 viên Cơm Nắm hồi 5% HP.
    /// </summary>
    public class Weapon_Pot : Weapon_MeleeBase
    {
        [Header("Pot Settings")]
        [SerializeField] private float vacuumRadius = 3.8f;
        [SerializeField] private int maxCapturedMobs = 3;
        [SerializeField] private float cannonLaunchSpeed = 16f;
        private readonly List<Enemy> _capturedEnemies = new List<Enemy>();

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

            Vector2 center = (Vector2)transform.position + attackDir * 1.5f;

            switch (step)
            {
                case 1:
                    // Hit 1: Gõ nắp nồi gây choáng Stun 0.35s
                    DamageData dmg1 = CreateDamageData();
                    DealDamageInArea(center, new Vector2(2.5f, 2.0f), 0f, dmg1, 5f);
                    ApplyStunToArea(center, 2.0f, 0.35f);
                    break;

                case 2:
                    // Hit 2: Hút quái vào nồi
                    StartCoroutine(RoutineVacuumEnemies(center));
                    break;

                case 3:
                    // Hit 3: Bắn quái đại bác (Kinematic Ragdoll Launch)
                    ExecuteCannonLaunch(attackDir);
                    break;
            }
        }

        private void ApplyStunToArea(Vector2 center, float radius, float duration)
        {
            int mask = TargetingUtility.EnemyLayerMask;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, mask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Stun, duration);
                }
            }
        }

        private IEnumerator RoutineVacuumEnemies(Vector2 center)
        {
            _capturedEnemies.Clear();
            int mask = TargetingUtility.EnemyLayerMask;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, vacuumRadius, mask);

            int count = 0;
            for (int i = 0; i < hits.Length && count < maxCapturedMobs; i++)
            {
                if (hits[i].TryGetComponent<Enemy>(out var enemy) && !enemy.IsBoss)
                {
                    _capturedEnemies.Add(enemy);
                    count++;
                }
            }

            float duration = 0.35f;
            float elapsed = 0f;
            Vector2 potMouthPos = (Vector2)transform.position + (Vector2)transform.right * 0.8f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                for (int i = 0; i < _capturedEnemies.Count; i++)
                {
                    if (_capturedEnemies[i] != null && _capturedEnemies[i].gameObject.activeInHierarchy)
                    {
                        _capturedEnemies[i].transform.position = Vector2.Lerp(_capturedEnemies[i].transform.position, potMouthPos, t * 0.5f);
                    }
                }
                yield return null;
            }

            DamageData suckDmg = CreateDamageData();
            suckDmg = new DamageData(suckDmg.Amount * 0.5f, suckDmg.IsCritical, ElementType.Tho, suckDmg.IsCounter, this);
            DealDamageInArea(potMouthPos, new Vector2(2.0f, 2.0f), 0f, suckDmg, 1f);
        }

        private void ExecuteCannonLaunch(Vector2 dir)
        {
            DamageData cannonDmg = CreateDamageData();
            cannonDmg = new DamageData(cannonDmg.Amount * 2.4f, cannonDmg.IsCritical, ElementType.Tho, cannonDmg.IsCounter, this);

            // Gây sát thương quét đường đạn
            Vector2 startPos = transform.position;
            DealDamageInArea(startPos + dir * 3.0f, new Vector2(6.0f, 2.5f), 0f, cannonDmg, 10f);

            // Bắn các quái đã gom đi
            for (int i = 0; i < _capturedEnemies.Count; i++)
            {
                if (_capturedEnemies[i] != null && _capturedEnemies[i].gameObject.activeInHierarchy)
                {
                    if (_capturedEnemies[i].TryGetComponent<EnemyStatusController>(out var status))
                    {
                        status.ApplyRagdollLaunch(dir, cannonLaunchSpeed, 0.7f, cannonDmg.Amount, 2.5f);
                    }
                }
            }
            _capturedEnemies.Clear();

            // Cơ chế cơm nắm hồi máu: Hồi trực tiếp 5% Max HP cho người chơi
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerHealth != null)
            {
                float healVal = PlayerProvider.PlayerHealth.MaxHealth * 0.05f;
                PlayerProvider.PlayerHealth.Heal(healVal);
            }
        }
    }
}

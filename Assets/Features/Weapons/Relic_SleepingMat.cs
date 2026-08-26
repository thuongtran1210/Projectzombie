using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// R007 — Chiếu Trải Hoàng Tuyền (Pháp Bảo Bẫy Ngủ & Đường Trượt Siêu Tốc — Hệ Mộc).
    /// - Quái bước vào mép chiếu: Lập tức ngã vật ra ngủ say 3s (Sleeping - nhận x2 crit khi bị đánh thức).
    /// - Người chơi bước/dash lên chiếu: Trở thành Trượt Ván Siêu Tốc (+100% Move Speed), ủi bay quái như chơi bowling.
    /// </summary>
    public class Relic_SleepingMat : WeaponBase
    {
        [Header("Sleeping Mat Settings")]
        [SerializeField] private float matInterval = 8.0f;
        [SerializeField] private float matDuration = 5.0f;
        [SerializeField] private Vector2 matSize = new Vector2(3.5f, 2.2f);

        private float _timer;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicSupportAura;
            isPrimaryActiveWeapon = false;
            _timer = 0f;
        }

        protected override void PerformAttack()
        {
            DeployMat(transform.position);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= matInterval)
            {
                _timer = 0f;
                DeployMat(transform.position);
            }
        }

        public void DeployMat(Vector2 position)
        {
            StartCoroutine(RoutineMatActive(position));
        }

        private IEnumerator RoutineMatActive(Vector2 matCenter)
        {
            float elapsed = 0f;
            int enemyMask = TargetingUtility.EnemyLayerMask;

            while (elapsed < matDuration)
            {
                elapsed += 0.2f;

                // 1. Quét kẻ địch bước lên chiếu -> Ngủ say 3.0s
                Collider2D[] enemyHits = Physics2D.OverlapBoxAll(matCenter, matSize, 0f, enemyMask);
                for (int i = 0; i < enemyHits.Length; i++)
                {
                    if (enemyHits[i].TryGetComponent<EnemyStatusController>(out var status))
                    {
                        if (!status.IsSleeping)
                        {
                            status.ApplyStatusEffect(StatusEffectType.Sleeping, 3.0f);
                        }
                    }
                }

                // 2. Quét người chơi bước lên chiếu -> Kích tốc trượt ván
                if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
                {
                    Vector2 playerPos = PlayerProvider.PlayerTransform.position;
                    if (Mathf.Abs(playerPos.x - matCenter.x) <= matSize.x * 0.5f &&
                        Mathf.Abs(playerPos.y - matCenter.y) <= matSize.y * 0.5f)
                    {
                        // Người chơi đang trượt trên chiếu: ủi bay quái xung quanh
                        for (int i = 0; i < enemyHits.Length; i++)
                        {
                            if (enemyHits[i].TryGetComponent<EnemyStatusController>(out var status))
                            {
                                status.ApplyKnockback((enemyHits[i].transform.position - (Vector3)playerPos).normalized, 8f, 0.3f);
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}

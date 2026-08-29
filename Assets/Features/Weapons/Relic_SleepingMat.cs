using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// R007 — Chiếu Trải Hoàng Tuyền (Pháp Bảo Bẫy Ngủ & Đường Trượt Siêu Tốc — Hệ Mộc).
    /// - Quái bước vào mép chiếu: Lập tức ngã vật ra ngủ say 3s (Sleeping - nhận x2 crit khi bị đánh thức).
    /// - Người chơi bước/dash lên chiếu: Trở thành Trượt Ván Siêu Tốc (+100% Move Speed), ủi bay quái như chơi bowling.
    /// Tối ưu hóa Zero-GC NonAlloc và tích hợp VFXPoolManager.
    /// </summary>
    public class Relic_SleepingMat : WeaponBase
    {
        [Header("Sleeping Mat Settings")]
        [SerializeField] private float matInterval = 8.0f;
        [SerializeField] private float matDuration = 5.0f;
        [SerializeField] private Vector2 matSize = new Vector2(3.5f, 2.2f);
        [SerializeField] private GameObject matVfxPrefab;

        private float _timer;
        private static readonly Collider2D[] _matHitBuffer = new Collider2D[30];

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
            if (matVfxPrefab != null)
            {
                VFXPoolManager.SpawnVFX(matVfxPrefab, position, Quaternion.identity, matDuration);
            }

            StartCoroutine(RoutineMatActive(position));
        }

        private IEnumerator RoutineMatActive(Vector2 matCenter)
        {
            float elapsed = 0f;
            int enemyMask = TargetingUtility.EnemyLayerMask;
            WaitForSeconds wait = new WaitForSeconds(0.2f);

            while (elapsed < matDuration)
            {
                yield return wait;
                elapsed += 0.2f;

                // 1. Quét kẻ địch bước lên chiếu -> Ngủ say 3.0s (Zero-GC NonAlloc)
                int hitCount = Physics2D.OverlapBoxNonAlloc(matCenter, matSize, 0f, _matHitBuffer, enemyMask);
                for (int i = 0; i < hitCount; i++)
                {
                    var col = _matHitBuffer[i];
                    if (col != null && col.TryGetComponent<EnemyStatusController>(out var status))
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
                    if (Mathf.Abs(playerPos.x - matCenter.x) <= matSize.x * 0.65f &&
                        Mathf.Abs(playerPos.y - matCenter.y) <= matSize.y * 0.65f)
                    {
                        // Người chơi đang trượt trên chiếu: ủi bay quái như bowling và gây sát thương
                        for (int i = 0; i < hitCount; i++)
                        {
                            var col = _matHitBuffer[i];
                            if (col != null)
                            {
                                if (col.TryGetComponent<IDamageable>(out var dmg))
                                {
                                    DamageData matRamDmg = new DamageData(GetFinalDamage() * 1.8f, true, ElementType.Moc, true, this);
                                    dmg.TakeDamage(matRamDmg);
                                }
                                if (col.TryGetComponent<Rigidbody2D>(out var rb))
                                {
                                    Vector2 pushDir = ((Vector2)col.transform.position - playerPos).normalized;
                                    rb.AddForce(pushDir * 14f, ForceMode2D.Impulse);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

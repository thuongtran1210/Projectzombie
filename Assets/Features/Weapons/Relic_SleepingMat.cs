using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Combat.Aiming;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// R007 — Chiếu Trải Hoàng Tuyền (Pháp Bảo Bẫy Ngủ & Di Dời Không Gian — Hệ Mộc).
    /// - Kỹ năng Chủ Động 2 Pha (2-Phase Recast Active Skill):
    ///   * Pha 1 (Setup Trap): Kéo ngắm / Bấm đặt Bẫy Chiếu Hoàng Tuyền tại vị trí chỉ định.
    ///     Quái bước vào mép chiếu sẽ lập tức ngã vật ra ngủ say (Sleeping - nhận x2 crit khi bị đánh thức).
    ///   * Pha 2 (Relocate Trap): Bấm/Kéo ngắm lần 2 trong cửa sổ 5s để DI DỜI BẪY sang vị trí mới!
    ///     Tấm chiếu phi vút xé gió sang tọa độ mới, ủi văng mọi quái vật trên đường bay và reset thời gian tồn tại.
    /// - Tương tác Người chơi: Lướt qua chiếu kích tốc ủi quái như chơi bowling.
    /// </summary>
    public class Relic_SleepingMat : WeaponBase, IAimableSkill
    {
        [Header("Sleeping Mat Base Settings (Level 1)")]
        [SerializeField] private float baseDuration = 4.0f;
        [SerializeField] private float baseSleepDuration = 1.5f;
        [SerializeField] private float baseRamDamageMultiplier = 0.85f;
        [SerializeField] private float baseRelocateDamageMultiplier = 1.25f;
        [SerializeField] private Vector2 matSize = new Vector2(3.0f, 1.8f);

        [Header("VFX References")]
        [SerializeField] private GameObject matVfxPrefab;
        [SerializeField] private GameObject slideHitVfxPrefab;

        public override SkillAimConfig AimConfig => IsInRecastWindow
            ? new SkillAimConfig(SkillAimType.VectorWall, 6.5f, matSize.x, matSize.y, true)
            : new SkillAimConfig(SkillAimType.VectorWall, 5.0f, matSize.x, matSize.y, true);

        public float CurrentDuration => baseDuration + (WeaponLevel - 1) * 0.4f;
        public float CurrentSleepDuration => baseSleepDuration + (WeaponLevel - 1) * 0.25f;
        public float CurrentRamDamageMultiplier => baseRamDamageMultiplier + (WeaponLevel - 1) * 0.15f;
        public float CurrentRelocateDamageMultiplier => baseRelocateDamageMultiplier + (WeaponLevel - 1) * 0.2f;

        private static readonly Collider2D[] _matHitBuffer = new Collider2D[30];
        private Coroutine _activeMatRoutine;
        private Vector2 _currentMatPosition;
        private GameObject _spawnedMatVfx;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicSupportAura;
            isPrimaryActiveWeapon = false;
            isPassiveRelic = false; // Bật nút bấm kỹ năng trên Mobile HUD
            hasRecastPhase = true;  // Cho phép bấm lần 2 để Di Dời Bẫy
            activeCooldown = Mathf.Max(5.5f, 7.5f - (WeaponLevel - 1) * 0.5f);
            recastWindowDuration = CurrentDuration;
            skillActionName = "Đặt Bẫy Chiếu";
        }

        protected override void PerformAttack()
        {
            PerformActiveRelicSkill(Vector2.zero);
        }

        #region ACTIVE RELIC SKILL (PHASE 1: PLACE TRAP & PHASE 2: RELOCATE TRAP)
        /// <summary>
        /// Pha 1: Kích hoạt đặt Bẫy Chiếu tại vị trí ngắm bắn hoặc dưới chân Hero.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            Vector2 targetPos = CalculateTargetPosition(customAimDirection, 4.0f);
            skillActionName = "Di Dời Bẫy";
            recastWindowDuration = CurrentDuration;

            DeployMatAtPosition(targetPos, CurrentDuration);
        }

        /// <summary>
        /// Pha 2: Tái kích hoạt để Di Dời Bẫy Chiếu sang vị trí chiến thuật mới.
        /// </summary>
        protected override void PerformRecastSkill(Vector2 customAimDirection = default)
        {
            Vector2 newTargetPos = CalculateTargetPosition(customAimDirection, 5.5f);
            skillActionName = "Đặt Bẫy Chiếu";

            StartCoroutine(RoutineRelocateMat(_currentMatPosition, newTargetPos));
        }

        private Vector2 CalculateTargetPosition(Vector2 aimDir, float maxDist)
        {
            Vector2 origin = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null 
                ? (Vector2)PlayerProvider.PlayerTransform.position 
                : (Vector2)transform.position;

            if (aimDir == Vector2.zero)
            {
                Transform nearest = TargetingUtility.FindNearestEnemy(origin, maxDist + 2.0f);
                if (nearest != null)
                {
                    Vector2 toEnemy = (Vector2)nearest.position - origin;
                    float dist = Mathf.Min(toEnemy.magnitude, maxDist);
                    return origin + toEnemy.normalized * dist;
                }
                return origin;
            }

            return origin + aimDir * maxDist;
        }

        private void DeployMatAtPosition(Vector2 position, float duration)
        {
            if (_activeMatRoutine != null)
            {
                StopCoroutine(_activeMatRoutine);
                _activeMatRoutine = null;
            }

            _currentMatPosition = position;

            if (matVfxPrefab != null)
            {
                if (_spawnedMatVfx != null && _spawnedMatVfx.activeInHierarchy)
                {
                    // Tái sử dụng chính chiếc chiếu đang hiển thị, di dời vị trí và reset timer
                    _spawnedMatVfx.transform.position = position;
                    var pooled = _spawnedMatVfx.GetComponent<PooledVFXInstance>();
                    if (pooled != null)
                    {
                        pooled.StartAutoRelease(matVfxPrefab, duration);
                    }
                    var particles = _spawnedMatVfx.GetComponentsInChildren<ParticleSystem>(true);
                    for (int i = 0; i < particles.Length; i++)
                    {
                        particles[i].Clear();
                        particles[i].Play();
                    }
                }
                else
                {
                    _spawnedMatVfx = VFXPoolManager.SpawnVFX(matVfxPrefab, position, Quaternion.identity, duration);
                }
            }

            _activeMatRoutine = StartCoroutine(RoutineMatActive(position, duration));
        }

        private IEnumerator RoutineRelocateMat(Vector2 oldPos, Vector2 newPos)
        {
            // 1. Dừng logic quét bẫy tại vị trí cũ
            if (_activeMatRoutine != null)
            {
                StopCoroutine(_activeMatRoutine);
                _activeMatRoutine = null;
            }

            // 2. Phi chiếu xé gió từ vị trí cũ sang vị trí mới trong 0.22s
            float flightTime = 0.22f;
            float elapsed = 0f;
            int enemyMask = TargetingUtility.EnemyLayerMask;
            HashSet<Collider2D> hitDuringFlight = new HashSet<Collider2D>();

            while (elapsed < flightTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flightTime);
                Vector2 currentFlightPos = Vector2.Lerp(oldPos, newPos, t);

                // Di chuyển chính chiếc chiếu theo đường bay
                if (_spawnedMatVfx != null)
                {
                    _spawnedMatVfx.transform.position = currentFlightPos;
                }

                // Quét quái vật trên đường bay di dời -> Gây sát thương húc bay
                int flightHits = Physics2D.OverlapBoxNonAlloc(currentFlightPos, matSize, 0f, _matHitBuffer, enemyMask);
                for (int i = 0; i < flightHits; i++)
                {
                    var col = _matHitBuffer[i];
                    if (col != null && !hitDuringFlight.Contains(col))
                    {
                        hitDuringFlight.Add(col);
                        if (col.TryGetComponent<IDamageable>(out var dmg))
                        {
                            DamageData flightDmg = new DamageData(GetFinalDamage() * CurrentRelocateDamageMultiplier, true, ElementType.Moc, true, this);
                            dmg.TakeDamage(flightDmg);

                            if (slideHitVfxPrefab != null)
                            {
                                VFXPoolManager.SpawnVFX(slideHitVfxPrefab, col.transform.position, Quaternion.identity, 0.5f);
                            }
                        }

                        if (col.TryGetComponent<Rigidbody2D>(out var rb))
                        {
                            Vector2 pushDir = (newPos - oldPos).normalized;
                            if (pushDir == Vector2.zero) pushDir = Vector2.up;
                            rb.AddForce(pushDir * 14f, ForceMode2D.Impulse);
                        }
                    }
                }

                yield return null;
            }

            // 3. Tiếp đất và mở chiếu tại vị trí mới (Không tạo chiếu mới, dùng lại chiếu vừa bay tới)
            DeployMatAtPosition(newPos, CurrentDuration);
        }
        #endregion

        private IEnumerator RoutineMatActive(Vector2 matCenter, float duration)
        {
            float elapsed = 0f;
            int enemyMask = TargetingUtility.EnemyLayerMask;
            WaitForSeconds wait = new WaitForSeconds(0.2f);

            HashSet<Collider2D> sleptEnemies = new HashSet<Collider2D>();
            Dictionary<Collider2D, float> ramCooldowns = new Dictionary<Collider2D, float>();

            Vector2 lastPlayerPos = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null 
                ? (Vector2)PlayerProvider.PlayerTransform.position 
                : matCenter;

            while (elapsed < duration)
            {
                yield return wait;
                elapsed += 0.2f;

                // 1. Quét kẻ địch bước lên chiếu -> Ngủ say (Chỉ dính 1 lần mỗi tấm chiếu)
                int hitCount = Physics2D.OverlapBoxNonAlloc(matCenter, matSize, 0f, _matHitBuffer, enemyMask);
                for (int i = 0; i < hitCount; i++)
                {
                    var col = _matHitBuffer[i];
                    if (col != null && !sleptEnemies.Contains(col) && col.TryGetComponent<EnemyStatusController>(out var status))
                    {
                        sleptEnemies.Add(col);
                        if (!status.IsSleeping)
                        {
                            status.ApplyStatusEffect(StatusEffectType.Sleeping, CurrentSleepDuration);
                        }
                    }
                }

                // 2. Quét người chơi bước/lướt lên chiếu -> Kích tốc trượt ván
                if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
                {
                    Vector2 playerPos = PlayerProvider.PlayerTransform.position;
                    bool isInsideMat = Mathf.Abs(playerPos.x - matCenter.x) <= matSize.x * 0.6f &&
                                       Mathf.Abs(playerPos.y - matCenter.y) <= matSize.y * 0.6f;

                    if (isInsideMat)
                    {
                        float playerMoveDist = Vector2.Distance(playerPos, lastPlayerPos);
                        bool isPlayerMoving = playerMoveDist >= 0.05f;

                        if (isPlayerMoving)
                        {
                            float now = Time.time;
                            for (int i = 0; i < hitCount; i++)
                            {
                                var col = _matHitBuffer[i];
                                if (col != null)
                                {
                                    if (!ramCooldowns.TryGetValue(col, out float nextHitTime) || now >= nextHitTime)
                                    {
                                        ramCooldowns[col] = now + 0.8f;

                                        if (col.TryGetComponent<IDamageable>(out var dmg))
                                        {
                                            DamageData matRamDmg = new DamageData(GetFinalDamage() * CurrentRamDamageMultiplier, true, ElementType.Moc, true, this);
                                            dmg.TakeDamage(matRamDmg);

                                            if (slideHitVfxPrefab != null)
                                            {
                                                VFXPoolManager.SpawnVFX(slideHitVfxPrefab, col.transform.position, Quaternion.identity, 0.6f);
                                            }
                                        }

                                        if (col.TryGetComponent<Rigidbody2D>(out var rb))
                                        {
                                            Vector2 pushDir = ((Vector2)col.transform.position - playerPos).normalized;
                                            if (pushDir == Vector2.zero) pushDir = Vector2.up;
                                            rb.AddForce(pushDir * 10f, ForceMode2D.Impulse);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    lastPlayerPos = playerPos;
                }
            }
        }
    }
}

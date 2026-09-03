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
    /// W_PIPE — Điếu Cày Cửu U (Pháp Bảo Hỗ Trợ Khống Chế & Thiêu Đốt — Hệ Hỏa).
    /// - Bị động định kỳ: Rít điếu cày ùng ục và nhả cụm mây khói thuốc rồng cuộn trước mặt làm quái say thuốc.
    /// - Chủ động (VectorWall): Nhả bức tường bão khói thuốc lào dài 5.2m - 7.5m chắn đường quái, gây say thuốc, giật lùi và nổ ho thiêu đốt.
    /// </summary>
    public class Weapon_Pipe : WeaponBase
    {
        [Header("Pipe Settings")]
        [SerializeField] private float smokeDuration = 4.0f;
        [SerializeField] private float smokeRadius = 1.6f;
        [SerializeField] private GameObject smokeVfxPrefab;
        [SerializeField] private Sprite pipeFollowerSprite;

        private readonly Collider2D[] _hitBuffer = new Collider2D[32];
        private GameObject _pipeFollowerObj;

        private void Awake()
        {
            EnsureVfxPrefab();
        }

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            EnsureVfxPrefab();
            weaponRole = WeaponRole.RelicSupportAura;
            isPrimaryActiveWeapon = false;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 9.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Bão Khói Thuốc Lào";

            CreatePipeFollower();
        }

        private void Update()
        {
            UpdatePipeFollower();
        }

        private void OnDestroy()
        {
            if (_pipeFollowerObj != null) Destroy(_pipeFollowerObj);
        }

        private void EnsureVfxPrefab()
        {
            if (smokeVfxPrefab == null)
            {
                smokeVfxPrefab = Resources.Load<GameObject>("VFX/VFX_Relic_Pipe_DragonSmoke");
#if UNITY_EDITOR
                if (smokeVfxPrefab == null)
                {
                    smokeVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_Relic_Pipe_DragonSmoke.prefab");
                }
#endif
            }

            if (pipeFollowerSprite == null)
            {
                pipeFollowerSprite = icon;
#if UNITY_EDITOR
                if (pipeFollowerSprite == null)
                {
                    pipeFollowerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/Icon_W_PIPE.png");
                }
#endif
            }
        }

        #region PIPE FOLLOWER VISUAL (CHIẾC ĐIẾU CÀY BAY TRÊN VAI TƯỚNG)
        private void CreatePipeFollower()
        {
            if (_pipeFollowerObj != null || pipeFollowerSprite == null) return;

            _pipeFollowerObj = new GameObject("Pipe_Orbital_Follower");
            var sr = _pipeFollowerObj.AddComponent<SpriteRenderer>();
            sr.sprite = pipeFollowerSprite;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 9;

            bool isEvolution = WeaponLevel >= MaxLevel;
            float scale = isEvolution ? 0.55f : 0.4f;
            _pipeFollowerObj.transform.localScale = Vector3.one * scale;
            if (isEvolution) sr.color = new Color(1f, 0.85f, 0.6f, 1f);
        }

        private void UpdatePipeFollower()
        {
            if (_pipeFollowerObj == null) return;

            Transform p = PlayerProvider.HasPlayer ? PlayerProvider.PlayerTransform : transform;
            if (p == null) return;

            // Lơ lửng lệch vai trái Hero với dao động nhấp nhô khói thuốc
            float bobbing = Mathf.Sin(Time.time * 2.8f) * 0.12f;
            Vector3 targetOffset = new Vector3(-0.55f, 0.7f + bobbing, 0f);
            Vector3 targetPos = p.position + targetOffset;

            _pipeFollowerObj.transform.position = Vector3.Lerp(_pipeFollowerObj.transform.position, targetPos, Time.deltaTime * 8.5f);
            
            // Góc nghiêng điếu cày tự nhiên (35 độ)
            float tilt = 35f + Mathf.Sin(Time.time * 1.5f) * 8f;
            _pipeFollowerObj.transform.rotation = Quaternion.Euler(0f, 0f, tilt);
        }
        #endregion

        #region PASSIVE PERIODIC SMOKE PUFF
        protected override void PerformAttack()
        {
            Vector2 forwardDir = transform.right;
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null) forwardDir = player.FacingVector;
            }

            bool isEvolution = WeaponLevel >= MaxLevel;
            Vector2 spawnPos = (Vector2)transform.position + forwardDir * 1.8f;
            float radius = isEvolution ? (smokeRadius * 1.4f) : smokeRadius;
            float duration = isEvolution ? (smokeDuration * 1.25f) : smokeDuration;

            if (smokeVfxPrefab != null)
            {
                float rotZ = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg;
                var vfx = ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(smokeVfxPrefab, spawnPos, Quaternion.Euler(0, 0, rotZ), duration, WeaponLevel);
                if (vfx != null)
                {
                    vfx.transform.localScale = Vector3.one * (isEvolution ? 1.4f : 1.0f);
                }
            }

            global::Core.Audio.AudioManager.Instance?.PlayStatusBurn(spawnPos);
            StartCoroutine(RoutineSmokeCircle(spawnPos, radius, duration, isEvolution));
        }
        #endregion

        #region ACTIVE RELIC SKILL (VECTOR WALL - BỨC TƯỜNG BÃO KHÓI RỒNG CUỘN)
        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(
            Combat.Aiming.SkillAimType.VectorWall, 
            WeaponLevel >= MaxLevel ? 7.2f : 5.5f, 
            WeaponLevel >= MaxLevel ? 7.5f : 5.2f, 
            0f, 
            true
        );

        /// <summary>
        /// Kỹ năng chủ động: Bão Khói Thuốc Lào — Rít hơi dài nhả bức tường bão khói diện rộng phong tỏa địa hình, quái say thuốc và nổ ho thiêu đốt.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            Vector2 forwardDir = customAimDirection != Vector2.zero ? customAimDirection : (Vector2)transform.right;
            if (customAimDirection == Vector2.zero && PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null) forwardDir = player.FacingVector;
            }

            float aimDist = isEvolution ? 5.2f : 3.8f;
            Vector2 wallCenter = (Vector2)transform.position + forwardDir * aimDist;
            float wallLength = isEvolution ? 7.5f : 5.2f;
            float wallThickness = 1.8f;
            float wallAngle = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg;
            float duration = isEvolution ? 6.5f : 5.0f;

            // Rung camera chấn động
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(isEvolution ? 0.3f : 0.18f, 0.3f);
            global::Core.Audio.AudioManager.Instance?.PlayStatusBurn(wallCenter);

            // Sinh dãy các cụm khói thuốc rồng cuộn trải đều dọc theo bức tường vuông góc
            Vector2 wallPerp = new Vector2(-forwardDir.y, forwardDir.x);
            int puffCount = isEvolution ? 6 : 4;
            float step = wallLength / (puffCount - 1);
            float startOffset = -wallLength * 0.5f;

            for (int i = 0; i < puffCount; i++)
            {
                Vector2 puffPos = wallCenter + wallPerp * (startOffset + i * step);
                if (smokeVfxPrefab != null)
                {
                    var vfx = ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(smokeVfxPrefab, puffPos, Quaternion.Euler(0, 0, wallAngle), duration, WeaponLevel);
                    if (vfx != null)
                    {
                        vfx.transform.localScale = Vector3.one * (isEvolution ? 1.35f : 0.95f);
                    }
                }
            }

            StartCoroutine(RoutineSmokeWall(wallCenter, wallLength, wallThickness, wallAngle, forwardDir, duration, isEvolution));
        }

        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            // Bồi đòn bạo kích: Tàn than hồng bốc cháy lan sang vùng lân cận
            if (heroDamage.IsCritical && enemyHit != null)
            {
                ApplyBurnToArea(enemyHit.transform.position, 2.2f, 2.5f, heroDamage.Amount * 0.45f);
            }
        }

        private void ApplyBurnToArea(Vector2 center, float radius, float duration, float dps)
        {
            int mask = TargetingUtility.EnemyLayerMask;
            int count = Physics2D.OverlapCircleNonAlloc(center, radius, _hitBuffer, mask);
            for (int i = 0; i < count; i++)
            {
                if (_hitBuffer[i] != null && _hitBuffer[i].TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Burn, duration, dps, 0.5f);
                }
            }
        }
        #endregion

        #region SMOKE COROUTINES (WALL & CIRCLE PHYSICS HIT DETECTION)
        private IEnumerator RoutineSmokeWall(Vector2 wallCenter, float wallLength, float wallThickness, float wallAngle, Vector2 forwardDir, float duration, bool isEvolution)
        {
            float elapsed = 0f;
            int mask = TargetingUtility.EnemyLayerMask;
            DamageData baseDmg = CreateDamageData();
            DamageData tickDmg = new DamageData(
                baseDmg.Amount * (isEvolution ? 0.65f : 0.4f),
                baseDmg.IsCritical,
                ElementType.Hoa,
                baseDmg.IsCounter,
                this
            );

            Vector2 boxSize = new Vector2(wallLength, wallThickness);

            while (elapsed < duration)
            {
                elapsed += 0.4f;
                int count = Physics2D.OverlapBoxNonAlloc(wallCenter, boxSize, wallAngle, _hitBuffer, mask);

                for (int i = 0; i < count; i++)
                {
                    var hit = _hitBuffer[i];
                    if (hit == null) continue;

                    // Sát thương thiêu đốt theo nhịp
                    if (hit.TryGetComponent<HealthSystem>(out var hp))
                    {
                        hp.TakeDamage(tickDmg);
                    }

                    if (hit.TryGetComponent<EnemyStatusController>(out var status))
                    {
                        // 1. Hiệu ứng Say Thuốc Lào: Giảm 60% tốc chạy & đi lảo đảo
                        status.ApplyStatusEffect(StatusEffectType.Stoned, isEvolution ? 1.5f : 1.0f);

                        // 2. Hiệu ứng Bỏng Lửa Hỏa
                        status.ApplyStatusEffect(StatusEffectType.Burn, 2.0f, baseDmg.Amount * 0.35f, 0.5f);

                        // 3. Đẩy nhẹ quái vật giật lùi ngược hướng tiến công
                        status.ApplyKnockback(-forwardDir, isEvolution ? 6.5f : 4.0f, 0.15f);

                        // 4. Cơ chế Tiến Hóa E_PIPE: Nổ Ho Dây Chuyền (Chain Cough Burst)
                        if (isEvolution && Random.value < 0.35f)
                        {
                            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(hit.transform.position);
                            if (hp != null)
                            {
                                hp.TakeDamage(new DamageData(baseDmg.Amount * 1.2f, true, ElementType.Hoa, false, this));
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(0.4f);
            }
        }

        private IEnumerator RoutineSmokeCircle(Vector2 center, float radius, float duration, bool isEvolution)
        {
            float elapsed = 0f;
            int mask = TargetingUtility.EnemyLayerMask;
            DamageData baseDmg = CreateDamageData();
            DamageData tickDmg = new DamageData(
                baseDmg.Amount * (isEvolution ? 0.5f : 0.35f),
                baseDmg.IsCritical,
                ElementType.Hoa,
                baseDmg.IsCounter,
                this
            );

            while (elapsed < duration)
            {
                elapsed += 0.5f;
                int count = Physics2D.OverlapCircleNonAlloc(center, radius, _hitBuffer, mask);

                for (int i = 0; i < count; i++)
                {
                    var hit = _hitBuffer[i];
                    if (hit == null) continue;

                    if (hit.TryGetComponent<HealthSystem>(out var hp)) hp.TakeDamage(tickDmg);
                    if (hit.TryGetComponent<EnemyStatusController>(out var status))
                    {
                        status.ApplyStatusEffect(StatusEffectType.Stoned, 0.8f);
                        status.ApplyStatusEffect(StatusEffectType.Burn, 1.5f, baseDmg.Amount * 0.25f, 0.5f);
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion
    }
}

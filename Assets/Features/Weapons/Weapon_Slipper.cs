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

        private void Awake()
        {
            EnsureVfxPrefab();
        }

        private Vector3 _lastSlipperApexPosition;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            EnsureVfxPrefab();
            weaponRole = WeaponRole.RelicOnHitTrigger;
            isPrimaryActiveWeapon = false;
            hasRecastPhase = true; // Bật cơ chế Recast 2 Phase
            recastWindowDuration = 3.0f;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 6.5f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Tổ Ong Lượn Cánh";
        }

        private void EnsureVfxPrefab()
        {
            if (whirlwindVfxPrefab == null)
            {
                whirlwindVfxPrefab = Resources.Load<GameObject>("VFX/VFX_Relic_Slipper_Whirlwind");
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
            global::Core.Audio.AudioManager.Instance?.PlaySlash(false, transform.position);
            StartCoroutine(RoutineThrowSlipper(dir, throwRange, 1.2f));
        }

        public override Combat.Aiming.SkillAimConfig AimConfig => IsInRecastWindow 
            ? new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.DashLine, 8.0f, 1.5f, 0f, true)
            : new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.CurvedTrajectory, throwRange * 1.5f, 1.5f, 40f, true);

        private GameObject _recastMarkerInstance;

        /// <summary>
        /// Phase 1: Tổ Ong Lượn Cánh — Quăng Boomerang Dép khổng lồ bay vòng cung gom quái.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            Vector2 dir = customAimDirection;
            if (dir == Vector2.zero)
            {
                Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 8.0f);
                if (nearest != null)
                {
                    dir = ((Vector2)nearest.position - (Vector2)transform.position).normalized;
                }
                else
                {
                    dir = transform.root.localScale.x >= 0 ? Vector2.right : Vector2.left;
                }
            }

            _lastSlipperApexPosition = transform.position + (Vector3)(dir * (throwRange * 1.4f));

            // Sinh Vòng Trận Báo Hiệu Điểm Đáp Phase 2 (Recast Dropkick Beacon)
            SpawnRecastGroundMarker(_lastSlipperApexPosition);

            global::Core.Audio.AudioManager.Instance?.PlaySlash(true, transform.position);
            StartCoroutine(RoutineThrowSlipper(dir, throwRange * 1.4f, 2.0f));
            StartCoroutine(RoutineWhirlwindSlippers());
        }

        private void SpawnRecastGroundMarker(Vector3 position)
        {
            if (_recastMarkerInstance != null) Destroy(_recastMarkerInstance);

            _recastMarkerInstance = new GameObject("VFX_Slipper_Recast_Beacon");
            _recastMarkerInstance.transform.position = position;

            var sr = _recastMarkerInstance.AddComponent<SpriteRenderer>();
            var circleSp = Resources.Load<Sprite>("Art/VFX/Indicators/TEX_Indicator_Circle");
#if UNITY_EDITOR
            if (circleSp == null)
            {
                circleSp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Indicators/TEX_Indicator_Circle.png");
            }
#endif
            sr.sprite = circleSp;
            sr.color = new Color(1f, 0.85f, 0.2f, 0.65f); // Vàng kim rực sáng
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 4;
            _recastMarkerInstance.transform.localScale = Vector3.one * 1.6f;

            // Tự hủy sau khi hết thời gian Recast window (3s)
            Destroy(_recastMarkerInstance, recastWindowDuration);
        }

        private void ClearRecastGroundMarker()
        {
            if (_recastMarkerInstance != null)
            {
                Destroy(_recastMarkerInstance);
                _recastMarkerInstance = null;
            }
        }

        /// <summary>
        /// Phase 2 (Recast): Song Phi Đoạt Mệnh — Tướng lướt vụt tới vị trí chiếc Dép đang xoay, tung cước dẫm nổ Shockwave tan xác bầy quái!
        /// </summary>
        protected override void PerformRecastSkill(Vector2 customAimDirection = default)
        {
            ClearRecastGroundMarker();

            Vector3 targetPos = _lastSlipperApexPosition;
            if (customAimDirection != Vector2.zero)
            {
                targetPos = transform.position + (Vector3)(customAimDirection * (throwRange * 1.5f));
            }

            StartCoroutine(RoutineSongPhiDropkick(targetPos));
        }

        private IEnumerator RoutineSongPhiDropkick(Vector3 targetPos)
        {
            Transform playerTf = transform.root;
            Vector3 startPos = playerTf.position;
            float dashDuration = 0.16f;
            float elapsed = 0f;

            global::Core.Audio.AudioManager.Instance?.PlayPlayerDash(startPos);

            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dashDuration);
                playerTf.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            playerTf.position = targetPos;

            // Nổ Shockwave diện rộng tại điểm đáp
            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(targetPos);
            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 4.5f, TargetingUtility.EnemyLayerMask);
            DamageData kickDamage = new DamageData(GetFinalDamage() * 3.5f, true, ElementType.Kim, true, this);

            foreach (var hit in hits)
            {
                if (hit != null && hit.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(kickDamage);
                }
                if (hit != null && hit.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 push = ((Vector2)hit.transform.position - (Vector2)targetPos).normalized;
                    rb.AddForce(push * 16f, ForceMode2D.Impulse);
                }
            }

            // Kích hoạt hiệu ứng Lốc Dép Vạn Năng bồi thêm
            StartCoroutine(RoutineWhirlwindSlippers());
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

            // Sinh Visual Chiếc Dép Bay Xoay Tròn (Thu nhỏ về tỉ lệ 0.32m chuẩn Chibi)
            GameObject slipperVisual = new GameObject("Slipper_Projectile_Visual");
            var sr = slipperVisual.AddComponent<SpriteRenderer>();
            var slipperSprite = Resources.Load<Sprite>("Tex_Slipper_Projectile");
#if UNITY_EDITOR
            if (slipperSprite == null)
            {
                slipperSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VFX/SkillLibrary/Textures/Tex_Slipper_Projectile.png");
            }
#endif
            sr.sprite = slipperSprite;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 12;
            slipperVisual.transform.localScale = Vector3.one * 0.32f; // Thu nhỏ 50% so với trước
            slipperVisual.transform.position = startPos;

            // Gắn TrailRenderer (Dải Năng Lượng Ribbon Vàng Kim uốn lượn liên tục bám theo dép)
            var trailRenderer = slipperVisual.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.22f;
            trailRenderer.startWidth = 0.35f;
            trailRenderer.endWidth = 0.02f;
            trailRenderer.minVertexDistance = 0.05f;
            trailRenderer.autodestruct = false;
            trailRenderer.sortingLayerName = "Skill";
            trailRenderer.sortingOrder = 11;

            Gradient trailGrad = new Gradient();
            trailGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.55f, 0.1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trailRenderer.colorGradient = trailGrad;

#if UNITY_EDITOR
            Material matTrail = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX/SkillLibrary/Materials/MAT_VFX_Slipper_Arc.mat");
            if (matTrail != null) trailRenderer.material = matTrail;
#endif

            // Gắn thêm Hạt Bụi Năng Lượng Lấp Lánh tản ra từ đuôi dép
            GameObject trailObj = new GameObject("Sparks");
            trailObj.transform.SetParent(slipperVisual.transform, false);
            var psTrail = trailObj.AddComponent<ParticleSystem>();
            psTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var mainT = psTrail.main;
            mainT.playOnAwake = false;
            mainT.duration = 1.0f;
            mainT.loop = true;
            mainT.startLifetime = 0.18f;
            mainT.startSpeed = 0.8f;
            mainT.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            mainT.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissT = psTrail.emission;
            emissT.rateOverTime = 25;

            var colT = psTrail.colorOverLifetime;
            colT.enabled = true;
            colT.color = trailGrad;

            var rendT = trailObj.GetComponent<ParticleSystemRenderer>();
#if UNITY_EDITOR
            Material matDrops = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX/SkillLibrary/Materials/MAT_VFX_Slipper_Drops.mat");
            if (matDrops != null) rendT.material = matDrops;
#endif
            rendT.sortingLayerName = "Skill";
            rendT.sortingOrder = 11;
            psTrail.Play();

            // 1. Bay tới đích theo đúng đường cong Parabol Bezier
            Vector2 perpendicular = new Vector2(-dir.y, dir.x);
            Vector2 controlPos = startPos + (dir.normalized * (range * 0.5f)) + (perpendicular * 1.8f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Quadratic Bezier Formula: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
                Vector2 currentPos = (1f - t) * (1f - t) * startPos + 2f * (1f - t) * t * controlPos + t * t * targetPos;
                slipperVisual.transform.position = currentPos;
                slipperVisual.transform.Rotate(0f, 0f, 1440f * Time.deltaTime); // Lộn nhào tốc độ cao
                DealDamageAtPosition(currentPos, dmg, 4f);
                yield return null;
            }

            // 2. Bay ngược về người chơi theo đường cong hồi quy
            elapsed = 0f;
            Vector2 returnStartPos = targetPos;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector2 playerPos = transform.position;
                Vector2 returnControlPos = returnStartPos + ((playerPos - returnStartPos) * 0.5f) - (perpendicular * 1.2f);
                Vector2 currentPos = (1f - t) * (1f - t) * returnStartPos + 2f * (1f - t) * t * returnControlPos + t * t * playerPos;
                slipperVisual.transform.position = currentPos;
                slipperVisual.transform.Rotate(0f, 0f, -1440f * Time.deltaTime);
                DealDamageAtPosition(currentPos, dmg, 3f);
                yield return null;
            }

            Destroy(slipperVisual);
        }

        private IEnumerator RoutineWhirlwindSlippers()
        {
            Vector2 center = transform.position;

            if (whirlwindVfxPrefab != null)
            {
                var vfxObj = ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(whirlwindVfxPrefab, center, Quaternion.identity, 0.5f);
                if (vfxObj != null)
                {
                    vfxObj.transform.localScale = Vector3.one * 0.48f; // Thu nhỏ 52% về chuẩn tỉ lệ nhân vật Chibi
                }
            }

            DamageData baseDmg = CreateDamageData();
            DamageData hitDmg = new DamageData(baseDmg.Amount * 0.5f, baseDmg.IsCritical, ElementType.Kim, baseDmg.IsCounter, this);

            // 4 đợt vả xoay tròn 360 độ (bán kính 1.8m chuẩn tỉ lệ bao quanh chân nhân vật)
            float whirlwindRadius = 1.8f;
            for (int wave = 0; wave < 4; wave++)
            {
                center = transform.position;
                int mask = TargetingUtility.EnemyLayerMask;
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, whirlwindRadius, mask);

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth > 0)
                    {
                        hp.TakeDamage(hitDmg);
                        if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                        {
                            // Kéo nhẹ quái lại gần tâm lốc
                            Vector2 pullDir = (center - (Vector2)hits[i].transform.position).normalized;
                            status.ApplyKnockback(-pullDir, 2.2f, 0.15f);

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

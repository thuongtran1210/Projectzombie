using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// W_SLIPPER — Dép Tổ Ong Thần Sa (Pháp Bảo Chủ Động Đa Giai Đoạn & Bồi Đòn — Hệ Kim).
    /// - Kỹ Năng Chủ Động (Recast 2 Phase):
    ///     + Phase 1: Quăng Boomerang Dép khổng lồ bay theo đường cong Parabol gom quái + Lốc Dép Vạn Năng.
    ///     + Phase 2 (Recast 3s): Tướng lướt vụt tới vị trí Dép xoay tung cước Song Phi dẫm nổ Shockwave 4m (350% Dmg, Knockback 16m/s, Stun 0.6s).
    /// - Kỹ Năng Bị Động:
    ///     + Hero chém trúng quái: Tự động phóng thêm 1 chiếc Dép Boomerang bồi đòn.
    ///     + Hero kết thúc Combo Hit 3: Kích hoạt Lốc Dép Vạn Năng vả liên hoàn 4 hit gây Quê Độ (Humiliated).
    /// </summary>
    public class Weapon_Slipper : WeaponBase
    {
        [Header("Slipper Settings")]
        [SerializeField] private float throwRange = 4.5f;
        [SerializeField] private float returnSpeed = 12f;
        [SerializeField] private float humiliatedChance = 0.5f;
        [SerializeField] private float autoWhirlwindCooldown = 3.5f;
        [SerializeField] private GameObject whirlwindVfxPrefab;
        [SerializeField] private Sprite slipperProjectileSprite;
        [SerializeField] private Material trailMaterial;
        [SerializeField] private Material dropsParticleMaterial;
        [SerializeField] private Sprite recastMarkerCircleSprite;

        private float _lastWhirlwindTime;

        private Vector3 _lastSlipperApexPosition;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicOnHitTrigger;
            isPrimaryActiveWeapon = false;
            hasRecastPhase = true; // Bật cơ chế Recast 2 Phase
            recastWindowDuration = 3.0f;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 6.5f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Tổ Ong Lượn Cánh";
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
            ? Combat.Aiming.SkillAimConfig.DefaultInstant // Phase 2: Khóa mục tiêu vào Dép, 1 chạm tung cước ngay lập tức
            : new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.CurvedTrajectory, throwRange * 1.5f, 1.5f, 40f, true);

        private GameObject _recastMarkerInstance;
        private Coroutine _recastMarkerTimerCoroutine;

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
                    var anim = transform.root.GetComponentInChildren<PlayerAnimator>();
                    float facing = anim != null ? anim.FacingDirection : (transform.root.localScale.x >= 0 ? 1f : -1f);
                    dir = facing >= 0f ? Vector2.right : Vector2.left;
                }
            }

            float actualThrowDist = throwRange * 1.4f;
            Vector3 startPos = transform.position;
            Vector3 desiredApexPos = startPos + (Vector3)(dir * actualThrowDist);

            // Chốt chặn an toàn: Cho phép bay qua tường ở giữa, nhưng điểm bãi đáp chiếc dép bắt buộc phải nằm hoàn toàn trên sàn gạch hợp lệ (không ra biển Left, không kẹt trong tường)
            _lastSlipperApexPosition = MovementPhysicsUtility.ValidateTeleportDestination(desiredApexPos, startPos, 0.45f);
            float finalDistance = Vector2.Distance(startPos, _lastSlipperApexPosition);
            if (finalDistance > 0.1f)
            {
                dir = ((Vector2)_lastSlipperApexPosition - (Vector2)startPos).normalized;
            }

            // Sinh/Tái sử dụng Vòng Trận Báo Hiệu Điểm Đáp Phase 2 (Recast Dropkick Beacon)
            SpawnRecastGroundMarker(_lastSlipperApexPosition);

            global::Core.Audio.AudioManager.Instance?.PlaySlash(true, transform.position);
            StartCoroutine(RoutineThrowSlipper(dir, finalDistance, 2.0f));
            StartCoroutine(RoutineWhirlwindSlippers());
        }

        private void SpawnRecastGroundMarker(Vector3 position)
        {
            if (_recastMarkerInstance == null)
            {
                _recastMarkerInstance = new GameObject("VFX_Slipper_Recast_Beacon");
                var sr = _recastMarkerInstance.AddComponent<SpriteRenderer>();
                sr.sprite = recastMarkerCircleSprite;
                sr.color = new Color(1f, 0.85f, 0.2f, 0.65f); // Vàng kim rực sáng
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 4;
                _recastMarkerInstance.transform.localScale = Vector3.one * 1.6f;
            }

            _recastMarkerInstance.transform.position = position;
            _recastMarkerInstance.SetActive(true);

            if (_recastMarkerTimerCoroutine != null) StopCoroutine(_recastMarkerTimerCoroutine);
            _recastMarkerTimerCoroutine = StartCoroutine(RoutineDisableRecastMarker(recastWindowDuration));
        }

        private IEnumerator RoutineDisableRecastMarker(float duration)
        {
            yield return new WaitForSeconds(duration);
            ClearRecastGroundMarker();
        }

        private void ClearRecastGroundMarker()
        {
            if (_recastMarkerTimerCoroutine != null)
            {
                StopCoroutine(_recastMarkerTimerCoroutine);
                _recastMarkerTimerCoroutine = null;
            }

            if (_recastMarkerInstance != null)
            {
                _recastMarkerInstance.SetActive(false);
            }
        }

        /// <summary>
        /// Phase 2 (Recast): Song Phi Đoạt Mệnh (Anchor Dropkick Leap)
        /// Khóa mục tiêu tuyệt đối vào vị trí chiếc Dép đang xoay, nhảy vọt trên không vượt mọi tường và bầy quái, đáp đất dẫm nổ Shockwave!
        /// </summary>
        protected override void PerformRecastSkill(Vector2 customAimDirection = default)
        {
            ClearRecastGroundMarker();

            Vector3 playerPos = transform.root.position;
            Vector3 targetPos = _lastSlipperApexPosition;

            // Fallback an toàn nếu chưa có vị trí Dép
            if (targetPos == Vector3.zero || Vector2.Distance(playerPos, targetPos) < 0.1f)
            {
                var anim = transform.root.GetComponentInChildren<PlayerAnimator>();
                float facing = anim != null ? anim.FacingDirection : 1f;
                Vector3 fallbackDir = facing >= 0f ? Vector3.right : Vector3.left;
                targetPos = playerPos + fallbackDir * (throwRange * 1.2f);
            }

            // Điểm đáp luôn được đảm bảo 100% nằm trên sàn hợp lệ (không ra biển Left, không kẹt tường)
            Vector3 validDashTarget = MovementPhysicsUtility.ValidateTeleportDestination(targetPos, playerPos, 0.45f);

            StartCoroutine(RoutineSongPhiDropkick(validDashTarget));
        }

        private IEnumerator RoutineSongPhiDropkick(Vector3 targetPos)
        {
            Transform playerTf = transform.root;
            Vector3 startPos = playerTf.position;
            Vector2 dashDir = ((Vector2)targetPos - (Vector2)startPos).normalized;
            float leapDuration = 0.16f;
            float elapsed = 0f;

            // 1. Kích hoạt Animation Dash & Quay mặt Tướng theo hướng phi thân
            if (playerTf.TryGetComponent<PlayerController>(out var playerCtrl))
            {
                var anim = playerTf.GetComponentInChildren<PlayerAnimator>();
                if (anim != null)
                {
                    if (dashDir.x != 0) anim.FlipToDirection(dashDir.x);
                    anim.ChangeAnimationState(PlayerAnimationState.Dash);
                }
            }

            global::Core.Audio.AudioManager.Instance?.PlayPlayerDash(startPos);

            // 2. Phi Thân Nhảy Vọt Không Gian (Aerial Parabolic Leap) — Nhảy vọt qua tường và quái
            while (elapsed < leapDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / leapDuration);
                // Ease-Out Cubic cho tốc độ phi thân cực nhanh và mạnh mẽ
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                // Cung parabol nhấc bổng trục Y nhẹ tạo cảm giác nhảy cước trên không
                float jumpArc = Mathf.Sin(t * Mathf.PI) * 0.75f;
                
                Vector3 groundPos = Vector3.Lerp(startPos, targetPos, easeT);
                playerTf.position = new Vector3(groundPos.x, groundPos.y + jumpArc, groundPos.z);
                yield return null;
            }
            playerTf.position = targetPos;

            // 3. Chạm đất tung cước Song Phi (Dropkick Impact Shockwave)
            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(targetPos);
            int kickHitCount = Physics2D.OverlapCircleNonAlloc(targetPos, 4.0f, _slipperHitBuffer, TargetingUtility.EnemyLayerMask);
            DamageData kickDamage = new DamageData(GetFinalDamage() * 3.5f, true, ElementType.Kim, true, this);

            for (int i = 0; i < kickHitCount; i++)
            {
                var hit = _slipperHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(kickDamage);
                }
                if (hit.TryGetComponent<EnemyStatusController>(out var status))
                {
                    Vector2 push = ((Vector2)hit.transform.position - (Vector2)targetPos).normalized;
                    if (push == Vector2.zero) push = dashDir;
                    status.ApplyKnockback(push, 16f, 0.35f);
                    status.ApplyStatusEffect(StatusEffectType.Stun, 0.6f);
                }
            }

            // Kích hoạt HitStop + Rung Camera đầm tay chuẩn đòn kết liễu
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestHitStop(0.08f);
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(0.35f, 0.6f);

            // Bồi thêm Lốc Dép Vạn Năng tại tâm vụ nổ Shockwave
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

        private static readonly Collider2D[] _slipperHitBuffer = new Collider2D[32];

        private void DealDamageAtPosition(Vector2 center, DamageData dmg, float radius)
        {
            int mask = TargetingUtility.EnemyLayerMask;
            int count = Physics2D.OverlapCircleNonAlloc(center, radius, _slipperHitBuffer, mask);
            for (int i = 0; i < count; i++)
            {
                var hit = _slipperHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<IDamageable>(out var dmgReceiver))
                {
                    dmgReceiver.TakeDamage(dmg);
                }

                if (hit.TryGetComponent<EnemyStatusController>(out var status))
                {
                    // Hất văng quái theo hướng lốc xoáy
                    Vector2 pushDir = ((Vector2)hit.transform.position - center).normalized;
                    status.ApplyKnockback(pushDir, 5f, 0.15f);

                    // Cơ chế Quê Độ (Humiliated)
                    if (Random.value <= (humiliatedChance * 0.5f))
                    {
                        status.ApplyStatusEffect(StatusEffectType.Humiliated, 1.5f);
                    }
                }
            }
        }

        private static Gradient _cachedTrailGrad;

        private static Gradient GetOrCreateTrailGradient()
        {
            if (_cachedTrailGrad == null)
            {
                _cachedTrailGrad = new Gradient();
                _cachedTrailGrad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.55f, 0.1f), 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
                );
            }
            return _cachedTrailGrad;
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
            sr.sprite = slipperProjectileSprite;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 12;
            slipperVisual.transform.localScale = Vector3.one * 0.32f;
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
            trailRenderer.colorGradient = GetOrCreateTrailGradient();
            if (trailMaterial != null) trailRenderer.material = trailMaterial;

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
            colT.color = GetOrCreateTrailGradient();

            var rendT = trailObj.GetComponent<ParticleSystemRenderer>();
            if (dropsParticleMaterial != null) rendT.material = dropsParticleMaterial;
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
                int count = Physics2D.OverlapCircleNonAlloc(center, whirlwindRadius, _slipperHitBuffer, mask);

                for (int i = 0; i < count; i++)
                {
                    var hit = _slipperHitBuffer[i];
                    if (hit == null) continue;

                    if (hit.TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth > 0)
                    {
                        hp.TakeDamage(hitDmg);
                        if (hit.TryGetComponent<EnemyStatusController>(out var status))
                        {
                            // Kéo nhẹ quái lại gần tâm lốc
                            Vector2 pullDir = (center - (Vector2)hit.transform.position).normalized;
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

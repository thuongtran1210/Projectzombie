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

        private void EnsureAssetsLoaded()
        {
            if (slipperProjectileSprite == null)
            {
#if UNITY_EDITOR
                slipperProjectileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/Icon_W_SLIPPER.png");
#endif
            }

            if (recastMarkerCircleSprite == null)
            {
#if UNITY_EDITOR
                recastMarkerCircleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Tex_VFX_Cinnabar_Shockwave_Ring.png");
                if (recastMarkerCircleSprite == null)
                {
                    recastMarkerCircleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Tex_VFX_DongSon_SonicWave.png");
                }
#endif
            }

            if (trailMaterial == null)
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                if (unlitShader != null)
                {
                    trailMaterial = new Material(unlitShader);
                }
            }

            if (dropsParticleMaterial == null)
            {
                Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                if (particleShader != null)
                {
                    dropsParticleMaterial = new Material(particleShader);
                }
            }
        }

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicOnHitTrigger;
            isPrimaryActiveWeapon = false;
            hasRecastPhase = true; // Bật cơ chế Recast 2 Phase
            recastWindowDuration = 3.0f;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 6.5f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Tổ Ong Lượn Cánh";

            EnsureAssetsLoaded();
        }

        protected override void PerformAttack()
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            int projectileCount = isEvolution ? 4 : (WeaponLevel >= 3 ? 2 : 1);

            // Tự động tìm kẻ địch gần nhất và ném dép Boomerang
            Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 8.0f);
            Vector2 baseDir = nearest != null ? ((Vector2)nearest.position - (Vector2)transform.position).normalized : (Vector2)transform.right;
            global::Core.Audio.AudioManager.Instance?.PlaySlash(false, transform.position);

            if (isEvolution)
            {
                // Dạng Tiến Hóa: VẠN DÉP QUY TÔNG — Bắn chùm 4 chiếc dép Boomerang khổng lồ tỏa hình nón
                float[] angles = new float[] { -25f, -8f, 8f, 25f };
                for (int i = 0; i < angles.Length; i++)
                {
                    Vector2 spreadDir = Quaternion.Euler(0, 0, angles[i]) * baseDir;
                    StartCoroutine(RoutineThrowSlipper(spreadDir, throwRange * 1.3f, 1.8f, isEvolution: true));
                }
            }
            else
            {
                // Dạng thường (Lv1 - Lv5)
                for (int i = 0; i < projectileCount; i++)
                {
                    float angleOffset = (i == 0) ? 0f : ((i % 2 == 1) ? 15f * ((i + 1) / 2) : -15f * (i / 2));
                    Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
                    StartCoroutine(RoutineThrowSlipper(dir, throwRange, 1.0f + (WeaponLevel - 1) * 0.15f, isEvolution: false));
                }
            }
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
                sr.color = new Color(1f, 0.85f, 0.25f, 0.6f); // Vàng kim thanh mảnh
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 4;
            }

            // Thu nhỏ về kích thước 0.55m chuẩn Chibi (thay vì 1.6m khổng lồ trước đó)
            _recastMarkerInstance.transform.localScale = Vector3.one * 0.55f;
            _recastMarkerInstance.transform.position = position;
            _recastMarkerInstance.SetActive(true);

            if (_recastMarkerTimerCoroutine != null) StopCoroutine(_recastMarkerTimerCoroutine);
            _recastMarkerTimerCoroutine = StartCoroutine(RoutineDisableRecastMarker(recastWindowDuration));
        }

        private IEnumerator RoutineDisableRecastMarker(float duration)
        {
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one * 0.55f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (_recastMarkerInstance != null && _recastMarkerInstance.activeSelf)
                {
                    // Xoay tròn nhẹ nhàng và co bóp theo nhịp sóng sin
                    _recastMarkerInstance.transform.Rotate(0f, 0f, -90f * Time.deltaTime);
                    float pulse = 1f + 0.08f * Mathf.Sin(elapsed * 8f);
                    _recastMarkerInstance.transform.localScale = baseScale * pulse;
                }
                yield return null;
            }

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
            bool isEvolution = WeaponLevel >= MaxLevel;
            float shockwaveRadius = isEvolution ? 7.0f : (2.5f + WeaponLevel * 0.3f);
            float damageMultiplier = isEvolution ? 6.5f : (2.2f + WeaponLevel * 0.3f);

            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(targetPos);
            int kickHitCount = Physics2D.OverlapCircleNonAlloc(targetPos, shockwaveRadius, _slipperHitBuffer, TargetingUtility.EnemyLayerMask);
            DamageData kickDamage = new DamageData(GetFinalDamage() * damageMultiplier, true, ElementType.Kim, true, this);

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
                    status.ApplyKnockback(push, isEvolution ? 24f : 14f, 0.4f);
                    status.ApplyStatusEffect(StatusEffectType.Stun, isEvolution ? 1.2f : 0.5f);
                    if (isEvolution)
                    {
                        status.ApplyStatusEffect(StatusEffectType.Humiliated, 3.0f);
                    }
                }
            }

            // Kích hoạt HitStop + Rung Camera đầm tay chuẩn đòn kết liễu
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestHitStop(isEvolution ? 0.15f : 0.06f);
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(isEvolution ? 0.6f : 0.25f, isEvolution ? 0.8f : 0.4f);

            // Bồi thêm Lốc Dép Vạn Năng tại tâm vụ nổ Shockwave (Tiến hóa nổ 8 đợt vả lốc xoáy)
            StartCoroutine(RoutineWhirlwindSlippers(isEvolution));
        }

        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            // Khi Hero chém trúng quái: Bồi thêm dép Boomerang phóng thẳng vào mục tiêu (Lv1: 30%, Tiến Hóa: 100%)
            float procChance = isEvolution ? 1.0f : (0.25f + WeaponLevel * 0.08f);
            if (enemyHit != null && Random.value <= procChance)
            {
                Vector2 dir = ((Vector2)enemyHit.transform.position - (Vector2)transform.position).normalized;
                StartCoroutine(RoutineThrowSlipper(dir, throwRange, isEvolution ? 1.5f : 0.8f, isEvolution: isEvolution));
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

        private IEnumerator RoutineThrowSlipper(Vector2 dir, float range, float dmgMult, bool isEvolution = false)
        {
            Vector2 startPos = transform.position;
            Vector2 targetPos = startPos + dir.normalized * range;
            float duration = range / returnSpeed;
            float elapsed = 0f;

            DamageData dmg = CreateDamageData();
            dmg = new DamageData(dmg.Amount * dmgMult, dmg.IsCritical, ElementType.Kim, dmg.IsCounter, this);

            // Sinh Visual Chiếc Dép Bay Xoay Tròn:
            // Lv1: Tỉ lệ nhỏ gọn 0.28m, Trail mỏng
            // Tiến Hóa (E_SLIPPER): Dép Vàng Khổng Lồ 0.75m, hào quang cực sáng
            GameObject slipperVisual = new GameObject(isEvolution ? "Giant_Golden_Slipper_Visual" : "Slipper_Projectile_Visual");
            var sr = slipperVisual.AddComponent<SpriteRenderer>();
            sr.sprite = slipperProjectileSprite;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 12;
            float scaleMultiplier = isEvolution ? 0.75f : (0.28f + WeaponLevel * 0.04f);
            slipperVisual.transform.localScale = Vector3.one * scaleMultiplier;
            slipperVisual.transform.position = startPos;
            if (isEvolution) sr.color = new Color(1f, 0.95f, 0.4f, 1f); // Hào quang hoàng kim

            // Gắn TrailRenderer (Dải Năng Lượng Ribbon Vàng Kim)
            var trailRenderer = slipperVisual.AddComponent<TrailRenderer>();
            trailRenderer.time = isEvolution ? 0.35f : 0.18f;
            trailRenderer.startWidth = isEvolution ? 0.75f : 0.25f;
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
            mainT.startLifetime = isEvolution ? 0.35f : 0.15f;
            mainT.startSpeed = isEvolution ? 2.0f : 0.6f;
            mainT.startSize = new ParticleSystem.MinMaxCurve(isEvolution ? 0.4f : 0.15f, isEvolution ? 0.8f : 0.3f);
            mainT.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissT = psTrail.emission;
            emissT.rateOverTime = isEvolution ? 60 : 20;

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
            float arcOffset = isEvolution ? 2.6f : 1.4f;
            Vector2 controlPos = startPos + (dir.normalized * (range * 0.5f)) + (perpendicular * arcOffset);
            float hitRadius = isEvolution ? 4.5f : (1.8f + WeaponLevel * 0.2f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Quadratic Bezier Formula: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
                Vector2 currentPos = (1f - t) * (1f - t) * startPos + 2f * (1f - t) * t * controlPos + t * t * targetPos;
                slipperVisual.transform.position = currentPos;
                slipperVisual.transform.Rotate(0f, 0f, (isEvolution ? 2160f : 1440f) * Time.deltaTime);
                DealDamageAtPosition(currentPos, dmg, hitRadius);
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
                Vector2 returnControlPos = returnStartPos + ((playerPos - returnStartPos) * 0.5f) - (perpendicular * (arcOffset * 0.8f));
                Vector2 currentPos = (1f - t) * (1f - t) * returnStartPos + 2f * (1f - t) * t * returnControlPos + t * t * playerPos;
                slipperVisual.transform.position = currentPos;
                slipperVisual.transform.Rotate(0f, 0f, -(isEvolution ? 2160f : 1440f) * Time.deltaTime);
                DealDamageAtPosition(currentPos, dmg, hitRadius * 0.85f);
                yield return null;
            }

            Destroy(slipperVisual);
        }

        private IEnumerator RoutineWhirlwindSlippers(bool isEvolution = false)
        {
            Vector2 center = transform.position;
            float whirlwindRadius = isEvolution ? 4.5f : (1.6f + WeaponLevel * 0.15f);
            int wavesCount = isEvolution ? 8 : (WeaponLevel >= 3 ? 5 : 3);

            if (whirlwindVfxPrefab != null)
            {
                var vfxObj = ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(whirlwindVfxPrefab, center, Quaternion.identity, 0.6f, WeaponLevel);
                if (vfxObj != null)
                {
                    vfxObj.transform.localScale = Vector3.one * (isEvolution ? 1.2f : 0.45f);
                }
            }
            else if (recastMarkerCircleSprite != null)
            {
                // Fallback tạo hiệu ứng Lốc Xoáy Bão Dép bằng Sprite Vàng Kim bùng nổ xoay tròn
                GameObject whirlVisual = new GameObject("VFX_Whirlwind_Fallback");
                whirlVisual.transform.position = center;
                var sr = whirlVisual.AddComponent<SpriteRenderer>();
                sr.sprite = recastMarkerCircleSprite;
                sr.color = isEvolution ? new Color(1f, 0.95f, 0.2f, 0.95f) : new Color(1f, 0.9f, 0.3f, 0.7f);
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 14;
                whirlVisual.transform.localScale = Vector3.one * (isEvolution ? 1.0f : 0.4f);

                StartCoroutine(RoutineAnimateWhirlwindVisual(whirlVisual, isEvolution ? 0.7f : 0.35f, isEvolution ? 4.8f : 2.0f));
            }

            DamageData baseDmg = CreateDamageData();
            DamageData hitDmg = new DamageData(baseDmg.Amount * (isEvolution ? 0.9f : 0.4f), baseDmg.IsCritical, ElementType.Kim, baseDmg.IsCounter, this);

            for (int wave = 0; wave < wavesCount; wave++)
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
                            // Kéo quái vào tâm lốc
                            Vector2 pullDir = (center - (Vector2)hit.transform.position).normalized;
                            status.ApplyKnockback(-pullDir, isEvolution ? 4.0f : 2.0f, 0.15f);

                            // Áp dụng Quê Độ (Humiliated)
                            if ((wave == wavesCount - 1 || isEvolution) && Random.value <= (isEvolution ? 0.9f : humiliatedChance))
                            {
                                status.ApplyStatusEffect(StatusEffectType.Humiliated, isEvolution ? 3.5f : 1.5f);
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(0.08f);
            }
        }

        private IEnumerator RoutineAnimateWhirlwindVisual(GameObject vfxObj, float duration, float targetScaleMax = 2.2f)
        {
            float elapsed = 0f;
            var sr = vfxObj.GetComponent<SpriteRenderer>();
            Color startColor = sr != null ? sr.color : Color.white;
            Vector3 startScale = Vector3.one * 0.4f;
            Vector3 targetScale = Vector3.one * targetScaleMax;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (vfxObj != null)
                {
                    vfxObj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    vfxObj.transform.Rotate(0f, 0f, 720f * Time.deltaTime);
                    if (sr != null)
                    {
                        sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
                    }
                }
                yield return null;
            }

            if (vfxObj != null)
            {
                Destroy(vfxObj);
            }
        }
    }
}

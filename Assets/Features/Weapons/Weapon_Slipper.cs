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
    /// W_SLIPPER — Dép Tổ Ong Thần Sa (Pháp Bảo Chủ Động Đa Giai Đoạn & Bồi Đòn — Hệ Kim).
    /// - Level 1 (Khởi Đầu):
    ///     + Đòn đánh: Ném 1 chiếc dép nhựa đơn lẻ bay theo đường cong Parabol rồi hồi quy về tay.
    ///     + Active Phase 1: Ném chiếc dép tới điểm chỉ định cắm xuống đất tạo bãi đáp.
    ///     + Active Phase 2: Nhảy tới dép dẫm nổ Shockwave nhỏ (2.2m, 200% Dmg, Stun 0.4s).
    ///     + Bị động: Bồi đòn 15% khi đánh trúng, Combo 3 kích lốc dép nhỏ.
    /// - Level 6 (Tiến Hóa: VẠN DÉP QUY TÔNG):
    ///     + [MỚI] Hộ Thể Song Dép Hoàng Kim: 2 chiếc Dép Vàng xoay quanh Hero chém văng mọi quái tiếp cận.
    ///     + [MỚI] Ma Trận Vạn Dép Hội Tụ: 4 chiếc dép vàng bay 4 hướng rồi ĐỒNG LOẠT LAO VỀ TÂM HỘI TỤ NỔ SHOCKWAVE gom toàn map.
    ///     + [MỚI] Phase 1 Trận Cột Hoàng Kim: Dép cắm đất biến thành Trụ Thần Sa phóng 3 đợt sóng xung kích giật sét.
    ///     + [MỚI] Phase 2 Thiên Cước Thiên Thạch: Tướng bay vút lên cao rồi giáng gót chân như thiên thạch nổ 3 tầng Shockwave 8m, áp hiệu ứng "Quê Độ Cuồng Bạo" 100% khiến quái đánh lẫn nhau trong 4s!
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

        // --- CƠ CHẾ TIẾN HÓA: HỘ THỂ SONG DÉP HOÀNG KIM ---
        private GameObject _orbitingShieldRoot;
        private Transform[] _orbitingSlippers;
        private float _orbitAngle;
        private float _lastOrbitDamageTime;

        private void EnsureAssetsLoaded()
        {
            if (slipperProjectileSprite == null)
            {
#if UNITY_EDITOR
                slipperProjectileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/Icon_W_SLIPPER.png");
#endif
                if (slipperProjectileSprite == null && icon != null)
                {
                    slipperProjectileSprite = icon;
                }
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
            hasRecastPhase = true;
            recastWindowDuration = 3.2f;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 6.5f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Tổ Ong Lượn Cánh";

            EnsureAssetsLoaded();
        }

        private void Update()
        {
            UpdateOrbitingSlippersAura();
        }

        private void OnDisable()
        {
            CleanupOrbitingShield();
            ClearRecastGroundMarker();
        }

        private void OnDestroy()
        {
            CleanupOrbitingShield();
            ClearRecastGroundMarker();
        }

        #region EVOLUTION PASSIVE: ORBITING SLIPPERS SHIELD
        private void UpdateOrbitingSlippersAura()
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            if (!isEvolution)
            {
                if (_orbitingShieldRoot != null) CleanupOrbitingShield();
                return;
            }

            // Khởi tạo 2 chiếc Dép Vàng Hộ Thể nếu chưa có
            if (_orbitingShieldRoot == null)
            {
                _orbitingShieldRoot = new GameObject("Evolution_Orbiting_Slippers_Shield");
                _orbitingShieldRoot.transform.SetParent(transform.root, false);
                _orbitingShieldRoot.transform.localPosition = Vector3.zero;

                _orbitingSlippers = new Transform[2];
                for (int i = 0; i < 2; i++)
                {
                    var slipper = new GameObject($"Orbit_Slipper_{i}");
                    slipper.transform.SetParent(_orbitingShieldRoot.transform, false);
                    var sr = slipper.AddComponent<SpriteRenderer>();
                    sr.sprite = slipperProjectileSprite;
                    sr.color = new Color(1f, 0.92f, 0.35f, 1f); // Hoàng Kim rực rỡ
                    sr.sortingLayerName = "Skill";
                    sr.sortingOrder = 13;
                    slipper.transform.localScale = Vector3.one * 0.45f;

                    // Dải trail nhỏ theo sau dép hộ thể
                    var tr = slipper.AddComponent<TrailRenderer>();
                    tr.time = 0.25f;
                    tr.startWidth = 0.22f;
                    tr.endWidth = 0.02f;
                    tr.minVertexDistance = 0.05f;
                    tr.sortingLayerName = "Skill";
                    tr.sortingOrder = 12;
                    tr.colorGradient = GetOrCreateTrailGradient();
                    if (trailMaterial != null) tr.material = trailMaterial;

                    _orbitingSlippers[i] = slipper.transform;
                }
            }

            // Xoay tròn quanh Hero
            _orbitAngle += 360f * Time.deltaTime;
            float radius = 1.35f;
            for (int i = 0; i < 2; i++)
            {
                if (_orbitingSlippers[i] == null) continue;
                float angleRad = (_orbitAngle + i * 180f) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * (radius * 0.75f), 0f);
                _orbitingSlippers[i].localPosition = offset;
                _orbitingSlippers[i].localRotation = Quaternion.Euler(0f, 0f, (_orbitAngle + i * 180f) + 90f);
            }

            // Gây sát thương và đẩy lùi quái lại gần
            if (Time.time >= _lastOrbitDamageTime + 0.35f)
            {
                _lastOrbitDamageTime = Time.time;
                DealOrbitContactDamage();
            }
        }

        private void DealOrbitContactDamage()
        {
            Vector3 center = transform.root.position;
            int count = Physics2D.OverlapCircleNonAlloc(center, 1.6f, _slipperHitBuffer, TargetingUtility.EnemyLayerMask);
            if (count <= 0) return;

            DamageData auraDmg = new DamageData(GetFinalDamage() * 0.55f, false, ElementType.Kim, false, this);
            for (int i = 0; i < count; i++)
            {
                var hit = _slipperHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(auraDmg);
                }
                if (hit.TryGetComponent<EnemyStatusController>(out var status))
                {
                    Vector2 push = ((Vector2)hit.transform.position - (Vector2)center).normalized;
                    status.ApplyKnockback(push, 8f, 0.15f);
                }
            }
        }

        private void CleanupOrbitingShield()
        {
            if (_orbitingShieldRoot != null)
            {
                Destroy(_orbitingShieldRoot);
                _orbitingShieldRoot = null;
                _orbitingSlippers = null;
            }
        }
        #endregion

        #region ATTACK SYSTEM (LV1 SINGLE SLIPPER vs EVOLUTION CONVERGENCE MATRIX)
        protected override void PerformAttack()
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 10.0f);
            Vector2 baseDir = nearest != null ? ((Vector2)nearest.position - (Vector2)transform.position).normalized : (Vector2)transform.right;
            global::Core.Audio.AudioManager.Instance?.PlaySlash(false, transform.position);

            if (isEvolution)
            {
                // TIẾN HÓA: MA TRẬN VẠN DÉP HỘI TỤ (Slipper Convergence Matrix Slam)
                // 4 chiếc dép vàng phóng cự ly cực đại (7.5m), xé toạc toàn bộ màn hình phía trước
                StartCoroutine(RoutineEvolutionConvergenceMatrix(baseDir));
            }
            else
            {
                // DẠNG THƯỜNG (Lv1 - Lv5): Ném 1-2 chiếc dép Boomerang đơn lẻ cự ly chuẩn (3.8m - 4.8m)
                int projectileCount = WeaponLevel >= 4 ? 2 : 1;
                float currentRange = 3.8f + (WeaponLevel - 1) * 0.25f;
                for (int i = 0; i < projectileCount; i++)
                {
                    float angleOffset = (i == 0) ? 0f : ((i % 2 == 1) ? 12f * ((i + 1) / 2) : -12f * (i / 2));
                    Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
                    StartCoroutine(RoutineThrowSlipper(dir, currentRange, 1.0f + (WeaponLevel - 1) * 0.18f, isEvolution: false));
                }
            }
        }

        private IEnumerator RoutineEvolutionConvergenceMatrix(Vector2 baseDir)
        {
            // Tỏa hình nón góc vàng (-24°, -8°, 8°, 24°), tầm bắn 7.2m vượt trội hoàn toàn Lv1
            float[] angles = new float[] { -24f, -8f, 8f, 24f };
            float evoRange = 7.2f;
            Vector2 startPos = transform.position;
            Vector2 convergenceCenter = startPos + baseDir * (evoRange * 0.95f);

            // Giai đoạn 1: 4 Dép Vàng Khổng Lồ phóng vút xé gió ra cực xa
            for (int i = 0; i < angles.Length; i++)
            {
                Vector2 spreadDir = Quaternion.Euler(0, 0, angles[i]) * baseDir;
                StartCoroutine(RoutineThrowSlipper(spreadDir, evoRange, 1.8f, isEvolution: true, targetConvergencePoint: convergenceCenter));
            }

            yield return null;
        }
        #endregion

        #region ACTIVE RELIC SKILL (PHASE 1: RIFT TOTEM & PHASE 2: METEOR DROPKICK)
        public override Combat.Aiming.SkillAimConfig AimConfig => IsInRecastWindow 
            ? Combat.Aiming.SkillAimConfig.DefaultInstant
            : new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.CurvedTrajectory, (WeaponLevel >= MaxLevel ? 7.5f : 4.5f), 1.5f, 40f, true);

        private GameObject _recastMarkerInstance;
        private Coroutine _recastMarkerTimerCoroutine;

        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            Vector2 dir = customAimDirection;
            if (dir == Vector2.zero)
            {
                Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 10.0f);
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

            // Kỹ năng chủ động: Lv1 ném 4.2m; Tiến hóa ném xa 7.5m
            float actualThrowDist = isEvolution ? 7.5f : (4.2f + (WeaponLevel - 1) * 0.3f);
            Vector3 startPos = transform.position;
            Vector3 desiredApexPos = startPos + (Vector3)(dir * actualThrowDist);

            _lastSlipperApexPosition = MovementPhysicsUtility.ValidateTeleportDestination(desiredApexPos, startPos, 0.45f);
            float finalDistance = Vector2.Distance(startPos, _lastSlipperApexPosition);
            if (finalDistance > 0.1f)
            {
                dir = ((Vector2)_lastSlipperApexPosition - (Vector2)startPos).normalized;
            }

            // Sinh Vòng Trận / Trụ Thần Sa Hoàng Kim tại điểm đáp
            SpawnRecastGroundMarker(_lastSlipperApexPosition, isEvolution);

            global::Core.Audio.AudioManager.Instance?.PlaySlash(true, transform.position);
            StartCoroutine(RoutineThrowSlipper(dir, finalDistance, isEvolution ? 2.5f : 1.8f, isEvolution: isEvolution));
            StartCoroutine(RoutineWhirlwindSlippers(isEvolution));
        }

        private void SpawnRecastGroundMarker(Vector3 position, bool isEvolution = false)
        {
            if (_recastMarkerInstance == null)
            {
                _recastMarkerInstance = new GameObject("VFX_Slipper_Recast_Beacon");
                var sr = _recastMarkerInstance.AddComponent<SpriteRenderer>();
                sr.sprite = recastMarkerCircleSprite;
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 4;
            }

            var srMarker = _recastMarkerInstance.GetComponent<SpriteRenderer>();
            srMarker.color = isEvolution ? new Color(1f, 0.95f, 0.2f, 0.95f) : new Color(1f, 0.85f, 0.25f, 0.6f);
            _recastMarkerInstance.transform.localScale = Vector3.one * (isEvolution ? 1.0f : 0.55f);
            _recastMarkerInstance.transform.position = position;
            _recastMarkerInstance.SetActive(true);

            if (_recastMarkerTimerCoroutine != null) StopCoroutine(_recastMarkerTimerCoroutine);
            _recastMarkerTimerCoroutine = StartCoroutine(RoutineDisableRecastMarker(recastWindowDuration, isEvolution));
        }

        private IEnumerator RoutineDisableRecastMarker(float duration, bool isEvolution)
        {
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one * (isEvolution ? 1.0f : 0.55f);
            float pulseTimer = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                pulseTimer += Time.deltaTime;

                if (_recastMarkerInstance != null && _recastMarkerInstance.activeSelf)
                {
                    _recastMarkerInstance.transform.Rotate(0f, 0f, (isEvolution ? -180f : -90f) * Time.deltaTime);
                    float pulse = 1f + (isEvolution ? 0.15f : 0.08f) * Mathf.Sin(elapsed * 8f);
                    _recastMarkerInstance.transform.localScale = baseScale * pulse;

                    // [TIẾN HÓA] Trụ Thần Sa phóng 3 đợt sóng xung kích điện kim trong thời gian chờ Recast
                    if (isEvolution && pulseTimer >= 0.9f)
                    {
                        pulseTimer = 0f;
                        DealTotemPulseWave(_recastMarkerInstance.transform.position);
                    }
                }
                yield return null;
            }

            ClearRecastGroundMarker();
        }

        private void DealTotemPulseWave(Vector3 center)
        {
            int count = Physics2D.OverlapCircleNonAlloc(center, 3.8f, _slipperHitBuffer, TargetingUtility.EnemyLayerMask);
            DamageData pulseDmg = new DamageData(GetFinalDamage() * 0.75f, false, ElementType.Kim, false, this);
            for (int i = 0; i < count; i++)
            {
                var hit = _slipperHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(pulseDmg);
                }
                if (hit.TryGetComponent<EnemyStatusController>(out var status))
                {
                    // Kéo quái về phía Trụ Thần Sa
                    Vector2 pull = ((Vector2)center - (Vector2)hit.transform.position).normalized;
                    status.ApplyKnockback(pull, 6.0f, 0.15f);
                    status.ApplyStatusEffect(StatusEffectType.Slow, 1.5f);
                }
            }
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

        protected override void PerformRecastSkill(Vector2 customAimDirection = default)
        {
            ClearRecastGroundMarker();

            Vector3 playerPos = transform.root.position;
            Vector3 targetPos = _lastSlipperApexPosition;

            if (targetPos == Vector3.zero || Vector2.Distance(playerPos, targetPos) < 0.1f)
            {
                var anim = transform.root.GetComponentInChildren<PlayerAnimator>();
                float facing = anim != null ? anim.FacingDirection : 1f;
                Vector3 fallbackDir = facing >= 0f ? Vector3.right : Vector3.left;
                targetPos = playerPos + fallbackDir * (throwRange * 1.2f);
            }

            Vector3 validDashTarget = MovementPhysicsUtility.ValidateTeleportDestination(targetPos, playerPos, 0.45f);
            StartCoroutine(RoutineSongPhiDropkick(validDashTarget));
        }

        private IEnumerator RoutineSongPhiDropkick(Vector3 targetPos)
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            Transform playerTf = transform.root;
            Vector3 startPos = playerTf.position;
            Vector2 dashDir = ((Vector2)targetPos - (Vector2)startPos).normalized;
            float leapDuration = isEvolution ? 0.22f : 0.16f;
            float elapsed = 0f;

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

            // [TIẾN HÓA] Phi Thân Thiên Thạch (Meteor Leap) — Nhảy cao vọt biến mất khỏi tầm quái rồi giáng gót
            float peakHeight = isEvolution ? 2.5f : 0.75f;
            while (elapsed < leapDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / leapDuration);
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                float jumpArc = Mathf.Sin(t * Mathf.PI) * peakHeight;
                
                Vector3 groundPos = Vector3.Lerp(startPos, targetPos, easeT);
                playerTf.position = new Vector3(groundPos.x, groundPos.y + jumpArc, groundPos.z);
                yield return null;
            }
            playerTf.position = targetPos;

            // [TIẾN HÓA] 3 Tầng Shockwave & Hiệu ứng Quê Độ Cuồng Bạo (Mass Humiliated Berserk)
            float shockwaveRadius = isEvolution ? 8.0f : (2.2f + WeaponLevel * 0.25f);
            float damageMultiplier = isEvolution ? 7.5f : (2.0f + WeaponLevel * 0.25f);

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
                    status.ApplyKnockback(push, isEvolution ? 28f : 12f, 0.35f);
                    status.ApplyStatusEffect(StatusEffectType.Stun, isEvolution ? 1.5f : 0.4f);
                    
                    if (isEvolution)
                    {
                        // Quê độ tuyệt đối 100% trong 4.0s khiến quái quay sang đấm lẫn nhau
                        status.ApplyStatusEffect(StatusEffectType.Humiliated, 4.0f);
                    }
                }
            }

            // HitStop + Camera Shake đầm tay chuẩn đòn kết liễu
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestHitStop(isEvolution ? 0.20f : 0.05f);
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(isEvolution ? 0.8f : 0.18f, isEvolution ? 0.9f : 0.3f);

            // Bồi thêm Lốc Dép Vạn Năng tại tâm vụ nổ Shockwave
            StartCoroutine(RoutineWhirlwindSlippers(isEvolution));
        }
        #endregion

        #region HERO TRIGGERS (ON-HIT & COMBO FINISHER)
        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            float procChance = isEvolution ? 1.0f : (0.15f + WeaponLevel * 0.07f);
            if (enemyHit != null && Random.value <= procChance)
            {
                Vector2 dir = ((Vector2)enemyHit.transform.position - (Vector2)transform.position).normalized;
                StartCoroutine(RoutineThrowSlipper(dir, throwRange, isEvolution ? 2.0f : 0.75f, isEvolution: isEvolution));
            }
        }

        public override void OnHeroComboFinished(int finalStep, Vector2 attackDirection)
        {
            if (finalStep == 3 && Time.time >= _lastWhirlwindTime + autoWhirlwindCooldown)
            {
                _lastWhirlwindTime = Time.time;
                StartCoroutine(RoutineWhirlwindSlippers(WeaponLevel >= MaxLevel));
            }
        }
        #endregion

        #region DAMAGE UTILITIES & ROUTINES
        private static readonly Collider2D[] _slipperHitBuffer = new Collider2D[64];

        private void DealDamageAtPosition(Vector2 center, DamageData dmg, float radius, bool isEvolution = false)
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
                    Vector2 pushDir = ((Vector2)hit.transform.position - center).normalized;
                    status.ApplyKnockback(pushDir, isEvolution ? 10f : 5f, 0.15f);

                    if (isEvolution || Random.value <= (humiliatedChance * 0.5f))
                    {
                        status.ApplyStatusEffect(StatusEffectType.Humiliated, isEvolution ? 3.0f : 1.5f);
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
                    new GradientColorKey[] { 
                        new GradientColorKey(new Color(1f, 1f, 0.85f), 0f),       // Lõi trắng vàng chói lòa
                        new GradientColorKey(new Color(1f, 0.85f, 0.25f), 0.35f),  // Thân vàng kim rực rỡ
                        new GradientColorKey(new Color(1f, 0.45f, 0.05f), 1f)     // Đuôi hổ phách nguyên tố Kim
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(1f, 0f), 
                        new GradientAlphaKey(0.85f, 0.5f), 
                        new GradientAlphaKey(0f, 1f) 
                    }
                );
            }
            return _cachedTrailGrad;
        }

        private IEnumerator RoutineThrowSlipper(Vector2 dir, float range, float dmgMult, bool isEvolution = false, Vector2? targetConvergencePoint = null)
        {
            Vector2 startPos = transform.position;
            Vector2 targetPos = startPos + dir.normalized * range;
            float duration = range / (returnSpeed * (isEvolution ? 1.35f : 1.0f));
            float elapsed = 0f;

            DamageData dmg = CreateDamageData();
            dmg = new DamageData(dmg.Amount * dmgMult, dmg.IsCritical, ElementType.Kim, dmg.IsCounter, this);

            GameObject slipperVisual = new GameObject(isEvolution ? "Giant_Golden_Slipper_Visual" : "Slipper_Projectile_Visual");
            var sr = slipperVisual.AddComponent<SpriteRenderer>();
            sr.sprite = slipperProjectileSprite;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 13;
            float scaleMultiplier = isEvolution ? 0.9f : (0.28f + WeaponLevel * 0.035f);
            slipperVisual.transform.localScale = Vector3.one * scaleMultiplier;
            slipperVisual.transform.position = startPos;
            if (isEvolution) sr.color = new Color(1f, 0.95f, 0.45f, 1f);

            // [MỚI] Hào Quang Hoàng Kim (Golden Aura Glow Sprite) bọc quanh thân dép
            GameObject auraObj = new GameObject("Aura_Glow");
            auraObj.transform.SetParent(slipperVisual.transform, false);
            var srAura = auraObj.AddComponent<SpriteRenderer>();
            srAura.sprite = recastMarkerCircleSprite != null ? recastMarkerCircleSprite : slipperProjectileSprite;
            srAura.color = isEvolution ? new Color(1f, 0.85f, 0.2f, 0.55f) : new Color(1f, 0.9f, 0.4f, 0.35f);
            srAura.sortingLayerName = "Skill";
            srAura.sortingOrder = 12;
            auraObj.transform.localScale = Vector3.one * (isEvolution ? 1.5f : 1.25f);

            var trailRenderer = slipperVisual.AddComponent<TrailRenderer>();
            trailRenderer.time = isEvolution ? 0.42f : 0.18f;
            trailRenderer.startWidth = isEvolution ? 0.95f : 0.22f;
            trailRenderer.endWidth = 0.02f;
            trailRenderer.minVertexDistance = 0.035f;
            trailRenderer.autodestruct = false;
            trailRenderer.sortingLayerName = "Skill";
            trailRenderer.sortingOrder = 11;
            trailRenderer.colorGradient = GetOrCreateTrailGradient();
            if (trailMaterial != null) trailRenderer.material = trailMaterial;

            GameObject trailObj = new GameObject("Sparks");
            trailObj.transform.SetParent(slipperVisual.transform, false);
            var psTrail = trailObj.AddComponent<ParticleSystem>();
            psTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var mainT = psTrail.main;
            mainT.playOnAwake = false;
            mainT.duration = 1.0f;
            mainT.loop = true;
            mainT.startLifetime = isEvolution ? 0.45f : 0.15f;
            mainT.startSpeed = isEvolution ? 3.0f : 0.6f;
            mainT.startSize = new ParticleSystem.MinMaxCurve(isEvolution ? 0.5f : 0.15f, isEvolution ? 1.0f : 0.3f);
            mainT.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissT = psTrail.emission;
            emissT.rateOverTime = isEvolution ? 80 : 15;

            var colT = psTrail.colorOverLifetime;
            colT.enabled = true;
            colT.color = GetOrCreateTrailGradient();

            var rendT = trailObj.GetComponent<ParticleSystemRenderer>();
            if (dropsParticleMaterial != null) rendT.material = dropsParticleMaterial;
            rendT.sortingLayerName = "Skill";
            rendT.sortingOrder = 11;
            psTrail.Play();

            Vector2 perpendicular = new Vector2(-dir.y, dir.x);
            float arcOffset = isEvolution ? 1.6f : 1.0f;
            Vector2 controlPos = startPos + (dir.normalized * (range * 0.5f)) + (perpendicular * arcOffset);
            float hitRadius = isEvolution ? 5.0f : (1.5f + WeaponLevel * 0.18f);
            float spinSpeed = isEvolution ? 2520f : 1080f;

            // 1. Bay tới đích
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector2 currentPos = (1f - t) * (1f - t) * startPos + 2f * (1f - t) * t * controlPos + t * t * targetPos;
                slipperVisual.transform.position = currentPos;
                slipperVisual.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
                DealDamageAtPosition(currentPos, dmg, hitRadius, isEvolution);
                yield return null;
            }

            // 2. Bay ngược về (Nếu có targetConvergencePoint thì lao về điểm hội tụ)
            elapsed = 0f;
            Vector2 returnStartPos = targetPos;
            Vector2 returnTargetPos = targetConvergencePoint.HasValue ? targetConvergencePoint.Value : (Vector2)transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (!targetConvergencePoint.HasValue) returnTargetPos = (Vector2)transform.position;

                Vector2 returnControlPos = returnStartPos + ((returnTargetPos - returnStartPos) * 0.5f) - (perpendicular * (arcOffset * 0.8f));
                Vector2 currentPos = (1f - t) * (1f - t) * returnStartPos + 2f * (1f - t) * t * returnControlPos + t * t * returnTargetPos;
                slipperVisual.transform.position = currentPos;
                slipperVisual.transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
                DealDamageAtPosition(currentPos, dmg, hitRadius * 0.85f, isEvolution);
                yield return null;
            }

            // Khi 4 dép hội tụ tại điểm tâm -> Kích nổ Hợp Kích (Convergence Shockwave)
            if (targetConvergencePoint.HasValue)
            {
                DealConvergenceShockwave(returnTargetPos, dmg);
            }

            Destroy(slipperVisual);
        }

        private void DealConvergenceShockwave(Vector2 center, DamageData baseDmg)
        {
            // Sinh Visual Sóng Kích Hội Tụ Hoàng Kim bùng nổ (Golden Shockwave Ring & Core Burst)
            if (recastMarkerCircleSprite != null)
            {
                GameObject slamVfx = new GameObject("VFX_Convergence_Slam_Burst");
                slamVfx.transform.position = center;
                var sr = slamVfx.AddComponent<SpriteRenderer>();
                sr.sprite = recastMarkerCircleSprite;
                sr.color = new Color(1f, 0.95f, 0.4f, 0.95f);
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 15;
                slamVfx.transform.localScale = Vector3.one * 0.4f;

                StartCoroutine(RoutineAnimateWhirlwindVisual(slamVfx, 0.4f, 4.5f));
            }

            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(center);
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(0.35f, 0.4f);

            DamageData slamDmg = new DamageData(baseDmg.Amount * 1.8f, true, ElementType.Kim, true, this);
            int count = Physics2D.OverlapCircleNonAlloc(center, 5.5f, _slipperHitBuffer, TargetingUtility.EnemyLayerMask);
            for (int i = 0; i < count; i++)
            {
                var hit = _slipperHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(slamDmg);
                }
                if (hit.TryGetComponent<EnemyStatusController>(out var status))
                {
                    // Hút toàn bộ quái về tâm hội tụ
                    Vector2 pull = (center - (Vector2)hit.transform.position).normalized;
                    status.ApplyKnockback(pull, 12f, 0.25f);
                    status.ApplyStatusEffect(StatusEffectType.Humiliated, 3.0f);
                }
            }
        }

        private IEnumerator RoutineWhirlwindSlippers(bool isEvolution = false)
        {
            Vector2 center = transform.position;
            float whirlwindRadius = isEvolution ? 5.5f : (1.4f + WeaponLevel * 0.18f);
            int wavesCount = isEvolution ? 8 : (WeaponLevel >= 3 ? 4 : 2);

            if (whirlwindVfxPrefab != null)
            {
                var vfxObj = ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(whirlwindVfxPrefab, center, Quaternion.identity, 0.6f, WeaponLevel);
                if (vfxObj != null)
                {
                    vfxObj.transform.localScale = Vector3.one * (isEvolution ? 1.4f : 0.4f);
                }
            }
            else if (recastMarkerCircleSprite != null)
            {
                GameObject whirlVisual = new GameObject("VFX_Whirlwind_Fallback");
                whirlVisual.transform.position = center;
                var sr = whirlVisual.AddComponent<SpriteRenderer>();
                sr.sprite = recastMarkerCircleSprite;
                sr.color = isEvolution ? new Color(1f, 0.95f, 0.2f, 0.95f) : new Color(1f, 0.9f, 0.3f, 0.6f);
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 14;
                whirlVisual.transform.localScale = Vector3.one * (isEvolution ? 1.2f : 0.35f);

                StartCoroutine(RoutineAnimateWhirlwindVisual(whirlVisual, isEvolution ? 0.8f : 0.3f, isEvolution ? 5.2f : 1.8f));
            }

            DamageData baseDmg = CreateDamageData();
            DamageData hitDmg = new DamageData(baseDmg.Amount * (isEvolution ? 1.1f : 0.35f), baseDmg.IsCritical, ElementType.Kim, baseDmg.IsCounter, this);

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
                            Vector2 pullDir = (center - (Vector2)hit.transform.position).normalized;
                            status.ApplyKnockback(-pullDir, isEvolution ? 6.0f : 1.8f, 0.15f);

                            if ((wave == wavesCount - 1 || isEvolution) && Random.value <= (isEvolution ? 1.0f : (humiliatedChance * 0.6f)))
                            {
                                status.ApplyStatusEffect(StatusEffectType.Humiliated, isEvolution ? 4.0f : 1.2f);
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
        #endregion
    }
}

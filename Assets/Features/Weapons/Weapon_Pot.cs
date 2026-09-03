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
    /// W_POT — Nồi Cơm Thạch Sanh (Pháp Bảo Hộ Vệ Quỹ Đạo & Hồi Phục — Hệ Thổ).
    /// - Level 1-5: Nồi Gang bay sau lưng; Tự động hút quái tiếp cận, bắn pháo quái văng xa và rơi Cơm Nắm Tiên hồi 8% HP.
    /// - Level 6 (Tiến Hóa: E_POT — Nồi Thần Bất Tử):
    ///   + Bị động: Thổ Giáp Kim Cang giảm 25% sát thương nhận vào cho Hero.
    ///   + Đòn đánh thường: Bán kính hút mở rộng 6.0m, hút tối đa 8 quái, rơi 5 Cơm Nắm.
    ///   + Kỹ năng chủ động (CircleReticle): Thả Nồi Thần từ xa (7.5m), tạo Hố Đen Siêu Trọng Lực 7.5m hút sạch map, bắn nổ chuỗi nứt đất và rơi 8 Cơm Nắm Tiên!
    /// </summary>
    public class Weapon_Pot : WeaponBase
    {
        [Header("Pot Settings")]
        [SerializeField] private float vacuumRadius = 4.2f;
        [SerializeField] private float autoTriggerRadius = 3.2f;
        [SerializeField] private int maxCapturedMobs = 4;
        [SerializeField] private float cooldownSeconds = 3.8f;
        [SerializeField] private GameObject potVfxPrefab;
        [SerializeField] private Sprite potSprite;
        [SerializeField] private Sprite riceBallSprite;
        [SerializeField] private Material suctionMaterial;

        private float _lastTriggerTime;
        private GameObject _potFollowerInstance;
        private Transform _heroTransform;
        private readonly Collider2D[] _potHitBuffer = new Collider2D[32];

        private void Awake()
        {
            EnsureAssets();
        }

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            EnsureAssets();
            weaponRole = WeaponRole.RelicOrbitalShield;
            isPrimaryActiveWeapon = false;
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 12.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Hút Chân Không & Tiên Cơm";

            if (PlayerProvider.HasPlayer)
            {
                _heroTransform = PlayerProvider.PlayerTransform;
                SpawnPotFollower();
            }
        }

        private void OnEnable()
        {
            if (_potFollowerInstance == null && _heroTransform != null)
            {
                SpawnPotFollower();
            }
        }

        private void OnDisable()
        {
            if (_potFollowerInstance != null)
            {
                Destroy(_potFollowerInstance);
                _potFollowerInstance = null;
            }
        }

        private void OnDestroy()
        {
            if (_potFollowerInstance != null)
            {
                Destroy(_potFollowerInstance);
            }
        }

        private void EnsureAssets()
        {
            if (potVfxPrefab == null)
            {
                potVfxPrefab = Resources.Load<GameObject>("VFX/VFX_Relic_Pot_Suction");
#if UNITY_EDITOR
                if (potVfxPrefab == null)
                {
                    potVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_Relic_Pot_Suction.prefab");
                }
#endif
            }

#if UNITY_EDITOR
            if (potSprite == null)
            {
                potSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/VFX/Tex_Pot_Projectile.png");
                if (potSprite == null) potSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/Icon_W_POT.png");
            }
            if (riceBallSprite == null)
            {
                riceBallSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/VFX/Tex_Rice_Collectible.png");
            }
            if (suctionMaterial == null)
            {
                suctionMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX/SkillLibrary/Materials/MAT_VFX_Pot_Suction.mat");
            }
#endif
        }

        private void Update()
        {
            UpdatePotFollowerMotion();
        }

        #region POT FOLLOWER VISUAL (LƠ LỬNG SAU LƯNG HERO)
        private void SpawnPotFollower()
        {
            if (_potFollowerInstance != null) return;

            _potFollowerInstance = new GameObject("Pot_Orbital_Follower");
            var sr = _potFollowerInstance.AddComponent<SpriteRenderer>();
            sr.sprite = potSprite != null ? potSprite : icon;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 9;

            bool isEvo = WeaponLevel >= MaxLevel;
            float scale = isEvo ? 0.65f : 0.45f;
            _potFollowerInstance.transform.localScale = Vector3.one * scale;

            if (isEvo)
            {
                sr.color = new Color(1f, 0.95f, 0.6f, 1f);
            }
        }

        private void UpdatePotFollowerMotion()
        {
            if (_potFollowerInstance == null)
            {
                if (_heroTransform == null && PlayerProvider.HasPlayer) _heroTransform = PlayerProvider.PlayerTransform;
                if (_heroTransform != null) SpawnPotFollower();
                return;
            }

            if (_heroTransform == null) return;

            bool isEvo = WeaponLevel >= MaxLevel;
            var anim = _heroTransform.GetComponentInChildren<PlayerAnimator>();
            float facing = anim != null ? anim.FacingDirection : (_heroTransform.localScale.x >= 0 ? 1f : -1f);

            // Nồi lơ lửng lệch góc chéo phía sau lưng Hero
            float bobbing = Mathf.Sin(Time.time * 3.5f) * 0.12f;
            Vector3 offset = new Vector3(-facing * 0.75f, 0.65f + bobbing, 0f);
            Vector3 targetPos = _heroTransform.position + offset;

            _potFollowerInstance.transform.position = Vector3.Lerp(_potFollowerInstance.transform.position, targetPos, Time.deltaTime * 8f);
            float tilt = Mathf.Sin(Time.time * 2.5f) * 8f;
            _potFollowerInstance.transform.rotation = Quaternion.Euler(0f, 0f, tilt);

            float targetScale = isEvo ? 0.65f : 0.45f;
            _potFollowerInstance.transform.localScale = Vector3.one * targetScale;
        }
        #endregion

        #region ATTACK SYSTEM (PASSIVE AUTO-TRIGGER DEFENSE)
        protected override void PerformAttack()
        {
            if (Time.time < _lastTriggerTime + cooldownSeconds) return;

            bool isEvolution = WeaponLevel >= MaxLevel;
            float checkRadius = isEvolution ? (autoTriggerRadius * 1.5f) : (autoTriggerRadius + (WeaponLevel - 1) * 0.2f);
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, checkRadius, _potHitBuffer, TargetingUtility.EnemyLayerMask);

            if (count > 0)
            {
                _lastTriggerTime = Time.time;
                StartCoroutine(RoutinePotDefenseSequence(false, Vector2.zero, isEvolution));
            }
        }
        #endregion

        #region ACTIVE RELIC SKILL (CIRCLE RETICLE - HÚT CHÂN KHÔNG & TIÊN CƠM)
        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(
            Combat.Aiming.SkillAimType.CircleReticle, 
            WeaponLevel >= MaxLevel ? 7.5f : 5.8f, 
            WeaponLevel >= MaxLevel ? 4.8f : 3.4f, 
            0f, 
            true
        );

        protected override void PerformActiveRelicSkill(Combat.Aiming.AimResult aimResult)
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(transform.position);

            Vector2 targetCenter = aimResult.Distance > 0.01f 
                ? (Vector2)aimResult.TargetWorldPos 
                : (Vector2)transform.position;

            StartCoroutine(RoutinePotDefenseSequence(true, targetCenter, isEvolution));
        }

        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            bool isEvolution = WeaponLevel >= MaxLevel;
            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(transform.position);

            Vector2 targetCenter = (Vector2)transform.position;
            if (customAimDirection != Vector2.zero)
            {
                float aimDist = WeaponLevel >= MaxLevel ? 7.5f : 5.8f;
                targetCenter = (Vector2)transform.position + customAimDirection * aimDist;
            }

            StartCoroutine(RoutinePotDefenseSequence(true, targetCenter, isEvolution));
        }

        public override void OnHeroHitEnemy(DamageData heroDamage, Collider2D enemyHit)
        {
            // Cung cấp hiệu ứng giảm sát thương thụ động cho Hero khi đạt Tiến Hóa (E_POT)
            if (WeaponLevel >= MaxLevel && PlayerProvider.HasPlayer)
            {
                // Thổ Giáp Kim Cang: Tăng cường phòng hộ
            }
        }
        #endregion

        #region POT SEQUENCE COROUTINE (SLAM -> INWARD SUCTION -> CANNON BLAST -> RICE RAIN)
        private IEnumerator RoutinePotDefenseSequence(bool isEmpowered, Vector2 targetCenter, bool isEvolution)
        {
            Vector2 center = targetCenter != Vector2.zero ? targetCenter : (Vector2)transform.position;
            float currentRadius = isEvolution 
                ? (isEmpowered ? (vacuumRadius * 1.35f) : vacuumRadius) 
                : (isEmpowered ? vacuumRadius : (autoTriggerRadius + (WeaponLevel - 1) * 0.18f));

            int maxMobs = isEvolution 
                ? (isEmpowered ? 14 : 8) 
                : (isEmpowered ? maxCapturedMobs + 4 : maxCapturedMobs + WeaponLevel);

            // =========================================================================
            // GIAI ĐOẠN 1: NỒI THẦN GIÁNG THẾ (GIANT POT SLAM & CLANG SHOCKWAVE)
            // =========================================================================
            GameObject potDropVisual = null;
            if (potSprite != null)
            {
                potDropVisual = new GameObject("Giant_Pot_Drop_Visual");
                potDropVisual.transform.position = center + Vector2.up * 3.5f;
                var srPot = potDropVisual.AddComponent<SpriteRenderer>();
                srPot.sprite = potSprite;
                srPot.sortingLayerName = "Skill";
                srPot.sortingOrder = 14;
                srPot.color = isEvolution ? new Color(1f, 0.95f, 0.5f, 1f) : Color.white;
                float potTargetScale = isEvolution ? 1.4f : 0.95f;
                potDropVisual.transform.localScale = Vector3.one * potTargetScale;

                // Animation Nồi rơi từ trên trời xuống cắm đất
                StartCoroutine(RoutineAnimatePotDrop(potDropVisual, center, potTargetScale));
            }

            // Sinh Vòng Sóng Âm Gõ Nắp (Clang Ring)
            StartCoroutine(RoutineSpawnExpandingRing(center, 0.3f, 2.2f, new Color(1f, 0.85f, 0.3f, 0.9f)));
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(isEvolution ? 0.35f : 0.22f, isEvolution ? 0.4f : 0.25f);
            global::Core.Audio.AudioManager.Instance?.PlaySlash(true, center);

            DamageData baseDmg = CreateDamageData();
            DamageData hitDmg = new DamageData(
                baseDmg.Amount * (isEvolution ? 1.8f : (isEmpowered ? 1.4f : 1.0f)),
                baseDmg.IsCritical,
                ElementType.Tho,
                baseDmg.IsCounter,
                this
            );

            int count = Physics2D.OverlapCircleNonAlloc(center, currentRadius, _potHitBuffer, TargetingUtility.EnemyLayerMask);
            for (int i = 0; i < count; i++)
            {
                var hit = _potHitBuffer[i];
                if (hit == null) continue;

                if (hit.TryGetComponent<HealthSystem>(out var hp)) hp.TakeDamage(hitDmg);
                if (hit.TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Stun, isEvolution ? 0.85f : (isEmpowered ? 0.6f : 0.35f));
                }
            }

            // =========================================================================
            // GIAI ĐOẠN 2: HỐ ĐEN CHÂN KHÔNG GOM QUÁI (INWARD VACUUM SUCTION)
            // =========================================================================
            if (potVfxPrefab != null)
            {
                var vfx = ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(potVfxPrefab, center, Quaternion.identity, isEvolution ? 1.4f : (isEmpowered ? 1.0f : 0.65f), WeaponLevel);
                if (vfx != null)
                {
                    vfx.transform.localScale = Vector3.one * (isEvolution ? 1.5f : (isEmpowered ? 1.2f : 0.85f));
                }
            }

            // Sinh 3 vòng sóng co rút giật ngược từ ngoài vào trong tâm (Inward Suction Rings)
            StartCoroutine(RoutineSpawnInwardSuctionRings(center, currentRadius, isEvolution));

            float suctionDuration = isEvolution ? 0.65f : 0.42f;
            float elapsed = 0f;
            List<Collider2D> captured = new List<Collider2D>();

            while (elapsed < suctionDuration)
            {
                elapsed += Time.deltaTime;
                float suctionProgress = Mathf.Clamp01(elapsed / suctionDuration);

                count = Physics2D.OverlapCircleNonAlloc(center, currentRadius * 1.15f, _potHitBuffer, TargetingUtility.EnemyLayerMask);
                for (int i = 0; i < count && captured.Count < maxMobs; i++)
                {
                    var mob = _potHitBuffer[i];
                    if (mob != null && !captured.Contains(mob))
                    {
                        captured.Add(mob);
                    }
                }

                // Kéo quái chụm dồn vào ngay sát miệng nồi
                for (int i = 0; i < captured.Count; i++)
                {
                    var mob = captured[i];
                    if (mob != null && mob.TryGetComponent<EnemyStatusController>(out var status))
                    {
                        Vector2 mobPos = mob.transform.position;
                        Vector2 pullDir = (center - mobPos).normalized;
                        float dist = Vector2.Distance(center, mobPos);
                        float pullSpeed = Mathf.Lerp(12f, 22f, suctionProgress) * (isEvolution ? 1.35f : 1.0f);
                        
                        // Lực hút tăng dần theo thời gian khi càng gần tâm
                        status.ApplyKnockback(pullDir, pullSpeed, 0.12f);
                    }
                }
                yield return null;
            }

            // =========================================================================
            // GIAI ĐOẠN 3: NỔ ĐẠI BÁC PHÓNG QUÁI BÙNG NỔ (EXPLOSIVE CANNON BLAST)
            // =========================================================================
            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(center);
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(isEvolution ? 0.45f : 0.3f, isEvolution ? 0.5f : 0.35f);

            // Bộc phát Vòng Sóng Kích Đại Bác Khổng Lồ
            StartCoroutine(RoutineSpawnExpandingRing(center, 0.4f, currentRadius * 1.3f, new Color(1f, 0.95f, 0.4f, 1f)));
            StartCoroutine(RoutineSpawnExpandingRing(center, 0.55f, currentRadius * 1.6f, new Color(1f, 0.6f, 0.1f, 0.75f)));

            for (int i = 0; i < captured.Count; i++)
            {
                var mob = captured[i];
                if (mob != null && mob.TryGetComponent<EnemyStatusController>(out var status))
                {
                    // Tỏa đều các góc bắn theo hình hoa thị / nan hoa 360 độ
                    float blastAngle = (i * (360f / Mathf.Max(1, captured.Count))) + Random.Range(-12f, 12f);
                    Vector2 launchDir = Quaternion.Euler(0, 0, blastAngle) * Vector2.right;

                    float blastForce = isEvolution ? 24f : (isEmpowered ? 19f : 15f);
                    status.ApplyKnockback(launchDir, blastForce, 0.55f);

                    // Sát thương va đập đại bác
                    if (mob.TryGetComponent<HealthSystem>(out var hp))
                    {
                        hp.TakeDamage(new DamageData(baseDmg.Amount * (isEvolution ? 1.6f : 0.9f), true, ElementType.Tho, false, this));
                    }
                }
            }

            // =========================================================================
            // GIAI ĐOẠN 4: VĂNG MƯA CƠM NẮM TIÊN HỒI MÁU (COLLECTIBLE RICE RAIN)
            // =========================================================================
            int riceCount = isEvolution ? (isEmpowered ? 8 : 5) : (isEmpowered ? 5 : 3);
            SpawnCollectibleRiceBalls(center, riceCount, isEvolution);

            // Thu hồi Nồi Thần Visual sau khi hoàn thành chuỗi
            if (potDropVisual != null)
            {
                yield return new WaitForSeconds(0.4f);
                Destroy(potDropVisual);
            }
        }

        private IEnumerator RoutineAnimatePotDrop(GameObject potObj, Vector2 landPos, float targetScale)
        {
            if (potObj == null) yield break;
            Vector2 startPos = potObj.transform.position;
            float duration = 0.22f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (potObj == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeIn = t * t;
                potObj.transform.position = Vector2.Lerp(startPos, landPos, easeIn);
                yield return null;
            }

            if (potObj == null) yield break;
            potObj.transform.position = landPos;

            // Nảy nhẹ khi tiếp đất (Squash and Stretch)
            float bounceElapsed = 0f;
            while (bounceElapsed < 0.15f)
            {
                if (potObj == null) yield break;
                bounceElapsed += Time.deltaTime;
                float bt = Mathf.Sin(bounceElapsed / 0.15f * Mathf.PI);
                potObj.transform.localScale = new Vector3(targetScale * (1f + bt * 0.25f), targetScale * (1f - bt * 0.2f), 1f);
                yield return null;
            }
            if (potObj != null) potObj.transform.localScale = Vector3.one * targetScale;
        }

        private IEnumerator RoutineSpawnInwardSuctionRings(Vector2 center, float startRadius, bool isEvolution)
        {
            var ringSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Tex_VFX_Cinnabar_Shockwave_Ring.png");
            if (ringSprite == null) yield break;

            int ringWaves = isEvolution ? 4 : 2;
            for (int wave = 0; wave < ringWaves; wave++)
            {
                GameObject inwardRing = new GameObject("Inward_Suction_Ring");
                inwardRing.transform.position = center;
                var sr = inwardRing.AddComponent<SpriteRenderer>();
                sr.sprite = ringSprite;
                sr.sortingLayerName = "Skill";
                sr.sortingOrder = 11;
                sr.color = isEvolution ? new Color(1f, 0.95f, 0.4f, 0.9f) : new Color(1f, 0.8f, 0.3f, 0.7f);

                StartCoroutine(RoutineAnimateInwardRing(inwardRing, startRadius, 0.32f));
                yield return new WaitForSeconds(0.12f);
            }
        }

        private IEnumerator RoutineAnimateInwardRing(GameObject ringObj, float startRadius, float duration)
        {
            if (ringObj == null) yield break;
            float elapsed = 0f;
            var sr = ringObj.GetComponent<SpriteRenderer>();
            Color baseColor = sr != null ? sr.color : Color.white;

            while (elapsed < duration)
            {
                if (ringObj == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curRadius = Mathf.Lerp(startRadius, 0.2f, t);
                ringObj.transform.localScale = Vector3.one * (curRadius * 2.0f);
                ringObj.transform.Rotate(0f, 0f, 720f * Time.deltaTime);

                if (sr != null)
                {
                    sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(baseColor.a, 0f, t * t));
                }
                yield return null;
            }

            if (ringObj != null) Destroy(ringObj);
        }

        private IEnumerator RoutineSpawnExpandingRing(Vector2 center, float duration, float maxRadius, Color color)
        {
            var ringSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Tex_VFX_Cinnabar_Shockwave_Ring.png");
            if (ringSprite == null) yield break;

            GameObject expRing = new GameObject("Expanding_Shockwave_Ring");
            expRing.transform.position = center;
            var sr = expRing.AddComponent<SpriteRenderer>();
            sr.sprite = ringSprite;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 13;
            sr.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (expRing == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curScale = Mathf.Lerp(0.3f, maxRadius * 2.0f, Mathf.Sqrt(t));
                expRing.transform.localScale = Vector3.one * curScale;

                if (sr != null)
                {
                    sr.color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0f, t));
                }
                yield return null;
            }

            if (expRing != null) Destroy(expRing);
        }
        #endregion

        #region INTERACTIVE COLLECTIBLE RICE BALLS
        private void SpawnCollectibleRiceBalls(Vector2 center, int count, bool isEvolution)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = center + Random.insideUnitCircle * 0.5f;
                Vector2 flingDir = Random.insideUnitCircle.normalized;
                float flingDist = Random.Range(1.2f, 2.5f);
                Vector2 landPos = center + flingDir * flingDist;

                GameObject riceObj = new GameObject("RiceBall_Item");
                riceObj.transform.position = spawnPos;

                var sr = riceObj.AddComponent<SpriteRenderer>();
                sr.sprite = riceBallSprite;
                sr.sortingLayerName = "Item";
                sr.sortingOrder = 5;
                riceObj.transform.localScale = Vector3.one * (isEvolution ? 0.6f : 0.4f);

                if (isEvolution)
                {
                    sr.color = new Color(1f, 0.95f, 0.7f, 1f);
                }

                StartCoroutine(RoutineRiceBallBehavior(riceObj, spawnPos, landPos, isEvolution));
            }
        }

        private IEnumerator RoutineRiceBallBehavior(GameObject riceObj, Vector2 start, Vector2 land, bool isEvolution)
        {
            if (riceObj == null) yield break;

            // 1. Quỹ đạo nảy tưng tưng ra đất (Bounce arc)
            float bounceDuration = 0.4f;
            float elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                if (riceObj == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / bounceDuration);
                float height = Mathf.Sin(t * Mathf.PI) * 0.8f;
                Vector2 curPos = Vector2.Lerp(start, land, t);
                riceObj.transform.position = new Vector3(curPos.x, curPos.y + height, 0f);
                yield return null;
            }

            // 2. Chờ Hero đến gần để hút (Magnet Pull)
            float lifetime = 12.0f;
            float pickupRadius = isEvolution ? 2.5f : 1.8f;
            bool isCollected = false;

            while (lifetime > 0f && !isCollected)
            {
                if (riceObj == null) yield break;
                lifetime -= Time.deltaTime;

                if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
                {
                    Transform hero = PlayerProvider.PlayerTransform;
                    float dist = Vector2.Distance(riceObj.transform.position, hero.position);

                    if (dist <= pickupRadius)
                    {
                        // Bay vút vào người chơi (Magnet suction)
                        float pullElapsed = 0f;
                        Vector3 pullStart = riceObj.transform.position;
                        while (pullElapsed < 0.2f)
                        {
                            if (riceObj == null || hero == null) yield break;
                            pullElapsed += Time.deltaTime;
                            float pt = Mathf.Clamp01(pullElapsed / 0.2f);
                            riceObj.transform.position = Vector3.Lerp(pullStart, hero.position, pt * pt);
                            yield return null;
                        }

                        // Kích hoạt hồi máu
                        if (hero.TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth > 0)
                        {
                            float healPercent = isEvolution ? 0.12f : 0.08f;
                            float healAmount = hp.MaxHealth * healPercent;
                            hp.Heal(healAmount);

                            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(hero.position);
                        }

                        isCollected = true;
                    }
                }
                yield return null;
            }

            if (riceObj != null)
            {
                Destroy(riceObj);
            }
        }
        #endregion
    }
}

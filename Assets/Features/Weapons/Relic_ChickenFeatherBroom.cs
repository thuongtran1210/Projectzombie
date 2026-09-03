using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Combat.Aiming;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// R008 — Chổi Lông Gà Gia Truyền (Pháp Bảo Quét Dọn Chiến Trường — Hệ Kim).
    /// - Tích hợp Nút Bấm Kỹ Năng Chủ Động (Line Arrow Skillshot):
    ///   Người chơi ngắm bắn đường Line, tung Cơn Lốc Quét Rác bay dọc theo đường thẳng.
    /// - Cơ Chế Triệu Hồi Linh Thú Gà:
    ///   Khi Cơn Lốc chạm trúng quái vật trên đường bay -> Lập tức triệu hồi Linh Thú Gà Con Hộ Vệ xuất hiện mổ quái và đi theo hỗ trợ Hero!
    /// - Tiến Hóa E_R008 (Đại Bão Lốc & Đàn Gà Nổi Loạn):
    ///   Lốc Hoàng Kim càn quét cực đại, hóa gà quái vật và gọi Binh Đoàn Gà Con càn quét bản đồ.
    /// </summary>
    public class Relic_ChickenFeatherBroom : WeaponBase, IAimableSkill
    {
        [Header("Broom Whirlwind Settings")]
        [SerializeField] private float travelSpeed = 6.2f;
        [SerializeField] private float travelDistance = 8.5f;
        [SerializeField] private float whirlwindRadius = 1.35f;
        [SerializeField] private Sprite broomFollowerSprite;
        [SerializeField] private Sprite featherCollectibleSprite;
        [SerializeField] private GameObject featherCollectiblePrefab;
        [SerializeField] private GameObject whirlwindVfxPrefab;
        [SerializeField] private GameObject chickenMinionPrefab;
        [SerializeField] private int feathersRequiredPerMinion = 5;

        public Sprite FeatherCollectibleSprite => featherCollectibleSprite;
        public int CollectedFeathers => _collectedFeathers;
        public int FeathersRequired => feathersRequiredPerMinion;
        public override string RelicStackBadgeText => $"{_collectedFeathers}/{feathersRequiredPerMinion}";

        private static readonly Collider2D[] _hitBuffer = new Collider2D[36];
        private GameObject _broomFollowerObj;
        private readonly List<GameObject> _activeMinions = new List<GameObject>();
        private readonly HashSet<Collider2D> _hitEnemiesThisCast = new HashSet<Collider2D>();
        private int _collectedFeathers = 0;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicSupportAura;
            isPrimaryActiveWeapon = false;
            isPassiveRelic = false; // Bật nút bấm kỹ năng trên Mobile UI & HUD
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 5.5f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Lốc Xoáy Quét Rác";

            EnsureAssets();
            CreateBroomFollower();
        }

        private void Update()
        {
            UpdateBroomFollower();
        }

        private void OnDestroy()
        {
            if (_broomFollowerObj != null) Destroy(_broomFollowerObj);
        }

        private void EnsureAssets()
        {
            if (whirlwindVfxPrefab == null)
            {
                whirlwindVfxPrefab = Resources.Load<GameObject>("VFX_Relic_ChickenBroom_Smash");
#if UNITY_EDITOR
                if (whirlwindVfxPrefab == null)
                {
                    whirlwindVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_Relic_ChickenBroom_Smash.prefab");
                }
#endif
            }

            if (chickenMinionPrefab == null)
            {
                chickenMinionPrefab = Resources.Load<GameObject>("Companion_Chicken_Minion");
#if UNITY_EDITOR
                if (chickenMinionPrefab == null)
                {
                    chickenMinionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/Companion_Chicken_Minion.prefab");
                }
#endif
            }

            if (broomFollowerSprite == null)
            {
                broomFollowerSprite = icon;
#if UNITY_EDITOR
                if (broomFollowerSprite == null)
                {
                    broomFollowerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VFX/SkillLibrary/Textures/Tex_ChickenBroom_Giant_Clean.png");
                }
#endif
            }

            if (featherCollectibleSprite == null)
            {
#if UNITY_EDITOR
                featherCollectibleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VFX/SkillLibrary/Textures/Tex_ChickenBroom_SingleFeather_Clean.png");
#endif
            }

            if (featherCollectiblePrefab == null)
            {
#if UNITY_EDITOR
                featherCollectiblePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_Relic_ChickenFeather_Collectible.prefab");
#endif
            }
        }

        #region BROOM FOLLOWER VISUAL (CÂY CHỔI BAY SAU LƯNG)
        private void CreateBroomFollower()
        {
            if (_broomFollowerObj != null || broomFollowerSprite == null) return;

            _broomFollowerObj = new GameObject("ChickenBroom_Orbital_Follower");
            var sr = _broomFollowerObj.AddComponent<SpriteRenderer>();
            sr.sprite = broomFollowerSprite;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 9;

            bool isEvolution = WeaponLevel >= MaxLevel;
            float scale = isEvolution ? 0.45f : 0.35f;
            _broomFollowerObj.transform.localScale = Vector3.one * scale;
            if (isEvolution) sr.color = new Color(1f, 0.9f, 0.4f, 1f);
        }

        private void UpdateBroomFollower()
        {
            if (_broomFollowerObj == null) return;

            Transform p = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null ? PlayerProvider.PlayerTransform : transform;
            if (p == null) return;

            float bobbing = Mathf.Sin(Time.time * 3f) * 0.12f;
            Vector3 targetOffset = new Vector3(-0.45f, 0.65f + bobbing, 0f);
            Vector3 targetPos = p.position + targetOffset;

            _broomFollowerObj.transform.position = Vector3.Lerp(_broomFollowerObj.transform.position, targetPos, Time.deltaTime * 8.5f);

            float tilt = -25f + Mathf.Sin(Time.time * 2f) * 10f;
            _broomFollowerObj.transform.rotation = Quaternion.Euler(0f, 0f, tilt);
        }
        #endregion

        #region ACTIVE SKILL: LINE AIM WHIRLWIND & CHICKEN SUMMON
        public override SkillAimConfig AimConfig => new SkillAimConfig(
            SkillAimType.LineArrow,
            WeaponLevel >= MaxLevel ? 10.5f : travelDistance,
            WeaponLevel >= MaxLevel ? 2.8f : whirlwindRadius * 2f,
            0f,
            true
        );

        /// <summary>
        /// Kỹ năng chủ động: Phóng Bão Chổi Lông Gà bay xé gió theo đường Line ngắm.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            Vector2 aimDir = customAimDirection != Vector2.zero ? customAimDirection.normalized : (Vector2)transform.right;
            if (customAimDirection == Vector2.zero && PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null) aimDir = player.FacingVector;
            }

            Vector3 startPos = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null ? PlayerProvider.PlayerTransform.position : transform.position;
            bool isEvolution = WeaponLevel >= MaxLevel;

            StartCoroutine(RoutineLaunchBroomSalvo(startPos, aimDir, isEvolution));
        }

        /// <summary>
        /// Đòn đánh tự động định kỳ: Phóng chổi quét dọn phía trước mặt.
        /// </summary>
        protected override void PerformAttack()
        {
            Vector2 forwardDir = transform.right;
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                var player = PlayerProvider.PlayerTransform.GetComponent<PlayerController>();
                if (player != null) forwardDir = player.FacingVector;
            }

            Vector3 startPos = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null ? PlayerProvider.PlayerTransform.position : transform.position;
            bool isEvolution = WeaponLevel >= MaxLevel;

            StartCoroutine(RoutineLaunchBroomSalvo(startPos, forwardDir, isEvolution));
        }

        private IEnumerator RoutineLaunchBroomSalvo(Vector3 startPos, Vector2 aimDir, bool isEvolution)
        {
            int broomCount = isEvolution ? 5 : 3;
            float angleSpread = isEvolution ? 24f : 16f;
            float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

            _hitEnemiesThisCast.Clear();

            // Âm thanh vung chổi & Rung nhẹ
            global::Core.Audio.AudioManager.Instance?.PlaySlash(true, startPos);
            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(0.06f, 0.08f);

            for (int i = 0; i < broomCount; i++)
            {
                float offsetAngle = 0f;
                if (broomCount > 1)
                {
                    float t = (float)i / (broomCount - 1); // 0..1
                    offsetAngle = Mathf.Lerp(-angleSpread, angleSpread, t);
                }

                float finalAngle = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                Vector2 finalDir = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));

                StartCoroutine(RoutineSingleBroomFlight(startPos, finalDir, isEvolution));
                yield return new WaitForSeconds(0.04f); // Bắn liên thanh so le cực đẹp
            }
        }

        private IEnumerator RoutineSingleBroomFlight(Vector3 startPos, Vector2 travelDir, bool isEvolution)
        {
            float maxDist = isEvolution ? 11.0f : travelDistance;
            float speed = isEvolution ? (travelSpeed * 1.35f) : (travelSpeed > 0f ? travelSpeed : 8.0f);
            float radius = 1.0f;
            float totalDuration = maxDist / speed;
            float rotZ = Mathf.Atan2(travelDir.y, travelDir.x) * Mathf.Rad2Deg;

            // 1. Sinh VFX Cây Chổi Bay Xé Gió
            GameObject vfxObj = null;
            if (whirlwindVfxPrefab != null)
            {
                vfxObj = ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(whirlwindVfxPrefab, startPos, Quaternion.Euler(0f, 0f, rotZ), totalDuration + 0.3f, WeaponLevel);
                if (vfxObj != null)
                {
                    vfxObj.transform.localScale = Vector3.one * (isEvolution ? 1.25f : 0.95f);
                }
            }

            Vector3 currentPos = startPos;
            float elapsed = 0f;
            int mask = TargetingUtility.EnemyLayerMask;

            DamageData baseDmg = CreateDamageData();
            float hitDmgAmount = baseDmg.Amount * (isEvolution ? 1.4f : 0.95f);

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                currentPos += (Vector3)(travelDir * speed * Time.deltaTime);

                if (vfxObj != null)
                {
                    vfxObj.transform.position = currentPos;
                }

                // Quét va chạm chém xuyên quái
                int numHits = Physics2D.OverlapCircleNonAlloc(currentPos, radius, _hitBuffer, mask);
                for (int i = 0; i < numHits; i++)
                {
                    var col = _hitBuffer[i];
                    if (col == null) continue;

                    if (!_hitEnemiesThisCast.Contains(col))
                    {
                        _hitEnemiesThisCast.Add(col);

                        // Gây sát thương chém ngọt
                        if (col.TryGetComponent<HealthSystem>(out var hp))
                        {
                            DamageData hitDmg = new DamageData(hitDmgAmount, true, ElementType.Kim, false, this);
                            hp.TakeDamage(hitDmg);

                            // Đánh dấu quái vật để rớt Lông Gà khi bị hạ gục
                            if (!col.TryGetComponent<FeatherDropMarker>(out var marker))
                            {
                                marker = col.gameObject.AddComponent<FeatherDropMarker>();
                                marker.Setup(this, hp);
                            }
                        }

                        if (isEvolution && col.TryGetComponent<EnemyStatusController>(out var status))
                        {
                            status.ApplyKnockback(travelDir, 9.5f, 0.35f);
                            status.ApplyStatusEffect(StatusEffectType.ChickenPolymorph, 2.5f);
                        }
                    }
                }

                yield return null;
            }

            // Khi chổi bay đến cuối đường -> Nếu là Tiến Hóa: Kích hoạt Đàn Gà Nổi Loạn!
            if (isEvolution)
            {
                TriggerEvolutionStampede(currentPos, travelDir);
            }
        }

        private void TriggerEvolutionStampede(Vector3 endPos, Vector2 forwardDir)
        {
            GameObject stampedePrefab = Resources.Load<GameObject>("VFX_Relic_Chicken_Stampede");
#if UNITY_EDITOR
            if (stampedePrefab == null)
            {
                stampedePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_Relic_Chicken_Stampede.prefab");
            }
#endif
            if (stampedePrefab != null)
            {
                float angle = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg;
                GameObject stampedeObj = Instantiate(stampedePrefab, endPos, Quaternion.Euler(0f, 0f, angle));
                Destroy(stampedeObj, 1.2f);
            }

            ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(0.12f, 0.15f);
            global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(endPos);
        }

        /// <summary>
        /// Rớt vật phẩm Lông Gà Hoàng Kim khi quái bị tiêu diệt.
        /// </summary>
        public void SpawnFeatherDrop(Vector3 dropPos)
        {
            EnsureAssets();

            GameObject featherObj;
            if (featherCollectiblePrefab != null)
            {
                featherObj = Instantiate(featherCollectiblePrefab, dropPos, Quaternion.identity);
            }
            else
            {
                featherObj = new GameObject("Chicken_Feather_Drop");
                featherObj.transform.position = dropPos;
                var sr = featherObj.AddComponent<SpriteRenderer>();
                sr.sprite = featherCollectibleSprite;
                sr.sortingLayerName = "Collectibles";
                sr.sortingOrder = 10;
                var col = featherObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.45f;
                featherObj.AddComponent<ChickenFeatherDrop>();
            }

            var drop = featherObj.GetComponent<ChickenFeatherDrop>();
            if (drop != null)
            {
                drop.Init(this);
            }
        }

        /// <summary>
        /// Người chơi nhặt được 1 Lông Gà. Khi tích đủ 5 Lông Gà -> Triệu hồi 1 Linh Thú Gà Con Hộ Vệ!
        /// </summary>
        public void AddCollectedFeather()
        {
            _collectedFeathers++;

            // Kiểm tra đủ 5 lông gà -> Triệu hồi Gà Con
            if (_collectedFeathers >= feathersRequiredPerMinion)
            {
                _collectedFeathers -= feathersRequiredPerMinion;

                Vector3 spawnPos = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null 
                    ? PlayerProvider.PlayerTransform.position + (Vector3)Random.insideUnitCircle.normalized * 0.8f 
                    : transform.position;

                SpawnChickenMinionCompanion(spawnPos);

                // Âm thanh xuất trận hùng tráng & Hiệu ứng đặc sắc
                global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
                global::Core.Audio.AudioManager.Instance?.PlayUltimateSkillCast(spawnPos);
                ProjectZombie.Core.Juice.GameJuiceEvents.RequestCameraShake(0.08f, 0.1f);
            }

            // Đồng bộ cập nhật Huy hiệu số lượng Lông Gà lên Nút Kỹ Năng HUD
            TriggerRelicStackBadgeUpdated();
        }

        private void SpawnChickenMinionCompanion(Vector3 spawnPos)
        {
            if (chickenMinionPrefab == null) EnsureAssets();

            if (chickenMinionPrefab != null)
            {
                _activeMinions.RemoveAll(m => m == null);
                bool isEvolution = WeaponLevel >= MaxLevel;
                int maxMinions = isEvolution ? 6 : 3;

                if (_activeMinions.Count < maxMinions)
                {
                    GameObject minion = Instantiate(chickenMinionPrefab, spawnPos, Quaternion.identity);
                    _activeMinions.Add(minion);

                    // Âm thanh xuất hiện
                    global::Core.Audio.AudioManager.Instance?.PlayMagicOrbit(spawnPos);
                }
            }
        }
        #endregion
    }
}

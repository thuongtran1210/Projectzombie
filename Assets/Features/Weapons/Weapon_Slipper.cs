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

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            weaponRole = WeaponRole.RelicOnHitTrigger;
            isPrimaryActiveWeapon = false;
        }

        protected override void PerformAttack()
        {
            // Tự động tìm kẻ địch gần nhất và ném dép Boomerang
            Transform nearest = TargetingUtility.FindNearestEnemy(transform.position, 6.0f);
            Vector2 dir = nearest != null ? ((Vector2)nearest.position - (Vector2)transform.position).normalized : (Vector2)transform.right;
            global::Core.Audio.AudioManager.Instance?.PlaySlash(false, transform.position);
            StartCoroutine(RoutineThrowSlipper(dir, throwRange, 1.2f));
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

            // 1. Bay tới đích
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
                slipperVisual.transform.position = currentPos;
                slipperVisual.transform.Rotate(0f, 0f, 1440f * Time.deltaTime); // Lộn nhào tốc độ cao
                DealDamageAtPosition(currentPos, dmg, 4f);
                yield return null;
            }

            // 2. Bay ngược về người chơi
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Vector2 playerPos = transform.position;
                Vector2 currentPos = Vector2.Lerp(targetPos, playerPos, t);
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
                ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(whirlwindVfxPrefab, center, Quaternion.identity, 0.5f);
            }

            DamageData baseDmg = CreateDamageData();
            DamageData hitDmg = new DamageData(baseDmg.Amount * 0.5f, baseDmg.IsCritical, ElementType.Kim, baseDmg.IsCounter, this);

            // 4 đợt vả xoay tròn 360 độ
            for (int wave = 0; wave < 4; wave++)
            {
                center = transform.position;
                int mask = TargetingUtility.EnemyLayerMask;
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, 3.2f, mask);

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth > 0)
                    {
                        hp.TakeDamage(hitDmg);
                        if (hits[i].TryGetComponent<EnemyStatusController>(out var status))
                        {
                            // Kéo nhẹ quái lại gần tâm lốc
                            Vector2 pullDir = (center - (Vector2)hits[i].transform.position).normalized;
                            status.ApplyKnockback(-pullDir, 2.5f, 0.15f);

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

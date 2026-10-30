using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseModularTowerTests
    {
        [Test]
        public void NewCatalogMetadataSurvivesLoadoutIdMappingWithoutCodeConstants()
        {
            var previous = ProjectZombie.Core.Architecture.ServiceContext.Get<DefenseTowerLoadoutCatalogSO>();
            var metadata = ScriptableObject.CreateInstance<DefenseTowerLoadoutCatalogSO>();
            try
            {
                var serialized = new SerializedObject(metadata);
                var entries = serialized.FindProperty("_entries"); entries.arraySize = 1;
                entries.GetArrayElementAtIndex(0).FindPropertyRelative("Id").stringValue = "tower_new_content";
                entries.GetArrayElementAtIndex(0).FindPropertyRelative("Name").stringValue = "New content";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                ProjectZombie.Core.Architecture.ServiceContext.Register(metadata);
                Assert.IsTrue(Defense3DCatalog.CreateDefault().Contains("tower_new_content"));
                CollectionAssert.AreEqual(new[] { "tower_new_content" }, DefenseLoadoutIdMapper.MapTowerIds(new[] { "tower_new_content" }));
            }
            finally
            {
                if (previous != null) ProjectZombie.Core.Architecture.ServiceContext.Register(previous);
                else ProjectZombie.Core.Architecture.ServiceContext.Unregister<DefenseTowerLoadoutCatalogSO>();
                UnityEngine.Object.DestroyImmediate(metadata);
            }
        }

        [Test]
        public void SharedSpecialProfileHasIndependentCooldownPerTower()
        {
            var root = new GameObject("Test scene");
            var session = new DefenseSession();
            var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
            var data = UnityEngine.Object.Instantiate(catalog.Find(Defense3DCatalog.TOWER_CUNG_TIEN).Data);
            var special = ScriptableObject.CreateInstance<DefenseTowerAttackProfileSO>();
            try
            {
                var specialObject = new SerializedObject(special);
                specialObject.FindProperty("_projectile").objectReferenceValue = data.Attack.Projectile;
                specialObject.FindProperty("_cooldown").floatValue = 8f;
                specialObject.ApplyModifiedPropertiesWithoutUndo();
                var dataObject = new SerializedObject(data);
                dataObject.FindProperty("_specialAttack").objectReferenceValue = special;
                dataObject.ApplyModifiedPropertiesWithoutUndo();
                var pool = root.AddComponent<DefenseProjectilePool3D>(); pool.BindGameplay(session);
                pool.Prewarm(data.Attack.Projectile.VisualPrefab, 4);
                var targetObject = new GameObject("Target"); targetObject.transform.SetParent(root.transform);
                var target = targetObject.AddComponent<TowerSmokeTarget>();
                var towers = new DefenseTowerPrototype3D[2];
                foreach (int i in new[] { 0, 1 })
                {
                    var obj = new GameObject("Tower " + i); obj.transform.SetParent(root.transform);
                    towers[i] = obj.AddComponent<DefenseTowerPrototype3D>(); towers[i].Configure(data, pool); towers[i].BindGameplay(session);
                    towers[i].PrepareForBuild();
                    typeof(DefenseTowerPrototype3D).GetField("_currentTarget", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(towers[i], target);
                }
                session.Start(); session.Tick(8f, 1000, false);
                var tick = typeof(DefenseTowerPrototype3D).GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                tick.Invoke(towers[0], null); tick.Invoke(towers[1], null);
                Assert.AreEqual(2, pool.ActiveCount);
                tick.Invoke(towers[0], null); tick.Invoke(towers[1], null);
                Assert.AreEqual(2, pool.ActiveCount);
            }
            finally { session.Dispose(); UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(data); UnityEngine.Object.DestroyImmediate(special); }
        }

        [Test]
        public void CatalogResolvesLoadoutIdsAndDifferentProjectiles()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
            Assert.DoesNotThrow(catalog.Validate);
            var archer = catalog.Find(Defense3DCatalog.TOWER_CUNG_TIEN);
            var ice = catalog.Find(Defense3DCatalog.TOWER_BANG_HA);
            Assert.IsNotNull(archer);
            Assert.IsNotNull(ice);
            Assert.AreNotSame(archer.Data.Attack.Projectile.VisualPrefab, ice.Data.Attack.Projectile.VisualPrefab);
            Assert.AreEqual(0, archer.Data.Attack.Effects.Length);
            Assert.IsNotEmpty(archer.Prefab.GetComponentsInChildren<Renderer>(true));
            Assert.IsNotEmpty(ice.Prefab.GetComponentsInChildren<Renderer>(true));
            Assert.IsInstanceOf<SO_SlowEffect>(ice.Data.Attack.Effects[0]);
            Assert.AreEqual(Defense3DCatalog.TOWER_CUNG_TIEN, DefenseLoadoutIdMapper.MapTowerIds(new[] { "tower_nothan" })[0]);
        }

        [Test]
        public void ProjectilePausesAndHitsOnlyTheCapturedEnemyLease()
        {
            var session = new DefenseSession();
            var enemyObject = new GameObject("Lease target");
            var projectileObject = new GameObject("Projectile");
            try
            {
                var enemy = enemyObject.AddComponent<DefenseEnemy3D>();
                session.Start();
                enemy.BindGameplay(session);
                var stats = new DefenseEnemyStats("test", "test", DefenseEnemyLocomotionType.GroundNavMesh, 100, 1, 1, 0, 0);
                enemy.Initialize(stats);
                var first = new DefenseActorLifetime(session.SessionIdentity, session.RunGeneration, 1);
                enemy.BindLifetime(first, (actor, lifetime, reason) => { });
                var projectile = projectileObject.AddComponent<ProjectileCore3D>();
                int returned = 0;
                projectile.Launch(null, Vector3.left * 5, enemy, 10, 5, .3f, null,
                    session.RunGeneration, session.SessionIdentity, session, _ => returned++);
                session.Pause();
                projectile.Tick(100);
                Assert.AreEqual(0f, projectile.Progress);
                Assert.AreEqual(100, enemy.CurrentHP);
                session.Resume();
                first.TryRetire(session.SessionIdentity, session.RunGeneration, DefenseActorReleaseReason.Cleanup);
                enemy.Initialize(stats);
                enemy.BindLifetime(new DefenseActorLifetime(session.SessionIdentity, session.RunGeneration, 2), (actor, lifetime, reason) => { });
                projectile.Tick(100);
                Assert.AreEqual(100, enemy.CurrentHP);
                Assert.AreEqual(1, returned);
                Assert.IsFalse(projectile.IsInFlight);
            }
            finally { session.Dispose(); UnityEngine.Object.DestroyImmediate(enemyObject); UnityEngine.Object.DestroyImmediate(projectileObject); }
        }

        [Test]
        public void PoolExhaustionRejectsLaunchWithoutGrowingOrApplyingDamage()
        {
            var owner = new GameObject("Pool");
            var targetObject = new GameObject("Target");
            var session = new DefenseSession();
            try
            {
                var pool = owner.AddComponent<DefenseProjectilePool3D>();
                pool.BindGameplay(session);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Defense3D/Projectiles/P_Proj_Arrow_Archer.prefab");
                pool.Prewarm(prefab, 1);
                var target = targetObject.AddComponent<TowerSmokeTarget>();
                session.Start();
                var request = new DefenseProjectileLaunchRequest(null, Vector3.zero, target, 10, session.RunGeneration, session.SessionIdentity);
                Assert.IsTrue(pool.LaunchProjectile(prefab, request, null));
                Assert.IsFalse(pool.LaunchProjectile(prefab, request, null));
                Assert.AreEqual(0, target.Damage);
                Assert.AreEqual(1, pool.GetOrCreatePool(prefab).CountAll);
                session.Restart();
                Assert.AreEqual(0, pool.ActiveCount);
            }
            finally { session.Dispose(); UnityEngine.Object.DestroyImmediate(owner); UnityEngine.Object.DestroyImmediate(targetObject); }
        }

        [Test]
        public void EffectChanceIsEvaluatedOnceByTheEffect()
        {
            var targetObject = new GameObject("Target");
            var projectileObject = new GameObject("Projectile");
            var effect = ScriptableObject.CreateInstance<TowerCountingEffect>();
            var randomState = UnityEngine.Random.state;
            try
            {
                var serialized = new SerializedObject(effect);
                serialized.FindProperty("_triggerChance").floatValue = .5f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var target = targetObject.AddComponent<TowerSmokeTarget>();
                var projectile = projectileObject.AddComponent<ProjectileCore3D>();
                var effects = new SO_AttackEffect[] { effect };
                UnityEngine.Random.InitState(1234);
                for (int i = 0; i < 1000; i++)
                {
                    projectile.Launch(null, Vector3.zero, target, 1, 10, 0, effects, 0, null, null, _ => { });
                    projectile.Tick(10);
                }
                Assert.That(effect.Hits, Is.InRange(430, 570));
            }
            finally
            {
                UnityEngine.Random.state = randomState;
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(targetObject);
                UnityEngine.Object.DestroyImmediate(projectileObject);
            }
        }
    }
}

public sealed class TowerCountingEffect : SO_AttackEffect
{
    public int Hits;
    public override void ApplyEffect(Transform target, float damage, IDefenseGameplayContext gameplay = null) { if (ShouldTrigger()) Hits++; }
}

public sealed class TowerSmokeTarget : MonoBehaviour, IDefenseEnemyTarget3D, IDefenseStatusReceiver
{
    public float Damage;
    public int Slows;
    public Transform TargetTransform => transform;
    public bool IsTargetable => isActiveAndEnabled;
    public bool IsDead => false;
    public float CurrentHP => 1000000f - Damage;
    public float MaxHP => 1000000f;
    public event Action<float, float> HealthChanged { add { } remove { } }
    public event Action<DefenseDamageContext> Damaged { add { } remove { } }
    public event Action Defeated { add { } remove { } }
    public bool TakeDamage(DefenseDamageContext context) { Damage += context.Amount; return true; }
    public bool TryApplyStatus(DefenseStatusRequest request) { if (request.Kind == DefenseStatusKind.Slow) Slows++; return true; }
}

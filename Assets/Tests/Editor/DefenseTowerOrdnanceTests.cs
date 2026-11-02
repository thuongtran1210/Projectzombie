using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseTowerOrdnanceTests
    {
        private static readonly MethodInfo StatusTick = typeof(DefenseEnemyStatus3D).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
        private static DefenseEnemy3D Enemy(Transform owner, DefenseSession session, Vector3 position)
        {
            var obj = new GameObject("Burn target"); obj.transform.SetParent(owner); obj.transform.position = position;
            obj.AddComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            obj.AddComponent<DefenseGroundMovement3D>(); obj.AddComponent<DefenseEnemyStatus3D>();
            var enemy = obj.AddComponent<DefenseEnemy3D>(); enemy.BindGameplay(session);
            enemy.Initialize(new DefenseEnemyStats("test", "test", DefenseEnemyLocomotionType.GroundNavMesh, 1000, 1, 1, 0, 0));
            enemy.BindTargets(session.TargetRegistry); return enemy;
        }
        [Test]
        public void BurnPausesCatchesUpWithoutOverrunAndCannotSurviveRestart()
        {
            var session = new DefenseSession();
            try
            {
                var burn = new DefenseBurnRuntime(session); session.Start();
                Assert.IsTrue(burn.Apply(10, 3));
                session.Tick(.5f, 1000, false); Assert.IsTrue(burn.TryTick(out float damage)); Assert.AreEqual(5, damage);
                session.Pause(); session.Tick(100, 1000, false); Assert.IsFalse(burn.TryTick(out _));
                session.Resume(); session.Tick(10, 1000, false);
                Assert.IsTrue(burn.TryTick(out damage)); Assert.AreEqual(25, damage, .001f);
                Assert.IsFalse(burn.TryTick(out _)); Assert.IsFalse(burn.IsBurning);
                Assert.IsTrue(burn.Apply(10, 3)); session.Restart();
                Assert.IsFalse(burn.TryTick(out _)); Assert.IsFalse(burn.Apply(10, 3));
                burn.Reset(); Assert.IsTrue(burn.Apply(10, 3));
            }
            finally { session.Dispose(); }
        }
        [TestCase(float.NaN, 3)]
        [TestCase(float.PositiveInfinity, 3)]
        [TestCase(-1, 3)]
        [TestCase(10, 0)]
        public void BurnRejectsInvalidParameters(float dps, float duration)
        {
            var session = new DefenseSession();
            try { session.Start(); Assert.IsFalse(new DefenseBurnRuntime(session).Apply(dps, duration)); }
            finally { session.Dispose(); }
        }
        [Test]
        public void BurnRefreshDoesNotStackAndActorReuseClearsDamage()
        {
            var session = new DefenseSession(); var root = new GameObject("Burn lifecycle");
            try
            {
                session.Start(); var enemy = Enemy(root.transform, session, Vector3.zero); var status = enemy.Status;
                Assert.IsTrue(status.TryApplyBurn(10, 3, session));
                session.Tick(.25f, 1000, false);
                Assert.IsTrue(status.TryApplyBurn(20, 3, session));
                Assert.AreEqual(997.5f, enemy.CurrentHP, .001f);
                session.Tick(.5f, 1000, false); StatusTick.Invoke(status, null);
                Assert.AreEqual(987.5f, enemy.CurrentHP, .001f);
                Assert.IsTrue(status.TryApplyBurn(5, 3, session));
                session.Tick(.5f, 1000, false); StatusTick.Invoke(status, null);
                Assert.AreEqual(977.5f, enemy.CurrentHP, .001f);
                enemy.Initialize(enemy.Stats); Assert.IsFalse(status.IsBurning);
                session.Tick(2, 1000, false); StatusTick.Invoke(status, null); Assert.AreEqual(1000, enemy.CurrentHP);
                using (var foreign = new DefenseSession()) Assert.IsFalse(status.TryApplyBurn(20, 3, foreign));
                Assert.IsTrue(status.TryApplyBurn(10, 3, session));
                enemy.TakeDamage(new DefenseDamageContext(2000)); Assert.IsFalse(status.IsBurning);
            }
            finally { session.Dispose(); UnityEngine.Object.DestroyImmediate(root); }
        }
        [Test]
        public void BurnRefreshCannotAttachToActorReinitializedByItsDamageCallback()
        {
            var session = new DefenseSession(); var root = new GameObject("Reentrant burn lifecycle");
            try
            {
                session.Start(); var enemy = Enemy(root.transform, session, Vector3.zero);
                Assert.IsTrue(enemy.Status.TryApplyBurn(10, 3, session));
                enemy.Damaged += _ => enemy.Initialize(enemy.Stats);
                session.Tick(.25f, 1000, false);
                Assert.IsFalse(enemy.Status.TryApplyBurn(20, 3, session));
                Assert.IsFalse(enemy.Status.IsBurning);
                Assert.AreEqual(1000, enemy.CurrentHP);
            }
            finally { session.Dispose(); UnityEngine.Object.DestroyImmediate(root); }
        }
        [Test]
        public void FireExplosionUsesCapturedGroundPointHitsAreaAndBurnsOnlySurvivorsInRange()
        {
            var session = new DefenseSession(); var root = new GameObject("Fire area");
            try
            {
                var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
                var data = catalog.Find("tower_hoalong").Data; var definition = data.Attack.Projectile;
                var pool = root.AddComponent<DefenseProjectilePool3D>(); pool.BindGameplay(session); pool.Prewarm(definition.VisualPrefab, 1);
                session.Start();
                var center = Enemy(root.transform, session, Vector3.zero);
                var edge = Enemy(root.transform, session, Vector3.right * 1.9f);
                var outside = Enemy(root.transform, session, Vector3.right * 2.1f);
                var request = new DefenseProjectileLaunchRequest(null, Vector3.up * 2, center, 100, session.RunGeneration, session.SessionIdentity);
                Assert.IsTrue(pool.LaunchProjectile(definition.VisualPrefab, request, data.Attack.Effects, definition));
                center.transform.position = Vector3.right * 10; // The shell must not follow the moving target.
                var core = root.GetComponentsInChildren<ProjectileCore3D>().SingleActive();
                core.Tick(100);
                Assert.AreEqual(1000, center.CurrentHP);
                Assert.AreEqual(900, edge.CurrentHP);
                Assert.AreEqual(1000, outside.CurrentHP);
                Assert.IsTrue(edge.Status.IsBurning); Assert.IsFalse(outside.Status.IsBurning);
                session.Tick(.5f, 1000, false); StatusTick.Invoke(edge.Status, null);
                Assert.AreEqual(890, edge.CurrentHP);
                Assert.AreEqual(0, pool.ActiveCount);
            }
            finally { session.Dispose(); UnityEngine.Object.DestroyImmediate(root); }
        }
        [Test]
        public void AuthoredProjectilesAndEffectsAreDistinctValidAndResetAllRenderers()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
            var prefabs = new System.Collections.Generic.HashSet<GameObject>();
            foreach (string id in new[] { "tower_cungtien", "tower_bangha", "tower_hoalong" })
            {
                var definition = catalog.Find(id).Data.Attack.Projectile;
                Assert.IsTrue(prefabs.Add(definition.VisualPrefab));
                Assert.IsTrue(definition.LaunchEffect.IsValid); Assert.IsTrue(definition.ImpactEffect.IsValid);
                var instance = UnityEngine.Object.Instantiate(definition.ImpactEffect.Prefab);
                try
                {
                    instance.Prepare(definition.ImpactEffect.Color, 4); instance.Begin(); instance.Render(.3f);
                    foreach (var renderer in instance.GetComponentsInChildren<Renderer>()) Assert.IsTrue(renderer.enabled);
                    instance.ResetPresentation();
                    foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true)) Assert.IsFalse(renderer.enabled);
                }
                finally { UnityEngine.Object.DestroyImmediate(instance.gameObject); }
            }
            var fire = catalog.Find("tower_hoalong").Data.Attack;
            Assert.AreEqual(DefensePayloadType.AoEExplosion, fire.Projectile.PayloadType);
            Assert.AreEqual(DefenseTrajectoryType.BallisticGround, fire.Projectile.TrajectoryType);
            Assert.IsInstanceOf<SO_BurnEffect>(fire.Effects[0]);
            Assert.IsTrue(fire.Projectile.StatusEffect.Sustained);
        }
    }
    internal static class TowerProjectileTestExtensions
    {
        public static ProjectileCore3D SingleActive(this ProjectileCore3D[] cores)
        {
            foreach (var core in cores) if (core.IsInFlight) return core;
            throw new InvalidOperationException("No active projectile");
        }
    }
}

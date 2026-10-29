using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using UnityEngine;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseEnemyFoundationTests
    {
        [TestCase("danbinh")]
        [TestCase("phutapdich")]
        [TestCase("thaylang")]
        public void InactiveMinionPrefab_CanInstantiateWithoutConstructorNavMeshErrors(string id)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Art/Minions/Vongxuyen/Prefabs/" + id + "Minion.prefab");
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = null;
            try
            {
                instance = Object.Instantiate(prefab);
                Assert.That(instance.activeSelf, Is.False);
                Assert.That(instance.GetComponent<DefenseMinion3D>(), Is.Not.Null);
                UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
            }
            finally { if (instance != null) Object.DestroyImmediate(instance); }
        }

        [Test]
        public void InitializeFromDefinition_UsesSnapshotWithoutMutatingSharedData()
        {
            var definition = ScriptableObject.CreateInstance<DefenseEnemyDataSO>();
            var root = new GameObject("IndependentEnemy");
            var session = new DefenseSession();
            try
            {
                var enemy = root.AddComponent<DefenseEnemy3D>();
                enemy.BindGameplay(session);
                session.Start();
                enemy.Initialize(definition);
                int deaths = 0;
                enemy.Defeated += () => deaths++;
                Assert.That(enemy.Stats.Speed, Is.EqualTo(definition.MoveSpeed));
                Assert.That(enemy.Stats.Damage, Is.EqualTo(definition.AttackDamage));
                Assert.That(enemy.Stats.WoodReward, Is.EqualTo(definition.WoodDrop));
                Assert.That(enemy.TakeDamage(new DefenseDamageContext(10000f)), Is.True);
                Assert.That(enemy.TakeDamage(new DefenseDamageContext(10000f)), Is.False);
                Assert.That(deaths, Is.EqualTo(1));
                Assert.That(definition.MaxHP, Is.EqualTo(130f));
                enemy.Initialize(definition);
                Assert.That(enemy.IsDead, Is.False);
                Assert.That(enemy.CurrentHP, Is.EqualTo(definition.MaxHP));
            }
            finally { session.Dispose(); Object.DestroyImmediate(root); Object.DestroyImmediate(definition); }
        }

        [Test]
        public void TargetQuery_AcceptsEnemyWithoutBridgeWalker_AndSkipsDeadDisabledTargets()
        {
            var nearRoot = new GameObject("NearEnemy");
            var farRoot = new GameObject("FarEnemy");
            var near = nearRoot.AddComponent<DefenseEnemy3D>();
            var far = farRoot.AddComponent<DefenseEnemy3D>();
            var origin = new Vector3(24000f, 0f, 0f);
            near.transform.position = origin + Vector3.right;
            far.transform.position = origin + Vector3.right * 4f;
            var session = new DefenseSession();
            near.BindGameplay(session);
            far.BindGameplay(session);
            session.Start();
            DefenseEnemyTargets3D.Register(session.TargetRegistry, near);
            DefenseEnemyTargets3D.Register(session.TargetRegistry, far);
            try
            {
                Assert.That(DefenseEnemyTargets3D.FindNearest(session.Targets, origin, 5f), Is.SameAs(near));
                near.TakeDamage(new DefenseDamageContext(10000f));
                Assert.That(DefenseEnemyTargets3D.FindNearest(session.Targets, origin, 5f), Is.SameAs(far));
                far.enabled = false;
                Assert.That(DefenseEnemyTargets3D.FindNearest(session.Targets, origin, 5f), Is.Null);
            }
            finally
            {
                DefenseEnemyTargets3D.Unregister(session.TargetRegistry, near);
                DefenseEnemyTargets3D.Unregister(session.TargetRegistry, far);
                session.Dispose();
                Object.DestroyImmediate(nearRoot);
                Object.DestroyImmediate(farRoot);
            }
        }

        [Test]
        public void AuthoredMaGiapDefinition_ContainsComposablePrefabAndAllSpawnStats()
        {
            var definition = Resources.Load<DefenseEnemyDataSO>(DefenseEnemyFactory3D.DefinitionsFolder + "magiap");
            Assert.That(definition, Is.Not.Null);
            var stats = definition.CreateStats();
            Assert.That(stats.EnemyId, Is.EqualTo("magiap"));
            Assert.That(definition.Prefab.GetComponent<DefenseEnemy3D>(), Is.Not.Null);
            Assert.That(definition.Prefab.GetComponent<DefenseGroundMovement3D>(), Is.Not.Null);
            Assert.That(definition.Prefab.GetComponent<DefenseEnemyAttack3D>(), Is.Not.Null);
            Assert.That(definition.Prefab.GetComponent<DefenseEnemyDrop3D>(), Is.Not.Null);
            Assert.That(stats.MaxHp, Is.EqualTo(definition.MaxHP));
            Assert.That(stats.AttackInterval, Is.EqualTo(definition.AttackInterval));
            Assert.That(stats.AttackRange, Is.EqualTo(definition.AttackRange));
        }

        [Test]
        public void EnemyPoolManager_ParentDisableCanReturnActorAndReuseItOutsideTheCallback()
        {
            var poolRoot = new GameObject("PoolRoot");
            var actors = new GameObject("Actors");
            var session = new DefenseSession();
            var pool = new DefenseEnemyPoolManager(poolRoot.transform);
            GameObject instance = null;
            try
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab");
                instance = pool.Get(prefab, actors.transform);
                var enemy = instance.GetComponent<DefenseEnemy3D>();
                enemy.BindGameplay(session);
                session.Start();
                var lifetime = new DefenseActorLifetime(session.SessionIdentity, session.RunGeneration, 1);
                int releases = 0;
                enemy.BindLifetime(lifetime, (actor, lease, reason) =>
                {
                    if (!lease.TryRetire(session.SessionIdentity, session.RunGeneration, reason)) return;
                    releases++;
                    Assert.That(pool.TryRelease(actor.gameObject), Is.True);
                });
                instance.SetActive(true);

                actors.SetActive(false);

                UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
                Assert.That(releases, Is.EqualTo(1));
                Assert.That(instance.activeSelf, Is.False);
                Assert.That(instance.transform.parent, Is.SameAs(actors.transform));
                var reused = pool.Get(prefab);
                Assert.That(reused, Is.SameAs(instance));
                Assert.That(reused.transform.parent, Is.SameAs(poolRoot.transform));
                Assert.That(reused.activeSelf, Is.False);
            }
            finally
            {
                session.Dispose();
                if (instance != null) Object.DestroyImmediate(instance);
                pool.Dispose();
                Object.DestroyImmediate(actors);
                Object.DestroyImmediate(poolRoot);
            }
        }

        [Test]
        public void EnemyPoolManager_CanAcquireReleaseAndReuseInstance()
        {
            var poolRoot = new GameObject("TestEnemyPoolRoot");
            var dummyPrefab = new GameObject("DummyEnemyPrefab");
            dummyPrefab.AddComponent<DefenseEnemy3D>();

            var poolManager = new DefenseEnemyPoolManager(poolRoot.transform);
            try
            {
                var first = poolManager.Get(dummyPrefab);
                Assert.That(first, Is.Not.Null);

                bool released = poolManager.TryRelease(first);
                Assert.That(released, Is.True);
                Assert.That(first.activeSelf, Is.False);
                Assert.That(first.transform.parent, Is.EqualTo(poolRoot.transform));

                var second = poolManager.Get(dummyPrefab);
                Assert.That(second, Is.SameAs(first), "ObjectPool should reuse the same GameObject instance without re-instantiating.");
            }
            finally
            {
                poolManager.Dispose();
                Object.DestroyImmediate(dummyPrefab);
                Object.DestroyImmediate(poolRoot);
            }
        }
    }
}

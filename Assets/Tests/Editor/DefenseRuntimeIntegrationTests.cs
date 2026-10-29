using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using UnityEditor;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseRuntimeIntegrationTests
    {
        private static readonly Vector3 TestOrigin = new Vector3(22000f, 1f, 0f);

        [UnitySetUp]
        public IEnumerator EnterRuntime()
        {
            yield return new EnterPlayMode();
            Time.timeScale = 1f;
            // Isolate the fixture from the currently open map's active run.
            // Play Mode exit restores all scene objects and authored values.
            foreach (var run in Object.FindObjectsOfType<DefenseRun6003D>()) run.gameObject.SetActive(false);
            foreach (var smoke in Object.FindObjectsOfType<DefenseEnemyBridgeSmokeTest3D>()) smoke.gameObject.SetActive(false);
            foreach (var wave in Object.FindObjectsOfType<DefenseWaveSlice3D>()) wave.gameObject.SetActive(false);
            foreach (var wallet in Object.FindObjectsOfType<DefenseWoodWallet3D>()) wallet.gameObject.SetActive(false);
            foreach (var enemy in Object.FindObjectsOfType<DefenseEnemyBridgeWalker3D>()) enemy.gameObject.SetActive(false);
            foreach (var drop in Object.FindObjectsOfType<DefenseWoodDrop3D>()) drop.gameObject.SetActive(false);
        }

        [UnityTearDown]
        public IEnumerator ExitRuntime()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
                yield return new ExitPlayMode();
            }
        }

        [UnityTest]
        public IEnumerator EnemyFoundation_RuntimeScenariosComplete()
        {
            // One Play Mode session avoids repeated domain-reload replay in UTF 1.1.33.
            yield return EnemyAttack_UsesDefinitionCooldown_AndStopsWhenDisabled();
            yield return EnemyDeath_WoodPersistsUntilApproach_AndRewardsExactlyOnce();
            yield return HeroAndTower_DamageEnemyWithoutBridgeWalker();
            yield return MaGiapFactory_FollowsNavMeshRoute_ThenAttacksBase();
            yield return YakshaTelegraph_DamagesInside_SparesOutside_AndStopsOnEnd();
            TestContext.WriteLine("RUNTIME_COMPLETED: AllScenarios");
        }

        private IEnumerator MaGiapFactory_FollowsNavMeshRoute_ThenAttacksBase()
        {
            var settings = NavMesh.GetSettingsByIndex(0);
            var sources = new System.Collections.Generic.List<NavMeshBuildSource> {
                new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box,
                    size = new Vector3(50f, 0.2f, 35f),
                    transform = Matrix4x4.TRS(new Vector3(0f, -0.1f, 0f), Quaternion.identity, Vector3.one),
                    area = 0
                }
            };
            var data = NavMeshBuilder.BuildNavMeshData(settings, sources,
                new Bounds(Vector3.zero, new Vector3(60f, 10f, 40f)), Vector3.zero, Quaternion.identity);
            Assert.That(data, Is.Not.Null);
            // Avoid sampling a disconnected surface from the open map. This is
            // Play Mode only; exiting restores the map's serialized NavMesh data.
            NavMesh.RemoveAllNavMeshData();
            var navMesh = NavMesh.AddNavMeshData(data);
            var definition = Object.Instantiate(Resources.Load<DefenseEnemyDataSO>(
                DefenseEnemyFactory3D.DefinitionsFolder + "magiap"));
            try
            {
                SetField(definition, "moveSpeed", 12f);
                SetField(definition, "attackInterval", 0.25f);
                var target = new GameObject("QA_RouteBase").AddComponent<DefenseBasePrototype3D>();
                target.transform.position = new Vector3(-12f, 0f, 0f);
                float hpBefore = target.CurrentHP;
                var actor = DefenseEnemyFactory3D.Spawn(definition, new Vector3(10f, 0f, -0.5f), 1, target);
                var movement = actor.GetComponent<DefenseGroundMovement3D>();
                int arrivals = 0;
                movement.Arrived += () => arrivals++;
                Assert.That(actor.CurrentHP, Is.EqualTo(definition.MaxHP));
                Assert.That(actor.GetComponent<NavMeshAgent>().speed, Is.EqualTo(12f));
                float deadline = Time.realtimeSinceStartup + 20f;
                while (target.CurrentHP == hpBefore && Time.realtimeSinceStartup < deadline)
                {
                    Assert.That(movement.HasFailed, Is.False);
                    Assert.That(actor.IsDead, Is.False);
                    yield return null;
                }
                Assert.That(arrivals, Is.EqualTo(1));
                Assert.That(target.CurrentHP, Is.LessThan(hpBefore));
                Assert.That(target.CurrentHP, Is.EqualTo(hpBefore - definition.AttackDamage));
                TestContext.WriteLine("RUNTIME_COMPLETED: MaGiapFactory");
                actor.gameObject.SetActive(false);
                yield return StageConfig_QueuesSpawnsAndReleasesCapacityAfterDeathAndRestart();
            }
            finally
            {
                navMesh.Remove();
                Object.Destroy(data);
                Object.Destroy(definition);
            }
        }

        private IEnumerator StageConfig_QueuesSpawnsAndReleasesCapacityAfterDeathAndRestart()
        {
            var config = Object.Instantiate(Resources.Load<StageSpawnConfig>(StageSpawnConfig.DefaultResource));
            var serialized = new SerializedObject(config);
            var events = serialized.FindProperty("events");
            events.arraySize = 2;
            for (int i = 0; i < 2; i++)
            {
                var entry = events.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("EventId").stringValue = "qa-spawn-" + i;
                entry.FindPropertyRelative("TriggerTime").floatValue = i * 0.1f;
                entry.FindPropertyRelative("Enemy").objectReferenceValue = Resources.Load<DefenseEnemyDataSO>(DefenseEnemyFactory3D.DefinitionsFolder + "magiap");
                entry.FindPropertyRelative("LeadEnemy").objectReferenceValue = null;
                entry.FindPropertyRelative("ZoneId").stringValue = "eastBank";
                entry.FindPropertyRelative("Count").intValue = 1;
            }
            var zone = serialized.FindProperty("zones").GetArrayElementAtIndex(0);
            zone.FindPropertyRelative("RelativeToEastMarker").boolValue = false;
            zone.FindPropertyRelative("Origin").vector3Value = new Vector3(10f, 1f, -0.5f);
            var points = zone.FindPropertyRelative("Points");
            points.arraySize = 1;
            points.GetArrayElementAtIndex(0).FindPropertyRelative("Offset").vector3Value = Vector3.zero;
            points.GetArrayElementAtIndex(0).FindPropertyRelative("Lane").intValue = 1;
            serialized.FindProperty("maxActiveEnemies").intValue = 1;
            serialized.FindProperty("bossSpawnTime").floatValue = 599f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var target = new GameObject("QA_ConfigBase").AddComponent<DefenseBasePrototype3D>();
            target.transform.position = new Vector3(-12f, 0f, 0f);
            var info = new GameObject("QA_ConfigSceneInfo").AddComponent<DefenseSceneInfo3D>();
            info.enabled = false;
            var marker = new GameObject("QA_SpawnMarker").transform;
            info.Configure(marker, target.transform, marker, new[] { marker, marker, marker },
                new[] { marker, marker, marker, marker, marker, marker, marker, marker, marker, marker });
            var run = new GameObject("QA_ConfigRun").AddComponent<DefenseRun6003D>();
            SetField(run, "sceneInfo", info);
            SetField(run, "defenseBase", target);
            SetField(run, "stageSpawnConfig", config);
            try
            {
                for (float until = Time.time + 0.25f; Time.time < until;) yield return null;
                var first = FindNamedEnemy("qa-spawn-0");
                Assert.That(first, Is.Not.Null);
                Assert.That(FindNamedEnemy("qa-spawn-1"), Is.Null, "active cap must hold the second job");
                first.TakeDamage(new DefenseDamageContext(10000f));
                yield return null;
                yield return null;
                yield return null;
                Assert.That(FindNamedEnemy("qa-spawn-1"), Is.Not.Null, "destroyed enemy must release its active slot");
                run.RestartRun();
                yield return null;
                yield return null;
                yield return null;
                Assert.That(FindNamedEnemy("qa-spawn-0"), Is.Not.Null);
                Assert.That(FindNamedEnemy("qa-spawn-1"), Is.Null, "old deferred removal must not release a new-run slot");
                TestContext.WriteLine("RUNTIME_COMPLETED: StageSpawnConfig");
            }
            finally
            {
                run.gameObject.SetActive(false);
                foreach (var enemy in Object.FindObjectsOfType<DefenseEnemy3D>())
                {
                    if (!enemy.name.Contains("qa-spawn-")) continue;
                    enemy.gameObject.SetActive(false);
                    Object.Destroy(enemy.gameObject);
                }
                Object.Destroy(run.gameObject);
                Object.Destroy(config);
            }
        }

        private static DefenseEnemy3D FindNamedEnemy(string eventId)
        {
            foreach (var enemy in Object.FindObjectsOfType<DefenseEnemy3D>())
                if (enemy.name.Contains(eventId)) return enemy;
            return null;
        }

        private IEnumerator HeroAndTower_DamageEnemyWithoutBridgeWalker()
        {
            var enemyObject = new GameObject("QA_ComposableEnemy");
            enemyObject.transform.position = TestOrigin;
            var enemy = enemyObject.AddComponent<DefenseEnemy3D>();
            var stats = DefenseEnemyRegistry.GetStats(DefenseEnemyRegistry.ID_MAGIAP);
            stats.MaxHp = 1000f;
            enemy.Initialize(stats);
            var hero = new GameObject("QA_ComposableHero");
            hero.transform.position = TestOrigin;
            hero.AddComponent<DefenseHeroAutoAttack3D>();
            yield return null;
            yield return null;
            Assert.That(enemy.CurrentHP, Is.EqualTo(965f));
            hero.SetActive(false);
            var tower = new GameObject("QA_ComposableTower");
            tower.transform.position = TestOrigin;
            tower.AddComponent<DefenseTowerPrototype3D>();
            yield return null;
            yield return null;
            Assert.That(enemy.CurrentHP, Is.EqualTo(943f));
            tower.SetActive(false);
            Assert.That(enemyObject.GetComponent<DefenseEnemyBridgeWalker3D>(), Is.Null);
            TestContext.WriteLine("RUNTIME_COMPLETED: HeroAndTower");
        }

        private IEnumerator EnemyAttack_UsesDefinitionCooldown_AndStopsWhenDisabled()
        {
            var root = new GameObject("QA_Attacker");
            var actor = root.AddComponent<DefenseEnemy3D>();
            var stats = DefenseEnemyRegistry.GetStats(DefenseEnemyRegistry.ID_MAGIAP);
            stats.AttackInterval = 0.5f;
            actor.Initialize(stats);
            var attack = root.AddComponent<DefenseEnemyAttack3D>();
            var baseObject = new GameObject("QA_TargetBase");
            var target = baseObject.AddComponent<DefenseBasePrototype3D>();
            float hpBefore = target.CurrentHP;
            attack.Begin(target);
            yield return null;
            yield return null;
            Assert.That(target.CurrentHP, Is.EqualTo(hpBefore - stats.Damage));
            for (float until1 = Time.time + 0.15f; Time.time < until1;) yield return null;
            Assert.That(target.CurrentHP, Is.EqualTo(hpBefore - stats.Damage));
            attack.enabled = false;
            for (float until2 = Time.time + 0.6f; Time.time < until2;) yield return null;
            Assert.That(target.CurrentHP, Is.EqualTo(hpBefore - stats.Damage));
            TestContext.WriteLine("RUNTIME_COMPLETED: EnemyAttack");
        }

        private IEnumerator EnemyDeath_WoodPersistsUntilApproach_AndRewardsExactlyOnce()
        {
            var hero = new GameObject("QA_WoodHero");
            hero.transform.position = TestOrigin + Vector3.right * 20f;
            var wallet = hero.AddComponent<DefenseWoodWallet3D>();
            var enemyObject = DefenseEnemyPrefab3D.Instantiate();
            enemyObject.GetComponent<NavMeshAgent>().enabled = false;
            enemyObject.transform.position = TestOrigin;
            var enemy = enemyObject.GetComponent<DefenseEnemyBridgeWalker3D>();
            enemy.PrepareArenaBoss(100f);
            int deaths = 0;
            enemy.Defeated += _ => deaths++;
            enemy.ReceiveDamage(1000f);
            enemy.ReceiveDamage(1000f);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(Object.FindObjectsOfType<DefenseWoodDrop3D>().Length, Is.EqualTo(1));
            var drop = Object.FindObjectOfType<DefenseWoodDrop3D>();
            Assert.That(wallet.Wood, Is.Zero, "death alone must not credit the wallet");
            yield return null;
            Assert.That(enemyObject == null, Is.True);
            for (float until3 = Time.time + 0.9f; Time.time < until3;) yield return null;
            Assert.That(drop != null, Is.True, "wood must remain while Hero is far away");
            Assert.That(wallet.Wood, Is.Zero);
            hero.transform.position = drop.transform.position;
            yield return null;
            yield return null;
            Assert.That(wallet.Wood, Is.EqualTo(10));
            Assert.That(drop == null, Is.True);
            yield return null;
            Assert.That(wallet.Wood, Is.EqualTo(10), "reward must be credited only once");

            drop = DefenseWoodDrop3D.Spawn(hero.transform.position, 20, hero.transform, wallet);
            Time.timeScale = 0f;
            for (float until4 = Time.realtimeSinceStartup + 0.2f; Time.realtimeSinceStartup < until4;) yield return null;
            Assert.That(drop != null, Is.True);
            Assert.That(wallet.Wood, Is.EqualTo(10), "pause must freeze pickup delay");
            Time.timeScale = 1f;
            for (float until5 = Time.time + 0.9f; Time.time < until5;) yield return null;
            Assert.That(drop == null, Is.True);
            Assert.That(wallet.Wood, Is.EqualTo(30));

            Object.Destroy(hero);
            yield return null;
            drop = DefenseWoodDrop3D.Spawn(TestOrigin, 10, null, null);
            for (float until6 = Time.time + 0.9f; Time.time < until6;) yield return null;
            Assert.That(drop != null, Is.True, "missing Hero must not suppress a drop");
            var replacementHero = new GameObject("QA_ReplacementHero");
            replacementHero.transform.position = drop.transform.position;
            var replacementWallet = replacementHero.AddComponent<DefenseWoodWallet3D>();
            yield return null;
            yield return null;
            Assert.That(replacementWallet.Wood, Is.EqualTo(10));
            Assert.That(drop == null, Is.True);
            TestContext.WriteLine("RUNTIME_COMPLETED: WoodPickup");
        }

        private IEnumerator YakshaTelegraph_DamagesInside_SparesOutside_AndStopsOnEnd()
        {
            var hero = new GameObject("QA_ArenaHero");
            hero.transform.position = TestOrigin;
            var health = hero.AddComponent<DefenseHeroHealth3D>();
            var boss = Object.Instantiate(Resources.Load<GameObject>("Defense3D/DefenseYaksha3D"));
            boss.name = "QA_ArenaYaksha";
            boss.transform.position = TestOrigin;
            var combat = boss.GetComponent<DefenseYakshaArenaCombat3D>();
            SetField(combat, "blastInterval", 0.25f);
            SetField(combat, "warningDuration", 0.2f);
            combat.Begin(health);
            var ring = boss.GetComponentInChildren<LineRenderer>();
            Assert.That(ring, Is.Not.Null);
            Assert.That(ring.enabled, Is.False);
            float hpBefore = health.CurrentHP;
            for (float until7 = Time.time + 0.3f; Time.time < until7;) yield return null;
            Assert.That(ring.enabled, Is.True);
            Assert.That(health.CurrentHP, Is.EqualTo(hpBefore), "warning must not deal damage");
            for (float until8 = Time.time + 0.2f; Time.time < until8;) yield return null;
            Assert.That(health.CurrentHP, Is.EqualTo(hpBefore - 30f));
            hero.transform.position = TestOrigin + Vector3.right * 20f;
            for (float until9 = Time.time + 0.6f; Time.time < until9;) yield return null;
            Assert.That(health.CurrentHP, Is.EqualTo(hpBefore - 30f));
            combat.EndArena();
            Assert.That(ring.enabled, Is.False);
            hero.transform.position = TestOrigin;
            for (float until10 = Time.time + 0.6f; Time.time < until10;) yield return null;
            Assert.That(health.CurrentHP, Is.EqualTo(hpBefore - 30f));
            TestContext.WriteLine("RUNTIME_COMPLETED: YakshaArena");
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}

using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseEnemyPrefab3DTests
    {
        [TestCase(false, DefenseEnemyPrefab3D.EnemyResource)]
        [TestCase(true, DefenseEnemyPrefab3D.BossResource)]
        public void SpawnedEnemy_KeepsEditableSourcePrefab(bool boss, string resource)
        {
            GameObject instance = DefenseEnemyPrefab3D.Instantiate(boss);
            try
            {
                Assert.That(PrefabUtility.IsPartOfPrefabInstance(instance), Is.True);
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(instance),
                    Is.SameAs(Resources.Load<GameObject>(resource)));
                Assert.That(instance.GetComponents<DefenseEnemyBridgeWalker3D>().Length, Is.EqualTo(1));
                Assert.That(instance.GetComponent<NavMeshAgent>(), Is.Not.Null);
                Assert.That(instance.GetComponent<Collider>(), Is.Null,
                    "Defense prototype enemies retain their visual-only collision setup");
                Assert.That(instance.GetComponent<Renderer>().sharedMaterial, Is.Not.Null);
                Assert.That(instance.GetComponent<DefenseEnemyBridgeWalker3D>().IsDead, Is.False);
            }
            finally { Object.DestroyImmediate(instance); }
        }
    }
}

using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseWoodDrop3DTests
    {
        [Test]
        public void SpawnWithoutCollector_StillCreatesVisiblePrefab()
        {
            var drop = DefenseWoodDrop3D.Spawn(new Vector3(22000f, 1f, 0f), 10, null, null);
            try
            {
                Assert.That(drop.gameObject.activeInHierarchy, Is.True);
                Assert.That(drop.GetComponent<Renderer>().enabled, Is.True);
                Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(drop.gameObject),
                    Is.SameAs(Resources.Load<GameObject>(DefenseWoodDrop3D.ResourcePath)));
                Assert.That(drop.transform.position.x, Is.EqualTo(22000f));
            }
            finally { Object.DestroyImmediate(drop.gameObject); }
        }

        [Test]
        public void SpawnOnHero_DoesNotCollectBeforePickupDelay()
        {
            var hero = new GameObject("WoodPickupTestHero");
            hero.transform.position = new Vector3(22000f, 0f, 0f);
            var wallet = hero.AddComponent<DefenseWoodWallet3D>();
            var drop = DefenseWoodDrop3D.Spawn(hero.transform.position, 10, hero.transform, wallet);
            int woodBefore = wallet.Wood;
            try
            {
                // Exercise the first update explicitly: the old implementation
                // collected and destroyed the pickup as soon as it spawned nearby.
                typeof(DefenseWoodDrop3D).GetMethod("Update",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(drop, null);
                Assert.That(drop != null && drop.gameObject.activeInHierarchy, Is.True);
                Assert.That(wallet.Wood, Is.EqualTo(woodBefore));
            }
            finally
            {
                if (drop != null) Object.DestroyImmediate(drop.gameObject);
                Object.DestroyImmediate(hero);
            }
        }

        [Test]
        public void WoodDropPool_CanAcquireReleaseAndReuseInstance()
        {
            var prefab = Resources.Load<GameObject>(DefenseWoodDrop3D.ResourcePath);
            Assert.That(prefab, Is.Not.Null);

            var poolRoot = new GameObject("TestWoodPoolRoot");
            var pool = new DefenseWoodDropPool(prefab, poolRoot.transform, defaultCapacity: 5, maxSize: 10);
            try
            {
                var drop1 = pool.Get();
                Assert.That(drop1.gameObject.activeInHierarchy, Is.True);
                var goInstanceId = drop1.gameObject.GetInstanceID();

                // Trả về pool
                pool.Release(drop1);
                Assert.That(drop1.gameObject.activeSelf, Is.False);

                // Lấy lại từ pool - phải tái sử dụng lại đúng instance vừa trả về (LIFO)
                var drop2 = pool.Get();
                Assert.That(drop2.gameObject.activeInHierarchy, Is.True);
                Assert.That(drop2.gameObject.GetInstanceID(), Is.EqualTo(goInstanceId));

                pool.Release(drop2);
            }
            finally
            {
                pool.Dispose();
                Object.DestroyImmediate(poolRoot);
            }
        }
    }
}

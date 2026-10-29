using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class DefenseProjectileCoreAndPoolTests
    {
        private GameObject _coreObj;
        private ProjectileCore3D _core;
        private ProjectileVisual3D _visual;

        [SetUp]
        public void SetUp()
        {
            _coreObj = new GameObject("Test_Projectile");
            _core = _coreObj.AddComponent<ProjectileCore3D>();

            var visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(_coreObj.transform);
            _visual = visualObj.AddComponent<ProjectileVisual3D>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_coreObj != null) Object.DestroyImmediate(_coreObj);
        }

        [Test]
        public void ProjectileCore3D_LaunchAndDespawn_InvokesCallback()
        {
            bool returnedToPool = false;

            _core.Launch(
                source: null,
                origin: Vector3.zero,
                target: null,
                damage: 50f,
                speed: 25f,
                arcHeight: 0.5f,
                effects: null,
                runGeneration: 1,
                sessionIdentity: new object(),
                gameplay: null,
                returnToPool: (c) => returnedToPool = true);

            Assert.IsTrue(_coreObj.activeSelf, "Projectile phải active sau khi Launch");

            _core.Despawn();

            Assert.IsFalse(_coreObj.activeSelf, "Projectile phải inactive sau khi Despawn");
            Assert.IsTrue(returnedToPool, "ReturnToPool callback phải được kích hoạt khi Despawn");
        }

        [Test]
        public void SO_TowerData_ContainsProjectileConfiguration()
        {
            var towerData = ScriptableObject.CreateInstance<SO_TowerData>();
            Assert.IsNotNull(towerData);
            Assert.AreEqual(22f, towerData.ProjectileSpeed, 0.01f);
            Assert.AreEqual(0.6f, towerData.ProjectileArcHeight, 0.01f);
            Object.DestroyImmediate(towerData);
        }

        [Test]
        public void DefenseProjectilePool3D_CreatesAndRecyclesInstances()
        {
            var poolObj = new GameObject("Test_Pool");
            var pool = poolObj.AddComponent<DefenseProjectilePool3D>();

            var samplePrefab = new GameObject("Sample_Proj_Prefab");
            samplePrefab.AddComponent<ProjectileCore3D>();

            var objectPool = pool.GetOrCreatePool(samplePrefab);
            Assert.IsNotNull(objectPool, "ObjectPool phải được tạo thành công");

            var proj1 = objectPool.Get();
            Assert.IsNotNull(proj1, "Instance lấy từ Pool không được null");

            objectPool.Release(proj1);
            Assert.IsFalse(proj1.gameObject.activeSelf, "Instance sau khi Release phải inactive");

            Object.DestroyImmediate(samplePrefab);
            Object.DestroyImmediate(poolObj);
        }
    }
}

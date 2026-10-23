using System.Collections.Generic;
using NUnit.Framework;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Tests.Editor
{
    /// <summary>
    /// EditMode physics checks; these exercise query code directly.
    /// They do NOT validate NavMesh baking, FixedUpdate movement or Android performance.
    /// </summary>
    [TestFixture]
    public sealed class Migration3DTraversalAndTargetingTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();
        private int _enemyLayer;
        private int _groundLayer;
        private int _obstacleLayer;

        [SetUp]
        public void SetUp()
        {
            _enemyLayer = LayerMask.NameToLayer("Enemy");
            _groundLayer = LayerMask.NameToLayer("Ground");
            _obstacleLayer = LayerMask.NameToLayer("Obstacle");
            Assert.That(_enemyLayer, Is.GreaterThanOrEqualTo(0), "Enemy layer must exist");
            Assert.That(_groundLayer, Is.GreaterThanOrEqualTo(0), "3D migration requires Ground layer");
            Assert.That(_obstacleLayer, Is.GreaterThanOrEqualTo(0), "Obstacle layer must exist");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            _created.Clear();
        }

        [Test]
        public void Targeting_ReturnsEntityRootOnlyOnce_WhenHitboxesAreChildren()
        {
            Vector3 center = new Vector3(9000f, 0f, 0f);
            var enemy = MakeEnemy(center + Vector3.right * 2f, 3);
            Physics.SyncTransforms();
            var matches = new List<Transform>(8);

            int count = TargetingUtility3D.FindTargetsNonAlloc(center, 7f, matches);
            Assert.That(count, Is.EqualTo(1), "three hitboxes belong to one enemy");
            Assert.That(matches[0], Is.SameAs(enemy.transform));
            Assert.That(TargetingUtility3D.FindTarget(center, 7f),
                Is.SameAs(enemy.transform));
        }

        [Test]
        public void Targeting_SkipsDeadEntity_AndDoesNotFavorExtraHitboxes()
        {
            Vector3 center = new Vector3(9000f, 0f, 20f);
            var dead = MakeEnemy(center + Vector3.right, 3);
            dead.GetComponent<HealthSystem>().SetCurrentHealth(0f);
            var live = MakeEnemy(center + Vector3.right * 4f, 1);
            Physics.SyncTransforms();

            var matches = new List<Transform>(8);
            Assert.That(TargetingUtility3D.FindTargetsNonAlloc(center, 8f, matches), Is.EqualTo(1));
            Assert.That(matches[0], Is.SameAs(live.transform));
            Assert.That(TargetingUtility3D.FindTarget(center, 8f, TargetPriority.LowestHealth),
                Is.SameAs(live.transform));
            for (int i = 0; i < 16; i++)
            {
                Assert.That(TargetingUtility3D.FindTarget(center, 8f, TargetPriority.RandomInRange),
                    Is.SameAs(live.transform));
            }
        }

        [Test]
        public void GroundProbe_IgnoresDefaultLayerPropAboveDedicatedGround()
        {
            Vector3 center = new Vector3(9000f, 0f, 100f);
            MakeGround(center, 10f);
            MakeBox("DefaultProp", center + Vector3.up, new Vector3(1f, 0.2f, 1f), 0);
            Physics.SyncTransforms();

            Assert.IsTrue(MovementPhysicsUtility3D.TryGetGroundHeight(
                center + Vector3.up, out float groundY));
            Assert.That(groundY, Is.EqualTo(0f).Within(0.05f));
        }

        [Test]
        public void DashStopsBeforeThreeDimensionalObstacle()
        {
            Vector3 center = new Vector3(9000f, 0f, 130f);
            MakeGround(center, 14f);
            MakeBox("Obstacle", center + new Vector3(2f, 1f, 0f),
                new Vector3(0.5f, 2f, 2f), _obstacleLayer);
            Physics.SyncTransforms();

            Vector3 result = MovementPhysicsUtility3D.CalculateDashDestination(
                center + Vector3.up, Vector3.right, 5f);
            Assert.That(result.x - center.x, Is.GreaterThan(0.1f));
            Assert.That(result.x - center.x, Is.LessThan(1.7f));
        }

        [Test]
        public void DashStopsAtGroundEdge_InsteadOfMovingAcrossEmptySpace()
        {
            Vector3 center = new Vector3(9000f, 0f, 160f);
            MakeGround(center, 2f);
            Physics.SyncTransforms();

            Vector3 result = MovementPhysicsUtility3D.CalculateDashDestination(
                center + Vector3.up, Vector3.right, 5f);
            Assert.That(result.x - center.x, Is.GreaterThan(0.1f));
            Assert.That(result.x - center.x, Is.LessThanOrEqualTo(1.0f));
        }

        [Test]
        public void TeleportCandidate_IsRecheckedAgainstObstacle()
        {
            Vector3 center = new Vector3(9000f, 0f, 190f);
            MakeGround(center, 16f);
            MakeBox("Obstacle", center + new Vector3(3f, 1f, 0f),
                new Vector3(1f, 2f, 2f), _obstacleLayer);
            Physics.SyncTransforms();

            Vector3 result = MovementPhysicsUtility3D.ClampToWalkableGround(
                center + new Vector3(3f, 1f, 0f),
                center + Vector3.up);
            Assert.IsTrue(MovementPhysicsUtility3D.IsPositionWalkable(result));
        }

        private GameObject MakeEnemy(Vector3 pos, int numberOfHitboxes)
        {
            var root = MakeBox("EnemyRoot", pos, Vector3.one * 0.1f, _enemyLayer, false);
            root.AddComponent<HealthSystem>();
            for (int i = 0; i < numberOfHitboxes; i++)
            {
                var child = new GameObject("Hitbox_" + i);
                child.layer = _enemyLayer;
                child.transform.SetParent(root.transform, false);
                child.transform.localPosition = new Vector3(i * 0.12f, 0.2f, 0f);
                child.AddComponent<BoxCollider>();
            }
            return root;
        }

        private void MakeGround(Vector3 center, float width)
        {
            MakeBox("Ground", center + Vector3.down * 0.1f,
                new Vector3(width, 0.2f, width), _groundLayer);
        }

        private GameObject MakeBox(string name, Vector3 position, Vector3 scale, int layer, bool collider = true)
        {
            var go = new GameObject(name);
            go.layer = layer;
            go.transform.position = position;
            go.transform.localScale = scale;
            if (collider) go.AddComponent<BoxCollider>();
            _created.Add(go);
            return go;
        }
    }
}

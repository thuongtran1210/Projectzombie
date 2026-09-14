using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Spawners.Spatial;

namespace ProjectZombie.Tests.Editor
{
    public class MapBoundarySpawnTests
    {
        private GameObject _mapInstance;
        private GameObject _boundariesObj;

        [SetUp]
        public void SetUp()
        {
            _mapInstance = new GameObject("MapInstance");
            _boundariesObj = new GameObject("Map_Boundaries");
            _boundariesObj.transform.SetParent(_mapInstance.transform);

            // Sàn đấu 50 x 30 (-25..25 theo X, -15..15 theo Y)
            CreateWall("Wall_Top", new Vector2(0f, 16f), new Vector2(54f, 2f));
            CreateWall("Wall_Bottom", new Vector2(0f, -16f), new Vector2(54f, 2f));
            CreateWall("Wall_Left", new Vector2(-26f, 0f), new Vector2(2f, 30f));
            CreateWall("Wall_Right", new Vector2(26f, 0f), new Vector2(2f, 30f));
        }

        private void CreateWall(string name, Vector2 pos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(_boundariesObj.transform);
            wall.transform.position = pos;
            var box = wall.AddComponent<BoxCollider2D>();
            box.size = size;
        }

        [TearDown]
        public void TearDown()
        {
            if (_mapInstance != null)
            {
                Object.DestroyImmediate(_mapInstance);
            }
        }

        [Test]
        public void ArenaBoundaryContext_CalculatesSafeBounds_FromMapBoundariesCorrectly()
        {
            var context = new ArenaBoundaryContext(safeMargin: 1.0f);
            context.AssignMapInstance(_mapInstance);

            Bounds safeBounds = context.SafeMapBounds;

            // Mặt trong: Left max.x = -25 + 1 (margin) = -24. Right min.x = 25 - 1 = 24.
            // Bottom max.y = -15 + 1 = -14. Top min.y = 15 - 1 = 14.
            Assert.AreEqual(-24f, safeBounds.min.x, 0.1f);
            Assert.AreEqual(24f, safeBounds.max.x, 0.1f);
            Assert.AreEqual(-14f, safeBounds.min.y, 0.1f);
            Assert.AreEqual(14f, safeBounds.max.y, 0.1f);
        }

        [Test]
        public void CameraAwareSpawnLocator_SpawnsInsideSafeBounds_AndFarFromPlayer()
        {
            var context = new ArenaBoundaryContext(safeMargin: 1.0f);
            context.AssignMapInstance(_mapInstance);

            var locator = new CameraAwareSpawnLocator(context, camera: null, cameraPadding: 2.0f);

            var playerObj = new GameObject("Player");
            playerObj.transform.position = Vector3.zero;

            try
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector3 spawnPos = locator.GetSpawnPosition(playerObj.transform, 12f, 20f);

                    // Điểm spawn phải nằm trọn trong SafeMapBounds
                    Assert.IsTrue(context.SafeMapBounds.Contains(spawnPos), $"Điểm {spawnPos} vượt ra ngoài SafeMapBounds!");

                    // Điểm spawn phải cách Player ít nhất 10m
                    float dist = Vector3.Distance(spawnPos, playerObj.transform.position);
                    Assert.GreaterOrEqual(dist, 10f, $"Điểm {spawnPos} quá gần Player (dist = {dist})!");
                }
            }
            finally
            {
                Object.DestroyImmediate(playerObj);
            }
        }

        [Test]
        public void CameraAwareSpawnLocator_PlayerNearCorner_DoesNotSpawnAdjacentToPlayer()
        {
            var context = new ArenaBoundaryContext(safeMargin: 1.0f);
            context.AssignMapInstance(_mapInstance);

            var locator = new CameraAwareSpawnLocator(context, camera: null, cameraPadding: 2.0f);

            var playerObj = new GameObject("PlayerCorner");
            // Đặt Player sát góc trên bên phải (23, 13)
            playerObj.transform.position = new Vector3(23f, 13f, 0f);

            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector3 spawnPos = locator.GetSpawnPosition(playerObj.transform, 12f, 22f);

                    Assert.IsTrue(context.SafeMapBounds.Contains(spawnPos), $"Điểm {spawnPos} vượt ra ngoài SafeMapBounds!");
                    float dist = Vector3.Distance(spawnPos, playerObj.transform.position);
                    Assert.GreaterOrEqual(dist, 10f, $"Điểm {spawnPos} spawn quá sát Player ở góc (dist = {dist})!");
                }
            }
            finally
            {
                Object.DestroyImmediate(playerObj);
            }
        }
    }
}

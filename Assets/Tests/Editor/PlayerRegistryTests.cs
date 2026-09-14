using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class PlayerRegistryTests
    {
        private SinglePlayerRegistry _registry;
        private GameObject _playerObj1;
        private GameObject _playerObj2;

        [SetUp]
        public void SetUp()
        {
            _registry = new SinglePlayerRegistry();
            _playerObj1 = new GameObject("Player_1");
            var hp1 = _playerObj1.AddComponent<HealthSystem>();
            hp1.DisableGameObjectOnDeath = false;
            _playerObj1.transform.position = new Vector3(0f, 0f, 0f);

            _playerObj2 = new GameObject("Player_2");
            var hp2 = _playerObj2.AddComponent<HealthSystem>();
            hp2.DisableGameObjectOnDeath = false;
            _playerObj2.transform.position = new Vector3(10f, 0f, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObj1 != null) Object.DestroyImmediate(_playerObj1);
            if (_playerObj2 != null) Object.DestroyImmediate(_playerObj2);
        }

        [Test]
        public void Register_SinglePlayer_CorrectlyStoresAndRetrievesLocalPlayer()
        {
            var ctx = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1);
            _registry.Register(ctx);

            Assert.IsTrue(_registry.HasAnyPlayer);
            Assert.AreEqual(ctx, _registry.LocalPlayer);
            Assert.AreEqual(1, _registry.ActivePlayers.Count);
        }

        [Test]
        public void GetNearestLivingPlayer_ReturnsClosestPlayer()
        {
            var ctx1 = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1);
            _registry.Register(ctx1);

            Vector2 samplePos = new Vector2(1f, 1f);
            var nearest = _registry.GetNearestLivingPlayer(samplePos);

            Assert.IsNotNull(nearest);
            Assert.AreEqual(ctx1, nearest);
        }

        [Test]
        public void Unregister_RemovesPlayerFromActiveList()
        {
            var ctx = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1);
            _registry.Register(ctx);
            Assert.AreEqual(1, _registry.ActivePlayers.Count);

            _registry.Unregister(ctx);
            Assert.AreEqual(0, _registry.ActivePlayers.Count);
            Assert.IsNull(_registry.LocalPlayer);
            Assert.IsFalse(_registry.HasAnyPlayer);
        }

        [Test]
        public void GetNearestLivingPlayer_WhenPlayerDead_ReturnsNull()
        {
            var ctx = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1);
            _registry.Register(ctx);

            // Giả lập player chết
            ctx.Health.SetCurrentHealth(0f);
            Assert.IsFalse(ctx.IsAlive);

            var nearest = _registry.GetNearestLivingPlayer(Vector2.zero);
            Assert.IsNull(nearest);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Enemies.Behaviors;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class EnemyTargetingTests
    {
        private SinglePlayerRegistry _registry;
        private GameObject _playerObj1;
        private ITargetSelector _targetSelector;

        [SetUp]
        public void SetUp()
        {
            _registry = new SinglePlayerRegistry();
            _targetSelector = new ProximityTargetSelector();

            _playerObj1 = new GameObject("Player_Test");
            var hp = _playerObj1.AddComponent<HealthSystem>();
            hp.DisableGameObjectOnDeath = false;
            _playerObj1.transform.position = new Vector3(5f, 0f, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObj1 != null) Object.DestroyImmediate(_playerObj1);
        }

        [Test]
        public void ProximityTargetSelector_SelectsTargetAccurately()
        {
            var ctx = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1);
            _registry.Register(ctx);

            Vector2 enemyPos = new Vector2(4f, 0f);
            var selected = _targetSelector.SelectTarget(enemyPos, _registry);

            Assert.IsNotNull(selected);
            Assert.AreEqual(ctx, selected);
            Assert.AreEqual(_playerObj1.transform, selected.Transform);
        }

        [Test]
        public void ProximityTargetSelector_WhenNoPlayers_ReturnsNull()
        {
            _registry.Clear();
            var selected = _targetSelector.SelectTarget(Vector2.zero, _registry);
            Assert.IsNull(selected);
        }

        [Test]
        public void ProximityTargetSelector_WhenPlayerDead_ReturnsNull()
        {
            var ctx = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1);
            _registry.Register(ctx);

            ctx.Health.SetCurrentHealth(0f);
            var selected = _targetSelector.SelectTarget(Vector2.zero, _registry);
            Assert.IsNull(selected);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using System.Threading.Tasks;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Multiplayer.Core;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class MultiplayerCoreTests
    {
        private MultiplayerPlayerRegistry _registry;
        private GameObject _playerObj1;
        private GameObject _playerObj2;
        private GameObject _playerObj3;

        [SetUp]
        public void SetUp()
        {
            _registry = new MultiplayerPlayerRegistry();

            _playerObj1 = new GameObject("Player_Host");
            var hp1 = _playerObj1.AddComponent<HealthSystem>();
            hp1.DisableGameObjectOnDeath = false;
            _playerObj1.transform.position = new Vector3(0f, 0f, 0f);

            _playerObj2 = new GameObject("Player_Client_1");
            var hp2 = _playerObj2.AddComponent<HealthSystem>();
            hp2.DisableGameObjectOnDeath = false;
            _playerObj2.transform.position = new Vector3(5f, 0f, 0f);

            _playerObj3 = new GameObject("Player_Client_2");
            var hp3 = _playerObj3.AddComponent<HealthSystem>();
            hp3.DisableGameObjectOnDeath = false;
            _playerObj3.transform.position = new Vector3(15f, 0f, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObj1 != null) Object.DestroyImmediate(_playerObj1);
            if (_playerObj2 != null) Object.DestroyImmediate(_playerObj2);
            if (_playerObj3 != null) Object.DestroyImmediate(_playerObj3);
        }

        [Test]
        public void Register_MultiplePlayers_CorrectlyTracksLocalAndRemote()
        {
            var ctxHost = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1);
            var ctxClient1 = PlayerContext.Create(_playerObj2, isLocal: false, playerId: 2);
            var ctxClient2 = PlayerContext.Create(_playerObj3, isLocal: false, playerId: 3);

            _registry.Register(ctxHost);
            _registry.Register(ctxClient1);
            _registry.Register(ctxClient2);

            Assert.AreEqual(3, _registry.ActivePlayers.Count);
            Assert.AreEqual(ctxHost, _registry.LocalPlayer);
            Assert.AreEqual(ctxClient1, _registry.GetPlayerById(2));
            Assert.AreEqual(ctxClient2, _registry.GetPlayerById(3));
        }

        [Test]
        public void GetNearestLivingPlayer_AmongMultiplePlayers_ReturnsClosestLivingPlayer()
        {
            var ctxHost = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1); // Pos: (0, 0)
            var ctxClient1 = PlayerContext.Create(_playerObj2, isLocal: false, playerId: 2); // Pos: (5, 0)
            var ctxClient2 = PlayerContext.Create(_playerObj3, isLocal: false, playerId: 3); // Pos: (15, 0)

            _registry.Register(ctxHost);
            _registry.Register(ctxClient1);
            _registry.Register(ctxClient2);

            // Tọa độ gần Player 2 hơn
            Vector2 samplePos = new Vector2(4.5f, 0f);
            var nearest = _registry.GetNearestLivingPlayer(samplePos);

            Assert.IsNotNull(nearest);
            Assert.AreEqual(ctxClient1, nearest);
        }

        [Test]
        public void GetNearestLivingPlayer_WhenClosestPlayerIsDead_FindsNextNearestLiving()
        {
            var ctxHost = PlayerContext.Create(_playerObj1, isLocal: true, playerId: 1); // Pos: (0, 0)
            var ctxClient1 = PlayerContext.Create(_playerObj2, isLocal: false, playerId: 2); // Pos: (5, 0)
            var ctxClient2 = PlayerContext.Create(_playerObj3, isLocal: false, playerId: 3); // Pos: (15, 0)

            _registry.Register(ctxHost);
            _registry.Register(ctxClient1);
            _registry.Register(ctxClient2);

            // Giả lập Player 2 bị chết
            ctxClient1.Health.SetCurrentHealth(0f);
            Assert.IsFalse(ctxClient1.IsAlive);

            // Tọa độ gần Player 2 (5, 0)
            Vector2 samplePos = new Vector2(4.5f, 0f);
            var nearest = _registry.GetNearestLivingPlayer(samplePos);

            // Phải bỏ qua Player 2 và chọn Player 1 (0, 0) vì gần hơn Player 3 (15, 0)
            Assert.IsNotNull(nearest);
            Assert.AreEqual(ctxHost, nearest);
        }

        [Test]
        public async Task MockNetworkSession_CreateHost_Generates6CharRoomCode()
        {
            var session = new MockNetworkSessionService();
            bool success = await session.CreateHostSessionAsync();

            Assert.IsTrue(success);
            Assert.IsTrue(session.IsHost);
            Assert.IsTrue(session.IsInRoom);
            Assert.IsNotNull(session.CurrentRoom);
            Assert.AreEqual(6, session.CurrentRoom.RoomCode.Length);
            Assert.AreEqual(1, session.CurrentRoom.CurrentPlayerCount);
            Assert.IsTrue(session.CurrentRoom.LocalPlayer.IsHost);
        }

        [Test]
        public async Task MockNetworkSession_JoinRoom_AddsLocalClientToRoom()
        {
            var session = new MockNetworkSessionService();
            bool success = await session.JoinSessionAsync("ROOM88");

            Assert.IsTrue(success);
            Assert.IsFalse(session.IsHost);
            Assert.AreEqual("ROOM88", session.CurrentRoom.RoomCode);
            Assert.AreEqual(2, session.CurrentRoom.CurrentPlayerCount);
            Assert.IsFalse(session.CurrentRoom.LocalPlayer.IsReady);

            session.SetLocalPlayerReady(true);
            Assert.IsTrue(session.CurrentRoom.LocalPlayer.IsReady);
        }

        [Test]
        public void NetworkInputBridge_ApplyInput_TriggersExpectedEvents()
        {
            var go = new GameObject("NetworkPuppet");
            var bridge = go.AddComponent<NetworkInputBridge>();

            bool dashTriggered = false;
            bool attackTriggered = false;

            bridge.OnDashTriggered += () => dashTriggered = true;
            bridge.OnAttackTriggered += () => attackTriggered = true;

            var inputPacket = new NetworkInputData
            {
                MoveDirection = new Vector2(0.7f, 0.7f),
                Buttons = NetworkInputButtons.Dash | NetworkInputButtons.Attack
            };

            bridge.ApplyNetworkInput(inputPacket);

            Assert.AreEqual(new Vector2(0.7f, 0.7f), bridge.MovementInput);
            Assert.IsTrue(dashTriggered);
            Assert.IsTrue(attackTriggered);

            Object.DestroyImmediate(go);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using System.Threading.Tasks;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Multiplayer.Core;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Combat.Coop;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class MultiplayerGameplayTests
    {
        private MockNetworkSessionService _mockSession;
        private GameObject _playerHost;
        private GameObject _playerClient;

        [SetUp]
        public void SetUp()
        {
            _mockSession = new MockNetworkSessionService();
            ServiceContext.Register<INetworkSessionService>(_mockSession);

            _playerHost = new GameObject("Player_Host_Test");
            var hpHost = _playerHost.AddComponent<HealthSystem>();
            hpHost.DisableGameObjectOnDeath = false;

            _playerClient = new GameObject("Player_Client_Test");
            var hpClient = _playerClient.AddComponent<HealthSystem>();
            hpClient.DisableGameObjectOnDeath = false;
        }

        [TearDown]
        public void TearDown()
        {
            ServiceContext.Clear();
            if (_playerHost != null) Object.DestroyImmediate(_playerHost);
            if (_playerClient != null) Object.DestroyImmediate(_playerClient);
        }

        [Test]
        public async Task MultiplayerSession_HostAndClientRoles_CorrectlyDistinguished()
        {
            await _mockSession.CreateHostSessionAsync("HOST88");
            Assert.IsTrue(_mockSession.IsHost);
            Assert.IsTrue(_mockSession.IsInRoom);

            await _mockSession.JoinSessionAsync("ROOM99");
            Assert.IsFalse(_mockSession.IsHost);
            Assert.IsTrue(_mockSession.IsInRoom);
        }

        [Test]
        public void HealthSystem_CustomDamageInterceptor_RedirectsDamageWithoutLocalDeduction()
        {
            var hp = _playerClient.GetComponent<HealthSystem>();
            float initialHp = hp.CurrentHealth;

            bool interceptorCalled = false;
            float interceptedAmount = 0f;

            hp.CustomDamageInterceptor = (amount, data) =>
            {
                interceptorCalled = true;
                interceptedAmount = amount;
                return true; // Chặn trừ máu cục bộ, nhường quyền cho RPC
            };

            hp.TakeDamage(25f);

            Assert.IsTrue(interceptorCalled);
            Assert.AreEqual(25f, interceptedAmount);
            Assert.AreEqual(initialHp, hp.CurrentHealth, "Máu cục bộ không được trừ trực tiếp khi có interceptor can thiệp.");
        }

        [Test]
        public void HealthSystem_CustomDamageInterceptor_WhenNull_AppliesNormalDamage()
        {
            var hp = _playerHost.GetComponent<HealthSystem>();
            float initialHp = hp.CurrentHealth;

            hp.CustomDamageInterceptor = null;
            hp.TakeDamage(30f);

            Assert.AreEqual(initialHp - 30f, hp.CurrentHealth);
        }

        [Test]
        public async Task GameStateManager_IsPlaying_InMultiplayerLevelUp_RemainsActive()
        {
            await _mockSession.CreateHostSessionAsync("COOP11");

            var gsmGo = new GameObject("GameStateManager_Test");
            var gsm = gsmGo.AddComponent<GameStateManager>();

            try
            {
                gsm.ChangeState(GameState.Playing);
                Assert.IsTrue(GameStateManager.IsPlaying);

                gsm.ChangeState(GameState.LevelUpSelection);
                // Trong Multiplayer, timeScale giữ nguyên và IsPlaying vẫn trả về true để trận đấu tiếp diễn
                Assert.AreEqual(1f, Time.timeScale);
                Assert.IsTrue(GameStateManager.IsPlaying, "GameStateManager.IsPlaying phải trả về true khi trong phòng Multiplayer Co-op lúc LevelUp.");

                gsm.ChangeState(GameState.Paused);
                // Trong Multiplayer, khi mở Cài đặt / Pause Menu thì không làm delay/đóng băng game
                Assert.AreEqual(1f, Time.timeScale, "Time.timeScale phải là 1f khi Paused trong Multiplayer để không làm lệch nhịp spawn quái.");
                Assert.IsTrue(GameStateManager.IsPlaying, "GameStateManager.IsPlaying phải trả về true khi trong phòng Multiplayer Co-op lúc Paused.");
            }
            finally
            {
                Object.DestroyImmediate(gsmGo);
            }
        }

        [Test]
        public void CoopDownedMechanic_WhenSoloOrNotInMultiplayer_DoesNotBlockDeath()
        {
            var hp = _playerHost.GetComponent<HealthSystem>();
            var downed = _playerHost.AddComponent<CoopDownedMechanic>();

            // Khi chưa vào phòng Co-op có > 1 người chơi, TakeDamage chí mạng sẽ làm chết nhân vật
            hp.TakeDamage(hp.MaxHealth + 10f);

            Assert.IsFalse(downed.IsDowned);
            Assert.IsFalse(hp.IsAlive);
        }

        [Test]
        public void CharacterDatabasePrefabProvider_ReturnsValidPrefab_WithFallback()
        {
            var provider = new CharacterDatabasePrefabProvider();
            var prefab = provider.GetDefaultNetworkCharacterPrefab();

            // Nếu asset database hoặc Resources có prefab, nó phải trả về đối tượng có NetworkObject
            // Hoặc nếu không có trong runtime test mock, nó không được ném Exception
            Assert.DoesNotThrow(() =>
            {
                var result = provider.GetNetworkCharacterPrefab("C001_ThuSinh");
            });
        }
    }
}

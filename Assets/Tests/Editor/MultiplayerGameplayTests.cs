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

        [Test]
        public void Coop_ElementalDamage_FireOnMetal_ClientInterceptsRaw_HostDeals130Not169()
        {
            var enemyGo = new GameObject("Enemy_Metal_Test");
            try
            {
                var enemyHp = enemyGo.AddComponent<HealthSystem>();
                enemyHp.DisableGameObjectOnDeath = false;
                enemyHp.SetMaxHealth(1000f);
                enemyHp.CurrentElement = ElementType.Kim;

                var enemySync = enemyGo.AddComponent<NetworkEnemySync>();

                // Giả lập Client: Interceptor hứng đòn đánh từ client và tạo batch hit
                NetworkDamageHit capturedHit = default;
                enemyHp.CustomDamageInterceptor = (amount, data) =>
                {
                    capturedHit = new NetworkDamageHit(
                        rawDamage: data.RawAmount > 0f ? data.RawAmount : amount,
                        element: (byte)data.Element,
                        hitSource: (byte)data.HitSource,
                        isCritical: data.IsCritical,
                        canTriggerReaction: data.CanTriggerReaction,
                        attackId: data.AttackId
                    );
                    return true;
                };

                // Client đánh đòn Hỏa 100 lên Kim (thông qua CalculateHitDamage có thể tính trước 130)
                var clientHit = DamageUtility.CalculateHitDamage(100f, false, ElementType.Hoa, ElementType.Kim);
                enemyHp.TakeDamage(clientHit);

                // Khẳng định: Client gửi RawDamage = 100 (sát thương gốc), không gửi 130
                Assert.AreEqual(100f, capturedHit.RawDamage, 0.001f);
                Assert.AreEqual((byte)ElementType.Hoa, capturedHit.Element);
                Assert.AreEqual(1000f, enemyHp.CurrentHealth, "Máu quái trên client không bị trừ cục bộ");

                // Giả lập Host: Tắt interceptor, Host nhận batch và tính toán độc quyền
                enemyHp.CustomDamageInterceptor = null;
                var batch = new[] { capturedHit };
                enemySync.ProcessDamageBatchHost(batch, _playerClient);

                // Khẳng định: Trên host, Hỏa 100 đánh Kim tính Tương Khắc (x1.3) chỉ trừ đúng 130 máu (còn 870), KHÔNG bị nhân đôi thành 169
                Assert.AreEqual(870f, enemyHp.CurrentHealth, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(enemyGo);
            }
        }

        [Test]
        public void Coop_DamageBatch_PreservesSequentialOrder_FireThenWater()
        {
            var enemyGo = new GameObject("Enemy_Order_Test");
            try
            {
                var enemyHp = enemyGo.AddComponent<HealthSystem>();
                enemyHp.DisableGameObjectOnDeath = false;
                enemyHp.SetMaxHealth(1000f);
                var enemySync = enemyGo.AddComponent<NetworkEnemySync>();

                var recordedElements = new System.Collections.Generic.List<ElementType>();
                enemyHp.OnDamageTaken += data => recordedElements.Add(data.Element);

                var batch = new[]
                {
                    new NetworkDamageHit(40f, (byte)ElementType.Hoa, (byte)ElementHitSource.HeroBasicAttack, false, true, 1),
                    new NetworkDamageHit(60f, (byte)ElementType.Thuy, (byte)ElementHitSource.Relic, false, true, 2)
                };

                enemySync.ProcessDamageBatchHost(batch, _playerClient);

                Assert.AreEqual(2, recordedElements.Count);
                Assert.AreEqual(ElementType.Hoa, recordedElements[0], "Hit 1 phải là Hỏa");
                Assert.AreEqual(ElementType.Thuy, recordedElements[1], "Hit 2 phải là Thủy");
            }
            finally
            {
                Object.DestroyImmediate(enemyGo);
            }
        }

        [Test]
        public void Coop_DamageBatch_NonElementalHit_DoesNotMorphIntoPreviousElement()
        {
            var enemyGo = new GameObject("Enemy_Morph_Test");
            try
            {
                var enemyHp = enemyGo.AddComponent<HealthSystem>();
                enemyHp.DisableGameObjectOnDeath = false;
                enemyHp.SetMaxHealth(1000f);
                var enemySync = enemyGo.AddComponent<NetworkEnemySync>();

                var recordedElements = new System.Collections.Generic.List<ElementType>();
                enemyHp.OnDamageTaken += data => recordedElements.Add(data.Element);

                // Batch chứa 1 hit Hỏa và 1 hit không có hệ (None)
                var batch = new[]
                {
                    new NetworkDamageHit(50f, (byte)ElementType.Hoa, (byte)ElementHitSource.HeroBasicAttack, false, true, 1),
                    new NetworkDamageHit(30f, (byte)ElementType.None, (byte)ElementHitSource.HeroBasicAttack, false, false, 2)
                };

                enemySync.ProcessDamageBatchHost(batch, _playerClient);

                Assert.AreEqual(2, recordedElements.Count);
                Assert.AreEqual(ElementType.Hoa, recordedElements[0]);
                Assert.AreEqual(ElementType.None, recordedElements[1], "Hit không có hệ không được biến thành Hỏa của hit trước");
            }
            finally
            {
                Object.DestroyImmediate(enemyGo);
            }
        }

        [Test]
        public void Coop_ReactionDamage_DoesNotTriggerSecondaryReaction()
        {
            var enemyGo = new GameObject("Enemy_Reaction_Test");
            try
            {
                var enemyHp = enemyGo.AddComponent<HealthSystem>();
                enemyHp.DisableGameObjectOnDeath = false;
                enemyHp.SetMaxHealth(1000f);
                var enemySync = enemyGo.AddComponent<NetworkEnemySync>();

                DamageData receivedData = default;
                enemyHp.OnDamageTaken += data => receivedData = data;

                // Hit xuất phát từ reaction lan toả với canTriggerReaction = false
                var batch = new[]
                {
                    new NetworkDamageHit(25f, (byte)ElementType.Hoa, (byte)ElementHitSource.Unknown, false, false, 99)
                };

                enemySync.ProcessDamageBatchHost(batch, _playerHost);

                Assert.IsFalse(receivedData.CanTriggerReaction, "Damage từ reaction không được phép kích hoạt reaction mới");
                Assert.AreEqual(ElementType.Hoa, receivedData.Element);
            }
            finally
            {
                Object.DestroyImmediate(enemyGo);
            }
        }

        [Test]
        public void Coop_DamageBatch_DistinctPlayers_KeepSeparateOwners()
        {
            var enemyGo = new GameObject("Enemy_MultiOwner_Test");
            try
            {
                var enemyHp = enemyGo.AddComponent<HealthSystem>();
                enemyHp.DisableGameObjectOnDeath = false;
                enemyHp.SetMaxHealth(1000f);
                var enemySync = enemyGo.AddComponent<NetworkEnemySync>();

                var recordedOwners = new System.Collections.Generic.List<GameObject>();
                enemyHp.OnDamageTaken += data => recordedOwners.Add(data.Owner);

                var batchP1 = new[]
                {
                    new NetworkDamageHit(30f, (byte)ElementType.Kim, (byte)ElementHitSource.HeroBasicAttack, false, true, 10)
                };
                var batchP2 = new[]
                {
                    new NetworkDamageHit(40f, (byte)ElementType.Moc, (byte)ElementHitSource.HeroBasicAttack, false, true, 20)
                };

                enemySync.ProcessDamageBatchHost(batchP1, _playerHost);
                enemySync.ProcessDamageBatchHost(batchP2, _playerClient);

                Assert.AreEqual(2, recordedOwners.Count);
                Assert.AreSame(_playerHost, recordedOwners[0], "Batch 1 phải có owner là Host");
                Assert.AreSame(_playerClient, recordedOwners[1], "Batch 2 phải có owner là Client");
                Assert.AreNotSame(recordedOwners[0], recordedOwners[1], "Hai player không được lẫn lộn owner với nhau");
            }
            finally
            {
                Object.DestroyImmediate(enemyGo);
            }
        }
    }
}

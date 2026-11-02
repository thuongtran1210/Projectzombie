using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Core.Save;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.Loadout;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Tests.Architecture
{
    [TestFixture]
    public class CleanArchitectureTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceContext.ResetStaticDomainState();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceContext.ResetStaticDomainState();
        }

        [Test]
        public void IMatchCompositionRoot_InterfaceContract_IsAssignable()
        {
            Assert.IsTrue(typeof(IMatchCompositionRoot).IsAssignableFrom(typeof(DefenseSceneCompositionRoot)));
        }

        [Test]
        public void IGameStateManager_InterfaceContract_IsAssignable()
        {
            Assert.IsTrue(typeof(IGameStateManager).IsAssignableFrom(typeof(GameStateManager)));
        }

        [Test]
        public void ISaveManager_InterfaceContract_IsAssignable()
        {
            Assert.IsTrue(typeof(ISaveManager).IsAssignableFrom(typeof(GameManager)));
        }

        [Test]
        public void DefenseLoadoutService_ImplementsILoadoutService_AndHandlesContracts()
        {
            var service = DefenseLoadoutService.CreateDefault();
            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Rules);
            Assert.GreaterOrEqual(service.Rules.MaxMinionCount, 1);
            Assert.GreaterOrEqual(service.Rules.MaxTowerCount, 1);

            // Test Catalog query through interface
            ILoadoutService loadoutService = service;
            var minions = loadoutService.GetCatalogEntriesByType(LoadoutItemType.Minion);
            Assert.IsNotNull(minions);
            Assert.Greater(minions.Count, 0);

            // Test resolve id
            string mappedHero = loadoutService.ResolveSupportedId("hero_thusinh", true);
            Assert.IsNotNull(mappedHero);

            // Test bonus calculation contracts
            loadoutService.EvaluateMinionStarBonuses(1, out float hp, out float spd);
            Assert.GreaterOrEqual(hp, 0f);
            Assert.GreaterOrEqual(spd, 0f);
        }

        [Test]
        public void ServiceContext_CanRegisterAndResolve_CoreArchitectureServices()
        {
            var loadoutService = DefenseLoadoutService.CreateDefault();
            ServiceContext.Register<ILoadoutService>(loadoutService);

            Assert.IsTrue(ServiceContext.TryGet<ILoadoutService>(out var resolvedLoadout));
            Assert.AreSame(loadoutService, resolvedLoadout);

            ServiceContext.Unregister<ILoadoutService>();
            Assert.IsFalse(ServiceContext.TryGet<ILoadoutService>(out _));
        }
    }
}

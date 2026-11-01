using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using Object = UnityEngine.Object;

/// <summary>Checks independent identities, transactional recruitment and data-driven UI without a combat scene.</summary>
public sealed class MinionRosterRuntimeTests
{
    private readonly List<Object> _objects = new List<Object>();
    private static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    private DefenseMinionDefinitionSO Definition(string id, DefenseMinionRole role = DefenseMinionRole.Militia)
    {
        var definition = ScriptableObject.CreateInstance<DefenseMinionDefinitionSO>(); _objects.Add(definition);
        Set(definition, "_id", id); Set(definition, "_woodCost", 40); Set(definition, "_capacity", 12); Set(definition, "_role", role);
        return definition;
    }
    [TearDown]
    public void Dispose()
    {
        foreach (var item in _objects) if (item != null) Object.DestroyImmediate(item);
        _objects.Clear();
    }

    [Test]
    public void SameBehaviorTypesRemainSeparateAndCommandsAreIndependent()
    {
        var militia = Definition("militia"); var archer = Definition("archer");
        var roster = new MinionRosterRuntime(); roster.Select(new[] { "militia", "archer" }, new[] { militia, archer });
        Assert.AreEqual(2, roster.Squads.Count);
        Assert.IsTrue(roster.CycleOrder("archer"));
        Assert.AreEqual(DefenseMinionOrder.Defend, roster.Squads[0].Order);
        Assert.AreEqual(DefenseMinionOrder.Follow, roster.Squads[1].Order);
        roster.ResetOrders(); Assert.AreEqual(DefenseMinionOrder.Defend, roster.Squads[1].Order);
        Assert.IsFalse(roster.CycleOrder("unequipped"));
    }

    [Test]
    public void FifthTypeRequiresOnlyCatalogDataAndSelection()
    {
        var definitions = new[] { Definition("a"), Definition("b"), Definition("c"), Definition("d"), Definition("e") };
        var roster = new MinionRosterRuntime(); roster.Select(new[] { "a", "b", "c", "d", "e" }, definitions);
        Assert.AreEqual(5, roster.Squads.Count);
        Assert.IsTrue(roster.TryGetSquad("E", out var fifth)); Assert.AreSame(definitions[4], fifth.Definition);
    }

    [Test]
    public void InvalidSelectionDoesNotReplaceCurrentRoster()
    {
        var definition = Definition("a"); var roster = new MinionRosterRuntime();
        roster.Select(new[] { "a" }, new[] { definition });
        int revision = roster.Revision;
        Assert.Throws<ArgumentException>(() => roster.Select(new[] { "missing" }, new[] { definition }));
        Assert.Throws<ArgumentException>(() => roster.Select(new[] { "a", "A" }, new[] { definition }));
        Assert.Throws<ArgumentException>(() => roster.Select(new[] { "a" }, new[] { definition, definition }));
        Assert.AreEqual(revision, roster.Revision); Assert.AreEqual("a", roster.Squads[0].Id);
    }

    [TestCase(false, 100, 0, MinionRecruitResult.Unavailable)]
    [TestCase(true, 20, 0, MinionRecruitResult.InsufficientWood)]
    [TestCase(true, 100, 12, MinionRecruitResult.Full)]
    public void RejectedRecruitmentDoesNotDeployOrSpend(bool available, int wood, int count, MinionRecruitResult expected)
    {
        var roster = Roster(); var gate = new Gate { CanRecruit = available }; var wallet = new Wallet { Wood = wood }; var pool = new Pool { Active = count };
        var service = new MinionRecruitmentService(roster, gate, wallet, pool);
        Assert.AreEqual(expected, service.TryRecruit("a")); Assert.AreEqual(0, pool.DeployCalls); Assert.AreEqual(wood, wallet.Wood);
        Assert.AreEqual(MinionRecruitResult.NotEquipped, service.TryRecruit("other"));
    }

    [Test]
    public void DeploymentFailureDoesNotChargeWood()
    {
        var wallet = new Wallet(); var pool = new Pool { DeploySucceeds = false };
        var service = new MinionRecruitmentService(Roster(), new Gate(), wallet, pool);
        Assert.AreEqual(MinionRecruitResult.DeploymentFailed, service.TryRecruit("a")); Assert.AreEqual(100, wallet.Wood);
    }

    [Test]
    public void SessionChangingDuringDeploymentReturnsTheActorWithoutCharge()
    {
        var gate = new Gate(); var wallet = new Wallet(); var pool = new Pool(); pool.DuringDeploy = () => gate.RunGeneration++;
        var service = new MinionRecruitmentService(Roster(), gate, wallet, pool);
        Assert.AreEqual(MinionRecruitResult.StaleSession, service.TryRecruit("a"));
        Assert.AreEqual(100, wallet.Wood); Assert.AreEqual(0, pool.Active); Assert.AreEqual(1, pool.Returns);
    }

    [Test]
    public void FailedPaymentReturnsTheActor()
    {
        var wallet = new Wallet { AcceptsPayment = false }; var pool = new Pool();
        var service = new MinionRecruitmentService(Roster(), new Gate(), wallet, pool);
        Assert.AreEqual(MinionRecruitResult.InsufficientWood, service.TryRecruit("a")); Assert.AreEqual(0, pool.Active);
    }

    [Test]
    public void SuccessfulRecruitmentSpendsOnceAndBlocksReentrantPurchase()
    {
        var wallet = new Wallet(); var pool = new Pool();
        var service = new MinionRecruitmentService(Roster(), new Gate(), wallet, pool);
        wallet.DuringSpend = () => Assert.AreEqual(MinionRecruitResult.Unavailable, service.TryRecruit("a"));
        Assert.AreEqual(MinionRecruitResult.Success, service.TryRecruit("a"));
        Assert.AreEqual(60, wallet.Wood); Assert.AreEqual(1, pool.Active); Assert.AreEqual(0, pool.Returns);
    }

    [Test]
    public void DeploymentExceptionReturnsLeaseAndDoesNotLockTheService()
    {
        var wallet = new Wallet(); var pool = new Pool();
        pool.DuringDeploy = () => throw new InvalidOperationException("Simulated deployment callback failure.");
        var service = new MinionRecruitmentService(Roster(), new Gate(), wallet, pool);
        Assert.Throws<InvalidOperationException>(() => service.TryRecruit("a"));
        Assert.AreEqual(0, pool.Active); Assert.AreEqual(100, wallet.Wood);
        pool.DuringDeploy = null;
        Assert.AreEqual(MinionRecruitResult.Success, service.TryRecruit("a"));
    }

    [Test]
    public void UiCanBindFiveTypesThenRebindOneWithoutLeavingOldRows()
    {
        var actor = new GameObject("RosterUiFixture"); _objects.Add(actor);
        var view = actor.AddComponent<DefenseVillageShopView>();
        Set(view, "_recruitTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(DefenseMinionRosterMigration.FOLDER + "MinionRecruitRow.prefab").GetComponent<DefenseMinionSquadRowView>());
        Set(view, "_commandTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(DefenseMinionRosterMigration.FOLDER + "MinionCommandRow.prefab").GetComponent<DefenseMinionSquadRowView>());
        var recruit = new GameObject("Recruit", typeof(RectTransform)); recruit.transform.SetParent(actor.transform);
        var command = new GameObject("Command", typeof(RectTransform)); command.transform.SetParent(actor.transform);
        Set(view, "_recruitContent", recruit.GetComponent<RectTransform>()); Set(view, "_commandContent", command.GetComponent<RectTransform>());
        view.BindSquads(new[] { "a", "b", "c", "d", "e" }); Assert.AreEqual(5, view.SquadRowCount);
        view.BindSquads(new[] { "e" }); Assert.AreEqual(1, view.SquadRowCount);
        int active = 0;
        foreach (var row in actor.GetComponentsInChildren<DefenseMinionSquadRowView>(true)) if (row.gameObject.activeSelf) { active++; Assert.AreEqual("e", row.Id); }
        Assert.AreEqual(2, active);
    }

    [Test]
    public void AuthoredCatalogHasUniqueIdsIconsAndMatchingPrefabDefinitions()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<DefenseMinionCatalogSO>(DefenseMinionRosterMigration.CATALOG);
        Assert.NotNull(catalog); Assert.GreaterOrEqual(catalog.Definitions.Count, 4);
        var ids = new HashSet<string>();
        foreach (var definition in catalog.Definitions)
        {
            Assert.IsTrue(ids.Add(definition.Id)); Assert.NotNull(definition.Icon); Assert.NotNull(definition.Prefab);
            Assert.AreSame(definition, definition.Prefab.Definition); Assert.Greater(definition.Capacity, 0);
        }
    }

    private MinionRosterRuntime Roster()
    {
        var roster = new MinionRosterRuntime(); roster.Select(new[] { "a" }, new[] { Definition("a") }); return roster;
    }
    private sealed class Gate : IMinionRecruitContext
    {
        public bool CanRecruit { get; set; } = true;
        public object SessionIdentity { get; } = new object();
        public int RunGeneration { get; set; } = 1;
    }
    private sealed class Wallet : IMinionRecruitWallet
    {
        public int Wood { get; set; } = 100; public bool AcceptsPayment = true; public Action DuringSpend;
        public bool TrySpend(int amount) { DuringSpend?.Invoke(); if (!AcceptsPayment || Wood < amount) return false; Wood -= amount; return true; }
    }
    private sealed class Pool : IMinionRecruitPool, IMinionRecruitLease
    {
        public int Active, Returns, DeployCalls; public bool DeploySucceeds = true; public Action DuringDeploy;
        public int Count(string id) => Active;
        public int Capacity(string id) => 12;
        public bool TryDeploy(string id, out IMinionRecruitLease lease)
        {
            DeployCalls++; lease = null; if (!DeploySucceeds) return false; Active++; lease = this; DuringDeploy?.Invoke(); return true;
        }
        public void ReleaseRecruit() { Active--; Returns++; }
    }
}

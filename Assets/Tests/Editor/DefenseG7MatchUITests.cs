using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using UnityEngine;

public sealed class DefenseG7MatchUITests
{
    private GameObject _testRoot;
    private DefenseMatchHUDView _hudView;
    private DefenseUpgradeModalView _upgradeModalView;
    private DefenseMatchResultModalView _resultModalView;
    private DefenseMatchPresenter _presenter;

    [SetUp]
    public void SetUp()
    {
        _testRoot = new GameObject("DefenseG7_TestRoot");
        _hudView = _testRoot.AddComponent<DefenseMatchHUDView>();
        _upgradeModalView = _testRoot.AddComponent<DefenseUpgradeModalView>();
        _resultModalView = _testRoot.AddComponent<DefenseMatchResultModalView>();
        _presenter = _testRoot.AddComponent<DefenseMatchPresenter>();

        // EditMode AddComponent does not invoke MonoBehaviour.Awake. Exercise the
        // presenter's real initialization before asserting its model contracts.
        var awake = typeof(DefenseMatchPresenter).GetMethod("Awake",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(awake, "Presenter must initialize its models in Awake.");
        awake.Invoke(_presenter, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (_testRoot != null)
            Object.DestroyImmediate(_testRoot);
    }

    [Test]
    public void Presenter_InitializesWithExperienceCoreAndUpgradeDeck()
    {
        Assert.IsNotNull(_presenter.ExperienceCore);
        Assert.IsNotNull(_presenter.UpgradeDeck);
        Assert.AreEqual(1, _presenter.ExperienceCore.CurrentLevel);
        Assert.AreEqual(0, _presenter.ExperienceCore.CurrentExp);
    }

    [Test]
    public void UpgradeDeck_DrawThreeChoices_AlwaysReturnsThreeUniqueValidCards()
    {
        var deck = _presenter.UpgradeDeck;
        var choices = deck.DrawThreeChoices();

        Assert.AreEqual(3, choices.Count);
        var seenIds = new HashSet<string>();
        foreach (var card in choices)
        {
            Assert.IsFalse(string.IsNullOrEmpty(card.CardId));
            Assert.IsFalse(string.IsNullOrEmpty(card.Title));
            Assert.IsFalse(seenIds.Contains(card.CardId), $"Duplicate card drawn: {card.CardId}");
            seenIds.Add(card.CardId);
        }
    }

    [Test]
    public void ExperienceCore_LevelUpTriggersLevelGainedAndPendingQueue()
    {
        var expCore = _presenter.ExperienceCore;
        int levelGainedCount = 0;
        expCore.LevelGained += lvl => levelGainedCount++;

        // Required for Lv1 -> 2 is 20*1 + 10 = 30 XP
        expCore.AddExp(30);
        Assert.AreEqual(1, levelGainedCount);
        Assert.AreEqual(1, expCore.PendingLevelUps);

        // Required for Lv2 -> 3 is 20*2 + 10 = 50 XP
        // Add 55 XP -> consumes 50, leaves 5 surplus
        expCore.AddExp(55);
        Assert.AreEqual(2, levelGainedCount);
        Assert.AreEqual(2, expCore.PendingLevelUps);
        Assert.AreEqual(5, expCore.CurrentExp);

        // Consuming pending levels
        Assert.IsTrue(expCore.TryConsumePendingLevelUp());
        Assert.AreEqual(2, expCore.CurrentLevel);
        Assert.AreEqual(1, expCore.PendingLevelUps);

        Assert.IsTrue(expCore.TryConsumePendingLevelUp());
        Assert.AreEqual(3, expCore.CurrentLevel);
        Assert.AreEqual(0, expCore.PendingLevelUps);

        Assert.IsFalse(expCore.TryConsumePendingLevelUp());
    }

    [Test]
    public void UpgradeDeck_SelectingCard_AppliesModifierCorrectly()
    {
        var deck = _presenter.UpgradeDeck;
        var testCard = new DefenseUpgradeCard("c1", "Test", "Desc", DefenseUpgradeType.HeroAttackDamage, 0.25f);

        Assert.AreEqual(0f, deck.GetModifier(DefenseUpgradeType.HeroAttackDamage));
        Assert.IsTrue(deck.SelectUpgrade(testCard));
        Assert.AreEqual(0.25f, deck.GetModifier(DefenseUpgradeType.HeroAttackDamage));

        // Stack another upgrade
        var testCard2 = new DefenseUpgradeCard("c2", "Test2", "Desc", DefenseUpgradeType.HeroAttackDamage, 0.15f);
        Assert.IsTrue(deck.SelectUpgrade(testCard2));
        Assert.AreEqual(0.40f, deck.GetModifier(DefenseUpgradeType.HeroAttackDamage));
    }

    [Test]
    public void Reset_RestoresPresenterSubsystemsToInitialState()
    {
        var expCore = _presenter.ExperienceCore;
        var deck = _presenter.UpgradeDeck;

        expCore.AddExp(100);
        deck.SelectUpgrade(new DefenseUpgradeCard("c1", "T", "D", DefenseUpgradeType.HeroMoveSpeed, 0.10f));

        Assert.Greater(expCore.PendingLevelUps, 0);
        Assert.Greater(deck.GetModifier(DefenseUpgradeType.HeroMoveSpeed), 0f);

        expCore.Reset();
        deck.Reset();

        Assert.AreEqual(1, expCore.CurrentLevel);
        Assert.AreEqual(0, expCore.CurrentExp);
        Assert.AreEqual(0, expCore.PendingLevelUps);
        Assert.AreEqual(0f, deck.GetModifier(DefenseUpgradeType.HeroMoveSpeed));
    }
}

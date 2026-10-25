using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseG5RogueliteTests
{
    [Test]
    public void ExperienceCore_CarriesOverExcessExp_AndQueuesLevels()
    {
        var expCore = new DefenseExperienceCore(startingLevel: 1);
        Assert.AreEqual(1, expCore.CurrentLevel);
        Assert.AreEqual(0, expCore.PendingLevelUps);

        // Level 1 requires 20 * 1 + 10 = 30 XP
        // Add 75 XP -> should complete Lv1 (cost 30, remaining 45), Lv2 (cost 50, need 5 more, so remaining 45) -> 1 level gained
        expCore.AddExp(75);
        Assert.AreEqual(1, expCore.PendingLevelUps);
        Assert.AreEqual(45, expCore.CurrentExp);

        // Consume level up
        Assert.IsTrue(expCore.TryConsumePendingLevelUp());
        Assert.AreEqual(2, expCore.CurrentLevel);
        Assert.AreEqual(0, expCore.PendingLevelUps);

        // Add 60 XP -> CurrentExp = 45 + 60 = 105; Lv2 requires 50 -> remaining 55; Lv3 requires 70 -> remaining 55 < 70 -> 1 level up
        expCore.AddExp(60);
        Assert.AreEqual(1, expCore.PendingLevelUps);
        Assert.AreEqual(55, expCore.CurrentExp);
    }

    [Test]
    public void UpgradeDeck_DrawsThreeUniqueCards_AndAppliesModifiers()
    {
        var deck = new DefenseUpgradeDeck();
        var cards = deck.DrawThreeCards(seed: 42);

        Assert.AreEqual(3, cards.Count);
        Assert.AreNotEqual(cards[0].CardId, cards[1].CardId);
        Assert.AreNotEqual(cards[1].CardId, cards[2].CardId);
        Assert.AreNotEqual(cards[0].CardId, cards[2].CardId);

        // Apply first card
        var card = cards[0];
        float prevMod = deck.GetModifier(card.UpgradeType);
        Assert.IsTrue(deck.TryApplyCard(card));
        Assert.AreEqual(prevMod + card.Value, deck.GetModifier(card.UpgradeType));
    }
}

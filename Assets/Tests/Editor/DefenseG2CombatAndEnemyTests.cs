using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseG2CombatAndEnemyTests
{
    [Test]
    public void EnemyRegistry_ContainsAllSevenArchetypes()
    {
        string[] expected = { "magiap", "thuyquai", "huyetdoi", "quaidu", "phicuong", "magiap_boss", "yaksha_boss" };
        foreach (var id in expected)
        {
            Assert.IsTrue(DefenseEnemyRegistry.TryGetStats(id, out var stats), $"Missing archetype: {id}");
            Assert.AreEqual(id, stats.EnemyId);
            Assert.Greater(stats.MaxHp, 0f);
            Assert.Greater(stats.Speed, 0f);
            Assert.Greater(stats.Damage, 0f);
        }
    }

    [Test]
    public void HealthCore_ClampsHP_AndTriggersDefeatExactlyOnce()
    {
        var health = new DefenseHealthCore(100f);
        int defeatCount = 0;
        health.Defeated += () => defeatCount++;

        // Invalid damages
        Assert.IsFalse(health.ApplyDamage(new DefenseDamageContext(0f)));
        Assert.IsFalse(health.ApplyDamage(new DefenseDamageContext(-50f)));
        Assert.AreEqual(100f, health.CurrentHp);

        // Valid damage
        Assert.IsTrue(health.ApplyDamage(new DefenseDamageContext(60f)));
        Assert.AreEqual(40f, health.CurrentHp);
        Assert.AreEqual(0, defeatCount);

        // Overkill damage
        Assert.IsTrue(health.ApplyDamage(new DefenseDamageContext(100f)));
        Assert.AreEqual(0f, health.CurrentHp);
        Assert.IsTrue(health.IsDead);
        Assert.AreEqual(1, defeatCount);

        // Same-frame simultaneous secondary damage hit
        Assert.IsFalse(health.ApplyDamage(new DefenseDamageContext(50f)));
        Assert.AreEqual(0f, health.CurrentHp);
        Assert.AreEqual(1, defeatCount); // Must still be exactly 1
    }

    [Test]
    public void RewardPipeline_GrantsRewardsOnce()
    {
        var pipeline = new DefenseRewardPipeline();
        int grantCount = 0;
        pipeline.RewardGranted += drop => grantCount++;

        var drop = new DefenseRewardDrop(10, 5, UnityEngine.Vector3.zero, "magiap");
        Assert.IsTrue(pipeline.TryGrantReward(drop));
        Assert.IsFalse(pipeline.TryGrantReward(drop));
        Assert.AreEqual(1, grantCount);
    }
}

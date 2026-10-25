using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using UnityEngine;

public sealed class DefenseG4DefenseAndHeroTests
{
    [Test]
    public void TowerSocket_BuildUpgradeRepairLifecycle()
    {
        var socket = new DefenseTowerSocketCore(0);
        Assert.IsFalse(socket.IsOccupied);
        Assert.AreEqual(0, socket.Level);

        // Cannot build without enough wood
        Assert.IsFalse(socket.TryBuild(20, out int spent));
        Assert.AreEqual(0, spent);

        // Build Level 1 with 25 wood
        Assert.IsTrue(socket.TryBuild(25, out spent));
        Assert.AreEqual(25, spent);
        Assert.AreEqual(1, socket.Level);
        Assert.AreEqual(150f, socket.Health.MaxHp);

        // Upgrade to Level 2 (needs 40 wood)
        Assert.IsTrue(socket.TryUpgrade(50, out spent));
        Assert.AreEqual(40, spent);
        Assert.AreEqual(2, socket.Level);
        Assert.AreEqual(300f, socket.Health.MaxHp);

        // Take damage and repair
        socket.Health.ApplyDamage(new DefenseDamageContext(100f));
        Assert.AreEqual(200f, socket.Health.CurrentHp);

        Assert.IsTrue(socket.TryRepair(20, out spent));
        Assert.AreEqual(15, spent);
        Assert.AreEqual(300f, socket.Health.CurrentHp);

        // Upgrade to Max Level 3 (needs 60 wood)
        Assert.IsTrue(socket.TryUpgrade(60, out spent));
        Assert.AreEqual(60, spent);
        Assert.AreEqual(3, socket.Level);

        // Cannot upgrade beyond Level 3
        Assert.IsFalse(socket.TryUpgrade(100, out spent));
    }

    [Test]
    public void HeroCombatState_ControlsAndCooldowns()
    {
        var hero = new DefenseHeroCombatState(DefenseHeroArchetype.ThuSinh);

        Assert.IsTrue(hero.CanAttack(isDown: false, isModal: false));
        Assert.IsTrue(hero.TryTriggerAttack(isDown: false, isModal: false));
        Assert.IsFalse(hero.CanAttack(isDown: false, isModal: false)); // On cooldown

        // Cannot attack or dash while Down
        Assert.IsFalse(hero.CanAttack(isDown: true, isModal: false));
        Assert.IsFalse(hero.CanDash(isDown: true, isModal: false));

        // Advance cooldown
        hero.Tick(hero.AttackCooldown);
        Assert.IsTrue(hero.CanAttack(isDown: false, isModal: false));

        // Dash trigger
        Assert.IsTrue(hero.TryTriggerDash(isDown: false, isModal: false));
        Assert.IsTrue(hero.IsDashing);
        hero.Tick(hero.DashDuration);
        Assert.IsFalse(hero.IsDashing);
    }

    [Test]
    public void DepToOngRelic_AimCastCooldownCancelFlow()
    {
        var relic = new DefenseDepToOngRelic();
        Assert.AreEqual(DefenseRelicState.Ready, relic.State);

        // Cannot aim if Down or Modal
        Assert.IsFalse(relic.TryStartAim(isDown: true, isModal: false));
        Assert.IsFalse(relic.TryStartAim(isDown: false, isModal: true));

        // Start aim
        Assert.IsTrue(relic.TryStartAim(isDown: false, isModal: false));
        Assert.AreEqual(DefenseRelicState.Aiming, relic.State);

        // Cancel aim does NOT trigger cooldown
        relic.CancelAim();
        Assert.AreEqual(DefenseRelicState.Ready, relic.State);
        Assert.AreEqual(0f, relic.CooldownRemaining);

        // Aim again and Cast
        relic.TryStartAim(false, false);
        Vector3 target = new Vector3(5f, 0f, 2f);
        bool castTriggered = false;
        relic.CastTriggered += pt =>
        {
            castTriggered = true;
            Assert.AreEqual(target, pt);
        };

        Assert.IsTrue(relic.TryCast(false, false, target));
        Assert.IsTrue(castTriggered);
        Assert.AreEqual(DefenseRelicState.OnCooldown, relic.State);
        Assert.AreEqual(DefenseDepToOngRelic.CooldownDuration, relic.CooldownRemaining);

        // Advance cooldown
        relic.Tick(12f, false, false);
        Assert.AreEqual(DefenseRelicState.Ready, relic.State);
    }
}

using System;
using NUnit.Framework;

namespace ProjectZombie.Features.CombatLab.Tests
{
    /// <summary>Guards damage attribution semantics and explicit-window DPS calculations.</summary>
    public sealed class CombatTrialTests
    {
        [Test]
        public void LethalHitSeparatesRequestedDamageHpAndOverkill()
        {
            var trial = new CombatTrial();
            trial.RecordHit(35d, 10d, 0d);
            Assert.That(trial.RequestedDamage, Is.EqualTo(35d));
            Assert.That(trial.HpRemoved, Is.EqualTo(10d));
            Assert.That(trial.Overkill, Is.EqualTo(25d));
        }

        [Test]
        public void IdleTimeRemainsInDpsDenominator()
        {
            var trial = new CombatTrial();
            trial.RecordHit(35d, 100d, 65d);
            Assert.That(trial.Dps(10d), Is.EqualTo(3.5d));
            Assert.That(trial.Dps(0d), Is.Zero);
        }

        [Test]
        public void GroupDamageIsSumOfActualHpLoss()
        {
            var trial = new CombatTrial();
            trial.RecordHit(35d, 100d, 65d);
            trial.RecordHit(35d, 10d, 0d);
            Assert.That(trial.Hits, Is.EqualTo(2));
            Assert.That(trial.Dps(1d), Is.EqualTo(45d));
        }

        [Test]
        public void MitigationIsNotMistakenForOverkill()
        {
            var trial = new CombatTrial();
            trial.RecordHit(35d, 100d, 80d);
            Assert.That(trial.HpRemoved, Is.EqualTo(20d));
            Assert.That(trial.Overkill, Is.Zero);
        }

        [Test]
        public void WindupBoundsNominalAttackSpeedCeiling()
        {
            Assert.That(CombatTrial.NominalDps(35d, .25d, .18d), Is.EqualTo(140d).Within(.0001d));
            Assert.That(CombatTrial.NominalDps(35d, .125d, .18d), Is.EqualTo(35d / .18d).Within(.0001d));
        }

        [Test]
        public void MeanIntervalUsesStartsIncludingIdleGaps()
        {
            var trial = new CombatTrial();
            trial.RecordAttack(0d);
            trial.RecordAttack(.25d);
            trial.RecordAttack(1d);
            Assert.That(trial.IntervalTotal / trial.IntervalSamples, Is.EqualTo(.5d));
            Assert.That(trial.Attacks, Is.EqualTo(3));
        }

        [Test]
        public void InvalidMeasurementsAreRejectedWithoutChangingTotals()
        {
            var trial = new CombatTrial();
            Assert.Throws<ArgumentOutOfRangeException>(() => trial.RecordHit(double.NaN, 10d, 0d));
            Assert.Throws<ArgumentOutOfRangeException>(() => trial.RecordHit(5d, 10d, 11d));
            Assert.Throws<ArgumentOutOfRangeException>(() => trial.Dps(-1d));
            Assert.Throws<ArgumentOutOfRangeException>(() => CombatTrial.NominalDps(35d, 0d, .18d));
            Assert.That(trial.Hits, Is.Zero);
        }
    }
}

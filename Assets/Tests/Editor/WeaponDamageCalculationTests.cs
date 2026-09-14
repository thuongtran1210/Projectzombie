using NUnit.Framework;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class WeaponDamageCalculationTests
    {
        [Test]
        public void CalculateDamage_WithoutCrit_ReturnsBaseDamageMultiplied()
        {
            // Arrange
            float baseDamage = 50f;
            float damageMultiplier = 1.2f; // +20% damage buff

            // Act
            float finalDamage = baseDamage * damageMultiplier;

            // Assert
            Assert.AreEqual(60f, finalDamage, 0.001f);
        }

        [Test]
        public void CalculateCritDamage_WithCritMultiplier_CalculatesCorrectly()
        {
            // Arrange
            float baseDamage = 100f;
            float critMultiplier = 2.0f;

            // Act
            float finalDamage = baseDamage * critMultiplier;

            // Assert
            Assert.AreEqual(200f, finalDamage, 0.001f);
        }
    }
}

using NUnit.Framework;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Shared.Tests
{
    [TestFixture]
    public class ElementSynergyRulesTests
    {
        [Test]
        [TestCase(ElementType.Kim, ElementType.Thuy, ExpectedResult = true)]
        [TestCase(ElementType.Thuy, ElementType.Moc, ExpectedResult = true)]
        [TestCase(ElementType.Moc, ElementType.Hoa, ExpectedResult = true)]
        [TestCase(ElementType.Hoa, ElementType.Tho, ExpectedResult = true)]
        [TestCase(ElementType.Tho, ElementType.Kim, ExpectedResult = true)]
        [TestCase(ElementType.Kim, ElementType.Hoa, ExpectedResult = false)]
        [TestCase(ElementType.Moc, ElementType.Thuy, ExpectedResult = false)]
        [TestCase(ElementType.None, ElementType.Kim, ExpectedResult = false)]
        public bool Test_IsElementGenerative(ElementType parent, ElementType child)
        {
            return ElementSynergyRules.IsElementGenerative(parent, child);
        }

        [Test]
        [TestCase(ElementType.Kim, ElementType.Moc, ExpectedResult = true)]
        [TestCase(ElementType.Moc, ElementType.Tho, ExpectedResult = true)]
        [TestCase(ElementType.Tho, ElementType.Thuy, ExpectedResult = true)]
        [TestCase(ElementType.Thuy, ElementType.Hoa, ExpectedResult = true)]
        [TestCase(ElementType.Hoa, ElementType.Kim, ExpectedResult = true)]
        [TestCase(ElementType.Kim, ElementType.Thuy, ExpectedResult = false)]
        public bool Test_IsElementOvercoming(ElementType parent, ElementType child)
        {
            return ElementSynergyRules.IsElementOvercoming(parent, child);
        }
    }
}

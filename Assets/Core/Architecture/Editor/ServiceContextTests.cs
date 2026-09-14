using NUnit.Framework;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Core.Architecture.Tests
{
    public interface ITestService
    {
        string GetMessage();
    }

    public class TestService : ITestService
    {
        public string GetMessage() => "Hello ServiceContext";
    }

    [TestFixture]
    public class ServiceContextTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceContext.ResetStaticDomainState();
        }

        [Test]
        public void Test_RegisterAndGet_ReturnsInstance()
        {
            var service = new TestService();
            ServiceContext.Register<ITestService>(service);

            var retrieved = ServiceContext.Get<ITestService>();
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Hello ServiceContext", retrieved.GetMessage());
        }

        [Test]
        public void Test_Unregister_RemovesInstance()
        {
            var service = new TestService();
            ServiceContext.Register<ITestService>(service);
            ServiceContext.Unregister<ITestService>();

            var retrieved = ServiceContext.Get<ITestService>();
            Assert.IsNull(retrieved);
        }

        [Test]
        public void Test_ResetStaticDomainState_ClearsAllServices()
        {
            var service = new TestService();
            ServiceContext.Register<ITestService>(service);

            ServiceContext.ResetStaticDomainState();

            Assert.IsFalse(ServiceContext.TryGet<ITestService>(out _));
        }
    }
}

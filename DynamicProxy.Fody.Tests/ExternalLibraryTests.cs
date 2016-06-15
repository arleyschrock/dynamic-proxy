using NUnit.Framework;
using DynamicProxy;
using DynamicProxy.Tests.ExternalLibrary;

[assembly: ProxyFor(typeof(ISomeService))]

namespace DynamicProxy.Fody.Tests
{
    [TestFixture]
    public class ExternalLibraryTests
    {
        [Test]
        public void SomeInterfaceService()
        {
            var proxy = Proxy.CreateProxy<ISomeService>(x => "foo");
            Assert.IsTrue(proxy.GetType().Assembly == typeof(ExternalLibraryTests).Assembly);
            var result = proxy.GetString("bar");
            Assert.AreEqual(result, "foo");
        }
    }
}
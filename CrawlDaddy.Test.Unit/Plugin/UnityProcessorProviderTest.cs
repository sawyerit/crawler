using Commoner.Core.Unity;
using CrawlDaddy.Core;
using CrawlDaddy.Plugin;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CrawlDaddy.Test.Unit.Plugin
{
    [TestFixture]
    public class UnityProcessorProviderTest
    {
        UnityProcessorProvider _uut;

        [SetUp]
        public void SetUp()
        {
            Factory.Container = new UnityContainer();
            _uut = new UnityProcessorProvider();
        }

        [Test]
        public void GetProviders_NoRegisteredProcessors_ReturnsEmptyCollection()
        {
            IEnumerable<ICrawlProcessor> result = _uut.GetProcessors();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetProviders_RegisteredProcessors_ReturnsNonEmptyCollection()
        {
            Factory.Container.RegisterType<ICrawlProcessor, DummyProcessor>("dummy1");
            Factory.Container.RegisterType<ICrawlProcessor, DummyProcessor>("dummy2");
            Factory.Container.RegisterType<ICrawlProcessor, DummyProcessor>("dummy3");

            IEnumerable<ICrawlProcessor> result = _uut.GetProcessors();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void GetProviders_RegisteredProcessors_ReturnsUniqueInstancesForEachProcessor()
        {
            Factory.Container.RegisterType<ICrawlProcessor, DummyProcessor>("dummy1");

            IEnumerable<ICrawlProcessor> result1 = _uut.GetProcessors();
            IEnumerable<ICrawlProcessor> result2 = _uut.GetProcessors();

            Assert.AreNotSame(result1.ElementAt(0), result2.ElementAt(0));
        }
    }

    public class DummyProcessor : ICrawlProcessor
    {
        public void ProcessCrawledPage(Abot.Poco.CrawlContext crawlContext, Abot.Poco.CrawledPage crawledPage)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessCrawledDomain(Abot.Poco.CrawlContext crawlContext)
        {
            throw new System.NotImplementedException();
        }
    }

}

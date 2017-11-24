using Abot.Crawler;
using CrawlDaddy.Plugin;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Plugin
{
    [TestFixture]
    public class AbotWebCrawlerFactoryTest
    {
        [Test]
        public void GetCrawlerInstance()
        {
            Assert.IsTrue(new AbotWebCrawlerFactory().CreateInstance() is WebCrawler);
            Assert.IsTrue(new AbotWebCrawlerFactory().CreateInstance() is PoliteWebCrawler);
        }
    }
}

using CrawlDaddy.Core.Poco;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Core.Poco
{
    [TestFixture]
    public class CrawlDaddyConfigTest
    {
        [Test]
        public void Constructor_IsChildOfAbotRateLimiter()
        {
            CrawlDaddyConfig uut = new CrawlDaddyConfig();

            Assert.AreEqual(0, uut.MinTimeBetweenDomainRetrievalRequestsInSecs);
            Assert.AreEqual(0, uut.MaxDomainRetrievalCount);
            Assert.AreEqual(0, uut.MaxConcurrentCrawls);
            Assert.AreEqual(0, uut.MaxPageProcessorTimeInMilliSecs);
            Assert.AreEqual(0, uut.MaxDomainProcessorTimeInMilliSecs);
        }
    }
}

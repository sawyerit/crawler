using Abot.Core;
using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Core
{
    [TestFixture]
    public class CustomRateLimiterTest
    {
        [Test]
        public void Constructor_IsChildOfAbotRateLimiter()
        {
            CustomRateLimiter uut = new CustomRateLimiter(new CrawlDaddyConfig { MinTimeBetweenDomainRetrievalRequestsInSecs = 20 });

            Assert.IsTrue(uut is RateLimiter);
            Assert.AreEqual(1, uut.Occurrences);
            Assert.AreEqual(20000, uut.TimeUnitMilliseconds);
        }
    }
}

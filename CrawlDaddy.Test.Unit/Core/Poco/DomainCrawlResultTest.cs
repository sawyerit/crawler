using CrawlDaddy.Core.Poco;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Core.Poco
{
    [TestFixture]
    public class DomainCrawlResultTest
    {
        [Test]
        public void Constructor()
        {
            DomainCrawlResult uut = new DomainCrawlResult();
            Assert.IsNull(uut.CrawlResult);
            Assert.IsNull(uut.Domain);
        }
    }
}

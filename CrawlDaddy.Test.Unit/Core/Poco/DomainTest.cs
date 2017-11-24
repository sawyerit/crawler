using CrawlDaddy.Core.Poco;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Core.Poco
{
    [TestFixture]
    public class DomainTest
    {
        [Test]
        public void Constructor()
        {
            Domain uut = new Domain();

            Assert.AreEqual(0, uut.DomainId);
            Assert.AreEqual(null, uut.Uri);
        }
    }
}

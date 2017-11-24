using CrawlDaddy.Core.Poco;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Core.Poco
{
    [TestFixture]
    public class DataComponentTest
    {
        [Test]
        public void Constructor()
        {
            DataComponent uut = new DataComponent();
            Assert.AreEqual(0, uut.AttributeId);
            Assert.AreEqual(0, uut.DomainId);
            Assert.AreEqual(null, uut.Attributes);
            Assert.AreEqual(null, uut.DomainUri);
            Assert.AreEqual(null, uut.FoundOnUri);
        }
    }
}

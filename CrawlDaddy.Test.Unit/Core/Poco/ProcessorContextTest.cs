using CrawlDaddy.Core.Poco;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Core.Poco
{
    [TestFixture]
    public class ProcessorContextTest
    {
        [Test]
        public void Constructor()
        {
            ProcessorContext uut = new ProcessorContext();
            Assert.IsNull(uut.PrimaryPersistenceProvider);
            Assert.IsNull(uut.BackupPersistenceProvider);
            Assert.IsNull(uut.Domain);
            Assert.IsNull(uut.CrawlProcessors);
        }
    }
}

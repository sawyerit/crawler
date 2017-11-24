using CrawlDaddy.Plugin.Processor;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Plugin.Processor
{
    [TestFixture]
    public class ProcessorResultTest
    {
        [Test]
        public void Constructor()
        {
            ProcessorResult uut = new ProcessorResult();

            Assert.IsNotNull(uut.Attributes);
            Assert.AreEqual(0, uut.Attributes.Count);
            Assert.AreEqual(0, uut.UniqueAttributeId);
            Assert.IsFalse(uut.IsAHit);
        }
    }
}

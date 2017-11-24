using CrawlDaddy.Plugin.Processor.SearchEngineMarketing;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Plugin.Processor.SearchEngineMarketing
{
    [TestFixture]
    public class SemListFromFileTest
    {
        [Test]
        public void Constructor()
        {
            SemListFromFile uut = new SemListFromFile();

            Assert.AreEqual(274, uut.Cities.Count);
            Assert.AreEqual(12, uut.LocalKeywords.Count);
            Assert.AreEqual(93, uut.States.Count);
            Assert.AreEqual(73876, uut.VerticalKeywords.Count);
        }
    }
}

using CrawlDaddy.Plugin.Processor.SearchEngineMarketing;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.Plugin.Processor.SearchEngineMarketing
{
    [TestFixture]
    public class VerticalKeywordTest
    {
        [Test]
        public void Constructor()
        {
            VerticalKeyword uut = new VerticalKeyword();

            Assert.AreEqual(0, uut.CategoryId);
            Assert.AreEqual("", uut.CategoryName);
            Assert.AreEqual("", uut.Text);
        }
    }
}

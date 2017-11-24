using CrawlDaddy.Plugin.Processor.SearchEngineMarketing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlDaddy.Test.Unit.Plugin.Processor.SearchEngineMarketing
{
    [TestFixture]
    public class SemListTest
    {
        [Test]
        public void Constructor()
        {
            SemList uut = new SemList();

            Assert.AreEqual(0, uut.Cities.Count);
            Assert.AreEqual(0, uut.LocalKeywords.Count);
            Assert.AreEqual(0, uut.States.Count);
            Assert.AreEqual(0, uut.VerticalKeywords.Count);
        }
    }
}

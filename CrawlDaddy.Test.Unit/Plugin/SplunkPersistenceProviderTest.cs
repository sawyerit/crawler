using CrawlDaddy.Core.Poco;
using CrawlDaddy.Plugin;
using log4net;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CrawlDaddy.Test.Unit.Plugin
{
    [TestFixture]
    public class SplunkPersistenceProviderTest
    {
        [Test]
        public void Save_AllParamsValid_CallsILog()
        {
            Mock<ILog> fakeLogger = new Mock<ILog>();

            DataComponent data = new DataComponent { ShopperId = "100", AttributeId = 111, DomainId = 222, Attributes = new Dictionary<string, string>() { { "aaa", "bbb" } }, DomainUri = new Uri("http://a.com/"), FoundOnUri = new Uri("http://a.com/a.html") };
            
            new SplunkPersistenceProvider(fakeLogger.Object).Save(data);

            fakeLogger.Verify(f => f.Info("shopperId=\"100\" domainId=\"222\" attributeId=\"111\" domain=\"http://a.com/\" foundOnPage=\"http://a.com/a.html\" aaa=\"bbb\""), Times.Exactly(1));
        }

        [Test]
        public void Save_NullDomainAndFoundOnPageUri_CallsILog()
        {
            Mock<ILog> fakeLogger = new Mock<ILog>();

            DataComponent data = new DataComponent { ShopperId = "100", AttributeId = 111, DomainId = 222, Attributes = new Dictionary<string, string>() { { "aaa", "bbb" } }, DomainUri = null, FoundOnUri = null };

            new SplunkPersistenceProvider(fakeLogger.Object).Save(data);

            fakeLogger.Verify(f => f.Info("shopperId=\"100\" domainId=\"222\" attributeId=\"111\" domain=\"\" foundOnPage=\"\" aaa=\"bbb\""), Times.Exactly(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Save_NullParam()
        {
            new SplunkPersistenceProvider().Save(null);
        }
                    
    }
}

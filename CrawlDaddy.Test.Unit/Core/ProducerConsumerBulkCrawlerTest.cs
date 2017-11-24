using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using NUnit.Framework;
using System;

namespace CrawlDaddy.Test.Unit.Core
{
    [TestFixture]
    public class ProducerConsumerBulkCrawlerTest : BulkCrawlerTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_DomainProducerIsNull()
        {
            new ProducerConsumerBulkCrawler(null, _fakeDomainConsumer.Object, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_DomainConsumerIsNull()
        {
            new ProducerConsumerBulkCrawler(_fakeDomainProducer.Object, null, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConfigIsNull()
        {
            new ProducerConsumerBulkCrawler(_fakeDomainProducer.Object, _fakeDomainConsumer.Object, null);
        }

        public override IBulkCrawler GetInstance(IDomainProducer producer, IDomainConsumer consumer, CrawlDaddyConfig config)
        {
            return new ProducerConsumerBulkCrawler(producer, consumer, config);
        }
    }
}

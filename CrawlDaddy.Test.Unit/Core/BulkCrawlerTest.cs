using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CrawlDaddy.Test.Unit.Core
{
    [TestFixture]
    public abstract class BulkCrawlerTest
    {
        protected IBulkCrawler _uut;
        protected Mock<IDomainProducer> _fakeDomainProducer;
        protected Mock<IDomainConsumer> _fakeDomainConsumer;
        protected CrawlDaddyConfig _dummyConfig;

        public abstract IBulkCrawler GetInstance(IDomainProducer producer, IDomainConsumer consumer, CrawlDaddyConfig config);

        [SetUp]
        public void SetUp()
        {
            _fakeDomainProducer = new Mock<IDomainProducer>();
            _fakeDomainConsumer = new Mock<IDomainConsumer>();

            _dummyConfig = new CrawlDaddyConfig
            {
                MaxConcurrentCrawls = 5
            };

            _uut = GetInstance(_fakeDomainProducer.Object, _fakeDomainConsumer.Object, _dummyConfig);
        }

        [Test]
        public void Start_ProducerIsCalled_OncePerSecond()
        {
            _uut.Start();
            System.Threading.Thread.Sleep(2500);
            _uut.Stop();

            _fakeDomainProducer.Verify(f => f.Produce(It.IsAny<BlockingCollection<Domain>>(), It.IsAny<List<DomainCrawlResult>>()), Times.Exactly(3));
        }

        [Test]
        public void Start_ProducerThrowsException_DoesNotCrash()
        {
            _fakeDomainProducer.Setup(f => f.Produce(It.IsAny<BlockingCollection<Domain>>(), It.IsAny<List<DomainCrawlResult>>())).Throws(new Exception("Oh no!!!"));

            _uut.Start();
            System.Threading.Thread.Sleep(100);
            _uut.Stop();

            _fakeDomainProducer.Verify(f => f.Produce(It.IsAny<BlockingCollection<Domain>>(), It.IsAny<List<DomainCrawlResult>>()), Times.Exactly(1));
        }

        [Test]
        public void Start_ConsumerThrowsException_DoesNotCrash()
        {
            //Arrange
            _fakeDomainProducer.Setup(f => f.Produce(It.IsAny<BlockingCollection<Domain>>(), It.IsAny<List<DomainCrawlResult>>()))
                .Callback((BlockingCollection<Domain> domainsToCrawl, List<DomainCrawlResult> crawledDomains) =>
                {
                    domainsToCrawl.Add(new Domain());
                });
            _fakeDomainConsumer.Setup(f => f.Consume(It.IsAny<Domain>(), It.IsAny<CancellationTokenSource>())).Throws(new Exception("Oh no!!!"));

            _uut.Start();
            System.Threading.Thread.Sleep(5000);//Give plenty of time for consumers to run. This will fail on the build server unless we give it a lot of time
            _uut.Stop();

            _fakeDomainProducer.VerifyAll();
            _fakeDomainConsumer.Verify(f => f.Consume(It.IsAny<Domain>(), It.IsAny<CancellationTokenSource>()), Times.AtLeast(1));
        }

        [Test]
        public void Start_WhenProducerAddsToDomainsToCrawlCollection_ConsumerIsCalledForEachAddedDomain()
        {
            //Arrange
            _fakeDomainProducer.Setup(f => f.Produce(It.IsAny<BlockingCollection<Domain>>(), It.IsAny<List<DomainCrawlResult>>()))
                .Callback((BlockingCollection<Domain> domainsToCrawl, List<DomainCrawlResult> crawledDomains) => 
                {
                    domainsToCrawl.Add(new Domain());
                    domainsToCrawl.Add(new Domain());
                    domainsToCrawl.Add(new Domain());
                    domainsToCrawl.Add(new Domain());
                    domainsToCrawl.Add(new Domain());
                });

            //Act
            _uut.Start();
            System.Threading.Thread.Sleep(5000);//Give plenty of time for consumers to run. This will fail on the build server unless we give it a lot of time
            _uut.Stop();

            //Assert
            _fakeDomainProducer.VerifyAll();
            _fakeDomainConsumer.Verify(f => f.Consume(It.IsAny<Domain>(), It.IsAny<CancellationTokenSource>()), Times.AtLeast(2));
        }

        [Test]
        public void Start_WhenConsumerReturnsCrawlResult_CrawlResultIsPassedBackToProducer()
        {
            _fakeDomainProducer.Setup(f => f.Produce(It.IsAny<BlockingCollection<Domain>>(), It.IsAny<List<DomainCrawlResult>>()))
                .Callback((BlockingCollection<Domain> domainsToCrawl, List<DomainCrawlResult> crawledDomains) =>
                {
                    domainsToCrawl.Add(new Domain());
                    domainsToCrawl.Add(new Domain());
                });
            _fakeDomainConsumer.Setup(f => f.Consume(It.IsAny<Domain>(), It.IsAny<CancellationTokenSource>())).Returns(new DomainCrawlResult());

            _uut.Start();
            System.Threading.Thread.Sleep(5000);//Give plenty of time for consumers to run. This will fail on the build server unless we give it a lot of time
            _uut.Stop();

            _fakeDomainProducer.Verify(f => f.Produce(It.IsAny<BlockingCollection<Domain>>(), It.Is<List<DomainCrawlResult>>(l => l.Count > 0)), Times.AtLeast(1));
        }

        protected BlockingCollection<Domain> GetDomainsToCrawl(int number)
        {
            Domain domain = new Domain();

            BlockingCollection<Domain> domainsToCrawl = new BlockingCollection<Domain>();
            for (int i = 0; i < number; i++)
                domainsToCrawl.Add(domain);

            return domainsToCrawl;
        }

        protected List<DomainCrawlResult> GetCrawledDomains(int number)
        {
            Domain domain = new Domain();

            List<DomainCrawlResult> crawledDomains = new List<DomainCrawlResult>();
            for (int i = 0; i < number; i++)
                crawledDomains.Add(GetDomainCrawlResult(domain));

            return crawledDomains;
        }

        protected DomainCrawlResult GetDomainCrawlResult(Domain domain)
        {
            ConcurrentDictionary<string, byte> dictionary = new ConcurrentDictionary<string, byte>();
            dictionary.TryAdd("a", 0);
            dictionary.TryAdd("b", 0);
            dictionary.TryAdd("c", 0);

            return new DomainCrawlResult
            {
                Domain = domain,
                CrawlResult = new CrawlResult
                {
                    Elapsed = TimeSpan.FromSeconds(10),
                    RootUri = new Uri("http://a.com"),
                    CrawlContext = new CrawlContext
                    {
                        CrawledUrls = dictionary
                    }
                }
            };
        }
    }
}

using Abot.Core;
using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CrawlDaddy.Test.Unit.Core
{
    [TestFixture]
    public class DomainProducerTest
    {
        DomainProducer _uut;
        Mock<IDomainProvider> _fakeDomainProvider;
        Mock<IDomainConsumer> _fakeDomainConsumer;
        Mock<IRateLimiter> _fakeRateLimiter;
        CrawlDaddyConfig _dummyConfig;

        [SetUp]
        public void SetUp()
        {
            _fakeDomainProvider = new Mock<IDomainProvider>();
            _fakeDomainConsumer = new Mock<IDomainConsumer>();
            _fakeRateLimiter = new Mock<IRateLimiter>();

            _dummyConfig = new CrawlDaddyConfig
            {
                MaxConcurrentCrawls = 11,
                MaxDomainRetrievalCount = 10
            };

            _uut = new DomainProducer(_fakeDomainProvider.Object, _fakeRateLimiter.Object, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_DomainProvider_IsNull()
        {
            new DomainProducer(null, _fakeRateLimiter.Object, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_RateLimiter_IsNull()
        {
            new DomainProducer(_fakeDomainProvider.Object, null, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConfigIsNull()
        {
            new DomainProducer(_fakeDomainProvider.Object, _fakeRateLimiter.Object, null);
        }

        [Test]
        public void Produce_DomainsToCrawlLessThan25PercentOfMax_CallDependencies()
        {
            _uut.Produce(GetDomainsToCrawl(2), GetCrawledDomains(5));

            _fakeRateLimiter.Verify(f => f.WaitToProceed(), Times.Exactly(1));
            _fakeDomainProvider.Verify(f => f.ReportCrawledDomains(It.Is<IEnumerable<Domain>>(e => e.Count() == 5)), Times.Exactly(1));
            _fakeDomainProvider.Verify(f => f.GetDomainsToCrawl(_dummyConfig.MaxDomainRetrievalCount, It.Is<string>(s => !string.IsNullOrWhiteSpace(s))), Times.Exactly(1));
        }

        [Test]
        public void Produce_DomainsToCrawlLessThan25PercentOfMax_AddsDomainProvidersResultToTheDomainsToCrawlCollection()
        {
            BlockingCollection<Domain> domainsToCrawl = GetDomainsToCrawl(2);
            List<Domain> domainsToBeAdded = new List<Domain>{ new Domain(), new Domain() };
            _fakeDomainProvider.Setup(f => f.GetDomainsToCrawl(It.IsAny<int>(), It.IsAny<string>())).Returns(domainsToBeAdded);
            
            _uut.Produce(domainsToCrawl, GetCrawledDomains(5));

            Assert.AreEqual(4, domainsToCrawl.Count);
        }

        [Test]
        public void Produce_NoCrawledDomainsToReport_DoesReportCompletedCrawls()
        {
            _uut.Produce(GetDomainsToCrawl(2), new List<DomainCrawlResult>());

            _fakeDomainProvider.Verify(f => f.ReportCrawledDomains(It.IsAny<IEnumerable<Domain>>()), Times.Exactly(0));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Produce_NullDomainsToCrawlCollection()
        {
            _uut.Produce(null, GetCrawledDomains(5));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Produce_NullCrawledDomainsCollection()
        {
            _uut.Produce(GetDomainsToCrawl(2), null);
        }

        [Test]
        public void Produce_DomainProviderThrowsExceptionWhileRetrievingDomainsToCrawl_NothingIsAddedToDomainsToCrawlCollection()
        {
            BlockingCollection<Domain> domainsToCrawl = GetDomainsToCrawl(2);
            _fakeDomainProvider.Setup(f => f.GetDomainsToCrawl(It.IsAny<int>(), It.IsAny<string>())).Throws(new Exception("oh no"));

            _uut.Produce(domainsToCrawl, GetCrawledDomains(5));

            Assert.AreEqual(2, domainsToCrawl.Count);
        }

        [Test]
        public void Produce_DomainProviderThrowsExceptionWhileReportedCrawledDomains_StillAddsDomainProvidersResultToTheDomainsToCrawlCollection()
        {
            BlockingCollection<Domain> domainsToCrawl = GetDomainsToCrawl(2);
            List<Domain> domainsToBeAdded = new List<Domain> { new Domain(), new Domain() };
            _fakeDomainProvider.Setup(f => f.GetDomainsToCrawl(It.IsAny<int>(), It.IsAny<string>())).Returns(domainsToBeAdded);
            _fakeDomainProvider.Setup(f => f.ReportCrawledDomains(It.IsAny<IEnumerable<Domain>>())).Throws(new Exception("oh no"));

            _uut.Produce(domainsToCrawl, GetCrawledDomains(5));

            Assert.AreEqual(4, domainsToCrawl.Count);
        }

        [Test]
        public void Produce_DomainsToCrawlAbove25PercentOfMax_DoesNothing()
        {
            _uut.Produce(GetDomainsToCrawl(3), GetCrawledDomains(5));

            _fakeRateLimiter.Verify(f => f.WaitToProceed(), Times.Exactly(0));
            _fakeDomainProvider.Verify(f => f.ReportCrawledDomains(It.IsAny<IEnumerable<Domain>>()), Times.Exactly(0));
            _fakeDomainProvider.Verify(f => f.GetDomainsToCrawl(_dummyConfig.MaxDomainRetrievalCount, It.IsAny<string>()), Times.Exactly(0));
        }

        private BlockingCollection<Domain> GetDomainsToCrawl(int number)
        {
            Domain domain = new Domain();

            BlockingCollection<Domain> domainsToCrawl = new BlockingCollection<Domain>();
            for (int i = 0; i < number; i++)
                domainsToCrawl.Add(domain);

            return domainsToCrawl;
        }

        private List<DomainCrawlResult> GetCrawledDomains(int number)
        {
            Domain domain = new Domain();

            List<DomainCrawlResult> crawledDomains = new List<DomainCrawlResult>();
            for (int i = 0; i < number; i++)
                crawledDomains.Add(GetDomainCrawlResult(domain));

            return crawledDomains;
        }

        private DomainCrawlResult GetDomainCrawlResult(Domain domain)
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

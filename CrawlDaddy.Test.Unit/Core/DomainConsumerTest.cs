using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CrawlDaddy.Test.Unit.Core
{
    [TestFixture]
    public class DomainConsumerTest
    {
        Mock<IWebCrawlerFactory> _fakeWebCrawlerFactory;
        Mock<IWebCrawler> _fakeWebCrawler;
        Mock<IProcessorProvider> _fakeProcessorProvider;
        Mock<ICrawlProcessor> _fakeProcessor1;
        Mock<ICrawlProcessor> _fakeProcessor2;
        Mock<ICrawlProcessor> _fakeProcessor3;
        IEnumerable<ICrawlProcessor> _dummyCrawlProcessors;
        ProcessorContext _dummyProcessorContext;
        CrawlDaddyConfig _dummyConfig;
        CancellationTokenSource _dummyCancellationToken;

        DomainConsumer _uut;

        [SetUp]
        public void SetUp()
        {
            _fakeWebCrawlerFactory = new Mock<IWebCrawlerFactory>();
            _fakeWebCrawler = new Mock<IWebCrawler>();
            _fakeProcessorProvider = new Mock<IProcessorProvider>();
            _fakeProcessor1 = new Mock<ICrawlProcessor>();
            _fakeProcessor2 = new Mock<ICrawlProcessor>();
            _fakeProcessor3 = new Mock<ICrawlProcessor>();

            _dummyCrawlProcessors = new List<ICrawlProcessor>()
            {
                _fakeProcessor1.Object,
                _fakeProcessor2.Object,
                _fakeProcessor3.Object
            };
            _dummyProcessorContext = new ProcessorContext
            {
                PrimaryPersistenceProvider = new Mock<IPersistenceProvider>().Object,
                BackupPersistenceProvider = new Mock<IPersistenceProvider>().Object
            };
            _dummyConfig = new CrawlDaddyConfig
            {
                MaxConcurrentCrawls = 11,
                MaxDomainProcessorTimeInMilliSecs = 2000,
                MaxPageProcessorTimeInMilliSecs = 2000,
                MaxDomainRetrievalCount = 10,
                MimeTypesToProcess = new[] { "text/html" },
                HttpStatusesToProcess = new[] { "200" }
            };
            _dummyCancellationToken = new CancellationTokenSource();

            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);
            _fakeWebCrawler.Setup(f => f.CrawlBag).Returns(new DummyCrawlBag());

            _uut = new DomainConsumer(_fakeWebCrawlerFactory.Object, _fakeProcessorProvider.Object, _dummyProcessorContext, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_CrawlFactory_IsNull()
        {
            new DomainConsumer(null, _fakeProcessorProvider.Object, _dummyProcessorContext, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ProcessorProvider_IsNull()
        {
            new DomainConsumer(_fakeWebCrawlerFactory.Object, null, _dummyProcessorContext, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ProcessorContext_IsNull()
        {
            new DomainConsumer(_fakeWebCrawlerFactory.Object, _fakeProcessorProvider.Object, null, _dummyConfig);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConfigIsNull()
        {
            new DomainConsumer(_fakeWebCrawlerFactory.Object, _fakeProcessorProvider.Object, _dummyProcessorContext, null);
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Consume_NullDomain()
        {
            _uut.Consume(null, _dummyCancellationToken);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Consume_NullCancellationToken()
        {
            _uut.Consume(new Domain { Uri = new Uri("http://www.adamthings.com") }, null);
        }

        [Test]
        public void Consume_ValidDomain_CallsPageProcessors()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
        }

        [Test]
        public void Consume_ValidDomain_CallsDomainProcessors()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };
            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult);
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://a.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
        }

        [Test]
        public void Consume_PageProcessorThrowsException_DoesNotCrash()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);
            _fakeProcessor1.Setup(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), crawledPage)).Throws(new Exception("oh no page"));

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
        }

        [Test, Ignore("Running the processor on a seperate thread just for timeout check was making the simplest processor timeout")]
        public void Consume_PageProcessorTimesOut_DoesNotCrash()
        {
            //Arrange
            _dummyConfig.MaxPageProcessorTimeInMilliSecs = 1000;
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };
            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), new CrawledPage(new Uri("http://a.com")))));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);
            _fakeProcessor1.Setup(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()))
                .Callback((CrawlContext cc, CrawledPage cp) => System.Threading.Thread.Sleep(10000));//ten seconds

            //Act
            Stopwatch timer = Stopwatch.StartNew();
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://a.com") }, _dummyCancellationToken);
            timer.Stop();

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));

            Assert.IsTrue(timer.ElapsedMilliseconds > 900);
            Assert.IsTrue(timer.ElapsedMilliseconds < 2000);
        }


        [Test]
        public void Consume_ValidDomain_NullAppConfigMimeTypesThrowsException_DoesNotCrash()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _dummyConfig.MimeTypesToProcess = null;

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
        }

        [Test]
        public void Consume_ValidDomain_NullAppConfigHttpStatusesToProcessThrowsException_DoesNotCrash()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _dummyConfig.HttpStatusesToProcess = null;

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
        }

        [Test]
        public void Consume_ValidDomain_EmptyAppConfigHttpStatusesToProcess_CrawlsPerformed()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _dummyConfig.HttpStatusesToProcess = new string[] { };

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
        }

        [Test]
        public void Consume_ValidDomain_EmptyAppConfigMimeTypes_CrawlsPerformed()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _dummyConfig.MimeTypesToProcess = new string[] { };

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
        }

        [Test]
        public void Consume_ValidDomain_AppConfigMimeTypesNullWebResponse_CrawlsPerformed()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _dummyConfig.MimeTypesToProcess = new string[] { };
            crawledPage.HttpWebResponse = null;

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
        }

        [Test]
        public void Consume_ValidDomain_AppConfigHttpStatusesToProcessNullWebResponse_CrawlsPerformed()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };

            CrawledPage crawledPage = new PageRequester(new CrawlConfiguration()).MakeRequest(new Uri("http://www.adamthings.com"));

            _dummyConfig.HttpStatusesToProcess = new string[] { };
            crawledPage.HttpWebResponse = null;

            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult)
                .Callback(() => _fakeWebCrawler
                    .Raise(f => f.PageCrawlCompleted += null, new PageCrawlCompletedArgs(GetCrawlContext(_dummyCrawlProcessors), crawledPage)));
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://www.adamthings.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(1));
        }


        [Test]
        public void Consume_DomainProcessorThrowsException_DoesNotCrash()
        {
            //Arrange
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };
            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult);
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);
            _fakeProcessor1.Setup(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>())).Throws(new Exception("oh no domain"));

            //Act
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://a.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
        }

        [Test, Ignore("Running the processor on a seperate thread just for timeout check was making the simplest processor timeout")]
        public void Consume_DomainProcessorTimesOut_DoesNotCrash()
        {
            //Arrange
            _dummyConfig.MaxDomainProcessorTimeInMilliSecs = 1000;
            CrawlResult fakeResult = new CrawlResult { CrawlContext = GetCrawlContext(_dummyCrawlProcessors) };
            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult);
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);
            _fakeProcessor1.Setup(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()))
                .Callback((CrawlContext cc) => System.Threading.Thread.Sleep(10000));//ten seconds

            //Act
            Stopwatch timer = Stopwatch.StartNew();
            _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://a.com") }, _dummyCancellationToken);
            timer.Stop();

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            _fakeProcessor1.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
            _fakeProcessor2.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));
            _fakeProcessor3.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(1));

            Assert.IsTrue(timer.ElapsedMilliseconds > 900);
            Assert.IsTrue(timer.ElapsedMilliseconds < 2000);
        }

        [Test]
        public void Consume_CrawlerThrowsException_PageAndDomainNotProcessed()
        {
            //Arrange
            Exception ex = new Exception("oh no");
            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Throws(ex);
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            DomainCrawlResult result = _uut.Consume(new Domain { DomainId = 1, Uri = new Uri("http://a.com") }, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            Assert.IsTrue(result.CrawlResult.ErrorOccurred);
            Assert.AreEqual(ex, result.CrawlResult.ErrorException);

            _fakeProcessor1.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
            _fakeProcessor2.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));
            _fakeProcessor3.Verify(f => f.ProcessCrawledPage(It.IsAny<CrawlContext>(), It.IsAny<CrawledPage>()), Times.Exactly(0));

            _fakeProcessor1.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(0));
            _fakeProcessor1.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(0));
            _fakeProcessor1.Verify(f => f.ProcessCrawledDomain(It.IsAny<CrawlContext>()), Times.Exactly(0));
        }


        [Test]
        public void Consume_ValidDomain_DomainResultPropertiesSet()
        {
            //Arrange
            Domain domain = new Domain { DomainId = 1, Uri = new Uri("http://a.com") };
            CrawlContext context = GetCrawlContext(_dummyCrawlProcessors);
            CrawlResult fakeResult = new CrawlResult { CrawlContext = context };
            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult);
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            DomainCrawlResult result = _uut.Consume(domain, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            Assert.AreEqual(domain, result.Domain);
            Assert.AreEqual(fakeResult, result.CrawlResult);
        }

        [Test]
        public void Consume_ValidDomain_CrawlerCrawlBagSet()
        {
            //Arrange
            Domain domain = new Domain { DomainId = 1, Uri = new Uri("http://a.com") };
            CrawlContext context = GetCrawlContext(_dummyCrawlProcessors);
            CrawlResult fakeResult = new CrawlResult { CrawlContext = context };
            _fakeWebCrawlerFactory.Setup(f => f.CreateInstance()).Returns(_fakeWebCrawler.Object);
            _fakeWebCrawler.Setup(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>())).Returns(fakeResult);
            _fakeProcessorProvider.Setup(f => f.GetProcessors()).Returns(_dummyCrawlProcessors);

            //Act
            DomainCrawlResult result = _uut.Consume(domain, _dummyCancellationToken);

            //Assert
            _fakeProcessorProvider.Verify(f => f.GetProcessors(), Times.Exactly(1));
            _fakeWebCrawlerFactory.Verify(f => f.CreateInstance(), Times.Exactly(1));
            _fakeWebCrawler.Verify(f => f.Crawl(It.IsAny<Uri>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(1));

            Assert.AreEqual(domain, _fakeWebCrawler.Object.CrawlBag.GoDaddyProcessorContext.Domain);
            Assert.AreEqual(_dummyProcessorContext.PrimaryPersistenceProvider, _fakeWebCrawler.Object.CrawlBag.GoDaddyProcessorContext.PrimaryPersistenceProvider);
            Assert.AreEqual(_dummyProcessorContext.BackupPersistenceProvider, _fakeWebCrawler.Object.CrawlBag.GoDaddyProcessorContext.BackupPersistenceProvider);
            Assert.AreEqual(_dummyCrawlProcessors, _fakeWebCrawler.Object.CrawlBag.GoDaddyProcessorContext.CrawlProcessors);
        }

        private CrawlContext GetCrawlContext(IEnumerable<ICrawlProcessor> processors)
        {
            return new CrawlContext
            {
                CrawlBag = new DummyCrawlBag
                {
                    GoDaddyProcessorContext = new ProcessorContext
                    {
                        CrawlProcessors = processors
                    }
                }
            };
        }
    }

    public class DummyCrawlBag
    {
        public dynamic GoDaddyProcessorContext { get; set; }
    }
}

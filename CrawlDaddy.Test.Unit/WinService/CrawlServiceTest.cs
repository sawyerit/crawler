using CrawlDaddy.Core;
using CrawlDaddy.WinService;
using Moq;
using NUnit.Framework;

namespace CrawlDaddy.Test.Unit.WinService
{
    #region Test Wrapper

    internal class CrawlServiceWrapper : CrawlService
    {
        public CrawlServiceWrapper(IBulkCrawler bulkCrawler)
            : base(bulkCrawler)
        {
        }

        public void CallOnStart(string[] args)
        {
            base.OnStart(args);
        }

        public void CallOnStop()
        {
            base.OnStop();
        }
    }

    #endregion

    [TestFixture]
    public class CrawlServiceTest
    {
        Mock<IBulkCrawler> _fakeBulkCrawler;
        CrawlServiceWrapper _uut;

        [SetUp]
        public void SetUp()
        {
            _fakeBulkCrawler = new Mock<IBulkCrawler>();

            _uut = new CrawlServiceWrapper(_fakeBulkCrawler.Object);
        }

        [Test]
        public void OnStart_CallsCrawlServiceManagerStartMethod()
        {
            _uut.CallOnStart(new string[0]);

            System.Threading.Thread.Sleep(15);
            _fakeBulkCrawler.Verify(f => f.Start(), Times.Exactly(1));
        }

        [Test]
        public void OnStop_CallsCrawlServiceManagerStopMethod()
        {
            _uut.CallOnStop();

            System.Threading.Thread.Sleep(15);
            _fakeBulkCrawler.Verify(f => f.Stop(), Times.Exactly(1));
        }
    }
}

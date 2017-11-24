using Abot.Core;
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CrawlDaddy.Core
{
    public interface IDomainProducer
    {
        void Produce(BlockingCollection<Domain> domainsToCrawl, List<DomainCrawlResult> crawledDomains);
    }

    public class DomainProducer : IDomainProducer
    {
        static ILog _logger = LogManager.GetLogger(typeof(DomainProducer).FullName);
				static ILog _producerLogger = LogManager.GetLogger("ProducerLogger");
        IDomainProvider _domainProvider;
        IRateLimiter _domainRetrievalRateLimiter;
        CrawlDaddyConfig _config;

        public DomainProducer(IDomainProvider domainProvider, IRateLimiter domainRetrievalRateLimiter, CrawlDaddyConfig config)
        {

            if (domainProvider == null)
                throw new ArgumentNullException("domainProvider");

            if (domainRetrievalRateLimiter == null)
                throw new ArgumentNullException("domainRetrievalRateLimiter");

            if (config == null)
                throw new ArgumentNullException("config");

            _domainProvider = domainProvider;
            _domainRetrievalRateLimiter = domainRetrievalRateLimiter;
            _config = config;
        }

        public void Produce(BlockingCollection<Domain> domainsToCrawl, List<DomainCrawlResult> crawledDomains)
        {
            if (domainsToCrawl == null)
                throw new ArgumentNullException("domainsToCrawl");

            if (crawledDomains == null)
                throw new ArgumentNullException("crawledDomains");

            //Only add to the domainsToCrawl collection if the consumers have completed more than 75% of the sites
            double bufferThreshold = (_config.MaxDomainRetrievalCount * .25);
            if (domainsToCrawl.Count < bufferThreshold)
            {
                _domainRetrievalRateLimiter.WaitToProceed();

                ReportCompletedCrawls(crawledDomains);
								
                foreach (Domain domain in GetDomainsToCrawl())
                    domainsToCrawl.Add(domain);
            }
        }

        private IEnumerable<Domain> GetDomainsToCrawl()
        {
            IEnumerable<Domain> domainsToCrawl = new List<Domain>();
            try
            {
								_producerLogger.DebugFormat("Retrieving up to [{0}] domains to crawl", _config.MaxDomainRetrievalCount); 

                domainsToCrawl = _domainProvider.GetDomainsToCrawl(_config.MaxDomainRetrievalCount, Environment.MachineName);

								_producerLogger.DebugFormat("[{0}] domains returned", domainsToCrawl.Count());
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception occurred retrieving domains to crawl", e);
            }
            return domainsToCrawl;
        }

        private void ReportCompletedCrawls(List<DomainCrawlResult> crawledDomains)
        {
            if (crawledDomains == null)
                return;

            List<Domain> crawledDomainsToReport = new List<Domain>();
            lock (crawledDomains)
            {
                crawledDomainsToReport = crawledDomains.Where(d => d.Domain != null).Select(r => r.Domain).ToList();
                crawledDomains.Clear();
            }

            if (crawledDomainsToReport.Count < 1)
                return;

            try
            {
                _domainProvider.ReportCrawledDomains(crawledDomainsToReport);
								_producerLogger.DebugFormat("Reported [{0}] completed crawls", crawledDomainsToReport.Count);
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception occurred reporting crawledDomains", e);
            }
        }
    }
}

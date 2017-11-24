
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlDaddy.Core
{
    public interface IBulkCrawler
    {
        void Start();
        void Stop();
    }

    public class ProducerConsumerBulkCrawler : IBulkCrawler
    {
        static ILog _logger = LogManager.GetLogger(typeof(ProducerConsumerBulkCrawler).FullName);
        protected IDomainProducer _domainProducer;
        protected IDomainConsumer _domainConsumer;
        protected CrawlDaddyConfig _config;

        protected BlockingCollection<Domain> _domainsToCrawl = new BlockingCollection<Domain>();
        protected List<DomainCrawlResult> _crawledDomains = new List<DomainCrawlResult>();
        
        protected CancellationTokenSource _producerCancellation = new CancellationTokenSource();
        protected CancellationTokenSource _consumerCancellation = new CancellationTokenSource();

        public ProducerConsumerBulkCrawler(IDomainProducer domainProducer, IDomainConsumer domainConsumer, CrawlDaddyConfig config)
        {
            if (domainProducer == null)
                throw new ArgumentNullException("domainProducer");

            if (domainConsumer == null)
                throw new ArgumentNullException("domainConsumer");

            if (config == null)
                throw new ArgumentNullException("config");

            _domainProducer = domainProducer;
            _domainConsumer = domainConsumer;
            _config = config;
        }

        public virtual void Start()
        {
            //Start 1 producer thread
            Task.Factory.StartNew(() =>
            {
                RunProducer(_producerCancellation);
            }, _producerCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _logger.Debug("Started [1] producer thread");

            //Start X consumer threads
            for (int i = 0; i < _config.MaxConcurrentCrawls; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    RunConsumer(_consumerCancellation);
                }, _consumerCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            _logger.DebugFormat("Started [{0}] consumer threads", _config.MaxConcurrentCrawls);
        }

        public virtual void Stop()
        {
            _producerCancellation.Cancel();
            _consumerCancellation.Cancel();
        }

        protected void RunProducer(CancellationTokenSource cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _domainsToCrawl.CompleteAdding();
                    _logger.Debug("Domain producer stopping due to cancellation.");
                    cancellationToken.Token.ThrowIfCancellationRequested();
                }

                try
                {
                    _domainProducer.Produce(_domainsToCrawl, _crawledDomains);
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Domain producer [{0}] threw exception during Produce().", _domainProducer.ToString());
                    _logger.Error(e);
                }

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.Token.ThrowIfCancellationRequested();

                System.Threading.Thread.Sleep(1000);
            }
        }

        protected void RunConsumer(CancellationTokenSource cancellationToken)
        {
            foreach (Domain domain in _domainsToCrawl.GetConsumingEnumerable())
            {
                DomainCrawlResult domainCrawlResult = null;

                try
                {
                    domainCrawlResult = _domainConsumer.Consume(domain, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Domain consumer [{0}] threw exception during Consume().", _domainConsumer.ToString());
                    _logger.Error(e);
                    Stop();
                }

                if (domainCrawlResult != null)
                {
                    lock (_crawledDomains)
                    {
                        _crawledDomains.Add(domainCrawlResult);
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.Token.ThrowIfCancellationRequested();
            }
        }
    }
}

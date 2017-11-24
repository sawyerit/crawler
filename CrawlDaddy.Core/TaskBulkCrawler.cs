
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlDaddy.Core
{
    public class TaskBulkCrawler : IBulkCrawler
    {
        static ILog _logger = LogManager.GetLogger(typeof(ProducerConsumerBulkCrawler).FullName);
        protected IDomainProducer _domainProducer;
        protected IDomainConsumer _domainConsumer;
        protected CrawlDaddyConfig _config;

        protected BlockingCollection<Domain> _domainsToCrawl = new BlockingCollection<Domain>();
        protected List<DomainCrawlResult> _crawledDomains = new List<DomainCrawlResult>();
        
        protected CancellationTokenSource _producerCancellation = new CancellationTokenSource();
        protected CancellationTokenSource _consumerCancellation = new CancellationTokenSource();

        public TaskBulkCrawler(IDomainProducer domainProducer, IDomainConsumer domainConsumer, CrawlDaddyConfig config)
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

            //Start 1 consumer thread
            Task.Factory.StartNew(() =>
            {
                RunConsumers(_consumerCancellation);
            }, _consumerCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _logger.Debug("Started [1] consumer thread");
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

                System.Threading.Thread.Sleep(1000);
            }
        }

        protected void RunConsumers(CancellationTokenSource cancellationToken)
        {
            ManualResetEvent manualReset = new ManualResetEvent(true);
            Object locker = new Object();
            
            ParallelOptions options = new ParallelOptions
            {
                CancellationToken = cancellationToken.Token,
                MaxDegreeOfParallelism = (_config.MaxConcurrentCrawls > 0) ? _config.MaxConcurrentCrawls : System.Environment.ProcessorCount
            };
            Parallel.ForEach(_domainsToCrawl.GetConsumingEnumerable(), options, domain => 
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.Token.ThrowIfCancellationRequested();

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
            });        
        }
    }
}

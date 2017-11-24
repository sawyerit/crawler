using Abot.Crawler;
using Abot.Poco;
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlDaddy.Core
{
    public interface IDomainConsumer
    {
        DomainCrawlResult Consume(Domain domain, CancellationTokenSource cancellationToken);
    }

    public class DomainConsumer : IDomainConsumer
    {
        static ILog _logger = LogManager.GetLogger(typeof(DomainConsumer).FullName);
        static ILog _throughputLogger = LogManager.GetLogger("ThroughputLogger");

        IWebCrawlerFactory _crawlerFactory;
        IProcessorProvider _processorProvider;
        ProcessorContext _processorContext;
        CrawlDaddyConfig _config;

        public DomainConsumer(IWebCrawlerFactory crawlerFactory, IProcessorProvider processorProvider, ProcessorContext processorContext, CrawlDaddyConfig config)
        {
            if (crawlerFactory == null)
                throw new ArgumentNullException("crawlerFactory");

            if (processorProvider == null)
                throw new ArgumentNullException("processorProvider");

            if (processorContext == null)
                throw new ArgumentNullException("processorContext");

            if (config == null)
                throw new ArgumentNullException("config");

            _crawlerFactory = crawlerFactory;
            _processorProvider = processorProvider;
            _processorContext = processorContext;
            _config = config;
        }

        public DomainCrawlResult Consume(Domain domain, CancellationTokenSource cancellationToken)
        {
            if (domain == null)
                throw new ArgumentNullException("domain");

            if(cancellationToken == null)
                throw new ArgumentNullException("cancellationToken");

            IEnumerable<ICrawlProcessor> processors = _processorProvider.GetProcessors().ToList();//have to .ToList() since the deferred execution will cause a new instance of each processor to be created with every page
            IWebCrawler crawler = CreateCrawlerInstance();

            DomainCrawlResult domainCrawlResult = new DomainCrawlResult();
            domainCrawlResult.Domain = domain;
            try
            {
                crawler.CrawlBag.GoDaddyProcessorContext = new ProcessorContext
                {
                    Domain = domain,
                    PrimaryPersistenceProvider = _processorContext.PrimaryPersistenceProvider,
                    BackupPersistenceProvider = _processorContext.BackupPersistenceProvider,
                    CrawlProcessors = processors
                };

                domainCrawlResult.CrawlResult = crawler.Crawl(domain.Uri, cancellationToken);

                ProcessCrawledDomain(domainCrawlResult.CrawlResult.CrawlContext);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Exception occurred while crawling [{0}], error: [{1}]", domain.Uri.AbsoluteUri, ex.Message);
                domainCrawlResult.CrawlResult = new CrawlResult{ ErrorException = ex };

                _logger.ErrorFormat(errorMessage, ex);
                //TODO Statsg fatal error occurred during crawl
                StatsGLoggerAppender.LogItem(StatLogType.CrawlDaddy_FatalErrorOccured, _config);
            }

            LogCrawlResult(domainCrawlResult.CrawlResult);
            return domainCrawlResult;
        }

        private IWebCrawler CreateCrawlerInstance()
        {
            IWebCrawler crawler = _crawlerFactory.CreateInstance();
            crawler.PageCrawlCompleted += (s, e) => ProcessCrawledPage(e.CrawlContext, e.CrawledPage);

            return crawler;
        }

        private void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
        {
            if (!IsHttpStatusInConfig(crawledPage))
                return;

            if (!IsMimeTypesToProcessInConfig(crawledPage))
                return;

            int timeoutInMilliSecs = _config.MaxPageProcessorTimeInMilliSecs;
            IEnumerable<ICrawlProcessor> processors =
                crawlContext.CrawlBag.GoDaddyProcessorContext.CrawlProcessors;

            //Did not do a parallel.ForEach because it would spawn to many threads and cause heavy thrashing, most processors would take a up to 30 secs to finish
            foreach (ICrawlProcessor processor in processors)
            {
                Stopwatch timer = Stopwatch.StartNew();
                try
                {
                    processor.ProcessCrawledPage(crawlContext, crawledPage);
                    timer.Stop();

                    if (timer.ElapsedMilliseconds > timeoutInMilliSecs)
                        _logger.ErrorFormat(
                            "Crawled page processor [{0}] completed processing page [{1}] in [{2}] millisecs, which is above configuration value MaxPageProcessorTimeInMilliSecs",
                            processor.ToString(), crawledPage.Uri, timer.ElapsedMilliseconds);
                    else
                        _logger.DebugFormat(
                            "Crawled page processor [{0}] completed processing page [{1}] in [{2}] millisecs",
                            processor.ToString(), crawledPage.Uri, timer.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat(
                        "Crawled page processor [{0}] threw exception while processing page [{1}]",
                        processor.ToString(), crawledPage.Uri);
                    _logger.Error(e);
                }
                finally
                {
                    if (timer != null && timer.IsRunning)
                        timer.Stop();
                }
            }
        }

        private bool IsMimeTypesToProcessInConfig(CrawledPage crawledPage)
        {
            //Default to False, so if config value is missing, we crawl everything.
            bool result = false;

            if (_config.MimeTypesToProcess.Any() && crawledPage.HttpWebResponse != null)
            {
                //Since its a contains we use the ! to invert the bool so that it behaves properly and looks pretty above.
                result = !_config.MimeTypesToProcess.Any(mime => crawledPage.HttpWebResponse.ContentType.Contains(mime));
            }

            return !result;
        }

        private bool IsHttpStatusInConfig(CrawledPage crawledPage)
        {
            //Default to False, so if config value is missing, we crawl everything.
            bool result = false;

            if (_config.HttpStatusesToProcess.Any() && crawledPage.HttpWebResponse != null)
            {
                result = Array.IndexOf(_config.HttpStatusesToProcess, Convert.ToInt32(crawledPage.HttpWebResponse.StatusCode).ToString()) < 0;
            }

            return !result;
        }

        private void ProcessCrawledDomain(CrawlContext crawlContext)
        {
            int timeoutInMilliSecs = _config.MaxDomainProcessorTimeInMilliSecs;
            IEnumerable<ICrawlProcessor> processors = crawlContext.CrawlBag.GoDaddyProcessorContext.CrawlProcessors;
            Parallel.ForEach(processors, processor =>
            {
                Stopwatch timer = Stopwatch.StartNew();
                try
                {
                    processor.ProcessCrawledDomain(crawlContext);
                    timer.Stop();

                    if (timer.ElapsedMilliseconds > timeoutInMilliSecs)
                        _logger.ErrorFormat("Crawled domain processor [{0}] completed processing domain [{1}] in [{2}] millisecs, which is above configuration value MaxDomainProcessorTimeInMilliSecs", processor.ToString(), crawlContext.RootUri, timer.ElapsedMilliseconds);
                    else
                        _logger.DebugFormat("Crawled domain processor [{0}] completed processing domain [{1}] in [{2}] millisecs", processor.ToString(), crawlContext.RootUri, timer.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Crawled domain processor [{0}] threw exception while processing domain [{1}]", processor.ToString(), crawlContext.RootUri);
                    _logger.Error(e);
                }
                finally
                {
                    if (timer != null && timer.IsRunning)
                        timer.Stop();
                }
            });
        }

        //private void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
        //{
        //    IEnumerable<ICrawlProcessor> processors = crawlContext.CrawlBag.GoDaddyProcessorContext.CrawlProcessors;

        //    Task timedPageProcessorTask;
        //    CancellationTokenSource tokenSource;
        //    System.Timers.Timer timeoutTimer;
        //    Stopwatch timer;
        //    foreach (ICrawlProcessor processor in processors)
        //    {
        //        tokenSource = new CancellationTokenSource();
        //        timeoutTimer = new System.Timers.Timer(_config.MaxPageProcessorTimeInMilliSecs);
        //        timeoutTimer.Elapsed += (sender, e) =>
        //        {
        //            timeoutTimer.Stop();
        //            tokenSource.Cancel();
        //            _logger.ErrorFormat("Crawled page processor [{0}] timed out on page [{1}]. Max configured processing time is [{2}] millisecs.", processor.ToString(), crawledPage.Uri, _config.MaxPageProcessorTimeInMilliSecs);
        //        };

        //        try
        //        {
        //            timeoutTimer.Start();
        //            timer = Stopwatch.StartNew();
        //            timedPageProcessorTask = Task.Factory.StartNew(() => processor.ProcessCrawledPage(crawlContext, crawledPage), tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        //            timedPageProcessorTask.Wait(_config.MaxPageProcessorTimeInMilliSecs + 50);//wait an additional 50 millisecs give timeouttimer a chance to log
        //            timeoutTimer.Stop();
        //            timer.Stop();
        //            _logger.DebugFormat("Crawled page processor [{0}] completed processing page [{1}] in [{2}] millisecs.", processor.ToString(), crawledPage.Uri, timer.ElapsedMilliseconds);
        //        }
        //        catch (AggregateException ae)
        //        {
        //            timeoutTimer.Stop();
        //            _logger.ErrorFormat("Crawled page processor [{0}] threw exception while processing page [{1}]", processor.ToString(), crawledPage.Uri);
        //            _logger.Error(ae);
        //        }
        //    }
        //}

        //private void ProcessCrawledDomain(CrawlContext crawlContext)
        //{
        //    IEnumerable<ICrawlProcessor> processors = crawlContext.CrawlBag.GoDaddyProcessorContext.CrawlProcessors;

        //    Task timedPageProcessorTask;
        //    CancellationTokenSource tokenSource;
        //    System.Timers.Timer timeoutTimer;
        //    Stopwatch timer;
        //    foreach (ICrawlProcessor processor in processors)
        //    {
        //        tokenSource = new CancellationTokenSource();
        //        timeoutTimer = new System.Timers.Timer(_config.MaxDomainProcessorTimeInMilliSecs);
        //        timeoutTimer.Elapsed += (sender, e) =>
        //        {
        //            timeoutTimer.Stop();
        //            tokenSource.Cancel();
        //            _logger.ErrorFormat("Crawled domain processor [{0}] timed out on domain [{1}]. Max configured processing time is [{2}] millisecs.", processor.ToString(), crawlContext.RootUri, _config.MaxDomainProcessorTimeInMilliSecs);
        //        };

        //        try
        //        {
        //            timeoutTimer.Start();
        //            timer = Stopwatch.StartNew();
        //            timedPageProcessorTask = Task.Factory.StartNew(() => processor.ProcessCrawledDomain(crawlContext), tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        //            timedPageProcessorTask.Wait(_config.MaxDomainProcessorTimeInMilliSecs + 50);//wait an additional 50 millisecs give timeouttimer a chance to log
        //            timeoutTimer.Stop();
        //            timer.Stop();
        //            _logger.DebugFormat("Crawled domain processor [{0}] completed processing domain [{1}] in [{2}] millisecs.", processor.ToString(), crawlContext.RootUri, timer.ElapsedMilliseconds);
        //        }
        //        catch (AggregateException ae)
        //        {
        //            timeoutTimer.Stop();
        //            _logger.ErrorFormat("Crawled domain processor [{0}] threw exception while processing domain [{1}]", processor.ToString(), crawlContext.RootUri);
        //            _logger.Error(ae);
        //        }
        //    }
        //}

        private void LogCrawlResult(CrawlResult crawlResult)
        {
            if (crawlResult.ErrorOccurred)
            {
                _logger.ErrorFormat("Crawl for domain [{0}] failed after [{1}] seconds, crawled [{2}] pages", crawlResult.RootUri, crawlResult.Elapsed.TotalSeconds, crawlResult.CrawlContext != null ? crawlResult.CrawlContext.CrawledUrls.Count : 0);
                _logger.Error(crawlResult.ErrorException);
                //TODO Statsg error occurred during crawl
                StatsGLoggerAppender.LogItem(StatLogType.CrawlDaddy_ErrorOccuredDuringCrawl, _config);
            }
            else
            {
                _logger.InfoFormat("Crawl for domain [{0}] completed successfully in [{1}] seconds, crawled [{2}] pages", crawlResult.RootUri, crawlResult.Elapsed.TotalSeconds, crawlResult.CrawlContext.CrawledUrls.Count);
                _throughputLogger.InfoFormat("Crawl for domain [{0}] completed successfully in [{1}] seconds, crawled [{2}] pages", crawlResult.RootUri, crawlResult.Elapsed.TotalSeconds, crawlResult.CrawlContext.CrawledUrls.Count);
            }
        }
    }
}

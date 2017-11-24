using Abot.Crawler;
using Abot.Poco;
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlDaddy.Core
{
    public class BIDomainConsumer : IDomainConsumer
    {
        static ILog _logger = LogManager.GetLogger(typeof(BIDomainConsumer).FullName);
        static ILog _throughputLogger = LogManager.GetLogger("ThroughputLogger");
        static ILog _splunkLogger = LogManager.GetLogger("SplunkLogger");

        IWebCrawlerFactory _crawlerFactory;
        IProcessorProvider _processorProvider;
        ProcessorContext _processorContext;
        CrawlDaddyConfig _config;

        #region constructor

        public BIDomainConsumer(IWebCrawlerFactory crawlerFactory, IProcessorProvider processorProvider, ProcessorContext processorContext, CrawlDaddyConfig config)
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

        #endregion


        public DomainCrawlResult Consume(Domain domain, CancellationTokenSource cancellationToken)
        {
            if (domain == null)
                throw new ArgumentNullException("domain");

            if (cancellationToken == null)
                throw new ArgumentNullException("cancellationToken");

            LogCrawlBegin(domain);

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

                //call parkedpage processor.  if parked, no need to crawl anything
                ICrawlProcessor parkedProc = processors.FirstOrDefault(p => p.GetType().Name == "ParkedCrawlProcessor");
                CrawlContext cc = new CrawlContext { RootUri = domain.Uri, CrawlBag = crawler.CrawlBag };
                if (!Object.Equals(null, parkedProc))
                    parkedProc.ProcessCrawledDomain(cc);

                //if not parked or theres no parked processor, continue crawling the site
                if (Object.Equals(null, parkedProc) || !cc.CrawlBag.NoCrawl)
                {
                    domainCrawlResult.CrawlResult = crawler.Crawl(domain.Uri, cancellationToken);
                    ProcessCrawledDomain(domainCrawlResult.CrawlResult.CrawlContext);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Exception occurred while crawling [{0}], error: [{1}]", domain.Uri.AbsoluteUri, ex.Message);
                domainCrawlResult.CrawlResult = new CrawlResult { ErrorException = ex };

                _logger.ErrorFormat(errorMessage, ex);
            }

            if (!Object.Equals(null, domainCrawlResult.CrawlResult)) //could be null if we don't crawl it due to being a parked page or no A record
                LogCrawlResult(domainCrawlResult.CrawlResult);

            return domainCrawlResult;
        }


        #region event handlers

        private void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
        {
            if (!IsHttpStatusInConfig(crawledPage))
                return;

            if (!IsMimeTypesToProcessInConfig(crawledPage))
                return;

            int timeoutInMilliSecs = _config.MaxPageProcessorTimeInMilliSecs;
            IEnumerable<ICrawlProcessor> processors = crawlContext.CrawlBag.GoDaddyProcessorContext.CrawlProcessors;

            //Did not do a parallel.ForEach because it would spawn to many threads and cause heavy thrashing, most processors would take a up to 30 secs to finish
            foreach (ICrawlProcessor processor in processors)
            {
                Stopwatch timer = Stopwatch.StartNew();
                try
                {
                    _splunkLogger.DebugFormat(",{0},Processing crawled page,{1}", processor.GetType().Name, crawlContext.RootUri.ToString());
                    processor.ProcessCrawledPage(crawlContext, crawledPage);
                    timer.Stop();

                    if (timer.ElapsedMilliseconds > timeoutInMilliSecs)
                        _logger.ErrorFormat("Crawled page processor [{0}] completed processing page [{1}] in [{2}] millisecs, which is above configuration value MaxPageProcessorTimeInMilliSecs"
                                                                , processor.ToString()
                                                                , crawledPage.Uri
                                                                , timer.ElapsedMilliseconds);
                    else
                        _logger.DebugFormat("Crawled page processor [{0}] completed processing page [{1}] in [{2}] millisecs"
                                                                , processor.ToString()
                                                                , crawledPage.Uri
                                                                , timer.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Crawled page processor [{0}] threw exception while processing page [{1}]", processor.ToString(), crawledPage.Uri);
                    _logger.Error(e);
                }
                finally
                {
                    if (timer != null && timer.IsRunning)
                        timer.Stop();
                }
            }
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
                    _splunkLogger.DebugFormat(",{0},Processing crawled domain,{1}", processor.GetType().Name, crawlContext.RootUri.ToString());
                    if (processor.GetType().Name != "ParkedCrawlProcessor")
                    {
                        processor.ProcessCrawledDomain(crawlContext);
                        timer.Stop();

                        if (timer.ElapsedMilliseconds > timeoutInMilliSecs)
                            _logger.ErrorFormat("Crawled domain processor [{0}] completed processing domain [{1}] in [{2}] millisecs, which is above configuration value MaxDomainProcessorTimeInMilliSecs"
                                                                    , processor.ToString()
                                                                    , crawlContext.RootUri
                                                                    , timer.ElapsedMilliseconds);
                        else
                            _logger.DebugFormat("Crawled domain processor [{0}] completed processing domain [{1}] in [{2}] millisecs"
                                                                    , processor.ToString()
                                                                    , crawlContext.RootUri
                                                                    , timer.ElapsedMilliseconds);
                    }
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

        #endregion


        #region helper methods

        private IWebCrawler CreateCrawlerInstance()
        {
            IWebCrawler crawler = _crawlerFactory.CreateInstance();
            crawler.PageCrawlCompleted += (s, e) => ProcessCrawledPage(e.CrawlContext, e.CrawledPage);

            return crawler;
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

        private void LogCrawlBegin(Domain domain)
        {
            _splunkLogger.InfoFormat("{0}", domain.Uri.ToString());
        }

        private void LogCrawlResult(CrawlResult crawlResult)
        {
            if (crawlResult.ErrorOccurred)
            {
                _logger.ErrorFormat("Crawl for domain [{0}] failed after [{1}] seconds, crawled [{2}] pages", crawlResult.RootUri, crawlResult.Elapsed.TotalSeconds, crawlResult.CrawlContext != null ? crawlResult.CrawlContext.CrawledUrls.Count : 0);
                _logger.Error(crawlResult.ErrorException);
            }
            else
            {
                //call api to log a complete (for monitoring)
                PostCompleteNotification(crawlResult);
                _logger.InfoFormat("Crawl for domain [{0}] completed successfully in [{1}] seconds, crawled [{2}] pages", crawlResult.RootUri, crawlResult.Elapsed.TotalSeconds, crawlResult.CrawlContext.CrawledUrls.Count);
                _throughputLogger.InfoFormat("Crawl for domain [{0}] completed successfully in [{1}] seconds, crawled [{2}] pages", crawlResult.RootUri, crawlResult.Elapsed.TotalSeconds, crawlResult.CrawlContext.CrawledUrls.Count);
            }
        }

        private void PostCompleteNotification(CrawlResult crawlResult)
        {
            HttpWebRequest requestPost = (HttpWebRequest)WebRequest.Create(_config.DomainCompleteAPI);
            requestPost.Method = "POST";
            requestPost.ContentType = "application/json";
            requestPost.UseDefaultCredentials = true;

            string requestData = string.Format("{{ Name : \"{0}\", DomainComplete : {1}, Pages : {2} }}", crawlResult.RootUri.Host, crawlResult.Elapsed.TotalSeconds, crawlResult.CrawlContext.CrawledUrls.Count);

            byte[] data = Encoding.UTF8.GetBytes(requestData);

            using (Stream dataStream = requestPost.GetRequestStream())
                dataStream.Write(data, 0, data.Length);

            HttpWebResponse response = requestPost.GetResponse() as HttpWebResponse;
        }

        #endregion




    }

}

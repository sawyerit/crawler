using Abot.Poco;
using CrawlDaddy.Core;

namespace CrawlDaddy.Plugin.Processor
{
    /// <summary>
    /// Only needs to look for a signature for the entire domain. So if a signature is found on a page then there is no need to process any future pages
    /// </summary>
    public abstract class OneHitPerDomainCrawlProcessor : CrawlProcessorBase, ICrawlProcessor
    {
        bool isAlreadyFoundOnSite = false;
        object locker = new object();
        
        public void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
        {
            ProcessorResult result;

            bool isFound = false;
            lock (locker)
            {
                if (isAlreadyFoundOnSite)
                    return;

                result = ProcessPage(crawlContext, crawledPage);
                isFound = isAlreadyFoundOnSite = result.IsAHit;
            }

            if (isFound)
                PageSave(crawlContext, crawledPage, result);
        }

        public void ProcessCrawledDomain(CrawlContext crawlContext)
        {
            //do nothing
        }

        protected abstract ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage);
    }
}

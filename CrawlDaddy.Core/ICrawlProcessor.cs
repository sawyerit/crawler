using Abot.Poco;

namespace CrawlDaddy.Core
{
    public interface ICrawlProcessor
    {
        /// <summary>
        /// Called when a page has been crawled. MUST BE THREAD SAFE.
        /// </summary>
        /// <param name="crawlContext">The crawl context of the entire crawl</param>
        /// <param name="crawledPage">The crawled page</param>
        void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage);

        /// <summary>
        /// Called when the crawl has completed. MUST BE THREAD SAFE.
        /// </summary>
        /// <param name="crawlContext">The crawl context of the entire crawl</param>
        void ProcessCrawledDomain(CrawlContext crawlContext);
    }
}

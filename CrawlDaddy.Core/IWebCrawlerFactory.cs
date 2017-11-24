
using Abot.Crawler;
namespace CrawlDaddy.Core
{
    public interface IWebCrawlerFactory
    {
        IWebCrawler CreateInstance();
    }
}

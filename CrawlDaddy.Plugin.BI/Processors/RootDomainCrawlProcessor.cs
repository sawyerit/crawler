using Abot.Poco;
using CrawlDaddy.Core;

namespace CrawlDaddy.Plugin.Processor
{
	/// <summary>
	/// Only needs to look for a signature on the root page. 
	/// </summary>
	public abstract class RootDomainCrawlProcessor : CrawlProcessorBase, ICrawlProcessor
	{
		bool isAlreadyFoundOnSite = false;
		object locker = new object();

		public void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			ProcessorResult result;

			bool isFound = false;

			if (!crawledPage.IsRoot)
				return;

			result = ProcessPage(crawlContext, crawledPage);

			if (result.IsAHit)
				PageSave(crawlContext, crawledPage, result);
		}

		public void ProcessCrawledDomain(CrawlContext crawlContext)
		{
			//do nothing
		}

		protected abstract ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage);
	}
}

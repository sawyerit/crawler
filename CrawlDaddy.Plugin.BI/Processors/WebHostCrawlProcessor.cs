using System;
using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Plugin.Processor;
using Drone.API.Dig;
using log4net;

namespace CrawlDaddy.Plugin.BI.Processors
{
	public class WebHostCrawlProcessor : CrawlProcessorBase, ICrawlProcessor
	{
		static ILog _logger = LogManager.GetLogger(typeof(WebHostCrawlProcessor).FullName);
		const int ATTRIB_TYPE_ID = 3;

		public void ProcessCrawledDomain(CrawlContext crawlContext)
		{
			string webhost = string.Empty;

			try
			{
				webhost = Dig.Instance.GetWebHostName(crawlContext.RootUri.DnsSafeHost);
			}
			catch (Exception e)
			{
				_logger.ErrorFormat("Exception occurred getting webhost name for [{0}]", crawlContext.RootUri.DnsSafeHost, e);
			}

			ProcessorResult result = new ProcessorResult { UniqueAttributeId = ATTRIB_TYPE_ID }; //mask
			result.IsAHit = webhost != "None";
			result.Attributes.Add(ATTRIB_TYPE_ID.ToString(), webhost);

			if (result.IsAHit)
				DomainSave(crawlContext, result);

		}

		public void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			//do nothing, this processor only happens on domain crawl
		}
	}
}

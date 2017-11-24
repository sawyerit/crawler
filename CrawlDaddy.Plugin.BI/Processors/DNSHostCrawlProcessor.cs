using System;
using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Plugin.Processor;
using Drone.API.Dig;
using log4net;

namespace CrawlDaddy.Plugin.BI.Processors
{
	public class DNSHostCrawlProcessor : CrawlProcessorBase, ICrawlProcessor
	{
		static ILog _logger = LogManager.GetLogger(typeof(WebHostCrawlProcessor).FullName);
		const int ATTRIB_TYPE_ID = 5;

		public void ProcessCrawledDomain(CrawlContext crawlContext)
		{
			string dnshost = string.Empty;

			try
			{
				dnshost = Dig.Instance.GetDNSHostName(crawlContext.RootUri.DnsSafeHost);
			}
			catch (Exception e)
			{
				_logger.ErrorFormat("Exception occurred getting dnshost name for [{0}]", crawlContext.RootUri.DnsSafeHost, e);
			}

			ProcessorResult result = new ProcessorResult { UniqueAttributeId = ATTRIB_TYPE_ID }; 
			result.IsAHit = dnshost != "None";
			result.Attributes.Add(ATTRIB_TYPE_ID.ToString(), dnshost);

			if (result.IsAHit)
				DomainSave(crawlContext, result);

		}

		public void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			//do nothing, this processor only happens on domain crawl
		}
	}
}

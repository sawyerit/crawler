using System;
using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Plugin.Processor;
using Drone.API.Dig;
using Drone.API.Dig.Ssl;
using log4net;

namespace CrawlDaddy.Plugin.BI.Processors
{
	public class SSLCrawlProcessor : CrawlProcessorBase, ICrawlProcessor
	{
		static ILog _logger = LogManager.GetLogger(typeof(WebHostCrawlProcessor).FullName);

		public void ProcessCrawledDomain(CrawlContext crawlContext)
		{
			SSLCert cert = null;

			try
			{
				cert = Dig.Instance.GetSSLVerification(crawlContext.RootUri.DnsSafeHost);
			}
			catch (Exception e)
			{
				_logger.ErrorFormat("Exception occurred getting ssl info for [{0}]", crawlContext.RootUri.DnsSafeHost, e);
			}

			if (cert != null)
			{
				if (cert.FixedName != "None")
				{
					ProcessorResult result = new ProcessorResult { UniqueAttributeId = 7 };
					result.Attributes.Add("7", cert.FixedName);

					DomainSave(crawlContext, result);
				}

				if (cert.SubjectType != "None")
				{
					ProcessorResult result = new ProcessorResult { UniqueAttributeId = 8 };
					result.Attributes.Add("8", cert.SubjectType);

					DomainSave(crawlContext, result);
				}
			}
		}

		public void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			//do nothing, this processor only happens on domain crawl
		}
	}
}

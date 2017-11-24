using Abot.Poco;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using CrawlDaddy.Plugin.Processor;
using Drone.API.MarketAnalysis;
using log4net;

namespace CrawlDaddy.Plugin.BI.Processors
{
	public class ShoppingCartCrawlProcessor : OneHitPerDomainCrawlProcessor
	{
        static ILog _logger = LogManager.GetLogger(typeof(ShoppingCartCrawlProcessor).FullName);
		const int ATTRIB_TYPE_ID = 13;

		protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			string cartName = string.Empty;

			try
			{
				cartName = MarketShareEngine.Instance.ShoppingCart(crawledPage.Uri.DnsSafeHost, crawledPage.HtmlDocument, crawledPage.HttpWebResponse, crawledPage.IsRoot);
			}
			catch (System.Exception e)
			{				
				_logger.ErrorFormat("Exception occurred getting shoppingcart for [{0}]", crawlContext.RootUri.DnsSafeHost, e);
			}
			
			ProcessorResult result = new ProcessorResult { UniqueAttributeId = ATTRIB_TYPE_ID };
			result.IsAHit = (cartName != "None");
			result.Attributes.Add(ATTRIB_TYPE_ID.ToString(), cartName);

			return result;
		}
	}
}

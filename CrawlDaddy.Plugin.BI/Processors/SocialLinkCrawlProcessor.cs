using Abot.Poco;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using CrawlDaddy.Plugin.Processor;
using CsQuery.Implementation;
using System.Text;

namespace CrawlDaddy.Plugin.BI.Processors
{
	public class SocialLinkCrawlProcessor : RootDomainCrawlProcessor
	{
		protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			ProcessorResult result = new ProcessorResult { UniqueAttributeId = 15 };

			var tweetTags = crawledPage.CsQueryDocument.Select("a[href*='twitter.com/']");
			var faceTags = crawledPage.CsQueryDocument.Select("a[href*='facebook.com/']");
			var linkedinTags = crawledPage.CsQueryDocument.Select("a[href^='linkedin.com/']");
			var youTubeTags = crawledPage.CsQueryDocument.Select("a[href^='youtube.com/']");
			var flickrTags = crawledPage.CsQueryDocument.Select("a[href^='flickr.com/']");

			if (tweetTags.Length > 0)
			{
				foreach (var tag in tweetTags)
				{
					string href = ((HtmlAnchorElement)tag).Href.ToLower();
					if (ValidateAndCleanTwitter(ref href))
					{
						result.Attributes.Add("twitter", href);
						break;
					}
				}
			}

			if (faceTags.Length > 0)
				result.Attributes.Add("facebook", ((HtmlAnchorElement)faceTags[0]).Href);

			if (linkedinTags.Length > 0)
				result.Attributes.Add("linkedin", ((HtmlAnchorElement)linkedinTags[0]).Href);

			if (youTubeTags.Length > 0)
				result.Attributes.Add("youtube", ((HtmlAnchorElement)youTubeTags[0]).Href);

			if (flickrTags.Length > 0)
				result.Attributes.Add("flickr", ((HtmlAnchorElement)flickrTags[0]).Href);

			result.IsAHit = result.Attributes.Count > 0;

			return result;
		}

		private bool ValidateAndCleanTwitter(ref string href)
		{
			foreach (string invTag in ConfigManager.InvalidTwitterTags)
			{
				if (href.Contains(invTag))
				{
					return false;
				}
			}

			StringBuilder sb = new StringBuilder(href);

			sb.Replace("https", string.Empty);
			sb.Replace("http", string.Empty);
			sb.Replace("www", string.Empty);
			sb.Replace("twitter.com", string.Empty);
			sb.Replace("#", string.Empty);
			sb.Replace("!", string.Empty);
			sb.Replace("@", string.Empty);
			sb.Replace("/", string.Empty);
			sb.Replace(".", string.Empty);
			sb.Replace(":", string.Empty);

			href = sb.ToString();
			
			return href != "";
		}
	}
}

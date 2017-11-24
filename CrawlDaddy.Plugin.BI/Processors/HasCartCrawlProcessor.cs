using Abot.Poco;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using CrawlDaddy.Plugin.Processor;
using CsQuery.Implementation;
using System;
using System.Collections.Generic;

namespace CrawlDaddy.Plugin.BI.Processors
{
	public class HasCartCrawlProcessor : OneHitPerDomainCrawlProcessor
	{
		List<KeyValuePair<string, string>> nodeQueryList = new List<KeyValuePair<string, string>>();

		protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			nodeQueryList.Add(new KeyValuePair<string, string>("cart","//a[contains(@href, 'cart')]"));
			nodeQueryList.Add(new KeyValuePair<string, string>("shoppingcart","//a[contains(@href, 'shoppingcart')]"));
			nodeQueryList.Add(new KeyValuePair<string, string>("checkout","//a[contains(@href, 'checkout')]"));

			ProcessorResult result = new ProcessorResult { UniqueAttributeId = 16 };

			result.IsAHit = FindTags(crawledPage, crawlContext.RootUri.DnsSafeHost.ToLower());

			if (result.IsAHit)
				result.Attributes.Add(result.UniqueAttributeId.ToString(), "true");

			return result;
		}

		private bool FindTags(CrawledPage crawledPage, string domain)
		{
			HtmlNodeCollection tags;
			foreach (KeyValuePair<string, string> qry in nodeQueryList)
			{
				tags = crawledPage.HtmlDocument.DocumentNode.SelectNodes(qry.Value);

				if (!Object.Equals(null, tags))
				{
					foreach (var item in tags)
					{
						string href = item.Attributes["href"].Value.ToLower();

						if (Regex.IsMatch(href, "(?:[^a-z]|^)" + qry.Key + "(?:[^a-z]|$)", RegexOptions.IgnoreCase))
						{
							if (!IsAbsoluteUrl(href))
							{
								//if its internal, its good.
								return true;
							}
							else
							{
								//if its on the same domain, its good
								return Uri.Compare(new Uri("http://" + domain), new Uri(href), UriComponents.Host, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0;
							}
						}
					}
				}
			}

			return false;
		}

		private bool IsAbsoluteUrl(string url)
		{
			Uri result;
			return Uri.TryCreate(url, UriKind.Absolute, out result);
		}

	}
}

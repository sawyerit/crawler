using Abot.Poco;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using CrawlDaddy.Plugin.Processor;
using CsQuery.Implementation;
using System;
using System.Collections.Generic;
using log4net;

namespace CrawlDaddy.Plugin.BI.Processors
{
	public class HasLoginCrawlProcessor : OneHitPerDomainCrawlProcessor
	{
		List<KeyValuePair<string, string>> nodeQueryList = new List<KeyValuePair<string, string>>();

		protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			nodeQueryList.Add(new KeyValuePair<string, string>("login", "//a[contains(@href, 'login')]"));
			nodeQueryList.Add(new KeyValuePair<string, string>("signin", "//a[contains(@href, 'signin')]"));
			
			ProcessorResult result = new ProcessorResult { UniqueAttributeId = 17 };

			//<input type="password"
			var pwdInputs = crawledPage.CsQueryDocument.Select("input[type='password']");
			if (pwdInputs.Length > 0)
				result.IsAHit = true;

			//check links
			if (!result.IsAHit)
				result.IsAHit = FindTags(crawledPage, crawlContext.RootUri.DnsSafeHost.ToLower());

			//if we found it, set it
			if (result.IsAHit)
				result.Attributes.Add(result.UniqueAttributeId.ToString(), "true");

			return result;
		}

		private bool IsAbsoluteUrl(string url)
		{
			Uri result;
			return Uri.TryCreate(url, UriKind.Absolute, out result);
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

						if (!href.Contains("wp-login.php") && Regex.IsMatch(href, "(?:[^a-z]|^)" + qry.Key + "(?:[^a-z]|$)", RegexOptions.IgnoreCase))
						{
							if (!IsAbsoluteUrl(href))
							{
								//if its internal, its good
								return true;
							}
							else
							{
								//if its on the same domain, its good
								return (Uri.Compare(new Uri("http://" + domain), new Uri(href), UriComponents.Host, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0);
							}
						}
					}
				}
			}

			return false;
		}
	}
}

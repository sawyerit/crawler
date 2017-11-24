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
	public class PIIFormCrawlProcessor : OneHitPerDomainCrawlProcessor
	{
		protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
		{
			ProcessorResult result = new ProcessorResult { UniqueAttributeId = 16 };

			result.IsAHit = FindFormElements(crawledPage);

			if (result.IsAHit)
				result.Attributes.Add(result.UniqueAttributeId.ToString(), "true");

			return result;
		}

		private bool FindFormElements(CrawledPage crawledPage)
		{
			//HtmlNodeCollection tags;

			//tags = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//input[@type=\"text\" and contains(@id, 'card')]"); 

			var tags = crawledPage.CsQueryDocument.Select("input[type=text][id*=card]");

			if (!Object.Equals(null, tags) && tags.Length > 0)
			{
				return true;
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

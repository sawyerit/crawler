using Abot.Poco;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrawlDaddy.Plugin.Processor.SearchEngineVisibility
{
    public class WordPressCrawlProcessor : OneHitPerDomainCrawlProcessor
    {
        private static string regexPattern = @"(<meta\s*name\s*=\s*(\""|')generator(\""|')\s*content\s*=\s*(\""|')WordPress[^>]*>)";
        Regex wordPressPattern = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
        {
            ProcessorResult result = new ProcessorResult
                {
                    UniqueAttributeId = 222
                };

            Match regexResult = wordPressPattern.Match(crawledPage.RawContent);
            if (regexResult.Success)
            {
                result.Attributes.Add("siteBuilder", "BlogWordPress");
                result.IsAHit = true;
                return result;
            }

            HtmlNodeCollection listhref = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//a[@href]") ?? new HtmlNodeCollection(null);

            if (listhref.Select(node => node.GetAttributeValue("href", "")).Any(content => content.Contains("wordpress.org")))
            {
                result.Attributes.Add("siteBuilder", "BlogWordPress");
                result.IsAHit = true;
                return result;
            }

            return result;
        }
    }
}

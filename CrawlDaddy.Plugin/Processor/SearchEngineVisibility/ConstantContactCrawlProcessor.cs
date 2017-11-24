using Abot.Poco;
using System.Text.RegularExpressions;

namespace CrawlDaddy.Plugin.Processor.SearchEngineVisibility
{
    public class ConstantContactCrawlProcessor : OneHitPerDomainCrawlProcessor
    {
        Regex constantContactPattern = new Regex("(constant[_ \\-]?contact)+|(ccoptin)+|(cc_newsletter)+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
        {
            ProcessorResult result = new ProcessorResult();
            result.UniqueAttributeId = 19;

            Match regexResult = constantContactPattern.Match(crawledPage.RawContent);
            if (regexResult.Success)
            {
							result.Attributes.Add(result.UniqueAttributeId.ToString(), "ConstantContact");
                result.IsAHit = true;
            }

            return result;
        }
    }
}

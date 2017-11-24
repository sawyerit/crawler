using CrawlDaddy.Core.Poco;
using System.Collections.Generic;

namespace CrawlDaddy.Core
{
    public interface IDomainProvider
    {
        IEnumerable<Domain> GetDomainsToCrawl(int limit, string lockIdentifier);

        void ReportCrawledDomains(IEnumerable<Domain> crawledDomains);
    }
}

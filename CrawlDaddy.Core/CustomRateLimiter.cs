using CrawlDaddy.Core.Poco;
using System;

namespace CrawlDaddy.Core
{
    public class CustomRateLimiter : Abot.Core.RateLimiter
    {
        public CustomRateLimiter(CrawlDaddyConfig config)
            : base(1, TimeSpan.FromSeconds(config.MinTimeBetweenDomainRetrievalRequestsInSecs))
        {

        }
    }
}

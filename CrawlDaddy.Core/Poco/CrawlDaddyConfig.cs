

namespace CrawlDaddy.Core.Poco
{
    public class CrawlDaddyConfig
    {
        public int MaxConcurrentCrawls { get; set; }

        public int MaxDomainRetrievalCount { get; set; }

        public int MinTimeBetweenDomainRetrievalRequestsInSecs { get; set; }

        public int MaxPageProcessorTimeInMilliSecs { get; set; }

        public int MaxDomainProcessorTimeInMilliSecs { get; set; }

        public int ConsumerThreadInstanceCheckInMillisecs { get; set; }

        public int ConsumerThreadNoWorkToDoMax { get; set; }

        public int ConsumerThreadStepUp { get; set; }

        public string[] MimeTypesToProcess { get; set; }

        public string[] HttpStatusesToProcess { get; set; }

        public bool StatsGEnabled { get; set; }

        public string StatsGHostConfigName { get; set; }

        public int StatsGPortConfigName { get; set; }

        public string CrawlDaddyAuthIdForSEV { get; set; }

        public int CrawlDaddyAppIdForSEV { get; set; }

        public string DomainCompleteAPI { get; set; }
    }
}

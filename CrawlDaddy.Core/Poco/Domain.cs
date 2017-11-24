using System;
using System.Collections.Generic;

namespace CrawlDaddy.Core.Poco
{
    [Serializable]
    public class Domain
    {
        public Uri Uri { get; set; }

        public long DomainId { get; set; }

        public string ShopperId { get; set; }

        /// <summary>
        /// History of what was already saved for this domain
        /// </summary>
        public Dictionary<string, string> DomainAttributes { get; set; }
    }
}

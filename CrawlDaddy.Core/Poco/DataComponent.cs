using System;
using System.Collections.Generic;

namespace CrawlDaddy.Core.Poco
{
    [Serializable]
    public class DataComponent
    {
        /// <summary>
        /// Customers unique shopper id
        /// </summary>
        public string ShopperId { get; set; }

        /// <summary>
        /// BI Team's domain id. 
        /// </summary>
        public long DomainId { get; set; }

        /// <summary>
        /// BI Team's Unique attribute id
        /// </summary>
        public int AttributeId { get; set; }

        /// <summary>
        /// Attribute key value pairs to associate with this domain
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// The uri of the domain
        /// </summary>
        public Uri DomainUri { get; set; }

        /// <summary>
        /// The uri the attribute was found on
        /// </summary>
        public Uri FoundOnUri { get; set; }
    }
}

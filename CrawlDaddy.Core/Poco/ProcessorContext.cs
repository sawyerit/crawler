using System.Collections.Generic;

namespace CrawlDaddy.Core.Poco
{
    public class ProcessorContext
    {
        /// <summary>
        /// The domain object represents the site being crawled
        /// </summary>
        public Domain Domain { get; set; }

        /// <summary>
        /// The primary persistence provider
        /// </summary>
        public IPersistenceProvider PrimaryPersistenceProvider { get; set; }

        /// <summary>
        /// The backup persistence provider
        /// </summary>
        public IPersistenceProvider BackupPersistenceProvider { get; set; }

        /// <summary>
        /// The processors that will be called on every page crawl completed event and site crawl completed event
        /// </summary>
        public IEnumerable<ICrawlProcessor> CrawlProcessors { get; set; }
    }
}

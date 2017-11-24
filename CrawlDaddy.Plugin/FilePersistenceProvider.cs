using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlDaddy.Plugin
{
    public class FilePersistenceProvider : IPersistenceProvider
    {
        static ILog _logger = LogManager.GetLogger(typeof(FilePersistenceProvider).FullName);

        public FilePersistenceProvider()
        {

        }

        public FilePersistenceProvider(ILog logger)
        {
            if (logger != null)
                _logger = logger;
        }

        public void Save(DataComponent dataComponent)
        {
            if (dataComponent == null)
                throw new ArgumentNullException("dataComponent");

            string domainUriValue = dataComponent.DomainUri == null ? "" : dataComponent.DomainUri.AbsoluteUri;
            string foundOnPageUriValue = dataComponent.FoundOnUri == null ? "" : dataComponent.FoundOnUri.AbsoluteUri;

            string message = string.Format(
                "shopperId={0}, domainId={1}, attributeId={2}, domain={3}, foundOnPage={4}, {5}", 
                dataComponent.ShopperId,
                dataComponent.DomainId, 
                dataComponent.AttributeId, 
                domainUriValue, 
                foundOnPageUriValue,
                GetPrintedAttributes(dataComponent.Attributes));

            _logger.Warn(message);
        }

        private string GetPrintedAttributes(Dictionary<string, string> attributes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string key in attributes.Keys)
            {
                builder.Append(key);
                builder.Append("=");
                builder.Append(attributes[key]);
                builder.Append(" ");
            }

            return builder.ToString().TrimEnd();
        }
    }
}

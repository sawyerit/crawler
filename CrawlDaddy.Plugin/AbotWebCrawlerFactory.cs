using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using CrawlDaddy.Core;
using System;

namespace CrawlDaddy.Plugin
{
    public class AbotWebCrawlerFactory : IWebCrawlerFactory
    {
        CrawlConfiguration _abotConfig;

        public AbotWebCrawlerFactory()
        {
            _abotConfig = GetCrawlConfigurationFromConfigFile();
        }

        public IWebCrawler CreateInstance()
        {   
            return new PoliteWebCrawler(_abotConfig, null, null, null, null, null, null, null, null);
        }

        private CrawlConfiguration GetCrawlConfigurationFromConfigFile()
        {
            AbotConfigurationSectionHandler configFromFile = AbotConfigurationSectionHandler.LoadFromXml();

            if (configFromFile == null)
                throw new ApplicationException("Config section \"abot\" was NOT found");

            return configFromFile.Convert();
        }
    }
}

using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using System;

namespace CrawlDaddy.Plugin.Debug
{
    public class ExceptionOnSavePersistenceProvider : IPersistenceProvider
    {
        public void Save(DataComponent dataComponent)
        {
            throw new Exception("Oh no, problem saving to persistence provider");
        }
    }
}

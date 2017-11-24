using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;

namespace CrawlDaddy.Plugin.Debug
{
    public class DoNothingPersistenceProvider : IPersistenceProvider
    {
        public void Save(DataComponent dataComponent)
        {
            //do nothing
        }
    }
}

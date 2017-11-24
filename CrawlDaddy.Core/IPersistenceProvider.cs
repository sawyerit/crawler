
using CrawlDaddy.Core.Poco;
namespace CrawlDaddy.Core
{
    public interface IPersistenceProvider
    {
        void Save(DataComponent dataComponent);
    }
}

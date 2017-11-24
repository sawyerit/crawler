using System.Collections.Generic;

namespace CrawlDaddy.Core
{
    public interface IProcessorProvider
    {
        IEnumerable<ICrawlProcessor> GetProcessors();
    }
}

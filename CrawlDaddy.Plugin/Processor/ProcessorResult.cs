using System.Collections.Generic;

namespace CrawlDaddy.Plugin.Processor
{
    public class ProcessorResult
    {
        public ProcessorResult()
        {
            Attributes = new Dictionary<string, string>();
        }

        public int UniqueAttributeId { get; set; }

        public bool IsAHit { get; set; }

        public Dictionary<string, string> Attributes { get; set;  }
    }
}

using System.Collections.Generic;

namespace CrawlDaddy.Plugin.Processor.SearchEngineMarketing
{
    public class SemList
    {
        public Dictionary<string, VerticalKeyword> VerticalKeywords { get; set; }
        public Dictionary<string, string> LocalKeywords { get; set; }
        public Dictionary<string, string> Cities { get; set; }
        public Dictionary<string, string> States { get; set; }

        public SemList()
        {
            VerticalKeywords = new Dictionary<string, VerticalKeyword>();
            LocalKeywords = new Dictionary<string, string>();
            Cities = new Dictionary<string, string>();
            States = new Dictionary<string, string>();
        }
    }
}

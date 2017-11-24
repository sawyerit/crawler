using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrawlDaddy.Plugin.Debug
{
    public class CsvDomainProvider : IDomainProvider
    {
        static ILog _logger = LogManager.GetLogger(typeof(CsvDomainProvider).FullName);
        string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "DomainsToCrawl.csv");
        int _expectedCsvCount = 3;
        Queue<Domain> _domains;

        public IEnumerable<Domain> GetDomainsToCrawl(int limit, string lockIdentifier)
        {
            if (_domains == null)
            {
                if (!File.Exists(_filePath))
                    throw new ApplicationException("Cannot find file " + _filePath);

                _domains = new Queue<Domain>();
                LoadFromFile();
            }

            List<Domain> domainsToReturn = new List<Domain>();
            for (int i = 0; i < limit; i++)
            {
                if (_domains.Count > 0)
                    domainsToReturn.Add(_domains.Dequeue());
            }

            return domainsToReturn;
        }

        public void ReportCrawledDomains(IEnumerable<Domain> crawledDomains)
        {
            //do nothing
        }

        private void LoadFromFile()
        {
            //List<Domain> domains = new List<Domain>();

            string[] allLines = File.ReadAllLines(_filePath);

            string[] csvElement;
            int currentLine = 0;
            foreach (string line in allLines)
            {
                currentLine++;

                if (line.StartsWith("//"))
                    continue;

                string trimmedLine = line.TrimEnd(',');

                csvElement = trimmedLine.Split(',');

                if (csvElement.Length != _expectedCsvCount)
                {
                    _logger.Error(string.Format("{0} has more/less than the expected {1} values on line {2}.", _filePath, _expectedCsvCount, currentLine));
                    continue;
                }


                try
                {
                    _domains.Enqueue(new Domain { ShopperId = csvElement[0], DomainId = Convert.ToInt64(csvElement[1]), Uri = new Uri("http://" + csvElement[2].Trim()) });
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Encountered an issue while attempting to parse data in file {0}, line {1}.", _filePath, currentLine);
                    _logger.Error(e);
                }
            }
        }
    }
}

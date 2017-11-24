using Commoner.Core.Unity;
using CrawlDaddy.Core;
using log4net;
using Microsoft.Practices.Unity;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace CrawlDaddy.WinService
{
    public partial class CrawlService : ServiceBase
    {
        static ILog _logger = LogManager.GetLogger(typeof(CrawlService).FullName);
        IBulkCrawler _bulkCrawler;

        public CrawlService()
            : this(null)
        {
        }

        public CrawlService(IBulkCrawler bulkCrawler)
        {
            InitializeComponent();

            if (bulkCrawler == null)
            {
                Factory.Configure(new ConfigFileFactoryRegistrar());
                _bulkCrawler = Factory.Container.Resolve<IBulkCrawler>();
            }
            else
            {
                _bulkCrawler = bulkCrawler;
            }
        }

        static void Main(string[] args)
        {
            CrawlService crawlService = new CrawlService();
            if (Environment.UserInteractive)
                RunAsConsoleApp(args, crawlService);
            else
                RunAsWindowsService(crawlService);
        }

        static void RunAsConsoleApp(string[] args, CrawlService crawlService)
        {
            Console.WriteLine("Hit enter at any time to stop the program");
            crawlService.OnStart(args);
            Console.Read();
            crawlService.OnStop();
        }

        static void RunAsWindowsService(CrawlService crawlService)
        {
            ServiceBase.Run(new ServiceBase[] { crawlService });
        }

        protected override void OnStart(string[] args)
        {
					Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1");
					Environment.SetEnvironmentVariable("COR_PROFILER", "{44a86cad-f7ee-429c-83eb-f3cde3b87b70}");
					Environment.SetEnvironmentVariable("COR_LINE_PROFILING", "1");
					Environment.SetEnvironmentVariable("COR_INTERACTION_PROFILING", "0");
					Environment.SetEnvironmentVariable("COR_GC_PROFILING", "1");
					Environment.SetEnvironmentVariable("CORCLR_ENABLE_PROFILING", "1");
					Environment.SetEnvironmentVariable("CORCLR_PROFILER", "{44a86cad-f7ee-429c-83eb-f3cde3b87b70}");

            Task task = Task.Factory.StartNew(_bulkCrawler.Start);
            _logger.Info("CrawlDaddy Service Started");
        }

        protected override void OnStop()
        {
            _bulkCrawler.Stop();
            _logger.Info("CrawlDaddy Service Stopped");
        }
    }
}

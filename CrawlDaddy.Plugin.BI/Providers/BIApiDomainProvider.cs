using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using log4net;

namespace CrawlDaddy.Plugin.BI.Providers
{
	public class BIApiDomainProvider : IDomainProvider
	{
		Queue<Domain> _domains;
		static ILog _logger = LogManager.GetLogger(typeof(BIApiDomainProvider).FullName);
		static string _providerAPI = ConfigManager.DomainUrl;

		public IEnumerable<Domain> GetDomainsToCrawl(int limit, string lockIdentifier)
		{
			List<Domain> domainsToReturn = new List<Domain>();

			if (_domains == null)
			{
				string result = string.Empty;
				HttpWebRequest requestGet = (HttpWebRequest)WebRequest.Create(string.Format("{0}={1}", _providerAPI, limit));
                requestGet.UseDefaultCredentials = true;

				try
				{
					// Get response  
					using (HttpWebResponse response = requestGet.GetResponse() as HttpWebResponse)
					{
						if (response.StatusCode == HttpStatusCode.OK)
						{
							StreamReader reader = new StreamReader(response.GetResponseStream());
							result = reader.ReadToEnd();
						}
						else
						{
							_logger.Error(string.Format("Could not get domains to process from API. Uri: {0}. StatusCode: {1}", requestGet.RequestUri.ToString(), response.StatusCode.ToString()));
							ExceptionExtensions.LogInformation("CrawlDaddy.Plugin.BI.Providers.BIApiDomainProvider.GetDomainsToCrawl()"
																								, string.Format("Could not get domains to process from API. Uri: {0}. StatusCode: {1}"
																																, requestGet.RequestUri.ToString()
																																, response.StatusCode.ToString()));
						}
					}
				}
				catch (System.Exception e)
				{
					_logger.ErrorFormat("Could not get domains to process from API. Uri: {0}.", requestGet.RequestUri.ToString());
					ExceptionExtensions.LogError(e, "CrawlDaddy.Plugin.BI.Providers.BIApiDomainProvider.GetDomainsToCrawl(ex)"
																				, string.Format("Could not get domains to process from API. Uri: {0}."
																												, requestGet.RequestUri.ToString()));
				}

				domainsToReturn = new JavaScriptSerializer().Deserialize<List<Domain>>(result);
			}

			return domainsToReturn;
		}

		public void ReportCrawledDomains(IEnumerable<Domain> crawledDomains)
		{
			//todo: do nothing?
		}
	}
}

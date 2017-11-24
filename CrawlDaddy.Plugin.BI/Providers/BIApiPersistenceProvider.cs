using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CrawlDaddy.Plugin.BI;
using System.Web.Script.Serialization;
using System.Linq;

namespace CrawlDaddy.Plugin.BI.Providers
{
	public class BIApiPersistenceProvider : BIApiPersistenceProviderBase
	{
		public BIApiPersistenceProvider() { }

		public override void Save(DataComponent dataComponent)
		{

			if (dataComponent == null || dataComponent.Attributes == null)
			{
				_persistenceLogger.Error("dataComponent was null.");
				return;
			}

			var val = new
			{
				TypeID = dataComponent.AttributeId
				, rptGDDomainsID = dataComponent.DomainId
				, ShopperID = dataComponent.ShopperId
				, Value = GetValue(dataComponent.Attributes, dataComponent.AttributeId)
			};


			//CreatePostRequest		
			Uri apiuri = new Uri(ConfigManager.PersistenceUrl);

			//send
			try
			{
				HttpStatusCode code = SendRequest(apiuri, val, true);
				if (code != HttpStatusCode.Created)
				{
					_persistenceLogger.WarnFormat("BIApiPersistenceProvider.SendDomainData failed. Trying again due to bad status code. HttpStatusCode: {0}", code);
					Save(dataComponent);
				}
			}
			catch (Exception e)
			{
				ExceptionExtensions.LogError(e, "BIApiPersistenceProvider.SendRequest()", "DomainID: " + dataComponent.DomainId + " TypeID: " + dataComponent.AttributeId);
			}
		}

		private object GetValue(Dictionary<string, string> dictionary, int p)
		{
			string val = string.Empty;

			if (p == 14 || p == 15)
			{
				val = string.Join(",", dictionary.Select(kv => kv.Key.ToString() + "|" + kv.Value.ToString()).ToArray());
			}
			else
			{
				if (dictionary.Count > 0)
					val = dictionary[dictionary.Keys.First()];
			}

			return val;
		}
	}
}

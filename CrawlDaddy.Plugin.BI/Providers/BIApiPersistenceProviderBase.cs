using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using log4net;

namespace CrawlDaddy.Plugin.BI.Providers
{
	public abstract class BIApiPersistenceProviderBase : IPersistenceProvider
	{
		JavaScriptSerializer _jserializer = new JavaScriptSerializer();
		protected static ILog _persistenceLogger = LogManager.GetLogger("PersistenceLogger");

		public HttpStatusCode SendRequest(Uri apiUri, object value, bool retry)
		{
			HttpStatusCode code = HttpStatusCode.InternalServerError;
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(apiUri);

			req.Method = "POST";
			req.ContentType = "application/json";
			req.KeepAlive = false;
			req.UseDefaultCredentials = true;

			try
			{
				byte[] data = Encoding.UTF8.GetBytes(_jserializer.Serialize(value));

				using (Stream dataStream = req.GetRequestStream())
				{
					dataStream.Write(data, 0, data.Length);
				}

				_persistenceLogger.DebugFormat("Sending data to {0}", apiUri);

				using (HttpWebResponse response = req.GetResponse() as HttpWebResponse)
				{
					code = response.StatusCode;
					string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
				}

				_persistenceLogger.DebugFormat("Data sent to {0}", apiUri);

				if (code != HttpStatusCode.Created && retry)
				{
					_persistenceLogger.WarnFormat("Trying again due to bad status code. HttpStatusCode: {0}", code);
					SendRequest(apiUri, value, false);
				}
			}
			catch (Exception e)
			{
				if (retry)
				{
					_persistenceLogger.WarnFormat("Trying again due to exception. HttpStatusCode: {0}", code);
					SendRequest(apiUri, value, true);
				}
				else
					throw e;
			}

			return code;
		}

		public abstract void Save(DataComponent dataComponent);				
	}
}

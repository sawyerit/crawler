using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Drone.API.Dig;
using Drone.API.Dig.Dns;
using Drone.API.Dig.Ssl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace WinDig.Tests
{
	[TestClass]
	public class DigTests
	{
		Dig _dig;

		[TestInitialize]
		public void InitializeTest()
		{
			_dig = new Dig();
		}

		[TestMethod]
		public void DigConstructor()
		{
			Dig dig = new Dig();

			Assert.IsNotNull(dig.Lookups);
			Assert.IsTrue(dig.Lookups.Count > 0);
		}

		[TestMethod]
		public void DigARecord()
		{
			Dig dig = new Dig();
			RecordA record = dig.GetARecord("godaddy.com");
		}

		[TestMethod]
		public void DigASN()
		{
			Dig dig = new Dig();
			string record = dig.GetASN(dig.GetIPAddress("dreamsencashed.com").ToString());

			Assert.AreEqual("17439", record);
		}

		[TestMethod]
		public void DigWebHostName()
		{
			Dig dig = new Dig();

			string record = dig.GetWebHostName("godaddy.com");

			Assert.IsTrue(record == "Self Hosted");
		}

		[TestMethod]
		public void DigIPAddress()
		{
			Dig dig = new Dig();
			IPAddress record = dig.GetIPAddress("godaddy.com");

			Assert.IsNotNull(record);
			Assert.IsFalse(String.IsNullOrEmpty(record.ToString()));
			Assert.AreEqual("97.74.104.201", record.ToString());
		}

		[TestMethod]
		public void DigMXRecord()
		{
			Dig dig = new Dig();
			List<RecordMX> records = dig.GetMXRecord("godaddy.com");

			Assert.IsNotNull(records);
			Assert.IsTrue(records.Count > 0);
			Debug.WriteLine(records[0].EXCHANGE);
			Assert.IsTrue(records[0].EXCHANGE.Contains("outlook"));
		}

		[TestMethod]
		public void DigNSRecord()
		{
			Dig dig = new Dig();
			List<RecordNS> records = dig.GetNSRecords("cars.com");

			Assert.IsNotNull(records);
			Assert.IsTrue(records.Count > 0);
		}

		[TestMethod]
		public void DigSSLCert()
		{
			Dig dig = new Dig();

			SSLCert cert = dig.GetSSLVerification("godaddy.com");

			Assert.IsNotNull(cert);
			Assert.IsNotNull(cert.IssuerName);
			Assert.IsNotNull(cert.SubjectType);
			Assert.AreEqual("www.godaddy.com", cert.SubjectType);

			cert = dig.GetSSLVerification("1footout.com");
			Assert.AreEqual(cert.FixedName, "None");

		}

		[TestMethod]
		public void DigWhoIs()
		{
			string registrar = _dig.GetRegistrar("0731CARD.COM");
			Assert.IsNotNull(registrar);

			registrar = _dig.GetRegistrar("notarealdomainxyz123asd");
			Assert.AreEqual(registrar, "N/A");
		}

		[TestMethod]
		public void FindCompanyInLookups()
		{
			Dig dig = new Dig();

			string foundName = dig.GetRegistrar("1computer.info");
			Assert.AreEqual("Network Solutions", foundName);

			//self hosted
			foundName = dig.GetEmailHostName("fash-art.com");
			Assert.AreEqual("Self Hosted", foundName);

			foundName = dig.GetEmailHostName("blooclick.com");
			Assert.AreEqual("ovh systems", foundName.ToLower());

			//email
			foundName = dig.GetEmailHostName("sawyerit.com");
			Assert.AreEqual("Go Daddy", foundName);

			//not found, use record
			foundName = dig.GetDNSHostName("travellution.com");
			Assert.AreEqual("technorail.com", foundName);

			//found, use company name
			foundName = dig.GetDNSHostName("godaddy.com");
			Assert.AreEqual("Go Daddy", foundName);

			//no SSL issuer
			foundName = dig.GetCompanyFromRecordName(dig.GetSSLVerification("cybergeekshop.com").IssuerName, "cybergeekshop.com", DigTypeEnum.SSL);
			Assert.AreEqual("None", foundName);

			//webhost (split AS name with -)
			foundName = dig.GetWebHostName("cybergeekshop.com");
			Assert.AreEqual("Unified Layer", foundName);

			foundName = dig.GetWebHostName("microteksystems.net");
			Assert.AreEqual("SoftLayer", foundName);

			//webhost (splitting AS Name without -)
			foundName = dig.GetWebHostName("eatads.com");
			Assert.AreEqual("Amazon", foundName);

		}

		[TestMethod]
		public void Follow301redirect()
		{
			//kraftymoms.com now shows Internap as host, GoDaddy as dns.  Webs.com uses internap.
			//We can do a head httprequest to see if there's a redirect.  If so, we can get the redirect
			//to page and do a webhost lookup on that. In this case, they both say Internap.
			//even a lookup of webs.com says internap


			//HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.kraftymoms.com");
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.facebook.com"); //feedroom.com
			request.AllowAutoRedirect = true;
			request.Method = "GET";
			HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

			string result = BuildTextFromResponse(resp);


			int code = (int)resp.StatusCode;
			string host = resp.Headers["Location"];

		}

		[TestMethod]
		public void DigWebHostName_Other()
		{
			//resolver will cache these, so remove the cache lookup
			//dig does a friendly name lookup as well, that can be removed for a VERY slight speed increase over a large #
			Dig dig = new Dig();

			List<string> domainList = new List<string>();
			List<string> webhostList = new List<string>();
			List<string> dnshostList = new List<string>();

			//domainList.Add("mprconsultinghk.com");
			domainList.Add("weebly.com");
			domainList.Add("kraftymoms.com");
			domainList.Add("paranique.com");
			domainList.Add("casaartandphoto.com");


			foreach (var item in domainList)
			{
				webhostList.Add(dig.GetWebHostName(item));
			}

			foreach (var item in domainList)
			{
				dnshostList.Add(dig.GetDNSHostName(item));
			}



		}

		[TestMethod]
		[Ignore]
		public void DigURL_PerformanceTest()
		{
			//resolver will cache these, so remove the cache lookup
			//dig does a friendly name lookup as well, that can be removed for a VERY slight speed increase over a large #
			Dig dig = new Dig();

			List<string> domainList = new List<string>();
			List<string> webhostList = new List<string>();
			List<string> errorList = new List<string>();

			domainList.Add("mprconsultinghk.com");
			domainList.Add("tnipresents.com");
			domainList.Add("kraftymoms.com");
			domainList.Add("paranique.com");
			domainList.Add("eatads.com");
			domainList.Add("travellution.com");
			domainList.Add("bee.com");
			domainList.Add("yahoo.com");
			domainList.Add("google.com");
			domainList.Add("microsoft.com");

			DateTime endTime;
			DateTime startTime = DateTime.Now;
			string header = string.Empty;
			int statCode = 0;

			startTime = DateTime.Now;
			Parallel.For(0, 100, (i) =>
			{
				foreach (var item in domainList)
				{
					try
					{
						CheckHead(ref statCode, ref header, item);
					}
					catch (Exception e)
					{
						if (e.Message.Contains("timed out"))
						{
							try
							{
								CheckHead(ref statCode, ref header, item);
							}
							catch (Exception ex)
							{
								if (ex.Message.Contains("timed out"))
								{
									errorList.Add("headrequest timed out");
								}
							}

						}
						else
						{
							errorList.Add(e.Message);
						}
					}

					try
					{
						//add host to list
						if (statCode == 301)
							webhostList.Add(dig.GetWebHostName(CleanUrl(header)));
						else
							webhostList.Add(dig.GetWebHostName(item));
					}
					catch (Exception)
					{
						webhostList.Add("webhost timed out");
					}
				}
			});
			endTime = DateTime.Now;
			TimeSpan elapsedTime1 = endTime.Subtract(startTime);

			List<string> timeoutList = webhostList.Where(item => item == "headrequest timed out").ToList();
			List<string> webtimeoutList = webhostList.Where(item => item == "webhost timed out").ToList();
		}

		[TestMethod]
		[Ignore]
		public void DigWRegistrar_PerformanceTest()
		{
			//resolver will cache these, so remove the cache lookup
			Dig dig = new Dig();

			List<string> domainList = new List<string>();
			List<string> webhostList = new List<string>();

			domainList.Add("coderow.com");
			domainList.Add("slashcommunity.com");
			domainList.Add("vantronix.com");
			domainList.Add("sociofy.com");
			domainList.Add("netconstructor.com");
			domainList.Add("dotfox.com");
			domainList.Add("go.co");
			domainList.Add("1computer.info");
			domainList.Add("andyet.net");
			domainList.Add("p1us.me");
			domainList.Add("10cms.com");
			domainList.Add("1010data.com");
			domainList.Add("1800vending.com");
			domainList.Add("easybacklog.com");
			domainList.Add("abcotechnology.com");
			domainList.Add("abcsignup.com");
			domainList.Add("airtag.com");
			domainList.Add("nuospace.com");
			domainList.Add("brightscope.com");
			domainList.Add("data180.com");

			DateTime endTime;
			DateTime startTime = DateTime.Now;
			Random rand = new Random();

			Parallel.For(0, 50, (i) =>
			{
				foreach (var item in domainList)
				{
					webhostList.Add(dig.GetRegistrar(item));
				}
			});

			endTime = DateTime.Now;
			TimeSpan elapsedTime1 = endTime.Subtract(startTime);
		}

		[TestMethod]
		public void CookieReader_test()
		{
			//make sure its not a blocked page
			//HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.spacecoastpetes.com/"); //zen cart (zenid)
			//HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.focusattack.com/"); //bigcommerce (shop_session_token)
			//HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.whatajewel.com"); //osCommerce (osCsid) also has perm 301redirect
			//HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.givethisgift.co.uk/"); //osCommerce (osCsid) no redirect
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.millardauctionco.com"); //webs.com (fwww)



			CookieContainer cookieJar = new CookieContainer();

			request.UserAgent = "User-Agent	Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
			request.CookieContainer = cookieJar;
			request.AllowAutoRedirect = true;
			request.Method = "GET";
			HttpWebResponse resp = (HttpWebResponse)request.GetResponse();


			CookieCollection cc1 = cookieJar.GetCookies(request.RequestUri);
			CookieCollection cc2 = cookieJar.GetCookies(resp.ResponseUri);



			string result = BuildTextFromResponse(resp);

		}

		private string BuildTextFromResponse(HttpWebResponse response)
		{
			string responseText = string.Empty;

			using (var streamReader = new StreamReader(response.GetResponseStream()))
				responseText = streamReader.ReadToEnd();

			return responseText;
		}

		private string CleanUrl(string url)
		{
			string cleanUrl = string.Empty;
			cleanUrl = url.Replace("http://", "").Replace("https://", "").Replace("htttp://", "").Replace("www.", "");

			if (cleanUrl.Count(z => z == '.') > 1)
			{
				if (cleanUrl.StartsWith("info.") || cleanUrl.StartsWith("case.") || cleanUrl.StartsWith("beta.") || cleanUrl.StartsWith("blog."))
					cleanUrl = cleanUrl.Remove(0, 5);

				if (cleanUrl.StartsWith("about."))
					cleanUrl = cleanUrl.Remove(0, 6);

				if (cleanUrl.StartsWith("global."))
					cleanUrl = cleanUrl.Remove(0, 7);

				if (cleanUrl.StartsWith("ir."))
					cleanUrl = cleanUrl.Remove(0, 3);
			}

			if (cleanUrl.Contains("/"))
				cleanUrl = cleanUrl.Remove(cleanUrl.IndexOf('/'), cleanUrl.Length - cleanUrl.IndexOf('/'));

			return cleanUrl;
		}
		
		private static void CheckHead(ref int statCode, ref string header, string item)
		{
			//check for redirect
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://" + item);
			request.AllowAutoRedirect = false;
			request.Method = "HEAD";
			request.Timeout = 5000;

			using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
			{
				header = resp.Headers["Location"];
				statCode = (int)resp.StatusCode;
			}
		}
	}
}

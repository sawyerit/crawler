using System;
using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Plugin.Processor;
using Drone.API.Dig;
using log4net;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace CrawlDaddy.Plugin.BI.Processors
{
    /// <summary>
    /// Determine if the site falls into the range of a GoDaddy parked page.  Record the IPAddress
    /// </summary>
    public class ParkedCrawlProcessor : CrawlProcessorBase, ICrawlProcessor
    {
        static ILog _logger = LogManager.GetLogger(typeof(ParkedCrawlProcessor).FullName);
        const int ATTRIB_TYPE_ID = 20;

        public void ProcessCrawledDomain(CrawlContext crawlContext)
        {
            string parkedType = string.Empty;
            IPAddress ip = null;
            crawlContext.CrawlBag.NoCrawl = false;

            try
            {
                ip = Dig.Instance.GetIPAddress(crawlContext.RootUri.DnsSafeHost);
                if (!Object.Equals(null, ip))
                    parkedType = FindParkedType(ip);                
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Exception occurred getting ipaddress name for [{0}] [{1}]", crawlContext.RootUri.DnsSafeHost, e);
            }

            ProcessorResult result = new ProcessorResult { UniqueAttributeId = ATTRIB_TYPE_ID };
            result.IsAHit = !string.IsNullOrEmpty(parkedType);

            //save isparkedpage
            if (result.IsAHit)
            {
                result.Attributes.Add(ATTRIB_TYPE_ID.ToString(), parkedType);
                crawlContext.CrawlBag.NoCrawl = true; //Park Forward?
                DomainSave(crawlContext, result);
            }

            //save ip address
            if (!Object.Equals(null, ip))
            {
                ProcessorResult resultIP = new ProcessorResult { UniqueAttributeId = 21 };
                resultIP.IsAHit = true;
                resultIP.Attributes.Add("21", ip.ToString());

                DomainSave(crawlContext, resultIP);
            }
            else
            {
                _logger.DebugFormat("Domain [{0}] has no A record.", crawlContext.RootUri.DnsSafeHost);
            }
        }

        public void ProcessCrawledPage(CrawlContext crawlContext, CrawledPage crawledPage)
        {//do nothing, this processor only happens on domain crawl
        }

        public static string FindParkedType(IPAddress address)
        {
            try
            {
                long ip = BitConverter.ToInt32(address.GetAddressBytes().Reverse().ToArray(), 0);

                foreach (IPRange range in ConfigManager.ParkedRanges)
                {
                    if (address.AddressFamily != range.AddressFamily)
                        continue;

                    long rangeLower = BitConverter.ToInt32(range.AddressLower.GetAddressBytes().Reverse().ToArray(), 0);
                    long rangeUpper = BitConverter.ToInt32(range.AddressUpper.GetAddressBytes().Reverse().ToArray(), 0);

                    if (ip >= rangeLower && ip <= rangeUpper)
                        return range.RangeType;
                }
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Exception occurred determining ParkedType for [{0}]", address.ToString(), e);
            }
            return string.Empty;
        }
    }

    public class IPRange
    {
        public IPAddress AddressLower { get; set; }
        public IPAddress AddressUpper { get; set; }
        public AddressFamily AddressFamily { get; set; }
        public string RangeType { get; set; }
    }
}

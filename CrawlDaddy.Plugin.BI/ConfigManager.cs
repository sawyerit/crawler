using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CrawlDaddy.Core;
using CrawlDaddy.Plugin.BI.Processors;
using System.Net;

namespace CrawlDaddy.Plugin.BI
{
    public static class ConfigManager
    {
        private static string _domainUrl;
        public static string DomainUrl
        {
            get
            {
                if (Object.Equals(null, _domainUrl))
                    _domainUrl = GetProviderAPI("BIApiDomainProvider");
                return _domainUrl;
            }
        }

        private static string _persistenceUrl;
        public static string PersistenceUrl
        {
            get
            {
                if (Object.Equals(null, _persistenceUrl))
                    _persistenceUrl = GetProviderAPI("BIApiPersistenceProvider");
                return _persistenceUrl;
            }
        }

        private static string[] _invalidTwitterTags;
        public static string[] InvalidTwitterTags
        {
            get
            {
                if (Object.Equals(null, _invalidTwitterTags))
                    _invalidTwitterTags = GetInvalidTwitterTags();
                return _invalidTwitterTags;
            }
        }

        private static List<IPRange> _parkedRanges;
        public static List<IPRange> ParkedRanges
        {
            get
            {
                if (Object.Equals(null, _parkedRanges) || _parkedRanges.Count <= 0)
                    _parkedRanges = GetParkedRanges();
                return _parkedRanges;
            }
        }


        private static string GetProviderAPI(string providerName)
        {
            string returnVal = string.Empty;

            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Xml", "biplugin.xml"));

                returnVal = xdoc.SelectSingleNode("plugin/providers/" + providerName.ToLowerInvariant() + "/uri").InnerText;
            }
            catch (Exception e)
            {
                ExceptionExtensions.LogError(e, "CrawlDaddy.Plugin.BI.ConfigManager.GetProviderAPI()", "provider: " + providerName);
            }

            return returnVal;
        }

        private static string[] GetInvalidTwitterTags()
        {
            string[] returnVal = { };

            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Xml", "biplugin.xml"));

                returnVal = xdoc.SelectSingleNode("plugin/processors/sociallink/twitter/invalidtags").InnerText.Split(',');
            }
            catch (Exception e)
            {
                ExceptionExtensions.LogError(e, "CrawlDaddy.Plugin.BI.ConfigManager.GetInvalidTwitterTags()");
            }

            return returnVal;
        }

        private static List<IPRange> GetParkedRanges()
        {
            List<IPRange> ranges = new List<IPRange>();

            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Xml", "biplugin.xml"));

                XmlNodeList nodelist = xdoc.SelectNodes("plugin/processors/parkedpage/range");
                foreach (XmlNode node in nodelist)
                {
                    IPAddress lowerAddress = IPAddress.Parse(node.SelectSingleNode("lower").InnerText);
                    IPAddress upperAddress = IPAddress.Parse(node.SelectSingleNode("upper").InnerText);
                    string type = node.SelectSingleNode("type").InnerText;
                    ranges.Add(new IPRange { AddressFamily = lowerAddress.AddressFamily, AddressLower = lowerAddress, AddressUpper = upperAddress, RangeType = type });
                }
            }
            catch (Exception e)
            {
                ExceptionExtensions.LogError(e, "CrawlDaddy.Plugin.BI.ConfigManager.GetParkedRanges()");
            }

            return ranges;
        }
    }
}

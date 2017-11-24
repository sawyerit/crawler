using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using CrawlDaddy.Core;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Drone.API.MarketAnalysis
{
    /// <summary>
    /// Class to read rules and apply them to the document
    /// </summary>
    public class MarketShareEngine
    {
        private readonly Dictionary<string, IProcessor> _processors = new Dictionary<string, IProcessor>();
        private readonly XmlDocument _sitebuilderRulesXML = new XmlDocument();
        private readonly XmlDocument _shoppingcartRulesXML = new XmlDocument();
        private int _maxPageByteSize;

        #region singleton constructor

        private static volatile MarketShareEngine instance;
        private static object syncRoot = new Object();

        private MarketShareEngine()
        {
            try
            {
                string xmlFolder = AppDomain.CurrentDomain.BaseDirectory + "\\Xml";
                XmlDocument _marketAnalysisXML = new XmlDocument();
                _marketAnalysisXML.Load(Path.Combine(xmlFolder, "MarketAnalysis.xml"));
                _maxPageByteSize = 1048576;

                _sitebuilderRulesXML.Load(Path.Combine(xmlFolder, "SiteBuilderRules.xml"));
                _shoppingcartRulesXML.Load(Path.Combine(xmlFolder, "ShoppingCartRules.xml"));

                XmlDocument ruleTypes = new XmlDocument();
                ruleTypes.Load(Path.Combine(xmlFolder, "RuleTypes.xml"));

                Assembly assembly = Assembly.GetExecutingAssembly();
                foreach (XmlNode ruleNode in ruleTypes.SelectNodes("//Rule"))
                {
                    Type type = assembly.GetType(String.Format("{0}.{1}", typeof(MarketShareEngine).Namespace, ruleNode.InnerText));
                    if (!Object.Equals(null, type))
                    {
                        IProcessor proc = Activator.CreateInstance(type) as IProcessor;
                        if (!Object.Equals(null, proc))
                        {
                            _processors.Add(ruleNode.InnerText.ToLower(), proc);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionExtensions.LogError(e, "API.MarketAnalysis.MarketShareEngine Constructor");
            }
        }

        public static MarketShareEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MarketShareEngine();
                    }
                }

                return instance;
            }
        }

        #endregion


        public string SiteBuilder(string domain, HtmlDocument document, HttpWebResponse response, bool isRoot)
        {
            DOMReader dom = new DOMReader(domain, document);

            if (!Object.Equals(null, response)) dom.ResponseCookies = response.Cookies;

            if (!Object.Equals(null, dom.Document.DocumentNode))
            {
                foreach (XmlNode productNode in _sitebuilderRulesXML.SelectNodes("//Product[@name]"))
                {
                    if (ProcessRuleNode(productNode, dom, isRoot))
                    {
                        dom = null;
                        return productNode.Attributes["name"].Value;
                    }
                }
            }

            dom = null;
            return "None";
        }

        public string ShoppingCart(string domain, HtmlDocument document, HttpWebResponse response, bool isRoot)
        {
            DOMReader dom = new DOMReader(domain, document);

            if (!Object.Equals(null, response)) dom.ResponseCookies = response.Cookies;

            if (!Object.Equals(null, dom.Document.DocumentNode))
            {
                foreach (XmlNode productNode in _shoppingcartRulesXML.SelectNodes("//Product[@name]"))
                {
                    if (ProcessRuleNode(productNode, dom, isRoot))
                    {
                        return productNode.Attributes["name"].Value;
                    }
                }
            }

            dom = null;
            return "None";
        }

        //parallels, causing obj ref errors
        //public string SiteBuilder(string domain, HtmlDocument document, HttpWebResponse response, bool isRoot)
        //{
        //    string retVal = "None";
        //    DOMReader dom = new DOMReader(domain, document);

        //    if (!Object.Equals(null, response)) dom.ResponseCookies = response.Cookies;

        //    if (!Object.Equals(null, dom.Document.DocumentNode))
        //    {
        //        var nodeList = new List<XmlNode>(_sitebuilderRulesXML.SelectNodes("//Product[@name]").Cast<XmlNode>());
        //        Parallel.ForEach(nodeList, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (productNode, loopState) =>
        //        {
        //            if (ProcessRuleNode(productNode, dom, loopState, isRoot))
        //            {
        //                loopState.Stop();
        //                retVal = productNode.Attributes["name"].Value;
        //            }
        //        });
        //    }

        //    dom = null;
        //    return retVal;
        //}

        //public string ShoppingCart(string domain, HtmlDocument document, HttpWebResponse response, bool isRoot)
        //{
        //    string retVal = "None";
        //    DOMReader dom = new DOMReader(domain, document);

        //    if (!Object.Equals(null, response)) dom.ResponseCookies = response.Cookies;

        //    if (!Object.Equals(null, dom.Document.DocumentNode))
        //    {
        //        var nodeList = new List<XmlNode>(_shoppingcartRulesXML.SelectNodes("//Product[@name]").Cast<XmlNode>());
        //        Parallel.ForEach(nodeList, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (productNode, loopState) =>
        //        {
        //            if (ProcessRuleNode(productNode, dom, loopState, isRoot))
        //            {
        //                loopState.Stop();
        //                retVal = productNode.Attributes["name"].Value;
        //            }
        //        });
        //    }
        //    dom = null;
        //    return retVal;
        //}


        #region private methods


        private bool ProcessRuleNode(XmlNode productNode, DOMReader dom, bool isRoot)
        {
            foreach (XmlNode andOrNode in productNode.SelectSingleNode("Rules").ChildNodes)
            {
                if (andOrNode.NodeType == XmlNodeType.Element)
                {
                    if (andOrNode.Name != "Rule")
                    {
                        bool match = ProcessLogicNode(andOrNode, dom, isRoot);
                        if (match)
                            return true;
                    }
                    else
                    {
                        //just in case there is only 1 rule in rules. No AND/OR
                        if (ValidateRule(new MarketShareRule(andOrNode), dom, isRoot))
                            return true;
                    }
                }
            }
            return false;
        }

        private bool ProcessLogicNode(XmlNode andOrNode, DOMReader dom, bool isRoot)
        {
            return andOrNode.Name == "And" ? ProcessAndRules(andOrNode, dom, isRoot) : ProcessOrRules(andOrNode, dom, isRoot);
        }

        private bool ProcessOrRules(XmlNode orNode, DOMReader dom, bool isRoot)
        {
            foreach (XmlNode node in orNode.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.Name != "Rule")
                    {
                        if (ProcessLogicNode(node, dom, isRoot))
                            return true;//short circuit if true
                    }
                    else
                    {
                        //process rule
                        if (ValidateRule(new MarketShareRule(node), dom, isRoot))
                            return true; //shortcircuit if true
                    }
                }
            }

            return false;
        }

        private bool ProcessAndRules(XmlNode andNode, DOMReader dom, bool isRoot)
        {
            int matchCount = 0;
            //if no min is specified, they all must be true
            int minCount = andNode.ChildNodes.Count;

            //set min number of rules that must be true
            if (!Object.Equals(null, andNode.Attributes["mincount"]))
            {
                if (!Int32.TryParse(andNode.Attributes["mincount"].Value, out minCount))
                {
                    minCount = andNode.ChildNodes.Count;
                }
            }

            //evaluate all childnodes
            foreach (XmlNode node in andNode.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.Name != "Rule")
                    {
                        if (ProcessLogicNode(node, dom, isRoot))
                            matchCount++;
                    }
                    else
                    {
                        if (ValidateRule(new MarketShareRule(node), dom, isRoot))
                            matchCount++;
                    }

                    if (matchCount >= minCount) break;
                }
            }

            return matchCount >= minCount;
        }

        private bool ValidateRule(MarketShareRule rule, DOMReader dom, bool isRoot)
        {
            //default to text, so that it will just regex the whole page
            IProcessor proc = _processors["text"];
            if (_processors.ContainsKey(rule.Type.ToLower()))
            {
                if (!rule.RootOnly || (isRoot && rule.RootOnly))
                {
                    proc = _processors[rule.Type.ToLower()];
                }
            }
            return proc.Process(dom, rule);
        }

        #endregion
    }
}

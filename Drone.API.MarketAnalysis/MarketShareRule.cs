using System;
using System.Xml;

namespace Drone.API.MarketAnalysis
{
    public class MarketShareRule
    {
        public bool RootOnly { get; set; }
        public string Type { get; set; }
        public string Property { get; set; }
        public string Value { get; set; }
        public bool IgnoreAbsURI { get; set; }

        public MarketShareRule(XmlNode ruleNode)
        {
            Type = ruleNode.Attributes["type"].Value;
            Property = ruleNode.Attributes["property"].Value;
            Value = ruleNode.Attributes["value"].Value;

            IgnoreAbsURI = false;
            if (ruleNode.Attributes["ignoreAbsURI"] != null)
                IgnoreAbsURI = Convert.ToBoolean(ruleNode.Attributes["ignoreAbsURI"].Value);
            
            RootOnly = false;
            if (ruleNode.Attributes["rootOnly"] != null)
                RootOnly = Convert.ToBoolean(ruleNode.Attributes["rootOnly"].Value);
        }

        public override string ToString()
        {
            return string.Format("Rule Type: {0}, Property: {1}, Value: {2}, IgnoreAbsURI: {3}, RootOnly: {4}", Type, Property, Value, IgnoreAbsURI, RootOnly);
        }
    }
}

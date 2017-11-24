using System;
using System.Xml;
using System.Xml.XPath;
using Drone.API.Dig.Common.Extensions;

namespace Drone.API.Dig.Common
{
	public static class XMLUtility
	{
		public static XmlNode GetNodeFromXml(IXPathNavigable Xml, string p)
		{
			XmlDocument xmlDoc = Xml as XmlDocument;
			return GetNodeFromXml(xmlDoc, p);
		}

		public static XmlNode GetNodeFromXml(XmlDocument xmlDoc, string p)
		{
			XmlNode xNode = null;

			if (!Object.Equals(xmlDoc, null))
			{
				try
				{
					xNode = xmlDoc.SelectSingleNode(p);
				}
				catch (Exception e)
				{
					ExceptionExtensions.LogError(e, "XMLUtility.GetNodeFromXml", "Node: " + p);
				}

				if (Object.Equals(xNode, null))
					ExceptionExtensions.LogError(new ArgumentNullException("Node not found in XML - XPath: " + p), "XMLUtility.GetNodeFromXml");
			}
			else
			{
				ExceptionExtensions.LogError(new ArgumentNullException("XML document was null. XPath: " + p), "XMLUtility.GetNodeFromXml");
			}

			return xNode;
		}

		public static XmlNode GetNodeFromNode(XmlNode nodeAccount, string p)
		{
			XmlNode xNode = null;

			try
			{
				xNode = nodeAccount.SelectSingleNode(p);
			}
			catch (Exception e)
			{
				ExceptionExtensions.LogError(e, "XMLUtility.GetNodeFromNode");
			}

			if (Object.Equals(xNode, null))
				ExceptionExtensions.LogError(new ArgumentNullException("Node not found in XML - XPath: " + p), "XMLUtility.GetNodeFromNode");

			return xNode;
		}
	}
}

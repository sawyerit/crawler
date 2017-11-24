using System.Configuration;
using System.IO;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using Drone.API.Dig.Common.Extensions;

namespace Drone.API.Dig.Common
{
	public static class Utility
	{
		private static object _hashLock = new object();
		private static Dictionary<string, object> lockHash = new Dictionary<string, object>();

		public static string ApplicationName
		{
			get { return "WinDig"; }
		}

		private static string _componentBaseFolder = string.Empty;

		public static string ComponentBaseFolder
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_componentBaseFolder))
					ComponentBaseFolder = AppDomain.CurrentDomain.BaseDirectory;

				return _componentBaseFolder;
			}
			set { _componentBaseFolder = value; }
		}

		private static string _logLocation
		{
			get
			{
				string folder = Utility.ComponentBaseFolder + "\\Logs";

				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);

				return folder;
			}
		}

		public static bool IsCriticalDBError(Exception e)
		{
			bool isCrit = false;
			if (e.Message.ToLowerInvariant().Contains("tempdb"))
			{
				isCrit = true;
			}
			return isCrit;
		}

		public static void WriteToLogFile(string fileName, string message)
		{
			string logFile = Path.Combine(_logLocation, fileName);

			//Get lock for the file we are writing to, if it doesn't exist, create it
			lock (_hashLock)
			{
				if (!lockHash.ContainsKey(fileName))
				{
					lockHash[fileName] = new Object();
				}
			}

			//write to file
			try
			{
				lock (lockHash[fileName])
					using (System.IO.StreamWriter file = new System.IO.StreamWriter(logFile, true))
					{
						file.WriteLine(message);
					}

			}
			catch (Exception e)
			{
				throw;
			}
		}

		public static bool FileExists(string fileName)
		{
			return File.Exists(Path.Combine(_logLocation, fileName));
		}

		public static void WriteToLogFile(string fileName, string[] message)
		{
			string logFile = Path.Combine(_logLocation, fileName);
			try
			{
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(logFile, true))
				{
					for (int i = 0; i < message.Length; i++)
					{
						if (i < message.Length - 1)
							file.WriteLine(message[i]);
						else
							file.Write(message[i]);
					}
				}
			}
			catch (Exception) { }
		}

		public static string ReadFirstLineFromFile(string fileName, bool delete)
		{
			string retVal = string.Empty;

			try
			{
				string file = Path.Combine(_logLocation, fileName);
				var lines = File.ReadAllLines(file);

				if (lines.Length > 0 && !String.IsNullOrWhiteSpace(lines[0]))
				{
					retVal = lines.FirstOrDefault();

					if (delete && !string.IsNullOrWhiteSpace(retVal))
						File.WriteAllLines(file, lines.Skip(1));
				}
				else
				{
					File.Delete(file);
				}
			}
			catch (Exception) { }

			return retVal;
		}

		public static List<string> ReadLinesFromFile(string fileName, int numLines, bool delete)
		{
			List<string> retVal = new List<string>();

			try
			{
				string file = Path.Combine(_logLocation, fileName);
				var lines = File.ReadAllLines(file);

				//get specified num of lines or the rest, whichever is smaller
				if (lines.Length > numLines)
					retVal = lines.Take(numLines).ToList();
				else
					retVal = lines.ToList();

				//delete the taken lines and write out the file
				if (lines.Length > 0)
				{
					if (delete && retVal.Count() > 0)
						File.WriteAllLines(file, lines.Skip(retVal.Count()));
				}
				else
				{
					File.Delete(file);
				}
			}
			catch (Exception) { }

			return retVal;
		}

		public static string CleanUrl(string url)
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

		public static void AddLineToFile(string fileName, string value)
		{
			try
			{
				string file = Path.Combine(_logLocation, fileName);
				string[] lines = File.ReadAllLines(file);
				if (lines.Length > 0)
				{
					if (lines[lines.Length - 1] == string.Empty)
					{
						lines[lines.Length - 1] = value + Environment.NewLine;
					}
					else
					{
						List<string> lineList = lines.ToList();
						lineList.Add(value + Environment.NewLine);
						lines = lineList.ToArray();
					}
				}
				File.WriteAllLines(file, lines);
			}
			catch (Exception) { }
		}

		public static string SerializeToXMLString<T>(object serialize)
		{
			try
			{
				StringWriter stringWriter = new StringWriter();
				XmlSerializer xs = new XmlSerializer(typeof(T));
				XmlWriterSettings xmlSettings = new XmlWriterSettings();
				xmlSettings.OmitXmlDeclaration = true;
				xmlSettings.Encoding = Encoding.UTF8;
				xmlSettings.Indent = false;
				xmlSettings.CloseOutput = true;

				XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlSettings);
				XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

				namespaces.Add(String.Empty, String.Empty);
				xs.Serialize(xmlWriter, serialize, namespaces);

				return stringWriter.ToString();
			}
			catch (Exception e)
			{
				//todo: add typeof(T).tostring here so we know whats failing?
				ExceptionExtensions.LogError(e, "Utility.SerializeToXMLString<T>()");
				return String.Empty;
			}

		}

		public static T DeserializeXMLString<T>(string deserialize)
		{
			try
			{
				T returnValue = default(T);
				XmlSerializer xs = new XmlSerializer(typeof(T));
				StringReader stringReader = new StringReader(deserialize);
				XmlTextReader xmlReader = null;

				try
				{
					xmlReader = new XmlTextReader(stringReader);
				}
				catch
				{
					if (stringReader != null) stringReader.Dispose();
				}

				using (xmlReader)
				{
					object obj = xs.Deserialize(xmlReader);
					returnValue = Conversions.ConvertTo(obj, default(T));
				}

				return returnValue;
			}

			catch (Exception)
			{
				return default(T);
			}

		}
	}
}
